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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using GeoAPI.Features;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using SharpMap.Features;

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
    [Serializable]
    public class FeatureProvider : FilterProvider, IProvider
    {
        private readonly IFeatureCollection _features;

        [NonSerialized]
        private object _featuresLock = new object();

        private int _srid = -1;

        /// <summary>
        /// Access to underlying <see cref="IFeatureCollection"/>
        /// </summary>
        public IFeatureCollection Features
        {
            get
            {
                lock (_features)
                {
                    return _features;
                }
            }
        }

        private static IFeatureCollection CreateFeatureCollection(params IGeometry[] geometrys)
        {
            IGeometryFactory geomFactory;
            if (geometrys == null || geometrys.Length == 0|| geometrys[0] == null)
                geomFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(0);
            else
                geomFactory = geometrys[0].Factory;

            var res = new FeatureCollection<uint>(new GeometryFeatureFactory(geomFactory));
            if (geometrys != null)
            {
                foreach (var geometry in geometrys)
                {
                    if (geometry != null)
                        res.Add((GeometryFeature) res.Factory.Create(geometry));
                }
            }
            return res;
        }

        private static IFeatureCollection CreateFeatureCollection(IEnumerable<IGeometry> geometrys)
        {
            return CreateFeatureCollection(geometrys.ToArray());
        }

        [Serializable]
        private class GeometryFeature : IFeature<uint>, IFeatureAttributes
        {
            private readonly GeometryFeatureFactory _factory;

            public GeometryFeature(GeometryFeatureFactory factory)
            {
                _factory = factory;
            }
            
            public IFeatureAttributes Attributes { get { return this; } }

            public IFeatureFactory Factory { get { return _factory; } }

            public IGeometry Geometry { get; set; }

            public bool HasOidAssigned { get { return Oid > 0; } }

            object IUnique.Oid
            {
                get { return Oid; }
                set { Oid = (uint)value; }
            }

            public uint Oid { get; set; }

            object IFeatureAttributes.this[int index]
            {
                get
                {
                    if (index != 0)
                        throw new ArgumentOutOfRangeException("index");
                    return Oid;
                }
                set
                {
                    if (index != 0)
                        throw new ArgumentOutOfRangeException("index");
                    if (value.GetType() != typeof(uint))
                        throw new ArgumentException("Oid must be of type Sytem.UInt32");
                    Oid = (uint)value;
                }
            }

            object IFeatureAttributes.this[string key]
            {
                get
                {
                    if (!string.Equals(key, "Oid"))
                        throw new ArgumentOutOfRangeException("key");
                    return Oid;
                }
                set
                {
                    if (!string.Equals(key, "Oid"))
                        throw new ArgumentOutOfRangeException("key");
                    Oid = (uint)value;
                }
            }

            public object Clone()
            {
                return new GeometryFeature(_factory)
                {
                    Oid = Oid,
                    Geometry = (IGeometry)Geometry.Clone()
                };
            }

            public void Dispose()
            {
            }

            public Type GetEntityType()
            {
                return typeof(uint);
            }
            public object[] GetValues()
            {
                return new [] {(object)Oid};
            }
        }

        [Serializable]
        private class GeometryFeatureFactory : IFeatureFactory<uint>
        {
            private uint _lastId;

            public GeometryFeatureFactory(int srid)
                :this(GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(srid))
            {
            }

            public GeometryFeatureFactory(IGeometryFactory factory)
            {
                GeometryFactory = factory;
            }

            public IList<IFeatureAttributeDefinition> AttributesDefinition
            {
                get
                {
                    return new List<IFeatureAttributeDefinition>(new[]
                    {
                        new FeatureAttributeDefinition
                        {
                            AttributeDescription = "Object Identifier",
                            AttributeName = "Oid",
                            AttributeType = typeof (uint),
                            Default = 0,
                            IsNullable = false
                        }
                    });
                }
            }

            public IGeometryFactory GeometryFactory { get; set; }

            public IFeatureFactory Clone()
            {
                return new GeometryFeatureFactory(GeometryFactory);
            }

            public IFeature Create()
            {
                return new GeometryFeature(this) { Oid = GetNewOid() };
            }

            public IFeature Create(IGeometry geometry)
            {
                Debug.Assert(geometry.SRID == GeometryFactory.SRID);
                return new GeometryFeature(this) { Oid = GetNewOid(), Geometry = geometry };
            }

            public uint GetNewOid()
            {
                return ++_lastId;
            }
        }

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureProvider"/>
        /// </summary>
        /// <param name="geometries">Set of geometries that this datasource should contain</param>
        public FeatureProvider(IEnumerable<IGeometry> geometries)
            : this(CreateFeatureCollection(geometries))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureProvider"/>
        /// </summary>
        /// <param name="features">Features to be included in this datasource</param>
        public FeatureProvider(IFeatureCollection features)
        {
            _features = features;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureProvider"/>
        /// </summary>
        /// <param name="geometry">Geometry to be in this datasource</param>
        public FeatureProvider(params IGeometry[] geometry)
            :this(CreateFeatureCollection(geometry))
        {
        }

        #endregion

        #region IProvider Members

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        public string ConnectionID
        {
            get { return _features.Name; }
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
        /// Gets a feature factory for the features
        /// </summary>
        public IFeatureFactory Factory
        {
            get
            {
                if (_features is IHasFeatureFactory)
                    return ((IHasFeatureFactory) _features).Factory;
                return null;
            }
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Do nothing;
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            if (_features is IDisposable)
                ((IDisposable)_features).Dispose();
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geom, IFeatureCollectionSet ds, CancellationToken? cancellationToken = null)
        {
            IFeatureCollection fdt;
            lock (_featuresLock)
                fdt = _features.Clone();

            var pg = new NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory().Create(geom);
            fdt.AddRange(EnumerateFeatures(pg));
            ds.Add(fdt);
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="view"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope view, IFeatureCollectionSet ds, CancellationToken? cancellationToken = null)
        {
            IFeatureCollection fdt;
            lock (_featuresLock)
                fdt = _features.Clone();

            fdt.AddRange(EnumerateFeatures(view));

            ds.Add(fdt);
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public Envelope GetExtents()
        {
            lock (_featuresLock)
            {
                if (_features.Count == 0)
                    return null;
                var box = new Envelope();

                foreach (var fdr in _features)
                {
                    if (fdr.Geometry != null && !fdr.Geometry.IsEmpty)
                        box.ExpandToInclude(fdr.Geometry.EnvelopeInternal);
                }
                return box;
            }
        }

        /// <summary>
        /// Gets a specific feature from the data source by its <paramref name="oid"/>
        /// </summary>
        /// <param name="oid">The id of the feature</param>
        /// <returns>A feature data row</returns>
        public IFeature GetFeatureByOid(object oid)
        {
            lock (_featuresLock)
                return _features[oid];
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            lock (_features)
                return _features.Count;
        }

        /// <summary>
        /// Returns features within the specified bounding box
        /// </summary>
        /// <param name="view">The view</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A stream of geometries</returns>
        public IEnumerable<IGeometry> GetGeometriesInView(Envelope view, CancellationToken? cancellationToken =null)
        {
            lock (_features)
            {
                foreach (var fdr in EnumerateFeatures(view))
                    yield return fdr.Geometry;
            }
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public IGeometry GetGeometryByOid(object oid)
        {
            lock (_featuresLock)
            {
                var f = _features[oid];
                return f != null ? f.Geometry : null;
            }
        }

        /// <summary>
        /// Returns all object identifiers of features whose boundingbox intersects <paramref name="view"/>.
        /// </summary>
        /// <param name="view">The view</param>
        /// <returns>A stream of object Ids</returns>
        public IEnumerable<object> GetOidsInView(Envelope view, CancellationToken? cancellationToken = null)
        {
            foreach (var feature in EnumerateFeatures(view))
            {
                yield return feature.Oid;
            }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Do nothing;
        }

        private IEnumerable<IFeature> EnumerateFeatures(Envelope bbox)
        {
            lock (_featuresLock)
            {
                if (FilterDelegate == null)
                    foreach (var feature in _features)
                    {
                        var geom = feature.Geometry;
                        var testBox = geom != null ? geom.EnvelopeInternal : new Envelope(bbox);
                        if (bbox.Intersects(testBox))
                            yield return feature;
                    }
                else
                    foreach (var feature in _features)
                    {
                        var geom = feature.Geometry;
                        var testBox = geom != null ? geom.EnvelopeInternal : new Envelope(bbox);
                        if (bbox.Intersects(testBox) && FilterDelegate(feature))
                            yield return feature;
                    }
            }
        }

        private IEnumerable<IFeature> EnumerateFeatures(IPreparedGeometry preparedGeometry)
        {
            lock (_featuresLock)
            {
                uint id = 0;
                if (FilterDelegate == null)
                    foreach (var feature in _features)
                    {
                        var geom = feature.Geometry;
                        if (preparedGeometry.Intersects(feature.Geometry))
                            yield return feature;
                        id++;
                    }
                else
                    foreach (var feature in _features)
                    {
                        if (FilterDelegate(feature) && preparedGeometry.Intersects(feature.Geometry))
                            yield return feature;
                        id++;
                    }
            }
        }
        #endregion

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _featuresLock = new object();
        }
    }
}