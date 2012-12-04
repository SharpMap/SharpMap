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

// SOURCECODE IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

using System;
using System.Globalization;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Converters.WellKnownText
{
    /// <summary>
    /// Outputs the textual representation of a <see cref="GeoAPI.Geometries.IGeometry"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The Well-Known Text (WKT) representation of Geometry is designed to exchange geometry data in ASCII form.</para>
    /// Examples of WKT representations of geometry objects are:
    /// <list type="table">
    /// <listheader><term>Geometry </term><description>WKT Representation</description></listheader>
    /// <item><term>A Point</term>
    /// <description>POINT(15 20)<br/> Note that point coordinates are specified with no separating comma.</description></item>
    /// <item><term>A LineString with four points:</term>
    /// <description>LINESTRING(0 0, 10 10, 20 25, 50 60)</description></item>
    /// <item><term>A Polygon with one exterior ring and one interior ring:</term>
    /// <description>POLYGON((0 0,10 0,10 10,0 10,0 0),(5 5,7 5,7 7,5 7, 5 5))</description></item>
    /// <item><term>A MultiPoint with three Point values:</term>
    /// <description>MULTIPOINT(0 0, 20 20, 60 60)</description></item>
    /// <item><term>A MultiLineString with two LineString values:</term>
    /// <description>MULTILINESTRING((10 10, 20 20), (15 15, 30 15))</description></item>
    /// <item><term>A MultiPolygon with two Polygon values:</term>
    /// <description>MULTIPOLYGON(((0 0,10 0,10 10,0 10,0 0)),((5 5,7 5,7 7,5 7, 5 5)))</description></item>
    /// <item><term>A GeometryCollection consisting of two Point values and one LineString:</term>
    /// <description>GEOMETRYCOLLECTION(POINT(10 10), POINT(30 30), LINESTRING(15 15, 20 20))</description></item>
    /// </list>
    /// </remarks>
    public class GeometryToWKT
    {
        /// <summary>
        /// Converts a Geometry to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A Geometry to write.</param>
        /// <returns>A &lt;Geometry Tagged Text&gt; string (see the OpenGIS Simple
        ///  Features Specification)</returns>
        public static string Write(IGeometry geometry)
        {
            StringWriter sw = new StringWriter();
            Write(geometry, sw);
            return sw.ToString();
        }

        /// <summary>
        /// Converts a Geometry to its Well-known Text representation.
        /// </summary>
        /// <param name="geometry">A geometry to process.</param>
        /// <param name="writer">Stream to write out the geometry's text representation.</param>
        /// <remarks>
        /// Geometry is written to the output stream as &lt;Geometry Tagged Text&gt; string (see the OpenGIS
        /// Simple Features Specification).
        /// </remarks>
        public static void Write(IGeometry geometry, StringWriter writer)
        {
            WKTWriter wkt = new WKTWriter();
            wkt.Write(geometry, writer);
            //AppendGeometryTaggedText(geometry, writer);
        }

        /// <summary>
        /// Converts a Geometry to &lt;Geometry Tagged Text &gt; format, then Appends it to the writer.
        /// </summary>
        /// <param name="geometry">The Geometry to process.</param>
        /// <param name="writer">The output stream to Append to.</param>
        private static void AppendGeometryTaggedText(IGeometry geometry, StringWriter writer)
        {
            if (geometry == null)
                throw new NullReferenceException("Cannot write Well-Known Text: geometry was null");
            
            if (geometry is IPoint)
            {
                var point = geometry as IPoint;
                AppendPointTaggedText(point, writer);
            }
            else if (geometry is ILineString)
                AppendLineStringTaggedText(geometry as ILineString, writer);
            else if (geometry is IPolygon)
                AppendPolygonTaggedText(geometry as IPolygon, writer);
            else if (geometry is IMultiPoint)
                AppendMultiPointTaggedText(geometry as IMultiPoint, writer);
            else if (geometry is IMultiLineString)
                AppendMultiLineStringTaggedText(geometry as IMultiLineString, writer);
            else if (geometry is IMultiPolygon)
                AppendMultiPolygonTaggedText(geometry as IMultiPolygon, writer);
            else if (geometry is IGeometryCollection)
                AppendGeometryCollectionTaggedText(geometry as IGeometryCollection, writer);
            else
                throw new NotSupportedException("Unsupported Geometry implementation:" + geometry.GetType().Name);
        }

        /// <summary>
        /// Converts a Coordinate to &lt;Point Tagged Text&gt; format,
        /// then Appends it to the writer.
        /// </summary>
        /// <param name="coordinate">the <code>Coordinate</code> to process</param>
        /// <param name="writer">the output writer to Append to</param>
        private static void AppendPointTaggedText(IPoint coordinate, StringWriter writer)
        {
            writer.Write("POINT ");
            AppendPointText(coordinate, writer);
        }

        /// <summary>
        /// Converts a LineString to LineString tagged text format, 
        /// </summary>
        /// <param name="lineString">The LineString to process.</param>
        /// <param name="writer">The output stream writer to Append to.</param>
        private static void AppendLineStringTaggedText(ILineString lineString, StringWriter writer)
        {
            writer.Write("LINESTRING ");
            AppendLineStringText(lineString, writer);
        }

        /// <summary>
        ///  Converts a Polygon to &lt;Polygon Tagged Text&gt; format,
        ///  then Appends it to the writer.
        /// </summary>
        /// <param name="polygon">Th Polygon to process.</param>
        /// <param name="writer">The stream writer to Append to.</param>
        private static void AppendPolygonTaggedText(IPolygon polygon, StringWriter writer)
        {
            writer.Write("POLYGON ");
            AppendPolygonText(polygon, writer);
        }

        /// <summary>
        /// Converts a MultiPoint to &lt;MultiPoint Tagged Text&gt;
        /// format, then Appends it to the writer.
        /// </summary>
        /// <param name="multipoint">The MultiPoint to process.</param>
        /// <param name="writer">The output writer to Append to.</param>
        private static void AppendMultiPointTaggedText(IMultiPoint multipoint, StringWriter writer)
        {
            writer.Write("MULTIPOINT ");
            AppendMultiPointText(multipoint, writer);
        }

        /// <summary>
        /// Converts a MultiLineString to &lt;MultiLineString Tagged
        /// Text&gt; format, then Appends it to the writer.
        /// </summary>
        /// <param name="multiLineString">The MultiLineString to process</param>
        /// <param name="writer">The output stream writer to Append to.</param>
        private static void AppendMultiLineStringTaggedText(IMultiLineString multiLineString, StringWriter writer)
        {
            writer.Write("MULTILINESTRING ");
            AppendMultiLineStringText(multiLineString, writer);
        }

        /// <summary>
        /// Converts a MultiPolygon to &lt;MultiPolygon Tagged
        /// Text&gt; format, then Appends it to the writer.
        /// </summary>
        /// <param name="multiPolygon">The MultiPolygon to process</param>
        /// <param name="writer">The output stream writer to Append to.</param>
        private static void AppendMultiPolygonTaggedText(IMultiPolygon multiPolygon, StringWriter writer)
        {
            writer.Write("MULTIPOLYGON ");
            AppendMultiPolygonText(multiPolygon, writer);
        }

        /// <summary>
        /// Converts a GeometryCollection to &lt;GeometryCollection Tagged
        /// Text&gt; format, then Appends it to the writer.
        /// </summary>
        /// <param name="geometryCollection">The GeometryCollection to process</param>
        /// <param name="writer">The output stream writer to Append to.</param>
        private static void AppendGeometryCollectionTaggedText(IGeometryCollection geometryCollection,
                                                               StringWriter writer)
        {
            writer.Write("GEOMETRYCOLLECTION ");
            AppendGeometryCollectionText(geometryCollection, writer);
        }


        /// <summary>
        /// Converts a Coordinate to Point Text format then Appends it to the writer.
        /// </summary>
        /// <param name="coordinate">The Coordinate to process.</param>
        /// <param name="writer">The output stream writer to Append to.</param>
        private static void AppendPointText(IPoint coordinate, StringWriter writer)
        {
            if (coordinate == null || coordinate.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                AppendCoordinate(coordinate.Coordinate, writer);
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a Coordinate to &lt;Point&gt; format, then Appends
        /// it to the writer. 
        /// </summary>
        /// <param name="coordinate">The Coordinate to process.</param>
        /// <param name="writer">The output writer to Append to.</param>
        private static void AppendCoordinate(Coordinate coordinate, StringWriter writer)
        {
            writer.Write(WriteNumber(coordinate[Ordinate.X]));
            writer.Write(' ');
            writer.Write(WriteNumber(coordinate[Ordinate.Y]));
            if (!double.IsNaN(coordinate.Z))
            {
                writer.Write(' ');
                writer.Write(WriteNumber(coordinate[Ordinate.Y]));
            }
        }

        /// <summary>
        /// Converts a double to a string, not in scientific notation.
        /// </summary>
        /// <param name="d">The double to convert.</param>
        /// <returns>The double as a string, not in scientific notation.</returns>
        private static string WriteNumber(double d)
        {
            return d.ToString(NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a LineString to &lt;LineString Text&gt; format, then
        /// Appends it to the writer.
        /// </summary>
        /// <param name="lineString">The LineString to process.</param>
        /// <param name="writer">The output stream to Append to.</param>
        private static void AppendLineStringText(ILineString lineString, StringWriter writer)
        {
            if (lineString == null || lineString.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                var vertices = lineString.Coordinates;
                writer.Write("(");
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendCoordinate(vertices[i], writer);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a Polygon to &lt;Polygon Text&gt; format, then
        /// Appends it to the writer.
        /// </summary>
        /// <param name="polygon">The Polygon to process.</param>
        /// <param name="writer"></param>
        private static void AppendPolygonText(IPolygon polygon, StringWriter writer)
        {
            if (polygon == null || polygon.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                AppendLineStringText(polygon.ExteriorRing, writer);
                if (polygon.NumInteriorRings > 0)
                {
                    foreach (var ring in polygon.InteriorRings)
                    {
                        writer.Write(", ");
                        AppendLineStringText(ring, writer);
                    }
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a MultiPoint to &lt;MultiPoint Text&gt; format, then
        /// Appends it to the writer.
        /// </summary>
        /// <param name="multiPoint">The MultiPoint to process.</param>
        /// <param name="writer">The output stream writer to Append to.</param>
        private static void AppendMultiPointText(IMultiPoint multiPoint, StringWriter writer)
        {
            if (multiPoint == null || multiPoint.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                var vertices = multiPoint.Coordinates;
                writer.Write("(");
                for (var i = 0; i < vertices.Length; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendCoordinate(vertices[i], writer);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a MultiLineString to &lt;MultiLineString Text&gt;
        /// format, then Appends it to the writer.
        /// </summary>
        /// <param name="multiLineString">The MultiLineString to process.</param>
        /// <param name="writer">The output stream writer to Append to.</param>
        private static void AppendMultiLineStringText(IMultiLineString multiLineString, StringWriter writer)
        {
            if (multiLineString == null || multiLineString.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (var i = 0; i < multiLineString.NumGeometries; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendLineStringText((ILineString)multiLineString.GetGeometryN(i), writer);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a MultiPolygon to &lt;MultiPolygon Text&gt; format, then Appends to it to the writer.
        /// </summary>
        /// <param name="multiPolygon">The MultiPolygon to process.</param>
        /// <param name="writer">The output stream to Append to.</param>
        private static void AppendMultiPolygonText(IMultiPolygon multiPolygon, StringWriter writer)
        {
            if (multiPolygon == null || multiPolygon.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (int i = 0; i < multiPolygon.NumGeometries; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendPolygonText((IPolygon)multiPolygon.GetGeometryN(i), writer);
                }
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a GeometryCollection to &lt;GeometryCollection Text &gt; format, then Appends it to the writer.
        /// </summary>
        /// <param name="geometryCollection">The GeometryCollection to process.</param>
        /// <param name="writer">The output stream writer to Append to.</param>
        private static void AppendGeometryCollectionText(IGeometryCollection geometryCollection, StringWriter writer)
        {
            if (geometryCollection == null || geometryCollection.IsEmpty)
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (var i = 0; i < geometryCollection.NumGeometries; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendGeometryTaggedText(geometryCollection[i], writer);
                }
                writer.Write(")");
            }
        }
    }
}