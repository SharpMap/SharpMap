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
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Converters.WellKnownText
{
    /// <summary>
    ///  Converts a Well-known Text representation to a <see cref="GeoAPI.Geometries.IGeometry"/> instance.
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
    public class GeometryFromWKT
    {
        /// <summary>
        /// Converts a Well-known text representation to a <see cref="GeoAPI.Geometries.IGeometry"/>.
        /// </summary>
        /// <param name="wellKnownText">A <see cref="GeoAPI.Geometries.IGeometry"/> tagged text string ( see the OpenGIS Simple Features Specification.</param>
        /// <returns>Returns a <see cref="GeoAPI.Geometries.IGeometry"/> specified by wellKnownText.  Throws an exception if there is a parsing problem.</returns>
        public static IGeometry Parse(string wellKnownText)
        {
            // throws a parsing exception is there is a problem.
            using (var reader = new StringReader(wellKnownText))
                return Parse(reader);
        }

        /// <summary>
        /// Converts a Well-known Text representation to a <see cref="GeoAPI.Geometries.IGeometry"/>.
        /// </summary>
        /// <param name="reader">A Reader which will return a Geometry Tagged Text
        /// string (see the OpenGIS Simple Features Specification)</param>
        /// <returns>Returns a <see cref="GeoAPI.Geometries.IGeometry"/> read from StreamReader. 
        /// An exception will be thrown if there is a parsing problem.</returns>
        public static IGeometry Parse(TextReader reader)
        {
            WKTReader wkt = new WKTReader();
            return wkt.Read(reader);
            /*
            var tokenizer = new WktStreamTokenizer(reader);
            return ReadGeometryTaggedText(tokenizer);
            */
        }

        /// <summary>
        /// Returns the next array of Coordinates in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text format.  The
        /// next element returned by the stream should be "(" (the beginning of "(x1 y1, x2 y2, ..., xn yn)" or
        /// "EMPTY".</param>
        /// <returns>The next array of Coordinates in the stream, or an empty array of "EMPTY" is the
        /// next element returned by the stream.</returns>
        private static Coordinate[] GetCoordinates(WktStreamTokenizer tokenizer)
        {
            var coordinates = new List<Coordinate>();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return coordinates.ToArray();

            var externalCoordinate = new Coordinate();
            externalCoordinate.X = GetNextNumber(tokenizer);
            externalCoordinate.Y = GetNextNumber(tokenizer);
            coordinates.Add(externalCoordinate);
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                var internalCoordinate = new Coordinate(GetNextNumber(tokenizer), GetNextNumber(tokenizer));
                coordinates.Add(internalCoordinate);
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return coordinates.ToArray();
        }


        /// <summary>
        /// Returns the next number in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known text format.  The next token
        /// must be a number.</param>
        /// <returns>Returns the next number in the stream.</returns>
        /// <remarks>
        /// ParseException is thrown if the next token is not a number.
        /// </remarks>
        private static double GetNextNumber(WktStreamTokenizer tokenizer)
        {
            tokenizer.NextToken();
            return tokenizer.GetNumericValue();
        }

        /// <summary>
        /// Returns the next "EMPTY" or "(" in the stream as uppercase text.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be "EMPTY" or "(".</param>
        /// <returns>the next "EMPTY" or "(" in the stream as uppercase
        /// text.</returns>
        /// <remarks>
        /// ParseException is thrown if the next token is not "EMPTY" or "(".
        /// </remarks>
        private static string GetNextEmptyOrOpener(WktStreamTokenizer tokenizer)
        {
            tokenizer.NextToken();
            string nextWord = tokenizer.GetStringValue();
            if (nextWord == "EMPTY" || nextWord == "(")
                return nextWord;

            throw new Exception("Expected 'EMPTY' or '(' but encountered '" + nextWord + "'");
        }

        /// <summary>
        /// Returns the next ")" or "," in the stream.
        /// </summary>
        /// <param name="tokenizer">tokenizer over a stream of text in Well-known Text
        /// format. The next token must be ")" or ",".</param>
        /// <returns>Returns the next ")" or "," in the stream.</returns>
        /// <remarks>
        /// ParseException is thrown if the next token is not ")" or ",".
        /// </remarks>
        private static string GetNextCloserOrComma(WktStreamTokenizer tokenizer)
        {
            tokenizer.NextToken();
            string nextWord = tokenizer.GetStringValue();
            if (nextWord == "," || nextWord == ")")
            {
                return nextWord;
            }
            throw new Exception("Expected ')' or ',' but encountered '" + nextWord + "'");
        }

        /// <summary>
        /// Returns the next ")" in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be ")".</param>
        /// <returns>Returns the next ")" in the stream.</returns>
        /// <remarks>
        /// ParseException is thrown if the next token is not ")".
        /// </remarks>
        private static string GetNextCloser(WktStreamTokenizer tokenizer)
        {
            string nextWord = GetNextWord(tokenizer);
            if (nextWord == ")")
                return nextWord;

            throw new Exception("Expected ')' but encountered '" + nextWord + "'");
        }

        /// <summary>
        /// Returns the next word in the stream as uppercase text.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be a word.</param>
        /// <returns>Returns the next word in the stream as uppercase text.</returns>
        /// <remarks>
        /// Exception is thrown if the next token is not a word.
        /// </remarks>
        private static string GetNextWord(WktStreamTokenizer tokenizer)
        {
            TokenType type = tokenizer.NextToken();
            string token = tokenizer.GetStringValue();
            if (type == TokenType.Number)
                throw new Exception("Expected a number but got " + token);
            if (type == TokenType.Word)
                return token.ToUpper();
            if (token == "(")
                return "(";
            if (token == ")")
                return ")";
            if (token == ",")
                return ",";

            throw new Exception("Not a valid symbol in WKT format.");
        }

        /// <summary>
        /// Creates a Geometry using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text
        /// format. The next tokens must form a &lt;Geometry Tagged Text&gt;.</param>
        /// <returns>Returns a Geometry specified by the next token in the stream.</returns>
        /// <remarks>
        /// Exception is thrown if the coordinates used to create a Polygon
        /// shell and holes do not form closed linestrings, or if an unexpected
        /// token is encountered.
        /// </remarks>
        private static IGeometry ReadGeometryTaggedText(WktStreamTokenizer tokenizer)
        {
            tokenizer.NextToken();
            var type = tokenizer.GetStringValue().ToUpper();
            IGeometry geometry;
            IGeometryFactory factory = new GeometryFactory();

            switch (type)
            {
                case "POINT":
                    geometry = ReadPointText(tokenizer, factory);
                    break;
                case "LINESTRING":
                    geometry = ReadLineStringText(tokenizer, factory);
                    break;
                case "MULTIPOINT":
                    geometry = ReadMultiPointText(tokenizer, factory);
                    break;
                case "MULTILINESTRING":
                    geometry = ReadMultiLineStringText(tokenizer, factory);
                    break;
                case "POLYGON":
                    geometry = ReadPolygonText(tokenizer, factory);
                    break;
                case "MULTIPOLYGON":
                    geometry = ReadMultiPolygonText(tokenizer, factory);
                    break;
                case "GEOMETRYCOLLECTION":
                    geometry = ReadGeometryCollectionText(tokenizer, factory);
                    break;
                default:
                    throw new Exception(String.Format(Map.NumberFormatEnUs, "Geometrytype '{0}' is not supported.",
                                                      type));
            }
            return geometry;
        }

        /// <summary>
        /// Creates a <see cref="IMultiPolygon"/> using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">tokenizer over a stream of text in Well-known Text
        /// format. The next tokens must form a MultiPolygon.</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>a <code>MultiPolygon</code> specified by the next token in the 
        /// stream, or if if the coordinates used to create the <see cref="IPolygon"/>
        /// shells and holes do not form closed linestrings.</returns>
        private static IMultiPolygon ReadMultiPolygonText(WktStreamTokenizer tokenizer, IGeometryFactory factory)
        {
            var polygons = new List<IPolygon>();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return factory.CreateMultiPolygon(polygons.ToArray());

            var polygon = ReadPolygonText(tokenizer, factory);
            polygons.Add(polygon);
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                polygon = ReadPolygonText(tokenizer, factory);
                polygons.Add(polygon);
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return factory.CreateMultiPolygon(polygons.ToArray());
        }

        /// <summary>
        /// Creates a Polygon using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text
        ///  format. The next tokens must form a &lt;Polygon Text&gt;.</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>Returns a Polygon specified by the next token
        ///  in the stream</returns>
        ///  <remarks>
        ///  ParseException is thrown if the coordinates used to create the Polygon
        ///  shell and holes do not form closed linestrings, or if an unexpected
        ///  token is encountered.
        ///  </remarks>
        private static IPolygon ReadPolygonText(WktStreamTokenizer tokenizer, IGeometryFactory factory)
        {
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return factory.CreatePolygon(null, null);

            var exteriorRing = factory.CreateLinearRing(GetCoordinates(tokenizer));
            nextToken = GetNextCloserOrComma(tokenizer);
            var interiorRings = new List<ILinearRing>();
            while (nextToken == ",")
            {
                //Add holes
                interiorRings.Add(factory.CreateLinearRing(GetCoordinates(tokenizer)));
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return factory.CreatePolygon(exteriorRing, interiorRings.ToArray());
        }


        /// <summary>
        /// Creates a Point using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text
        /// format. The next tokens must form a &lt;Point Text&gt;.</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>Returns a Point specified by the next token in
        /// the stream.</returns>
        /// <remarks>
        /// ParseException is thrown if an unexpected token is encountered.
        /// </remarks>
        private static IPoint ReadPointText(WktStreamTokenizer tokenizer, IGeometryFactory factory)
        {
            var nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return factory.CreatePoint((Coordinate)null);

            var c = new Coordinate(GetNextNumber(tokenizer), GetNextNumber(tokenizer));
            GetNextCloser(tokenizer);
            
            return factory.CreatePoint(c);
        }

        /// <summary>
        /// Creates a Point using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text
        /// format. The next tokens must form a &lt;Point Text&gt;.</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>Returns a Point specified by the next token in
        /// the stream.</returns>
        /// <remarks>
        /// ParseException is thrown if an unexpected token is encountered.
        /// </remarks>
        private static IMultiPoint ReadMultiPointText(WktStreamTokenizer tokenizer, IGeometryFactory factory)
        {
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return factory.CreateMultiPoint((Coordinate[])null);

            var points = new List<Coordinate>();
            points.Add(new Coordinate(GetNextNumber(tokenizer), GetNextNumber(tokenizer)));
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                points.Add(new Coordinate(GetNextNumber(tokenizer), GetNextNumber(tokenizer)));
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return factory.CreateMultiPoint(points.ToArray());
        }

        /// <summary>
        /// Creates a <see cref="IMultiLineString"/> using the next token in the stream. 
        /// </summary>
        /// <param name="tokenizer">tokenizer over a stream of text in Well-known Text format. The next tokens must form a MultiLineString Text</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>a <see cref="MultiLineString"/> specified by the next token in the stream</returns>
        private static IMultiLineString ReadMultiLineStringText(WktStreamTokenizer tokenizer, IGeometryFactory factory)
        {
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return factory.CreateMultiLineString(null);

            var lineStrings = new List<ILineString>();
            lineStrings.Add(ReadLineStringText(tokenizer, factory));
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                lineStrings.Add(ReadLineStringText(tokenizer, factory));
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return factory.CreateMultiLineString(lineStrings.ToArray());
        }

        /// <summary>
        /// Creates a LineString using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-known Text format.  The next
        ///   tokens must form a LineString Text.</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>Returns a LineString specified by the next token in the stream.</returns>
        /// <remarks>
        /// ParseException is thrown if an unexpected token is encountered.
        /// </remarks>
        private static ILineString ReadLineStringText(WktStreamTokenizer tokenizer, IGeometryFactory factory)
        {
            return factory.CreateLineString(GetCoordinates(tokenizer));
        }

        /// <summary>
        /// Creates a <see cref="GeometryCollection"/> using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer"> Tokenizer over a stream of text in Well-known Text
        /// format. The next tokens must form a GeometryCollection Text.</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>
        /// A <see cref="GeometryCollection"/> specified by the next token in the stream.</returns>
        private static IGeometryCollection ReadGeometryCollectionText(WktStreamTokenizer tokenizer, IGeometryFactory factory)
        {
            var nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken.Equals("EMPTY"))
                return factory.CreateGeometryCollection(null);
            var geometries = new List<IGeometry>();
            geometries.Add(ReadGeometryTaggedText(tokenizer));
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken.Equals(","))
            {
                geometries.Add(ReadGeometryTaggedText(tokenizer));
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return factory.CreateGeometryCollection(geometries.ToArray());
        }
    }
}