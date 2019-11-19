using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    public class OleDbPointProviderTest : ProviderTest
    {
        private string _tableName;

        public override void OneTimeSetUp()
        {
            if (System.IntPtr.Size == 8)
                Assert.Ignore("Only run in 32bit mode, because most Excel installations are 32bit.");
            base.OneTimeSetUp();

            try
            {
                // Check if the OLE DB provider is available
                Microsoft.Win32.RegistryKey registryKey =
                    Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(Properties.Settings.Default.OleDbProvider);
                if (registryKey != null)
                {
                    registryKey.Close();
                }
                else
                {
                    Assert.Ignore("OLE DB provider " + Properties.Settings.Default.OleDbProvider + " is not found.");
                }
            }
            catch (System.Security.SecurityException)
            {
                Assert.Ignore($"Can't query if {Properties.Settings.Default.OleDbProvider} is installed.");
            }

            _tableName = WriteCsv();
        }

        private static string WriteCsv()
        {
            var filename = System.IO.Path.GetTempFileName();
            filename = System.IO.Path.ChangeExtension(filename, ".csv");

            var schemaFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "schema.ini");
            if (System.IO.File.Exists(schemaFile)) System.IO.File.Delete(schemaFile);
            using (var sr = new System.IO.StreamWriter(System.IO.File.OpenWrite(schemaFile)))
            {
                sr.WriteLine("[{0}]", System.IO.Path.GetFileName(filename));
                sr.WriteLine("Format=Delimited(;)");
            }

            using (var sr = new System.IO.StreamWriter(System.IO.File.OpenWrite(filename)))
            {
                sr.WriteLine("ID;Name;X;Y");
                sr.WriteLine("1;One;{0};{1}", 429012.5, 360443.18);
                sr.WriteLine("2;Two;{0};{1}", 429001.59, 360446.98);
                sr.WriteLine("3;Three;{0};{1}", 429003.31, 360425.45);
                sr.WriteLine("4;Four;{0};{1}", 429016.9, 360413.04);
            }

            return System.IO.Path.GetFileName(filename);
        }

        private OleDbPoint CreateProvider()
        {
            var p = new OleDbPoint(
                "Provider=" + Properties.Settings.Default.OleDbProvider + ";Data Source=\"" + System.IO.Path.GetTempPath() + "\";" +
                "Extended Properties=\"text;HDR=Yes;FMT=Delimited\"", _tableName, "ID", "X", "Y");

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
                Assert.AreEqual(3, (int)feature[p.ObjectIdColumn]);
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
                Assert.AreEqual(_tableName, table.TableName);
                Assert.AreEqual(4, table.Rows.Count);
            }
        }

        [Test]
        public void TestExecuteIntersectionQueryAgainstEnvelopeEqualsGetOidsInView()
        {
            using (var p = CreateProvider())
            {
                var ext = p.GetExtents();
                var fds = new FeatureDataSet();
                Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(ext, fds));
                Assert.AreEqual(1, fds.Tables.Count);
                var table = fds.Tables[0];
                Assert.AreEqual(_tableName, table.TableName);
                Assert.AreEqual(4, table.Rows.Count);

                var oids = p.GetObjectIDsInView(ext);

                Assert.AreEqual(table.Rows.Count, oids.Count);
                foreach (FeatureDataRow row in table.Select())
                    Assert.IsTrue(oids.Contains((uint)(int)row[0]));
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
                Assert.AreEqual(_tableName, table.TableName);
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
                Assert.AreEqual(_tableName, table.TableName);
                Assert.AreEqual(3, table.Rows.Count);
            }
        }
    }
}
