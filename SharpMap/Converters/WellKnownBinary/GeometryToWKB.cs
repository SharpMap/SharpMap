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

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.IO;

namespace SharpMap.Converters.WellKnownBinary
{
    /// <summary>
    /// Converts a <see cref="NetTopologySuite.Geometries.Geometry"/> instance to a Well-known Binary string representation.
    /// </summary>
    /// <remarks>
    /// <para>The Well-known Binary Representation for <see cref="NetTopologySuite.Geometries.Geometry"/> (WKBGeometry) provides a portable 
    /// representation of a <see cref="NetTopologySuite.Geometries.Geometry"/> value as a contiguous stream of bytes. It permits <see cref="NetTopologySuite.Geometries.Geometry"/> 
    /// values to be exchanged between an ODBC client and an SQL database in binary form.</para>
    /// <para>The Well-known Binary Representation for <see cref="NetTopologySuite.Geometries.Geometry"/> is obtained by serializing a <see cref="NetTopologySuite.Geometries.Geometry"/>
    /// instance as a sequence of numeric types drawn from the set {Unsigned Integer, Double} and
    /// then serializing each numeric type as a sequence of bytes using one of two well defined,
    /// standard, binary representations for numeric types (NDR, XDR). The specific binary encoding
    /// (NDR or XDR) used for a geometry byte stream is described by a one byte tag that precedes
    /// the serialized bytes. The only difference between the two encodings of geometry is one of
    /// byte order, the XDR encoding is Big Endian, the NDR encoding is Little Endian.</para>
    /// </remarks> 
    public class GeometryToWKB
    {
        //private const byte WKBByteOrder = 0;

        /// <summary>
        /// Writes a geometry to a byte array using little endian byte encoding
        /// </summary>
        /// <param name="g">The geometry to write</param>
        /// <returns>WKB representation of the geometry</returns>
        public static byte[] Write(Geometry g)
        {
            return Write(g, WkbByteOrder.Ndr);
        }

        /// <summary>
        /// Writes a geometry to a byte array using the specified encoding.
        /// </summary>
        /// <param name="g">The geometry to write</param>
        /// <param name="wkbByteOrder">Byte order</param>
        /// <returns>WKB representation of the geometry</returns>
        public static byte[] Write(Geometry g, WkbByteOrder wkbByteOrder)
        {
            ByteOrder order;
            switch (wkbByteOrder)
            {
                case WkbByteOrder.Xdr:
                    order = ByteOrder.BigEndian;
                    break;
                case WkbByteOrder.Ndr:
                    order = ByteOrder.LittleEndian;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("wkbByteOrder");
            }

            WKBWriter wkb = new WKBWriter(order);
            return wkb.Write(g);

            /*
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            //Write the byteorder format.
            bw.Write((byte) wkbByteOrder);

            //Write the type of this geometry
            WriteType(g, bw, wkbByteOrder);

            //Write the geometry
            WriteGeometry(g, bw, wkbByteOrder);

            return ms.ToArray();
            */
        }

        /// <summary>
        /// Writes an unsigned integer to the binarywriter using the specified encoding
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="writer">Binary Writer</param>
        /// <param name="byteOrder">byteorder</param>
        private static void WriteUInt32(UInt32 value, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                writer.Write(BitConverter.ToUInt32(bytes, 0));
            }
            else
                writer.Write(value);
        }

        /// <summary>
        /// Writes a double to the binarywriter using the specified encoding
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="writer">Binary Writer</param>
        /// <param name="byteOrder">byteorder</param>
        private static void WriteDouble(double value, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                writer.Write(BitConverter.ToDouble(bytes, 0));
            }
            else
                writer.Write(value);
        }

        #region Methods

        /// <summary>
        /// Writes the type number for this geometry.
        /// </summary>
        /// <param name="geometry">The geometry to determine the type of.</param>
        /// <param name="bWriter">Binary Writer</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteType(Geometry geometry, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Determine the type of the geometry.
            switch (geometry.OgcGeometryType)
            {
                //Points are type 1.
                case OgcGeometryType.Point:
                    WriteUInt32((uint)WKBGeometryType.wkbPoint, bWriter, byteorder);
                    break;
                //Linestrings are type 2.
                case OgcGeometryType.LineString:
                    WriteUInt32((uint)WKBGeometryType.wkbLineString, bWriter, byteorder);
                    break;
                //Polygons are type 3.
                case OgcGeometryType.Polygon:
                    WriteUInt32((uint)WKBGeometryType.wkbPolygon, bWriter, byteorder);
                    break;
                //Mulitpoints are type 4.
                case OgcGeometryType.MultiPoint:
                    WriteUInt32((uint)WKBGeometryType.wkbMultiPoint, bWriter, byteorder);
                    break;
                //Multilinestrings are type 5.
                case OgcGeometryType.MultiLineString:
                    WriteUInt32((uint)WKBGeometryType.wkbMultiLineString, bWriter, byteorder);
                    break;
                //Multipolygons are type 6.
                case OgcGeometryType.MultiPolygon:
                    WriteUInt32((uint)WKBGeometryType.wkbMultiPolygon, bWriter, byteorder);
                    break;
                //Geometrycollections are type 7.
                case OgcGeometryType.GeometryCollection:
                    WriteUInt32((uint)WKBGeometryType.wkbGeometryCollection, bWriter, byteorder);
                    break;
                //If the type is not of the above 7 throw an exception.
                default:
                    throw new ArgumentException("Invalid Geometry Type");
            }
        }

        /// <summary>
        /// Writes the geometry to the binary writer.
        /// </summary>
        /// <param name="geometry">The geometry to be written.</param>
        /// <param name="bWriter"></param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteGeometry(Geometry geometry, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            switch (geometry.OgcGeometryType)
            {
                //Write the point.
                case OgcGeometryType.Point:
                    WritePoint((Point)geometry, bWriter, byteorder);
                    break;
                case OgcGeometryType.LineString:
                    var ls = (LineString)geometry;
                    WriteLineString(ls, bWriter, byteorder);
                    break;
                case OgcGeometryType.Polygon:
                    WritePolygon((Polygon)geometry, bWriter, byteorder);
                    break;
                //Write the Multipoint.
                case OgcGeometryType.MultiPoint:
                    WriteMultiPoint((MultiPoint)geometry, bWriter, byteorder);
                    break;
                //Write the Multilinestring.
                case OgcGeometryType.MultiLineString:
                    WriteMultiLineString((MultiLineString)geometry, bWriter, byteorder);
                    break;
                //Write the Multipolygon.
                case OgcGeometryType.MultiPolygon:
                    WriteMultiPolygon((MultiPolygon)geometry, bWriter, byteorder);
                    break;
                //Write the Geometrycollection.
                case OgcGeometryType.GeometryCollection:
                    WriteGeometryCollection((GeometryCollection)geometry, bWriter, byteorder);
                    break;
                //If the type is not of the above 7 throw an exception.
                default:
                    throw new ArgumentException("Invalid Geometry Type");
            }
        }

        /// <summary>
        /// Writes a point.
        /// </summary>
        /// <param name="point">The point to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WritePoint(Coordinate point, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the x coordinate.
            WriteDouble(point.X, bWriter, byteorder);
            //Write the y coordinate.
            WriteDouble(point.Y, bWriter, byteorder);
        }

        /// <summary>
        /// Writes a point.
        /// </summary>
        /// <param name="point">The point to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WritePoint(Point point, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the coordinate.
            WritePoint(point.Coordinate, bWriter, byteorder);
        }


        /// <summary>
        /// Writes a linestring.
        /// </summary>
        /// <param name="ls">The linestring to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteLineString(LineString ls, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            var vertices = ls.Coordinates;

            //Write the number of points in this linestring.
            WriteUInt32((uint)vertices.Length, bWriter, byteorder);

            //Loop on each vertices.
            foreach (var p in vertices)
                WritePoint(p, bWriter, byteorder);
        }


        /// <summary>
        /// Writes a polygon.
        /// </summary>
        /// <param name="poly">The polygon to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WritePolygon(Polygon poly, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Get the number of rings in this polygon.
            var numRings = poly.NumInteriorRings + 1;

            //Write the number of rings to the stream (add one for the shell)
            WriteUInt32((uint)numRings, bWriter, byteorder);

            //Write the exterior of this polygon.
            WriteLineString(poly.ExteriorRing, bWriter, byteorder);

            //Loop on the number of rings - 1 because we already wrote the shell.
            foreach (LinearRing lr in poly.InteriorRings)
                //Write the (lineString)LinearRing.
                WriteLineString(lr, bWriter, byteorder);
        }

        /// <summary>
        /// Writes a multipoint.
        /// </summary>
        /// <param name="mp">The multipoint to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteMultiPoint(MultiPoint mp, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            var vertices = mp.Coordinates;

            //Write the number of points.
            WriteUInt32((uint)vertices.Length, bWriter, byteorder);

            //Loop on the number of points.
            foreach (var p in vertices)
            {
                //Write Points Header
                bWriter.Write((byte)byteorder);
                WriteUInt32((uint)WKBGeometryType.wkbPoint, bWriter, byteorder);
                //Write each point.
                WritePoint(p, bWriter, byteorder);
            }
        }

        /// <summary>
        /// Writes a multilinestring.
        /// </summary>
        /// <param name="mls">The multilinestring to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteMultiLineString(MultiLineString mls, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of linestrings.
            int num = mls.NumGeometries;
            WriteUInt32((uint)num, bWriter, byteorder);

            //Loop on the number of linestrings. 
            //NOTE: by contract, the first item returned 
            //      from GetEnumerator (i.e. using foreach) is the MultiLineString itself!
            for (int i = 0; i < num; i++)
            {
                LineString ls = (LineString)mls.GetGeometryN(i);
                //Write LineString Header
                bWriter.Write((byte)byteorder);
                WriteUInt32((uint)WKBGeometryType.wkbLineString, bWriter, byteorder);
                //Write each linestring.
                WriteLineString(ls, bWriter, byteorder);
            }
        }

        /// <summary>
        /// Writes a multipolygon.
        /// </summary>
        /// <param name="mp">The mulitpolygon to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteMultiPolygon(MultiPolygon mp, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of polygons.
            int num = mp.NumGeometries;
            WriteUInt32((uint)num, bWriter, byteorder);

            //Loop on the number of polygons.
            //NOTE: by contract, the first item returned 
            //      from GetEnumerator (i.e. using foreach) is the MultiPolygon itself!
            for (int i = 0; i < num; i++)
            {
                Polygon poly = (Polygon)mp.GetGeometryN(i);
                //Write polygon header
                bWriter.Write((byte)byteorder);
                WriteUInt32((uint)WKBGeometryType.wkbPolygon, bWriter, byteorder);
                //Write each polygon.
                WritePolygon(poly, bWriter, byteorder);
            }
        }


        /// <summary>
        /// Writes a geometrycollection.
        /// </summary>
        /// <param name="gc">The geometrycollection to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteGeometryCollection(GeometryCollection gc, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Get the number of geometries in this geometrycollection.
            var num = gc.NumGeometries;

            //Write the number of geometries.
            WriteUInt32((uint)num, bWriter, byteorder);

            //Loop on the number of geometries.
            //NOTE: by contract, the first item returned 
            //      from GetEnumerator (i.e. using foreach) is the GeometryCollection itself!
            for (var i = 0; i < num; i++)
            {
                Geometry geom = gc.GetGeometryN(i);
                //Write the byte-order format of the following geometry.
                bWriter.Write((byte)byteorder);
                //Write the type of each geometry.                
                WriteType(geom, bWriter, byteorder);
                //Write each geometry.
                WriteGeometry(geom, bWriter, byteorder);
            }
        }

        #endregion
    }
}
