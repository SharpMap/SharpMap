// Copyright 2006 - Morten Nielsen (www.iter.dk)
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
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Converters.WellKnownText;
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
    /// laySelected.DataSource = new GeometryProvider(geometries);
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
    /// SharpMap.Layers.VectorLayer layerVehicles = new SharpMap.Layers.VectorLayer("Vehicles");
    /// layerVehicles.DataSource = new SharpMap.Data.Providers.GeometryProvider(geometries);
    /// layerVehicles.Style.Symbol = Bitmap.FromFile(@"C:\data\car.gif");
    /// myMap.Layers.Add(layerVehicles);
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable]
    public class GeometryProvider : PreparedGeometryProvider
    {
        private List<IGeometry> _geometries;

        /// <summary>
        /// Gets or sets the geometries this datasource contains
        /// </summary>
        public IList<IGeometry> Geometries
        {
            get { return _geometries; }
            set
            {
                if (!ReferenceEquals(_geometries, value))
                {
                    var list = value as List<IGeometry> ?? new List<IGeometry>(value);
                    _geometries = list;

                    if (_geometries != null && _geometries.Count > 0)
                        SRID = _geometries[0].SRID;
                }
            }
        }

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="geometries">Set of geometries that this datasource should contain</param>
        public GeometryProvider(IEnumerable<IGeometry> geometries)
        {
            Geometries = new List<IGeometry>(geometries);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="feature">Feature to be in this datasource</param>
        public GeometryProvider(FeatureDataRow feature)
        {
            Geometries = new List<IGeometry> {feature.Geometry};
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="features">Features to be included in this datasource</param>
        public GeometryProvider(FeatureDataTable features)
        {
            var geometries = new List<IGeometry>();
            for (var i = 0; i < features.Count; i++)
                geometries.Add(features[i].Geometry);
            Geometries = geometries;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="geometry">Geometry to be in this datasource</param>
        public GeometryProvider(IGeometry geometry)
        {
            Geometries = geometry != null ? new List<IGeometry> { geometry } : new List<IGeometry>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="wellKnownBinaryGeometry"><see cref="GeoAPI.Geometries.IGeometry"/> as Well-known Binary to be included in this datasource</param>
        public GeometryProvider(byte[] wellKnownBinaryGeometry) 
            : this(GeometryFromWKB.Parse(wellKnownBinaryGeometry, GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryProvider"/>
        /// </summary>
        /// <param name="wellKnownTextGeometry"><see cref="GeoAPI.Geometries.IGeometry"/> as Well-known Text to be included in this datasource</param>
        public GeometryProvider(string wellKnownTextGeometry) : this(GeometryFromWKT.Parse(wellKnownTextGeometry))
        {
        }

        #endregion

        #region IProvider Members

        /// <summary>
        /// Returns features within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var list = new Collection<IGeometry>();
            for (var i = 0; i < _geometries.Count; i++)
                if (!_geometries[i].IsEmpty)
                    if (bbox.Intersects(_geometries[i].EnvelopeInternal))
                        list.Add(_geometries[i]);
            return list;
        }

        /// <summary>
        /// Returns all objects whose boundingbox intersects 'bbox'.
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var list = new Collection<uint>();
            for (int i = 0; i < _geometries.Count; i++)
                if (bbox.Intersects(_geometries[i].EnvelopeInternal))
                    list.Add((uint) i);
            return list;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public override IGeometry GetGeometryByID(uint oid)
        {
            return _geometries[(int) oid];
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            throw new NotSupportedException("Attribute data is not supported by the GeometryProvider.");
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="box"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public override void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            throw new NotSupportedException("Attribute data is not supported by the GeometryProvider.");
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public override int GetFeatureCount()
        {
            return _geometries.Count;
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public override FeatureDataRow GetFeature(uint rowId)
        {
            throw new NotSupportedException("Attribute data is not supported by the GeometryProvider.");
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public override Envelope GetExtents()
        {
            if (_geometries.Count == 0)
                return null;
            var box = new Envelope(_geometries[0].EnvelopeInternal);

            for (var i = 0; i < _geometries.Count; i++)
            {
                if (!_geometries[i].IsEmpty)
                    box.ExpandToInclude(_geometries[i].EnvelopeInternal);
            }
            return box;
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (_geometries != null)
            {
                _geometries.Clear();
                _geometries = null;
            }
        }

        #endregion
    }
}