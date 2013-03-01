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
using GeoAPI.Geometries;

namespace GeoAPI.CoordinateSystems.Transformations
{
    /// <summary>
    /// Helper class for transforming <see cref="GeoAPI.Geometries.IGeometry"/>
    /// </summary>
    public class GeometryTransform
    {
        /// <summary>
        /// Transforms a <see cref="Envelope"/>.
        /// </summary>
        /// <param name="box">BoundingBox to transform</param>
        /// <param name="transform">Math Transform</param>
        /// <returns>Transformed object</returns>
        public static Envelope TransformBox(Envelope box, IMathTransform transform)
        {
            if (box == null)
                return null;
            var corners = new Coordinate[4];
            var ll = box.Min().ToDoubleArray();
            var ur = box.Max().ToDoubleArray();
            var llTrans = transform.Transform(ll);
            var urTrans = transform.Transform(ur);
            corners[0] = new Coordinate(llTrans[0], llTrans[1]); //lower left
            corners[2] = new Coordinate(llTrans[0], urTrans[1]); //upper left
            corners[1] = new Coordinate(urTrans[0], urTrans[1]); //upper right
            corners[3] = new Coordinate(urTrans[0], llTrans[1]); //lower right

            var result = new Envelope(corners[0]);
            for (var i = 1; i < 4; i++)
                result.ExpandToInclude(corners[i]);
            return result;
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IGeometry"/>.
        /// </summary>
        /// <param name="g">Geometry to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed Geometry</returns>
        public static IGeometry TransformGeometry(IGeometry g, IMathTransform transform, IGeometryFactory targetFactory)
        {
            if (g == null)
                return null;
            if (g is IPoint)
                return TransformPoint(g as IPoint, transform, targetFactory);
            if (g is ILineString)
                return TransformLineString(g as ILineString, transform, targetFactory);
            if (g is IPolygon)
                return TransformPolygon(g as IPolygon, transform, targetFactory);
            if (g is IMultiPoint)
                return TransformMultiPoint(g as IMultiPoint, transform, targetFactory);
            if (g is IMultiLineString)
                return TransformMultiLineString(g as IMultiLineString, transform, targetFactory);
            if (g is IMultiPolygon)
                return TransformMultiPolygon(g as IMultiPolygon, transform, targetFactory);
            if (g is IGeometryCollection)
                return TransformGeometryCollection(g as IGeometryCollection, transform, targetFactory);
            throw new ArgumentException("Could not transform geometry type '" + g.GetType() + "'");
        }

        /// <summary>
        /// Function to transform a <paramref name="c"/> using <paramref name="transform"/>
        /// </summary>
        /// <param name="c">The coordinate</param>
        /// <param name="transform">The transformation</param>
        /// <returns>A transformed coordinate</returns>
        public static Coordinate TransformCoordinate(Coordinate c, IMathTransform transform)
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
        private static Coordinate[] TransformCoordinates(Coordinate[] c, IMathTransform transform)
        {
            var res = new Coordinate[c.Length];
            for (var i = 0; i < c.Length; i++ )
            {
                var ordinates = transform.Transform(c[i].ToDoubleArray());
                res[i] = new Coordinate(ordinates[0], ordinates[1]);
            }
            return res;
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IPoint"/>.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed Point</returns>
        public static IPoint TransformPoint(IPoint p, IMathTransform transform, IGeometryFactory targetFactory)
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
        /// Transforms a <see cref="GeoAPI.Geometries.ILineString"/>.
        /// </summary>
        /// <param name="l">LineString to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed LineString</returns>
        public static ILineString TransformLineString(ILineString l, IMathTransform transform, IGeometryFactory targetFactory)
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
        /// Transforms a <see cref="GeoAPI.Geometries.ILinearRing"/>.
        /// </summary>
        /// <param name="r">LinearRing to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed LinearRing</returns>
        public static ILinearRing TransformLinearRing(ILinearRing r, IMathTransform transform, IGeometryFactory targetFactory)
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
        /// Transforms a <see cref="GeoAPI.Geometries.IPolygon"/>.
        /// </summary>
        /// <param name="p">Polygon to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed Polygon</returns>
        public static IPolygon TransformPolygon(IPolygon p, IMathTransform transform, IGeometryFactory targetFactory)
        {
            var shell = TransformLinearRing((ILinearRing) p.ExteriorRing, transform, targetFactory);
            ILinearRing[] holes = null;
            var holesCount = p.NumInteriorRings;
            if (holesCount > 0)
            {
                holes = new ILinearRing[holesCount];
                for (var i = 0; i < holesCount; i++)
                    holes[i] = TransformLinearRing((ILinearRing)p.GetInteriorRingN(i), transform, targetFactory);
            }
            return targetFactory.CreatePolygon(shell, holes);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IMultiPoint"/>.
        /// </summary>
        /// <param name="points">MultiPoint to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed MultiPoint</returns>
        public static IMultiPoint TransformMultiPoint(IMultiPoint points, IMathTransform transform, IGeometryFactory targetFactory)
        {
            return targetFactory.CreateMultiPoint(TransformCoordinates(points.Coordinates, transform));
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IMultiLineString"/>.
        /// </summary>
        /// <param name="lines">MultiLineString to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed MultiLineString</returns>
        public static IMultiLineString TransformMultiLineString(IMultiLineString lines, IMathTransform transform, IGeometryFactory targetFactory)
        {
            var lineList = new ILineString[lines.NumGeometries];
            for (var i = 0; i < lines.NumGeometries; i++)
            {
                var line = (ILineString)lines[i];
                lineList[i] = TransformLineString(line, transform, targetFactory);
            }
            return targetFactory.CreateMultiLineString(lineList);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IMultiPolygon"/>.
        /// </summary>
        /// <param name="polys">MultiPolygon to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed MultiPolygon</returns>
        public static IMultiPolygon TransformMultiPolygon(IMultiPolygon polys, IMathTransform transform, IGeometryFactory targetFactory)
        {
            var polyList = new IPolygon[polys.NumGeometries];
            for (var i = 0; i < polys.NumGeometries; i++)
            {
                var poly = (IPolygon) polys[i];
                polyList[i] = TransformPolygon(poly, transform, targetFactory);
            }
            return targetFactory.CreateMultiPolygon(polyList);
        }

        /// <summary>
        /// Transforms a <see cref="GeoAPI.Geometries.IGeometryCollection"/>.
        /// </summary>
        /// <param name="geoms">GeometryCollection to transform</param>
        /// <param name="transform">MathTransform</param>
        /// <param name="targetFactory">The factory to create the target geometry</param>
        /// <returns>Transformed GeometryCollection</returns>
        public static IGeometryCollection TransformGeometryCollection(IGeometryCollection geoms, IMathTransform transform, IGeometryFactory targetFactory)
        {
            var geomList = new IGeometry[geoms.NumGeometries];
            for(var i = 0; i < geoms.NumGeometries; i++)
            {
                geomList[i] = TransformGeometry(geoms[i], transform, targetFactory);
            }
            return targetFactory.CreateGeometryCollection(geomList);
        }
    }
}

#endif