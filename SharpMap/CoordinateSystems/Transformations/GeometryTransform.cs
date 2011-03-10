// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

#if !DotSpatialProjections
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpMap.Geometries;

namespace ProjNet.CoordinateSystems.Transformations
{
    /// <summary>
    /// Helper class for transforming <see cref="SharpMap.Geometries.Geometry"/>
    /// </summary>
    public class GeometryTransform
    {
        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.BoundingBox"/>.
        /// </summary>
        /// <param name="box">BoundingBox to transform</param>
        /// <param name="transform">Math Transform</param>
        /// <returns>Transformed object</returns>
        public static BoundingBox TransformBox(BoundingBox box, IMathTransform transform)
        {
            if (box == null)
                return null;
            Point[] corners = new Point[4];
            corners[0] = new Point(transform.Transform(box.Min.ToDoubleArray())); //LL
            corners[1] = new Point(transform.Transform(box.Max.ToDoubleArray())); //UR
            corners[2] = new Point(transform.Transform(new Point(box.Min.X, box.Max.Y).ToDoubleArray())); //UL
            corners[3] = new Point(transform.Transform(new Point(box.Max.X, box.Min.Y).ToDoubleArray())); //LR

            BoundingBox result = corners[0].GetBoundingBox();
            for (int i = 1; i < 4; i++)
                result = result.Join(corners[i].GetBoundingBox());
            return result;
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        /// <param name="g">Geometry to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed Geometry</returns>
        public static Geometry TransformGeometry(Geometry g, IMathTransform transform)
        {
            if (g == null)
                return null;
            if (g is Point)
                return TransformPoint(g as Point, transform);
            if (g is LineString)
                return TransformLineString(g as LineString, transform);
            if (g is Polygon)
                return TransformPolygon(g as Polygon, transform);
            if (g is MultiPoint)
                return TransformMultiPoint(g as MultiPoint, transform);
            if (g is MultiLineString)
                return TransformMultiLineString(g as MultiLineString, transform);
            if (g is MultiPolygon)
                return TransformMultiPolygon(g as MultiPolygon, transform);
            if (g is GeometryCollection)
                return TransformGeometryCollection(g as GeometryCollection, transform);
            throw new ArgumentException("Could not transform geometry type '" + g.GetType() + "'");
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.Point"/>.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed Point</returns>
        public static Point TransformPoint(Point p, IMathTransform transform)
        {
            try
            {
                return new Point(transform.Transform(p.ToDoubleArray()));
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
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed LineString</returns>
        public static LineString TransformLineString(LineString l, IMathTransform transform)
        {
            try
            {
                List<double[]> points = new List<double[]>();

                for (int i = 0; i < l.Vertices.Count; i++)
                    points.Add(new double[2] {l.Vertices[i].X, l.Vertices[i].Y});

                return new LineString(transform.TransformList(points));
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
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed LinearRing</returns>
        public static LinearRing TransformLinearRing(LinearRing r, IMathTransform transform)
        {
            try
            {
                List<double[]> points = new List<double[]>();

                for (int i = 0; i < r.Vertices.Count; i++)
                    points.Add(new double[2] {r.Vertices[i].X, r.Vertices[i].Y});

                return new LinearRing(transform.TransformList(points));
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
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed Polygon</returns>
        public static Polygon TransformPolygon(Polygon p, IMathTransform transform)
        {
            Polygon pOut = new Polygon(TransformLinearRing(p.ExteriorRing, transform));
            //pOut.InteriorRings = new Collection<LinearRing>(p.InteriorRings.Count); //Pre-inialize array size for better performance
            pOut.InteriorRings = new Collection<LinearRing>();
            for (int i = 0; i < p.InteriorRings.Count; i++)
                pOut.InteriorRings.Add(TransformLinearRing(p.InteriorRings[i], transform));
            return pOut;
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.MultiPoint"/>.
        /// </summary>
        /// <param name="points">MultiPoint to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed MultiPoint</returns>
        public static MultiPoint TransformMultiPoint(MultiPoint points, IMathTransform transform)
        {
            List<double[]> pts = new List<double[]>();
            for (int i = 0; i < points.NumGeometries; i++)
                pts.Add(new double[2] {points[0].X, points[1].Y});

            return new MultiPoint(transform.TransformList(pts));
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.MultiLineString"/>.
        /// </summary>
        /// <param name="lines">MultiLineString to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed MultiLineString</returns>
        public static MultiLineString TransformMultiLineString(MultiLineString lines, IMathTransform transform)
        {
            MultiLineString lOut = new MultiLineString();
            //lOut.LineStrings = new Collection<LineString>(lines.LineStrings.Count); //Pre-inialize array size for better performance
            lOut.LineStrings = new Collection<LineString>(); //Pre-inialize array size for better performance
            for (int i = 0; i < lines.LineStrings.Count; i++)
                lOut.LineStrings.Add(TransformLineString(lines[i], transform));
            return lOut;
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.MultiPolygon"/>.
        /// </summary>
        /// <param name="polys">MultiPolygon to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed MultiPolygon</returns>
        public static MultiPolygon TransformMultiPolygon(MultiPolygon polys, IMathTransform transform)
        {
            MultiPolygon pOut = new MultiPolygon();
            //pOut.Polygons = new Collection<Polygon>(polys.Polygons.Count); //Pre-inialize array size for better performance
            pOut.Polygons = new Collection<Polygon>();
            for (int i = 0; i < polys.NumGeometries; i++)
                pOut.Polygons.Add(TransformPolygon(polys[i], transform));
            return pOut;
        }

        /// <summary>
        /// Transforms a <see cref="SharpMap.Geometries.GeometryCollection"/>.
        /// </summary>
        /// <param name="geoms">GeometryCollection to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <returns>Transformed GeometryCollection</returns>
        public static GeometryCollection TransformGeometryCollection(GeometryCollection geoms, IMathTransform transform)
        {
            GeometryCollection gOut = new GeometryCollection();
            //gOut.Collection = new Collection<Geometry>(geoms.Collection.Count); //Pre-inialize array size for better performance
            gOut.Collection = new Collection<Geometry>(); //Pre-inialize array size for better performance
            for (int i = 0; i < geoms.Collection.Count; i++)
                gOut.Collection.Add(TransformGeometry(geoms[i], transform));
            return gOut;
        }
    }
}
#endif