using System;
using System.IO;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace UnitTests.Serialization
{
    public class ProviderTest : BaseSerializationTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        internal static IProvider CreateProvider(string name=null)
        {
            name = name ?? "geometryprovider";
            switch (name.ToLower())
            {
                default:
                //case "geometryprovider":
                    return CreateProvider<GeometryProvider>();
                case "shapefile":
                    return CreateProvider<ShapeFile>();
                case "postgis":
                    return CreateProvider<PostGIS>();
                case "managedspatialite":
                    return CreateProvider<ManagedSpatiaLite>();
                case "spatialite":
                    return CreateProvider<SpatiaLite>();
            }
        }

        internal static T CreateProvider<T>()
        where T: IProvider
        {
            try
            {
                var gf = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory();

                if (typeof(T) == typeof(GeometryProvider))
                    return (T) (IProvider) new GeometryProvider(gf.CreatePoint(new Coordinate(1, 1)));

                if (typeof(T) == typeof(ShapeFile))
                    return (T) (IProvider) new ShapeFile(TestUtility.GetPathToTestFile("roads_ugl.shp"));

                if (typeof(T) == typeof(PostGIS))
                    return (T) (IProvider) new PostGIS(
                        "Host=ivv-t3s.ivv-aachen.de;Port=5432;integrated security=true;database=postgis_sample;",
                        "rivers", "wkb_geometry", "ogc_fid");

                if (typeof(T) == typeof(ManagedSpatiaLite))
                    return (T)(IProvider)new ManagedSpatiaLite($"Data Source={TestUtility.GetPathToTestFile("test-2.3.sqlite")};",
                        "HighWays", "Geometry", "PK_UID");

                if (typeof(T) == typeof(SpatiaLite))
                    return (T)(IProvider)new SpatiaLite($"Data Source={TestUtility.GetPathToTestFile("test-2.3.sqlite")};",
                        "HighWays", "Geometry", "PK_UID");

#if !LINUX
            if (typeof(T) == typeof(SqlServer2008))
                return (T)(IProvider)new SqlServer2008("Data Source=IVV-SQLD; Database=OBE;Integrated Security=SSPI;",
                                                       "Roads", "wkb_geometry", "ogc_fid", SqlServerSpatialObjectType.Geometry);
#endif
                throw new NotSupportedException();
            }
            catch (Exception e)
            {
                Assert.Ignore("Failed to open connection\n{0}\n{1}", e.Message, e.StackTrace);
                throw;
            }
        }

        [Test]
        public void TestGeometryProvider()
        {
            var gpS = CreateProvider<GeometryProvider>();
            var gpD = SandD(gpS, GetFormatter());

            Assert.AreEqual(gpS.GetType(), gpD.GetType());
            Assert.AreEqual(gpS.Geometries.Count, gpD.Geometries.Count);
            Assert.AreEqual(gpS.GetExtents(), gpD.GetExtents());

            Assert.IsTrue(gpS.Geometries[0].Coordinate.X == 1);
        }
#if !LINUX
        [Test]
        public void TestSpatiaLite2()
        {
            var spatiaLite2S = new ManagedSpatiaLite($"Data Source={Path.Combine("TestData", "test-2.3.sqlite")};Database=Regions;",
                                                     "Roads", "Geometry", "PK_UID");

            var spatiaLite2 = SandD(spatiaLite2S, GetFormatter());

            Assert.AreEqual(spatiaLite2S.ConnectionString, spatiaLite2.ConnectionString);
            Assert.AreEqual(spatiaLite2S.Table, spatiaLite2.Table);
            Assert.AreEqual(spatiaLite2S.GeometryColumn, spatiaLite2.GeometryColumn);
            Assert.AreEqual(spatiaLite2S.ObjectIdColumn, spatiaLite2.ObjectIdColumn);
            Assert.AreEqual(spatiaLite2S.SRID, spatiaLite2.SRID);
        }
#endif

        [Test, Ignore("Postgres Connection string needs to be ok")]
        public void TestPostGIS()
        {
            using (var pS = CreateProvider<PostGIS>())
            {
                //new PostGIS(Properties.Settings.Default.PostGis,
                //                 "rivers", "wkb_geometry", "ogc_fid");
                PostGIS pD = null;
                Assert.DoesNotThrow(() => pD = SandD(pS, GetFormatter()));

                Assert.AreEqual(pS.ConnectionString, pD.ConnectionString);
                Assert.AreEqual(pS.Table, pD.Table);
                Assert.AreEqual(pS.GeometryColumn, pD.GeometryColumn);
                Assert.AreEqual(pS.ObjectIdColumn, pD.ObjectIdColumn);
                Assert.AreEqual(pS.SRID, pD.SRID);
                pD.Dispose();
            }
        }

#if !LINUX
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestSqlServer2008(SqlServerSpatialObjectType spatialType)
        {
            var sql2008S = new SqlServer2008("Data Source=IVV-SQLD; Database=OBE;Integrated Security=SSPI;",
                                             "roads", "spatialCol", "idCol",
                                             spatialType, 
                                             4326, 
                                             SqlServer2008ExtentsMode.EnvelopeAggregate);

            var sql2008D = SandD(sql2008S, GetFormatter());

            Assert.AreEqual(sql2008S.ConnectionString, sql2008D.ConnectionString);
            Assert.AreEqual(sql2008S.Table, sql2008D.Table);
            Assert.AreEqual(sql2008S.TableSchema, sql2008D.TableSchema);
            Assert.AreEqual(sql2008S.GeometryColumn, sql2008D.GeometryColumn);
            Assert.AreEqual(sql2008S.ObjectIdColumn, sql2008D.ObjectIdColumn);
            Assert.AreEqual(sql2008S.SpatialObjectType, sql2008D.SpatialObjectType);
            Assert.AreEqual(sql2008S.SRID, sql2008D.SRID);
            Assert.AreEqual(sql2008S.ExtentsMode, sql2008D.ExtentsMode);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestSqlServer2008Ex(SqlServerSpatialObjectType spatialType)
        {
            var sql2008S = new SqlServer2008Ex("Data Source=IVV-SQLD; Database=OBE;Integrated Security=SSPI;",
                                             "roads", "spatialCol", "idCol",
                                             spatialType,
                                             4326,
                                             SqlServer2008ExtentsMode.EnvelopeAggregate);

            var sql2008D = SandD(sql2008S, GetFormatter());

            Assert.AreEqual(sql2008S.ConnectionString, sql2008D.ConnectionString);
            Assert.AreEqual(sql2008S.Table, sql2008D.Table);
            Assert.AreEqual(sql2008S.TableSchema, sql2008D.TableSchema);
            Assert.AreEqual(sql2008S.GeometryColumn, sql2008D.GeometryColumn);
            Assert.AreEqual(sql2008S.ObjectIdColumn, sql2008D.ObjectIdColumn);
            Assert.AreEqual(sql2008S.SpatialObjectType, sql2008D.SpatialObjectType);
            Assert.AreEqual(sql2008S.SRID, sql2008D.SRID);
            Assert.AreEqual(sql2008S.ExtentsMode, sql2008D.ExtentsMode);
        }

#endif

    }
}
