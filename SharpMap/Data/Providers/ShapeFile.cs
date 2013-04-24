// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
#if !DotSpatialProjections
using ProjNet.Converters.WellKnownText;
using GeoAPI.CoordinateSystems;
#else
using DotSpatial.Projections;
#endif
using GeoAPI;
using GeoAPI.Geometries;
using SharpMap.Utilities.SpatialIndexing;
using Common.Logging;

namespace SharpMap.Data.Providers
{
    /// <summary>
	/// Shapefile dataprovider
	/// </summary>
	/// <remarks>
	/// <para>The ShapeFile provider is used for accessing ESRI ShapeFiles. The ShapeFile should at least contain the
	/// [filename].shp, [filename].idx, and if feature-data is to be used, also [filename].dbf file.</para>
	/// <para>The first time the ShapeFile is accessed, SharpMap will automatically create a spatial index
	/// of the shp-file, and save it as [filename].shp.sidx. If you change or update the contents of the .shp file,
	/// delete the .sidx file to force SharpMap to rebuilt it. In web applications, the index will automatically
	/// be cached to memory for faster access, so to reload the index, you will need to restart the web application
	/// as well.</para>
	/// <para>
	/// M and Z values in a shapefile is ignored by SharpMap.
	/// </para>
	/// </remarks>
	/// <example>
	/// Adding a datasource to a layer:
	/// <code lang="C#">
	/// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
	/// myLayer.DataSource = new SharpMap.Data.Providers.ShapeFile(@"C:\data\MyShapeData.shp");
	/// </code>
	/// </example>
	public class ShapeFile : FilterProvider, IProvider
	{
	    readonly ILog _logger = LogManager.GetLogger(typeof(ShapeFile));

        //#region Delegates

        ///// <summary>
        ///// Filter Delegate Method
        ///// </summary>
        ///// <remarks>
        ///// The FilterMethod delegate is used for applying a method that filters data from the dataset.
        ///// The method should return 'true' if the feature should be included and false if not.
        ///// <para>See the <see cref="FilterDelegate"/> property for more info</para>
        ///// </remarks>
        ///// <seealso cref="FilterDelegate"/>
        ///// <param name="dr"><see cref="SharpMap.Data.FeatureDataRow"/> to test on</param>
        ///// <returns>true if this feature should be included, false if it should be filtered</returns>
        //public delegate bool FilterMethod(FeatureDataRow dr);

        //#endregion

        private ShapeFileHeader _header;
        private ShapeFileIndex _index;
        
#if !DotSpatialProjections
		private ICoordinateSystem _coordinateSystem;
#else
		private ProjectionInfo _coordinateSystem;
#endif
		private bool _coordsysReadFromFile;
		private Envelope _envelope;
		private bool _fileBasedIndex;
	    private readonly bool _fileBasedIndexWanted;
		private string _filename;
        private string _dbfFile;
        private Encoding _dbfSpecifiedEncoding = null;
		private int _srid = -1;

	    private readonly object _shapeFileLock = new object();
		
	    private IGeometryFactory _factory;
        private static int _memoryCacheLimit = 50000;
        private static readonly object GspLock = new object();

#if USE_MEMORYMAPPED_FILE
        private static Dictionary<string,System.IO.MemoryMappedFiles.MemoryMappedFile> _memMappedFiles;
        private static Dictionary<string, int> _memMappedFilesRefConter;
        private bool _haveRegistredForUsage = false;
        private bool _haveRegistredForShxUsage = false;
        static ShapeFile()
        {
            _memMappedFiles = new Dictionary<string, System.IO.MemoryMappedFiles.MemoryMappedFile>();
            _memMappedFilesRefConter = new Dictionary<string, int>();
            SpatialIndexCreationOption = SpatialIndexCreation.Recursive;
        }
#else
        static ShapeFile()
        {
            SpatialIndexCreationOption = SpatialIndexCreation.Recursive;
        }
#endif
		private readonly bool _useMemoryCache;
		private DateTime _lastCleanTimestamp = DateTime.Now;
		private readonly TimeSpan _cacheExpireTimeout = TimeSpan.FromMinutes(1);
        private readonly object _cacheLock = new object();
        private readonly Dictionary <uint,FeatureDataRow> _cacheDataTable = new Dictionary<uint,FeatureDataRow>();

		/// <summary>
		/// Tree used for fast query of data
		/// </summary>
		private QuadTree _tree;

		/// <summary>
		/// Initializes a ShapeFile DataProvider without a file-based spatial index.
		/// </summary>
		/// <param name="filename">Path to shape file</param>
		public ShapeFile(string filename) : this(filename, false)
		{
		}

		/// <summary>
		/// Initializes a ShapeFile DataProvider.
		/// </summary>
		/// <remarks>
		/// <para>If FileBasedIndex is true, the spatial index will be read from a local copy. If it doesn't exist,
		/// it will be generated and saved to [filename] + '.sidx'.</para>
		/// <para>Using a file-based index is especially recommended for ASP.NET applications which will speed up
		/// start-up time when the cache has been emptied.
		/// </para>
		/// </remarks>
		/// <param name="filename">Path to shape file</param>
		/// <param name="fileBasedIndex">Use file-based spatial index</param>
		public ShapeFile(string filename, bool fileBasedIndex)
		{
			_filename = filename;
		    var shxFile = Path.ChangeExtension(filename, ".shx");
            if (!File.Exists(shxFile)) ShapeFileIndex.Create(_filename);

		    _fileBasedIndexWanted = fileBasedIndex;
			_fileBasedIndex = (fileBasedIndex) && File.Exists(shxFile);
			_dbfFile = Path.ChangeExtension(filename, ".dbf");

			//Parse shape header
			ParseHeader();
			//Read projection file
			ParseProjection();
        
            //By default, don't enable _MemoryCache if there are a lot of features
            _useMemoryCache = GetFeatureCount() <= MemoryCacheLimit;
        }

        /// <summary>
        /// Cleans the internal memory cached, expurging the objects that are not in the viewarea anymore
        /// </summary>
        /// <param name="objectlist">OID of the objects in the current viewarea</param>
        private void CleanInternalCache(Collection<uint> objectlist)
        {
            if (!_useMemoryCache)
            {
                return;
            }

            lock (_cacheLock)
            {
                //Only execute this if the memorycache is active and the expiretimespan has timed out
                if (DateTime.Now.Subtract(_lastCleanTimestamp) > _cacheExpireTimeout)
                {
                    var notIntersectOid = new Collection<uint>();

                    //identify the not intersected oid
                    foreach (var oid in _cacheDataTable.Keys)
                    {
                        if (!objectlist.Contains(oid))
                        {
                            notIntersectOid.Add(oid);
                        }
                    }

                    //Clean the cache
                    foreach (uint oid in notIntersectOid)
                    {
                        _cacheDataTable.Remove(oid);
                    }

                    //Reset the lastclean timestamp
                    _lastCleanTimestamp = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how many features are allowed for memory cache approach
        /// </summary>
	    protected static int MemoryCacheLimit
	    {
	        get { return _memoryCacheLimit; }
	        set { _memoryCacheLimit = value; }
	    }

        private void ClearingOfCachedDataRequired(object sender, EventArgs e)
        {
            if (_useMemoryCache)
                lock (_cacheLock)
                {
                    _cacheDataTable.Clear();
                }
        }

	    /// <summary>
	    /// Initializes a ShapeFile DataProvider.
	    /// </summary>
	    /// <remarks>
	    /// <para>If FileBasedIndex is true, the spatial index will be read from a local copy. If it doesn't exist,
	    /// it will be generated and saved to [filename] + '.sidx'.</para>
	    /// <para>Using a file-based index is especially recommended for ASP.NET applications which will speed up
	    /// start-up time when the cache has been emptied.
	    /// </para>
	    /// </remarks>
	    /// <param name="filename">Path to shape file</param>
	    /// <param name="fileBasedIndex">Use file-based spatial index</param>
	    /// <param name="useMemoryCache">Use the memory cache. BEWARE in case of large shapefiles</param>
	    public ShapeFile(string filename, bool fileBasedIndex, bool useMemoryCache) : this(filename, fileBasedIndex,useMemoryCache,0)
		{
		}

	    /// <summary>
	    /// Initializes a ShapeFile DataProvider.
	    /// </summary>
	    /// <remarks>
	    /// <para>If FileBasedIndex is true, the spatial index will be read from a local copy. If it doesn't exist,
	    /// it will be generated and saved to [filename] + '.sidx'.</para>
	    /// <para>Using a file-based index is especially recommended for ASP.NET applications which will speed up
	    /// start-up time when the cache has been emptied.
	    /// </para>
	    /// </remarks>
	    /// <param name="filename">Path to shape file</param>
	    /// <param name="fileBasedIndex">Use file-based spatial index</param>
	    /// <param name="useMemoryCache">Use the memory cache. BEWARE in case of large shapefiles</param>
	    /// <param name="SRID">The spatial reference id</param>
	    public ShapeFile(string filename, bool fileBasedIndex, bool useMemoryCache,int SRID) : this(filename, fileBasedIndex)
		{
			_useMemoryCache = useMemoryCache;
			this.SRID=SRID;
		}
		
		/// <summary>
		/// Gets or sets the coordinate system of the ShapeFile. If a shapefile has 
		/// a corresponding [filename].prj file containing a Well-Known Text 
		/// description of the coordinate system this will automatically be read.
		/// If this is not the case, the coordinate system will default to null.
		/// </summary>
		/// <exception cref="ApplicationException">An exception is thrown if the coordinate system is read from file.</exception>
#if !DotSpatialProjections
		public ICoordinateSystem CoordinateSystem
#else
		public ProjectionInfo CoordinateSystem
#endif
		{
			get { return _coordinateSystem; }
			set
			{
				if (_coordsysReadFromFile)
					throw new ApplicationException("Coordinate system is specified in projection file and is read only");
				_coordinateSystem = value;
			}
		}


		/// <summary>
		/// Gets the <see cref="SharpMap.Data.Providers.ShapeType">shape geometry type</see> in this shapefile.
		/// </summary>
		/// <remarks>
		/// The property isn't set until the first time the datasource has been opened,
		/// and will throw an exception if this property has been called since initialization. 
		/// <para>All the non-Null shapes in a shapefile are required to be of the same shape
		/// type.</para>
		/// </remarks>
		public ShapeType ShapeType
		{
			get { return _header.ShapeType; }
		}


		/// <summary>
		/// Gets or sets the filename of the shapefile
		/// </summary>
		/// <remarks>If the filename changes, indexes will be rebuilt</remarks>
		public string Filename
		{
			get { return _filename; }
			set
			{
				if (value != _filename)
				{
                    _filename = value;
                    _fileBasedIndex = (_fileBasedIndexWanted) && File.Exists(Path.ChangeExtension(value, ".shx"));

                    _dbfFile = Path.ChangeExtension(value, ".dbf");
                    
					ParseHeader();
					ParseProjection();
					_tree = null;
				}
			}
		}

		/// <summary>
		/// Gets or sets the encoding used for parsing strings from the DBase DBF file.
		/// </summary>
		/// <remarks>
		/// The DBase default encoding is <see cref="System.Text.Encoding.UTF8"/>.
		/// </remarks>
        public Encoding Encoding
        {
            get
            {
                Encoding ret;
                using (var DbaseFile = OpenDbfStream())
                {
                    ret = DbaseFile.Encoding;
                }
                return ret;
            }
            set { _dbfSpecifiedEncoding = value; }
        }

		#region Disposers and finalizers

		private bool _disposed;

		/// <summary>
		/// Disposes the object
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
                {
                    Close();
                    _envelope = null;
                    _tree = null;
#if USE_MEMORYMAPPED_FILE
                    if (_memMappedFilesRefConter.ContainsKey(_filename))
                    {
                        _memMappedFilesRefConter[_filename]--;
                        if (_memMappedFilesRefConter[_filename] == 0)
                        {
                            _memMappedFiles[_filename].Dispose();
                            _memMappedFiles.Remove(_filename);
                            _memMappedFilesRefConter.Remove(_filename);
                        }
                    }
                    string shxFile = Path.ChangeExtension(_filename,".shx");
                    if (_memMappedFilesRefConter.ContainsKey(shxFile))
                    {
                        _memMappedFilesRefConter[shxFile]--;
                        if (_memMappedFilesRefConter[shxFile] <= 0)
                        {
                            _memMappedFiles[shxFile].Dispose();
                            _memMappedFilesRefConter.Remove(shxFile);
                            _memMappedFiles.Remove(shxFile);

                        }
                    }
#endif
				}
				_disposed = true;
			}
		}

		/// <summary>
		/// Finalizes the object
		/// </summary>
		~ShapeFile()
		{
			Dispose();
		}

		#endregion

		#region IProvider Members

        private Stream OpenShapefileStream()
        {
            Stream s;
#if USE_MEMORYMAPPED_FILE

                s = CheckCreateMemoryMappedStream(_filename, ref _haveRegistredForUsage);
#else
            s = new FileStream(_filename, FileMode.Open, FileAccess.Read);
#endif
            InitializeShape(_filename, _fileBasedIndex);
            return s;
        }


        private DbaseReader OpenDbfStream()
        {
            DbaseReader dbfFile = null;
            if (File.Exists(_dbfFile))
            {
                dbfFile = new DbaseReader(_dbfFile);
                if (_dbfSpecifiedEncoding != null)
                    dbfFile.Encoding = _dbfSpecifiedEncoding;

                dbfFile.EncodingChanged += ClearingOfCachedDataRequired;
                dbfFile.IncludeOidChanged += ClearingOfCachedDataRequired;
                dbfFile.Open();
            }

            return dbfFile;
        }

		/// <summary>
		/// Opens the datasource
		/// </summary>
		public void Open()
		{
            //Load SpatialIndexIfNotLoaded
            if (_tree == null)
            {
                using (Stream s = OpenShapefileStream())
                {
                    s.Close();
                }
            }
//            // TODO:
//            // Get a Connector.  The connector returned is guaranteed to be connected and ready to go.
//            // Pooling.Connector connector = Pooling.ConnectorPool.ConnectorPoolManager.RequestConnector(this,true);

//        //	if (!_isOpen )
//        //	{
////                if (File.Exists(shxFile))
////                {
////#if USE_MEMORYMAPPED_FILE
////                    _fsShapeIndex = CheckCreateMemoryMappedStream(shxFile, ref _haveRegistredForShxUsage);
////#else
////                    _fsShapeIndex = new FileStream(shxFile, FileMode.Open, FileAccess.Read);
////#endif
////                    _brShapeIndex = new BinaryReader(_fsShapeIndex, Encoding.Unicode);
////                }
//#if USE_MEMORYMAPPED_FILE

//                _fsShapeFile = CheckCreateMemoryMappedStream(_filename, ref _haveRegistredForUsage);
//#else
//                _fsShapeFile = new FileStream(_filename, FileMode.Open, FileAccess.Read);
//#endif
//                //_brShapeFile = new BinaryReader(_fsShapeFile);
//                //// Create array to hold the index array for this open session
//                ////_offsetOfRecord = new int[_featureCount];
//                //_offsetOfRecord = new ShapeFileIndexEntry[_featureCount];
//                //PopulateIndexes(shxFile);
//                InitializeShape(_filename, _fileBasedIndex);
//                if (DbaseFile != null)
//                    DbaseFile.Open();
//                _isOpen = true;

//            }
		}
#if USE_MEMORYMAPPED_FILE
        private Stream CheckCreateMemoryMappedStream(string filename, ref bool haveRegistredForUsage)
        {
            if (!_memMappedFiles.ContainsKey(filename))
            {
                System.IO.MemoryMappedFiles.MemoryMappedFile memMappedFile = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(filename, FileMode.Open);
                _memMappedFiles.Add(filename, memMappedFile);
            }
            if (!haveRegistredForUsage)
            {
                if (_memMappedFilesRefConter.ContainsKey(filename))
                    _memMappedFilesRefConter[filename]++;
                else
                    _memMappedFilesRefConter.Add(filename, 1);

                haveRegistredForUsage = true;
            }

            return _memMappedFiles[filename].CreateViewStream();
        }
#endif

		/// <summary>
		/// Closes the datasource
		/// </summary>
		public void Close()
		{
			/*if (!_disposed)
			{
                if (_isOpen)
				{
					//_brShapeFile.Close();
					_fsShapeFile.Close();
                    //if (_brShapeIndex != null)
                    //{
                    //    _brShapeIndex.Close();
                    //    _fsShapeIndex.Close();
                    //}

                    //// Give back the memory from the index array.
                    //_offsetOfRecord = null;

					if (DbaseFile != null)
						DbaseFile.Close();
					_isOpen = false;
				}
            }*/
		}

		/// <summary>
		/// Returns true if the datasource is currently open
		/// </summary>		
		public bool IsOpen
		{
            get { return false; }
		}

		/// <summary>
		/// Returns geometries whose bounding box intersects 'bbox'
		/// </summary>
		/// <remarks>
		/// <para>Please note that this method doesn't guarantee that the geometries returned actually intersect 'bbox', but only
		/// that their boundingbox intersects 'bbox'.</para>
		/// <para>This method is much faster than the QueryFeatures method, because intersection tests
		/// are performed on objects simplified by their boundingbox, and using the Spatial Index.</para>
		/// </remarks>
		/// <param name="bbox"></param>
		/// <returns></returns>
		public Collection<IGeometry> GetGeometriesInView(Envelope bbox)
		{
			//Use the spatial index to get a list of features whose boundingbox intersects bbox
			var objectlist = GetObjectIDsInView(bbox);
			if (objectlist.Count == 0) //no features found. Return an empty set
				return new Collection<IGeometry>();

            if (FilterDelegate != null)
                return GetGeometriesInViewWithFilter(objectlist);

		    return GetGeometriesInViewWithoutFilter(objectlist);
		}

        private Collection<IGeometry> GetGeometriesInViewWithFilter(Collection<uint> oids)
        {
            Collection<IGeometry> result = null;
            using (Stream s = OpenShapefileStream())
            {
                using (BinaryReader br = new BinaryReader(s))
                {
                    using (DbaseReader DbaseFile = OpenDbfStream())
                    {
                        result = new Collection<IGeometry>();
                        var table = DbaseFile.NewTable;
                        var tmpOids = new Collection<uint>();
                        foreach (var oid in oids)
                        {
                            var fdr = getFeature(oid, table, br, DbaseFile);
                            if (!FilterDelegate(fdr)) continue;
                            result.Add(fdr.Geometry);
                            tmpOids.Add(oid);
                        }

                        CleanInternalCache(tmpOids);
                        DbaseFile.Close();
                    }
                    br.Close();
                }
                s.Close();
            }
            return result;
        }

        private Collection<IGeometry> GetGeometriesInViewWithoutFilter(Collection<uint> oids)
        {
            var result = new Collection<IGeometry>();
            using (Stream s = OpenShapefileStream())
            {
                using (BinaryReader br = new BinaryReader(s))
                {
                    using (DbaseReader DbaseFile = OpenDbfStream())
                    {
                        foreach (var oid in oids)
                        {
                            result.Add(GetGeometryByID(oid, br, DbaseFile));
                        }

                        DbaseFile.Close();
                    }
                    br.Close();
                }
                s.Close();
            }

            CleanInternalCache(oids);
            return result;
        }

		/// <summary>
		/// Returns all objects whose boundingbox intersects bbox.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Please note that this method doesn't guarantee that the geometries returned actually intersect 'bbox', but only
		/// that their boundingbox intersects 'bbox'.
		/// </para>
		/// </remarks>
		/// <param name="bbox"></param>
		/// <param name="ds"></param>
		/// <returns></returns>
        public void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet ds)
        {
            // Do true intersection query
            if (DoTrueIntersectionQuery)
            {
                ExecuteIntersectionQuery(Factory.ToGeometry(bbox), ds);
                return;
            }

            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            var objectlist = GetObjectIDsInView(bbox);

            using (Stream s = OpenShapefileStream())
            {
                using (BinaryReader br = new BinaryReader(s))
                {
                    using (DbaseReader DbaseFile = OpenDbfStream())
                    {
                        var dt = DbaseFile.NewTable;
                        dt.BeginLoadData();

                        for (var i = 0; i < objectlist.Count; i++)
                        {
                            FeatureDataRow fdr;
                            fdr = (FeatureDataRow)dt.LoadDataRow(DbaseFile.GetValues(objectlist[i]), true);
                            fdr.Geometry = ReadGeometry(objectlist[i], br, DbaseFile);

                            //Test if the feature data row corresponds to the FilterDelegate
                            if (FilterDelegate != null && !FilterDelegate(fdr))
                                fdr.Delete();
                        }

                        dt.EndLoadData();
                        dt.AcceptChanges();

                        ds.Tables.Add(dt);
                        DbaseFile.Close();
                    }
                    br.Close();
                }
                s.Close();
            }
            CleanInternalCache(objectlist);
        }

		/// <summary>
		/// Returns geometry Object IDs whose bounding box intersects 'bbox'
		/// </summary>
		/// <param name="bbox"></param>
		/// <returns></returns>
        public Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            var res = _tree.Search(bbox);

            /*Sort oids so we get a forward only read of the shapefile*/
            List<uint> ret = new List<uint>(res);
            ret.Sort();

            return new Collection<uint>(ret);
        }

		/// <summary>
		/// Returns the geometry corresponding to the Object ID
		/// </summary>
		/// <remarks>FilterDelegate is no longer applied to this ge</remarks>
		/// <param name="oid">Object ID</param>
		/// <returns>The geometry at the Id</returns>
        public IGeometry GetGeometryByID(uint oid)
        {
            IGeometry geom;
            using (Stream s = OpenShapefileStream())
            {
                using (BinaryReader br = new BinaryReader(s))
                {
                    using (DbaseReader dbf = OpenDbfStream())
                    {
                        geom = ReadGeometry(oid, br, dbf);
                        dbf.Close();
                    }
                    br.Close();
                }
                s.Close();
            }
            return geom;
        }
            /// <summary>
		/// Returns the geometry corresponding to the Object ID
		/// </summary>
		/// <remarks>FilterDelegate is no longer applied to this ge</remarks>
		/// <param name="oid">Object ID</param>
		/// <returns>The geometry at the Id</returns>
        private IGeometry GetGeometryByID(uint oid, BinaryReader br, DbaseReader dbf)
        {
            if (_useMemoryCache)
            {
                FeatureDataRow fdr;
                lock (_cacheLock)
                {
                    _cacheDataTable.TryGetValue(oid, out fdr);
                }

                if (fdr == null)
                {
                    fdr = GetFeature(oid);
                }

                return fdr.Geometry;
            }
            IGeometry geom = ReadGeometry(oid, br, dbf);
            return geom;
        }

        /// <summary>
        /// Gets or sets a value indicating that for <see cref="ExecuteIntersectionQuery(GeoAPI.Geometries.Envelope,SharpMap.Data.FeatureDataSet)"/> the intersection of the geometries and the envelope should be tested.
        /// </summary>
        public bool DoTrueIntersectionQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the provider should check if geometry belongs to a deleted record.
        /// </summary>
        /// <remarks>This really slows rendering performance down</remarks>
        public bool CheckIfRecordIsDeleted { get; set; }

		/// <summary>
		/// Returns the data associated with all the geometries that are intersected by <paramref name="geom"/>.
		/// </summary>
		/// <param name="geom">The geometry to test intersection for</param>
		/// <param name="ds">FeatureDataSet to fill data into</param>
        public virtual void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            var bbox = new Envelope(geom.EnvelopeInternal);

            //Get a list of objects that possibly intersect with geom.
            var objectlist = GetObjectIDsInView(bbox);

            //Get a prepared geometry object
            var prepGeom = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);

            using (Stream s = OpenShapefileStream())
            {
                using (BinaryReader br = new BinaryReader(s))
                {
                    using (DbaseReader DbaseFile = OpenDbfStream())
                    {
                        //Get an empty table
                        var dt = DbaseFile.NewTable;
                        dt.BeginLoadData();

                        var tmpOids = new Collection<uint>();
                        //Cycle through all object ids
                        foreach (var oid in objectlist)
                        {
                            //Get the geometry
                            var testGeom = ReadGeometry(oid, br, DbaseFile);

                            //We do not have a geometry => we do not have a feature
                            if (testGeom == null)
                                continue;

                            //Does the geometry really intersect with geom?
                            if (!prepGeom.Intersects(testGeom))
                                continue;

                            //Get the feature data row and assign the geometry
                            FeatureDataRow fdr;
                            fdr = (FeatureDataRow)dt.LoadDataRow(DbaseFile.GetValues(oid), true);
                            fdr.Geometry = testGeom;

                            //Test if the feature data row corresponds to the FilterDelegate
                            if (FilterDelegate != null && !FilterDelegate(fdr))
                                fdr.Delete();
                            else
                                tmpOids.Add(oid);
                        }

                        dt.EndLoadData();
                        dt.AcceptChanges();

                        ds.Tables.Add(dt);
                        DbaseFile.Close();
                        CleanInternalCache(tmpOids);
                    }
                    br.Close();
                }
                s.Close();
            }
        }


		/// <summary>
		/// Returns the total number of features in the datasource (without any filter applied)
		/// </summary>
		/// <returns></returns>
		public int GetFeatureCount()
		{
			return _index.FeatureCount;
		}

		

		/// <summary>
		/// Returns the extents of the datasource
		/// </summary>
		/// <returns></returns>
		public Envelope GetExtents()
		{
            if (_tree == null)
                throw new ApplicationException(
                    "File hasn't been spatially indexed. Try opening the datasource before retriving extents");
			return _tree.Box;
		}

		/// <summary>
		/// Gets the connection ID of the datasource
		/// </summary>
		/// <remarks>
		/// The connection ID of a shapefile is its filename
		/// </remarks>
		public string ConnectionID
		{
			get { return _filename; }
		}

		/// <summary>
		/// Gets or sets the spatial reference ID (CRS)
		/// </summary>
		public virtual int SRID
		{
			get { return _srid; }
			set
			{
			    _srid = value;
			    lock (GspLock)
			        Factory = GeometryServiceProvider.Instance.CreateGeometryFactory(value);
			}
		}

		#endregion

		private void InitializeShape(string filename, bool fileBasedIndex)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException(String.Format("Could not find file \"{0}\"", filename));
			if (!filename.ToLower().EndsWith(".shp"))
				throw (new Exception("Invalid shapefile filename: " + filename));

			LoadSpatialIndex(fileBasedIndex); //Load spatial index			
		}

	    /// <summary>
	    /// Reads and parses the header of the .shp index file
	    /// </summary>
	    private void ParseHeader()
	    {
            _header = new ShapeFileHeader(_filename);
	        var shxPath = Path.ChangeExtension(_filename, ".shx");
            if (!File.Exists(shxPath))
            {
                ShapeFileIndex.Create(_filename);
            }
            _index = new ShapeFileIndex(Path.ChangeExtension(_filename, ".shx"));

            /*
#if USE_MEMORYMAPPED_FILE
            var fsShapeFile = CheckCreateMemoryMappedStream(_filename, ref _haveRegistredForUsage);
#else
            var fsShapeFile = new FileStream(_filename, FileMode.Open, FileAccess.Read);
#endif
            using (var brShapeFile = new BinaryReader(_fsShapeFile, Encoding.Unicode))
            {
                //Check file header
                if (brShapeFile.ReadInt32() != 170328064)
                    //File Code is actually 9994, but in Little Endian Byte Order this is '170328064'
                    throw (new ApplicationException("Invalid Shapefile (.shp)"));

                //Read filelength as big-endian. The length is based on 16bit words
                fsShapeFile.Seek(24, 0); //seek to File Length
                _fileSize = 2*SwapByteOrder(brShapeFile.ReadInt32());

                fsShapeFile.Seek(32, 0); //seek to ShapeType
                _shapeType = (ShapeType) brShapeFile.ReadInt32();

                //Read the spatial bounding box of the contents
                fsShapeFile.Seek(36, 0); //seek to box
                _envelope = new Envelope(new Coordinate(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()),
                                         new Coordinate(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));

                // Work out the numberof features, if we have an index file use that
                var shxFile = Path.ChangeExtension(_filename, ".shx");
                if (File.Exists(shxFile))
                    //if (brShapeIndex != null)
                {
                    using (var brShapeIndex = new BinaryReader(
                        new FileStream(shxFile, FileMode.Open, FileAccess.Read),
                        Encoding.Unicode))
                    {
                        brShapeIndex.BaseStream.Seek(24, 0); //seek to File Length
                        var indexFileSize = SwapByteOrder(brShapeIndex.ReadInt32());
                            //Read filelength as big-endian. The length is based on 16bit words
                        _featureCount = (2*indexFileSize - 100)/8;
                            //Calculate FeatureCount. Each feature takes up 8 bytes. The header is 100 bytes
                    }
                    //#if USE_MEMORYMAPPED_FILE
                    //                _fsShapeIndex = CheckCreateMemoryMappedStream(Path.ChangeExtension(_filename, ".shx"), ref _haveRegistredForShxUsage);
                    //#else
                    //                _fsShapeIndex = new FileStream(Path.ChangeExtension(_filename, ".shx"), FileMode.Open, FileAccess.Read);
                    //#endif
                    //                _brShapeIndex = new BinaryReader(_fsShapeIndex, Encoding.Unicode);

                    //                _brShapeIndex.BaseStream.Seek(24, 0); //seek to File Length
                    //                var indexFileSize = SwapByteOrder(_brShapeIndex.ReadInt32()); //Read filelength as big-endian. The length is based on 16bit words
                    //                _featureCount = (2 * indexFileSize - 100) / 8; //Calculate FeatureCount. Each feature takes up 8 bytes. The header is 100 bytes

                    //                _brShapeIndex.Close();
                    //                _fsShapeIndex.Close();
                }
                else
                {
                    // Move to the start of the data
                    fsShapeFile.Seek(100, 0); //Skip content length
                    long offset = 100; // Start of the data records

                    // Loop through the data to extablish the number of features contained within the data file
                    while (offset < _fileSize)
                    {
                        ++_featureCount;

                        fsShapeFile.Seek(offset + 4, 0); //Skip content length
                        var dataLength = 2*SwapByteOrder(brShapeFile.ReadInt32());

                        // This is to cover the chance when the data is corupt
                        // as seen with the sample counties file, in this example the index file
                        // has been adjusted to cover the problem.
                        if ((offset + dataLength) > _fileSize)
                        {
                            --_featureCount;
                        }

                        offset += dataLength; // Add Record data length
                        offset += 8; //  Plus add the record header size
                    }
                }
                //brShapeFile.Close();
                //fsShapeFile.Close();
            }
             */
		}

		/// <summary>
		/// Reads and parses the projection if a projection file exists
		/// </summary>
		private void ParseProjection()
		{
			string projfile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename) +
							  ".prj";
            if (File.Exists(projfile))
            {
                try
                {
                    string wkt = File.ReadAllText(projfile);
#if !DotSpatialProjections
                    _coordinateSystem = (ICoordinateSystem)CoordinateSystemWktReader.Parse(wkt);
                    SRID = (int)_coordinateSystem.AuthorityCode;
#else
					_coordinateSystem = ProjectionInfo.FromEsriString(wkt);
#endif
                    _coordsysReadFromFile = true;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Coordinate system file '" + projfile +
                                       "' found, but could not be parsed. WKT parser returned:" + ex.Message);
                    throw;
                }
            }
            else
            {
                if (_coordinateSystem == null)
                    SRID = 0;
                else
                {
#if !DotSpatialProjections
                    SRID = (int) _coordinateSystem.AuthorityCode;
#else
			        SRID = _coordinateSystem.EpsgCode;
#endif
                }
            }
		}

        ///// <summary>
        ///// If an index file is present (.shx) it reads the record offsets from the .shx index file and returns the information in an array.
        ///// IfF an indexd array is not present it works out the indexes from the data file, by going through the record headers, finding the
        ///// data lengths and workingout the offsets. Which ever method is used a array of index is populated to be use by the other methods.
        ///// This array is created when the open method is called, and removed when the close method called.
        ///// </summary>
        //private void PopulateIndexes(string shxFile)
        //{
        //    if (File.Exists(shxFile))
        //    {
        //        using (var brShapeIndex = new BinaryReader(File.OpenRead(shxFile)))
        //        {
        //            brShapeIndex.BaseStream.Seek(100, 0);  //skip the header

        //            for (int x = 0; x < _featureCount; ++x)
        //            {
        //                _offsetOfRecord[x] = new ShapeFileIndexEntry(brShapeIndex);
        //                //_offsetOfRecord[x] = 2 * SwapByteOrder(brShapeIndex.ReadInt32()); //Read shape data position // ibuffer);
        //                //brShapeIndex.BaseStream.Seek(brShapeIndex.BaseStream.Position + 4, 0); //Skip content length
        //            }
        //        }
        //    }
        //    //if (_brShapeIndex != null)
        //    //{
        //    //    _brShapeIndex.BaseStream.Seek(100, 0);  //skip the header

        //    //    for (int x = 0; x < _featureCount; ++x)
        //    //    {
        //    //        _offsetOfRecord[x] = 2 * SwapByteOrder(_brShapeIndex.ReadInt32()); //Read shape data position // ibuffer);
        //    //        _brShapeIndex.BaseStream.Seek(_brShapeIndex.BaseStream.Position + 4, 0); //Skip content length
        //    //    }
        //    //}
        //    else  
        //    {
        //        // we need to create an index from the shape file

        //        // Record the current position pointer for later
        //        var oldPosition = _brShapeFile.BaseStream.Position;
  
        //        // Move to the start of the data
        //        _brShapeFile.BaseStream.Seek(100, 0); //Skip content length
        //        long offset = 100; // Start of the data records
                
        //        for (int x = 0; x < _featureCount; ++x)
        //        {
        //            var recordOffset = offset;
        //            //_offsetOfRecord[x] = (int)offset;
                   
        //            _brShapeFile.BaseStream.Seek(offset + 4, 0); //Skip content length
        //            int dataLength = 2 * SwapByteOrder(_brShapeFile.ReadInt32());
        //            _offsetOfRecord[x] = new ShapeFileIndexEntry((int)recordOffset, dataLength);
        //            offset += dataLength; // Add Record data length
        //            offset += 8; //  Plus add the record header size
        //        }

        //        // Return the position pointer
        //        _brShapeFile.BaseStream.Seek(oldPosition, 0);
        //    }
		//}

		///<summary>
		///Swaps the byte order of an int32
		///</summary>
		/// <param name="i">Integer to swap</param>
		/// <returns>Byte Order swapped int32</returns>
		internal static int SwapByteOrder(int i)
		{
			var buffer = BitConverter.GetBytes(i);
			Array.Reverse(buffer, 0, buffer.Length);
			return BitConverter.ToInt32(buffer, 0);
		}

		/// <summary>
		/// Loads a spatial index from a file. If it doesn't exist, one is created and saved
		/// </summary>
		/// <param name="filename"></param>
		/// <returns>QuadTree index</returns>
		private QuadTree CreateSpatialIndexFromFile(string filename)
		{
			if (File.Exists(filename + ".sidx"))
			{
				try
				{
				    var sw = new Stopwatch();
                    sw.Start();
                    var tree = QuadTree.FromFile(filename + ".sidx");
                    sw.Stop();
                    if (_logger.IsDebugEnabled)
                        _logger.DebugFormat("Linear creation of QuadTree took {0}ms", sw.ElapsedMilliseconds);
				    return tree;
				}
				catch (QuadTree.ObsoleteFileFormatException)
				{
					File.Delete(filename + ".sidx");
				    CreateSpatialIndexFromFile(filename);
				}
				catch (Exception ex)
				{
                    _logger.Error(ex);
					throw ex;
				}
			}

            // Need to create the spatial index from scratch
            switch (SpatialIndexCreationOption)
            {
                case SpatialIndexCreation.Linear:
                    return CreateSpatialIndexLinear(filename);
                default:
                    return CreateSpatialIndexRecursive(filename);
            }
            //tree.SaveIndex(filename + ".sidx");
		    //return tree;
		}

        /// <summary>
		/// Generates a spatial index for a specified shape file.
		/// </summary>
		/// <param name="filename"></param>
		private QuadTree CreateSpatialIndexLinear(string filename)
        {
            var extent = _envelope;
            var sw = new Stopwatch();
            sw.Start();
            var root = QuadTree.CreateRootNode(extent);
            var h = new Heuristic
                        {
                            maxdepth = (int) Math.Ceiling(Math.Log(GetFeatureCount(), 2)),
                            // These are not used for this approach
                            minerror = 10,
                            tartricnt = 5,
                            mintricnt = 2
                        };

            uint i = 0;
            foreach (var box in GetAllFeatureBoundingBoxes())
            {
                //is the box valid?
                if (!box.IsNull) continue;

                //create box object and add to root.
                var g = new QuadTree.BoxObjects {Box = box, ID = i};
                root.AddNode(g, h);
                i++;
            }

            sw.Stop();
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Linear creation of QuadTree took {0}ms", sw.ElapsedMilliseconds);
            }

            if (_fileBasedIndexWanted && !string.IsNullOrEmpty(filename))
                root.SaveIndex(filename + ".sidx");
            return root;


        }
        /// <summary>
		/// Generates a spatial index for a specified shape file.
		/// </summary>
		/// <param name="filename">The filename</param>
		private QuadTree CreateSpatialIndexRecursive(string filename)
		{
            var sw = new Stopwatch();
            sw.Start();

            var objList = new List<QuadTree.BoxObjects>();
			//Convert all the geometries to boundingboxes 
			uint i = 0;
			foreach (var box in GetAllFeatureBoundingBoxes())
			{
				if (box.IsNull) continue;

                var g = new QuadTree.BoxObjects {Box = box, ID = i};
				objList.Add(g);
				i++;
			}

			Heuristic heur;
			heur.maxdepth = (int) Math.Ceiling(Math.Log(GetFeatureCount(), 2));
			heur.minerror = 10;
			heur.tartricnt = 5;
			heur.mintricnt = 2;
            var root =  new QuadTree(objList, 0, heur);

            sw.Stop();
            if (_logger.IsDebugEnabled)
                _logger.DebugFormat("Recursive creation of QuadTree took {0}ms", sw.ElapsedMilliseconds);

            if (_fileBasedIndexWanted && !String.IsNullOrEmpty(filename))
                root.SaveIndex(filename + ".sidx");

            return root;
		}

        //private void LoadSpatialIndex()
        //{
        //    LoadSpatialIndex(false, false);
        //}

		private void LoadSpatialIndex(bool loadFromFile)
		{
			LoadSpatialIndex(false, loadFromFile);
		}

        /// <summary>
        /// Options to create the <see cref="QuadTree"/> spatial index
        /// </summary>
        public enum SpatialIndexCreation
        {
            /// <summary>
            /// Loads all the bounding boxes in builds the QuadTree from the list of nodes.
            /// This is memory expensive!
            /// </summary>
            Recursive = 0,

            /// <summary>
            /// Creates a root node by the bounds of the ShapeFile and adds each node one-by-one-
            /// </summary>
            Linear,
        }

        /// <summary>
        /// The Spatial index create
        /// </summary>
        public static SpatialIndexCreation SpatialIndexCreationOption { get; set; }


		private void LoadSpatialIndex(bool forceRebuild, bool loadFromFile)
		{
			//Only load the tree if we haven't already loaded it, or if we want to force a rebuild
			if (_tree == null || forceRebuild)
			{
			    Func<string, QuadTree> createSpatialIndex;
                if (SpatialIndexCreationOption == SpatialIndexCreation.Recursive)
                    createSpatialIndex = CreateSpatialIndexRecursive;
                else
                    createSpatialIndex = CreateSpatialIndexLinear;
                
                // Is this a web application? If so lets store the index in the cache so we don't
				// need to rebuild it for each request
                if (Web.HttpCacheUtility.IsWebContext)
                {
                    if (!Web.HttpCacheUtility.TryGetValue(_filename, out _tree))
                    {
                        if (!loadFromFile)
                            _tree = createSpatialIndex(_filename);
                        else
                            _tree = CreateSpatialIndexFromFile(_filename);
                        //Store the tree in the web cache
                        //TODO: Remove this when connection pooling is implemented
                        Web.HttpCacheUtility.TryAddValue(_filename, _tree, TimeSpan.FromDays(1));
                    }
                }
				/*
                if (HttpContext.Current != null)
				{
					//Check if the tree exists in the cache
					if (HttpContext.Current.Cache[_filename] != null)
						_tree = (QuadTree) HttpContext.Current.Cache[_filename];
					else
					{
						if (!loadFromFile)
							_tree = createSpatialIndex(_filename);
						else
							_tree = CreateSpatialIndexFromFile(_filename);
						//Store the tree in the web cache
						//TODO: Remove this when connection pooling is implemented
						HttpContext.Current.Cache.Insert(_filename, _tree, null, Cache.NoAbsoluteExpiration,
														 TimeSpan.FromDays(1));
					}
				}
                 */
				else if (!loadFromFile)
					_tree = createSpatialIndex(_filename);
				else
					_tree = CreateSpatialIndexFromFile(_filename);
			}
		}

		/// <summary>
		/// Forces a rebuild of the spatial index. If the instance of the ShapeFile provider
		/// uses a file-based index the file is rewritten to disk.
		/// </summary>
		public void RebuildSpatialIndex()
		{
			if (_fileBasedIndex)
			{
				if (File.Exists(_filename + ".sidx"))
					File.Delete(_filename + ".sidx");
				_tree = CreateSpatialIndexFromFile(_filename);
			}
			else
			{
			    switch(SpatialIndexCreationOption)
			    {
                    case SpatialIndexCreation.Linear:
                        _tree = CreateSpatialIndexLinear(_filename);
                        break;
                    default:
                        _tree = CreateSpatialIndexRecursive(_filename);
                        break;
			    }
			}

            /*
			if (HttpContext.Current != null)
				//TODO: Remove this when connection pooling is implemented:
				HttpContext.Current.Cache.Insert(_filename, _tree, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1));
             */
            if (Web.HttpCacheUtility.IsWebContext)
            {
                Web.HttpCacheUtility.TryAddValue(_filename, _tree, TimeSpan.FromDays(1));
            }
		}

        /*
	    private delegate bool RecordDeletedFunction(uint oid);
        private static bool NoRecordDeleted(uint oid)
        {
            return false;
        }
         */

		/// <summary>
		/// Reads all boundingboxes of features in the shapefile. This is used for spatial indexing.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Envelope> GetAllFeatureBoundingBoxes()
		{
			//List<BoundingBox> boxes = new List<BoundingBox>();

            /*
		    RecordDeletedFunction recDel = dbaseFile != null
		                                       ? (RecordDeletedFunction) dbaseFile.RecordDeleted
		                                       : NoRecordDeleted;
             */
		    var fsShapeFile = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		    var shapeType = _header.ShapeType;

            using (var brShapeFile = new BinaryReader(fsShapeFile, Encoding.Unicode))
		    {
		        if (shapeType == ShapeType.Point || shapeType == ShapeType.PointZ ||
		            shapeType == ShapeType.PointM)
		        {
		            for (var a = 0; a < _index.FeatureCount; ++a)
		            {
		                //if (recDel((uint)a)) continue;

                        fsShapeFile.Seek(_index.GetOffset((uint)a) + 8, 0); //skip record number and content length
		                if ((ShapeType) brShapeFile.ReadInt32() != ShapeType.Null)
		                {
		                    yield return new Envelope(new Coordinate(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
		                }
		            }
		        }
		        else
		        {
		            for (var a = 0; a < _index.FeatureCount; ++a)
		            {
		                //if (recDel((uint)a)) continue;
		                fsShapeFile.Seek(_index.GetOffset((uint)a) + 8, 0); //skip record number and content length
		                if ((ShapeType) brShapeFile.ReadInt32() != ShapeType.Null)
		                    yield return new Envelope(new Coordinate(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()),
		                                              new Coordinate(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
		                //boxes.Add(new BoundingBox(brShapeFile.ReadDouble(), brShapeFile.ReadDouble(),
		                //                          brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
		            }
		        }
		    }
		    //return boxes;
		}

	    /// <summary>
        /// Gets or sets the geometry factory
        /// </summary>
        protected IGeometryFactory Factory
	    {
	        get
	        {
                if (_srid == -1)
                    SRID = 0;
                return _factory;
	        }
	        set
	        {
	            _factory = value;
	            _srid = _factory.SRID;
	        }
	    }

	    /// <summary>
		/// Reads and parses the geometry with ID 'oid' from the ShapeFile
		/// </summary>
		/// <param name="oid">Object ID</param>
        /// <param name="br">BinaryReader of the ShapeFileStream</param>
        /// <param name="dBaseReader">dBaseReader of the DBaseFile</param>
		/// <returns>geometry</returns>
        private IGeometry ReadGeometry(uint oid, BinaryReader br, DbaseReader dBaseReader)
        {
            // Do we want to receive geometries of deleted records as well?
            if (CheckIfRecordIsDeleted)
            {
                //Test if record is deleted
                if (dBaseReader.RecordDeleted(oid)) return null;
            }

            var diff = _index.GetOffset(oid) - br.BaseStream.Position;
            br.BaseStream.Seek(diff, SeekOrigin.Current);
            return ParseGeometry(Factory, _header.ShapeType, br);
        }

        private static IGeometry ParseGeometry(IGeometryFactory factory, ShapeType shapeType, BinaryReader brGeometryStream)
        {
            //Skip record number and content length
            brGeometryStream.BaseStream.Seek(8, SeekOrigin.Current); 

            var type = (ShapeType)brGeometryStream.ReadInt32(); //Shape type
                if (type == ShapeType.Null)
                    return null;

                if (shapeType == ShapeType.Point || shapeType == ShapeType.PointM || shapeType == ShapeType.PointZ)
                {
                    return factory.CreatePoint(new Coordinate(brGeometryStream.ReadDouble(), brGeometryStream.ReadDouble()));
                }

                if (shapeType == ShapeType.Multipoint || shapeType == ShapeType.MultiPointM ||
                    shapeType == ShapeType.MultiPointZ)
                {
                    brGeometryStream.BaseStream.Seek(32, SeekOrigin.Current); //skip min/max box
                    var nPoints = brGeometryStream.ReadInt32(); // get the number of points
                    if (nPoints == 0)
                        return null;
                    var feature = new Coordinate[nPoints];
                    for (var i = 0; i < nPoints; i++)
                        feature[i] = new Coordinate(brGeometryStream.ReadDouble(), brGeometryStream.ReadDouble());

                    return factory.CreateMultiPoint(feature);
                }

                if (shapeType == ShapeType.PolyLine || shapeType == ShapeType.Polygon ||
                    shapeType == ShapeType.PolyLineM || shapeType == ShapeType.PolygonM ||
                    shapeType == ShapeType.PolyLineZ || shapeType == ShapeType.PolygonZ)
                {
                    brGeometryStream.BaseStream.Seek(32, SeekOrigin.Current); //skip min/max box

                    var nParts = brGeometryStream.ReadInt32(); // get number of parts (segments)
                    if (nParts == 0 || nParts < 0)
                        return null;

                    var nPoints = brGeometryStream.ReadInt32(); // get number of points
                    var segments = new int[nParts + 1];
                    //Read in the segment indexes
                    for (var b = 0; b < nParts; b++)
                        segments[b] = brGeometryStream.ReadInt32();
                    //add end point
                    segments[nParts] = nPoints;

                    if ((int)shapeType % 10 == 3)
                    {
                        var lineStrings = new ILineString[nParts];
                        for (var lineID = 0; lineID < nParts; lineID++)
                        {
                            var line = new Coordinate[segments[lineID + 1] - segments[lineID]];
                            var offset = segments[lineID];
                            for (var i = segments[lineID]; i < segments[lineID + 1]; i++)
                                line[i - offset] = new Coordinate(brGeometryStream.ReadDouble(), brGeometryStream.ReadDouble());
                            lineStrings[lineID] = factory.CreateLineString(line);
                        }

                        if (lineStrings.Length == 1)
                            return lineStrings[0];

                        return factory.CreateMultiLineString(lineStrings);
                    }

                    //First read all the rings
                    var rings = new ILinearRing[nParts];
                    for (var ringID = 0; ringID < nParts; ringID++)
                    {
                        var ring = new Coordinate[segments[ringID + 1] - segments[ringID]];
                        var offset = segments[ringID];
                        for (var i = segments[ringID]; i < segments[ringID + 1]; i++)
                            ring[i - offset] = new Coordinate(brGeometryStream.ReadDouble(), brGeometryStream.ReadDouble());
                        rings[ringID] = factory.CreateLinearRing(ring);
                    }

                    ILinearRing exteriorRing;
                    var isCounterClockWise = new bool[rings.Length];
                    var polygonCount = 0;
                    for (var i = 0; i < rings.Length; i++)
                    {
                        isCounterClockWise[i] = rings[i].IsCCW();
                        if (!isCounterClockWise[i])
                            polygonCount++;
                    }
                    if (polygonCount == 1) //We only have one polygon
                    {
                        exteriorRing = rings[0];
                        ILinearRing[] interiorRings = null;
                        if (rings.Length > 1)
                        {
                            interiorRings = new ILinearRing[rings.Length - 1];
                            Array.Copy(rings, 1, interiorRings, 0, interiorRings.Length);
                        }
                        return factory.CreatePolygon(exteriorRing, interiorRings);
                    }

                    var polygons = new List<IPolygon>();
                    exteriorRing = rings[0];
                    var holes = new List<ILinearRing>();

                    for (var i = 1; i < rings.Length; i++)
                    {
                        if (!isCounterClockWise[i])
                        {
                            polygons.Add(factory.CreatePolygon(exteriorRing, holes.ToArray()));
                            holes.Clear();
                            exteriorRing = rings[i];
                        }
                        else
                            holes.Add(rings[i]);
                    }
                    polygons.Add(factory.CreatePolygon(exteriorRing, holes.ToArray()));

                    return factory.CreateMultiPolygon(polygons.ToArray());
                }

	        throw (new ApplicationException("Shapefile type " + shapeType + " not supported"));
	    }

        /// <summary>
        /// Gets a <see cref="FeatureDataRow"/> from the datasource at the specified index
        /// <para/>
        /// Please note well: It is not checked whether 
        /// <list type="Bullet">
        /// <item>the data record matches the <see cref="FilterProvider.FilterDelegate"/> assigned.</item>
        /// </list>
        /// </summary>
        /// <param name="rowId">The object identifier for the record</param>
        /// <returns>The feature data row</returns>
        public FeatureDataRow GetFeature(uint rowId)
        {
            return GetFeature(rowId, null);
        }

		/// <summary>
		/// Gets a datarow from the datasource at the specified index belonging to the specified datatable
        /// <para/>
        /// Please note well: It is not checked whether 
        /// <list type="Bullet">
        /// <item>the data record matches the <see cref="FilterProvider.FilterDelegate"/> assigned.</item>
        /// </list>
        /// </summary>
        /// <param name="rowId">The object identifier for the record</param>
        /// <param name="dt">The datatable the feature should belong to.</param>
        /// <returns>The feature data row</returns>
        public FeatureDataRow GetFeature(uint rowId, FeatureDataTable dt)
        {
            FeatureDataRow ret = null;
            using (Stream s = OpenShapefileStream())
            {
                using (BinaryReader br = new BinaryReader(s))
                {
                    using (DbaseReader dbfReader = OpenDbfStream())
                    {
                        if (dt == null)
                            dt = dbfReader.NewTable;
                        ret = getFeature(rowId, dt, br, dbfReader);
                        dbfReader.Close();
                    }
                    br.Close();
                }
                s.Close();
            }
            return ret;
        }

        private FeatureDataRow getFeature(uint rowid, FeatureDataTable dt, BinaryReader br, DbaseReader dbfFileReader)
        {
            Debug.Assert(dt != null);
            if (dbfFileReader != null)
            {
                FeatureDataRow fdr;

                //MemoryCache
                if (_useMemoryCache)
                {
                    lock (_cacheLock)
                    {
                        _cacheDataTable.TryGetValue(rowid, out fdr);
                    }
                    if (fdr == null)
                    {
                        fdr = dbfFileReader.GetFeature(rowid, dt);
                        fdr.Geometry = ReadGeometry(rowid, br, dbfFileReader);
                        lock (_cacheLock)
                        {
                            // It is already present, so just pass it
                            if (_cacheDataTable.ContainsKey(rowid))
                                return fdr;

                            _cacheDataTable.Add(rowid, fdr);
                        }
                    }

                    //Make a copy to return
                    var fdrNew = dt.NewRow();
                    Array.Copy(fdr.ItemArray, 0, fdrNew.ItemArray, 0, fdr.ItemArray.Length);
                    //for (var i = 0; i < fdr.Table.Columns.Count; i++)
                    //{
                    //    fdrNew[i] = fdr[i];
                    //}
                    fdrNew.Geometry = fdr.Geometry;
                    return fdr;
                }

                fdr = dbfFileReader.GetFeature(rowid, dt);

                // GetFeature returns null if the record has deleted flag
                if (fdr == null)
                    return null;

                // Read the geometry
                fdr.Geometry = ReadGeometry(rowid, br, dbfFileReader);

                return fdr;
            }

            throw (new ApplicationException(
                "An attempt was made to read DBase data from a shapefile without a valid .DBF file"));
        }
	}
}