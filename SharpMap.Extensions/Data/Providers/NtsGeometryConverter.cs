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
using NTSCoordinate = GeoAPI.Geometries.ICoordinate;
using NTSGeometry=GeoAPI.Geometries.IGeometry;
using NTSPoint = GeoAPI.Geometries.IPoint;
using NTSLineString = GeoAPI.Geometries.ILineString;
using NTSLinearRing = GeoAPI.Geometries.ILinearRing;
using NTSPolygon = GeoAPI.Geometries.IPolygon;
using NTSMultiPoint = GeoAPI.Geometries.IMultiPoint;
using NTSMultiLineString = GeoAPI.Geometries.IMultiLineString;
using NTSMultiPolygon = GeoAPI.Geometries.IMultiPolygon;
using NTSGeometryCollection = GeoAPI.Geometries.IGeometryCollection;

namespace SharpMap.Converters.NTS
{
    using GeoAPI.Geometries;

    /// <summary>
    /// Provides static methods that performs conversions 
    /// between geometric elements provides by SharpMap and NetTopologySuite libraries.
    /// </summary>
    public static class GeometryConverter
    {
        /// <summary>
        /// Converts any <see cref="SharpMap.Geometries.Geometry"/> array to the correspondant 
        /// <see cref="GisSharpBlog.NetTopologySuite.Geometries.Geometry" /> array.
        /// </summary>
        public static NTSGeometry[] ToNTSGeometry(Geometries.Geometry[] geometries,
            IGeometryFactory factory)
        {
            NTSGeometry[] converted = new NTSGeometry[geometries.Length];
            int index = 0;
            foreach (Geometries.Geometry geometry in geometries)
                converted[index++] = ToNTSGeometry(geometry, factory);
            if ((geometries.Length != converted.Length))
                throw new ApplicationException("Conversion error");
            return converted;
        }

        /// <summary>
        /// Converts any <see cref="GisSharpBlog.NetTopologySuite.Geometries.Geometry"/> to the correspondant 
        /// <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        public static NTSGeometry ToNTSGeometry(Geometries.Geometry geometry, IGeometryFactory factory)
        {
            if (geometry == null)
                throw new NullReferenceException("geometry");

            if (TypeOf(geometry, typeof(Geometries.Point)))
                return ToNTSPoint(geometry as Geometries.Point, factory);

            if (TypeOf(geometry, typeof (Geometries.LineString)))
                return ToNTSLineString(geometry as Geometries.LineString, factory);

            if (TypeOf(geometry, typeof (Geometries.Polygon)))
                return ToNTSPolygon(geometry as Geometries.Polygon, factory);

            if (TypeOf(geometry, typeof (Geometries.MultiPoint)))
                return ToNTSMultiPoint(geometry as Geometries.MultiPoint, factory);

            if (TypeOf(geometry, typeof (Geometries.MultiLineString)))
                return ToNTSMultiLineString(geometry as Geometries.MultiLineString, factory);

            if (TypeOf(geometry, typeof (Geometries.MultiPolygon)))
                return ToNTSMultiPolygon(geometry as Geometries.MultiPolygon, factory);

            if (TypeOf(geometry, typeof (Geometries.GeometryCollection)))
                return ToNTSGeometryCollection(geometry as Geometries.GeometryCollection, factory);

            var message = String.Format("Type {0} not supported", geometry.GetType().FullName);
            throw new NotSupportedException(message);
        }

        private static bool TypeOf(Geometries.Geometry geometry, Type type)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");
            if (type == null)
                throw new ArgumentNullException("type");
            
            return geometry.GetType() == type;
        }

        /// <summary>
        /// Converts any <see cref="GisSharpBlog.NetTopologySuite.Geometries.Geometry"/> array to the correspondant 
        /// <see cref="SharpMap.Geometries.Geometry"/> array.
        /// </summary>
        public static Geometries.Geometry[] ToSharpMapGeometry(NTSGeometry[] geometries)
        {
            Geometries.Geometry[] converted = new Geometries.Geometry[geometries.Length];
            int index = 0;
            foreach (NTSGeometry geometry in geometries)
                converted[index++] = ToSharpMapGeometry(geometry);
            if ((geometries.Length != converted.Length))
                throw new ApplicationException("Conversion error");
            return converted;
        }

        /// <summary>
        /// Converts any <see cref="SharpMap.Geometries.Geometry"/> to the correspondant 
        /// <see cref="GisSharpBlog.NetTopologySuite.Geometries.Geometry"/>.
        /// </summary>
        public static Geometries.Geometry ToSharpMapGeometry(NTSGeometry geometry)
        {
            if (geometry == null)
                throw new NullReferenceException("geometry");

            if (TypeOf(geometry, typeof(NTSPoint)))
                return ToSharpMapPoint(geometry as NTSPoint);

            if (TypeOf(geometry, typeof (NTSLineString)))
                return ToSharpMapLineString(geometry as NTSLineString);

            if (TypeOf(geometry, typeof (NTSPolygon)))
                return ToSharpMapPolygon(geometry as NTSPolygon);

            if (TypeOf(geometry, typeof (NTSMultiPoint)))
                return ToSharpMapMultiPoint(geometry as NTSMultiPoint);

            if (TypeOf(geometry, typeof (NTSMultiLineString)))
                return ToSharpMapMultiLineString(geometry as NTSMultiLineString);

            if (TypeOf(geometry, typeof (NTSMultiPolygon)))
                return ToSharpMapMultiPolygon(geometry as NTSMultiPolygon);

            if (TypeOf(geometry, typeof (NTSGeometryCollection)))
                return ToSharpMapGeometryCollection(geometry as NTSGeometryCollection);

            var message = String.Format("Type {0} not supported", geometry.GetType().FullName);
            throw new NotSupportedException(message);
        }

        private static bool TypeOf(NTSGeometry geometry, Type type)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");
            if (type == null)
                throw new ArgumentNullException("type");

            var interfaces = geometry.GetType().GetInterfaces();
            foreach (Type item in interfaces)
                if (item == type)
                    return true;
            return false;
        }

        /// <summary>
        /// Converts the <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/> instance <paramref name="envelope"/>
        /// into a correspondant <see cref="SharpMap.Geometries.BoundingBox"/>.
        /// </summary>
        public static BoundingBox ToSharpMapBoundingBox(Envelope envelope)
        {
            return new BoundingBox(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY);
        }

        /// <summary>
        /// Converts the <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/> instance <paramref name="envelope"/>
        /// into a correspondant <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        public static Geometries.Geometry ToSharpMapGeometry(Envelope envelope)
        {
            return ToSharpMapGeometry(new BoundingBox(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY));
        }

        /// <summary>
        /// Converts the <see cref="SharpMap.Geometries.BoundingBox"/> instance <paramref name="boundingBox"/>
        /// into a correspondant <see cref="SharpMap.Geometries.Polygon"/>.
        /// </summary>
        public static Geometries.Geometry ToSharpMapGeometry(BoundingBox boundingBox)
        {
            Collection<Geometries.Point> vertices = new Collection<Geometries.Point>();
            vertices.Add(new Geometries.Point(boundingBox.Min.X, boundingBox.Min.Y));
            vertices.Add(new Geometries.Point(boundingBox.Max.X, boundingBox.Min.Y));
            vertices.Add(new Geometries.Point(boundingBox.Max.X, boundingBox.Max.Y));
            vertices.Add(new Geometries.Point(boundingBox.Min.X, boundingBox.Max.Y));
            vertices.Add(new Geometries.Point(boundingBox.Min.X, boundingBox.Min.Y));
            Geometries.LinearRing exterior = new Geometries.LinearRing(vertices);
            return new Geometries.Polygon(exterior);
        }

        /// <summary>
        /// Converts the <see cref="SharpMap.Geometries.BoundingBox"/> instance <paramref name="boundingBox"/>
        /// into a correspondant <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/>.
        /// </summary>
        public static Envelope ToNTSEnvelope(BoundingBox boundingBox)
        {
            return new Envelope(boundingBox.Min.X, boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Y);
        }

        #region Internal NTS Converters     

        internal static NTSCoordinate ToNTSCoordinate(Geometries.Point geom, IGeometryFactory factory)
        {
            return new Coordinate(geom.X, geom.Y);
        }

        internal static NTSPoint ToNTSPoint(Geometries.Point point, IGeometryFactory factory)
        {
            return factory.CreatePoint(ToNTSCoordinate(point, factory));
        }

        internal static NTSLineString ToNTSLineString(Geometries.LineString geom,
            IGeometryFactory factory)
        {
            NTSCoordinate[] coordinates = new NTSCoordinate[geom.NumPoints];
            int index = 0;
            foreach (Geometries.Point point in geom.Vertices)
                coordinates[index++] = ToNTSCoordinate(point, factory);
            return factory.CreateLineString(coordinates) as NTSLineString;
        }

        internal static NTSLinearRing ToNTSLinearRing(Geometries.LinearRing geom,
            IGeometryFactory factory)
        {
            NTSCoordinate[] coordinates = new NTSCoordinate[geom.NumPoints];
            int index = 0;
            foreach (Geometries.Point point in geom.Vertices)
                coordinates[index++] = ToNTSCoordinate(point, factory);
            return factory.CreateLinearRing(coordinates) as NTSLinearRing;
        }

        internal static NTSPolygon ToNTSPolygon(Geometries.Polygon geom,
            IGeometryFactory factory)
        {
            NTSLinearRing shell = ToNTSLinearRing(geom.ExteriorRing, factory);
            NTSLinearRing[] holes = new NTSLinearRing[geom.InteriorRings.Count];
            int index = 0;
            foreach (Geometries.LinearRing hole in geom.InteriorRings)
                holes[index++] = ToNTSLinearRing(hole, factory);
            return factory.CreatePolygon(shell, holes) as NTSPolygon;
        }

        internal static NTSMultiPoint ToNTSMultiPoint(Geometries.MultiPoint geom,
            IGeometryFactory factory)
        {
           NTSPoint[] points = new NTSPoint[geom.Points.Count];
            int index = 0;
            foreach (Geometries.Point point in geom.Points)
                points[index++] = ToNTSPoint(point, factory);
            return factory.CreateMultiPoint(points) as NTSMultiPoint;
        }

        internal static NTSMultiLineString ToNTSMultiLineString(Geometries.MultiLineString geom,
            IGeometryFactory factory)
        {
            NTSLineString[] lstrings = new NTSLineString[geom.LineStrings.Count];
            int index = 0;
            foreach (Geometries.LineString lstring in geom.LineStrings)
                lstrings[index++] = ToNTSLineString(lstring, factory);
            return factory.CreateMultiLineString(lstrings) as NTSMultiLineString;
        }

        internal static NTSMultiPolygon ToNTSMultiPolygon(Geometries.MultiPolygon geom,
            IGeometryFactory factory)
        {
            NTSPolygon[] polygons = new NTSPolygon[geom.Polygons.Count];
            int index = 0;
            foreach (Geometries.Polygon polygon in geom.Polygons)
                polygons[index++] = ToNTSPolygon(polygon, factory);
            return factory.CreateMultiPolygon(polygons) as NTSMultiPolygon;
        }

        internal static NTSGeometryCollection ToNTSGeometryCollection(Geometries.GeometryCollection geom,
            IGeometryFactory factory)
        {
            NTSGeometry[] geometries = new NTSGeometry[geom.Collection.Count];
            int index = 0;
            foreach (Geometries.Geometry geometry in geom.Collection)
                geometries[index++] = ToNTSGeometry(geometry, factory);
            return factory.CreateGeometryCollection(geometries) as NTSGeometryCollection;
        }

        #endregion

        #region Internal SharpMap Converters

        internal static Geometries.Point ToSharpMapPoint(NTSCoordinate coordinate)
        {
            return new Geometries.Point(coordinate.X, coordinate.Y);
        }

        internal static Geometries.Point ToSharpMapPoint(NTSPoint geom)
        {
            Debug.Assert(geom.Coordinates.Length == 1);
            return ToSharpMapPoint((geom.Coordinate as Coordinate));
        }

        internal static Geometries.LineString ToSharpMapLineString(NTSLineString geom)
        {
            Collection<Geometries.Point> vertices = new Collection<Geometries.Point>();
            foreach (Coordinate coordinate in geom.Coordinates)
                vertices.Add(ToSharpMapPoint(coordinate));
            return new Geometries.LineString(vertices);
        }

        internal static Geometries.LinearRing ToSharpMapLinearRing(NTSLinearRing geom)
        {
            Collection<Geometries.Point> vertices = new Collection<Geometries.Point>();
            foreach (Coordinate coordinate in geom.Coordinates)
                vertices.Add(ToSharpMapPoint(coordinate));
            return new Geometries.LinearRing(vertices);
        }

        internal static Geometries.Polygon ToSharpMapPolygon(NTSPolygon geom)
        {
            Geometries.LinearRing exteriorRing = ToSharpMapLinearRing((NTSLinearRing) geom.ExteriorRing);
            Collection<Geometries.LinearRing> interiorRings = new Collection<Geometries.LinearRing>();
            foreach (NTSLineString interiorRing in geom.InteriorRings)
                interiorRings.Add(ToSharpMapLinearRing((NTSLinearRing) interiorRing));
            return new Geometries.Polygon(exteriorRing, interiorRings);
        }

        internal static Geometries.MultiPoint ToSharpMapMultiPoint(NTSMultiPoint geom)
        {
            Geometries.MultiPoint collection = new Geometries.MultiPoint();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.Point point in geom.Geometries)
                collection.Points.Add(ToSharpMapPoint(point));
            return collection;
        }

        internal static Geometries.MultiLineString ToSharpMapMultiLineString(NTSMultiLineString geom)
        {
            Geometries.MultiLineString collection = new Geometries.MultiLineString();
            foreach (NTSLineString lineString in geom.Geometries)
                collection.LineStrings.Add(ToSharpMapLineString(lineString));
            return collection;
        }

        internal static Geometries.MultiPolygon ToSharpMapMultiPolygon(NTSMultiPolygon geom)
        {
            Geometries.MultiPolygon collection = new Geometries.MultiPolygon();
            foreach (NTSPolygon polygon in geom.Geometries)
                collection.Polygons.Add(ToSharpMapPolygon(polygon));
            return collection;
        }

        internal static Geometries.GeometryCollection ToSharpMapGeometryCollection(NTSGeometryCollection geom)
        {
            Geometries.GeometryCollection collection = new Geometries.GeometryCollection();
            foreach (NTSGeometry geometry in geom.Geometries)
                collection.Collection.Add(ToSharpMapGeometry(geometry));
            return collection;
        }

        #endregion
    }
}