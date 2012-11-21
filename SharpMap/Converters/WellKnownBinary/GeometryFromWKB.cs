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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Converters.WellKnownBinary
{
    /// <summary>
    ///  Converts Well-known Binary representations to a <see cref="GeoAPI.Geometries.IGeometry"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The Well-known Binary Representation for <see cref="GeoAPI.Geometries.IGeometry"/> (WKBGeometry) provides a portable 
    /// representation of a <see cref="GeoAPI.Geometries.IGeometry"/> value as a contiguous stream of bytes. It permits <see cref="GeoAPI.Geometries.IGeometry"/> 
    /// values to be exchanged between an ODBC client and an SQL database in binary form.</para>
    /// <para>The Well-known Binary Representation for <see cref="GeoAPI.Geometries.IGeometry"/> is obtained by serializing a <see cref="GeoAPI.Geometries.IGeometry"/>
    /// instance as a sequence of numeric types drawn from the set {Unsigned Integer, Double} and
    /// then serializing each numeric type as a sequence of bytes using one of two well defined,
    /// standard, binary representations for numeric types (NDR, XDR). The specific binary encoding
    /// (NDR or XDR) used for a geometry byte stream is described by a one byte tag that precedes
    /// the serialized bytes. The only difference between the two encodings of geometry is one of
    /// byte order, the XDR encoding is Big Endian, the NDR encoding is Little Endian.</para>
    /// </remarks> 
    public class GeometryFromWKB
    {
        /// <summary>
        /// Creates a <see cref="GeoAPI.Geometries.IGeometry"/> from the supplied byte[] containing the Well-known Binary representation.
        /// </summary>
        /// <param name="bytes">byte[] containing the Well-known Binary representation.</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>A <see cref="GeoAPI.Geometries.IGeometry"/> bases on the supplied Well-known Binary representation.</returns>
        public static IGeometry Parse(byte[] bytes, IGeometryFactory factory)
        {            
            // Create a memory stream using the suppiled byte array.
            using (var ms = new MemoryStream(bytes))
            {
                // Create a new binary reader using the newly created memorystream.
                using (var reader = new BinaryReader(ms))
                {
                    // Call the main create function.
                    return Parse(reader, factory);
                }                
            }            
        }

        /// <summary>
        /// Creates a <see cref="GeoAPI.Geometries.IGeometry"/> based on the Well-known binary representation.
        /// </summary>
        /// <param name="reader">A <see cref="System.IO.BinaryReader">BinaryReader</see> used to read the Well-known binary representation.</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>A <see cref="GeoAPI.Geometries.IGeometry"/> based on the Well-known binary representation.</returns>
        public static IGeometry Parse(BinaryReader reader, IGeometryFactory factory)
        {
            WKBReader wkb = new WKBReader();
            return wkb.Read(reader.BaseStream);

            /*
            // Get the first Byte in the array. This specifies if the WKB is in
            // XDR (big-endian) format of NDR (little-endian) format.
            var byteOrder = reader.ReadByte();

            // Get the type of this geometry.
            var type = ReadUInt32(reader, (WkbByteOrder) byteOrder);

            switch ((WKBGeometryType) type)
            {
                case WKBGeometryType.wkbPoint:
                    return CreateWKBPoint(reader, (WkbByteOrder) byteOrder, factory);

                case WKBGeometryType.wkbLineString:
                    return CreateWKBLineString(reader, (WkbByteOrder)byteOrder, factory);

                case WKBGeometryType.wkbPolygon:
                    return CreateWKBPolygon(reader, (WkbByteOrder)byteOrder, factory);

                case WKBGeometryType.wkbMultiPoint:
                    return CreateWKBMultiPoint(reader, (WkbByteOrder)byteOrder, factory);

                case WKBGeometryType.wkbMultiLineString:
                    return CreateWKBMultiLineString(reader, (WkbByteOrder)byteOrder, factory);

                case WKBGeometryType.wkbMultiPolygon:
                    return CreateWKBMultiPolygon(reader, (WkbByteOrder)byteOrder, factory);

                case WKBGeometryType.wkbGeometryCollection:
                    return CreateWKBGeometryCollection(reader, (WkbByteOrder)byteOrder, factory);

                default:
                    if (!Enum.IsDefined(typeof (WKBGeometryType), type))
                        throw new ArgumentException("Geometry type not recognized");
                    else
                        throw new NotSupportedException("Geometry type '" + type + "' not supported");
            }
            */
        }

        private static IPoint CreateWKBPoint(BinaryReader reader, WkbByteOrder byteOrder, IGeometryFactory factory)
        {
            // Create and return the point.
            return factory.CreatePoint(new Coordinate(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder)));
        }

        private static Coordinate[] ReadCoordinates(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of points in this linestring.
            var numPoints = (int) ReadUInt32(reader, byteOrder);

            // Create a new array of coordinates.
            var coords = new Coordinate[numPoints];

            // Loop on the number of points in the ring.
            for (var i = 0; i < numPoints; i++)
            {
                // Add the coordinate.
                coords[i] = new Coordinate(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
            }
            return coords;
        }

        private static ILineString CreateWKBLineString(BinaryReader reader, WkbByteOrder byteOrder, IGeometryFactory factory)
        {
            var arrPoint = ReadCoordinates(reader, byteOrder);
            return factory.CreateLineString(arrPoint);
        }

        private static ILinearRing CreateWKBLinearRing(BinaryReader reader, WkbByteOrder byteOrder, IGeometryFactory factory)
        {
            var points = new List<Coordinate>(ReadCoordinates(reader, byteOrder));
            if (!points[0].Equals2D(points[points.Count-1]))
                points.Add(new Coordinate(points[0]));
            return factory.CreateLinearRing(points.ToArray());
        }

        private static IPolygon CreateWKBPolygon(BinaryReader reader, WkbByteOrder byteOrder, IGeometryFactory factory)
        {
            // Get the Number of rings in this Polygon.
            var numRings = (int) ReadUInt32(reader, byteOrder);

            Debug.Assert(numRings >= 1, "Number of rings in polygon must be 1 or more.");

            var shell = CreateWKBLinearRing(reader, byteOrder, factory);
            
            var holes = new ILinearRing[--numRings];
            for (var i = 0; i < numRings; i++)
                holes[i] = CreateWKBLinearRing(reader, byteOrder, factory);

            return factory.CreatePolygon(shell, holes);
        }

        private static IMultiPoint CreateWKBMultiPoint(BinaryReader reader, WkbByteOrder byteOrder, IGeometryFactory factory)
        {
            // Get the number of points in this multipoint.
            var numPoints = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the points.
            var points = new IPoint[numPoints];

            // Loop on the number of points.
            for (var i = 0; i < numPoints; i++)
            {
                // Read point header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // TODO: Validate type

                // Create the next point and add it to the point array.
                points[i] = CreateWKBPoint(reader, byteOrder, factory);
            }
            return factory.CreateMultiPoint(points);
        }

        private static IMultiLineString CreateWKBMultiLineString(BinaryReader reader, WkbByteOrder byteOrder, IGeometryFactory factory)
        {
            // Get the number of linestrings in this multilinestring.
            var numLineStrings = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the linestrings .
            var lines = new ILineString[numLineStrings];

            // Loop on the number of linestrings.
            for (var i = 0; i < numLineStrings; i++)
            {
                // Read linestring header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // Create the next linestring and add it to the array.
                lines[i] = CreateWKBLineString(reader, byteOrder, factory);
            }

            // Create and return the MultiLineString.
            return factory.CreateMultiLineString(lines);
        }

        private static IMultiPolygon CreateWKBMultiPolygon(BinaryReader reader, WkbByteOrder byteOrder, IGeometryFactory factory)
        {
            // Get the number of Polygons.
            var numPolygons = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the Polygons.
            var polygons = new IPolygon[numPolygons];

            // Loop on the number of polygons.
            for (var i = 0; i < numPolygons; i++)
            {
                // read polygon header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // TODO: Validate type

                // Create the next polygon and add it to the array.
                polygons[i] = CreateWKBPolygon(reader, byteOrder, factory);
            }

            //Create and return the MultiPolygon.
            return factory.CreateMultiPolygon(polygons);
        }

        private static IGeometry CreateWKBGeometryCollection(BinaryReader reader, WkbByteOrder byteOrder, IGeometryFactory factory)
        {
            // The next byte in the array tells the number of geometries in this collection.
            var numGeometries = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the geometries.
            var geometries = new IGeometry[numGeometries];

            // Loop on the number of geometries.
            for (var i = 0; i < numGeometries; i++)
            {
                // Call the main create function with the next geometry.
                geometries[i] = Parse(reader, factory);
            }

            // Create and return the next geometry.
            return factory.CreateGeometryCollection(geometries);
        }

        private static uint ReadUInt32(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadUInt32());
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            if (byteOrder == WkbByteOrder.Ndr)
                return reader.ReadUInt32();
            
            throw new ArgumentException("Byte order not recognized");
        }

        private static double ReadDouble(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadDouble());
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }
            if (byteOrder == WkbByteOrder.Ndr)
                return reader.ReadDouble();
            
            throw new ArgumentException("Byte order not recognized");
        }
    }
}