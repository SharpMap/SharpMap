using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpMap.Geometries;

#if DotSpatialProjections
#pragma warning disable 1587

namespace DotSpatial.Projections
{
    /// <summary>
    /// Helper class for transforming <see cref="SharpMap.Geometries.Geometry"/>
    /// </summary>
    public static class GeometryTransform
    {
        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.BoundingBox"/>
        /// </summary>
        /// <param name="box">Geometry to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed BoundingBox</returns>
        public static BoundingBox TransformBox(BoundingBox box, ProjectionInfo from, ProjectionInfo to)
        {
            var corners = new[] { box.Left, box.Bottom, box.Left, box.Top, box.Right, box.Top, box.Right, box.Bottom };
            Reproject.ReprojectPoints(corners, null, from, to, 0, 4);

            return new BoundingBox(corners[0], corners[1], corners[4], corners[5]).Join(
                   new BoundingBox(corners[2], corners[3], corners[6], corners[7]));
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        /// <param name="g">Geometry to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed Geometry</returns>
        public static Geometry TransformGeometry(Geometry g, ProjectionInfo from, ProjectionInfo to)
        {
            if (g == null)
                return null;
            if (g is Point)
                return TransformPoint(g as Point, from, to);
            if (g is LineString)
                return TransformLineString(g as LineString, from, to);
            if (g is Polygon)
                return TransformPolygon(g as Polygon, from, to);
            if (g is MultiPoint)
                return TransformMultiPoint(g as MultiPoint, from, to);
            if (g is MultiLineString)
                return TransformMultiLineString(g as MultiLineString, from, to);
            if (g is MultiPolygon)
                return TransformMultiPolygon(g as MultiPolygon, from, to);
            if (g is GeometryCollection)
                return TransformGeometryCollection(g as GeometryCollection, from, to);
            throw new ArgumentException("Could not transform geometry type '" + g.GetType() + "'");
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.Point"/>.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed Point</returns>
        public static Point TransformPoint(Point p, ProjectionInfo from, ProjectionInfo to)
        {
            try
            {
                double[] coords = p.ToDoubleArray();
                Reproject.ReprojectPoints(coords, null, from, to, 0, 1);
                return new Point(coords);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.LineString"/>.
        /// </summary>
        /// <param name="l">LineString to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed LineString</returns>
        public static LineString TransformLineString(LineString l, ProjectionInfo from, ProjectionInfo to)
        {
            try
            {
                List<double[]> points = new List<double[]>();

                for (int i = 0; i < l.Vertices.Count; i++)
                    points.Add(new double[] {l.Vertices[i].X, l.Vertices[i].Y});

                return new LineString(TransformList(points, from, to));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.LinearRing"/>.
        /// </summary>
        /// <param name="r">LinearRing to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed LinearRing</returns>
        public static LinearRing TransformLinearRing(LinearRing r, ProjectionInfo from, ProjectionInfo to)
        {
            try
            {
                List<double[]> points = new List<double[]>();

                for (int i = 0; i < r.Vertices.Count; i++)
                    points.Add(new double[] {r.Vertices[i].X, r.Vertices[i].Y});

                return new LinearRing(TransformList(points, from, to));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.Polygon"/>.
        /// </summary>
        /// <param name="p">Polygon to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed Polygon</returns>
        public static Polygon TransformPolygon(Polygon p, ProjectionInfo from, ProjectionInfo to)
        {
            var pOut = new Polygon(TransformLinearRing(p.ExteriorRing, from, to))
                           {InteriorRings = new Collection<LinearRing>()};
            //pOut.InteriorRings = new Collection<LinearRing>(p.InteriorRings.Count); //Pre-inialize array size for better performance
            for (var i = 0; i < p.InteriorRings.Count; i++)
                pOut.InteriorRings.Add(TransformLinearRing(p.InteriorRings[i], from, to));
            return pOut;
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.MultiPoint"/>.
        /// </summary>
        /// <param name="points">MultiPoint to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed MultiPoint</returns>
        public static MultiPoint TransformMultiPoint(MultiPoint points, ProjectionInfo from, ProjectionInfo to)
        {
            var pts = new List<double[]>();
            for (var i = 0; i < points.NumGeometries; i++)
                pts.Add(new[] {points[0].X, points[1].Y});

            return new MultiPoint(TransformList(pts, from, to));
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.MultiLineString"/>.
        /// </summary>
        /// <param name="lines">MultiLineString to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed MultiLineString</returns>
        public static MultiLineString TransformMultiLineString(MultiLineString lines, ProjectionInfo from, ProjectionInfo to)
        {
            var lOut = new MultiLineString {LineStrings = new Collection<LineString>()};

            for (var i = 0; i < lines.LineStrings.Count; i++)
                lOut.LineStrings.Add(TransformLineString(lines[i], from, to));
            return lOut;
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.MultiPolygon"/>.
        /// </summary>
        /// <param name="polys">MultiPolygon to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed MultiPolygon</returns>
        public static MultiPolygon TransformMultiPolygon(MultiPolygon polys, ProjectionInfo from, ProjectionInfo to)
        {
            var pOut = new MultiPolygon {Polygons = new Collection<Polygon>()};
            for (var i = 0; i < polys.NumGeometries; i++)
                pOut.Polygons.Add(TransformPolygon(polys[i], from, to));
            return pOut;
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.GeometryCollection"/>.
        /// </summary>
        /// <param name="geoms">GeometryCollection to transform</param>
        /// <param name="from">Source Projection</param>
        /// <param name="to">Target Projection</param>
        /// <returns>Transformed GeometryCollection</returns>
        public static GeometryCollection TransformGeometryCollection(GeometryCollection geoms, ProjectionInfo from, ProjectionInfo to)
        {
            var gOut = new GeometryCollection {Collection = new Collection<Geometry>()};
            for (var i = 0; i < geoms.Collection.Count; i++)
                gOut.Collection.Add(TransformGeometry(geoms[i], from, to));
            return gOut;
        }

        private static IEnumerable<double[]> TransformList(IEnumerable<double[]> list, ProjectionInfo from, ProjectionInfo to)
        {
            foreach (double[] coord in list)
            {
                Reproject.ReprojectPoints(coord, null, from, to, 0, 1);
                yield return coord;
            }
        }

    }

    /// <summary>
    /// Interface for coordiante transfromations
    /// </summary>
    public interface ICoordinateTransformation
    {
        /// <summary>
        /// Source coordinate reference System 
        /// </summary>
        ProjectionInfo Source { get; }
        /// <summary>
        /// Target coordinate reference system
        /// </summary>
        ProjectionInfo Target { get; }
    }

    /// <summary>
    /// Coordinate transformation class
    /// </summary>
    public class CoordinateTransformation : ICoordinateTransformation
    {
        public ProjectionInfo Source { get; set; }

        public ProjectionInfo Target { get; set; }
    }
}
#pragma warning restore 1587
#endif