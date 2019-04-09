using System;
using NUnit.Framework;
//using NUnit.Framework.SyntaxHelpers;
using Microsoft.SqlServer.Types;
using SharpMap.Converters.SqlServer2008SpatialObjects;
using SharpMap.Converters.WellKnownText;
using SharpMap.Geometries;
using Geometry = GeoAPI.Geometries.IGeometry;
using SharpMap.Data.Providers;

namespace UnitTests.Converters
{
    [TestFixture]
    [Ignore("Requires SqlServerSpatial")]
    public class SqlServer2008
    {
        private const string Point = "POINT (20.564 46.3493254)";
        private const string Multipoint = "MULTIPOINT (20.564 46.3493254, 45 32, 23 54)";
        private const string Linestring = "LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)";

        private const string MultiLinestring =
            "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (21 21, 50 20, 50 60, 21 21))";

        private const string Polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 21 29, 29 29, 29 21, 21 21))";
        private const string MultiPolygon = "MULTIPOLYGON (((20 20, 20 30, 30 30, 30 20, 20 20)), ((41 41, 41 49, 49 49, 49 41, 41 41)))";

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        }


        private Geometry ToSqlServerAndBack(Geometry gIn, SqlServerSpatialObjectType spatialType)
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
            Geometry gPn = GeometryFromWKT.Parse(Point);
            Geometry gMp = GeometryFromWKT.Parse(Multipoint);
            Geometry gLi = GeometryFromWKT.Parse(Linestring);
            Geometry gML = GeometryFromWKT.Parse(MultiLinestring);
            Geometry gPl = GeometryFromWKT.Parse(Polygon);
            Geometry gMPol = GeometryFromWKT.Parse(MultiPolygon);

            // Geography requires valid SRID
            gPn.SRID = srid;
            gMp.SRID = srid;
            gLi.SRID = srid;
            gML.SRID = srid;
            gPl.SRID = srid;
            gMPol.SRID = srid;

            var comparison = new Comparison<Geometry>((u, v) => u.EqualsExact(v) ? 0 : 1);

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
            Geometry gPn = GeometryFromWKT.Parse(Point);
            gPn.SRID = srid;
            Geometry gMp = GeometryFromWKT.Parse(Multipoint);
            gMp.SRID = srid;
            Geometry gLi = GeometryFromWKT.Parse(Linestring);
            gLi.SRID = srid;
            Geometry gML = GeometryFromWKT.Parse(MultiLinestring);
            gML.SRID = srid;
            Geometry gPl = GeometryFromWKT.Parse(Polygon);
            gPl.SRID = srid;

            System.Diagnostics.Trace.WriteLine(spatialType.ToString());
            if (spatialType == SqlServerSpatialObjectType.Geography)
                System.Diagnostics.Trace.WriteLine("SqlServer syntax (STGeomFromText does not require .ReorientObject()):  SELECT geography::STGeomFromText(' insert WKT '), 4326)");

            Geometry gPnBuffer30 = SpatialOperationsEx.Buffer(gPn, bufferDist, spatialType);
            System.Diagnostics.Trace.WriteLine(gPnBuffer30.ToString());

            Geometry gPnBuffer30IntersectiongPl = SpatialOperationsEx.Intersection(gPnBuffer30, gPl, spatialType);
            System.Diagnostics.Trace.WriteLine(gPnBuffer30IntersectiongPl.ToString());

            Geometry gUnion = SpatialOperationsEx.Union(gPn, spatialType, gMp, gML, gLi, gPl);
            System.Diagnostics.Trace.WriteLine(gUnion.ToString());

        }

        [Test]
        [ExpectedException(typeof(SqlGeometryConverterException))]
        public void FailingConversionGeom()
        {
            //Prepare data
            const string invalidMultiPolygon = "MULTIPOLYGON (((20 20, 20 30, 30 30, 30 20, 20 20)), ((21 21, 21 29, 29 29, 29 21, 21 21)))";
            Geometry gMP = GeometryFromWKT.Parse(invalidMultiPolygon);
            gMP.SRID = 4326;
            Assert.AreEqual(gMP, ToSqlServerAndBack(gMP, SqlServerSpatialObjectType.Geometry));
        }

        [Test]
        [ExpectedException(typeof(SqlGeographyConverterException))]
        public void FailingConversionGeog()
        {
            //Prepare data
            const string invalidMultiPolygon = "MULTIPOLYGON (((20 20, 20 30, 30 30, 30 20, 20 20)), ((21 21, 21 29, 29 29, 29 21, 21 21)))";
            Geometry gMP = GeometryFromWKT.Parse(invalidMultiPolygon);
            gMP.SRID = 4326;
            Assert.AreEqual(gMP, ToSqlServerAndBack(gMP, SqlServerSpatialObjectType.Geography));
        }

        private string GetTestFile()
        {
            return System.IO.Path.Combine(GetPathToTestDataDir(), "SPATIAL_F_SKARVMUFF.shp");
            //return System.IO.Path.Combine(GetPathToTestDataDir(), "roads_ugl.shp");
        }

        private string GetPathToTestDataDir()
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(GetType().Assembly.CodeBase.Replace("file:///", "")), @"TestData\");
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestShapeFile(SqlServerSpatialObjectType spatialType)
        {
            using (var p = new SharpMap.Data.Providers.ShapeFile(GetTestFile(), true))
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
