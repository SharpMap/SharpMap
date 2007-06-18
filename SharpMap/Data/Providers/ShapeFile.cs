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
using System.IO;
using System.Drawing;

namespace SharpMap.Data.Providers
{
	/// <summary>
	/// Shapefile geometry type.
	/// </summary>
	public enum ShapeType : int
	{
		/// <summary>
		/// Null shape with no geometric data
		/// </summary>
		Null = 0,
		/// <summary>
		/// A point consists of a pair of double-precision coordinates.
		/// SharpMap interpretes this as <see cref="SharpMap.Geometries.Point"/>
		/// </summary>
		Point = 1,
		/// <summary>
		/// PolyLine is an ordered set of vertices that consists of one or more parts. A part is a
		/// connected sequence of two or more points. Parts may or may not be connected to one
		///	another. Parts may or may not intersect one another.
		/// SharpMap interpretes this as either <see cref="SharpMap.Geometries.LineString"/> or <see cref="SharpMap.Geometries.MultiLineString"/>
		/// </summary>
		PolyLine = 3,
		/// <summary>
		/// A polygon consists of one or more rings. A ring is a connected sequence of four or more
		/// points that form a closed, non-self-intersecting loop. A polygon may contain multiple
		/// outer rings. The order of vertices or orientation for a ring indicates which side of the ring
		/// is the interior of the polygon. The neighborhood to the right of an observer walking along
		/// the ring in vertex order is the neighborhood inside the polygon. Vertices of rings defining
		/// holes in polygons are in a counterclockwise direction. Vertices for a single, ringed
		/// polygon are, therefore, always in clockwise order. The rings of a polygon are referred to
		/// as its parts.
		/// SharpMap interpretes this as either <see cref="SharpMap.Geometries.Polygon"/> or <see cref="SharpMap.Geometries.MultiPolygon"/>
		/// </summary>
		Polygon = 5,
		/// <summary>
		/// A MultiPoint represents a set of points.
		/// SharpMap interpretes this as <see cref="SharpMap.Geometries.MultiPoint"/>
		/// </summary>
		Multipoint = 8,
		/// <summary>
		/// A PointZ consists of a triplet of double-precision coordinates plus a measure.
		/// SharpMap interpretes this as <see cref="SharpMap.Geometries.Point"/>
		/// </summary>
		PointZ = 11,
		/// <summary>
		/// A PolyLineZ consists of one or more parts. A part is a connected sequence of two or
		/// more points. Parts may or may not be connected to one another. Parts may or may not
		/// intersect one another.
		/// SharpMap interpretes this as <see cref="SharpMap.Geometries.LineString"/> or <see cref="SharpMap.Geometries.MultiLineString"/>
		/// </summary>
		PolyLineZ = 13,
		/// <summary>
		/// A PolygonZ consists of a number of rings. A ring is a closed, non-self-intersecting loop.
		/// A PolygonZ may contain multiple outer rings. The rings of a PolygonZ are referred to as
		/// its parts.
		/// SharpMap interpretes this as either <see cref="SharpMap.Geometries.Polygon"/> or <see cref="SharpMap.Geometries.MultiPolygon"/>
		/// </summary>
		PolygonZ = 15,
		/// <summary>
		/// A MultiPointZ represents a set of <see cref="PointZ"/>s.
		/// SharpMap interpretes this as <see cref="SharpMap.Geometries.MultiPoint"/>
		/// </summary>
		MultiPointZ = 18,
		/// <summary>
		/// A PointM consists of a pair of double-precision coordinates in the order X, Y, plus a measure M.
		/// SharpMap interpretes this as <see cref="SharpMap.Geometries.Point"/>
		/// </summary>
		PointM = 21,
		/// <summary>
		/// A shapefile PolyLineM consists of one or more parts. A part is a connected sequence of
		/// two or more points. Parts may or may not be connected to one another. Parts may or may
		/// not intersect one another.
		/// SharpMap interpretes this as <see cref="SharpMap.Geometries.LineString"/> or <see cref="SharpMap.Geometries.MultiLineString"/>
		/// </summary>
		PolyLineM = 23,
		/// <summary>
		/// A PolygonM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
		/// SharpMap interpretes this as either <see cref="SharpMap.Geometries.Polygon"/> or <see cref="SharpMap.Geometries.MultiPolygon"/>
		/// </summary>
		PolygonM = 25,
		/// <summary>
		/// A MultiPointM represents a set of <see cref="PointM"/>s.
		/// SharpMap interpretes this as <see cref="SharpMap.Geometries.MultiPoint"/>
		/// </summary>
		MultiPointM = 28,
		/// <summary>
		/// A MultiPatch consists of a number of surface patches. Each surface patch describes a
		/// surface. The surface patches of a MultiPatch are referred to as its parts, and the type of
		/// part controls how the order of vertices of an MultiPatch part is interpreted.
		/// SharpMap doesn't support this feature type.
		/// </summary>
		MultiPatch = 31
	};

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
	public class ShapeFile : SharpMap.Data.Providers.IProvider, IDisposable
	{
		private ShapeType _ShapeType;
		private string _Filename;
		private SharpMap.Geometries.BoundingBox _Envelope;
		private DbaseReader dbaseFile;
		private FileStream fsShapeIndex;
		private BinaryReader brShapeIndex;
		private FileStream fsShapeFile;
		private BinaryReader brShapeFile;
		private bool _FileBasedIndex;
		private bool _IsOpen;
		private bool _CoordsysReadFromFile = false;
		
		//private int[] _LengthOfRecord;
		private int _FeatureCount;

		/// <summary>
		/// Tree used for fast query of data
		/// </summary>
		private SharpMap.Utilities.SpatialIndexing.QuadTree tree;

		/// <summary>
		/// Initializes a ShapeFile DataProvider without a file-based spatial index.
		/// </summary>
		/// <param name="filename">Path to shape file</param>
		public ShapeFile(string filename) : this(filename,false) { }

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
			_Filename = filename;
			_FileBasedIndex = fileBasedIndex;

			//Initialize DBF
			string dbffile = _Filename.Substring(0, _Filename.LastIndexOf(".")) + ".dbf";
			if (File.Exists(dbffile))
				dbaseFile = new DbaseReader(dbffile);
			//Parse shape header
			ParseHeader();
			//Read projection file
			ParseProjection();
		}

		/// <summary>
		/// Opens the datasource
		/// </summary>
		public void Open()
		{
			// TODO:
			// Get a Connector.  The connector returned is guaranteed to be connected and ready to go.
			// Pooling.Connector connector = Pooling.ConnectorPool.ConnectorPoolManager.RequestConnector(this,true);

			if (!_IsOpen)
			{
				fsShapeIndex = new FileStream(_Filename.Remove(_Filename.Length - 4, 4) + ".shx", FileMode.Open, FileAccess.Read);
				brShapeIndex = new BinaryReader(fsShapeIndex, System.Text.Encoding.Unicode);
				fsShapeFile = new FileStream(_Filename, FileMode.Open, FileAccess.Read);
				brShapeFile = new BinaryReader(fsShapeFile);
				InitializeShape(_Filename, _FileBasedIndex);
				if (dbaseFile != null)
					dbaseFile.Open();
				_IsOpen = true;
			}
		}

		/// <summary>
		/// Closes the datasource
		/// </summary>
		public void Close()
		{
			if (!disposed)
			{
				//TODO: (ConnectionPooling)
				/*	if (connector != null)
					{ Pooling.ConnectorPool.ConnectorPoolManager.Release...()
				}*/
				if (_IsOpen)
				{
					brShapeFile.Close();
					fsShapeFile.Close();
					brShapeIndex.Close();
					fsShapeIndex.Close();
					if (dbaseFile != null)
						dbaseFile.Close();
					_IsOpen = false;
				}
			}
		}

		private void InitializeShape(string filename, bool FileBasedIndex)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException(String.Format("Could not find file \"{0}\"", filename));
			if (!filename.ToLower().EndsWith(".shp"))
				throw (new System.Exception("Invalid shapefile filename: " + filename));

			LoadSpatialIndex(FileBasedIndex); //Load spatial index			
		}

		private SharpMap.CoordinateSystems.ICoordinateSystem _CoordinateSystem;

		/// <summary>
		/// Gets or sets the coordinate system of the ShapeFile. If a shapefile has 
		/// a corresponding [filename].prj file containing a Well-Known Text 
		/// description of the coordinate system this will automatically be read.
		/// If this is not the case, the coordinate system will default to null.
		/// </summary>
		/// <exception cref="ApplicationException">An exception is thrown if the coordinate system is read from file.</exception>
		public SharpMap.CoordinateSystems.ICoordinateSystem CoordinateSystem
		{
			get { return _CoordinateSystem; }
			set {
				if (_CoordsysReadFromFile)
					throw new ApplicationException("Coordinate system is specified in projection file and is read only");
				_CoordinateSystem = value; }
		}
	

		/// <summary>
		/// Returns true if the datasource is currently open
		/// </summary>		
		public bool IsOpen
		{
			get { return _IsOpen; }
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
			get {
				return _ShapeType; }
		}
	
		
		/// <summary>
		/// Gets or sets the filename of the shapefile
		/// </summary>
		/// <remarks>If the filename changes, indexes will be rebuilt</remarks>
		public string Filename
		{
			get { return _Filename; }
			set {
				if (value != _Filename)
				{
					_Filename = value;
					if (this.IsOpen)
						throw new ApplicationException("Cannot change filename while datasource is open");

					ParseHeader();
					ParseProjection();
					tree = null;
				}
			}
		}

		/// <summary>
		/// Gets or sets the encoding used for parsing strings from the DBase DBF file.
		/// </summary>
		/// <remarks>
		/// The DBase default encoding is <see cref="System.Text.Encoding.UTF7"/>.
		/// </remarks>
		public System.Text.Encoding Encoding
		{
			get { return dbaseFile.Encoding; }
			set { dbaseFile.Encoding = value; }
		}

		#region Disposers and finalizers

		private bool disposed = false;

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
			if (!disposed)
			{
				if (disposing)
				{
					Close();
					_Envelope = null;
					tree = null;
				}
				disposed = true;
			}
		}

		/// <summary>
		/// Finalizes the object
		/// </summary>
		~ShapeFile()
		{
			this.Dispose();
		}
		#endregion

		
		/// <summary>
		/// Reads and parses the header of the .shx index file
		/// </summary>
		private void ParseHeader()
		{
			fsShapeIndex = new FileStream(_Filename.Remove(_Filename.Length - 4, 4) + ".shx", FileMode.Open, FileAccess.Read);
			brShapeIndex = new BinaryReader(fsShapeIndex, System.Text.Encoding.Unicode);
			
			brShapeIndex.BaseStream.Seek(0, 0);
			//Check file header
			if (brShapeIndex.ReadInt32() != 170328064) //File Code is actually 9994, but in Little Endian Byte Order this is '170328064'
				throw (new ApplicationException("Invalid Shapefile Index (.shx)"));

			brShapeIndex.BaseStream.Seek(24, 0); //seek to File Length
			int IndexFileSize = SwapByteOrder(brShapeIndex.ReadInt32()); //Read filelength as big-endian. The length is based on 16bit words
			_FeatureCount = (2 * IndexFileSize - 100) / 8; //Calculate FeatureCount. Each feature takes up 8 bytes. The header is 100 bytes

			brShapeIndex.BaseStream.Seek(32, 0); //seek to ShapeType
			_ShapeType = (ShapeType)brShapeIndex.ReadInt32();

			//Read the spatial bounding box of the contents
			brShapeIndex.BaseStream.Seek(36, 0); //seek to box
			_Envelope = new SharpMap.Geometries.BoundingBox(brShapeIndex.ReadDouble(), brShapeIndex.ReadDouble(), brShapeIndex.ReadDouble(), brShapeIndex.ReadDouble());

			brShapeIndex.Close();
			fsShapeIndex.Close();			
		}
		/// <summary>
		/// Reads and parses the projection if a projection file exists
		/// </summary>
		private void ParseProjection()
		{
			string projfile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename) + ".prj";
			if (System.IO.File.Exists(projfile))
			{
				try
				{
					string wkt = System.IO.File.ReadAllText(projfile);
					_CoordinateSystem = (SharpMap.CoordinateSystems.ICoordinateSystem)SharpMap.Converters.WellKnownText.CoordinateSystemWktReader.Parse(wkt);
					_CoordsysReadFromFile = true;
				}
				catch(System.Exception ex) {
					System.Diagnostics.Trace.TraceWarning("Coordinate system file '" + projfile + "' found, but could not be parsed. WKT parser returned:" + ex.Message);
					throw (ex);
				}
			}
		}

		/// <summary>
		/// Reads the record offsets from the .shx index file and returns the information in an array
		/// </summary>
		private int[] ReadIndex()
		{
			int[] OffsetOfRecord = new int[ _FeatureCount ];
			brShapeIndex.BaseStream.Seek(100, 0);  //skip the header
			
			for (int x=0; x < _FeatureCount; ++x ) 
			{
				OffsetOfRecord[x] = 2 * SwapByteOrder(brShapeIndex.ReadInt32()); //Read shape data position // ibuffer);
				brShapeIndex.BaseStream.Seek(brShapeIndex.BaseStream.Position + 4, 0); //Skip content length
			}
			return OffsetOfRecord;
		}

		/// <summary>
		/// Gets the file position of the n'th shape
		/// </summary>
		/// <param name="n">Shape ID</param>
		/// <returns></returns>
		private int GetShapeIndex(uint n)
		{
			brShapeIndex.BaseStream.Seek(100+n*8, 0);  //seek to the position of the index
			return 2 * SwapByteOrder(brShapeIndex.ReadInt32()); //Read shape data position
		}

		///<summary>
		///Swaps the byte order of an int32
		///</summary>
		/// <param name="i">Integer to swap</param>
		/// <returns>Byte Order swapped int32</returns>
		private int SwapByteOrder (int i) 
		{
			byte[] buffer = BitConverter.GetBytes(i);
			Array.Reverse(buffer, 0, buffer.Length);	
			return BitConverter.ToInt32(buffer, 0);
		}

		/// <summary>
		/// Loads a spatial index from a file. If it doesn't exist, one is created and saved
		/// </summary>
		/// <param name="filename"></param>
		/// <returns>QuadTree index</returns>
		private Utilities.SpatialIndexing.QuadTree CreateSpatialIndexFromFile(string filename)
		{
			if (System.IO.File.Exists(filename + ".sidx"))
			{
				try
				{
					return Utilities.SpatialIndexing.QuadTree.FromFile(filename + ".sidx");
				}
				catch(Utilities.SpatialIndexing.QuadTree.ObsoleteFileFormatException)
				{
					System.IO.File.Delete(filename + ".sidx");
					return CreateSpatialIndexFromFile(filename);
				}
				catch (System.Exception ex) { throw ex; }
			}
			else
			{
				Utilities.SpatialIndexing.QuadTree tree = CreateSpatialIndex(_Filename);
				tree.SaveIndex(filename + ".sidx");
				return tree;
			}
		}

		/// <summary>
		/// Generates a spatial index for a specified shape file.
		/// </summary>
		/// <param name="filename"></param>
		private Utilities.SpatialIndexing.QuadTree CreateSpatialIndex(string filename)
		{
			List<Utilities.SpatialIndexing.QuadTree.BoxObjects> objList = new List<Utilities.SpatialIndexing.QuadTree.BoxObjects>();
			//Convert all the geometries to boundingboxes 
			uint i = 0;
			foreach (SharpMap.Geometries.BoundingBox box in GetAllFeatureBoundingBoxes())
			{
				if (!double.IsNaN(box.Left) && !double.IsNaN(box.Right) && !double.IsNaN(box.Bottom) && !double.IsNaN(box.Top))
				{
					Utilities.SpatialIndexing.QuadTree.BoxObjects g = new Utilities.SpatialIndexing.QuadTree.BoxObjects();
					g.box = box;
					g.ID = i;
					objList.Add(g);
					i++;
				}
			}

			Utilities.SpatialIndexing.Heuristic heur;
			heur.maxdepth = (int)Math.Ceiling(Math.Log(this.GetFeatureCount(), 2));
			heur.minerror = 10;
			heur.tartricnt = 5;
			heur.mintricnt = 2;
			return new Utilities.SpatialIndexing.QuadTree(objList, 0, heur);
		}

		private void LoadSpatialIndex() { LoadSpatialIndex(false,false); }
		private void LoadSpatialIndex(bool LoadFromFile) { LoadSpatialIndex(false, LoadFromFile); }
		private void LoadSpatialIndex(bool ForceRebuild, bool LoadFromFile)
		{
			//Only load the tree if we haven't already loaded it, or if we want to force a rebuild
			if (tree == null || ForceRebuild)
			{
				// Is this a web application? If so lets store the index in the cache so we don't
				// need to rebuild it for each request
				if (System.Web.HttpContext.Current != null)
				{
					//Check if the tree exists in the cache
					if (System.Web.HttpContext.Current.Cache[_Filename] != null)
						tree = (Utilities.SpatialIndexing.QuadTree)System.Web.HttpContext.Current.Cache[_Filename];
					else
					{
						if (!LoadFromFile)
							tree = CreateSpatialIndex(_Filename);
						else
							tree = CreateSpatialIndexFromFile(_Filename);
						//Store the tree in the web cache
						//TODO: Remove this when connection pooling is implemented
						System.Web.HttpContext.Current.Cache.Insert(_Filename, tree, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1));
					}
				}
				else
					if (!LoadFromFile)
						tree = CreateSpatialIndex(_Filename);
					else
						tree = CreateSpatialIndexFromFile(_Filename);
			}
		}

		/// <summary>
		/// Forces a rebuild of the spatial index. If the instance of the ShapeFile provider
		/// uses a file-based index the file is rewritten to disk.
		/// </summary>
		public void RebuildSpatialIndex()
		{
			if (this._FileBasedIndex)
			{
				if (System.IO.File.Exists(_Filename + ".sidx"))
					System.IO.File.Delete(_Filename + ".sidx");
				tree = CreateSpatialIndexFromFile(_Filename);
			}
			else
				tree = CreateSpatialIndex(_Filename);
			if (System.Web.HttpContext.Current != null)
				//TODO: Remove this when connection pooling is implemented:
				System.Web.HttpContext.Current.Cache.Insert(_Filename, tree, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1));
		}

		/// <summary>
		/// Reads all boundingboxes of features in the shapefile. This is used for spatial indexing.
		/// </summary>
		/// <returns></returns>
		private List<SharpMap.Geometries.BoundingBox> GetAllFeatureBoundingBoxes()
		{
			int[] offsetOfRecord = ReadIndex(); //Read the whole .idx file

			List<SharpMap.Geometries.BoundingBox> boxes = new List<SharpMap.Geometries.BoundingBox>();
			
			if (_ShapeType == ShapeType.Point)
			{
				for (int a = 0; a < _FeatureCount; ++a)
				{
					fsShapeFile.Seek(offsetOfRecord[a]+8, 0); //skip record number and content length
					if ((ShapeType)brShapeFile.ReadInt32() != ShapeType.Null)
					{
						double x = brShapeFile.ReadDouble();
						double y = brShapeFile.ReadDouble();
						boxes.Add(new SharpMap.Geometries.BoundingBox(x, y, x, y));
					}
				}
			}
			else
			{
				for (int a = 0; a < _FeatureCount; ++a)
				{
					fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); //skip record number and content length
					if ((ShapeType)brShapeFile.ReadInt32() != ShapeType.Null)
						boxes.Add(new SharpMap.Geometries.BoundingBox(brShapeFile.ReadDouble(), brShapeFile.ReadDouble(), brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
				}
			}
			return boxes;
		}

		#region IProvider Members

		/// <summary>
		/// Returns geometries whose bounding box intersects 'bbox'
		/// </summary>
		/// <remarks>
		/// <para>Please note that this method doesn't guarantee that the geometries returned actually intersect 'bbox', but only
		/// that their boundingbox intersects 'bbox'.</para>
		/// <para>This method is much faster than the QueryFeatures method, because intersection tests
		/// are performed on objects simplifed by their boundingbox, and using the Spatial Index.</para>
		/// </remarks>
		/// <param name="bbox"></param>
		/// <returns></returns>
		public Collection<SharpMap.Geometries.Geometry> GetGeometriesInView(SharpMap.Geometries.BoundingBox bbox)
		{
			//Use the spatial index to get a list of features whose boundingbox intersects bbox
			Collection<uint> objectlist = GetObjectIDsInView(bbox);
			if (objectlist.Count == 0) //no features found. Return an empty set
				return new Collection<SharpMap.Geometries.Geometry>();

            //Collection<SharpMap.Geometries.Geometry> geometries = new Collection<SharpMap.Geometries.Geometry>(objectlist.Count);
            Collection<SharpMap.Geometries.Geometry> geometries = new Collection<SharpMap.Geometries.Geometry>();

			for (int i = 0; i < objectlist.Count; i++)
			{
				SharpMap.Geometries.Geometry g = GetGeometryByID(objectlist[i]);
				if(g!=null)
					geometries.Add(g);
			}
			return geometries;
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
		public void ExecuteIntersectionQuery(SharpMap.Geometries.BoundingBox bbox, SharpMap.Data.FeatureDataSet ds)
		{
			//Use the spatial index to get a list of features whose boundingbox intersects bbox
			Collection<uint> objectlist = GetObjectIDsInView(bbox);
			SharpMap.Data.FeatureDataTable dt = dbaseFile.NewTable;

			for (int i = 0; i < objectlist.Count; i++)
			{
				SharpMap.Data.FeatureDataRow fdr = dbaseFile.GetFeature(objectlist[i], dt);
				fdr.Geometry = ReadGeometry(objectlist[i]);
				if (fdr.Geometry != null)
					if (fdr.Geometry.GetBoundingBox().Intersects(bbox))
						if (FilterDelegate == null || FilterDelegate(fdr))
							dt.AddRow(fdr);
			}
			ds.Tables.Add(dt);
		}

		/// <summary>
		/// Returns geometry Object IDs whose bounding box intersects 'bbox'
		/// </summary>
		/// <param name="bbox"></param>
		/// <returns></returns>
		public Collection<uint> GetObjectIDsInView(SharpMap.Geometries.BoundingBox bbox)
		{
			if (!this.IsOpen)
				throw (new ApplicationException("An attempt was made to read from a closed datasource"));
			//Use the spatial index to get a list of features whose boundingbox intersects bbox
			return tree.Search(bbox);
		}

		/// <summary>
		/// Returns the geometry corresponding to the Object ID
		/// </summary>
		/// <param name="oid">Object ID</param>
		/// <returns>geometry</returns>
		public SharpMap.Geometries.Geometry GetGeometryByID(uint oid)
		{
			if (FilterDelegate != null) //Apply filtering
			{
				FeatureDataRow fdr = GetFeature(oid);
				if (fdr!=null)
					return fdr.Geometry;
				else
					return null;
			}
			else return ReadGeometry(oid);
		}

		/// <summary>
		/// Reads and parses the geometry with ID 'oid' from the ShapeFile
		/// </summary>
		/// <remarks><see cref="FilterDelegate">Filtering</see> is not applied to this method</remarks>
		/// <param name="oid">Object ID</param>
		/// <returns>geometry</returns>
		private SharpMap.Geometries.Geometry ReadGeometry(uint oid)
		{		
			brShapeFile.BaseStream.Seek(GetShapeIndex(oid) + 8, 0); //Skip record number and content length
			ShapeType type = (ShapeType)brShapeFile.ReadInt32(); //Shape type
			if (type == ShapeType.Null)
				return null;
			if (_ShapeType == ShapeType.Point || _ShapeType==ShapeType.PointM || _ShapeType==ShapeType.PointZ)
			{
				SharpMap.Geometries.Point tempFeature = new SharpMap.Geometries.Point();
				return new SharpMap.Geometries.Point(brShapeFile.ReadDouble(), brShapeFile.ReadDouble());
			}
			else if (_ShapeType == ShapeType.Multipoint || _ShapeType == ShapeType.MultiPointM || _ShapeType == ShapeType.MultiPointZ)
			{
				brShapeFile.BaseStream.Seek(32 + brShapeFile.BaseStream.Position, 0); //skip min/max box
				SharpMap.Geometries.MultiPoint feature = new SharpMap.Geometries.MultiPoint();
				int nPoints = brShapeFile.ReadInt32(); // get the number of points
				if (nPoints == 0)
					return null;
				for (int i = 0; i < nPoints; i++)
					feature.Points.Add(new SharpMap.Geometries.Point(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));

				return feature;
			}
			else if (	_ShapeType == ShapeType.PolyLine || _ShapeType == ShapeType.Polygon ||
						_ShapeType == ShapeType.PolyLineM || _ShapeType == ShapeType.PolygonM ||
						_ShapeType == ShapeType.PolyLineZ || _ShapeType == ShapeType.PolygonZ)
			{
				brShapeFile.BaseStream.Seek(32 + brShapeFile.BaseStream.Position, 0); //skip min/max box

				int nParts = brShapeFile.ReadInt32(); // get number of parts (segments)
				if (nParts == 0)
					return null;
				int nPoints = brShapeFile.ReadInt32(); // get number of points

				int[] segments = new int[nParts + 1];
				//Read in the segment indexes
				for (int b = 0; b < nParts; b++)
					segments[b] = brShapeFile.ReadInt32();
				//add end point
				segments[nParts] = nPoints;

				if ((int)_ShapeType%10 == 3)
				{
					SharpMap.Geometries.MultiLineString mline = new SharpMap.Geometries.MultiLineString();
					for (int LineID = 0; LineID < nParts; LineID++)
					{
						SharpMap.Geometries.LineString line = new SharpMap.Geometries.LineString();
						for (int i = segments[LineID]; i < segments[LineID + 1]; i++)
							line.Vertices.Add(new SharpMap.Geometries.Point(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
						mline.LineStrings.Add(line);
					}
					if (mline.LineStrings.Count == 1)
						return mline[0];
					return mline;
				}
				else //(_ShapeType == ShapeType.Polygon etc...)
				{
					
					//First read all the rings
					List<SharpMap.Geometries.LinearRing> rings = new List<SharpMap.Geometries.LinearRing>();
					for (int RingID = 0; RingID < nParts; RingID++)
					{
						SharpMap.Geometries.LinearRing ring = new SharpMap.Geometries.LinearRing();
						for (int i = segments[RingID]; i < segments[RingID + 1]; i++)
							ring.Vertices.Add(new SharpMap.Geometries.Point(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
						rings.Add(ring);
					}
					bool[] IsCounterClockWise = new bool[rings.Count];
					int PolygonCount = 0;
					for (int i = 0; i < rings.Count;i++)
					{
						IsCounterClockWise[i] = rings[i].IsCCW();
						if (!IsCounterClockWise[i])
							PolygonCount++;
					}
					if (PolygonCount == 1) //We only have one polygon
					{
						SharpMap.Geometries.Polygon poly = new SharpMap.Geometries.Polygon();
						poly.ExteriorRing = rings[0];
						if (rings.Count > 1)
							for (int i = 1; i < rings.Count; i++)
								poly.InteriorRings.Add(rings[i]);
						return poly;
					}
					else
					{
						SharpMap.Geometries.MultiPolygon mpoly = new SharpMap.Geometries.MultiPolygon();
						SharpMap.Geometries.Polygon poly = new SharpMap.Geometries.Polygon();
						poly.ExteriorRing = rings[0];
						for (int i = 1; i < rings.Count;i++)
						{
							if (!IsCounterClockWise[i])
							{
								mpoly.Polygons.Add(poly);
								poly = new SharpMap.Geometries.Polygon(rings[i]);
							}
							else
								poly.InteriorRings.Add(rings[i]);
						}
						mpoly.Polygons.Add(poly);
						return mpoly;
					}					
				}
			}
			else
				throw (new ApplicationException("Shapefile type " + _ShapeType.ToString() + " not supported"));
		}

		/// <summary>
		/// Returns the data associated with all the geometries that are intersected by 'geom'.
		/// Please note that the ShapeFile provider currently doesn't fully support geometryintersection
		/// and thus only BoundingBox/BoundingBox querying are performed. The results are NOT
		/// guaranteed to lie withing 'geom'.
		/// </summary>
		/// <param name="geom"></param>
		/// <param name="ds">FeatureDataSet to fill data into</param>
		public void ExecuteIntersectionQuery(SharpMap.Geometries.Geometry geom, FeatureDataSet ds)
		{			
			SharpMap.Data.FeatureDataTable dt = (SharpMap.Data.FeatureDataTable)dbaseFile.NewTable;
			SharpMap.Geometries.BoundingBox bbox = geom.GetBoundingBox();
			//Get candidates by intersecting the spatial index tree
			Collection<uint> objectlist = tree.Search(bbox);

			if (objectlist.Count == 0)
				return;

			for (int j = 0; j < objectlist.Count; j++)
			{
				for (uint i = (uint)dt.Rows.Count - 1; i >= 0; i--)
				{
					FeatureDataRow fdr = GetFeature(objectlist[j],dt);
					if (fdr.Geometry != null)
						if (fdr.Geometry.GetBoundingBox().Intersects(bbox))
						//replace above line with this:  if(fdr.Geometry.Intersects(bbox))  when relation model is complete
							if (FilterDelegate == null || FilterDelegate(fdr))
								dt.AddRow(fdr);
				}
			}
			ds.Tables.Add(dt);
		}
	

		/// <summary>
		/// Returns the total number of features in the datasource (without any filter applied)
		/// </summary>
		/// <returns></returns>
		public int GetFeatureCount()
		{
			return _FeatureCount;
		}

		/// <summary>
		/// Filter Delegate Method
		/// </summary>
		/// <remarks>
		/// The FilterMethod delegate is used for applying a method that filters data from the dataset.
		/// The method should return 'true' if the feature should be included and false if not.
		/// <para>See the <see cref="FilterDelegate"/> property for more info</para>
		/// </remarks>
		/// <seealso cref="FilterDelegate"/>
		/// <param name="dr"><see cref="SharpMap.Data.FeatureDataRow"/> to test on</param>
		/// <returns>true if this feature should be included, false if it should be filtered</returns>
		public delegate bool FilterMethod(SharpMap.Data.FeatureDataRow dr);
		private FilterMethod _FilterDelegate;
		/// <summary>
		/// Filter Delegate Method for limiting the datasource
		/// </summary>
		/// <remarks>
		/// <example>
		/// Using an anonymous method for filtering all features where the NAME column starts with S:
		/// <code lang="C#">
		/// myShapeDataSource.FilterDelegate = new SharpMap.Data.Providers.ShapeFile.FilterMethod(delegate(SharpMap.Data.FeatureDataRow row) { return (!row["NAME"].ToString().StartsWith("S")); });
		/// </code>
		/// </example>
		/// <example>
		/// Declaring a delegate method for filtering (multi)polygon-features whose area is larger than 5.
		/// <code>
		/// myShapeDataSource.FilterDelegate = CountryFilter;
		/// [...]
		/// public static bool CountryFilter(SharpMap.Data.FeatureDataRow row)
		/// {
		///		if(row.Geometry.GetType()==typeof(SharpMap.Geometries.Polygon))
		///			return ((row.Geometry as SharpMap.Geometries.Polygon).Area>5);
		///		if (row.Geometry.GetType() == typeof(SharpMap.Geometries.MultiPolygon))
		///			return ((row.Geometry as SharpMap.Geometries.MultiPolygon).Area > 5);
		///		else return true;
		/// }
		/// </code>
		/// </example>
		/// </remarks>
		/// <seealso cref="FilterMethod"/>
		public FilterMethod FilterDelegate
		{
			get
			{
				return _FilterDelegate;
			}
			set
			{
				_FilterDelegate = value;
			}
		}

		/*
		/// <summary>
		/// Returns a colleciton of columns from the datasource [NOT IMPLEMENTED]
		/// </summary>
		public System.Data.DataColumnCollection Columns
		{
			get {
				if (dbaseFile != null)
				{
					System.Data.DataTable dt = dbaseFile.DataTable;
					return dt.Columns;
				}
				else
					throw (new ApplicationException("An attempt was made to read DBase data from a shapefile without a valid .DBF file"));
			}
		}*/

		/// <summary>
		/// Gets a datarow from the datasource at the specified index
		/// </summary>
		/// <param name="RowID"></param>
		/// <returns></returns>
		public SharpMap.Data.FeatureDataRow GetFeature(uint RowID)
		{
			return GetFeature(RowID, null);
		}

		/// <summary>
		/// Gets a datarow from the datasource at the specified index belonging to the specified datatable
		/// </summary>
		/// <param name="RowID"></param>
		/// <param name="dt">Datatable to feature should belong to.</param>
		/// <returns></returns>
		public SharpMap.Data.FeatureDataRow GetFeature(uint RowID, FeatureDataTable dt)
		{
			if (dbaseFile != null)
			{
				SharpMap.Data.FeatureDataRow dr = (SharpMap.Data.FeatureDataRow)dbaseFile.GetFeature(RowID, (dt==null)?dbaseFile.NewTable:dt);
				dr.Geometry = ReadGeometry(RowID);
				if (FilterDelegate == null || FilterDelegate(dr))
					return dr;
				else
					return null;
			}
			else
				throw (new ApplicationException("An attempt was made to read DBase data from a shapefile without a valid .DBF file"));
		}

		/// <summary>
		/// Returns the extents of the datasource
		/// </summary>
		/// <returns></returns>
		public SharpMap.Geometries.BoundingBox GetExtents()
		{
			if (tree == null)
				throw new ApplicationException("File hasn't been spatially indexed. Try opening the datasource before retriving extents");
			return tree.Box;
		}

		/// <summary>
		/// Gets the connection ID of the datasource
		/// </summary>
		/// <remarks>
		/// The connection ID of a shapefile is its filename
		/// </remarks>
		public string ConnectionID
		{
			get { return this._Filename; }
		}

		private int _SRID = -1;
		/// <summary>
		/// Gets or sets the spatial reference ID (CRS)
		/// </summary>
		public int SRID
		{
			get { return _SRID; }
			set { _SRID = value; }
		}
		
		#endregion
	}
}
