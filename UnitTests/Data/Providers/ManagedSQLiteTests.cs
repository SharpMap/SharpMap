using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace UnitTests.Data.Providers
{
    [TestFixture, Category("KnownToFailOnTeamCityAtCodebetter")]
    public class ManagedSQLiteTests
    {
        private string GetTestDBPath()
        {
            return "Data Source=" + Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.CodeBase.Replace("file:///", "")), @"TestData\test-2.3.sqlite");
        }
        [TestFixtureSetUp]
        public void SetupFixture()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        [Test]
        public void TestReadPoints()
        {
            SharpMap.Data.Providers.ManagedSpatiaLite sq = new SharpMap.Data.Providers.ManagedSpatiaLite(GetTestDBPath(), "towns", "Geometry", "ROWID");
            var ext = sq.GetExtents();
            var ds = new SharpMap.Data.FeatureDataSet();
            sq.ExecuteIntersectionQuery(ext, ds);
            Assert.AreEqual(8101, ds.Tables[0].Count);

        }
        [Test]
        public void TestReadLines()
        {
            SharpMap.Data.Providers.ManagedSpatiaLite sq = new SharpMap.Data.Providers.ManagedSpatiaLite(GetTestDBPath(), "highways", "Geometry", "ROWID");
            var ext = sq.GetExtents();
            var ds = new SharpMap.Data.FeatureDataSet();
            sq.ExecuteIntersectionQuery(ext, ds);
            Assert.AreEqual(775, ds.Tables[0].Count);

        }

        [Test]
        public void TestReadPolys()
        {
            SharpMap.Data.Providers.ManagedSpatiaLite sq = new SharpMap.Data.Providers.ManagedSpatiaLite(GetTestDBPath(), "regions", "Geometry", "ROWID");
            var ext = sq.GetExtents();
            var ds = new SharpMap.Data.FeatureDataSet();
            sq.ExecuteIntersectionQuery(ext, ds);
            Assert.AreEqual(109, ds.Tables[0].Count);

        }
    }
}
