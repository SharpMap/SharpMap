using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    public class ShapeFileWithMemoryCacheThreadingTest : ThreadingTest
    {
        private struct tmp {}

        private static string TestDataPath 
        {
            get
            {
                var t = new tmp();
                var codeBase = Path.GetDirectoryName(t.GetType().Assembly.CodeBase);
                return Path.Combine(new Uri(codeBase).LocalPath, "TestData", "roads_ugl.shp");
            }
        }

        public ShapeFileWithMemoryCacheThreadingTest()
            : base(new ShapeFile(TestDataPath, false, true))
        {
        }
    }

    public class ShapeFileThreadingTest : ThreadingTest
    {
        private struct tmp { }

        internal static string TestDataPath
        {
            get
            {
                var t = new tmp();
                var codeBase = Path.GetDirectoryName(t.GetType().Assembly.CodeBase);
                return Path.Combine(new Uri(codeBase).LocalPath, "TestData", "roads_ugl.shp");
            }
        }

        public ShapeFileThreadingTest()
            : base(new ShapeFile(TestDataPath, false, false))
        {
        }
    }

    [Ignore("Only run if you have a proper postgis connection")]
    public class PostGisThreadingTest : ThreadingTest
    {
        public PostGisThreadingTest()
            : base(Serialization.ProviderTest.CreateProvider<PostGIS>())
        {
        }
    }

    [Ignore("Only run if you have a proper ManagedSpatiaLite connection")]
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
        static ThreadingTest ()
        {
            GeoAPI.GeometryServiceProvider.Instance =
                NetTopologySuite.NtsGeometryServices.Instance;
        }

        private const int NumberOfTests = 500;
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

        private Envelope GetRandomEnvelope()
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
                    Assert.IsTrue(false);
            }
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

        [Test]
        public void TestTwoOpenClose()
        {
            ///Simulates two threads using the same provider at the same time..
            var provider = new ShapeFile(ShapeFileThreadingTest.TestDataPath, false, false);
            provider.Open();
            provider.Open();
            provider.GetGeometriesInView(GetRandomEnvelope());
            provider.Close();
            provider.GetGeometriesInView(GetRandomEnvelope());
            provider.Close();
        }

        [Test]
        public void TestTwoThreadsUsingDifferentProviders()
        {
            ///Simulates two threads using the same provider at the same time..
            var provider1 = new ShapeFile(ShapeFileThreadingTest.TestDataPath, false, false);
            var provider2 = new ShapeFile(ShapeFileThreadingTest.TestDataPath, false, false);
            provider1.Open();
            provider2.Open();
            provider1.GetGeometriesInView(GetRandomEnvelope());
            provider1.Close();
            provider2.GetGeometriesInView(GetRandomEnvelope());
            provider2.Close();
        }

        private void ExecuteGetGeometriesInView(object arguments)
        {
            var env = GetRandomEnvelope();
            Console.WriteLine("Tread {0}: {2}. GetGeometriesInView({1})", Thread.CurrentThread.ManagedThreadId, env, arguments);
            try
            {
                var geoms = _provider.GetGeometriesInView(env);
                Console.WriteLine("Tread {0}: {2}.  {1} geometries", Thread.CurrentThread.ManagedThreadId, geoms.Count, arguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Tread {0}: {1}. failed:\n{2}", Thread.CurrentThread.ManagedThreadId, arguments, ex.Message);
                _testsFailed[(int)arguments] = ex;
            }
            
            SignalRun((int)arguments);
        }
        private void ExecuteExecuteFeatureQueryEnvelope(object arguments)
        {
            var env = GetRandomEnvelope();
            Console.WriteLine("Tread {0}:  {2}. ExecuteFeatureQuery with ({1})", Thread.CurrentThread.ManagedThreadId, env, arguments);
            try
            {
                var fds = new FeatureDataSet();
                _provider.ExecuteIntersectionQuery(env, fds);
                Console.WriteLine("Tread {0}:  {2}. {1} features", Thread.CurrentThread.ManagedThreadId, fds.Tables[0].Count, arguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Tread {0}: {1}. failed:\n{2}", Thread.CurrentThread.ManagedThreadId, arguments, ex.Message);
                _testsFailed[(int)arguments] = ex;
            }

            SignalRun((int)arguments);
        }
        private void ExecuteExecuteFeatureQueryGeometry(object arguments)
        {
            var env = GetRandomEnvelope();
            var geom = GeometryFactory.Default.ToGeometry(env);
            Console.WriteLine("Tread {0}:  {2}. ExecuteFeatureQuery with ({1})", Thread.CurrentThread.ManagedThreadId, geom, arguments);
            try
            {
                var fds = new FeatureDataSet();
                _provider.ExecuteIntersectionQuery(geom, fds);
                Console.WriteLine("Tread {0}:  {2}. {1} features", Thread.CurrentThread.ManagedThreadId, fds.Tables[0].Count, arguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Tread {0}: {1}. failed:\n{2}", Thread.CurrentThread.ManagedThreadId, arguments, ex.Message);
                _testsFailed[(int)arguments] = ex;
            }

            SignalRun((int)arguments);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

    }
}