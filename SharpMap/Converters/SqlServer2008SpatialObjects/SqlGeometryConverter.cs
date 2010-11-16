using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using SMGeometry = SharpMap.Geometries.Geometry;
using SMGeometryType = SharpMap.Geometries.GeometryType2;
using SMPoint = SharpMap.Geometries.Point;
using SMLineString = SharpMap.Geometries.LineString;
using SMLinearRing = SharpMap.Geometries.LinearRing;
using SMPolygon = SharpMap.Geometries.Polygon;
using SMMultiPoint = SharpMap.Geometries.MultiPoint;
using SMMultiLineString = SharpMap.Geometries.MultiLineString;
using SMMultiPolygon = SharpMap.Geometries.MultiPolygon;
using SMGeometryCollection = SharpMap.Geometries.GeometryCollection;


namespace SharpMap.Converters.SqlServer2008SpatialObjects
{
    public static class SqlGeometryConverter
    {
        public static SqlGeometry ToSqlServerGeometry(SMGeometry smGeometry)
        {
            SqlGeometryBuilder builder = new SqlGeometryBuilder();
            SharpMapGeometryToSqlGeometry(builder, smGeometry);
            return builder.ConstructedGeometry;
        }

        private static void SharpMapGeometryToSqlGeometry(SqlGeometryBuilder geomBuilder, SMGeometry smGeometry)
        {
            
            switch (smGeometry.GeometryType)
            {
                case SMGeometryType.Point:
                    SharpMapPointToSqlGeometry(geomBuilder, smGeometry as SMPoint);
                    break;
                case SMGeometryType.LineString:
                    SharpMapLineStringToSqlGeometry(geomBuilder, smGeometry as SMLineString);
                    break;
                case SMGeometryType.Polygon:
                    SharpMapPolygonToSqlGeometry(geomBuilder, smGeometry as SMPolygon);
                    break;
                case SMGeometryType.MultiPoint:
                    SharpMapMultiPointToSqlGeometry(geomBuilder, smGeometry as SMMultiPoint);
                    break;
                case SMGeometryType.MultiLineString:
                    SharpMapMultiLineStringToSqlGeometry(geomBuilder, smGeometry as SMMultiLineString);
                    break;
                case SMGeometryType.MultiPolygon:
                    SharpMapMultiPolygonToSqlGeometry(geomBuilder, smGeometry as SMMultiPolygon);
                    break;
                case SMGeometryType.GeometryCollection:
                    SharpMapGeometryCollectionToSqlGeometry(geomBuilder, smGeometry as SMGeometryCollection);
                    break;
                default:
                    throw new ArgumentException(
                        String.Format("Cannot convert '{0}' geometry type", smGeometry.GeometryType), "smGeometry");
            }
        }

        private static void SharpMapGeometryCollectionToSqlGeometry(SqlGeometryBuilder geomBuilder, SMGeometryCollection geometryCollection)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.GeometryCollection);
            for (int i = 0; i < geometryCollection.NumGeometries; i++ )
                SharpMapGeometryToSqlGeometry(geomBuilder, geometryCollection.Geometry(i));
            geomBuilder.EndGeometry();
        }

        private static void SharpMapMultiPolygonToSqlGeometry(SqlGeometryBuilder geomBuilder, SMMultiPolygon multiPolygon)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.MultiPolygon);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
                SharpMapPolygonToSqlGeometry(geomBuilder, multiPolygon.Geometry(i) as SMPolygon);
            geomBuilder.EndGeometry();
        }

        private static void SharpMapMultiLineStringToSqlGeometry(SqlGeometryBuilder geomBuilder, SMMultiLineString multiLineString)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
                SharpMapLineStringToSqlGeometry(geomBuilder, multiLineString.Geometry(i) as SMLineString);
            geomBuilder.EndGeometry();
        }

        private static void SharpMapMultiPointToSqlGeometry(SqlGeometryBuilder geomBuilder, SMMultiPoint multiPoint)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.MultiPoint);
            for(int i = 0; i <= multiPoint.NumGeometries; i++)
                SharpMapPointToSqlGeometry(geomBuilder, multiPoint.Geometry(i));
            geomBuilder.EndGeometry();
        }

        private static void SharpMapPointToSqlGeometry(SqlGeometryBuilder geomBuilder, SMPoint point)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.Point);
            geomBuilder.BeginFigure(point.X, point.Y);
            geomBuilder.EndFigure();
            geomBuilder.EndGeometry();
        }

        private static void SharpMapLineStringToSqlGeometry(SqlGeometryBuilder geomBuilder, SMLineString lineString)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.LineString);
            SMPoint point = lineString.StartPoint;
            geomBuilder.BeginFigure(point.X, point.Y);
            for (int i = 1; i < lineString.NumPoints; i++)
            {
                point = lineString.Point(i);
                geomBuilder.AddLine(point.X, point.Y);
            }
            geomBuilder.EndFigure();
            geomBuilder.EndGeometry();
        }

        private static void SharpMapPolygonToSqlGeometry(SqlGeometryBuilder geomBuilder, SMPolygon polygon)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.Polygon);
            AddRing(geomBuilder, polygon.ExteriorRing);
            for (int i = 0; i < polygon.NumInteriorRing; i++)
                AddRing(geomBuilder, polygon.InteriorRing(i));
            geomBuilder.EndGeometry();
        }

        private static void AddRing(SqlGeometryBuilder builder, SMLinearRing linearRing)
        {
            SMPoint pt = linearRing.StartPoint;
            builder.BeginFigure(pt.X, pt.Y);
            for (int i = 1; i < linearRing.NumPoints; i++)
            {
                pt = linearRing.Point(i);
                builder.AddLine(pt.X, pt.Y);
            }
            builder.EndFigure();
        }

        public static SMGeometry ToSharpMapGeometry(SqlGeometry geometry)
        {
            OpenGisGeometryType geometryType =
                (OpenGisGeometryType) Enum.Parse(typeof (OpenGisGeometryType), (string) geometry.STGeometryType());
            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    return SqlGeometryToSharpMapPoint(geometry);
                case OpenGisGeometryType.LineString:
                    return SqlGeometryToSharpMapLineString(geometry);
                case OpenGisGeometryType.Polygon:
                    return SqlGeometryToSharpMapPolygon(geometry);
                case OpenGisGeometryType.MultiPoint:
                    return SqlGeometryToSharpMapMultiPoint(geometry);
                case OpenGisGeometryType.MultiLineString:
                    return SqlGeometryToSharpMapMultiLineString(geometry);
                case OpenGisGeometryType.MultiPolygon:
                    return SqlGeometryToSharpMapMultiPolygon(geometry);
                case OpenGisGeometryType.GeometryCollection:
                    return SqlGeometryToSharpMapGeometryCollection(geometry);
            }
            throw new ArgumentException(string.Format("Cannot convert SqlServer '{0}' to Sharpmap.Geometry", geometry.STGeometryType()), "geometry");
        }

        private static SMGeometryCollection SqlGeometryToSharpMapGeometryCollection(SqlGeometry geometry)
        {
            SMGeometryCollection geometryCollection = new SMGeometryCollection();
            for(int i = 0; i < geometry.STNumGeometries(); i++)
                geometryCollection.Collection.Add(ToSharpMapGeometry(geometry.STGeometryN(i)));
            return geometryCollection;
        }

        private static SMMultiPolygon SqlGeometryToSharpMapMultiPolygon(SqlGeometry geometry)
        {
            SMMultiPolygon multiPolygon = new SMMultiPolygon();
            for(int i = 0; i < geometry.STNumGeometries(); i++)
                multiPolygon.Polygons.Add((SMPolygon)SqlGeometryToSharpMapPolygon(geometry.STGeometryN(i)));
            return multiPolygon;
        }

        private static SMMultiLineString SqlGeometryToSharpMapMultiLineString(SqlGeometry geometry)
        {
            SMMultiLineString multiLineString = new SMMultiLineString();
            for (int i = 0; i < geometry.STNumGeometries(); i++)
                multiLineString.LineStrings.Add((SMLineString)SqlGeometryToSharpMapLineString(geometry.STGeometryN(i)));
            return multiLineString;
        }

        private static SMGeometry SqlGeometryToSharpMapMultiPoint(SqlGeometry geometry)
        {
            SMMultiPoint multiPoint = new SMMultiPoint();
            for (int i = 0; i < geometry.STNumGeometries(); i++)
                multiPoint.Points.Add((SMPoint)SqlGeometryToSharpMapPoint(geometry.STGeometryN(i)));
            return multiPoint;
        }

        private static SMGeometry SqlGeometryToSharpMapPoint(SqlGeometry geometry)
        {
            return new SMPoint((double)geometry.STX, (double)geometry.STY);
        }

        private static IEnumerable<SMPoint> GetPointsEnumerable(SqlGeometry geometry)
        {
            foreach (SMPoint point in GetPoints(geometry))
                yield return point;
        }

        private static IList<SMPoint> GetPoints(SqlGeometry geometry)
        {
            SMPoint[] pts = new SMPoint[(int)geometry.STNumPoints()];
            for (int i = 0; i < (int)geometry.STNumPoints(); i++)
            {
                SqlGeometry ptGeometry = geometry.STPointN(i);
                pts[i] = new SMPoint((double)ptGeometry.STX, (double)ptGeometry.STY);
            }
            return pts;

        }

        private static SMGeometry SqlGeometryToSharpMapLineString(SqlGeometry geometry)
        {
            return new SMLineString(GetPoints(geometry));
        }

        private static SMGeometry SqlGeometryToSharpMapPolygon(SqlGeometry geometry)
        {
            //exterior ring
            SMLinearRing exterior = new SMLinearRing(GetPoints(geometry.STExteriorRing()));
            SMLinearRing[] interior = null;
            if (geometry.STNumInteriorRing()>0)
            {
                interior = new SMLinearRing[(int)geometry.STNumInteriorRing()];
                for (int i = 0; i < geometry.STNumInteriorRing(); i++)
                    interior[i] = new SMLinearRing(GetPoints(geometry.STInteriorRingN(i)));
            }
            return new SMPolygon(exterior, interior);
        }
    }
}