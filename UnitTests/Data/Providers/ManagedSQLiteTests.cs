//#if !LINUX
using System;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Distance;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    [NUnit.Framework.TestFixture, NUnit.Framework.Category("KnownToFailOnTeamCityAtCodebetter")]
    public class ManagedSQLiteTests : ProviderTest
    {
//#if LINUX
//        [NUnit.Framework.OneTimeSetUp]
//        public void OneTimeSetUp()
//        {
//            if (Environment.OSVersion.Platform == PlatformID.Unix)
//            {
//                if (!File.Exists("libSQLite.Interop.so"))
//                    throw new IgnoreException($"'libSQLite.Interop.so' not present");
//            }
//        }
//#endif

        private string GetTestDBPath()
        {
            return $"Data Source={TestUtility.GetPathToTestFile("test-2.3.sqlite")};";
        }

        private SharpMap.Data.Providers.ManagedSpatiaLite CreateProvider(string tableName)
        {
            return new SharpMap.Data.Providers.ManagedSpatiaLite(GetTestDBPath(), tableName, "Geometry", "PK_UID");
        }

        [NUnit.Framework.Test]
        public void TestCaseSensitiveKeyColumn()
        {
            var path = Path.GetTempFileName();
            try
            {
                var sqlite = new System.Data.SQLite.SQLiteConnection("Data Source=" + path);
                sqlite.Open();
                var cmd = sqlite.CreateCommand();
                cmd.CommandText = "create table test(col_ID integer primary key, name text, shape blob)";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                sqlite.Close();
                sqlite.Dispose();
                using (var sq = new ManagedSpatiaLite("Data Source=" + path, "test", "shape", "COL_ID"))
                {
                    var ext = new Envelope();
                    var ds = new SharpMap.Data.FeatureDataSet();
                    sq.ExecuteIntersectionQuery(ext, ds);
                    NUnit.Framework.Assert.AreEqual(0, ds.Tables[0].Count);
                }

            }
            catch (Exception ex)
            {
                Assert.Fail("Got exception, should not happen.\n{0}\n{1}", ex.Message, ex.StackTrace);

            }
            finally
            {
                File.Delete(path);
            }

        }

        [NUnit.Framework.Test]
        public void TestReadPoints()
        {
            using (var sq = CreateProvider("towns"))
            {
                var ext = sq.GetExtents();
                var ds = new SharpMap.Data.FeatureDataSet();
                sq.ExecuteIntersectionQuery(ext, ds);
                NUnit.Framework.Assert.AreEqual(8101, ds.Tables[0].Count);
            }

        }
        [NUnit.Framework.Test]
        public void TestReadLines()
        {
            using (var sq = CreateProvider("highways"))
            {
                var ext = sq.GetExtents();
                var ds = new SharpMap.Data.FeatureDataSet();
                sq.ExecuteIntersectionQuery(ext, ds);
                NUnit.Framework.Assert.AreEqual(775, ds.Tables[0].Count);

                var env = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(sq.SRID).ToGeometry(ext);
                var dsEnv = new SharpMap.Data.FeatureDataSet();
                sq.ExecuteIntersectionQuery(env, dsEnv);
                NUnit.Framework.Assert.AreEqual(775, dsEnv.Tables[0].Count);
            }
        }

        [NUnit.Framework.Test]
        public void TestReadPolys()
        {
            using (var sq = 
                new SharpMap.Data.Providers.ManagedSpatiaLite(GetTestDBPath(), "regions", "Geometry", "ROWID"))
            {
                var ext = sq.GetExtents();
                var ds = new SharpMap.Data.FeatureDataSet();
                sq.ExecuteIntersectionQuery(ext, ds);
                NUnit.Framework.Assert.AreEqual(109, ds.Tables[0].Count);
            }
        }

        [NUnit.Framework.Test]
        public void TestExecuteIntersectionQueryRegion53WithTowns()
        {
            using (var sq = CreateProvider("regions"))
            {
                var g = sq.GetGeometryByID(53);
                var env = g.EnvelopeInternal;

                using (var sqPoints = CreateProvider("towns"))
                {
                    var featureCount = sqPoints.GetFeatureCount();

                    sqPoints.UseSpatiaLiteIndex = true;
                    var fds = new SharpMap.Data.FeatureDataSet();
                    NUnit.Framework.Assert.DoesNotThrow(() => sqPoints.ExecuteIntersectionQuery(env, fds));
                    NUnit.Framework.Assert.AreEqual(1, fds.Tables.Count);
                    var table1 = fds.Tables[0];
                    NUnit.Framework.Assert.AreEqual("towns", table1.TableName);
                    NUnit.Framework.Assert.IsTrue(table1.Rows.Count > 0);
                    NUnit.Framework.Assert.IsTrue(featureCount > table1.Rows.Count);

                    fds = new SharpMap.Data.FeatureDataSet();
                    NUnit.Framework.Assert.DoesNotThrow(() => sqPoints.ExecuteIntersectionQuery(g, fds));
                    NUnit.Framework.Assert.AreEqual(1, fds.Tables.Count);
                    var table2 = fds.Tables[0];
                    NUnit.Framework.Assert.AreEqual("towns", table2.TableName);

                    NUnit.Framework.Assert.IsTrue(table2.Rows.Count > 0);
                    NUnit.Framework.Assert.IsTrue(featureCount > table2.Rows.Count);
                    NUnit.Framework.Assert.IsTrue(table1.Rows.Count > table2.Rows.Count);

                }
            }
        }
    }
}
//#endif
