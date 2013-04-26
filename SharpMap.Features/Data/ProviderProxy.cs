using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Features;
using GeoAPI.Geometries;
using GeoAPI.SpatialReference;
using SharpMap.Data.Providers;

namespace SharpMap.Data
{
    public class ProviderProxy : IFeatureProvider
    {
        private readonly IProvider _provider;
        private ISpatialReference _spatialReference;

        public ProviderProxy(IProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            _provider = provider;
        }

        public IProvider Provider { get { return _provider; } }

        public ISpatialReference SpatialReference
        {
            get
            {
                if (_spatialReference == null)
                {
                    //_spatialReference = Base.Services.Reprojector.Factory.CreateFromSrid(_provider.SRID);
                }
                return _spatialReference;
            }
        }

        public Envelope GetExtents()
        {
            return _provider.GetExtents();
        }

        public IGeometryReader ExecuteGeometryReader(SpatialPredicate predicate, Envelope envelope)
        {
            if (predicate != SpatialPredicate.Intersects)
                throw new NotSupportedException();
            
            return new ProviderGeometryReader(_provider.GetGeometriesInView(envelope));
        }

        public IGeometryReader ExecuteGeometryReader(SpatialPredicate predicate, IGeometry geometry)
        {
            if (predicate != SpatialPredicate.Intersects)
                throw new NotSupportedException();

            var envelope = geometry.EnvelopeInternal;
            var geometries = _provider.GetGeometriesInView(envelope);
            
            //ToDo Prepared geometries!
            return new ProviderGeometryReader(Filter(geometries, geometry.Intersects));
        }

        private static IEnumerable<IGeometry> Filter(IEnumerable<IGeometry> geometries, Func<IGeometry, bool> predicate)
        {
            foreach (var geometry in geometries)
            {
                if (predicate(geometry))
                    yield return geometry;
            }
        }

        public IFeatureReader ExecuteFeatureReader(SpatialPredicate predicate, Envelope envelope)
        {
            if (predicate != SpatialPredicate.Intersects)
                throw new NotSupportedException();

            var fds = new FeatureDataSet();
            _provider.ExecuteIntersectionQuery(envelope, fds);
            return new ProviderFeatureDataRowReader(fds.Tables[0]);
        }

        public IFeatureReader ExecuteFeatureReader(SpatialPredicate predicate, IGeometry geometry)
        {
            if (predicate != SpatialPredicate.Intersects)
                throw new NotSupportedException();

            var fds = new FeatureDataSet();
            _provider.ExecuteIntersectionQuery(geometry, fds);
            return new ProviderFeatureDataRowReader(fds.Tables[0]);
        }

#region Private Helper

        private class ProviderGeometryReader : IGeometryReader
        {
            private readonly Collection<IGeometry> _geometries;
            private int _index = -1;

            public ProviderGeometryReader(IEnumerable<IGeometry> geometries)
            {
                if (geometries is Collection<IGeometry>)
                {
                    _geometries = geometries as Collection<IGeometry>;
                }
                else
                {
                    _geometries = new Collection<IGeometry>();
                    foreach (var geometry in geometries)
                    {
                        _geometries.Add(geometry);
                    }
                }
                
                HasRecords = _geometries != null && _geometries.Count > 0;
            }


            public bool HasRecords { get; private set; }
            
            public bool Read()
            {
                if (!HasRecords) return false;
                _index ++;
                return _index < _geometries.Count;
            }

            public IGeometry Geometry
            {
                get
                {
                    if (_index < 0 || _index >= _geometries.Count)
                        throw new InvalidOperationException();
                    return _geometries[_index];
                }
            }
        }

        private class ProviderFeatureDataRowReader : IFeatureReader
        {
            private readonly FeatureDataTable _featureDataTable;
            private readonly IEnumerator _featureEnumerator;
            private FeatureDataRowProxy _feature;

            public ProviderFeatureDataRowReader(FeatureDataTable fdt)
            {
                _featureDataTable = fdt;
                HasRecords = _featureDataTable != null && _featureDataTable.Rows.Count > 0;
                
                _featureEnumerator = fdt.GetEnumerator();
            }


            public bool HasRecords { get; private set; }
            
            public bool Read()
            {
                _feature = null;
                var res = _featureEnumerator.MoveNext();
                if (res) _feature = new FeatureDataRowProxy((FeatureDataRow)_featureEnumerator.Current);
                return res;
            }

            public IGeometry Geometry
            {
                get 
                { 
                    return _feature != null 
                        ? _feature.Geometry
                        : null; 
                }
            }

            public IFeatureAttributes Attributes
            {
                get
                {
                    return _feature != null
                            ? _feature.Attributes
                            : null;
                }
            }

            public IFeature Feature { get { return _feature; } }
        }

#endregion
    }
}