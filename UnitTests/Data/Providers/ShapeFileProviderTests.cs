using System.IO;
using NUnit.Framework;
namespace UnitTests.Data.Providers
{

    [NUnit.Framework.TestFixture]
    public class ShapeFileProviderTests
    {
        private long _msLineal;
        private long _msVector;
        private const int NumberOfRenderCycles = 1;

        [NUnit.Framework.TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }

        [NUnit.Framework.TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            System.Console.WriteLine("Speed comparison:");
            System.Console.WriteLine("VectorLayer\tLinealLayer\tRatio");
            System.Console.WriteLine(string.Format("{0}\t{1}\t{2:N}", _msVector, _msLineal,
                                                   ((double)_msLineal / _msVector * 100)));
        }


        private string GetTestFile()
        {
            return Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.CodeBase.Replace("file:///", "")), @"TestData\roads_ugl.shp");
        }

        [NUnit.Framework.Test]
        public void TestPerformanceVectorLayer()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(GetTestFile()),
                                          "Specified shapefile is not present!");

            var map = new SharpMap.Map(new System.Drawing.Size(1024, 768));

            var shp = new SharpMap.Data.Providers.ShapeFile(GetTestFile(), false, false);
            var lyr = new SharpMap.Layers.VectorLayer("Roads", shp);

            map.Layers.Add(lyr);
            map.ZoomToExtents();

            RepeatedRendering(map, shp.GetFeatureCount(), NumberOfRenderCycles, out _msVector);

            var res = map.GetMap();
            var path = System.IO.Path.ChangeExtension(GetTestFile(), ".vector.png");
            res.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            System.Console.WriteLine("\nResult saved at file://" + path.Replace('\\', '/'));
        }

        [NUnit.Framework.Test]
        public void TestPerformanceLinealLayer()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(GetTestFile()),
                                          "Specified shapefile is not present!");

            var map = new SharpMap.Map(new System.Drawing.Size(1024, 768));

            var shp = new SharpMap.Data.Providers.ShapeFile(GetTestFile(), false, false);
            var lyr = new SharpMap.Layers.Symbolizer.LinealVectorLayer("Roads", shp)
                          {
                              Symbolizer =
                                  new SharpMap.Rendering.Symbolizer.BasicLineSymbolizer
                                      {Line = new System.Drawing.Pen(System.Drawing.Color.Black)}
                          };
            map.Layers.Add(lyr);
            map.ZoomToExtents();

            RepeatedRendering(map, shp.GetFeatureCount(), NumberOfRenderCycles, out _msLineal);
            System.Console.WriteLine("\nWith testing if record is deleted ");
            
            shp.CheckIfRecordIsDeleted = true;
            long tmp;
            RepeatedRendering(map, shp.GetFeatureCount(), 1, out tmp);

            var res = map.GetMap();
            var path = System.IO.Path.ChangeExtension(GetTestFile(), "lineal.png");
            res.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            System.Console.WriteLine("\nResult saved at file://" + path.Replace('\\', '/'));
        }

        private static void RepeatedRendering(SharpMap.Map map, int numberOfFeatures, int numberOfTimes, out long avgRenderTime)
        {
            System.Console.WriteLine("Rendering Map with " + numberOfFeatures + " features");
            var totalRenderTime = 0L;
            var sw = new System.Diagnostics.Stopwatch();
            for (var i = 1; i <= numberOfTimes; i++)
            {
                System.Console.Write(string.Format("Rendering {0}x time(s)", i));
                sw.Start();
                map.GetMap();
                sw.Stop();
                System.Console.WriteLine(" in " +
                                         sw.ElapsedMilliseconds.ToString(
                                             System.Globalization.NumberFormatInfo.CurrentInfo) + "ms.");
                totalRenderTime += sw.ElapsedMilliseconds;
                sw.Reset();
            }

            avgRenderTime = totalRenderTime/numberOfTimes;
            System.Console.WriteLine("\n Average rendering time:" + avgRenderTime.ToString(
                                         System.Globalization.NumberFormatInfo.CurrentInfo) + "ms.");
        }

        [NUnit.Framework.Test]
        public void TestGetFeature()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(GetTestFile()),
                                          "Specified shapefile is not present!");

            var shp = new SharpMap.Data.Providers.ShapeFile(GetTestFile(), false, false);
            shp.Open();
            var feat = shp.GetFeature(0);
            Assert.IsNotNull(feat);
            shp.Close();
        }

        [NUnit.Framework.Test]
        public void TestExecuteIntersectionQuery()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(GetTestFile()),
                                          "Specified shapefile is not present!");

            var shp = new SharpMap.Data.Providers.ShapeFile(GetTestFile(), false, false);
            shp.Open();

            var fds = new SharpMap.Data.FeatureDataSet();
            var bbox = shp.GetExtents();
            //narrow it down
            bbox.ExpandBy(-0.425*bbox.Width, -0.425*bbox.Height);

            //Just to avoid that initial query does not impose performance penalty
            shp.DoTrueIntersectionQuery = false;
            shp.ExecuteIntersectionQuery(bbox, fds);
            fds.Tables.RemoveAt(0);

            //Perform query once more
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            shp.ExecuteIntersectionQuery(bbox, fds);
            sw.Stop();
            System.Console.WriteLine("Queried using just envelopes:\n" + fds.Tables[0].Rows.Count + " in " +
                                     sw.ElapsedMilliseconds + "ms.");
            fds.Tables.RemoveAt(0);
            
            shp.DoTrueIntersectionQuery = true;
            sw.Reset();
            sw.Start();
            shp.ExecuteIntersectionQuery(bbox, fds);
            sw.Stop();
            System.Console.WriteLine("Queried using prepared geometries:\n" + fds.Tables[0].Rows.Count + " in " +
                                     sw.ElapsedMilliseconds + "ms.");
        }

        [NUnit.Framework.Test]
        public void TestExecuteIntersectionQueryWithFilterDelegate()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(GetTestFile()),
                                          "Specified shapefile is not present!");

            var shp = new SharpMap.Data.Providers.ShapeFile(GetTestFile(), false, false);
            shp.Open();

            var fds = new SharpMap.Data.FeatureDataSet();
            var bbox = shp.GetExtents();
            //narrow it down
            bbox.ExpandBy(-0.425*bbox.Width, -0.425*bbox.Height);

            //Just to avoid that initial query does not impose performance penalty
            shp.DoTrueIntersectionQuery = false;
            shp.FilterDelegate = JustTracks;

            shp.ExecuteIntersectionQuery(bbox, fds);
            fds.Tables.RemoveAt(0);

            //Perform query once more
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            shp.ExecuteIntersectionQuery(bbox, fds);
            sw.Stop();
            System.Console.WriteLine("Queried using just envelopes:\n" + fds.Tables[0].Rows.Count + " in " +
                                     sw.ElapsedMilliseconds + "ms.");
            fds.Tables.RemoveAt(0);
            
            shp.DoTrueIntersectionQuery = true;
            sw.Reset();
            sw.Start();
            shp.ExecuteIntersectionQuery(bbox, fds);
            sw.Stop();
            System.Console.WriteLine("Queried using prepared geometries:\n" + fds.Tables[0].Rows.Count + " in " +
                                     sw.ElapsedMilliseconds + "ms.");
        }

        public static bool JustTracks(SharpMap.Data.FeatureDataRow fdr)
        {
            //System.Console.WriteLine(fdr [0] + ";"+ fdr[4]);
            var s = fdr[4] as string;
            if (s != null)
                return s == "track";
            return true;
        }
    }
}