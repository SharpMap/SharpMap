using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using Microsoft.SqlServer.Types;
using SharpMap.Converters.SqlServer2008SpatialObjects;
using SharpMap.Converters.WellKnownText;
using SharpMap.Geometries;
using SharpMap.Data.Providers;

namespace UnitTests.Converters
{
    [TestFixture]
#if LINUX
    [Ignore("Requires SqlServerSpatial")]
#endif
    public class SqlServer2008
    {
        private const string Point = "POINT (20.564 46.3493254)";
        private const string Multipoint = "MULTIPOINT (20.564 46.3493254, 45 32, 23 54)";
        private const string Linestring = "LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)";

        private const string MultiLinestring =
            "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (21 21, 50 20, 50 60, 21 21))";

        private const string Polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 21 29, 29 29, 29 21, 21 21))";
        private const string MultiPolygon = "MULTIPOLYGON (((20 20, 20 30, 30 30, 30 20, 20 20)), ((41 41, 41 49, 49 49, 49 41, 41 41)))";

        [OneTimeSetUp]
        public void SetupFixture()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        }


        private IGeometry ToSqlServerAndBack(IGeometry gIn, SqlServerSpatialObjectType spatialType)
        {
            Assert.That(gIn, Is.Not.Null);
            //Assert.That(gIn.SRID, Is.EqualTo(-1));
            //gIn.SRID = 0;
            Assert.That(gIn.SRID, Is.GreaterThan(0));

            switch (spatialType)
            {
                case SqlServerSpatialObjectType.Geography:
                    SqlGeography sqlGeography = SqlGeographyConverter.ToSqlGeography(gIn);
                    //if (gIn is NetTopologySuite.Geometries.Polygon || gIn is NetTopologySuite.Geometries.MultiPolygon)
                    //{
                    //    sqlGeography.ReorientObject().MakeValid();
                    //}
                    return SqlGeographyConverter.ToSharpMapGeometry(sqlGeography, new NetTopologySuite.Geometries.GeometryFactory());

                default:
                    SqlGeometry sqlGeometry = SqlGeometryConverter.ToSqlGeometry(gIn);
                    return SqlGeometryConverter.ToSharpMapGeometry(sqlGeometry, new NetTopologySuite.Geometries.GeometryFactory());
            }
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void ConvertAndBack(SqlServerSpatialObjectType spatialType)
        {
            int srid = 4326;

            //Prepare data
            var gPn = GeometryFromWKT.Parse(Point);
            var gMp = GeometryFromWKT.Parse(Multipoint);
            var gLi = GeometryFromWKT.Parse(Linestring);
            var gML = GeometryFromWKT.Parse(MultiLinestring);
            var gPl = GeometryFromWKT.Parse(Polygon);
            var gMPol = GeometryFromWKT.Parse(MultiPolygon);

            // Geography requires valid SRID
            gPn.SRID = srid;
            gMp.SRID = srid;
            gLi.SRID = srid;
            gML.SRID = srid;
            gPl.SRID = srid;
            gMPol.SRID = srid;

            var comparison = new Comparison<IGeometry>((u, v) => u.EqualsExact(v) ? 0 : 1);

            Assert.That(ToSqlServerAndBack(gPn, spatialType), Is.EqualTo(gPn).Using(comparison));
            Assert.That(ToSqlServerAndBack(gMp, spatialType), Is.EqualTo(gMp).Using(comparison));
            Assert.That(ToSqlServerAndBack(gLi, spatialType), Is.EqualTo(gLi).Using(comparison));
            Assert.That(ToSqlServerAndBack(gML, spatialType), Is.EqualTo(gML).Using(comparison));
            Assert.That(ToSqlServerAndBack(gPl, spatialType), Is.EqualTo(gPl).Using(comparison));
            Assert.That(ToSqlServerAndBack(gMPol, spatialType), Is.EqualTo(gMPol).Using(comparison));
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry, 30)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography, 1111120)]
        public void Operations(SqlServerSpatialObjectType spatialType, double bufferDist)
        {
            int srid = (spatialType == SqlServerSpatialObjectType.Geometry ? 0 : 4326);

            //Prepare data
            var gPn = GeometryFromWKT.Parse(Point);
            gPn.SRID = srid;
            var gMp = GeometryFromWKT.Parse(Multipoint);
            gMp.SRID = srid;
            var gLi = GeometryFromWKT.Parse(Linestring);
            gLi.SRID = srid;
            var gML = GeometryFromWKT.Parse(MultiLinestring);
            gML.SRID = srid;
            var gPl = GeometryFromWKT.Parse(Polygon);
            gPl.SRID = srid;

            System.Diagnostics.Trace.WriteLine(spatialType.ToString());
            if (spatialType == SqlServerSpatialObjectType.Geography)
                System.Diagnostics.Trace.WriteLine("SqlServer syntax (STGeomFromText does not require .ReorientObject()):  SELECT geography::STGeomFromText(' insert WKT '), 4326)");

            var gPnBuffer30 = SpatialOperationsEx.Buffer(gPn, bufferDist, spatialType);
            System.Diagnostics.Trace.WriteLine(gPnBuffer30.ToString());

            var gPnBuffer30IntersectiongPl = SpatialOperationsEx.Intersection(gPnBuffer30, gPl, spatialType);
            System.Diagnostics.Trace.WriteLine(gPnBuffer30IntersectiongPl.ToString());

            var gUnion = SpatialOperationsEx.Union(gPn, spatialType, gMp, gML, gLi, gPl);
            System.Diagnostics.Trace.WriteLine(gUnion.ToString());

        }

        [Test]
        public void FailingConversionGeom()
        {
            //Prepare data
            const string invalidMultiPolygon = "MULTIPOLYGON (((20 20, 20 30, 30 30, 30 20, 20 20)), ((21 21, 21 29, 29 29, 29 21, 21 21)))";
            var gMP = GeometryFromWKT.Parse(invalidMultiPolygon);
            gMP.SRID = 4326;
            Assert.Throws<SqlGeometryConverterException>(() => gMP = ToSqlServerAndBack(gMP, SqlServerSpatialObjectType.Geometry));
        }

        [Test]
        public void FailingConversionGeog()
        {
            //Prepare data
            const string invalidMultiPolygon = "MULTIPOLYGON (((20 20, 20 30, 30 30, 30 20, 20 20)), ((21 21, 21 29, 29 29, 29 21, 21 21)))";
            var gMP = GeometryFromWKT.Parse(invalidMultiPolygon);
            gMP.SRID = 4326;
            Assert.Throws<SqlGeographyConverterException>(() => gMP = ToSqlServerAndBack(gMP, SqlServerSpatialObjectType.Geography));
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestShapeFile(SqlServerSpatialObjectType spatialType)
        {
            using (var p = new ShapeFile(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp"), true))
            {
                p.Open();

                var env = p.GetExtents();
                if (spatialType == SqlServerSpatialObjectType.Geography && (env.MaxY > 90 || env.MaxY < -90))
                    Assert.Ignore("Test file Y values exceed valid latitudes");

                for (uint i = 0; i < p.GetFeatureCount(); i++)
                {
                    var fdr = p.GetFeature(i);
                    if (fdr.Geometry == null)
                        continue;

                    try
                    {
                        fdr.Geometry.SRID = 4326;
                        var res = ToSqlServerAndBack(fdr.Geometry, spatialType);
                        Assert.AreEqual(fdr.Geometry, res);
                        System.Diagnostics.Trace.WriteLine(string.Format("Feature {0} ({1}) converted!", i, fdr[0]));
                    }
                    catch (SqlGeometryConverterException)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Feature {0} ({1}) conversion failed!", i, fdr[0]));
                    }
                    catch (SqlGeographyConverterException)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Feature {0} ({1}) conversion failed!", i, fdr[0]));
                    }
                }
            }
        }

    }
}
