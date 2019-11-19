// Copyright 2007: Christian Graefe
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
using System.Collections.ObjectModel;
using Common.Logging;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Extensions.Data;
//using SharpMap.Geometries;
using BoundingBox = GeoAPI.Geometries.Envelope;
using Geometry=GeoAPI.Geometries.IGeometry;
using OgrOgr = OSGeo.OGR.Ogr;
using OgrDataSource = OSGeo.OGR.DataSource;
using OgrLayer = OSGeo.OGR.Layer;
using OgrGeometry = OSGeo.OGR.Geometry;
using OgrEnvelope = OSGeo.OGR.Envelope;
using OgrFeature = OSGeo.OGR.Feature;
using OgrFeatureDefn = OSGeo.OGR.FeatureDefn;
using OgrFieldDefn = OSGeo.OGR.FieldDefn;
using OgrFieldType = OSGeo.OGR.FieldType;
using OsrSpatialReference = OSGeo.OSR.SpatialReference;
using OgrGeometryType = OSGeo.OGR.wkbGeometryType;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Ogr provider for SharpMap
    /// Using the csharp and native dlls provided with FwTools. See version FWToolsVersion property below.
    /// <code>
    /// SharpMap.Layers.VectorLayer vLayerOgr = new SharpMap.Layers.VectorLayer("MapInfoLayer");
    /// vLayerOgr.DataSource = new SharpMap.Data.Providers.Ogr(@"D:\GeoData\myWorld.tab");
    /// </code>
    /// </summary>
    [Serializable]
    public class Ogr : BaseProvider
    {
        private static ILog _log = LogManager.GetLogger<Ogr>();

        static Ogr()
        {
            GdalConfiguration.ConfigureOgr();
        }

        #region Fields

        [NonSerialized]
        private BoundingBox _bbox;
        [NonSerialized]
        private readonly OgrDataSource _ogrDataSource;
        //[NonSerialized]
        //private OgrLayer _ogrLayer;
        private String _filename;
        private String _definitionQuery = "";
        private int _layerIndex;

        #endregion

        #region Properties

        /// <summary>
        ///  Gets the version of fwTools that was used to compile and test this Ogr Provider
        /// </summary>
        public static string FWToolsVersion
        {
#pragma warning disable 612,618
            get { return FwToolsHelper.FwToolsVersion; }
#pragma warning restore 612,618
        }

        /// <summary>
        /// return the file name of the datasource
        /// </summary>
        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        public String DefinitionQuery
        {
            get { return _definitionQuery; }
            set { _definitionQuery = value; }
        }

        public Int32 NumberOfLayers
        {
            get
            {
                int numberOfLayers = _ogrDataSource.GetLayerCount();
                return numberOfLayers;
            }
        }

        private OgrLayer GetLayer(int layerIndex)
        {
            return _ogrDataSource.GetLayerByIndex(layerIndex);
        }

        /// <summary>
        /// Method to get a layer based on a private copy of the data source
        /// </summary>
        /// <param name="layerIndex">The layer index</param>
        /// <param name="ogrDataSource">The data source</param>
        /// <returns></returns>
        private OgrLayer GetLayer(int layerIndex, out OgrDataSource ogrDataSource)
        {
            ogrDataSource = OgrOgr.OpenShared(Filename, 0);
            return ogrDataSource.GetLayerByIndex(layerIndex);
        }

        /// <summary>
        /// Gets a value indicating that this layer holds features, that is entities have a geometry
        /// </summary>
        public Boolean IsFeatureDataLayer
        {
            get
            {
                using (var ogrLayer = GetLayer(LayerIndex))
                {
                    //ogrLayer.ResetReading();
                    var numFeatures = ogrLayer.GetFeatureCount(1);
                    if (numFeatures <= 0)
                        return false;

                    using (var feature = ogrLayer.GetNextFeature())
                    {
                        if (feature == null)
                            return false;

                        using (var geom = feature.GetGeometryRef())
                        {
                            if (geom == null)
                                return false;

                            return geom.GetGeometryType() != OgrGeometryType.wkbNone;
                        }
                    }
                }
            }
        }

        public String OgrGeometryTypeString
        {
            get
            {
                using (var ogrLayer = GetLayer(LayerIndex))
                {
                    var numFeatures = ogrLayer.GetFeatureCount(1);
                    if (numFeatures <= 0)
                        return string.Format("{0}", OgrGeometryType.wkbNone);
                    using (var feature = ogrLayer.GetNextFeature())
                    {
                        if (feature == null)
                            return string.Format("{0}", OgrGeometryType.wkbNone);

                        using (var geom = feature.GetGeometryRef())
                        {
                            if (geom == null)
                                return string.Format("{0}", OgrGeometryType.wkbNone);

                            return string.Format("{0}", geom.GetGeometryType());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the name of the layer set or set the layer by its name
        /// </summary>
        ///<remarks>
        /// If the name set is not within the layer collection of the
        /// datasource the old layer is kept.
        ///</remarks>
        public string LayerName
        {
            get
            {
                using(var ogrLayer = GetLayer(LayerIndex))
                using(var ogrLayerDefn = ogrLayer.GetLayerDefn())
                    return ogrLayerDefn.GetName();
            }
            set
            {
                try
                {
                    using (var ogrLayer = _ogrDataSource.GetLayerByName(value))
                    {
                        //*** FIX: Must check for null since GetLayerByName returns null if layer name does not exist.
                        if (ogrLayer != null)
                        {
                            //_ogrLayer = layer;
                            ConnectionID = string.Format("Data Source={0};Layer{1}", _ogrDataSource.name, value);
                        }
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                { }
            }
        }

        /// <summary>
        /// Function to test if a the datasource contains the specified <paramref name="layerName"/>
        /// </summary>
        /// <param name="layerName">The name of the layer</param>
        /// <returns><c>true</c> if the layer is present, otherwise <c>false</c></returns>
        public Boolean ContainsLayer(string layerName)
        {
            using (OgrLayer layer = _ogrDataSource.GetLayerByName(layerName))
            {
                return (layer != null);
            }
        }

        /// <summary>
        /// Get the index of the layer set or set the layer by its index
        /// </summary>
        public Int32 LayerIndex
        {
            get
            {
                return _layerIndex;
            }
            set
            {
                if (value < 0 || _ogrDataSource.GetLayerCount() - 1 < value)
                    throw new ArgumentOutOfRangeException("value");
                _layerIndex = value;
            }
        }

        #endregion

        #region Constructors

        ///// <summary>
        ///// Loads a Ogr datasource with the specified layer
        ///// </summary>
        ///// <param name="filename">datasource</param>
        ///// <param name="layerName">name of layer</param>
        /////If you want this functionality use
        /////<example>
        /////SharpMap.Data.Providers.Ogr prov = new SharpMap.Data.Providers.Ogr(datasource);
        /////prov.LayerName = layerName;
        /////</example>
        //[Obsolete("This constructor does not work well with VB.NET. Use LayerName property instead")]
        //public Ogr(string filename, string layerName)
        //{
        //    Filename = filename;

        //    _ogrDataSource = OgrOgr.Open(filename, 1);
        //    _ogrLayer = _ogrDataSource.GetLayerByName(layerName);
        //    OsrSpatialReference spatialReference = _ogrLayer.GetSpatialRef();
        //    if (spatialReference != null)
        //        SRID = spatialReference.AutoIdentifyEPSG();
        //}

        /// <summary>
        /// Loads a Ogr datasource with the specified layer
        /// </summary>
        /// <param name="filename">datasource</param>
        /// <param name="layerNum">number of layer</param>
        public Ogr(string filename, int layerNum)
        {
            Filename = filename;
            ConnectionID = filename;

            _ogrDataSource = OgrOgr.OpenShared(filename, 0);
            using (var ogrLayer = GetLayer(layerNum))
            {
                OsrSpatialReference spatialReference = ogrLayer.GetSpatialRef();
                if (spatialReference != null)
                    SRID = spatialReference.AutoIdentifyEPSG();
            }
            _layerIndex = layerNum;
        }

        ///// <summary>
        ///// Loads a Ogr datasource with the specified layer
        ///// </summary>
        ///// <param name="datasource">datasource</param>
        ///// <param name="layerNum">number of layer</param>
        ///// <param name="name">Returns the name of the loaded layer</param>
        /////If you want this functionality use
        /////<example>
        /////SharpMap.Data.Providers.Ogr prov = new SharpMap.Data.Providers.Ogr(datasource, layerNum);
        /////string layerName = prov.Layername;
        /////</example>
        //[Obsolete("This constructor does not work well with VB.NET. Use LayerName property instead")]
        //public Ogr(string datasource, int layerNum, out string name)
        //    : this(datasource, layerNum)
        //{
        //    name = _ogrLayer.GetName();
        //}

        /// <summary>
        /// Loads a Ogr datasource with the first layer
        /// </summary>
        /// <param name="datasource">datasource</param>
        public Ogr(string datasource)
            : this(datasource, 0)
        {
        }

        ///// <summary>
        ///// Loads a Ogr datasource with the first layer
        ///// </summary>
        ///// <param name="datasource">datasource</param>
        ///// <param name="name">Returns the name of the loaded layer</param>
        /////<remarks>
        /////This constructor is obsolete!
        /////If you want this functionality use
        /////<example>
        /////SharpMap.Data.Providers.Ogr prov = new SharpMap.Data.Providers.Ogr(datasource);
        /////string layerName = prov.Layername;
        /////</example>
        /////</remarks>
        //[Obsolete("This constructor does not work well with VB.NET. Use LayerName property instead")]
        //public Ogr(string datasource, out string name)
        //    : this(datasource, 0, out name)
        //{
        //}

        #endregion

        #region IProvider Members

        /// <summary>
        /// Boundingbox of the dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public override BoundingBox GetExtents()
        {
            if (_bbox == null)
            {
                OgrEnvelope ogrEnvelope = new OgrEnvelope();
                using (var ogrLayer = GetLayer(LayerIndex))
                {
                    if (ogrLayer != null) ogrLayer.GetExtent(ogrEnvelope, 1);
                }

                _bbox = new BoundingBox(ogrEnvelope.MinX, ogrEnvelope.MaxX,
                                        ogrEnvelope.MinY, ogrEnvelope.MaxY);
            }

            return _bbox;
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public override int GetFeatureCount()
        {
            using(var ogrLayer = GetLayer(_layerIndex))
                return (int)ogrLayer.GetFeatureCount(1);
        }

        /// <summary>
        /// Returns a FeatureDataRow based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>FeatureDataRow</returns>
        public override FeatureDataRow GetFeature(uint rowId)
        {
            FeatureDataRow res = null;
            var reader = new OgrGeometryReader(Factory);
            using (var ogrLayer = GetLayer(_layerIndex))
            {
                var fdt = ReadColumnDefinition(ogrLayer);
                fdt.BeginLoadData();
                using (var feature = ogrLayer.GetFeature((int) rowId))
                {
                    //res = LoadOgrFeatureToFeatureDataRow(fdt, feature, Factory);
                    res = LoadOgrFeatureToFeatureDataRow(fdt, feature, reader);
                }
                fdt.EndLoadData();
            }
            return res;
        }

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            var objectIDs = new Collection<uint>();
            OgrDataSource ogrDataSource;
            using (var ogrLayer = GetLayer(_layerIndex, out ogrDataSource))
            {
                ogrLayer.SetSpatialFilterRect(bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY);

                OgrFeature ogrFeature;
                while ((ogrFeature = ogrLayer.GetNextFeature()) != null)
                {
                    objectIDs.Add((uint) ogrFeature.GetFID());
                    ogrFeature.Dispose();
                }
                ogrDataSource.Dispose();
            }
            return objectIDs;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public override Geometry GetGeometryByID(uint oid)
        {
            using (var ogrLayer = GetLayer(_layerIndex))
            using (var ogrFeature = ogrLayer.GetFeature((int)oid))
            {
                using (var gr = ogrFeature.GetGeometryRef())
                {
                    var reader = new WKBReader(Factory);
                    //var g = ParseOgrGeometry(gr, Factory);
                    var g = new OgrGeometryReader(Factory).Read(gr) ;
                    return g;
                }
            }
        }

        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            var geoms = new Collection<Geometry>();

            OgrDataSource ogrDataSource;
            using (var ogrLayer = GetLayer(_layerIndex, out ogrDataSource))
            {
                ogrLayer.SetSpatialFilterRect(bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY);
                try
                {
                    var reader = new OgrGeometryReader(Factory);
                    OgrFeature ogrFeature;
                    while ((ogrFeature = ogrLayer.GetNextFeature()) != null)
                    {
                        using (var gr = ogrFeature.GetGeometryRef())
                        {
                            //var geom = ParseOgrGeometry(gr, Factory);
                            var geom = reader.Read(gr);
                            if (geom != null)
                                geoms.Add(geom);
                        }
                        ogrFeature.Dispose();
                    }
                    ogrDataSource.Dispose();
                }
                catch (Exception ex)
                {
                    _log.Debug(t => t(ex.Message));
                }
            }
            return geoms;
        }


        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="bbox">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public override void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet ds)
        {
            OgrDataSource ogrDataSource;
            using (var ogrLayer = GetLayer(_layerIndex, out ogrDataSource))
            {
                ogrLayer.SetSpatialFilterRect(bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY);
                ExecuteIntersectionQuery(ds, ogrLayer);
                ogrDataSource.Dispose();
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        protected override void OnExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            using (var ogrGeometry = OgrGeometry.CreateFromWkb(GeometryToWKB.Write(geom)))
            {
                OgrDataSource ogrDataSource;
                using (var ogrLayer = GetLayer(_layerIndex, out ogrDataSource))
                {
                    ogrLayer.SetSpatialFilter(ogrGeometry);
                    ExecuteIntersectionQuery(ds, ogrLayer);
                    ogrDataSource.Dispose();
                }
            }

        }

        private void ExecuteIntersectionQuery(FeatureDataSet ds, OgrLayer ogrLayer)
        {
            ogrLayer.SetAttributeFilter(String.IsNullOrEmpty(_definitionQuery) ? "" : _definitionQuery);
            ogrLayer.ResetReading();

            //reads the column definition of the layer/feature
            var myDt = ReadColumnDefinition(ogrLayer);
            var reader = new OgrGeometryReader(Factory);
            myDt.BeginLoadData();
            OgrFeature ogrFeature;
            while ((ogrFeature = ogrLayer.GetNextFeature()) != null)
            {
                //LoadOgrFeatureToFeatureDataRow(myDt, ogrFeature, Factory);
                LoadOgrFeatureToFeatureDataRow(myDt, ogrFeature, reader);
                ogrFeature.Dispose();
            }
            myDt.EndLoadData();

            ds.Tables.Add(myDt);

        }

        #endregion

        #region Disposers and finalizers

        protected override void ReleaseManagedResources()
        {
            if (_ogrDataSource != null)
                _ogrDataSource.Dispose();
            
            base.ReleaseManagedResources();
        }

        #endregion

        #region private methods for data conversion sharpmap <--> ogr

        /// <summary>
        /// Reads the field types from the OgrFeatureDefinition -> OgrFieldDefinition
        /// </summary>
        /// <param name="oLayer">OgrLayer</param>
        /// <returns>The feature data table</returns>
        private static FeatureDataTable ReadColumnDefinition(OgrLayer oLayer)
        {
            var fdt = new FeatureDataTable
            {
                TableName = oLayer.GetName()
            };

            using (var ogrFeatureDefn = oLayer.GetLayerDefn())
            {
                int iField;

                for (iField = 0; iField < ogrFeatureDefn.GetFieldCount(); iField++)
                {
                    using (var ogrFldDef = ogrFeatureDefn.GetFieldDefn(iField))
                    {
                        var type= ogrFldDef.GetFieldType();
                        switch (type)
                        {
                            case OgrFieldType.OFTInteger:
                                fdt.Columns.Add(ogrFldDef.GetName(), typeof(int));
                                break;
                            case OgrFieldType.OFTIntegerList:
                                fdt.Columns.Add(ogrFldDef.GetName(), typeof(int[]));
                                break;
                            case OgrFieldType.OFTInteger64:
                                fdt.Columns.Add(ogrFldDef.GetName(), typeof(long));
                                break;
                            case OgrFieldType.OFTInteger64List:
                                fdt.Columns.Add(ogrFldDef.GetName(), typeof(long[]));
                                break;
                            case OgrFieldType.OFTReal:
                                fdt.Columns.Add(ogrFldDef.GetName(), typeof(double));
                                break;
                            case OgrFieldType.OFTRealList:
                                fdt.Columns.Add(ogrFldDef.GetName(), typeof(double[]));
                                break;
                            case OgrFieldType.OFTWideString:
                            case OgrFieldType.OFTString:
                                var c = fdt.Columns.Add(ogrFldDef.GetName(), typeof(string));
                                c.MaxLength = ogrFldDef.GetWidth();
                                break;
                            case OgrFieldType.OFTStringList:
                            case OgrFieldType.OFTWideStringList:
                                fdt.Columns.Add(ogrFldDef.GetName(), typeof(string[]));
                                break;
                            case OgrFieldType.OFTDate:
                            case OgrFieldType.OFTTime:
                            case OgrFieldType.OFTDateTime:
                                fdt.Columns.Add(ogrFldDef.GetName(), typeof(DateTime));
                                break;
                            default:
                                {
                                    var tmpFldDef = ogrFldDef;
                                    _log.Debug(t => t("Not supported type: {0} [{1}]", type, tmpFldDef.GetName()));
                                    break;
                                }
                        }
                    }
                }
            }
            return fdt;
        }

        //private static Geometry ParseOgrGeometry(OgrGeometry ogrGeometry, WKBReader reader)
        ////private static Geometry ParseOgrGeometry(OgrGeometry ogrGeometry, IGeometryFactory factory)
        //{
        //    if (ogrGeometry != null)
        //    {
                
        //        //Just in case it isn't 2D
        //        ogrGeometry.FlattenTo2D();
        //        var wkbBuffer = new byte[ogrGeometry.WkbSize()];
        //        ogrGeometry.ExportToWkb(wkbBuffer);
        //        //var geom = GeometryFromWKB.Parse(wkbBuffer, factory);
        //        var geom = reader.Read(wkbBuffer);
        //        if (geom == null)
        //            _log.Debug(t=> t("Failed to parse '{0}'", ogrGeometry.GetGeometryType()));
        //        return geom;

        //        ogrGeometry.GetGeometry()
        //    }
        //    return null;
        //}

        //private static FeatureDataRow LoadOgrFeatureToFeatureDataRow(FeatureDataTable table, OSGeo.OGR.Feature ogrFeature, GeoAPI.Geometries.IGeometryFactory factory)
        private static FeatureDataRow LoadOgrFeatureToFeatureDataRow(FeatureDataTable table, OSGeo.OGR.Feature ogrFeature, OgrGeometryReader reader)
        {
            var values = new object[ogrFeature.GetFieldCount()];
            
            for (var iField = 0; iField < ogrFeature.GetFieldCount(); iField++)
            {
                // No need to get field value if there's no value available...
                if (!ogrFeature.IsFieldSet(iField))
                {
                    continue;
                }

                int count;
                switch (ogrFeature.GetFieldType(iField))
                {
                    case OgrFieldType.OFTString:
                    case OgrFieldType.OFTWideString:
                        values[iField] = ogrFeature.GetFieldAsString(iField);
                        break;
                    case OgrFieldType.OFTStringList:
                    case OgrFieldType.OFTWideStringList:
                        values[iField] = ogrFeature.GetFieldAsStringList(iField);
                        break;
                    case OgrFieldType.OFTInteger:
                        values[iField] = ogrFeature.GetFieldAsInteger(iField);
                        break;
                    case OgrFieldType.OFTInteger64:
                        values[iField] = ogrFeature.GetFieldAsInteger64(iField);
                        break;
                    case OgrFieldType.OFTIntegerList:
                        values[iField] = ogrFeature.GetFieldAsIntegerList(iField, out count);
                        break;
                    case OgrFieldType.OFTInteger64List:
                        values[iField] = null; // ogrFeature.GetFieldAsInteger64List(iField, out count);
                        break;
                    case OgrFieldType.OFTReal:
                        values[iField] = ogrFeature.GetFieldAsDouble(iField);
                        break;
                    case OgrFieldType.OFTRealList:
                        values[iField] = ogrFeature.GetFieldAsDoubleList(iField, out count);
                        break;
                    case OgrFieldType.OFTDate:
                    case OgrFieldType.OFTDateTime:
                    case OgrFieldType.OFTTime:
                        int y, m, d, h, mi, tz;
                        float s;
                        ogrFeature.GetFieldAsDateTime(iField, out y, out m, out d, out h, out mi, out s, out tz);
                        try
                        {
                            if (y == 0 && m == 0 && d == 0)
                                values[iField] = DateTime.MinValue.AddMinutes(h * 60 + mi);
                            else
                                values[iField] = new DateTime(y, m, d, h, mi, (int)s);
                        }
// ReSharper disable once EmptyGeneralCatchClause
                        catch { }
                        break;
                    
                    default:
                        var iTmpField = iField;
                        _log.Debug(t => t("Cannot handle Ogr DataType '{0}'", ogrFeature.GetFieldType(iTmpField)));
                        break;
                }
            }

            var fdr = (FeatureDataRow)table.LoadDataRow(values, true);

            using (var gr = ogrFeature.GetGeometryRef())
            {
                //fdr.Geometry = ParseOgrGeometry(gr, factory);
                fdr.Geometry = reader.Read(gr);
            }
            return fdr;
        }

        #endregion

        public FeatureDataSet ExecuteQuery(string query)
        {
            return ExecuteQuery(query, null);
        }

        public FeatureDataSet ExecuteQuery(string query, OgrGeometry filter)
        {
            try
            {
                var results = _ogrDataSource.ExecuteSQL(query, filter, "");
                results.ResetReading();

                //reads the column definition of the layer/feature
                var ds = new FeatureDataSet();
                var myDt = ReadColumnDefinition(results);

                myDt.BeginLoadData();
                var reader = new OgrGeometryReader(Factory);
                OgrFeature ogrFeature;
                while ((ogrFeature = results.GetNextFeature()) != null)
                {
                    //LoadOgrFeatureToFeatureDataRow(myDt, ogrFeature, Factory);
                    LoadOgrFeatureToFeatureDataRow(myDt, ogrFeature, reader);
                    ogrFeature.Dispose();
                }
                myDt.EndLoadData();

                ds.Tables.Add(myDt);
                _ogrDataSource.ReleaseResultSet(results);

                return ds;
            }
            catch (Exception exc)
            {
                _log.Debug(t => t(exc.ToString()));
                return new FeatureDataSet();
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that is within 'distance' of 'geom'
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [Obsolete("Use ExecuteIntersectionQuery instead")]
        public FeatureDataTable QueryFeatures(Geometry geom, double distance)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the features that intersects with 'geom'
        /// </summary>
        /// <param name="geom">Geometry</param>
        /// <returns>FeatureDataTable</returns>
        public FeatureDataTable ExecuteIntersectionQuery(Geometry geom)
        {
            var fds = new FeatureDataSet();
            ExecuteIntersectionQuery(geom, fds);
            return fds.Tables[0];
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        [Obsolete("Use ExecuteIntersectionQuery(BoundingBox,FeatureDataSet) instead")]
        public void GetFeaturesInView(BoundingBox bbox, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }

        #region CreateFromFeatureDataTable
        /// <summary>
        /// Creates an OGR data source from a FeatureDataTable
        /// </summary>
        /// <param name="table">The name of the table</param>
        /// <param name="geometryType">The geometry type</param>
        /// <param name="driver">The driver</param>
        /// <param name="connection">The connection string</param>
        /// <param name="driverOptions">The options for the driver</param>
        /// <param name="layerOptions">The options for the layer</param>
        public static void CreateFromFeatureDataTable(FeatureDataTable table, 
            OgcGeometryType geometryType, int srid, string driver, string connection, string[] driverOptions = null, string[] layerOptions = null)
        {
            if (table == null)
                throw new ArgumentNullException("table");

            if (table.Rows.Count == 0)
                throw new ArgumentException("The table contains no rows", "table");

            if (geometryType < OgcGeometryType.Point || geometryType > OgcGeometryType.MultiPolygon)
                throw new ArgumentException("Invalid geometry type", "geometryType");

            if (string.IsNullOrWhiteSpace(driver))
                throw new ArgumentException("No driver specified", "driver");

            var dr = OSGeo.OGR.Ogr.GetDriverByName(driver);
            if (dr == null)
                throw new Exception(string.Format("Cannot load driver '{0}'!", driver));

            //if (!dr.TestCapability("ODrCCreateDataSource"))
            //    throw new Exception(string.Format("Driver '{0}' cannot create a data source!", driver));

            // Create the data source
            var ds = dr.CreateDataSource(connection, driverOptions);
            //if (!ds.TestCapability("ODsCCreateLayer"))
            //    throw new Exception(string.Format("Driver '{0}' cannot create a layer!", driver));

            // Create the spatial reference
            var sr = new OSGeo.OSR.SpatialReference(string.Empty);
            sr.ImportFromEPSG(srid);

            // Create the layer
            var lyr = ds.CreateLayer(table.TableName, sr, (OgrGeometryType)geometryType, layerOptions);
            sr.Dispose();

            //lyr.GetSpatialRef();
            foreach (System.Data.DataColumn dc in table.Columns)
            {
                using (var fldDef = GetFieldDefinition(dc))
                    lyr.CreateField(fldDef, 0);
            }

            using (var ld = lyr.GetLayerDefn())
            {
                foreach (FeatureDataRow fdr in table.Rows)
                {
                    if ((int)fdr.Geometry.OgcGeometryType != (int)geometryType)
                        continue;

                    using (var feature = new OgrFeature(ld))
                    {
                        feature.SetGeometry(OgrGeometry.CreateFromWkb(fdr.Geometry.AsBinary()));
                        var idx = -1;
                        foreach (System.Data.DataColumn dc in table.Columns)
                        {
                            idx++;
                            var fd = ld.GetFieldDefn(idx);
                            DateTime dt;
                            switch (fd.GetFieldType())
                            {
                                case OgrFieldType.OFTBinary:
                                    //Nothing
                                    break;
                                case OgrFieldType.OFTDate:
                                    dt = ((DateTime)fdr[dc]).Date;
                                    feature.SetField(idx, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
                                    break;
                                case OgrFieldType.OFTDateTime:
                                    dt = (DateTime)fdr[dc];
                                    feature.SetField(idx, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
                                    break;
                                case OgrFieldType.OFTTime:
                                    var tod = ((DateTime)fdr[dc]).TimeOfDay;
                                    feature.SetField(idx, 0, 0, 0, tod.Hours, tod.Minutes, tod.Seconds, 0);
                                    break;
                                case OgrFieldType.OFTInteger:
                                    feature.SetField(idx, Convert.ToInt32(fdr[dc]));
                                    break;
                                case OgrFieldType.OFTIntegerList:
                                    var il = GetIntegerList(fdr[dc], dc.DataType);
                                    feature.SetFieldIntegerList(idx, il.Length, il);
                                    break;
                                case OgrFieldType.OFTReal:
                                    feature.SetField(idx, Convert.ToDouble(fdr[dc]));
                                    break;
                                case OgrFieldType.OFTRealList:
                                    var dl = GetDoubleList(fdr[dc], dc.DataType);
                                    feature.SetFieldDoubleList(idx, dl.Length, dl);
                                    break;
                                case OgrFieldType.OFTString:
                                    feature.SetField(idx, Convert.ToString(fdr[dc]));
                                    break;
                                case OgrFieldType.OFTStringList:
                                    var sl = (string[])fdr[dc];
                                    feature.SetFieldStringList(idx, sl);
                                    break;

                            }
                            fd.Dispose();
                        }
                        lyr.CreateFeature(feature);
                        feature.Dispose();
                    }
                    //ld.Dispose();
                }
            }

            lyr.Dispose();
            ds.Dispose();
            dr.Dispose();
        }

        private static double[] GetDoubleList(object o, System.Type type)
        {
            double[] res;
            if (type == typeof(float[]))
            {
                var fa = (float[])o;
                res = new double[fa.Length];
                for (var i = 0; i < fa.Length; i++) res[i] = fa[i];
                return res;
            }
            if (type == typeof(double[]))
            {
                res = (double[])o;
                return res;
            }
            if (type == typeof(long[]))
            {
                var la = (long[])o;
                res = new double[la.Length];
                for (var i = 0; i < la.Length; i++) res[i] = la[i];
                return res;
            }
            throw new InvalidOperationException("Cannot transform {0} to a list of doubles");
        }

        private static int[] GetIntegerList(object o, System.Type type)
        {
            int[] res;
            if (type == typeof(Int16[]))
            {
                var sa = (short[])o;
                res = new int[sa.Length];
                for (var i = 0; i < sa.Length; i++) res[i] = sa[i];
                return res;
            }
            if (type == typeof(int[]))
            {
                res = (int[])o;
                return res;
            }
            throw new InvalidOperationException("Cannot transform {0} to a list of integers");
        }

        private static OgrFieldDefn GetFieldDefinition(System.Data.DataColumn dc)
        {
            var tmp = new OgrFieldDefn(dc.ColumnName, GetFieldType(dc.DataType));
            if (dc.MaxLength > 0)
                tmp.SetWidth(dc.MaxLength);
            return tmp;
        }

        private static OgrFieldType GetFieldType(Type dataType)
        {
            switch (dataType.FullName)
            {
                case "System.String":
                    return OgrFieldType.OFTString;

                case "System.DateTime":
                    return OgrFieldType.OFTDateTime;
                    return OgrFieldType.OFTDate;
                    return OgrFieldType.OFTTime;

                case "System.Byte[]":
                    return OgrFieldType.OFTBinary;

                case "System.Byte":
                case "System.Int16":
                case "System.Int32":
                    return OgrFieldType.OFTInteger;

                case "System.Int16[]":
                case "System.Int32[]":
                    return OgrFieldType.OFTIntegerList;

                case "System.Single":
                case "System.Double":
                case "System.Int64":
                    return OgrFieldType.OFTReal;

                //This should not happen
                case "System.Single[]":
                case "System.Double[]":
                case "System.Int64[]":
                    return OgrFieldType.OFTRealList;

                //don't know when this is supposed to happen
                case "xxx":
                    return OgrFieldType.OFTWideString;
                    return OgrFieldType.OFTWideStringList;
            }
            throw new NotSupportedException();
        }
        #endregion

        #region Nested OgrGeometryReader class

        private class OgrGeometryReader
        {
            private readonly IGeometryFactory _factory;
            private WKBReader _reader;

            /// <summary>
            /// Creates a n instance of this class
            /// </summary>
            /// <param name="factory">The factory to use</param>
            public OgrGeometryReader(IGeometryFactory factory)
            {
                _factory = factory;
            }

            /// <summary>
            /// A WKB reader
            /// </summary>
            private WKBReader Reader { get { return _reader ?? (_reader = new WKBReader(_factory)); } }

            /// <summary>
            /// Method to read the geometry
            /// </summary>
            /// <param name="geom"></param>
            /// <returns></returns>
            public IGeometry Read(OgrGeometry geom)
            {
                if (geom == null)
                    return null;

                var type = geom.GetGeometryType();
                switch (type)
                {
                    case OgrGeometryType.wkbPoint:
                    case OgrGeometryType.wkbPoint25D:
                        return ReadPoint(type, geom);

                    case OgrGeometryType.wkbLineString:
                    case OgrGeometryType.wkbLineString25D:
                        return ReadLineString(type, geom);

                    default:
                        return ReadWkb(type, geom);
                }
            }

            private IGeometry ReadWkb(OgrGeometryType type, OgrGeometry geom)
            {
                var b = new byte[geom.WkbSize()];
                geom.ExportToWkb(b);
                return Reader.Read(b);
            }

            private IGeometry ReadLineString(OgrGeometryType type, OgrGeometry geom)
            {
                var count = geom.GetPointCount();
                var dimension = geom.GetCoordinateDimension();

                var cs = _factory.CoordinateSequenceFactory.Create(count, dimension);
                if (dimension > 2)
                    for (var i = 0; i < cs.Count; i++)
                    {
                        cs.SetOrdinate(i, Ordinate.X, geom.GetX(i));
                        cs.SetOrdinate(i, Ordinate.Y, geom.GetY(i));
                        cs.SetOrdinate(i, Ordinate.Z, geom.GetZ(i));
                    }
                else
                    for (var i = 0; i < cs.Count; i++)
                    {
                        cs.SetOrdinate(i, Ordinate.X, geom.GetX(i));
                        cs.SetOrdinate(i, Ordinate.Y, geom.GetY(i));
                    }

                return _factory.CreateLineString(cs);
            }

            private IGeometry ReadPoint(OgrGeometryType type, OgrGeometry geom)
            {
                var c = new Coordinate(geom.GetX(0), geom.GetY(0));
                if ((int)type > 0x1000000) c.Z = geom.GetZ(0);

                return _factory.CreatePoint(c);
            }
        }

        #endregion
    }
}
