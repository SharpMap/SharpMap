using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Utilities.Indexing;

namespace UnitTests.Data.Providers
{
    public class ShapeFileWithMemoryCacheThreadingTest : ThreadingTest
    {
        private static string TestDataPath 
        {
            get => TestUtility.GetPathToTestFile("roads_ugl.shp");
        }

        public ShapeFileWithMemoryCacheThreadingTest()
            : base(new ShapeFile(TestDataPath, false, true))
        {
        }

        [Test, Description("Simulates two threads using the same provider at the same time.")]
        public void TestTwoOpenClose()
        {
            //Simulates two threads using the same provider at the same time..
            var provider = new ShapeFile(TestDataPath, false, true);
            provider.Open();
            provider.Open();
            provider.GetGeometriesInView(GetRandomEnvelope());
            provider.Close();
            provider.GetGeometriesInView(GetRandomEnvelope());
            provider.Close();
        }

        [Test, Description("Simulates two threads using the datasource with different providers at the same time.")]
        public void TestTwoThreadsUsingDifferentProviders()
        {
            var provider1 = new ShapeFile(TestDataPath, false, true);
            var provider2 = new ShapeFile(TestDataPath, false, true);
            provider1.Open();
            provider2.Open();
            provider1.GetGeometriesInView(GetRandomEnvelope());
            provider1.Close();
            provider2.GetGeometriesInView(GetRandomEnvelope());
            provider2.Close();
        }

    }

    public class ShapeFileThreadingTest : ThreadingTest
    {
        internal static string TestDataPath
        {
            get { return TestUtility.GetPathToTestFile("roads_ugl.shp"); }
        }

        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            ShapeFile.SpatialIndexFactory = new SharpSbnIndexFactory();
        }

        public ShapeFileThreadingTest()
            : base(new ShapeFile(TestDataPath, true, false))
        {
        }

        [Test, Description("Simulates two threads using the same provider at the same time.")]
        public void TestTwoOpenClose()
        {
            //Simulates two threads using the same provider at the same time..
            var provider = new ShapeFile(TestDataPath, true, false);
            provider.Open();
            provider.Open();
            provider.GetGeometriesInView(GetRandomEnvelope());
            provider.Close();
            provider.GetGeometriesInView(GetRandomEnvelope());
            provider.Close();
        }

        [Test, Description("Simulates two threads using the datasource with different providers at the same time.")]
        public void TestTwoThreadsUsingDifferentProviders()
        {
            var provider1 = new ShapeFile(TestDataPath, true, false);
            var provider2 = new ShapeFile(TestDataPath, true, false);
            provider1.Open();
            provider2.Open();
            provider1.GetGeometriesInView(GetRandomEnvelope());
            provider1.Close();
            provider2.GetGeometriesInView(GetRandomEnvelope());
            provider2.Close();
        }

    }

    //[Ignore("Only run if you have a proper postgis connection")]
    public class PostGisThreadingTest : ThreadingTest
    {
        public PostGisThreadingTest()
            : base(Serialization.ProviderTest.CreateProvider<PostGIS>())
        {
        }
    }

#if LINUX
    [Ignore("Only run if you have a proper ManagedSpatiaLite connection")]
#endif
    public class ManagedSpatiaLiteThreadingTest : ThreadingTest
    {
        public ManagedSpatiaLiteThreadingTest()
            : base(Serialization.ProviderTest.CreateProvider<ManagedSpatiaLite>())
        {
        }
    }

    [Ignore("Only run if you have a proper sqlserver2008 connection")]
    public class SqlSever2008ThreadingTest : ThreadingTest
    {
        public SqlSever2008ThreadingTest()
            : base(Serialization.ProviderTest.CreateProvider<SqlServer2008>())
        {
        }
    }

    [TestFixture]
    public abstract class ThreadingTest
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(ThreadingTest));
        
        static ThreadingTest ()
        {
            GeoAPI.GeometryServiceProvider.Instance =
                NetTopologySuite.NtsGeometryServices.Instance;
        }

        private const int NumberOfTests = 200;
        private readonly IProvider _provider;
        private readonly Envelope _extents;
        private readonly Random _rnd = new Random(6584);

        private readonly List<bool> _testsRun = new List<bool>(NumberOfTests);
        private readonly List<Exception> _testsFailed = new List<Exception>(NumberOfTests);

        protected ThreadingTest(IProvider provider)
        {
            _provider = provider;
            _provider.Open();
            _extents = provider.GetExtents();
            _provider.Close();
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
        }

        protected Envelope GetRandomEnvelope()
        {
            var minX = NextRandom(-0.1, 0.5, _extents.MinX, _extents.Width);
            var minY = NextRandom(-0.1, 0.5, _extents.MinY, _extents.Height);
            var maxX = NextRandom(-0.5, 0.1, _extents.MaxX, _extents.Width);
            var maxY = NextRandom(-0.5, 0.1, _extents.MaxY, _extents.Height);

            return new Envelope(minX, maxX, minY, maxY);
        }

        private double NextRandom(double min, double max, double start, double amount)
        {
            var d = max - min;
            return start + min * amount + d*amount*_rnd.NextDouble();
        }

        [Test]
        public void TestGetGeometriesInView()
        {
            Assert.DoesNotThrow(() => RunTest(0));
        }

        [Test]
        public void TestExecuteIntersectionQueryEnvelope()
        {
            Assert.DoesNotThrow(() => RunTest(1));
        }

        [Test]
        public void TestExecuteIntersectionQueryGeometry()
        {
            Assert.DoesNotThrow(() => RunTest(2));
        }

        [Test]
        public void TestMixed()
        {
            Assert.DoesNotThrow(() => RunTest(3));
        }

        public void RunTest(int kind)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            _provider.Open();

            _testsRun.Clear();
            _testsFailed.Clear();

            for (var i = 0; i < NumberOfTests; i++)
            {
                _testsRun.Add(false);
                _testsFailed.Add(null);

                switch (kind)
                {
                    case 0:
                        ThreadPool.QueueUserWorkItem(ExecuteGetGeometriesInView, i);
                        break;
                    case 1:
                        ThreadPool.QueueUserWorkItem(ExecuteExecuteFeatureQueryEnvelope, i);
                        break;
                    case 2:
                        ThreadPool.QueueUserWorkItem(ExecuteExecuteFeatureQueryGeometry, i);
                        break;
                    case 3:
                        switch (_rnd.Next(0, 3))
                        {
                            case 0:
                                ThreadPool.QueueUserWorkItem(ExecuteGetGeometriesInView, i);
                                break;
                            case 1:
                                ThreadPool.QueueUserWorkItem(ExecuteExecuteFeatureQueryEnvelope, i);
                                break;
                            case 2:
                                ThreadPool.QueueUserWorkItem(ExecuteExecuteFeatureQueryGeometry, i);
                                break;
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            WaitAllRun();

            _provider.Close();

            foreach (var exception in _testsFailed)
            {
                if (exception != null)
                    throw exception;
            }
            sw.Stop();
            System.Diagnostics.Trace.WriteLine($"\nTest performed in {sw.ElapsedMilliseconds}ms");
        }

        private readonly object _runLock = new object();

        private void SignalRun(int id)
        {
            lock (_runLock)
            {
                _testsRun[id] = true;
            }
        }

        private void WaitAllRun()
        {
            while (true)
            {
                Thread.Sleep(2500);
                lock (_runLock)
                {
                    var allRun = true;
                    foreach (var b in _testsRun)
                    {
                        if (!b)
                        {
                            allRun = false;
                            break;
                        }
                    }
                    if (allRun) break;
                }
            }
        }

        private void ExecuteGetGeometriesInView(object arguments)
        {
            var env = GetRandomEnvelope();
            System.Diagnostics.Trace.WriteLine(string.Format(
                "Thread {0}: {2}. GetGeometriesInView({1})", Thread.CurrentThread.ManagedThreadId, env, arguments));
            try
            {
                var geoms = _provider.GetGeometriesInView(env);
                System.Diagnostics.Trace.WriteLine(string.Format(
                    "Thread {0}: {2}.  {1} geometries", Thread.CurrentThread.ManagedThreadId, geoms.Count, arguments));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(
                    "Thread {0}: {1}. failed:\n{2}", Thread.CurrentThread.ManagedThreadId, arguments, ex.Message));
                _testsFailed[(int)arguments] = ex;
            }
            
            SignalRun((int)arguments);
        }
        private void ExecuteExecuteFeatureQueryEnvelope(object arguments)
        {
            var env = GetRandomEnvelope();
            System.Diagnostics.Trace.WriteLine(string.Format(
                "Thread {0}:  {2}. ExecuteFeatureQuery with ({1})", Thread.CurrentThread.ManagedThreadId, env, arguments));
            try
            {
                var fds = new FeatureDataSet();
                _provider.ExecuteIntersectionQuery(env, fds);
                System.Diagnostics.Trace.WriteLine(string.Format(
                    "Thread {0}:  {2}. {1} features", Thread.CurrentThread.ManagedThreadId, fds.Tables[0].Count, arguments));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(
                    "Thread {0}: {1}. failed:\n{2}", Thread.CurrentThread.ManagedThreadId, arguments, ex.Message));
                _testsFailed[(int)arguments] = ex;
            }

            SignalRun((int)arguments);
        }
        private void ExecuteExecuteFeatureQueryGeometry(object arguments)
        {
            var env = GetRandomEnvelope();
            var geom = GeometryFactory.Default.ToGeometry(env);
            System.Diagnostics.Trace.WriteLine(string.Format(
                "Thread {0}:  {2}. ExecuteFeatureQuery with ({1})", Thread.CurrentThread.ManagedThreadId, geom, arguments));
            try
            {
                var fds = new FeatureDataSet();
                _provider.ExecuteIntersectionQuery(geom, fds);
                System.Diagnostics.Trace.WriteLine(string.Format(
                    "Thread {0}:  {2}. {1} features", Thread.CurrentThread.ManagedThreadId, fds.Tables[0].Count, arguments));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(
                    "Thread {0}: {1}. failed:\n{2}", Thread.CurrentThread.ManagedThreadId, arguments, ex.Message));
                _testsFailed[(int)arguments] = ex;
            }

            SignalRun((int)arguments);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _provider.Dispose();
        }

    }
}
