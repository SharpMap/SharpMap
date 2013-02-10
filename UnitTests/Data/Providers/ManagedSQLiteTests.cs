namespace UnitTests.Data.Providers
{
    [NUnit.Framework.TestFixture, NUnit.Framework.Category("KnownToFailOnTeamCityAtCodebetter")]
    public class ManagedSQLiteTests : ProviderTest
    {
        private string GetTestDBPath()
        {
            return "Data Source=" + GetTestDataFilePath("test-2.3.sqlite") + ";";
        }

        [NUnit.Framework.Test]
        public void TestReadPoints()
        {
            using (var sq = 
                new SharpMap.Data.Providers.ManagedSpatiaLite(GetTestDBPath(), "towns", "Geometry", "ROWID"))
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
            using (var sq =
                new SharpMap.Data.Providers.ManagedSpatiaLite(GetTestDBPath(), "highways", "Geometry", "ROWID"))
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
    }
}
