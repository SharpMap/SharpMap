using System.Collections.Generic;
using System.Globalization;
using DotSpatial.Projections;
using GeoAPI.Features;
using GeoAPI.Geometries;
using GeoAPI.SpatialReference;
using GeoAPI.Utilities;

namespace SharpMap.SpatialReference
{
    /// <summary>
    /// Implementation of a <see cref="IReprojector"/>" that uses <see href="http://dotspatial.codeplex.com"/>'s reprojection functionality
    /// </summary>
    public class DotSpatialReprojector : IReprojector
    {
        private static volatile int _newId = 1000000;

        private static readonly ThreadSafeStore<ISpatialReference, ProjectionInfo> Infos = 
            new ThreadSafeStore<ISpatialReference, ProjectionInfo>(CreateProjectionInfo);
        
        private readonly ThreadSafeStore<ISpatialReference, IGeometryFactory> _factories = 
            new ThreadSafeStore<ISpatialReference, IGeometryFactory>(CreateGeometryFactory);

        private static ProjectionInfo GetProjectionInfo(ISpatialReference spatialReference)
        {
            return Infos.Get(spatialReference);
        }

        private static ProjectionInfo CreateProjectionInfo(ISpatialReference spatialReference)
        {
            return ProjectionInfo.FromProj4String(spatialReference.Definition);
        }

        private IGeometryFactory GetFactory(ISpatialReference spatialReference)
        {
            return _factories.Get(spatialReference);
        }


        private static IGeometryFactory CreateGeometryFactory(ISpatialReference spatialReference)
        {
            var split = spatialReference.Oid.Split(new [] {':'});
            int srid;
            if (split.Length < 2)
            {
                srid = ++_newId;
            }
            else
            {
                if (!(int.TryParse(split[1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out srid)))
                    srid = ++_newId;
            }

            return GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(srid);
        }

        public Coordinate Reproject(Coordinate coordinate, ISpatialReference @from, ISpatialReference to)
        {
            double[] xy, z;
            ToDotSpatial(coordinate, out xy, out z);
            DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, GetProjectionInfo(@from), GetProjectionInfo(to), 0, 1);
            return ToGeoAPI(xy, z);
        }

        public Envelope Reproject(Envelope envelope, ISpatialReference @from, ISpatialReference to)
        {
            double[] xy;
            ToDotSpatial(envelope, out xy);
            DotSpatial.Projections.Reproject.ReprojectPoints(xy, null, GetProjectionInfo(@from), GetProjectionInfo(to), 0, 4);
            return ToGeoAPI(xy);
        }

        public ICoordinateSequence Reproject(ICoordinateSequence sequence, ISpatialReference @from, ISpatialReference to)
        {
            double[] xy, z, m;
            ToDotSpatial(sequence, out xy, out z, out m);
            DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, GetProjectionInfo(@from), GetProjectionInfo(to), 0, sequence.Count);
            return ToGeoAPI(DefaultSequenceFactory, xy, z, m);
        }

        protected ICoordinateSequenceFactory DefaultSequenceFactory { get; private set; }

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

        #region Static helper methods and functions
        
        private static void ToDotSpatial(Coordinate c, out double[] xy, out double[] z)
        {
            xy = new[] { c.X, c.Y };
            z = double.IsNaN(c.Z) ? null : new[] { c.Z };
        }

        private static void ToDotSpatial(Envelope c, out double[] xy)
        {
            xy = new[] { c.MinX, c.MinY, c.MaxX, c.MinY, c.MaxX, c.MaxY, c.MinX, c.MaxY };
        }

        private static void ToDotSpatial(ICoordinateSequence c, out double[] xy, out double[] z, out double[] m)
        {
            xy = new double[2 * c.Count];
            z = ((c.Ordinates & Ordinates.Z) == Ordinates.Z) ? new double[c.Count] : null;
            m = ((c.Ordinates & Ordinates.M) == Ordinates.M) ? new double[c.Count] : null;

            var j = 0;
            for (var i = 0; i < c.Count; i++)
            {
                xy[j++] = c.GetOrdinate(i, Ordinate.X);
                xy[j++] = c.GetOrdinate(i, Ordinate.Y);
            }

            if (z != null)
            {
                for (var i = 0; i < c.Count; i++)
                    xy[i] = c.GetOrdinate(i, Ordinate.Z);
            }

            if (m != null)
            {
                for (var i = 0; i < c.Count; i++)
                    xy[i] = c.GetOrdinate(i, Ordinate.M);
            }
        }

        private static Coordinate ToGeoAPI(double[] xy, double[] z, int index = 0)
        {
            return new Coordinate(xy[index], xy[index + 1], z == null ? Coordinate.NullOrdinate : z[index]);
        }

        private static Envelope ToGeoAPI(double[] xy)
        {
            var i = 0;
            var res = new Envelope(new Coordinate(xy[i++], xy[i++]));
            res.ExpandToInclude(new Coordinate(xy[i++], xy[i++]));
            res.ExpandToInclude(new Coordinate(xy[i++], xy[i++]));
            res.ExpandToInclude(new Coordinate(xy[i++], xy[i]));

            return res;
        }

        private static ICoordinateSequence ToGeoAPI(ICoordinateSequenceFactory factory, double[] xy, double[] z, double[] m)
        {
            var ordinates = Ordinates.XY;
            if (z != null) ordinates |= Ordinates.Z;
            if (m != null) ordinates |= Ordinates.M;

            var res = factory.Create(xy.Length / 2, ordinates);
            var j = 0;
            for (var i = 0; i < res.Count; i++)
            {
                res.SetOrdinate(i, Ordinate.X, xy[j++]);
                res.SetOrdinate(i, Ordinate.Y, xy[j++]);
            }

            if (z != null && HasOrdinate(res, Ordinate.Z))
            {
                for (var i = 0; i < res.Count; i++)
                {
                    res.SetOrdinate(i, Ordinate.Z, z[i]);
                }
            }

            if (m != null && HasOrdinate(res, Ordinate.M))
            {
                for (var i = 0; i < res.Count; i++)
                {
                    res.SetOrdinate(i, Ordinate.Z, m[i]);
                }
            }

            return res;
        }

        private static bool HasOrdinate(ICoordinateSequence seq, Ordinate ordinate)
        {
            return (seq.Ordinates & OrdinatesUtility.ToOrdinatesFlag(new[] { ordinate })) != Ordinates.None;
        }

        #endregion

    }
}