#region License

/*
 *  The attached / following is part of SharpMap.SqlServerSpatialObjects.
 *  
 *  SharpMap.SqlServerSpatialObjects is free software � 2010 Ingenieurgruppe IVV GmbH & Co. KG, 
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
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GeoAPI.Geometries;
using Microsoft.SqlServer.Types;
using SharpMap.Data.Providers;
using SMGeometry = GeoAPI.Geometries.IGeometry;
using SMGeometryType = GeoAPI.Geometries.OgcGeometryType;
using SMPoint = GeoAPI.Geometries.IPoint;
using SMLineString = GeoAPI.Geometries.ILineString;
using SMLinearRing = GeoAPI.Geometries.ILinearRing;
using SMPolygon = GeoAPI.Geometries.IPolygon;
using SMMultiPoint = GeoAPI.Geometries.IMultiPoint;
using SMMultiLineString = GeoAPI.Geometries.IMultiLineString;
using SMMultiPolygon = GeoAPI.Geometries.IMultiPolygon;
using SMGeometryCollection = GeoAPI.Geometries.IGeometryCollection;
using Factory = GeoAPI.Geometries.IGeometryFactory;

namespace SharpMap.Converters.SqlServer2008SpatialObjects
{
    [Serializable]
    public class SqlGeometryConverterException : Exception
    {
        /// <summary>
        /// The geometry to convert
        /// </summary>
        public readonly SMGeometry Geometry;

        public SqlGeometryConverterException()
        {}

        public SqlGeometryConverterException(SMGeometry geometry)
            :this("Failed to convert SharpMapGeometry", geometry)
        {
            Geometry = geometry;
        }

        public SqlGeometryConverterException(Exception inner, SMGeometry geometry)
            : this("Failed to convert SharpMapGeometry", inner, geometry)
        {
        }

        public SqlGeometryConverterException(string message, SMGeometry geometry)
            : base(message)
        {
            Geometry = geometry;
        }

        public SqlGeometryConverterException(string message, Exception inner, SMGeometry geometry)
            : base(message, inner)
        {
            Geometry = geometry;
        }

        protected SqlGeometryConverterException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            //Geometry = (SMGeometry) info.GetValue("geom", typeof (SMGeometry));
        }

    }

    public static class SqlGeometryConverter
    {
        private static readonly GeoAPI.IGeometryServices Services = GeoAPI.GeometryServiceProvider.Instance;

        public static double ReduceTolerance = 1d;
        
        static  SqlGeometryConverter()
        {
            SqlServer2008Ex.LoadSqlServerTypes();    
        }
        
        public static SqlGeometry ToSqlGeometry(SMGeometry smGeometry)
        {
            SqlGeometryBuilder builder = new SqlGeometryBuilder();
            builder.SetSrid(smGeometry.SRID);

            SharpMapGeometryToSqlGeometry(builder, smGeometry);

            SqlGeometry g = builder.ConstructedGeometry;
            if (!g.STIsValid())
            {
                try
                {
                    g = g.Reduce(ReduceTolerance);
                    g = g.MakeValid();
                }
                catch (Exception ex)
                {
                    throw new SqlGeometryConverterException(ex, smGeometry);
                }
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
            
            switch (smGeometry.OgcGeometryType)
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
                SharpMapGeometryToSqlGeometry(geomBuilder, geometryCollection[i]);
            geomBuilder.EndGeometry();
        }

        private static void SharpMapMultiPolygonToSqlGeometry(SqlGeometryBuilder geomBuilder, SMMultiPolygon multiPolygon)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.MultiPolygon);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
                SharpMapPolygonToSqlGeometry(geomBuilder, multiPolygon[i] as SMPolygon);
            geomBuilder.EndGeometry();
        }

        private static void SharpMapMultiLineStringToSqlGeometry(SqlGeometryBuilder geomBuilder, SMMultiLineString multiLineString)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
                SharpMapLineStringToSqlGeometry(geomBuilder, multiLineString[i] as SMLineString);
            geomBuilder.EndGeometry();
        }

        private static void SharpMapMultiPointToSqlGeometry(SqlGeometryBuilder geomBuilder, SMMultiPoint multiPoint)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.MultiPoint);
            for(int i = 0; i < multiPoint.NumPoints; i++)
                SharpMapPointToSqlGeometry(geomBuilder, multiPoint[i] as SMPoint);
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
            var coords = lineString.Coordinates;
            geomBuilder.BeginFigure(coords[0].X, coords[0].Y);
            for (int i = 1; i < lineString.NumPoints; i++)
            {
                var point = coords[i];
                geomBuilder.AddLine(point.X, point.Y);
            }
            geomBuilder.EndFigure();
            geomBuilder.EndGeometry();
        }

        private static void SharpMapPolygonToSqlGeometry(SqlGeometryBuilder geomBuilder, SMPolygon polygon)
        {
            geomBuilder.BeginGeometry(OpenGisGeometryType.Polygon);
            AddRing(geomBuilder, (SMLinearRing )polygon.ExteriorRing);
            for (int i = 0; i < polygon.NumInteriorRings; i++)
                AddRing(geomBuilder, (SMLinearRing )polygon.GetInteriorRingN(i));
            geomBuilder.EndGeometry();
        }

        private static void AddRing(SqlGeometryBuilder builder, SMLinearRing linearRing)
        {
            if (linearRing.NumPoints < 3)
                return;

            //if (linearRing.Area == 0)
            //    return;

            var coords = linearRing.Coordinates;
            builder.BeginFigure(coords[0].X, coords[0].Y);
            for (var i = 1; i < linearRing.NumPoints; i++)
            {
                var pt = coords[i];
                builder.AddLine(pt.X, pt.Y);
            }
            builder.EndFigure();
        }

        public static SMGeometry ToSharpMapGeometry(SqlGeometry geometry)
        {
            return ToSharpMapGeometry(geometry, null);
        }
        public static SMGeometry ToSharpMapGeometry(SqlGeometry geometry, Factory factory)
        {
            if (geometry == null) return null;
            if (geometry.IsNull) return null;

            var fact = factory ?? Services.CreateGeometryFactory((int) geometry.STSrid);
            
            if (geometry.STIsEmpty())
                return fact.CreateGeometryCollection(null);
            
            if (!geometry.STIsValid()) 
                geometry = geometry.MakeValid();
            
            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), (string)geometry.STGeometryType());
            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    return SqlGeometryToSharpMapPoint(geometry, fact);
                case OpenGisGeometryType.LineString:
                    return SqlGeometryToSharpMapLineString(geometry, fact);
                case OpenGisGeometryType.Polygon:
                    return SqlGeometryToSharpMapPolygon(geometry, fact);
                case OpenGisGeometryType.MultiPoint:
                    return SqlGeometryToSharpMapMultiPoint(geometry, fact);
                case OpenGisGeometryType.MultiLineString:
                    return SqlGeometryToSharpMapMultiLineString(geometry, fact);
                case OpenGisGeometryType.MultiPolygon:
                    return SqlGeometryToSharpMapMultiPolygon(geometry, fact);
                case OpenGisGeometryType.GeometryCollection:
                    return SqlGeometryToSharpMapGeometryCollection(geometry, fact);
            }
            throw new ArgumentException(string.Format("Cannot convert SqlServer '{0}' to Sharpmap.Geometry", geometry.STGeometryType()), "geometry");
        }

        public static IEnumerable<SMGeometry> ToSharpMapGeometries(IEnumerable<SqlGeometry> sqlGeometries)
        {
            return ToSharpMapGeometries(sqlGeometries,null);
        }

        public static IEnumerable<SMGeometry> ToSharpMapGeometries(IEnumerable<SqlGeometry> sqlGeometries, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)sqlGeometries.First().STSrid);
            
            foreach (var sqlGeometry in sqlGeometries)
                yield return ToSharpMapGeometry(sqlGeometry, fact);
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

        private static SMGeometryCollection SqlGeometryToSharpMapGeometryCollection(SqlGeometry geometry, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geometry.STSrid);
            var geoms = new SMGeometry[(int)geometry.STNumGeometries()];
            for (var i = 1; i <= geometry.STNumGeometries(); i++)
                geoms[i-1] = ToSharpMapGeometry(geometry.STGeometryN(i), fact);
            return fact.CreateGeometryCollection(geoms);
        }

        private static SMMultiPolygon SqlGeometryToSharpMapMultiPolygon(SqlGeometry geometry, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geometry.STSrid);
            var polygons = new SMPolygon[(int)geometry.STNumGeometries()];
            for (var i = 1; i <= geometry.STNumGeometries(); i++)
                polygons[i-1] = (SMPolygon)SqlGeometryToSharpMapPolygon(geometry.STGeometryN(i), fact);
            return fact.CreateMultiPolygon(polygons);
        }

        private static SMMultiLineString SqlGeometryToSharpMapMultiLineString(SqlGeometry geometry, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geometry.STSrid);
            var lineStrings = new SMLineString[(int)geometry.STNumGeometries()];
            for (var i = 1; i <= geometry.STNumGeometries(); i++)
                lineStrings[i-1] = (SMLineString)SqlGeometryToSharpMapLineString(geometry.STGeometryN(i), fact);
            return fact.CreateMultiLineString(lineStrings);
        }

        private static SMGeometry SqlGeometryToSharpMapMultiPoint(SqlGeometry geometry, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geometry.STSrid);
            var points = new SMPoint[(int)geometry.STNumGeometries()];
            for (var i = 1; i <= geometry.STNumGeometries(); i++)
                points[i-1] = (SMPoint) SqlGeometryToSharpMapPoint(geometry.STGeometryN(i), fact);
            return fact.CreateMultiPoint(points);
        }

        private static SMGeometry SqlGeometryToSharpMapPoint(SqlGeometry geometry, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geometry.STSrid);
            return fact.CreatePoint(new Coordinate( (double)geometry.STX, (double)geometry.STY));
        }

        private static Coordinate[] GetPoints(SqlGeometry geometry)
        {
            var pts = new Coordinate[(int)geometry.STNumPoints()];
            for (int i = 1; i <= (int)geometry.STNumPoints(); i++)
            {
                var ptGeometry = geometry.STPointN(i);
                pts[i-1] = new Coordinate((double)ptGeometry.STX, (double)ptGeometry.STY);
            }
            return pts;
        }

        private static SMGeometry SqlGeometryToSharpMapLineString(SqlGeometry geometry, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geometry.STSrid);
            return fact.CreateLineString(GetPoints(geometry));
        }

        private static SMGeometry SqlGeometryToSharpMapPolygon(SqlGeometry geometry, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geometry.STSrid);
            //exterior ring
            var exterior = fact.CreateLinearRing(GetPoints(geometry.STExteriorRing()));
            SMLinearRing[] interior = null;
            if (geometry.STNumInteriorRing()>0)
            {
                interior = new SMLinearRing[(int)geometry.STNumInteriorRing()];
                for (var i = 1; i <= geometry.STNumInteriorRing(); i++)
                    interior[i - 1] = fact.CreateLinearRing(GetPoints(geometry.STInteriorRingN(i)));
            }
            return Services.CreateGeometryFactory((int)geometry.STSrid).CreatePolygon(exterior, interior);
        }
    }
}
