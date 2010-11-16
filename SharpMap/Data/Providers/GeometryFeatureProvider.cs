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
using SharpMap.Geometries;

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
    /// List&#60;SharpMap.Geometries.Geometry&#62; geometries = new List&#60;SharpMap.Geometries.Geometry&#62;();
    /// //Add two points
    /// geometries.Add(new SharpMap.Geometries.Point(23.345,64.325));
    /// geometries.Add(new SharpMap.Geometries.Point(23.879,64.194));
    /// SharpMap.Layers.VectorLayer layerVehicles = new SharpMap.Layers.VectorLayer("Vechicles");
    /// layerVehicles.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(geometries);
    /// layerVehicles.Style.Symbol = Bitmap.FromFile(@"C:\data\car.gif");
    /// myMap.Layers.Add(layerVehicles);
    /// </code>
    /// </example>
    /// </remarks>
    public class GeometryFeatureProvider : IProvider, IDisposable
    {
        private readonly FeatureDataTable _features;
        private int _SRID = -1;

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="geometries">Set of geometries that this datasource should contain</param>
        public GeometryFeatureProvider(IEnumerable<Geometry> geometries)
        {
            _features = new FeatureDataTable();
            foreach (Geometry geom in geometries)
            {
                FeatureDataRow fdr = _features.NewRow();
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
        public GeometryFeatureProvider(Geometry geometry)
        {
            _features = new FeatureDataTable();
            FeatureDataRow fdr = _features.NewRow();
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
            get { return _features; }
        }

        #region IProvider Members

        /// <summary>
        /// Returns features within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            Collection<Geometry> list = new Collection<Geometry>();

            foreach (FeatureDataRow fdr in _features.Rows)
                if (!fdr.Geometry.IsEmpty())
                    if (fdr.Geometry.GetBoundingBox().Intersects(bbox))
                        list.Add(fdr.Geometry);
            return list;
        }

        /// <summary>
        /// Returns all objects whose boundingbox intersects 'bbox'.
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            Collection<uint> list = new Collection<uint>();
            for (int i = 0; i < _features.Rows.Count; i++)
                if ((_features.Rows[i] as FeatureDataRow).Geometry.GetBoundingBox().Intersects(bbox))
                    list.Add((uint) i);
            return list;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public Geometry GetGeometryByID(uint oid)
        {
            return (_features.Rows[(int) oid] as FeatureDataRow).Geometry;
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            FeatureDataTable fdt = new FeatureDataTable();
            fdt = _features.Clone();

            foreach (FeatureDataRow fdr in _features)
                if (fdr.Geometry.GetBoundingBox().Intersects(geom))
                {
                    fdt.LoadDataRow(fdr.ItemArray, false);
                    (fdt.Rows[fdt.Rows.Count - 1] as FeatureDataRow).Geometry = fdr.Geometry;
                }

            ds.Tables.Add(fdt);
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="box"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            FeatureDataTable fdt = new FeatureDataTable();
            fdt = _features.Clone();

            foreach (FeatureDataRow fdr in _features)
                if (fdr.Geometry.GetBoundingBox().Intersects(box))
                {
                    fdt.LoadDataRow(fdr.ItemArray, false);
                    (fdt.Rows[fdt.Rows.Count - 1] as FeatureDataRow).Geometry = fdr.Geometry;
                }

            ds.Tables.Add(fdt);
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            return _features.Rows.Count;
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns></returns>
        public FeatureDataRow GetFeature(uint RowID)
        {
            return _features[(int) RowID];
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public BoundingBox GetExtents()
        {
            if (_features.Rows.Count == 0)
                return null;
            BoundingBox box = null;
            foreach (FeatureDataRow fdr in _features.Rows)
                if (fdr.Geometry != null && !fdr.Geometry.IsEmpty())
                    box = box == null ? fdr.Geometry.GetBoundingBox() : box.Join(fdr.Geometry.GetBoundingBox());

            return box;
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
            get { return _SRID; }
            set { _SRID = value; }
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