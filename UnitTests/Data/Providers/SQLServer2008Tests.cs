using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.IO;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    [NUnit.Framework.TestFixture]
    public class SQLServer2008Tests
    {
        [TestFixtureSetUp]
        public void SetupFixture()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        [NUnit.Framework.TestCase("[sde].[gisadmin.di]", "sde", "gisadmin.di")]
        [NUnit.Framework.TestCase("[sde.gisadmin].[di]", "sde.gisadmin", "di")]
        [NUnit.Framework.TestCase("sde.gisadmin.di", "sde.gisadmin", "di")]
        public void VerifySchemaDetection(string schemaTable, string tableSchema, string table)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = new SharpMap.Data.Providers.SqlServer2008("", schemaTable, "oidcolumn");
            Assert.AreEqual(tableSchema, sq.TableSchema);
            Assert.AreEqual(table, sq.Table);
            Assert.AreEqual("oidcolumn", sq.ObjectIdColumn);

            System.Reflection.PropertyInfo pi = sq.GetType().GetProperty("QualifiedTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
            string qualifiedTable = (string)pi.GetValue(sq, null);
            Assert.IsTrue(qualifiedTable.Contains(tableSchema));
            Assert.IsTrue(qualifiedTable.Contains(table));
        }
    }

    [NUnit.Framework.TestFixture]
    [Ignore("Requires SqlServerSpatial")]
    public class SQLServer2008DbTests
    {
        // Geography requires valid SRID (0 not acceptable)
        private const int geogSrid = 4326;

        private string GetTestFile()
        {
            return Path.Combine(GetPathToTestDataDir(), "roads_ugl.shp");
        }
        private string GetPathToTestDataDir()
        {
            return Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.CodeBase.Replace("file:///", "")), @"TestData\");
        }

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder(UnitTests.Properties.Settings.Default.SqlServer2008);
            if (string.IsNullOrEmpty(connStrBuilder.DataSource) || string.IsNullOrEmpty(connStrBuilder.InitialCatalog))
            {
                Assert.Ignore("Requires SQL Server connectionstring");
            }

            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            // Set up sample tables (Geometry + Geography)
            using (SqlConnection conn = new SqlConnection(UnitTests.Properties.Settings.Default.SqlServer2008))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE roads_ugl_geom(ID int identity(1,1) PRIMARY KEY, NAME nvarchar(100), GEOM geometry)";
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE roads_ugl_geog(ID int identity(1,1) PRIMARY KEY, NAME nvarchar(100), GEOG geography)";
                    cmd.ExecuteNonQuery();
                }


                // Load data
                using (SharpMap.Data.Providers.ShapeFile shapeFile = new SharpMap.Data.Providers.ShapeFile(GetTestFile()))
                {
                    shapeFile.Open();

                    IEnumerable<uint> indexes = shapeFile.GetObjectIDsInView(shapeFile.GetExtents());

                    indexes = indexes.Take(100);

                    var cmdGeom = new SqlCommand("INSERT INTO roads_ugl_geom(NAME, GEOM) VALUES (@Name, geometry::STGeomFromText(@Geom, @Srid))", conn);
                    var cmdGeog = new SqlCommand("INSERT INTO roads_ugl_geog(NAME, GEOG) VALUES (@Name, geography::STGeomFromText(@Geog, @Srid))", conn);

                    foreach (uint idx in indexes)
                    {
                        SharpMap.Data.FeatureDataRow feature = shapeFile.GetFeature(idx);

                        if (cmdGeom.Parameters.Count == 0)
                        {
                            cmdGeom.Parameters.AddWithValue("@Geom", feature.Geometry.AsText());
                            cmdGeom.Parameters.AddWithValue("@Name", feature["NAME"]);
                            cmdGeom.Parameters.AddWithValue("@Srid", shapeFile.SRID);
                        }
                        else
                        {
                            cmdGeom.Parameters[0].Value = feature.Geometry.AsText();
                            cmdGeom.Parameters[1].Value = feature["NAME"];
                        }
                        cmdGeom.ExecuteNonQuery();

                        if (cmdGeog.Parameters.Count == 0)
                        {
                            cmdGeog.Parameters.AddWithValue("@Geog", feature.Geometry.AsText());
                            cmdGeog.Parameters.AddWithValue("@Name", feature["NAME"]);
                            cmdGeog.Parameters.AddWithValue("@Srid", geogSrid);
                        }
                        else
                        {
                            cmdGeog.Parameters[0].Value = feature.Geometry.AsText();
                            cmdGeog.Parameters[1].Value = feature["NAME"];
                        }
                        cmdGeog.ExecuteNonQuery();
                    }

                    cmdGeom.Dispose();
                    cmdGeog.Dispose();
                }

                // Create spatial indexes
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE SPATIAL INDEX [IX_roads_ugl_GEOM] ON [dbo].[roads_ugl_geom](GEOM) USING GEOMETRY_GRID WITH (BOUNDING_BOX =(-98, 40, -82, 50), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM))";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE SPATIAL INDEX [IX_roads_ugl_GEOG] ON [dbo].[roads_ugl_geog](GEOG)";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [TestFixtureTearDown]
        public void TearDownTestFixture()
        {
            SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder(UnitTests.Properties.Settings.Default.SqlServer2008);
            if (string.IsNullOrEmpty(connStrBuilder.DataSource) || string.IsNullOrEmpty(connStrBuilder.InitialCatalog))
            {
                return;
            }

            // Drop sample table
            using (SqlConnection conn = new SqlConnection(UnitTests.Properties.Settings.Default.SqlServer2008))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DROP TABLE roads_ugl_geom";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DROP TABLE roads_ugl_geog";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //private SharpMap.Data.Providers.SqlServer2008 GetTestProvider()
        //{
        //    return GetTestProvider(SqlServerSpatialObjectType.Geometry);
        //}

        private SharpMap.Data.Providers.SqlServer2008 GetTestProvider(SqlServerSpatialObjectType spatialType)
        {
            switch (spatialType)
            {
                case SqlServerSpatialObjectType.Geography:
                    // NB note forcing WGS84
                    return new SharpMap.Data.Providers.SqlServer2008(UnitTests.Properties.Settings.Default.SqlServer2008,
                        "roads_ugl_geog", "GEOG", "ID", spatialType, geogSrid, SqlServer2008ExtentsMode.QueryIndividualFeatures);
                default:
                    return new SharpMap.Data.Providers.SqlServer2008(UnitTests.Properties.Settings.Default.SqlServer2008,
                        "roads_ugl_geom", "GEOM", "ID", spatialType, 0, SqlServer2008ExtentsMode.QueryIndividualFeatures);
            }
        }

        private SharpMap.Data.Providers.SqlServer2008Ex GetTestProviderEx(SqlServerSpatialObjectType spatialType)
        {
            switch (spatialType)
            {
                case SqlServerSpatialObjectType.Geography:
                    // NB note forcing WGS84
                    return new SharpMap.Data.Providers.SqlServer2008Ex(UnitTests.Properties.Settings.Default.SqlServer2008,
                        "roads_ugl_geog", "GEOG", "ID", spatialType, geogSrid, SqlServer2008ExtentsMode.EnvelopeAggregate);
                default:
                    return new SharpMap.Data.Providers.SqlServer2008Ex(UnitTests.Properties.Settings.Default.SqlServer2008,
                        "roads_ugl_geom", "GEOM", "ID", spatialType, 0, SqlServer2008ExtentsMode.EnvelopeAggregate);
            }
        }

        /// <summary>
        /// Get the envelope of the entire roads_ugl file
        /// </summary>
        private GeoAPI.Geometries.Envelope GetTestEnvelope()
        {
            return SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse("POLYGON ((-97.23724071609665 41.698023105763589, -82.424263624596563 41.698023105763589, -82.424263624596563 49.000629000758515, -97.23724071609665 49.000629000758515, -97.23724071609665 41.698023105763589))").EnvelopeInternal;
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetExtentsQueryIndividualFeatures(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.QueryIndividualFeatures;
            GeoAPI.Geometries.Envelope extents = sq.GetExtents();

            Assert.IsNotNull(extents);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetExtentsSpatialIndex(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            if (spatialType == SqlServerSpatialObjectType.Geography)
            {
                var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.SpatialIndex;
                });
            }
            else
            {
                sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.SpatialIndex;
                GeoAPI.Geometries.Envelope extents = sq.GetExtents();

                Assert.IsNotNull(extents);
            }
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetExtentsEnvelopeAggregate(SqlServerSpatialObjectType spatialType)
        {
            using (SqlConnection conn = new SqlConnection(UnitTests.Properties.Settings.Default.SqlServer2008))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT SERVERPROPERTY('productversion')";
                    string productversion = (string)cmd.ExecuteScalar();
                    if (Version.Parse(productversion).Major < 11)
                    {
                        Assert.Ignore("Requires SQL Server 2012 connection");
                    }
                }
            }

            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.EnvelopeAggregate;
            GeoAPI.Geometries.Envelope extents = sq.GetExtents();

            Assert.IsNotNull(extents);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetGeometriesInView(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetGeometriesInViewDefinitionQuery(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.DefinitionQuery = "NAME LIKE 'A%'";

            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.LessOrEqual(geometries.Count, 100);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetGeometriesInViewNOLOCK(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.NoLockHint = true;
            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetGeometriesInViewFORCESEEK(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.ForceSeekHint = true;
            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry, "IX_roads_ugl_GEOM")]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography, "IX_roads_ugl_GEOG")]
        public void TestGetGeometriesInViewFORCEINDEX(SqlServerSpatialObjectType spatialType, string indexName)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.ForceIndex = indexName;
            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry, "IX_roads_ugl_GEOM")]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography, "IX_roads_ugl_GEOG")]
        public void TestGetGeometriesInViewAllHints(SqlServerSpatialObjectType spatialType, string indexName)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.NoLockHint = true;
            sq.ForceSeekHint = true;
            sq.ForceIndex = indexName;
            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetGeometriesInViewEx(SqlServerSpatialObjectType spatialType)
        {
            // Note:
            // This test may fail with an InvalidCastException. This is caused by multiple versions of the 
            // Microsoft.SqlServer.Types assembly being available (e.g. SQL 2008 and 2012).
            // This can be solved with a <bindingRedirect> in the .config file.
            // http://connect.microsoft.com/SQLServer/feedback/details/685654/invalidcastexception-retrieving-sqlgeography-column-in-ado-net-data-reader

            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProviderEx(spatialType);

            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count);
        }

        [NUnit.Framework.Test()]
        [Ignore("Do not run performance test by default, because it might fail because of external factors (busy CPU).")]
        public void TestPerformanceSqlServer2008ExProvider()
        {
            // Note:
            // This test may fail with an InvalidCastException. This is caused by multiple versions of the 
            // Microsoft.SqlServer.Types assembly being available (e.g. SQL 2008 and 2012).
            // This can be solved with a <bindingRedirect> in the .config file.

            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(SqlServerSpatialObjectType.Geography);
            SharpMap.Data.Providers.SqlServer2008 sqex = GetTestProviderEx(SqlServerSpatialObjectType.Geography);
            GeoAPI.Geometries.Envelope envelope = GetTestEnvelope();
            List<TimeSpan> measurements = new List<TimeSpan>(200);
            List<TimeSpan> measurementsex = new List<TimeSpan>(200);
            System.Diagnostics.Stopwatch timer;

            // 10 "startup" runs, followed by 200 measured runs
            for (int i = -10; i < 200; i++)
            {
                timer = System.Diagnostics.Stopwatch.StartNew();
                sq.GetGeometriesInView(envelope);
                timer.Stop();
                if (i >= 0) measurements.Add(timer.Elapsed);

                timer = System.Diagnostics.Stopwatch.StartNew();
                sqex.GetGeometriesInView(envelope);
                timer.Stop();
                if (i >= 0) measurementsex.Add(timer.Elapsed);
            }

            // Remove 10 slowest and 10 fastest times:
            measurements = measurements.OrderBy(x => x).Skip(10).Take(measurements.Count - 20).ToList();
            measurementsex = measurementsex.OrderBy(x => x).Skip(10).Take(measurementsex.Count - 20).ToList();

            // Average time:
            TimeSpan avg = TimeSpan.FromTicks((long)measurements.Average(x => x.Ticks));
            TimeSpan avgex = TimeSpan.FromTicks((long)measurementsex.Average(x => x.Ticks));

            // The SqlServer2008Ex provider should be faster:
            Assert.Less(avgex, avg);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetObjectIDsInView(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            var objectIds = sq.GetObjectIDsInView(GetTestEnvelope());

            Assert.IsNotNull(objectIds);
            Assert.AreEqual(100, objectIds.Count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestExecuteIntersectionQuery(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();

            sq.ExecuteIntersectionQuery(GetTestEnvelope(), ds);

            Assert.AreEqual(100, ds.Tables[0].Rows.Count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry, "IX_roads_ugl_GEOM")]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography, "IX_roads_ugl_GEOG")]
        public void TestExecuteIntersectionQueryAllHints(SqlServerSpatialObjectType spatialType, string indexName)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.NoLockHint = true;
            sq.ForceSeekHint = true;
            sq.ForceIndex = indexName;

            SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();

            sq.ExecuteIntersectionQuery(GetTestEnvelope(), ds);

            Assert.AreEqual(100, ds.Tables[0].Rows.Count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetFeatureCount(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            int count = sq.GetFeatureCount();

            Assert.AreEqual(100, count);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetFeatureCountWithDefinitionQuery(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            sq.DefinitionQuery = "NAME LIKE 'A%'";

            int count = sq.GetFeatureCount();

            Assert.LessOrEqual(count, 100);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetFeature(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            var feature = sq.GetFeature(1);

            Assert.IsNotNull(feature);
        }

        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geometry)]
        [NUnit.Framework.TestCase(SqlServerSpatialObjectType.Geography)]
        public void TestGetFeatureNonExisting(SqlServerSpatialObjectType spatialType)
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider(spatialType);

            var feature = sq.GetFeature(99999999);

            Assert.IsNull(feature);
        }
    }
}
