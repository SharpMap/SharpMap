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
    /// List&#60;GeoAPI.Geometries.IGeometry&#62; geometries = new List&#60;GeoAPI.Geometries.IGeometry&#62;();
    /// //Add two points
    /// geometries.Add(new SharpMap.Geometries.Point(23.345,64.325));
    /// geometries.Add(new SharpMap.Geometries.Point(23.879,64.194));
    /// SharpMap.Layers.VectorLayer layerVehicles = new SharpMap.Layers.VectorLayer("Vehicles");
    /// layerVehicles.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(geometries);
    /// layerVehicles.Style.Symbol = Bitmap.FromFile(@"C:\data\car.gif");
    /// myMap.Layers.Add(layerVehicles);
    /// </code>
    /// </example>
    /// </remarks>
    public class GeometryFeatureProvider : FilterProvider, IProvider
    {
        private readonly object _featuresLock = new object();
        private readonly FeatureDataTable _features;
        private int _srid = -1;

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="geometries">Set of geometries that this datasource should contain</param>
        public GeometryFeatureProvider(IEnumerable<IGeometry> geometries)
        {
            _features = new FeatureDataTable();
            foreach (var geom in geometries)
            {
                var fdr = _features.NewRow();
                fdr.Geometry = geom;
                _features.AddRow(fdr);
            }
            _features.TableCleared += HandleFeaturesCleared;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="features">Features to be included in this datasource</param>
        public GeometryFeatureProvider(FeatureDataTable features)
        {
            _features = features;
            _features.TableCleared += HandleFeaturesCleared;
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
            _features.AddRow(fdr);
            _features.TableCleared += HandleFeaturesCleared;
        }

        #endregion

        private void HandleFeaturesCleared(object sender, DataTableClearEventArgs e)
        {
            //maybe clear extents
            //ok, maybe not
        }

        /// <summary>
        /// Access to underlying <see cref="FeatureDataTable"/>
        /// </summary>
        public FeatureDataTable Features
        {
            get
            {
                lock (_features)
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

            lock (_features)
            {
                foreach (FeatureDataRow fdr in _features.Rows)
                    if (!fdr.Geometry.IsEmpty)
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
            lock (_featuresLock)
            {
                uint id = 0;
                if (FilterDelegate == null)
                    foreach (FeatureDataRow feature in _features.Rows)
                    {
                        var geom = feature.Geometry;
                        var testBox = geom != null ? geom.EnvelopeInternal : new Envelope(bbox);
                        if (bbox.Intersects(testBox))
                            yield return new KeyValuePair<uint, FeatureDataRow>(id, feature);
                        id++;
                    }
                else
                    foreach (FeatureDataRow feature in _features.Rows)
                    {
                        var geom = feature.Geometry;
                        var testBox = geom != null ? geom.EnvelopeInternal : new Envelope(bbox);
                        if (bbox.Intersects(testBox) && FilterDelegate(feature))
                            yield return new KeyValuePair<uint, FeatureDataRow>(id, feature);
                        id++;
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
            lock (_featuresLock)
            {
                return ((FeatureDataRow)_features.Rows[(int)oid]).Geometry;
            }
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            FeatureDataTable fdt;
            lock (_featuresLock)
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
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="box"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            FeatureDataTable fdt;
            lock (_featuresLock)
                fdt = _features.Clone();

            fdt.BeginLoadData();
            foreach (var idFeature in EnumerateFeatures(box))
            {
                var fdr = idFeature.Value;
                fdt.LoadDataRow(fdr.ItemArray, false);
                var geom =  fdr.Geometry;
                if (geom != null)
                    ((FeatureDataRow)fdt.Rows[fdt.Rows.Count - 1]).Geometry = (IGeometry)geom.Clone();
            }
            fdt.EndLoadData();

            ds.Tables.Add(fdt);
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            lock(_featuresLock)
                return _features.Rows.Count;
        }

        /// <summary>
        /// Gets a specific feature from the data source by its <paramref name="rowId"/>
        /// </summary>
        /// <param name="rowId">The id of the row</param>
        /// <returns>A feature data row</returns>
        public FeatureDataRow GetFeature(uint rowId)
        {
            lock (_featuresLock)
            {
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

            return null;
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public Envelope GetExtents()
        {
            lock (_featuresLock)
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
        public string ConnectionID
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
            _features.Dispose();
        }

        #endregion
    }
}