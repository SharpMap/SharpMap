using System;
using System.Diagnostics;
using GeoAPI;
using NUnit.Framework;

namespace UnitTests
{
    [SetUpFixture]
    public class UnitTestsFixture
    {
        private Stopwatch _stopWatch;

        [SetUp]
        public void RunBeforeAnyTests()
        {
            GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            
            _stopWatch = new Stopwatch();
            Console.WriteLine("Starting tests");
            _stopWatch.Start();
        }

        [TearDown]
        public void RunAfterAllTests()
        {
            _stopWatch.Stop();
            Console.WriteLine("All tests accomplished in {0}ms", _stopWatch.ElapsedMilliseconds);
        }
    }
}