using System.Data.OleDb;
using SharpMap.Data.Providers;

namespace ExampleCodeSnippets
{
    [NUnit.Framework.TestFixture]
    public class NtsTests
    {
        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance =
                NetTopologySuite.NtsGeometryServices.Instance;
        }

        [NUnit.Framework.Test]
        public void TestDiscussionNtsAndBaffeled()
        {
            var reader = new NetTopologySuite.IO.WKTReader();
            var poly = reader.Read(
                @"POLYGON ((428999.76819468878 360451.93329044303, 428998.25517286535 360420.80827007542,
429023.1119599645 360406.75878171506, 429004.52340613387 360451.71714446822, 
429004.52340613387 360451.71714446822, 428999.76819468878 360451.93329044303))");

            var points = new System.Collections.Generic.List<GeoAPI.Geometries.IGeometry>(new []
                {
                    reader.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(429012.5, 360443.18)),
                    reader.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(429001.59, 360446.98)),
                    reader.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(429003.31, 360425.45)),
                    reader.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(429016.9, 360413.04))
                });

            var inside = new System.Collections.Generic.List<bool>(new[] {false, true, true, true});

            for (var i = 0; i < points.Count; i++)
            {
                NUnit.Framework.Assert.AreEqual(inside[i], poly.Intersects(points[i]));
            }

            var prepPoly = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(poly);
            for (var i = 0; i < points.Count; i++)
            {
                NUnit.Framework.Assert.AreEqual(inside[i], prepPoly.Intersects(points[i]));
            }
        }

        [NUnit.Framework.Test]
        public void TestDiscussionNtsAndBaffeledOleDb()
        {
            var reader = new NetTopologySuite.IO.WKTReader();
            var poly = reader.Read(
                @"POLYGON ((428999.76819468878 360451.93329044303, 428998.25517286535 360420.80827007542,
429023.1119599645 360406.75878171506, 429004.52340613387 360451.71714446822, 
429004.52340613387 360451.71714446822, 428999.76819468878 360451.93329044303))");

            var table = WriteCsv();

            var p = new DbPoint(OleDbFactory.Instance, 
                "Provider=" + Properties.Settings.Default.OleDbProvider + ";Data Source=\"" + System.IO.Path.GetTempPath() + "\";" +
                "Extended Properties=\"text;HDR=Yes;FMT=Delimited\"", table, "ID", "X", "Y");

            var extents = p.GetExtents();
            NUnit.Framework.Assert.AreEqual(4, p.GetFeatureCount());
            p.DefinitionQuery = "Name='One'";
            NUnit.Framework.Assert.AreEqual(1, p.GetFeatureCount());
            var fdr = p.GetFeature(1);
            NUnit.Framework.Assert.AreEqual(1, (int)fdr[0]);
            p.DefinitionQuery = string.Empty;

            var fds = new SharpMap.Data.FeatureDataSet();
            p.ExecuteIntersectionQuery(extents, fds);
            NUnit.Framework.Assert.AreEqual(1, fds.Tables.Count);
            NUnit.Framework.Assert.AreEqual(4, fds.Tables[0].Rows.Count);
            fds.Tables.Clear();

            p.ExecuteIntersectionQuery(poly, fds);
            NUnit.Framework.Assert.AreEqual(1, fds.Tables.Count);
            NUnit.Framework.Assert.AreEqual(3, fds.Tables[0].Rows.Count);

            var inside = new System.Collections.Generic.List<bool>(new[] { false, true, true, true });
            NUnit.Framework.Assert.AreEqual(System.Linq.Enumerable.Count(inside, (b) => b == true), fds.Tables[0].Rows.Count);

            var ext = p.GetExtents();
            var oids = p.GetObjectIDsInView(ext);
            NUnit.Framework.Assert.AreEqual(4, oids.Count);
            
            System.IO.File.Delete(System.IO.Path.Combine(System.IO.Path.GetTempPath(), table));
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
                sr.WriteLine("1;One;{0};{1}", 429012.5,360443.18);
                sr.WriteLine("2;Two;{0};{1}",429001.59,360446.98);
                sr.WriteLine("3;Three;{0};{1}",429003.31,360425.45);
                sr.WriteLine("4;Four;{0};{1}",429016.9,360413.04);
            }

            return System.IO.Path.GetFileName(filename);
        }
    }
}
