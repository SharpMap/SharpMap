#region License

/*
 *  The attached / following is part of SharpMap.SqlServerSpatialObjects.
 *  
 *  SharpMap.SqlServerSpatialObjects is free software © 2010 Ingenieurgruppe IVV GmbH & Co. KG, 
 *  www.ivv-aachen.de; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/.
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  Author: Felix Obermaier 2010
 *  
 */

#endregion
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
    //[Serializable]
    public class SqlGeometryConverterException : Exception
    {
        /// <summary>
        /// The geometry to convert
        /// </summary>
        public readonly SMGeometry Geometry;

        public SqlGeometryConverterException(SMGeometry geometry)
            :this("Failed to convert SharpMapGeometry", geometry)
        {
            Geometry = geometry;
        }

        public SqlGeometryConverterException(string message, SMGeometry geometry)
            : base(message)
        {
            Geometry = geometry;
        }
    }

    public static class SqlGeometryConverter
    {
        public static double ReduceTolerance = 1d;

        public static SqlGeometry ToSqlGeometry(SMGeometry smGeometry)
        {
            SqlGeometryBuilder builder = new SqlGeometryBuilder();
            
#if !DotSpatialProjections
            if (smGeometry.SpatialReference != null)
                builder.SetSrid((int) smGeometry.SpatialReference.AuthorityCode);
#else
            if (smGeometry.SpatialReference != null)
                builder.SetSrid((int) smGeometry.SpatialReference.EpsgCode);
#endif
            else
                builder.SetSrid(0);

            SharpMapGeometryToSqlGeometry(builder, smGeometry);

            SqlGeometry g = builder.ConstructedGeometry;
            if (!g.STIsValid())
            {
                g.Reduce(ReduceTolerance);
                g.MakeValid();
            }

            if (!g.STIsValid())
                throw new SqlGeometryConverterException(smGeometry);

            return g;
        }

        public static IEnumerable<SqlGeometry> ToSqlGeometries(IEnumerable<SMGeometry> smGeometries)
        {
            foreach (SMGeometry smGeometry in smGeometries)
                yield return ToSqlGeometry(smGeometry);
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
            for(int i = 0; i < multiPoint.NumGeometries; i++)
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
            if (linearRing.NumPoints < 3)
                return;

            if (linearRing.Area == 0)
                return;

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
            if (geometry == null) return null;

            if (geometry.STIsEmpty())
                return new SMGeometryCollection();
            
            if (!geometry.STIsValid()) 
                geometry.MakeValid();
            
            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), (string)geometry.STGeometryType());
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

        public static IEnumerable<SMGeometry> ToSharpMapGeometries(IEnumerable<SqlGeometry> sqlGeometries)
        {
            foreach (SqlGeometry sqlGeometry in sqlGeometries)
                yield return ToSharpMapGeometry(sqlGeometry);
        }

        /*
        private static OpenGisGeometryType ParseGeometryType(string stGeometryType)
        {
            switch (stGeometryType.ToUpper())
            {
                case "POINT":
                    return OpenGisGeometryType.Point;
                case "LINESTRING":
                    return OpenGisGeometryType.LineString;
                case "POLYGON":
                    return OpenGisGeometryType.Polygon;
                case "MULTIPOINT":
                    return OpenGisGeometryType.MultiPoint;
                case "MULTILINESTRING":
                    return OpenGisGeometryType.MultiLineString;
                case "MULTIPOLYGON":
                    return OpenGisGeometryType.MultiPolygon;
                case "GEOMETRYCOLLECTION":
                    return OpenGisGeometryType.GeometryCollection;
            }
            throw new ArgumentException(String.Format("Invalid geometrytype '{0}'!", stGeometryType), "stGeometryType");
        }
        */

        private static SMGeometryCollection SqlGeometryToSharpMapGeometryCollection(SqlGeometry geometry)
        {
            SMGeometryCollection geometryCollection = new SMGeometryCollection();
            for(int i = 1; i <= geometry.STNumGeometries(); i++)
                geometryCollection.Collection.Add(ToSharpMapGeometry(geometry.STGeometryN(i)));
            return geometryCollection;
        }

        private static SMMultiPolygon SqlGeometryToSharpMapMultiPolygon(SqlGeometry geometry)
        {
            SMMultiPolygon multiPolygon = new SMMultiPolygon();
            for(int i = 1; i <= geometry.STNumGeometries(); i++)
                multiPolygon.Polygons.Add((SMPolygon)SqlGeometryToSharpMapPolygon(geometry.STGeometryN(i)));
            return multiPolygon;
        }

        private static SMMultiLineString SqlGeometryToSharpMapMultiLineString(SqlGeometry geometry)
        {
            SMMultiLineString multiLineString = new SMMultiLineString();
            for (int i = 1; i <= geometry.STNumGeometries(); i++)
                multiLineString.LineStrings.Add((SMLineString)SqlGeometryToSharpMapLineString(geometry.STGeometryN(i)));
            return multiLineString;
        }

        private static SMGeometry SqlGeometryToSharpMapMultiPoint(SqlGeometry geometry)
        {
            SMMultiPoint multiPoint = new SMMultiPoint();
            for (int i = 1; i <= geometry.STNumGeometries(); i++)
                multiPoint.Points.Add((SMPoint)SqlGeometryToSharpMapPoint(geometry.STGeometryN(i)));
            return multiPoint;
        }

        private static SMGeometry SqlGeometryToSharpMapPoint(SqlGeometry geometry)
        {
            return new SMPoint((double)geometry.STX, (double)geometry.STY);
        }

        private static IList<SMPoint> GetPoints(SqlGeometry geometry)
        {
            SMPoint[] pts = new SMPoint[(int)geometry.STNumPoints()];
            for (int i = 1; i <= (int)geometry.STNumPoints(); i++)
            {
                SqlGeometry ptGeometry = geometry.STPointN(i);
                pts[i-1] = new SMPoint((double)ptGeometry.STX, (double)ptGeometry.STY);
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
                for (int i = 1; i <= geometry.STNumInteriorRing(); i++)
                    interior[i-1] = new SMLinearRing(GetPoints(geometry.STInteriorRingN(i)));
            }
            return new SMPolygon(exterior, interior);
        }
    }
}