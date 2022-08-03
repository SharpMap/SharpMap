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

using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;
using System;

// ReSharper disable once CheckNamespace
namespace GeoAPI.CoordinateSystems.Transformations
{
    /// <summary>
    /// Helper class for transforming <see cref="T:GeoAPI.Geometries.Geometry"/>
    /// </summary>
    public class GeometryTransform
    {
        /// <summary>
        /// Transforms a <see cref="Envelope"/>.
        /// </summary>
        /// <param name="box">BoundingBox to transform</param>
        /// <param name="transform">Math Transform</param>
        /// <returns>Transformed object</returns>
        public static Envelope TransformBox(Envelope box, MathTransform transform)
        {
            if (box == null)
                return null;

            if (box.IsNull)
                return new Envelope(box);

            var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();
            var ring = factory.ToGeometry(box);
            ring = NetTopologySuite.Densify.Densifier.Densify(ring, ring.Length / 100d);
            ring = TransformGeometry(ring, transform, factory);

            var res = ring.EnvelopeInternal;
            var t = transform.Transform(new[] { box.Centre.X, box.Centre.Y });
            res.ExpandToInclude(t[0], t[1]);
            return res;
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.Geometry"/>.
        /// </summary>
        /// <param name="g">Geometry to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed Geometry</returns>
        public static Geometry TransformGeometry(Geometry g, MathTransform transform, GeometryFactory targetFactory)
        {
            if (g == null)
                return null;
            if (g is Point)
                return TransformPoint(g as Point, transform, targetFactory);
            if (g is LineString)
                return TransformLineString(g as LineString, transform, targetFactory);
            if (g is Polygon)
                return TransformPolygon(g as Polygon, transform, targetFactory);
            if (g is MultiPoint)
                return TransformMultiPoint(g as MultiPoint, transform, targetFactory);
            if (g is MultiLineString)
                return TransformMultiLineString(g as MultiLineString, transform, targetFactory);
            if (g is MultiPolygon)
                return TransformMultiPolygon(g as MultiPolygon, transform, targetFactory);
            if (g is GeometryCollection)
                return TransformGeometryCollection(g as GeometryCollection, transform, targetFactory);
            throw new ArgumentException("Could not transform geometry type '" + g.GetType() + "'");
        }

        /// <summary>
        /// Function to transform a <paramref name="c"/> using <paramref name="transform"/>
        /// </summary>
        /// <param name="c">The coordinate</param>
        /// <param name="transform">The transformation</param>
        /// <returns>A transformed coordinate</returns>
        public static Coordinate TransformCoordinate(Coordinate c, MathTransform transform)
        {
            var ordinates = transform.Transform(c.ToDoubleArray());
            return new Coordinate(ordinates[0], ordinates[1]);
        }

        /// <summary>
        /// Function to transform <paramref name="c"/> using <paramref name="transform"/>
        /// </summary>
        /// <param name="c">The array of coordinates</param>
        /// <param name="transform">The transformation</param>
        /// <returns>An array of transformed coordinates</returns>
        private static Coordinate[] TransformCoordinates(Coordinate[] c, MathTransform transform)
        {
            var res = new Coordinate[c.Length];
            for (var i = 0; i < c.Length; i++)
            {
                var ordinates = transform.Transform(c[i].ToDoubleArray());
                res[i] = new Coordinate(ordinates[0], ordinates[1]);
            }
            return res;
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.Point"/>.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed Point</returns>
        public static Point TransformPoint(Point p, MathTransform transform, GeometryFactory targetFactory)
        {
            try
            {

                return targetFactory.CreatePoint(TransformCoordinate(p.Coordinate, transform));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.LineString"/>.
        /// </summary>
        /// <param name="l">LineString to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed LineString</returns>
        public static LineString TransformLineString(LineString l, MathTransform transform, GeometryFactory targetFactory)
        {
            try
            {
                return targetFactory.CreateLineString(TransformCoordinates(l.Coordinates, transform));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.LinearRing"/>.
        /// </summary>
        /// <param name="r">LinearRing to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed LinearRing</returns>
        public static LinearRing TransformLinearRing(LinearRing r, MathTransform transform, GeometryFactory targetFactory)
        {
            try
            {
                return targetFactory.CreateLinearRing(TransformCoordinates(r.Coordinates, transform));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.Polygon"/>.
        /// </summary>
        /// <param name="p">Polygon to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed Polygon</returns>
        public static Polygon TransformPolygon(Polygon p, MathTransform transform, GeometryFactory targetFactory)
        {
            var shell = TransformLinearRing((LinearRing)p.ExteriorRing, transform, targetFactory);
            LinearRing[] holes = null;
            var holesCount = p.NumInteriorRings;
            if (holesCount > 0)
            {
                holes = new LinearRing[holesCount];
                for (var i = 0; i < holesCount; i++)
                    holes[i] = TransformLinearRing((LinearRing)p.GetInteriorRingN(i), transform, targetFactory);
            }
            return targetFactory.CreatePolygon(shell, holes);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.MultiPoint"/>.
        /// </summary>
        /// <param name="points">MultiPoint to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed MultiPoint</returns>
        public static MultiPoint TransformMultiPoint(MultiPoint points, MathTransform transform, GeometryFactory targetFactory)
        {
            return targetFactory.CreateMultiPointFromCoords(TransformCoordinates(points.Coordinates, transform));
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.MultiLineString"/>.
        /// </summary>
        /// <param name="lines">MultiLineString to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed MultiLineString</returns>
        public static MultiLineString TransformMultiLineString(MultiLineString lines, MathTransform transform, GeometryFactory targetFactory)
        {
            var lineList = new LineString[lines.NumGeometries];
            for (var i = 0; i < lines.NumGeometries; i++)
            {
                var line = (LineString)lines[i];
                lineList[i] = TransformLineString(line, transform, targetFactory);
            }
            return targetFactory.CreateMultiLineString(lineList);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.MultiPolygon"/>.
        /// </summary>
        /// <param name="polys">MultiPolygon to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed MultiPolygon</returns>
        public static MultiPolygon TransformMultiPolygon(MultiPolygon polys, MathTransform transform, GeometryFactory targetFactory)
        {
            var polyList = new Polygon[polys.NumGeometries];
            for (var i = 0; i < polys.NumGeometries; i++)
            {
                var poly = (Polygon)polys[i];
                polyList[i] = TransformPolygon(poly, transform, targetFactory);
            }
            return targetFactory.CreateMultiPolygon(polyList);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.GeometryCollection"/>.
        /// </summary>
        /// <param name="geoms">GeometryCollection to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed GeometryCollection</returns>
        public static GeometryCollection TransformGeometryCollection(GeometryCollection geoms, MathTransform transform, GeometryFactory targetFactory)
        {
            var geomList = new Geometry[geoms.NumGeometries];
            for (var i = 0; i < geoms.NumGeometries; i++)
            {
                geomList[i] = TransformGeometry(geoms[i], transform, targetFactory);
            }
            return targetFactory.CreateGeometryCollection(geomList);
        }
    }
}
