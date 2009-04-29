// Copyright 2006 - Diego Guidi
//
// This file is part of NtsProvider.
// NtsProvider is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NtsProvider; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GisSharpBlog.NetTopologySuite.Geometries;
using SharpMap.Geometries;
using Geometry=GisSharpBlog.NetTopologySuite.Geometries.Geometry;
using GeometryCollection=GisSharpBlog.NetTopologySuite.Geometries.GeometryCollection;
using LinearRing=GisSharpBlog.NetTopologySuite.Geometries.LinearRing;
using LineString=GisSharpBlog.NetTopologySuite.Geometries.LineString;
using MultiLineString=GisSharpBlog.NetTopologySuite.Geometries.MultiLineString;
using MultiPoint=GisSharpBlog.NetTopologySuite.Geometries.MultiPoint;
using MultiPolygon=GisSharpBlog.NetTopologySuite.Geometries.MultiPolygon;
using Point=SharpMap.Geometries.Point;
using Polygon=GisSharpBlog.NetTopologySuite.Geometries.Polygon;

namespace SharpMap.Converters.NTS
{
    /// <summary>
    /// Provides static methods that performs conversions 
    /// between geometric elements provides by SharpMap and NetTopologySuite libraries.
    /// </summary>
    public static class GeometryConverter
    {
        /// <summary>
        /// Converts any <see cref="SharpMap.Geometries.Geometry"/> array to the correspondant 
        /// <see cref=GisSharpBlog.NetTopologySuite.Geometries.Geometry"/> array.
        /// </summary>
        /// <param name="geometries"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static Geometry[] ToNTSGeometry(Geometries.Geometry[] geometries,
                                               GeometryFactory factory)
        {
            Geometry[] converted = new Geometry[geometries.Length];
            int index = 0;
            foreach (Geometries.Geometry geometry in geometries)
                converted[index++] = ToNTSGeometry(geometry, factory);
            if ((geometries.Length != converted.Length))
                throw new ApplicationException("Conversion error");
            return converted;
        }

        /// <summary>
        /// Converts any <see cref=GisSharpBlog.NetTopologySuite.Geometries.Geometry"/> to the correspondant 
        /// <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static Geometry ToNTSGeometry(Geometries.Geometry geometry,
                                             GeometryFactory factory)
        {
            if (geometry == null)
                throw new NullReferenceException("geometry");

            if (geometry.GetType() == typeof (Point))
                return ToNTSPoint(geometry as Point, factory);

            else if (geometry.GetType() == typeof (Geometries.LineString))
                return ToNTSLineString(geometry as Geometries.LineString, factory);

            else if (geometry.GetType() == typeof (Geometries.Polygon))
                return ToNTSPolygon(geometry as Geometries.Polygon, factory);

            else if (geometry.GetType() == typeof (Geometries.MultiPoint))
                return ToNTSMultiPoint(geometry as Geometries.MultiPoint, factory);

            else if (geometry.GetType() == typeof (Geometries.MultiLineString))
                return ToNTSMultiLineString(geometry as Geometries.MultiLineString, factory);

            else if (geometry.GetType() == typeof (Geometries.MultiPolygon))
                return ToNTSMultiPolygon(geometry as Geometries.MultiPolygon, factory);

            else if (geometry.GetType() == typeof (Geometries.GeometryCollection))
                return ToNTSGeometryCollection(geometry as Geometries.GeometryCollection, factory);

            else throw new NotSupportedException("Type " + geometry.GetType().FullName + " not supported");
        }

        /// <summary>
        /// Converts any <see cref=GisSharpBlog.NetTopologySuite.Geometries.Geometry"/> array to the correspondant 
        /// <see cref="SharpMap.Geometries.Geometry"/> array.
        /// </summary>
        /// <param name="geometries"></param>
        /// <returns></returns>
        public static Geometries.Geometry[] ToSharpMapGeometry(Geometry[] geometries)
        {
            Geometries.Geometry[] converted = new Geometries.Geometry[geometries.Length];
            int index = 0;
            foreach (Geometry geometry in geometries)
                converted[index++] = ToSharpMapGeometry(geometry);
            if ((geometries.Length != converted.Length))
                throw new ApplicationException("Conversion error");
            return converted;
        }

        /// <summary>
        /// Converts any <see cref="SharpMap.Geometries.Geometry"/> to the correspondant 
        /// <see cref=GisSharpBlog.NetTopologySuite.Geometries.Geometry"/>.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static Geometries.Geometry ToSharpMapGeometry(Geometry geometry)
        {
            if (geometry == null)
                throw new NullReferenceException("geometry");

            if (geometry.GetType() == typeof (GisSharpBlog.NetTopologySuite.Geometries.Point))
                return ToSharpMapPoint(geometry as GisSharpBlog.NetTopologySuite.Geometries.Point);

            else if (geometry.GetType() == typeof (LineString))
                return ToSharpMapLineString(geometry as LineString);

            else if (geometry.GetType() == typeof (Polygon))
                return ToSharpMapPolygon(geometry as Polygon);

            else if (geometry.GetType() == typeof (MultiPoint))
                return ToSharpMapMultiPoint(geometry as MultiPoint);

            else if (geometry.GetType() == typeof (MultiLineString))
                return ToSharpMapMultiLineString(geometry as MultiLineString);

            else if (geometry.GetType() == typeof (MultiPolygon))
                return ToSharpMapMultiPolygon(geometry as MultiPolygon);

            else if (geometry.GetType() == typeof (GeometryCollection))
                return ToSharpMapGeometryCollection(geometry as GeometryCollection);

            else throw new NotSupportedException("Type " + geometry.GetType().FullName + " not supported");
        }

        /// <summary>
        /// Converts the <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/> instance <paramref name="envelope"/>
        /// into a correspondant <see cref="SharpMap.Geometries.BoundingBox"/>.
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static BoundingBox ToSharpMapBoundingBox(Envelope envelope)
        {
            return new BoundingBox(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY);
        }

        /// <summary>
        /// Converts the <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/> instance <paramref name="envelope"/>
        /// into a correspondant <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static Geometries.Geometry ToSharpMapGeometry(Envelope envelope)
        {
            return ToSharpMapGeometry(new BoundingBox(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY));
        }

        /// <summary>
        /// Converts the <see cref="SharpMap.Geometries.BoundingBox"/> instance <paramref name="boundingBox"/>
        /// into a correspondant <see cref="SharpMap.Geometries.Polygon"/>.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static Geometries.Geometry ToSharpMapGeometry(BoundingBox boundingBox)
        {
            Collection<Point> vertices = new Collection<Point>();
            vertices.Add(new Point(boundingBox.Min.X, boundingBox.Min.Y));
            vertices.Add(new Point(boundingBox.Max.X, boundingBox.Min.Y));
            vertices.Add(new Point(boundingBox.Max.X, boundingBox.Max.Y));
            vertices.Add(new Point(boundingBox.Min.X, boundingBox.Max.Y));
            vertices.Add(new Point(boundingBox.Min.X, boundingBox.Min.Y));
            Geometries.LinearRing exterior = new Geometries.LinearRing(vertices);
            return new Geometries.Polygon(exterior);
        }

        /// <summary>
        /// Converts the <see cref="SharpMap.Geometries.BoundingBox"/> instance <paramref name="boundingBox"/>
        /// into a correspondant <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/>.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static Envelope ToNTSEnvelope(BoundingBox boundingBox)
        {
            return new Envelope(boundingBox.Min.X, boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Y);
        }

        #region Internal NTS Converters     

        internal static Coordinate ToNTSCoordinate(Point point,
                                                   GeometryFactory factory)
        {
            return new Coordinate(point.X, point.Y);
        }

        internal static GisSharpBlog.NetTopologySuite.Geometries.Point ToNTSPoint(Point point,
                                                                                  GeometryFactory factory)
        {
            return
                factory.CreatePoint(ToNTSCoordinate(point, factory)) as GisSharpBlog.NetTopologySuite.Geometries.Point;
        }

        internal static LineString ToNTSLineString(Geometries.LineString lineString,
                                                   GeometryFactory factory)
        {
            Coordinate[] coordinates = new Coordinate[lineString.NumPoints];
            int index = 0;
            foreach (Point point in lineString.Vertices)
                coordinates[index++] = ToNTSCoordinate(point, factory);
            return factory.CreateLineString(coordinates) as LineString;
        }

        internal static LinearRing ToNTSLinearRing(Geometries.LinearRing linearRing,
                                                   GeometryFactory factory)
        {
            Coordinate[] coordinates = new Coordinate[linearRing.NumPoints];
            int index = 0;
            foreach (Point point in linearRing.Vertices)
                coordinates[index++] = ToNTSCoordinate(point, factory);
            return factory.CreateLinearRing(coordinates) as LinearRing;
        }

        internal static Polygon ToNTSPolygon(Geometries.Polygon polygon,
                                             GeometryFactory factory)
        {
            LinearRing shell = ToNTSLinearRing(polygon.ExteriorRing, factory);
            LinearRing[] holes = new LinearRing[polygon.InteriorRings.Count];
            int index = 0;
            foreach (Geometries.LinearRing hole in polygon.InteriorRings)
                holes[index++] = ToNTSLinearRing(hole, factory);
            return factory.CreatePolygon(shell, holes) as Polygon;
        }

        internal static MultiPoint ToNTSMultiPoint(Geometries.MultiPoint multiPoint,
                                                   GeometryFactory factory)
        {
            GisSharpBlog.NetTopologySuite.Geometries.Point[] points =
                new GisSharpBlog.NetTopologySuite.Geometries.Point[multiPoint.Points.Count];
            int index = 0;
            foreach (Point point in multiPoint.Points)
                points[index++] = ToNTSPoint(point, factory);
            return factory.CreateMultiPoint(points) as MultiPoint;
        }

        internal static MultiLineString ToNTSMultiLineString(Geometries.MultiLineString multiLineString,
                                                             GeometryFactory factory)
        {
            LineString[] lstrings = new LineString[multiLineString.LineStrings.Count];
            int index = 0;
            foreach (Geometries.LineString lstring in multiLineString.LineStrings)
                lstrings[index++] = ToNTSLineString(lstring, factory);
            return factory.CreateMultiLineString(lstrings) as MultiLineString;
        }

        internal static MultiPolygon ToNTSMultiPolygon(Geometries.MultiPolygon multiPolygon,
                                                       GeometryFactory factory)
        {
            Polygon[] polygons = new Polygon[multiPolygon.Polygons.Count];
            int index = 0;
            foreach (Geometries.Polygon polygon in multiPolygon.Polygons)
                polygons[index++] = ToNTSPolygon(polygon, factory);
            return factory.CreateMultiPolygon(polygons) as MultiPolygon;
        }

        internal static GeometryCollection ToNTSGeometryCollection(Geometries.GeometryCollection geometryCollection,
                                                                   GeometryFactory factory)
        {
            Geometry[] geometries = new Geometry[geometryCollection.Collection.Count];
            int index = 0;
            foreach (Geometries.Geometry geometry in geometryCollection.Collection)
                geometries[index++] = ToNTSGeometry(geometry, factory);
            return factory.CreateGeometryCollection(geometries) as GeometryCollection;
        }

        #endregion

        #region Internal SharpMap Converters

        internal static Point ToSharpMapPoint(Coordinate coordinate)
        {
            return new Point(coordinate.X, coordinate.Y);
        }

        internal static Point ToSharpMapPoint(GisSharpBlog.NetTopologySuite.Geometries.Point point)
        {
            Debug.Assert(point.Coordinates.Length == 1);
            return ToSharpMapPoint((point.Coordinate as Coordinate));
        }

        internal static Geometries.LineString ToSharpMapLineString(LineString lineString)
        {
            Collection<Point> vertices = new Collection<Point>();
            foreach (Coordinate coordinate in lineString.Coordinates)
                vertices.Add(ToSharpMapPoint(coordinate));
            return new Geometries.LineString(vertices);
        }

        internal static Geometries.LinearRing ToSharpMapLinearRing(LinearRing lineString)
        {
            Collection<Point> vertices = new Collection<Point>();
            foreach (Coordinate coordinate in lineString.Coordinates)
                vertices.Add(ToSharpMapPoint(coordinate));
            return new Geometries.LinearRing(vertices);
        }

        internal static Geometries.Polygon ToSharpMapPolygon(Polygon polygon)
        {
            Geometries.LinearRing exteriorRing = ToSharpMapLinearRing((LinearRing) polygon.ExteriorRing);
            Collection<Geometries.LinearRing> interiorRings = new Collection<Geometries.LinearRing>();
            foreach (LineString interiorRing in polygon.InteriorRings)
                interiorRings.Add(ToSharpMapLinearRing((LinearRing) interiorRing));
            return new Geometries.Polygon(exteriorRing, interiorRings);
        }

        internal static Geometries.MultiPoint ToSharpMapMultiPoint(MultiPoint multiPoint)
        {
            Geometries.MultiPoint collection = new Geometries.MultiPoint();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.Point point in multiPoint.Geometries)
                collection.Points.Add(ToSharpMapPoint(point));
            return collection;
        }

        internal static Geometries.MultiLineString ToSharpMapMultiLineString(MultiLineString multiLineString)
        {
            Geometries.MultiLineString collection = new Geometries.MultiLineString();
            foreach (LineString lineString in multiLineString.Geometries)
                collection.LineStrings.Add(ToSharpMapLineString(lineString));
            return collection;
        }

        internal static Geometries.MultiPolygon ToSharpMapMultiPolygon(MultiPolygon multiPolygon)
        {
            Geometries.MultiPolygon collection = new Geometries.MultiPolygon();
            foreach (Polygon polygon in multiPolygon.Geometries)
                collection.Polygons.Add(ToSharpMapPolygon(polygon));
            return collection;
        }

        internal static Geometries.GeometryCollection ToSharpMapGeometryCollection(GeometryCollection geometryCollection)
        {
            Geometries.GeometryCollection collection = new Geometries.GeometryCollection();
            foreach (Geometry geometry in geometryCollection.Geometries)
                collection.Collection.Add(ToSharpMapGeometry(geometry));
            return collection;
        }

        #endregion
    }
}