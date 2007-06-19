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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

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
        public static GisSharpBlog.NetTopologySuite.Geometries.Geometry[] ToNTSGeometry(SharpMap.Geometries.Geometry[] geometries,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {            
            GisSharpBlog.NetTopologySuite.Geometries.Geometry[] converted = new GisSharpBlog.NetTopologySuite.Geometries.Geometry[geometries.Length];
            int index = 0;
            foreach (SharpMap.Geometries.Geometry geometry in geometries)
                converted[index++] = GeometryConverter.ToNTSGeometry(geometry, factory);
            if((geometries.Length != converted.Length))
                throw new ApplicationException("Conversion error");
            return converted;
        }

        /// <summary>
        /// Converts any <see cref=GisSharpBlog.NetTopologySuite.Geometries.Geometry"/> to the correspondant 
        /// <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GisSharpBlog.NetTopologySuite.Geometries.Geometry ToNTSGeometry(SharpMap.Geometries.Geometry geometry,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            if (geometry == null)
                throw new NullReferenceException("geometry");

            if (geometry.GetType() == typeof(SharpMap.Geometries.Point))
                return ToNTSPoint(geometry as SharpMap.Geometries.Point, factory);

            else if (geometry.GetType() == typeof(SharpMap.Geometries.LineString))
                return ToNTSLineString(geometry as SharpMap.Geometries.LineString, factory);

            else if (geometry.GetType() == typeof(SharpMap.Geometries.Polygon))
                return ToNTSPolygon(geometry as SharpMap.Geometries.Polygon, factory);

            else if (geometry.GetType() == typeof(SharpMap.Geometries.MultiPoint))
                return ToNTSMultiPoint(geometry as SharpMap.Geometries.MultiPoint, factory);

            else if (geometry.GetType() == typeof(SharpMap.Geometries.MultiLineString))
                return ToNTSMultiLineString(geometry as SharpMap.Geometries.MultiLineString, factory);

            else if (geometry.GetType() == typeof(SharpMap.Geometries.MultiPolygon))
                return ToNTSMultiPolygon(geometry as SharpMap.Geometries.MultiPolygon, factory);

            else if (geometry.GetType() == typeof(SharpMap.Geometries.GeometryCollection))
                return ToNTSGeometryCollection(geometry as SharpMap.Geometries.GeometryCollection, factory);

            else throw new NotSupportedException("Type " + geometry.GetType().FullName + " not supported");
        }

        /// <summary>
        /// Converts any <see cref=GisSharpBlog.NetTopologySuite.Geometries.Geometry"/> array to the correspondant 
        /// <see cref="SharpMap.Geometries.Geometry"/> array.
        /// </summary>
        /// <param name="geometries"></param>
        /// <returns></returns>
        public static SharpMap.Geometries.Geometry[] ToSharpMapGeometry(GisSharpBlog.NetTopologySuite.Geometries.Geometry[] geometries)
        {
            SharpMap.Geometries.Geometry[] converted = new SharpMap.Geometries.Geometry[geometries.Length];
            int index = 0;
            foreach (GisSharpBlog.NetTopologySuite.Geometries.Geometry geometry in geometries)
                converted[index++] = GeometryConverter.ToSharpMapGeometry(geometry);
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
        public static SharpMap.Geometries.Geometry ToSharpMapGeometry(GisSharpBlog.NetTopologySuite.Geometries.Geometry geometry)
        {
            if (geometry == null)
                throw new NullReferenceException("geometry");

            if(geometry.GetType() ==  typeof(GisSharpBlog.NetTopologySuite.Geometries.Point))
                return ToSharpMapPoint(geometry as GisSharpBlog.NetTopologySuite.Geometries.Point);

            else if(geometry.GetType() ==  typeof(GisSharpBlog.NetTopologySuite.Geometries.LineString))
                return ToSharpMapLineString(geometry as GisSharpBlog.NetTopologySuite.Geometries.LineString);

            else if (geometry.GetType() == typeof(GisSharpBlog.NetTopologySuite.Geometries.Polygon))
                return ToSharpMapPolygon(geometry as GisSharpBlog.NetTopologySuite.Geometries.Polygon);

            else if (geometry.GetType() == typeof(GisSharpBlog.NetTopologySuite.Geometries.MultiPoint))
                return ToSharpMapMultiPoint(geometry as GisSharpBlog.NetTopologySuite.Geometries.MultiPoint);

            else if (geometry.GetType() == typeof(GisSharpBlog.NetTopologySuite.Geometries.MultiLineString))
                return ToSharpMapMultiLineString(geometry as GisSharpBlog.NetTopologySuite.Geometries.MultiLineString);

            else if (geometry.GetType() == typeof(GisSharpBlog.NetTopologySuite.Geometries.MultiPolygon))
                return ToSharpMapMultiPolygon(geometry as GisSharpBlog.NetTopologySuite.Geometries.MultiPolygon);

            else if (geometry.GetType() == typeof(GisSharpBlog.NetTopologySuite.Geometries.GeometryCollection))
                return ToSharpMapGeometryCollection(geometry as GisSharpBlog.NetTopologySuite.Geometries.GeometryCollection);
            
            else throw new NotSupportedException("Type " + geometry.GetType().FullName + " not supported");           
        }

        /// <summary>
        /// Converts the <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/> instance <paramref name="envelope"/>
        /// into a correspondant <see cref="SharpMap.Geometries.BoundingBox"/>.
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static SharpMap.Geometries.BoundingBox ToSharpMapBoundingBox(GisSharpBlog.NetTopologySuite.Geometries.Envelope envelope)
        {
            return new SharpMap.Geometries.BoundingBox(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY);           
        }

        /// <summary>
        /// Converts the <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/> instance <paramref name="envelope"/>
        /// into a correspondant <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static SharpMap.Geometries.Geometry ToSharpMapGeometry(GisSharpBlog.NetTopologySuite.Geometries.Envelope envelope)
        {
            return ToSharpMapGeometry(new SharpMap.Geometries.BoundingBox(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY));
        }

        /// <summary>
        /// Converts the <see cref="SharpMap.Geometries.BoundingBox"/> instance <paramref name="boundingBox"/>
        /// into a correspondant <see cref="SharpMap.Geometries.Polygon"/>.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static SharpMap.Geometries.Geometry ToSharpMapGeometry(SharpMap.Geometries.BoundingBox boundingBox)
        {
            Collection<SharpMap.Geometries.Point> vertices = new Collection<SharpMap.Geometries.Point>();
            vertices.Add(new SharpMap.Geometries.Point(boundingBox.Min.X, boundingBox.Min.Y));
            vertices.Add(new SharpMap.Geometries.Point(boundingBox.Max.X, boundingBox.Min.Y));
            vertices.Add(new SharpMap.Geometries.Point(boundingBox.Max.X, boundingBox.Max.Y));
            vertices.Add(new SharpMap.Geometries.Point(boundingBox.Min.X, boundingBox.Max.Y));
            vertices.Add(new SharpMap.Geometries.Point(boundingBox.Min.X, boundingBox.Min.Y));
            SharpMap.Geometries.LinearRing exterior = new SharpMap.Geometries.LinearRing(vertices);
            return new SharpMap.Geometries.Polygon(exterior);
        }

        /// <summary>
        /// Converts the <see cref="SharpMap.Geometries.BoundingBox"/> instance <paramref name="boundingBox"/>
        /// into a correspondant <see cref="GisSharpBlog.NetTopologySuite.Geometries.Envelope"/>.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static GisSharpBlog.NetTopologySuite.Geometries.Envelope ToNTSEnvelope(SharpMap.Geometries.BoundingBox boundingBox)
        {            
            return new GisSharpBlog.NetTopologySuite.Geometries.Envelope(boundingBox.Min.X, boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Y);
        }

        #region Internal NTS Converters     
   
        internal static GisSharpBlog.NetTopologySuite.Geometries.Coordinate ToNTSCoordinate(SharpMap.Geometries.Point point, 
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {            
            return new GisSharpBlog.NetTopologySuite.Geometries.Coordinate(point.X, point.Y);               
        }
        
        internal static GisSharpBlog.NetTopologySuite.Geometries.Point ToNTSPoint(SharpMap.Geometries.Point point,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            return factory.CreatePoint(ToNTSCoordinate(point, factory)) as GisSharpBlog.NetTopologySuite.Geometries.Point;            
        }

        internal static GisSharpBlog.NetTopologySuite.Geometries.LineString ToNTSLineString(SharpMap.Geometries.LineString lineString,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            GisSharpBlog.NetTopologySuite.Geometries.Coordinate[] coordinates = new GisSharpBlog.NetTopologySuite.Geometries.Coordinate[lineString.NumPoints];
            int index = 0;
            foreach (SharpMap.Geometries.Point point in lineString.Vertices)
                coordinates[index++] = ToNTSCoordinate(point, factory);
            return factory.CreateLineString(coordinates) as GisSharpBlog.NetTopologySuite.Geometries.LineString;
        }

        internal static GisSharpBlog.NetTopologySuite.Geometries.LinearRing ToNTSLinearRing(SharpMap.Geometries.LinearRing linearRing,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            GisSharpBlog.NetTopologySuite.Geometries.Coordinate[] coordinates = new GisSharpBlog.NetTopologySuite.Geometries.Coordinate[linearRing.NumPoints];
            int index = 0;
            foreach (SharpMap.Geometries.Point point in linearRing.Vertices)
                coordinates[index++] = ToNTSCoordinate(point, factory);
            return factory.CreateLinearRing(coordinates) as GisSharpBlog.NetTopologySuite.Geometries.LinearRing;
        }

        internal static GisSharpBlog.NetTopologySuite.Geometries.Polygon ToNTSPolygon(SharpMap.Geometries.Polygon polygon,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            GisSharpBlog.NetTopologySuite.Geometries.LinearRing shell = ToNTSLinearRing(polygon.ExteriorRing, factory);
            GisSharpBlog.NetTopologySuite.Geometries.LinearRing[] holes = new GisSharpBlog.NetTopologySuite.Geometries.LinearRing[polygon.InteriorRings.Count];
            int index = 0;
            foreach (SharpMap.Geometries.LinearRing hole in polygon.InteriorRings)
                holes[index++] = ToNTSLinearRing(hole, factory);
            return factory.CreatePolygon(shell, holes) as GisSharpBlog.NetTopologySuite.Geometries.Polygon;
        }

        internal static GisSharpBlog.NetTopologySuite.Geometries.MultiPoint ToNTSMultiPoint(SharpMap.Geometries.MultiPoint multiPoint,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            GisSharpBlog.NetTopologySuite.Geometries.Point[] points = new GisSharpBlog.NetTopologySuite.Geometries.Point[multiPoint.Points.Count];
            int index = 0;
            foreach (SharpMap.Geometries.Point point in multiPoint.Points)
                points[index++] = ToNTSPoint(point, factory);
            return factory.CreateMultiPoint(points) as GisSharpBlog.NetTopologySuite.Geometries.MultiPoint;
        }

        internal static GisSharpBlog.NetTopologySuite.Geometries.MultiLineString ToNTSMultiLineString(SharpMap.Geometries.MultiLineString multiLineString,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            GisSharpBlog.NetTopologySuite.Geometries.LineString[] lstrings = new GisSharpBlog.NetTopologySuite.Geometries.LineString[multiLineString.LineStrings.Count];
            int index = 0;
            foreach (SharpMap.Geometries.LineString lstring in multiLineString.LineStrings)
                lstrings[index++] = ToNTSLineString(lstring, factory);
            return factory.CreateMultiLineString(lstrings) as GisSharpBlog.NetTopologySuite.Geometries.MultiLineString;
        }

        internal static GisSharpBlog.NetTopologySuite.Geometries.MultiPolygon ToNTSMultiPolygon(SharpMap.Geometries.MultiPolygon multiPolygon,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            GisSharpBlog.NetTopologySuite.Geometries.Polygon[] polygons = new GisSharpBlog.NetTopologySuite.Geometries.Polygon[multiPolygon.Polygons.Count];
            int index = 0;
            foreach (SharpMap.Geometries.Polygon polygon in multiPolygon.Polygons)
                polygons[index++] = ToNTSPolygon(polygon, factory);
            return factory.CreateMultiPolygon(polygons) as GisSharpBlog.NetTopologySuite.Geometries.MultiPolygon;
        }

        internal static GisSharpBlog.NetTopologySuite.Geometries.GeometryCollection ToNTSGeometryCollection(SharpMap.Geometries.GeometryCollection geometryCollection,
            GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory factory)
        {
            GisSharpBlog.NetTopologySuite.Geometries.Geometry[] geometries = new GisSharpBlog.NetTopologySuite.Geometries.Geometry[geometryCollection.Collection.Count];
            int index = 0;
            foreach (SharpMap.Geometries.Geometry geometry in geometryCollection.Collection)
                geometries[index++] = ToNTSGeometry(geometry, factory);
            return factory.CreateGeometryCollection(geometries) as GisSharpBlog.NetTopologySuite.Geometries.GeometryCollection;
        }

        #endregion

        #region Internal SharpMap Converters

        internal static SharpMap.Geometries.Point ToSharpMapPoint(GisSharpBlog.NetTopologySuite.Geometries.Coordinate coordinate)
        {            
            return new SharpMap.Geometries.Point(coordinate.X, coordinate.Y);
        }

        internal static SharpMap.Geometries.Point ToSharpMapPoint(GisSharpBlog.NetTopologySuite.Geometries.Point point)
        {            
            Debug.Assert(point.Coordinates.Length == 1);
            return ToSharpMapPoint((point.Coordinate  as GisSharpBlog.NetTopologySuite.Geometries.Coordinate));
        }

        internal static SharpMap.Geometries.LineString ToSharpMapLineString(GisSharpBlog.NetTopologySuite.Geometries.LineString lineString)
        {
            Collection<SharpMap.Geometries.Point> vertices = new Collection<SharpMap.Geometries.Point>();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.Coordinate coordinate in lineString.Coordinates)
                vertices.Add(ToSharpMapPoint(coordinate));            
            return new SharpMap.Geometries.LineString(vertices);
        }

        internal static SharpMap.Geometries.LinearRing ToSharpMapLinearRing(GisSharpBlog.NetTopologySuite.Geometries.LinearRing lineString)
        {
            Collection<SharpMap.Geometries.Point> vertices = new Collection<SharpMap.Geometries.Point>();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.Coordinate coordinate in lineString.Coordinates)
                vertices.Add(ToSharpMapPoint(coordinate));
            return new SharpMap.Geometries.LinearRing(vertices);
        }

        internal static SharpMap.Geometries.Polygon ToSharpMapPolygon(GisSharpBlog.NetTopologySuite.Geometries.Polygon polygon)
        {
            SharpMap.Geometries.LinearRing exteriorRing = ToSharpMapLinearRing((GisSharpBlog.NetTopologySuite.Geometries.LinearRing)polygon.ExteriorRing);
            Collection<SharpMap.Geometries.LinearRing> interiorRings = new Collection<SharpMap.Geometries.LinearRing>();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.LineString interiorRing in polygon.InteriorRings)
                interiorRings.Add(ToSharpMapLinearRing((GisSharpBlog.NetTopologySuite.Geometries.LinearRing)interiorRing));
            return new SharpMap.Geometries.Polygon(exteriorRing, interiorRings);
        }

        internal static SharpMap.Geometries.MultiPoint ToSharpMapMultiPoint(GisSharpBlog.NetTopologySuite.Geometries.MultiPoint multiPoint)
        {
            SharpMap.Geometries.MultiPoint collection = new SharpMap.Geometries.MultiPoint();
            foreach(GisSharpBlog.NetTopologySuite.Geometries.Point point in multiPoint.Geometries)
                collection.Points.Add(ToSharpMapPoint(point));
            return collection;
        }

        internal static SharpMap.Geometries.MultiLineString ToSharpMapMultiLineString(GisSharpBlog.NetTopologySuite.Geometries.MultiLineString multiLineString)
        {
            SharpMap.Geometries.MultiLineString collection = new SharpMap.Geometries.MultiLineString();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.LineString lineString in multiLineString.Geometries)
                collection.LineStrings.Add(ToSharpMapLineString(lineString));
            return collection;
        }

        internal static SharpMap.Geometries.MultiPolygon ToSharpMapMultiPolygon(GisSharpBlog.NetTopologySuite.Geometries.MultiPolygon multiPolygon)
        {
            SharpMap.Geometries.MultiPolygon collection = new SharpMap.Geometries.MultiPolygon();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.Polygon polygon in multiPolygon.Geometries)
                collection.Polygons.Add(ToSharpMapPolygon(polygon));
            return collection;
        }

        internal static SharpMap.Geometries.GeometryCollection ToSharpMapGeometryCollection(GisSharpBlog.NetTopologySuite.Geometries.GeometryCollection geometryCollection)
        {
            SharpMap.Geometries.GeometryCollection collection = new SharpMap.Geometries.GeometryCollection();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.Geometry geometry in geometryCollection.Geometries)
                collection.Collection.Add(ToSharpMapGeometry(geometry));
            return collection;
        }

        #endregion

    }
}
