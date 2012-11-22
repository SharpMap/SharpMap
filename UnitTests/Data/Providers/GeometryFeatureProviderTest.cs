using System;
using System.Threading;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    public class GeometryFeatureProviderTest
    {
        [NUnit.Framework.SetUp]
        public void TestSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            using (var sf = new ShapeFile(GetTestFile()))
            {
                sf.Open();
                var fds = new FeatureDataSet();
                sf.ExecuteIntersectionQuery(sf.GetExtents(), fds);
                _provider = new GeometryFeatureProvider(fds.Tables[0]);
            }
        }

        private string GetTestFile()
        {
            return System.IO.Path.Combine(GetPathToTestDataDir(), "roads_ugl.shp");
        }
        
        private string GetPathToTestDataDir()
        {
            return System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(GetType().Assembly.CodeBase.Replace("file:///", "")), @"TestData\");
        }

        private GeometryFeatureProvider _provider;
        private readonly Random _rnd = new Random(47652);

        [NUnit.Framework.Test]
        public void TestConcurrentExecuteIntersectionQuerysDontThrowException()
        {
            const int count = 1000;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Parallel.For(0, count, DoExecuteIntersectionQuery);
            sw.Stop();

            Console.WriteLine("\n{0} ExecuteIntersectionQuery(Envelope, FeatureDataSet) performed in {1}ms", count, sw.ElapsedMilliseconds);

        }

        private void DoExecuteIntersectionQuery(int obj)
        {
            var ext = _provider.GetExtents();
            
            var minX = _rnd.Next((int)ext.MinX, (int)ext.MaxX);
            var maxX = _rnd.Next((int)minX, (int)ext.MaxX);
            var minY = _rnd.Next((int)ext.MinY, (int)ext.MaxY);
            var maxY = _rnd.Next((int)minY, (int)ext.MaxY);
            var box = new Envelope(minX, maxX, minY, maxY);

            Console.WriteLine(@"{0:000}/{2:00}: Executing intersection query agains {1}", obj, box, Thread.CurrentThread.ManagedThreadId);
            var fds = new FeatureDataSet();
            _provider.ExecuteIntersectionQuery(box, fds);
            var table = fds.Tables[0];
            var count = table != null ? table.Rows.Count : 0;
            Console.WriteLine(@"{0:000}/{3:00}: Executed intersection query agains {1} returned {2} features", obj, box, count, Thread.CurrentThread.ManagedThreadId);
            
        }
    }
}