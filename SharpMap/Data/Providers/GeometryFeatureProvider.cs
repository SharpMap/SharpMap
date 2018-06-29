// Copyright 2006 - Morten Nielsen (www.iter.dk)
// Copyright 2007 - Christian Gräfe (www.sharptools.de)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using GeoAPI.Geometries;
using System.ComponentModel;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Datasource for storing a limited set of geometries.
    /// </summary>
    /// <remarks>
    /// <para>The GeometryProvider doesn’t utilize performance optimizations of spatial indexing,
    /// and thus is primarily meant for rendering a limited set of Geometries.</para>
    /// <para>A common use of the GeometryProvider is for highlighting a set of selected features.</para>
    /// <example>
    /// The following example gets data within a BoundingBox of another datasource and adds it to the map.
    /// <code lang="C#">
    /// List&#60;Geometry&#62; geometries = myMap.Layers[0].DataSource.GetGeometriesInView(myBox);
    /// VectorLayer laySelected = new VectorLayer("Selected Features");
    /// laySelected.DataSource = new GeometryFeatureProvider(geometries);
    /// laySelected.Style.Outline = new Pen(Color.Magenta, 3f);
    /// laySelected.Style.EnableOutline = true;
    /// myMap.Layers.Add(laySelected);
    /// </code>
    /// </example>
    /// <example>
    /// Adding points of interest to the map. This is useful for vehicle tracking etc.
    /// <code lang="C#">
    /// GeoAPI.Geometries.IGeometryFactory gf = new NetTopologySuite.Geometries.GeometryFactory();
    /// List&#60;GeoAPI.Geometries.IGeometry&#62; geometries = new List&#60;GeoAPI.Geometries.IGeometry&#62;();
    /// //Add two points
    /// geometries.Add(gf.CreatePoint(23.345,64.325));
    /// geometries.Add(gf.CreatePoint(23.879,64.194));
    /// SharpMap.Layers.VectorLayer layerVehicles = new SharpMap.Layers.VectorLayer("Vehicles");
    /// layerVehicles.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(geometries);
    /// layerVehicles.Style.Symbol = Bitmap.FromFile(@"C:\data\car.gif");
    /// myMap.Layers.Add(layerVehicles);
    /// </code>
    /// </example>
    /// </remarks>
    public class GeometryFeatureProvider : FilterProvider, IProvider
    {
        private readonly FeatureDataTable _features;
        private int _srid = -1;
        private int _oid = -1; // primary key index from fdt schema or subsequently added unique constraint

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="geometries">Set of geometries that this datasource should contain</param>
        public GeometryFeatureProvider(IEnumerable<IGeometry> geometries)
        {
            _features = new FeatureDataTable();
            _features.BeginLoadData();
            foreach (var geom in geometries)
            {
                var fdr = _features.NewRow();
                fdr.Geometry = geom;
                _features.AddRow(fdr);
            }
            _features.AcceptChanges();
            _features.EndLoadData();

            _features.TableCleared += HandleFeaturesCleared;
            _features.Constraints.CollectionChanged += HandleConstraintsCollectionChanged;

            if (_features.Count > 0 && _features[0].Geometry != null)
                SRID = _features[0].Geometry.SRID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="features">Features to be included in this datasource</param>
        public GeometryFeatureProvider(FeatureDataTable features)
        {
            _features = features;
            _features.TableCleared += HandleFeaturesCleared;
            _features.Constraints.CollectionChanged += HandleConstraintsCollectionChanged;

            if (_features != null && _features.Count > 0)
                if (_features[0].Geometry != null)
                    SRID = _features[0].Geometry.SRID;

            // eg ShapeFile datasource with IncludeOid = true
            if (features.PrimaryKey.Length == 1)
                _oid = ValidateOidDataType(features.PrimaryKey[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="geometry">Geometry to be in this datasource</param>
        public GeometryFeatureProvider(IGeometry geometry)
        {
            _features = new FeatureDataTable();
            var fdr = _features.NewRow();
            fdr.Geometry = geometry;
            if (geometry != null)
                SRID = geometry.SRID;
            _features.AddRow(fdr);
            _features.AcceptChanges();

            _features.TableCleared += HandleFeaturesCleared;
            _features.Constraints.CollectionChanged += HandleConstraintsCollectionChanged;
        }

        #endregion

        private void HandleFeaturesCleared(object sender, DataTableClearEventArgs e)
        {
            //maybe clear extents, reset SRID
            //ok, maybe not
        }

        private void HandleConstraintsCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            // rare cases when PK is explicitly added/removed subsequent to constructor
            if (e.Element is UniqueConstraint)
            {
                UniqueConstraint uc;
                uc = (UniqueConstraint)e.Element;
                // Ensure we are dealing with PK....
                // IsPrimaryKey only True for REMOVE or REFRESH. When ADDing new PK constraint 
                // IsPrimaryKey is False until after constraint applied, so for this case confirm 
                // no existing PK and check constraint for single col, !AllowDBNull and Unique
                if ((uc.IsPrimaryKey && _features.PrimaryKey.Length == 1) ||
                    (_features.PrimaryKey.Length == 0 && uc.Columns.Length == 1 &&
                    !uc.Columns[0].AllowDBNull && uc.Columns[0].Unique))
                {
                    if (e.Action == CollectionChangeAction.Remove)
                        _oid = -1;
                    else
                        _oid = ValidateOidDataType(uc.Columns[0]);
                }
            }
        }

        private int ValidateOidDataType(DataColumn pk)
        {
            switch (Type.GetTypeCode(pk.DataType))
            {
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return pk.Ordinal;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Access to underlying <see cref="FeatureDataTable"/>
        /// </summary>
        public FeatureDataTable Features
        {
            get
            {
                lock (_features.Rows.SyncRoot)
                {
                    return _features;
                }
            }
        }

        #region IProvider Members

        /// <summary>
        /// Returns features within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var list = new Collection<IGeometry>();

            lock (_features.Rows.SyncRoot)
            {
                foreach (FeatureDataRow fdr in _features.Rows)
                    if (fdr.Geometry != null && !fdr.Geometry.IsEmpty)
                        if (FilterDelegate == null || FilterDelegate(fdr))
                        {
                            if (bbox.Intersects(fdr.Geometry.EnvelopeInternal))
                                list.Add(fdr.Geometry);
                        }
            }
            return list;
        }

        private IEnumerable<KeyValuePair<uint, FeatureDataRow>> EnumerateFeatures(Envelope bbox)
        {
            lock (_features.Rows.SyncRoot)
            {
                uint id = 0;
                foreach (FeatureDataRow feature in _features.Rows)
                {
                    var geom = feature.Geometry;
                    if (geom != null && !geom.IsEmpty)
                    {
                        if (bbox.Intersects(geom.EnvelopeInternal) && (FilterDelegate == null || FilterDelegate(feature)))
                        {
                            if (_oid == -1)
                            {
                                yield return new KeyValuePair<uint, FeatureDataRow>(id, feature);
                                id++;
                            }
                            else
                            {
                                yield return new KeyValuePair<uint, FeatureDataRow>(Convert.ToUInt32(feature[_oid]), feature);
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns all objects whose boundingbox intersects 'bbox'.
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var list = new Collection<uint>();

            foreach (var idFeature in EnumerateFeatures(bbox))
                list.Add(idFeature.Key);

            return list;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public IGeometry GetGeometryByID(uint oid)
        {
            lock (_features.Rows.SyncRoot)
            {
                if (_oid == -1)
                {
                    if (oid >= _features.Rows.Count)
                        return null;
                    else
                        return ((FeatureDataRow)_features.Rows[(int)oid]).Geometry;
                }
                else
                {
                    var dr = _features.Rows.Find(oid);
                    if (dr != null)
                        return ((FeatureDataRow)dr).Geometry;
                    else
                        return null;
                }

            }
        }

        /// <summary>
        /// Add datatable to dataset and populate with intersecting features (perform bounding box intersect followed by geom intersect)
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            FeatureDataTable fdt;
            lock (_features.Columns.SyncRoot)
                fdt = _features.Clone();

            fdt.BeginLoadData();
            var pg = new NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory().Create(geom);
            foreach (var idFeature in EnumerateFeatures(geom.EnvelopeInternal))
            {
                var fdr = idFeature.Value;
                if (pg.Intersects(fdr.Geometry))
                {
                    fdt.LoadDataRow(fdr.ItemArray, true);
                    var tmpGeom = fdr.Geometry;
                    if (tmpGeom != null)
                        ((FeatureDataRow)fdt.Rows[fdt.Rows.Count - 1]).Geometry = (IGeometry)tmpGeom.Clone();
                }
            }
            fdt.EndLoadData();

            ds.Tables.Add(fdt);
        }

        /// <summary>
        /// Add datatable to dataset and populate with interesecting features
        /// </summary>
        /// <param name="box"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            FeatureDataTable fdt;
            lock (_features.Columns.SyncRoot)
                fdt = _features.Clone();

            fdt.BeginLoadData();
            foreach (var idFeature in EnumerateFeatures(box))
            {
                var fdr = idFeature.Value;
                fdt.LoadDataRow(fdr.ItemArray, false);
                var geom = fdr.Geometry;
                if (geom != null)
                    ((FeatureDataRow)fdt.Rows[fdt.Rows.Count - 1]).Geometry = (IGeometry)geom.Clone();
            }
            fdt.AcceptChanges();
            fdt.EndLoadData();

            ds.Tables.Add(fdt);
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            lock (_features.Rows.SyncRoot)
                return _features.Rows.Count;
        }

        /// <summary>
        /// Gets a specific feature from the data source by its <paramref name="rowId"/>
        /// </summary>
        /// <param name="rowId">The row index or OID (if primary key enabled) of the feature</param>
        /// <returns>A feature data row</returns>
        public FeatureDataRow GetFeature(uint rowId)
        {
            lock (_features.Rows.SyncRoot)
            {
                if (_oid == -1)
                {
                    // find by row number
                    if (rowId >= _features.Rows.Count)
                    {
                        return null;
                    }
                    else if (FilterDelegate != null && FilterDelegate(_features[(int)rowId]))
                    {
                        return _features[(int)rowId];
                    }
                    else if (rowId < _features.Rows.Count)
                    {
                        return _features[(int)rowId];
                    }
                }
                else
                {
                    // find by primary key
                    DataRow dr;
                    dr = _features.Rows.Find(rowId);
                    if (dr == null)
                    {
                        return null;
                    }
                    else if (FilterDelegate != null && FilterDelegate((FeatureDataRow)dr))
                    {
                        return (FeatureDataRow)dr;
                    }
                    else
                    {
                        return (FeatureDataRow)dr;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public Envelope GetExtents()
        {
            lock (_features.Rows.SyncRoot)
            {
                if (_features.Rows.Count == 0)
                    return null;

                var box = new Envelope();

                foreach (FeatureDataRow fdr in _features.Rows)
                {
                    if (fdr.Geometry != null && !fdr.Geometry.IsEmpty)
                        box.ExpandToInclude(fdr.Geometry.EnvelopeInternal);
                }
                return box;
            }
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// The ConnectionID is meant for Connection Pooling which doesn't apply to this datasource. Instead
        /// <c>String.Empty</c> is returned.
        /// </remarks>
        public virtual string ConnectionID
        {
            get { return String.Empty; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Do nothing;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Do nothing;
        }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return true; }
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set { _srid = value; }
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            _features.TableCleared -= HandleFeaturesCleared;
            _features.Constraints.CollectionChanged -= HandleConstraintsCollectionChanged;

            _features.Dispose();
        }

        #endregion
    }
}
