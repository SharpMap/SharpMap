using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    [Category("RequiresWindows")]
    public class DbTwoPointLineProviderTest : ProviderTest
    {
        private System.Data.Common.DbConnection _connection; // Keep connection active; when closed, the in-memory database is dropped

        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            _connection = WriteSqlLite();
        }

        private static System.Data.Common.DbConnection WriteSqlLite()
        {
            // Set up sample table
            var conn = System.Data.SQLite.SQLiteFactory.Instance.CreateConnection();

            conn.ConnectionString = "FullUri=file::memory:?cache=shared;ToFullPath=false";
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "CREATE TABLE DbTwoPointLineProviderTest(ID integer primary key, Name text, X1 real, Y1 real, X2 real, Y2 real);";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "INSERT INTO DbTwoPointLineProviderTest(ID, Name, X1, Y1, X2, Y2) VALUES" +
                    "   (1, 'T1', 100, 100, 170, 100)," +
                    "   (2, 'T2', 135, 100, 135, 170)," +
                    "   (3, 'e1', 180, 130, 180, 170)," +
                    "   (4, 'e2', 180, 130, 210, 130)," +
                    "   (5, 'e3', 180, 150, 200, 150)," +
                    "   (6, 'e4', 180, 170, 210, 170)," +
                    "   (7, 's1', 220, 130, 250, 130)," +
                    "   (8, 's2', 220, 130, 220, 150)," +
                    "   (9, 's3', 220, 150, 250, 150)," +
                    "   (10, 's4', 250, 150, 250, 170)," +
                    "   (11, 's5', 250, 170, 220, 170)," +
                    "   (12, 't1', 260, 130, 290, 130)," +
                    "   (13, 't2', 275, 130, 275, 170)";
                cmd.ExecuteNonQuery();
            }

            return conn;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _connection.Close();
        }

        private DbTwoPointLine CreateProvider()
        {
            var p = new DbTwoPointLine(System.Data.SQLite.SQLiteFactory.Instance,
                "FullUri=file::memory:?cache=shared;ToFullPath=false", "DbTwoPointLineProviderTest", "ID", "X1", "Y1", "X2", "Y2");

            return p;

        }

        [Test]
        public void TestGetExtents()
        {
            using (var p = CreateProvider())
            {
                Envelope env = null;
                Assert.DoesNotThrow(() => env = p.GetExtents());
                Assert.IsNotNull(env);
                Assert.IsFalse(env.IsNull);
            }
        }

        [Test]
        public void TestGetFeatureCount()
        {
            using (var p = CreateProvider())
            {
                var numFeatures = 0;
                Assert.DoesNotThrow(() => numFeatures = p.GetFeatureCount());
                Assert.AreEqual(13, numFeatures);
                p.DefinitionQuery = "Name='e1'";
                Assert.DoesNotThrow(() => numFeatures = p.GetFeatureCount());
                Assert.AreEqual(1, numFeatures);
            }
        }

        [Test]
        public void TestGetFeature()
        {
            using (var p = CreateProvider())
            {
                FeatureDataRow feature = null;
                Assert.DoesNotThrow(() => feature = p.GetFeature(3));
                Assert.IsNotNull(feature);
                Assert.AreEqual(3, Convert.ToInt32(feature[p.ObjectIdColumn]));
                Assert.AreEqual(feature.Geometry.Centroid.Coordinate,
                    new Coordinate(180, 150));
            }
        }

        [Test]
        public void TestGetGeometryById()
        {
            using (var p = CreateProvider())
            {
                IGeometry feature = null;
                Assert.DoesNotThrow(() => feature = p.GetGeometryByID(3));
                Assert.IsNotNull(feature);
                Assert.AreEqual(feature.Centroid.Coordinate,
                    new Coordinate(180, 150));
            }
        }

        [Test]
        public void TestExecuteIntersectionQueryAgainstEnvelope()
        {
            using (var p = CreateProvider())
            {
                var fds = new FeatureDataSet();
                Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(p.GetExtents(), fds));
                Assert.AreEqual(1, fds.Tables.Count);
                var table = fds.Tables[0];
                Assert.AreEqual(13, table.Rows.Count);
            }
        }

        [Test]
        public void TestExecuteIntersectionQueryAgainstEnvelopeEqualsGetOidsInView()
        {
            using (var p = CreateProvider())
            {
                var ext =p.GetExtents();
                var fds = new FeatureDataSet();
                Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(ext, fds));
                Assert.AreEqual(1, fds.Tables.Count);
                var table = fds.Tables[0];
                Assert.AreEqual(13, table.Rows.Count);

                var oids = p.GetObjectIDsInView(ext);

                Assert.AreEqual(table.Rows.Count, oids.Count);
                foreach (FeatureDataRow row in table.Select())
                    Assert.IsTrue(oids.Contains(Convert.ToUInt32(row[0])));
            }
        }

        [Test]
        public void TestExecuteIntersectionQueryAgainstEnvelopeEqualsGetGeometriesInView()
        {
            using (var p = CreateProvider())
            {
                var ext = p.GetExtents();
                var fds = new FeatureDataSet();
                Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(ext, fds));
                Assert.AreEqual(1, fds.Tables.Count);
                var table = fds.Tables[0];
                Assert.AreEqual(13, table.Rows.Count);

                var geoms = p.GetGeometriesInView(ext);

                Assert.AreEqual(table.Rows.Count, geoms.Count);
                foreach (FeatureDataRow row in table.Select())
                    Assert.IsTrue(geoms.Contains(row.Geometry));
            }
        }

        [Test]
        public void TestExecuteIntersectionQueryAgainstGeometry()
        {
            using (var p = CreateProvider())
            {
                var reader = new NetTopologySuite.IO.WKTReader();
                var poly = reader.Read(
                    @"POLYGON ((100 100, 170 100, 170 170, 100 170, 100 100))");

                var fds = new FeatureDataSet();
                Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(poly, fds));
                Assert.AreEqual(1, fds.Tables.Count);
                var table = fds.Tables[0];
                Assert.AreEqual(2, table.Rows.Count);
            }
        }

        [Test]
        public void TestExecuteIntersectionQueryAgainstGeometryIntersectionButNoPointsWithin()
        {
            using (var p = CreateProvider())
            {
                var reader = new NetTopologySuite.IO.WKTReader();
                var poly = reader.Read(
                    @"POLYGON ((101 101, 169 101, 169 169, 101 169, 101 101))");

                var fds = new FeatureDataSet();
                Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(poly, fds));
                Assert.AreEqual(1, fds.Tables.Count);
                var table = fds.Tables[0];
                Assert.AreEqual(1, table.Rows.Count);
                Assert.AreEqual("T2", table.Rows[0][1].ToString());
            }
        }
    }
}
