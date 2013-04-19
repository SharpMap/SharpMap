using System;
using System.Collections.Generic;
using System.Globalization;
using GeoAPI;
using GeoAPI.Features;
using GeoAPI.Geometries;
using GeoAPI.SpatialReference;
using GeoAPI.Utilities;

namespace SharpMap.SpatialReference
{
    /// <summary>
    /// Reprojector proxy class used and accessed within SharpMap
    /// </summary>
    public class Reprojector : IReprojector
    {
        /// <summary>
        /// The <see cref="IReprojector"/> implementation used internally
        /// </summary>
        private readonly IReprojectorCore _instance;

        private readonly ThreadSafeStore<ISpatialReference, IGeometryFactory> _factories =
            new ThreadSafeStore<ISpatialReference, IGeometryFactory>(CreateGeometryFactory);

        private IGeometryFactory GetFactory(ISpatialReference spatialReference)
        {
            return _factories.Get(spatialReference);
        }

        private static volatile int _newId = 1000000;

        private static IGeometryFactory CreateGeometryFactory(ISpatialReference spatialReference)
        {
            var split = spatialReference.Oid.Split(new[] { ':' });
            int srid;
            if (split.Length < 2)
            {
                srid = ++_newId;
            }
            else
            {
                if (!(Int32.TryParse(split[1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out srid)))
                    srid = ++_newId;
            }

            return GeometryServiceProvider.Instance.CreateGeometryFactory(srid);
        }


        /// <summary>
        /// Creates an instance of this class, assigning a <see cref="NoopReprojector"/>
        /// </summary>
        public Reprojector()
            :this(new NoopReprojector())
        {}

        /// <summary>
        /// Creates an instance of this class, that uses <paramref name="instance"/> <see cref="IReprojector"/> internally.
        /// </summary>
        /// <param name="instance"></param>
        public Reprojector(IReprojectorCore instance)
        {
            _instance = instance;
        }

        public Coordinate Reproject(Coordinate point, ISpatialReference from, ISpatialReference to)
        {
            return _instance.Reproject(point, from, to);
        }

        public Envelope Reproject(Envelope envelope, ISpatialReference @from, ISpatialReference to)
        {
            return _instance.Reproject(envelope, from, to);
        }

        public ICoordinateSequence Reproject(ICoordinateSequence sequence, ISpatialReference @from, ISpatialReference to)
        {
            return _instance.Reproject(sequence, from, to);
        }

        public ISpatialReferenceFactory Factory { get { return _instance.Factory; } }


        public IGeometry Reproject(IGeometry geometry, ISpatialReference @from, ISpatialReference to)
        {
            var factory = GetFactory(to);

            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    return ReprojectPoint(factory, (IPoint)geometry, @from, to);

                case OgcGeometryType.LineString:
                    return ReprojectLineString(factory, (ILineString)geometry, @from, to);

                case OgcGeometryType.Polygon:
                    return ReprojectPolygon(factory, (IPolygon)geometry, @from, to);

                default:
                    var lst = new List<IGeometry>(geometry.NumGeometries);
                    for (var i = 0; i < geometry.NumGeometries; i++)
                    {
                        lst.Add(Reproject(geometry.GetGeometryN(i), from, to));
                    }
                    return factory.BuildGeometry(lst);
            }
        }

        private IPoint ReprojectPoint(IGeometryFactory factory, IPoint point, ISpatialReference @from,
                                      ISpatialReference to)
        {
            return factory.CreatePoint(Reproject(point.CoordinateSequence, from, to));
        }

        private ILineString ReprojectLineString(IGeometryFactory factory, ILineString line, ISpatialReference @from,
                                                  ISpatialReference to, bool lineString = true)
        {
            return lineString
                       ? factory.CreateLineString(Reproject(line.CoordinateSequence, from, to))
                       : factory.CreateLinearRing(Reproject(line.CoordinateSequence, from, to));
        }

        private IPolygon ReprojectPolygon(IGeometryFactory factory, IPolygon polygon, ISpatialReference @from,
                                                 ISpatialReference to)
        {
            var exterior = (ILinearRing)ReprojectLineString(factory, polygon.ExteriorRing, from, to, false);
            var interior = new ILinearRing[polygon.NumInteriorRings];
            for (var i = 0; i < polygon.NumInteriorRings; i++)
            {
                interior[i] = (ILinearRing)ReprojectLineString(factory, polygon.GetInteriorRingN(i), from, to, false);
            }

            return factory.CreatePolygon(exterior, interior);
        }

        public IFeature Reproject(IFeature feature, ISpatialReference @from, ISpatialReference to)
        {
            feature.Geometry = Reproject(feature.Geometry, @from, to);
            return feature;
        }

        public IEnumerable<IFeature> Reproject(IEnumerable<IFeature> features, ISpatialReference @from, ISpatialReference to)
        {
            foreach (var feature in features)
            {
                yield return Reproject(feature, @from, @to);
            }
        }
    }
}