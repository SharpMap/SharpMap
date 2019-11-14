using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    [Category("RequiresWindows")]
    public class DbPointProviderTest : ProviderTest
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
                    "CREATE TABLE DbPointProviderTest(ID integer primary key, Name text, X real, Y real);";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "INSERT INTO DbPointProviderTest(ID, Name, X, Y) VALUES" +
                    "   (1, 'One', 429012.5, 360443.18)," +
                    "   (2, 'Two', 429001.59, 360446.98)," +
                    "   (3, 'Three', 429003.31, 360425.45)," +
                    "   (4, 'Four', 429016.9, 360413.04)";
                cmd.ExecuteNonQuery();
            }

            return conn;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _connection.Close();
        }

        private DbPoint CreateProvider()
        {
            var p = new DbPoint(System.Data.SQLite.SQLiteFactory.Instance,
                "FullUri=file::memory:?cache=shared;ToFullPath=false", "DbPointProviderTest", "ID", "X", "Y");

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
                Assert.AreEqual(4, numFeatures);
                p.DefinitionQuery = "Name='Two'";
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
                    new Coordinate(429003.31, 360425.45));
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
                    new Coordinate(429003.31, 360425.45));
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
                Assert.AreEqual(4, table.Rows.Count);
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
                Assert.AreEqual(4, table.Rows.Count);

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
                Assert.AreEqual(4, table.Rows.Count);

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
                    @"POLYGON ((428999.76819468878 360451.93329044303, 428998.25517286535 360420.80827007542,
429023.1119599645 360406.75878171506, 429004.52340613387 360451.71714446822, 
429004.52340613387 360451.71714446822, 428999.76819468878 360451.93329044303))");

                var fds = new FeatureDataSet();
                Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(poly, fds));
                Assert.AreEqual(1, fds.Tables.Count);
                var table = fds.Tables[0];
                Assert.AreEqual(3, table.Rows.Count);
            }
        }
    }
}
