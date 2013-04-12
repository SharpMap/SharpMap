using System;
using DotSpatial.Projections;
using GeoAPI.Geometries;
using GeoAPI.SpatialReference;
using GeoAPI.Utilities;

namespace SharpMap.SpatialReference
{
    /// <summary>
    /// Implementation of a <see cref="IReprojector"/>" that uses <see href="http://dotspatial.codeplex.com"/>'s reprojection functionality
    /// </summary>
    public class DotSpatialReprojector : IReprojectorCore
    {
        private static readonly ThreadSafeStore<ISpatialReference, ProjectionInfo> Infos = 
            new ThreadSafeStore<ISpatialReference, ProjectionInfo>(CreateProjectionInfo);

        private static ProjectionInfo GetProjectionInfo(ISpatialReference spatialReference)
        {
            return Infos.Get(spatialReference);
        }

        private static ProjectionInfo CreateProjectionInfo(ISpatialReference spatialReference)
        {
            var pisr = spatialReference as DotSpatialSpatialReference;
            if (pisr != null && pisr.ProjectionInfo != null)
                return pisr.ProjectionInfo;

            switch (spatialReference.DefinitionType)
            {
                case SpatialReferenceDefinitionType.Proj4:
                    return ProjectionInfo.FromProj4String(spatialReference.Definition);
                
                case SpatialReferenceDefinitionType.AuthorityCode:
                    var ac = spatialReference.Definition.Split(new [] {':'});
                    var srid = int.Parse(ac[1]);
                    return ProjectionInfo.FromEpsgCode(srid);

                case SpatialReferenceDefinitionType.WellKnownText:
                    return ProjectionInfo.FromEsriString(spatialReference.Definition);
                
                default:
                    throw new NotSupportedException();

            }
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