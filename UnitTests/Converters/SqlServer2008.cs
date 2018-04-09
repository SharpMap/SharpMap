using System;
using NUnit.Framework;
//using NUnit.Framework.SyntaxHelpers;
using Microsoft.SqlServer.Types;
using SharpMap.Converters.SqlServer2008SpatialObjects;
using SharpMap.Converters.WellKnownText;
using SharpMap.Geometries;
using Geometry = GeoAPI.Geometries.IGeometry;

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

        private Geometry ToSqlServerAndBack(Geometry gIn)
        {
            Assert.That(gIn, Is.Not.Null);
            Assert.That(gIn.SRID, Is.EqualTo(-1));
            gIn.SRID = 0;
            SqlGeometry sqlGeometry = SqlGeometryConverter.ToSqlGeometry(gIn);
            Geometry gOut = SqlGeometryConverter.ToSharpMapGeometry(sqlGeometry, new NetTopologySuite.Geometries.GeometryFactory());
            return gOut;
        }

        [Test]
        public void ConvertAndBack()
        {
            //Prepare data
            Geometry gPn = GeometryFromWKT.Parse(Point);
            Geometry gMp = GeometryFromWKT.Parse(Multipoint);
            Geometry gLi = GeometryFromWKT.Parse(Linestring);
            Geometry gML = GeometryFromWKT.Parse(MultiLinestring);
            Geometry gPl = GeometryFromWKT.Parse(Polygon);
            Geometry gMPol = GeometryFromWKT.Parse(MultiPolygon);

            var comparison = new Comparison<Geometry>((u, v) => u.EqualsExact(v) ? 0 : 1);

            Assert.That(ToSqlServerAndBack(gPn), Is.EqualTo(gPn).Using(comparison));
            Assert.That(ToSqlServerAndBack(gMp), Is.EqualTo(gMp).Using(comparison));
            Assert.That(ToSqlServerAndBack(gLi), Is.EqualTo(gLi).Using(comparison));
            Assert.That(ToSqlServerAndBack(gML), Is.EqualTo(gML).Using(comparison));
            Assert.That(ToSqlServerAndBack(gPl), Is.EqualTo(gPl).Using(comparison));
            Assert.That(ToSqlServerAndBack(gMPol), Is.EqualTo(gMPol).Using(comparison));
        }

        [Test]
        public void Operations()
        {
            //Prepare data
            Geometry gPn = GeometryFromWKT.Parse(Point);
            gPn.SRID = 0;
            Geometry gMp = GeometryFromWKT.Parse(Multipoint);
            gMp.SRID = 0;
            Geometry gLi = GeometryFromWKT.Parse(Linestring);
            gLi.SRID = 0;
            Geometry gML = GeometryFromWKT.Parse(MultiLinestring);
            gML.SRID = 0;
            Geometry gPl = GeometryFromWKT.Parse(Polygon);
            gPl.SRID = 0;

            Geometry gPnBuffer30 = SpatialOperationsEx.Buffer(gPn, 30);
            Console.WriteLine(gPnBuffer30.ToString());

            Geometry gPnBuffer30IntersectiongPl = SpatialOperationsEx.Intersection(gPnBuffer30, gPl);
            Console.WriteLine(gPnBuffer30IntersectiongPl.ToString());

            Geometry gUnion = SpatialOperationsEx.Union(gPn, gMp, gML, gLi, gPl);
            Console.WriteLine(gUnion.ToString());

        }

        [Test]
        [ExpectedException(typeof(SqlGeometryConverterException))]
        public void FailingConversions()
        {
            //Prepare data
            const string invalidMultiPolygon = "MULTIPOLYGON (((20 20, 20 30, 30 30, 30 20, 20 20)), ((21 21, 21 29, 29 29, 29 21, 21 21)))";
            Geometry gMP = GeometryFromWKT.Parse(invalidMultiPolygon);
            Assert.AreEqual(gMP, ToSqlServerAndBack(gMP));
        }

        private string GetTestFile()
        {
            return System.IO.Path.Combine(GetPathToTestDataDir(), "SPATIAL_F_SKARVMUFF.shp");
        }

        private string GetPathToTestDataDir()
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(GetType().Assembly.CodeBase.Replace("file:///", "")), @"TestData\");
        }

        [Test]
        public void TestShapeFile()
        {
            using (var p = new SharpMap.Data.Providers.ShapeFile(GetTestFile(), true))
            {
                p.Open();

                for (uint i = 0; i < p.GetFeatureCount(); i++ )
                {
                    var fdr = p.GetFeature(i);
                    try
                    {
                        fdr.Geometry.SRID = -1;
                        var res = ToSqlServerAndBack(fdr.Geometry);
                        Assert.AreEqual(fdr.Geometry, res);
                        Console.WriteLine(string.Format("Feature {0} ({1}) converted!", i, fdr[0]));
                    }
                    catch (SqlGeometryConverterException)
                    {
                        Console.WriteLine(string.Format("Feature {0} ({1}) conversion failed!", i, fdr[0]));
                    }
                }
            }
        }

    }
}   