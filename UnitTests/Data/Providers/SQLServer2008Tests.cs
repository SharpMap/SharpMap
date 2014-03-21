using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.IO;

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

        [NUnit.Framework.Test()]
        public void VerifySchemaDetection()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = new SharpMap.Data.Providers.SqlServer2008("", "schema.TableName", "oidcolumn");
            Assert.AreEqual("schema", sq.TableSchema);
            Assert.AreEqual("TableName", sq.Table);
            Assert.AreEqual("oidcolumn", sq.ObjectIdColumn);
        }
    }

    [NUnit.Framework.TestFixture]
    public class SQLServer2008DbTests
    {
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

            // Set up sample table
            using (SqlConnection conn = new SqlConnection(UnitTests.Properties.Settings.Default.SqlServer2008))
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    // The ID column cannot simply be int, because that would cause GetOidsInView to fail. The provider internally works with uint
                    cmd.CommandText = "CREATE TABLE roads_ugl(ID decimal(10,0) identity(1,1) PRIMARY KEY, NAME nvarchar(100), GEOM geometry)";
                    cmd.ExecuteNonQuery();
                }
                
                // Load data
                using (SharpMap.Data.Providers.ShapeFile shapeFile = new SharpMap.Data.Providers.ShapeFile(GetTestFile()))
                {
                    shapeFile.Open();

                    IEnumerable<uint> indexes = shapeFile.GetOidsInView(shapeFile.GetExtents());

                    indexes = indexes.Take(100);

                    foreach (uint idx in indexes)
                    {
                        var feature = shapeFile.GetFeatureByOid(idx);

                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO roads_ugl(NAME, GEOM) VALUES (@Name, geometry::STGeomFromText(@Geom, @Srid))";

                            cmd.Parameters.AddWithValue("@Geom", feature.Geometry.AsText());
                            cmd.Parameters.AddWithValue("@Name", feature.Attributes["NAME"]);
                            cmd.Parameters.AddWithValue("@Srid", shapeFile.SRID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Create spatial index
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE SPATIAL INDEX [IX_roads_ugl_GEOM] ON [dbo].[roads_ugl](GEOM)USING  GEOMETRY_GRID WITH (BOUNDING_BOX =(-98, 40, -82, 50), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM))";
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
                    cmd.CommandText = "DROP TABLE roads_ugl";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private SharpMap.Data.Providers.SqlServer2008 GetTestProvider()
        {
            return new SharpMap.Data.Providers.SqlServer2008(UnitTests.Properties.Settings.Default.SqlServer2008, "roads_ugl", "GEOM", "ID");
        }

        private SharpMap.Data.Providers.SqlServer2008Ex GetTestProviderEx()
        {
            return new SharpMap.Data.Providers.SqlServer2008Ex(UnitTests.Properties.Settings.Default.SqlServer2008, "roads_ugl", "GEOM", "ID");
        }

        /// <summary>
        /// Get the envelope of the entire roads_ugl file
        /// </summary>
        private GeoAPI.Geometries.Envelope GetTestEnvelope()
        {
            return SharpMap.Converters.WellKnownText.GeometryFromWKT.Parse("POLYGON ((-97.23724071609665 41.698023105763589, -82.424263624596563 41.698023105763589, -82.424263624596563 49.000629000758515, -97.23724071609665 49.000629000758515, -97.23724071609665 41.698023105763589))").EnvelopeInternal;
        }

        [NUnit.Framework.Test()]
        public void TestGetExtentsQueryIndividualFeatures()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.QueryIndividualFeatures;
            GeoAPI.Geometries.Envelope extents = sq.GetExtents();

            Assert.IsNotNull(extents);
        }

        [NUnit.Framework.Test()]
        public void TestGetExtentsSpatialIndex()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.SpatialIndex;
            GeoAPI.Geometries.Envelope extents = sq.GetExtents();

            Assert.IsNotNull(extents);
        }

        [NUnit.Framework.Test()]
        public void TestGetExtentsEnvelopeAggregate()
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

            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.ExtentsMode = SharpMap.Data.Providers.SqlServer2008ExtentsMode.EnvelopeAggregate;
            GeoAPI.Geometries.Envelope extents = sq.GetExtents();

            Assert.IsNotNull(extents);
        }

        [NUnit.Framework.Test()]
        public void TestGetGeometriesInView()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count());
        }

        [NUnit.Framework.Test()]
        public void TestGetGeometriesInViewDefinitionQuery()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.DefinitionQuery = "NAME LIKE 'A%'";

            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.LessOrEqual(geometries.Count(), 100);
        }

        [NUnit.Framework.Test()]
        public void TestGetGeometriesInViewNOLOCK()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.NoLockHint = true;
            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count());
        }

        [NUnit.Framework.Test()]
        public void TestGetGeometriesInViewFORCESEEK()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.ForceSeekHint = true;
            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count());
        }

        [NUnit.Framework.Test()]
        public void TestGetGeometriesInViewFORCEINDEX()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.ForceIndex = "IX_roads_ugl_GEOM";
            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count());
        }

        [NUnit.Framework.Test()]
        public void TestGetGeometriesInViewAllHints()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.NoLockHint = true;
            sq.ForceSeekHint = true;
            sq.ForceIndex = "IX_roads_ugl_GEOM";
            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count());
        }

        [NUnit.Framework.Test()]
        public void TestGetGeometriesInViewEx()
        {
            // Note:
            // This test may fail with an InvalidCastException. This is caused by multiple versions of the 
            // Microsoft.SqlServer.Types assembly being available (e.g. SQL 2008 and 2012).
            // This can be solved with a <bindingRedirect> in the .config file.
            // http://connect.microsoft.com/SQLServer/feedback/details/685654/invalidcastexception-retrieving-sqlgeography-column-in-ado-net-data-reader

            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProviderEx();

            var geometries = sq.GetGeometriesInView(GetTestEnvelope());

            Assert.IsNotNull(geometries);
            Assert.AreEqual(100, geometries.Count());
        }

        [NUnit.Framework.Test()]
        public void TestGetObjectIDsInView()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            var objectIds = sq.GetOidsInView(GetTestEnvelope());

            Assert.IsNotNull(objectIds);
            Assert.AreEqual(100, objectIds.Count());
        }

        [NUnit.Framework.Test()]
        public void TestExecuteIntersectionQuery()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();

            sq.ExecuteIntersectionQuery(GetTestEnvelope(), ds);

            Assert.AreEqual(100, ds.Tables[0].Rows.Count);
        }

        [NUnit.Framework.Test()]
        public void TestExecuteIntersectionQueryAllHints()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.NoLockHint = true;
            sq.ForceSeekHint = true;
            sq.ForceIndex = "IX_roads_ugl_GEOM";

            SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();

            sq.ExecuteIntersectionQuery(GetTestEnvelope(), ds);

            Assert.AreEqual(100, ds.Tables[0].Rows.Count);
        }

        [NUnit.Framework.Test()]
        public void TestGetFeatureCount()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            int count = sq.GetFeatureCount();

            Assert.AreEqual(100, count);
        }

        [NUnit.Framework.Test()]
        public void TestGetFeatureCountWithDefinitionQuery()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            sq.DefinitionQuery = "NAME LIKE 'A%'";

            int count = sq.GetFeatureCount();

            Assert.LessOrEqual(count, 100);
        }

        [NUnit.Framework.Test()]
        public void TestGetFeature()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            var feature = sq.GetFeatureByOid(1);

            Assert.IsNotNull(feature);
        }

        [NUnit.Framework.Test()]
        public void TestGetFeatureNonExisting()
        {
            SharpMap.Data.Providers.SqlServer2008 sq = GetTestProvider();

            var feature = sq.GetFeatureByOid(99999999);

            Assert.IsNull(feature);
        }
    }
}
