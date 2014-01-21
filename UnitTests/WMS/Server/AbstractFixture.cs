using NUnit.Framework;
using SharpMap;
using SharpMap.Web.Wms;

namespace UnitTests.WMS.Server
{
    [TestFixture]
    public abstract class AbstractFixture
    {
        public Map Map { get; private set; }
        public Capabilities.WmsServiceDescription Desc { get; private set; }

        [TestFixtureSetUp]
        public void setup_fixture()
        {
            Desc = Helper.Description();
            Assert.That(Desc, Is.Not.Null);
            Map = Helper.Default();
            Assert.That(Map, Is.Not.Null);
        }

        [TestFixtureTearDown]
        public void teardown_fixture()
        {
            if (Map != null)
                Map.Dispose();
            Map = null;
        }
    }
}