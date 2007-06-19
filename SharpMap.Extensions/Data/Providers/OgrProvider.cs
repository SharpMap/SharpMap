/*
 * Created by SharpDevelop.
 * User: Christian
 * Date: 29.04.2006
 * Time: 10:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.ObjectModel;
using OSGeo.OGR;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Ogr provider for SharpMap
    /// using the C# SWIG wrapper of GDAL/OGR
    /// <code>
    /// SharpMap.Layers.VectorLayer vLayerOgr = new SharpMap.Layers.VectorLayer("MapInfoLayer");
    /// vLayerOgr.DataSource = new SharpMap.Data.Providers.Ogr(@"D:\GeoData\myWorld.tab");
    /// </code>
    /// </summary>
    public class Ogr : SharpMap.Data.Providers.IProvider, IDisposable
    {

        private SharpMap.Geometries.BoundingBox _bbox;
        private String m_Filename;

        private OSGeo.OGR.DataSource _OgrDataSource;
        private OSGeo.OGR.Layer _OgrLayer;

        /// <summary>
        /// return the file name of the datasource
        /// </summary>
        public string Filename
        {
            get { return m_Filename; }
            set { m_Filename = value; }
        }

        #region constructor

        /// <summary>
        /// Loads a Ogr datasource with the specified layer
        /// </summary>
        /// <param name="Filename">datasource</param>
        /// <param name="LayerName">name of layer</param>
        public Ogr(string Filename, string LayerName)
        {
            this.Filename = Filename;

            OSGeo.OGR.Ogr.RegisterAll();
            _OgrDataSource = OSGeo.OGR.Ogr.Open(this.Filename, 1);
            _OgrLayer = _OgrDataSource.GetLayerByName(LayerName);
        }


        /// <summary>
        /// Loads a Ogr datasource with the specified layer
        /// </summary>
        /// <param name="Filename">datasource</param>
        /// <param name="LayerNum">number of layer</param>
        public Ogr(string Filename, int LayerNum)
        {
            this.Filename = Filename;
            OSGeo.OGR.Ogr.RegisterAll();

            _OgrDataSource = OSGeo.OGR.Ogr.Open(this.Filename, 0);
            _OgrLayer = _OgrDataSource.GetLayerByIndex(LayerNum);
        }

        /// <summary>
        /// Loads a Ogr datasource with the specified layer
        /// </summary>
        /// <param name="Filename">datasource</param>
        /// <param name="LayerNum">number of layer</param>
        /// <param name="name">Returns the name of the loaded layer</param>
        public Ogr(string Filename, int LayerNum, out string name)
            : this(Filename, LayerNum)
        {
            name = _OgrLayer.GetName();
        }

        /// <summary>
        /// Loads a Ogr datasource with the first layer
        /// </summary>
        /// <param name="Filename">datasource</param>
        public Ogr(string Filename)
            : this(Filename, 0)
        {

        }

        /// <summary>
        /// Loads a Ogr datasource with the first layer
        /// </summary>
        /// <param name="Filename">datasource</param>
        /// <param name="name">Returns the name of the loaded layer</param>
        public Ogr(string Filename, out string name)
            : this(Filename, 0, out name)
        {

        }
        #endregion

        #region IProvider Members


        /// <summary>
        /// Boundingbox of the dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public SharpMap.Geometries.BoundingBox GetExtents()
        {
            if (this._bbox == null)
            {
                OSGeo.OGR.Envelope _OgrEnvelope = new Envelope();
                int i = _OgrLayer.GetExtent(_OgrEnvelope, 1);

                this._bbox = new SharpMap.Geometries.BoundingBox(_OgrEnvelope.MinX,
                                                                 _OgrEnvelope.MinY,
                                                                 _OgrEnvelope.MaxX,
                                                                 _OgrEnvelope.MaxY);

            }

            return _bbox;
        }

        public FeatureDataSet ExecuteQuery(string query)
        {
            return ExecuteQuery(query, null);
        }

        public FeatureDataSet ExecuteQuery(string query, Geometry filter)
        {
            try
            {
                FeatureDataSet ds = new FeatureDataSet();
                FeatureDataTable myDt = new FeatureDataTable();

                Layer results = _OgrDataSource.ExecuteSQL(query, filter, "");

                //reads the column definition of the layer/feature
                ReadColumnDefinition(myDt, results);

                OSGeo.OGR.Feature _OgrFeature;
                results.ResetReading();
                while ((_OgrFeature = results.GetNextFeature()) != null)
                {
                    FeatureDataRow _dr = myDt.NewRow();
                    for (int iField = 0; iField < _OgrFeature.GetFieldCount(); iField++)
                    {
                        if (myDt.Columns[iField].DataType == System.Type.GetType("System.String"))
                            _dr[iField] = _OgrFeature.GetFieldAsString(iField);
                        else if (myDt.Columns[iField].GetType() == System.Type.GetType("System.Int32"))
                            _dr[iField] = _OgrFeature.GetFieldAsInteger(iField);
                        else if (myDt.Columns[iField].GetType() == System.Type.GetType("System.Double"))
                            _dr[iField] = _OgrFeature.GetFieldAsDouble(iField);
                        else
                            _dr[iField] = _OgrFeature.GetFieldAsString(iField);
                    }

                    _dr.Geometry = this.ParseOgrGeometry(_OgrFeature.GetGeometryRef());
                    myDt.AddRow(_dr);
                }
                ds.Tables.Add(myDt);
                _OgrDataSource.ReleaseResultSet(results);

                return ds;
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                return new FeatureDataSet();
            }
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            return _OgrLayer.GetFeatureCount(1);
        }

        /// <summary>
        /// Returns the data associated with all the geometries that is within 'distance' of 'geom'
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [Obsolete("Use ExecuteIntersectionQuery instead")]
        public FeatureDataTable QueryFeatures(SharpMap.Geometries.Geometry geom, double distance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the features that intersects with 'geom'
        /// </summary>
        /// <param name="geom">Geometry</param>
        /// <returns>FeatureDataTable</returns>
        public FeatureDataTable ExecuteIntersectionQuery(SharpMap.Geometries.Geometry geom)
        {
            FeatureDataSet fds = new FeatureDataSet();
            ExecuteIntersectionQuery(geom, fds);
            return fds.Tables[0];
        }

        /// <summary>
        /// Returns a FeatureDataRow based on a RowID
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns>FeatureDataRow</returns>
        public FeatureDataRow GetFeature(uint RowID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        public string ConnectionID
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            _IsOpen = true;
        }
        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            _IsOpen = false;
        }

        private bool _IsOpen;

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _IsOpen; }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        [Obsolete("Use ExecuteIntersectionQuery(BoundingBox,FeatureDataSet) instead")]
        public void GetFeaturesInView(SharpMap.Geometries.BoundingBox bbox, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(SharpMap.Geometries.BoundingBox bbox)
        {
            _OgrLayer.SetSpatialFilterRect(bbox.Min.X, bbox.Min.Y, bbox.Max.X, bbox.Max.Y);
            OSGeo.OGR.Feature _OgrFeature = null;
            _OgrLayer.ResetReading();

            Collection<uint> _ObjectIDs = new Collection<uint>();

            while ((_OgrFeature = _OgrLayer.GetNextFeature()) != null)
            {
                _ObjectIDs.Add((uint)_OgrFeature.GetFID());
                _OgrFeature.Dispose();
            }
            return _ObjectIDs;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public SharpMap.Geometries.Geometry GetGeometryByID(uint oid)
        {
            using (OSGeo.OGR.Feature _OgrFeature = _OgrLayer.GetFeature((int)oid))
                return this.ParseOgrGeometry(_OgrFeature.GetGeometryRef());
        }

        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<SharpMap.Geometries.Geometry> GetGeometriesInView(SharpMap.Geometries.BoundingBox bbox)
        {
            Collection<SharpMap.Geometries.Geometry> geoms = new Collection<SharpMap.Geometries.Geometry>();

            _OgrLayer.SetSpatialFilterRect(bbox.Left, bbox.Bottom, bbox.Right, bbox.Top);
            OSGeo.OGR.Feature _OgrFeature = null;

            _OgrLayer.ResetReading();
            while ((_OgrFeature = _OgrLayer.GetNextFeature()) != null)
            {
                geoms.Add(this.ParseOgrGeometry(_OgrFeature.GetGeometryRef()));
                _OgrFeature.Dispose();
            }

            return geoms;
        }

        private int _SRID = -1;

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _SRID; }
            set { _SRID = value; }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="bbox">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(SharpMap.Geometries.BoundingBox bbox, FeatureDataSet ds)
        {
            FeatureDataTable myDt = new FeatureDataTable();

            _OgrLayer.SetSpatialFilterRect(bbox.Left, bbox.Bottom, bbox.Right, bbox.Top);

            //reads the column definition of the layer/feature
            this.ReadColumnDefinition(myDt, _OgrLayer);

            OSGeo.OGR.Feature _OgrFeature;
            _OgrLayer.ResetReading();
            while ((_OgrFeature = _OgrLayer.GetNextFeature()) != null)
            {
                FeatureDataRow _dr = myDt.NewRow();
                for (int iField = 0; iField < _OgrFeature.GetFieldCount(); iField++)
                {
                    if (myDt.Columns[iField].DataType == System.Type.GetType("System.String"))
                        _dr[iField] = _OgrFeature.GetFieldAsString(iField);
                    else if (myDt.Columns[iField].GetType() == System.Type.GetType("System.Int32"))
                        _dr[iField] = _OgrFeature.GetFieldAsInteger(iField);
                    else if (myDt.Columns[iField].GetType() == System.Type.GetType("System.Double"))
                        _dr[iField] = _OgrFeature.GetFieldAsDouble(iField);
                    else
                        _dr[iField] = _OgrFeature.GetFieldAsString(iField);
                }

                _dr.Geometry = this.ParseOgrGeometry(_OgrFeature.GetGeometryRef());
                myDt.AddRow(_dr);
            }
            ds.Tables.Add(myDt);
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(SharpMap.Geometries.Geometry geom, FeatureDataSet ds)
        {
            throw new NotImplementedException();
        }


        #endregion

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

        internal void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && _OgrDataSource != null)
                {
                    _OgrDataSource.Dispose();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~Ogr()
        {
            Close();
            Dispose();
        }
        #endregion

        #region private methods for data conversion sharpmap <--> ogr

        /// <summary>
        /// Reads the field types from the OgrFeatureDefinition -> OgrFieldDefinition
        /// </summary>
        /// <param name="fdt">FeatureDatatTable</param>
        /// <param name="oLayer">OgrLayer</param>
        private void ReadColumnDefinition(FeatureDataTable fdt, OSGeo.OGR.Layer oLayer)
        {
            using (OSGeo.OGR.FeatureDefn _OgrFeatureDefn = oLayer.GetLayerDefn())
            {
                int iField;

                for (iField = 0; iField < _OgrFeatureDefn.GetFieldCount(); iField++)
                {
                    using (OSGeo.OGR.FieldDefn _OgrFldDef = _OgrFeatureDefn.GetFieldDefn(iField))
                    {
                        OSGeo.OGR.FieldType type;
                        switch ((type = _OgrFldDef.GetFieldType()))
                        {
                            case OSGeo.OGR.FieldType.OFTInteger:
                                fdt.Columns.Add(_OgrFldDef.GetName(), System.Type.GetType("System.Int32"));
                                break;
                            case OSGeo.OGR.FieldType.OFTReal:
                                fdt.Columns.Add(_OgrFldDef.GetName(), System.Type.GetType("System.Double"));
                                break;
                            case OSGeo.OGR.FieldType.OFTString:
                                fdt.Columns.Add(_OgrFldDef.GetName(), System.Type.GetType("System.String"));
                                break;
                            case OSGeo.OGR.FieldType.OFTWideString:
                                fdt.Columns.Add(_OgrFldDef.GetName(), System.Type.GetType("System.String"));
                                break;
                            default:
                                {
                                    //fdt.Columns.Add(_OgrFldDef.GetName(), System.Type.GetType("System.String"));
                                    System.Diagnostics.Debug.WriteLine("Not supported type: " + type + " [" + _OgrFldDef.GetName() + "]");
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private SharpMap.Geometries.Geometry ParseOgrGeometry(OSGeo.OGR.Geometry OgrGeometry)
        {
            byte[] wkbBuffer = new byte[OgrGeometry.WkbSize()];
            int i = OgrGeometry.ExportToWkb(wkbBuffer);
            return SharpMap.Converters.WellKnownBinary.GeometryFromWKB.Parse(wkbBuffer);
        }
        #endregion
    }
}
