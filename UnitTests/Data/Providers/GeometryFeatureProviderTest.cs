using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests.Data.Providers
{
    public class GeometryFeatureProviderTest : ProviderTest
    {
        [NUnit.Framework.SetUp]
        public void TestSetUp()
        {
            using (var sf = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("roads_ugl.shp")))
            {
                sf.Open();
                var fds = new SharpMap.Data.FeatureDataSet();
                sf.ExecuteIntersectionQuery(sf.GetExtents(), fds);
                _provider = new SharpMap.Data.Providers.GeometryFeatureProvider(fds.Tables[0]);
            }
        }

        private SharpMap.Data.Providers.GeometryFeatureProvider _provider;
        private readonly Random _rnd = new Random(47652);

        [NUnit.Framework.Test]
        public void TestConcurrentExecuteIntersectionQuerysDontThrowException()
        {
            const int count = 1000;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Parallel.For(0, count, DoExecuteIntersectionQuery);
            sw.Stop();

            System.Diagnostics.Trace.WriteLine(String.Format(
                "\n{0} ExecuteIntersectionQuery(Envelope, FeatureDataSet) performed in {1}ms", count, sw.ElapsedMilliseconds));

        }

        private void DoExecuteIntersectionQuery(int obj)
        {
            var ext = _provider.GetExtents();
            
            var minX = _rnd.Next((int)ext.MinX, (int)ext.MaxX);
            var maxX = _rnd.Next((int)minX, (int)ext.MaxX);
            var minY = _rnd.Next((int)ext.MinY, (int)ext.MaxY);
            var maxY = _rnd.Next((int)minY, (int)ext.MaxY);
            var box = new GeoAPI.Geometries.Envelope(minX, maxX, minY, maxY);

            System.Diagnostics.Trace.WriteLine(string.Format(
                @"{0:000}/{2:00}: Executing intersection query agains {1}", 
                obj, box, Thread.CurrentThread.ManagedThreadId));
            var fds = new SharpMap.Data.FeatureDataSet();
            _provider.ExecuteIntersectionQuery(box, fds);
            var table = fds.Tables[0];
            var count = table != null ? table.Rows.Count : 0;
            System.Diagnostics.Trace.WriteLine(string.Format(
                @"{0:000}/{3:00}: Executed intersection query agains {1} returned {2} features", 
                obj, box, count, Thread.CurrentThread.ManagedThreadId));
            
        }

        [NUnit.Framework.Test]
        public void GetItemByID()
        {
            var fdt = new SharpMap.Data.FeatureDataTable();
            fdt.Columns.Add(new DataColumn("ID", typeof(uint)));

            var gf = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory();

            for (var i = 0; i < 5; i++)
            {
                var row = fdt.NewRow();
                row[0] = (uint)i;
                row.Geometry = gf.CreatePoint(new GeoAPI.Geometries.Coordinate(i, i));
                fdt.AddRow(row);
            }
            var layer = new SharpMap.Layers.VectorLayer("TMP", new SharpMap.Data.Providers.GeometryFeatureProvider(fdt));

            var res = ((SharpMap.Data.Providers.IProvider)layer.DataSource).GetFeature(0);
            NUnit.Framework.Assert.IsNotNull(res);
        }

        [NUnit.Framework.Test]
        public void TestFindGeoNearPoint()
        {
            var fdt = new SharpMap.Data.FeatureDataTable();
            fdt.Columns.Add(new DataColumn("ID", typeof(uint)));

            var gf = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory();

            for (var i = 0; i < 5; i++)
            {
                var row = fdt.NewRow();
                row[0] = (uint)i;
                row.Geometry = gf.CreatePoint(new GeoAPI.Geometries.Coordinate(i, i));
                fdt.AddRow(row);
            }
            var layer = new SharpMap.Layers.VectorLayer("TMP", new SharpMap.Data.Providers.GeometryFeatureProvider(fdt));

            var res = FindGeoNearPoint(gf.CreatePoint(new GeoAPI.Geometries.Coordinate(0.1, 0.1)), layer, 0.2d);
            NUnit.Framework.Assert.IsNotNull(res);
            NUnit.Framework.Assert.AreEqual(0, (uint)res[0]);
        }

public static SharpMap.Data.FeatureDataRow FindGeoNearPoint(
    GeoAPI.Geometries.IPoint point, SharpMap.Layers.VectorLayer layer, double amountGrow)
{
    var box = new GeoAPI.Geometries.Envelope(point.Coordinate);
    box.ExpandBy(amountGrow);

    var fds = new SharpMap.Data.FeatureDataSet();
    layer.DataSource.ExecuteIntersectionQuery(box, fds);

    SharpMap.Data.FeatureDataRow result = null;
    var minDistance = double.MaxValue;

    foreach (SharpMap.Data.FeatureDataTable fdt in fds.Tables)
    {
        foreach (SharpMap.Data.FeatureDataRow fdr in fdt.Rows)
        {
            if (fdr.Geometry != null)
            {
                var distance = point.Distance(fdr.Geometry);
                if (distance < minDistance)
                {
                    result = fdr;
                    minDistance = distance;
                }
            }
        }
    }
    return result;
}

    }
}
