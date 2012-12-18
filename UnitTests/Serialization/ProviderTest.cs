using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace UnitTests.Serialization
{
    public class ProviderTest : BaseSerializationTest
    {
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        [Test]
        public void TestGeometryProvider()
        {
            var gf = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory();
            gf.CreatePoint(new Coordinate(1, 1));
            var gpS = new GeometryProvider(gf.CreatePoint(new Coordinate(1, 1)));

            var gpD = SandD(gpS, GetFormatter());

            Assert.AreEqual(gpS.GetType(), gpD.GetType());
            Assert.AreEqual(gpS.Geometries.Count, gpD.Geometries.Count);
            Assert.AreEqual(gpS.GetExtents(), gpD.GetExtents());

            Assert.IsTrue(gpS.Geometries[0].Coordinate.X == 1);
        }

        [Test]
        public void TestSpatiaLite2()
        {
            var spatiaLite2S = new ManagedSpatiaLite("Data Source=test-2.3.sqlite;Database=Regions;",
                                                     "Roads", "Geometry", "PK_UID");

            var spatiaLite2 = SandD(spatiaLite2S, GetFormatter());

            Assert.AreEqual(spatiaLite2S.ConnectionString, spatiaLite2.ConnectionString);
            Assert.AreEqual(spatiaLite2S.Table, spatiaLite2.Table);
            Assert.AreEqual(spatiaLite2S.GeometryColumn, spatiaLite2.GeometryColumn);
            Assert.AreEqual(spatiaLite2S.ObjectIdColumn, spatiaLite2.ObjectIdColumn);
            Assert.AreEqual(spatiaLite2S.SRID, spatiaLite2.SRID);
        }

        [Test]
        public void TestSqlServer2008()
        {
            var sql2008S = new SqlServer2008("Data Source=IVV-SQLD; Database=OBE;Integrated Security=SSPI;",
                                             "roads", "wkb_geometry", "ogc_fid", SqlServerSpatialObjectType.Geometry);

            var sql2008D = SandD(sql2008S, GetFormatter());

            Assert.AreEqual(sql2008S.ConnectionString, sql2008D.ConnectionString);
            Assert.AreEqual(sql2008S.Table, sql2008D.Table);
            Assert.AreEqual(sql2008S.TableSchema, sql2008D.TableSchema);
            Assert.AreEqual(sql2008S.GeometryColumn, sql2008D.GeometryColumn);
            Assert.AreEqual(sql2008S.ObjectIdColumn, sql2008D.ObjectIdColumn);
            Assert.AreEqual(sql2008S.SpatialObjectType, sql2008D.SpatialObjectType);
            Assert.AreEqual(sql2008S.SRID, sql2008D.SRID);
        }
    
    }
}