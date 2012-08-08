using System;
using GeoAPI;
using GeoAPI.Geometries;
using BoundingBox = GeoAPI.Geometries.Envelope;


#if DotSpatialProjections
#pragma warning disable 1587

namespace DotSpatial.Projections
{
    /// <summary>
    /// Helper class for transforming <see cref="GeoAPI.Geometries.IGeometry"/>
    /// </summary>
    public static class GeometryTransform
    {
        /// <summary>
        /// Transforms a <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="box">Geometry to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed BoundingBox</returns>
        public static BoundingBox TransformBox(BoundingBox box, ProjectionInfo from, ProjectionInfo to)
        {
            var corners = new[] { box.MinX, box.MinY, box.MinX, box.MaxY, box.MaxX, box.MaxY, box.MaxX, box.MinY };
            Reproject.ReprojectPoints(corners, null, from, to, 0, 4);

            var res = new BoundingBox(corners[0], corners[4], corners[1], corners[5]);
            res.ExpandToInclude(new BoundingBox(corners[2], corners[6], corners[3], corners[7]));
            return res;
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IGeometry"/>.
        /// </summary>
        /// <param name="g">Geometry to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <param name="toFactory">The factory to create the transformed geometry</param>
        /// <returns>Transformed Geometry</returns>
        public static IGeometry TransformGeometry(IGeometry g, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            if (g == null)
                return null;
            if (g is IPoint)
                return TransformPoint(g as IPoint, from, to, toFactory);
            if (g is ILineString)
                return TransformLineString(g as ILineString, from, to, toFactory);
            if (g is IPolygon)
                return TransformPolygon(g as IPolygon, from, to, toFactory);
            if (g is IMultiPoint)
                return TransformMultiPoint(g as IMultiPoint, from, to, toFactory);
            if (g is IMultiLineString)
                return TransformMultiLineString(g as IMultiLineString, from, to, toFactory);
            if (g is IMultiPolygon)
                return TransformMultiPolygon(g as IMultiPolygon, from, to, toFactory);
            if (g is IGeometryCollection)
                return TransformGeometryCollection(g as IGeometryCollection, from, to, toFactory);
            throw new ArgumentException("Could not transform geometry type '" + g.GetType() + "'");
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.Coordinate"/>.
        /// </summary>
        /// <param name="c">Point to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed coordinate</returns>
        public static Coordinate TransformCoordinate(Coordinate c, ProjectionInfo from, ProjectionInfo to)
        {
            var xy = c.ToDoubleArray();
            double[] z = !Double.IsNaN(c.Z) ? new double[1] : null;

            Reproject.ReprojectPoints(xy, z, from, to, 0, 1);
            return new Coordinate(xy[0], xy[1]) {Z = z != null ? z[0] : Coordinate.NullOrdinate};

        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IPoint"/>.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <param name="from">The source Projection</param>
        /// <param name="to">The target Projection</param>
        /// <param name="toFactory">The factory to create geometries for <paramref name="to"/></param>
        /// <returns>Transformed Point</returns>
        public static IPoint TransformPoint(IPoint p, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            try
            {
                var toSeq = TransformSequence(p.CoordinateSequence, from, to, toFactory.CoordinateSequenceFactory);
                return toFactory.CreatePoint(toSeq);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.ILineString"/>.
        /// </summary>
        /// <param name="l">LineString to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <param name="toFactory">The factory to create geometries for <paramref name="to"/></param>
        /// <returns>Transformed LineString</returns>
        public static ILineString TransformLineString(ILineString l, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            try
            {
                var toSeq = TransformSequence(l.CoordinateSequence, from, to, toFactory.CoordinateSequenceFactory);
                return toFactory.CreateLineString(toSeq);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.ILinearRing"/>.
        /// </summary>
        /// <param name="r">LinearRing to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <param name="toFactory">The factory to create geometries for <paramref name="to"/></param>
        /// <returns>Transformed LinearRing</returns>
        public static ILinearRing TransformLinearRing(ILinearRing r, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            try
            {
                var toSeq = TransformSequence(r.CoordinateSequence, from, to, toFactory.CoordinateSequenceFactory);
                return toFactory.CreateLinearRing(toSeq);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IPolygon"/>.
        /// </summary>
        /// <param name="p">Polygon to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <param name="toFactory">The factory to create geometries for <paramref name="to"/></param>
        /// <returns>Transformed Polygon</returns>
        public static IPolygon TransformPolygon(IPolygon p, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            var shell = toFactory.CreateLinearRing(TransformSequence(p.Shell.CoordinateSequence, from, to, toFactory.CoordinateSequenceFactory));
            var holes = new ILinearRing[p.NumInteriorRings];
            for (var i = 0; i < p.NumInteriorRings; i++)
                holes[i] = toFactory.CreateLinearRing(TransformSequence(p.GetInteriorRingN(i).CoordinateSequence, from, to, toFactory.CoordinateSequenceFactory));

            return toFactory.CreatePolygon(shell, holes);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IMultiPoint"/>.
        /// </summary>
        /// <param name="points">MultiPoint to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <param name="toFactory">The factory to create geometries for <paramref name="to"/></param>
        /// <returns>Transformed MultiPoint</returns>
        public static IMultiPoint TransformMultiPoint(IMultiPoint points, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            try
            {
                var seq = toFactory.CoordinateSequenceFactory.Create(points.Coordinates);
                var toSeq = TransformSequence(seq, from, to, toFactory.CoordinateSequenceFactory);
                return toFactory.CreateMultiPoint(toSeq);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IMultiLineString"/>.
        /// </summary>
        /// <param name="lines">MultiLineString to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <param name="toFactory">The factory to create geometries for <paramref name="to"/></param>
        /// <returns>Transformed MultiLineString</returns>
        public static IMultiLineString TransformMultiLineString(IMultiLineString lines, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            var l = new ILineString[lines.Count];
            for (var i = 0; i < lines.Count; i++)
                l[i] = TransformLineString((ILineString)lines.GetGeometryN(i), from, to, toFactory);

            return toFactory.CreateMultiLineString(l);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IMultiPolygon"/>.
        /// </summary>
        /// <param name="polys">MultiPolygon to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <param name="toFactory">The factory to create geometries for <paramref name="to"/></param>
        /// <returns>Transformed MultiPolygon</returns>
        public static IMultiPolygon TransformMultiPolygon(IMultiPolygon polys, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            var pOut = new IPolygon[polys.Count];
            for (var i = 0; i < polys.Count; i++)
                pOut[i] = TransformPolygon((IPolygon)polys.GetGeometryN(i), from, to, toFactory);

            return toFactory.CreateMultiPolygon(pOut);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IGeometryCollection"/>.
        /// </summary>
        /// <param name="geoms">GeometryCollection to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <param name="toFactory">The factory to create geometries for <paramref name="to"/></param>
        /// <returns>Transformed GeometryCollection</returns>
        public static IGeometryCollection TransformGeometryCollection(IGeometryCollection geoms, ProjectionInfo from, ProjectionInfo to, IGeometryFactory toFactory)
        {
            var gOut = new IGeometry[geoms.Count];
            for (var i = 0; i < geoms.Count; i++)
                gOut[i] = TransformGeometry(geoms.GetGeometryN(i), from, to, toFactory);

            return toFactory.CreateGeometryCollection(gOut);
        }

        private static ICoordinateSequence TransformSequence(ICoordinateSequence sequence, ProjectionInfo from, ProjectionInfo to, ICoordinateSequenceFactory factory)
        {
            var res = factory.Create(sequence.Count, sequence.Ordinates);

            double[] z;
            double[] ordinates;
            if (sequence is NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequence)
            {
                var dss = (NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequence) sequence;
                z = (double[])dss.Z.Clone();
                ordinates = (double[])dss.XY.Clone();
            }
            else
            {
                ordinates = ToDoubleArray(sequence, out z);
            }

            Reproject.ReprojectPoints(ordinates, z, from, to, 0, sequence.Count);

            if (res is NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequence)
            {
                var dss = (NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequence) res;
                Array.Copy(ordinates, dss.XY, ordinates.Length);
                if (z != null)
                    Array.Copy(z, dss.Z, z.Length);
            }
            else
            {
                var j = 0;
                for (var i = 0; i < sequence.Count; i++)
                {
                    res.SetOrdinate(i, Ordinate.X, ordinates[j++]);
                    res.SetOrdinate(i, Ordinate.Y, ordinates[j++]);
                    if (z != null)
                        res.SetOrdinate(i, Ordinate.Z, z[i]);
                }
            }
            return res;
        }

        private static double[] ToDoubleArray(ICoordinateSequence sequence, out double[] z)
        {
            var res = new double[sequence.Count*2];
            z = ((sequence.Ordinates & Ordinates.Z) == Ordinates.Z) ? new double[sequence.Count] : null;
            
            var j = 0;
            for (var i = 0; i < sequence.Count; i++)
            {
                res[j++] = sequence.GetOrdinate(i, Ordinate.X);
                res[j++] = sequence.GetOrdinate(i, Ordinate.Y);
                if (z != null)
                    z[i] = sequence.GetOrdinate(i, Ordinate.Z);
            }
            return res;
        }
    }

    /// <summary>
    /// Interface for coordiante transfromations
    /// </summary>
    public interface ICoordinateTransformation
    {
        /// <summary>
        /// Gets the source coordinate reference System 
        /// </summary>
        ProjectionInfo Source { get; }

        /// <summary>
        /// Gets the factory that can create geometries in the <see cref="Source"/> coordinate reference system
        /// </summary>
        IGeometryFactory SourceFactory { get; }

        /// <summary>
        /// Gets the target coordinate reference system
        /// </summary>
        ProjectionInfo Target { get; }

        /// <summary>
        /// Gets the factory that can create geometries in the <see cref="Target"/> coordinate reference system
        /// </summary>
        IGeometryFactory TargetFactory { get; }
    }

    /// <summary>
    /// Coordinate transformation class
    /// </summary>
    public class CoordinateTransformation : ICoordinateTransformation
    {
        private ProjectionInfo _source;
        private IGeometryFactory _sourceFactory;
        private ProjectionInfo _target;
        private IGeometryFactory _targetFactory;

        public ProjectionInfo Source
        {
            get { return _source; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                
                _source = value;
                _sourceFactory = GeometryServiceProvider.Instance.CreateGeometryFactory(_source.EpsgCode);
            }
        }

        /// <summary>
        /// Gets the factory that can create geometries in the <see cref="ICoordinateTransformation.Source"/> coordinate reference system
        /// </summary>
        public IGeometryFactory SourceFactory
        {
            get { return _sourceFactory; }
        }

        public ProjectionInfo Target
        {
            get { return _target; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                _target = value;
                _targetFactory = GeometryServiceProvider.Instance.CreateGeometryFactory(_target.EpsgCode);
            }
        }

        /// <summary>
        /// Gets the factory that can create geometries in the <see cref="ICoordinateTransformation.Target"/> coordinate reference system
        /// </summary>
        public IGeometryFactory TargetFactory
        {
            get { return _targetFactory; }
        }
    }
}
#pragma warning restore 1587
#endif