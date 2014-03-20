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

        [SetUp]
        public void setup()
        {
            Desc = Helper.Description();
            Assert.That(Desc, Is.Not.Null);
            Map = Helper.Default();
            Assert.That(Map, Is.Not.Null);
        }

        [TearDown]
        public void teardown()
        {
            if (Map != null)
                Map.Dispose();
            Map = null;
        }        
    }
}