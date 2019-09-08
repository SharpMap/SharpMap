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
    public class SqlGeographyConverterException : Exception
    {
        /// <summary>
        /// The geometry to convert
        /// </summary>
        public readonly SMGeometry Geometry;

        public SqlGeographyConverterException()
        { }

        public SqlGeographyConverterException(SMGeometry geometry)
            : this("Failed to convert SharpMapGeometry", geometry)
        {
            Geometry = geometry;
        }

        public SqlGeographyConverterException(Exception inner, SMGeometry geometry)
            : this("Failed to convert SharpMapGeometry", inner, geometry)
        {
        }

        public SqlGeographyConverterException(string message, SMGeometry geometry)
            : base(message)
        {
            Geometry = geometry;
        }

        public SqlGeographyConverterException(string message, Exception inner, SMGeometry geometry)
            : base(message, inner)
        {
            Geometry = geometry;
        }

        protected SqlGeographyConverterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //Geometry = (SMGeometry) info.GetValue("geom", typeof (SMGeometry));
        }

    }
    public static class SqlGeographyConverter
    {
        private static readonly GeoAPI.IGeometryServices Services = GeoAPI.GeometryServiceProvider.Instance;

        // For GEOGRAPHY: tolerance is measured in the units defined by the unit_of_measure column of the  
        // sys.spatial_reference_systems table corresponding to the SRID in which the instance is defined
        public static double ReduceTolerance = 1d;

        static SqlGeographyConverter()
        {
            SqlServer2008Ex.LoadSqlServerTypes();
        }
        
        public static SqlGeography ToSqlGeography(SMGeometry smGeometry)
        {
            SqlGeographyBuilder builder = new SqlGeographyBuilder();
            builder.SetSrid(smGeometry.SRID);

            SharpMapGeometryToSqlGeography(builder, smGeometry);

            SqlGeography g = builder.ConstructedGeography;
            if (!g.STIsValid())
            {
                try
                {
                    g = g.Reduce(ReduceTolerance);
                    g = g.MakeValid();
                }
                catch (Exception ex)
                {
                    throw new SqlGeographyConverterException(ex, smGeometry);
                }
            }

            if (!g.STIsValid())
                throw new SqlGeographyConverterException(smGeometry);

            return g;

        }

        public static IEnumerable<SqlGeography> ToSqlGeographies(IEnumerable<SMGeometry> smGeometries)
        {
            foreach (SMGeometry smGeometry in smGeometries)
                yield return ToSqlGeography(smGeometry);
        }

        private static void SharpMapGeometryToSqlGeography(SqlGeographyBuilder geogBuilder, SMGeometry smGeometry)
        {

            switch (smGeometry.OgcGeometryType)
            {
                case SMGeometryType.Point:
                    SharpMapPointToSqlGeography(geogBuilder, smGeometry as SMPoint);
                    break;
                case SMGeometryType.LineString:
                    SharpMapLineStringToSqlGeography(geogBuilder, smGeometry as SMLineString);
                    break;
                case SMGeometryType.Polygon:
                    SharpMapPolygonToSqlGeography(geogBuilder, smGeometry as SMPolygon);
                    break;
                case SMGeometryType.MultiPoint:
                    SharpMapMultiPointToSqlGeography(geogBuilder, smGeometry as SMMultiPoint);
                    break;
                case SMGeometryType.MultiLineString:
                    SharpMapMultiLineStringToSqlGeography(geogBuilder, smGeometry as SMMultiLineString);
                    break;
                case SMGeometryType.MultiPolygon:
                    SharpMapMultiPolygonToSqlGeography(geogBuilder, smGeometry as SMMultiPolygon);
                    break;
                case SMGeometryType.GeometryCollection:
                    SharpMapGeometryCollectionToSqlGeography(geogBuilder, smGeometry as SMGeometryCollection);
                    break;
                default:
                    throw new ArgumentException(
                        String.Format("Cannot convert '{0}' geography type", smGeometry.GeometryType), "smGeometry");
            }
        }

        private static void SharpMapGeometryCollectionToSqlGeography(SqlGeographyBuilder geogBuilder, SMGeometryCollection geometryCollection)
        {
            geogBuilder.BeginGeography(OpenGisGeographyType.GeometryCollection);
            for (int i = 0; i < geometryCollection.NumGeometries; i++)
                SharpMapGeometryToSqlGeography(geogBuilder, geometryCollection[i]);
            geogBuilder.EndGeography();
        }

        private static void SharpMapMultiPolygonToSqlGeography(SqlGeographyBuilder geogBuilder, SMMultiPolygon multiPolygon)
        {
            geogBuilder.BeginGeography(OpenGisGeographyType.MultiPolygon);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
                SharpMapPolygonToSqlGeography(geogBuilder, multiPolygon[i] as SMPolygon);
            geogBuilder.EndGeography();
        }

        private static void SharpMapMultiLineStringToSqlGeography(SqlGeographyBuilder geogBuilder, SMMultiLineString multiLineString)
        {
            geogBuilder.BeginGeography(OpenGisGeographyType.MultiLineString);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
                SharpMapLineStringToSqlGeography(geogBuilder, multiLineString[i] as SMLineString);
            geogBuilder.EndGeography();
        }

        private static void SharpMapMultiPointToSqlGeography(SqlGeographyBuilder geogBuilder, SMMultiPoint multiPoint)
        {
            geogBuilder.BeginGeography(OpenGisGeographyType.MultiPoint);
            for (int i = 0; i < multiPoint.NumPoints; i++)
                SharpMapPointToSqlGeography(geogBuilder, multiPoint[i] as SMPoint);
            geogBuilder.EndGeography();
        }

        private static void SharpMapPointToSqlGeography(SqlGeographyBuilder geogBuilder, SMPoint point)
        {
            geogBuilder.BeginGeography(OpenGisGeographyType.Point);
            geogBuilder.BeginFigure(point.Y, point.X);
            geogBuilder.EndFigure();
            geogBuilder.EndGeography();
        }

        private static void SharpMapLineStringToSqlGeography(SqlGeographyBuilder geomBuilder, SMLineString lineString)
        {
            geomBuilder.BeginGeography(OpenGisGeographyType.LineString);
            var coords = lineString.Coordinates;
            geomBuilder.BeginFigure(coords[0].Y, coords[0].X);
            for (int i = 1; i < lineString.NumPoints; i++)
            {
                var point = coords[i];
                geomBuilder.AddLine(point.Y, point.X);
            }
            geomBuilder.EndFigure();
            geomBuilder.EndGeography();
        }

        private static void SharpMapPolygonToSqlGeography(SqlGeographyBuilder geogBuilder, SMPolygon polygon)
        {
            geogBuilder.BeginGeography(OpenGisGeographyType.Polygon);
            //Note: Reverse Exterior ring orientation
            AddRing(geogBuilder, (SMLinearRing)polygon.ExteriorRing.Reverse());
            for (int i = 0; i < polygon.NumInteriorRings; i++)
                AddRing(geogBuilder, (SMLinearRing)polygon.GetInteriorRingN(i));
            geogBuilder.EndGeography();
        }

        private static void AddRing(SqlGeographyBuilder builder, SMLinearRing linearRing)
        {
            if (linearRing.NumPoints < 3)
                return;

            //if (linearRing.Area == 0)
            //    return;

            var coords = linearRing.Coordinates;
            builder.BeginFigure(coords[0].Y, coords[0].X);
            for (var i = 1; i < linearRing.NumPoints; i++)
            {
                var pt = coords[i];
                builder.AddLine(pt.Y, pt.X);
            }
            builder.EndFigure();
        }

        public static SMGeometry ToSharpMapGeometry(SqlGeography geography)
        {
            return ToSharpMapGeometry(geography, null);
        }
        public static SMGeometry ToSharpMapGeometry(SqlGeography geography, Factory factory)
        {
            if (geography == null) return null;
            if (geography.IsNull) return null;
            var fact = factory ?? Services.CreateGeometryFactory((int)geography.STSrid);

            if (geography.STIsEmpty())
                return fact.CreateGeometryCollection(null);

            if (!geography.STIsValid())
                geography = geography.MakeValid();

            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), (string)geography.STGeometryType());
            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    return SqlGeographyToSharpMapPoint(geography, fact);
                case OpenGisGeometryType.LineString:
                    return SqlGeographyToSharpMapLineString(geography, fact);
                case OpenGisGeometryType.Polygon:
                    return SqlGeographyToSharpMapPolygon(geography, fact);
                case OpenGisGeometryType.MultiPoint:
                    return SqlGeographyToSharpMapMultiPoint(geography, fact);
                case OpenGisGeometryType.MultiLineString:
                    return SqlGeographyToSharpMapMultiLineString(geography, fact);
                case OpenGisGeometryType.MultiPolygon:
                    return SqlGeographyToSharpMapMultiPolygon(geography, fact);
                case OpenGisGeometryType.GeometryCollection:
                    return SqlGeographyToSharpMapGeometryCollection(geography, fact);
            }
            throw new ArgumentException(string.Format("Cannot convert SqlServer '{0}' to Sharpmap.Geometry", geography.STGeometryType()), "geography");
        }

        public static IEnumerable<SMGeometry> ToSharpMapGeometries(IEnumerable<SqlGeography> sqlGeographies)
        {
            return ToSharpMapGeometries(sqlGeographies, null);
        }

        public static IEnumerable<SMGeometry> ToSharpMapGeometries(IEnumerable<SqlGeography> sqlGeographies, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)sqlGeographies.First().STSrid);

            foreach (var sqlGeography in sqlGeographies)
                yield return ToSharpMapGeometry(sqlGeography, fact);
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

        private static SMGeometryCollection SqlGeographyToSharpMapGeometryCollection(SqlGeography geography, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geography.STSrid);
            var geoms = new SMGeometry[(int)geography.STNumGeometries()];
            for (var i = 1; i <= geography.STNumGeometries(); i++)
                geoms[i - 1] = ToSharpMapGeometry(geography.STGeometryN(i), fact);
            return fact.CreateGeometryCollection(geoms);
        }

        private static SMMultiPolygon SqlGeographyToSharpMapMultiPolygon(SqlGeography geography, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geography.STSrid);
            var polygons = new SMPolygon[(int)geography.STNumGeometries()];
            for (var i = 1; i <= geography.STNumGeometries(); i++)
                polygons[i - 1] = (SMPolygon)SqlGeographyToSharpMapPolygon(geography.STGeometryN(i), fact);
            return fact.CreateMultiPolygon(polygons);
        }

        private static SMMultiLineString SqlGeographyToSharpMapMultiLineString(SqlGeography geography, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geography.STSrid);
            var lineStrings = new SMLineString[(int)geography.STNumGeometries()];
            for (var i = 1; i <= geography.STNumGeometries(); i++)
                lineStrings[i - 1] = (SMLineString)SqlGeographyToSharpMapLineString(geography.STGeometryN(i), fact);
            return fact.CreateMultiLineString(lineStrings);
        }

        private static SMGeometry SqlGeographyToSharpMapMultiPoint(SqlGeography geography, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geography.STSrid);
            var points = new SMPoint[(int)geography.STNumGeometries()];
            for (var i = 1; i <= geography.STNumGeometries(); i++)
                points[i - 1] = (SMPoint)SqlGeographyToSharpMapPoint(geography.STGeometryN(i), fact);
            return fact.CreateMultiPoint(points);
        }

        private static SMGeometry SqlGeographyToSharpMapPoint(SqlGeography geography, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geography.STSrid);
            return fact.CreatePoint(new Coordinate((double)geography.Long, (double)geography.Lat));
        }

        private static Coordinate[] GetPoints(SqlGeography geography)
        {
            var pts = new Coordinate[(int)geography.STNumPoints()];
            for (int i = 1; i <= (int)geography.STNumPoints(); i++)
            {
                var ptGeometry = geography.STPointN(i);
                pts[i - 1] = new Coordinate((double)ptGeometry.Long, (double)ptGeometry.Lat);
            }
            return pts;
        }

        private static SMGeometry SqlGeographyToSharpMapLineString(SqlGeography geography, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geography.STSrid);
            return fact.CreateLineString(GetPoints(geography));
        }

        private static SMGeometry SqlGeographyToSharpMapPolygon(SqlGeography geography, Factory factory)
        {
            var fact = factory ?? Services.CreateGeometryFactory((int)geography.STSrid);

            // courtesy of NetTopologySuite.Io.SqlServerBytes
            var rings = new List<ILinearRing>();
            for (var i = 1; i <= geography.NumRings(); i++)
                rings.Add(fact.CreateLinearRing(GetPoints(geography.RingN(i))));

            var shellCCW = rings.FirstOrDefault(r => r.IsCCW);
            // NB: reverse exterio ring orientation
            var shellCW = fact.CreateLinearRing(shellCCW.Reverse().Coordinates);

            return fact.CreatePolygon(shellCW, Enumerable.ToArray(rings.Where(r => r != shellCCW)));
        }

    }
}
