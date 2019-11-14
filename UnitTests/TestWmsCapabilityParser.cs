using System;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMap.Web.Wms;

namespace UnitTests
{
    [TestFixture]
    public class TestWmsCapabilityParser
    {
        [Test]
        [Ignore("http://wms.iter.dk is no longer available")]
        public void AddLayerFail()
        {
            WmsLayer layer = new WmsLayer("wms", "http://wms.iter.dk/example_capabilities_1_3_0.xml");
            Assert.Throws<ArgumentException>(() => layer.AddLayer("NonExistingLayer") );
        }

        [Test]
        [Ignore("http://wms.iter.dk is no longer available")]
        public void AddLayerOK()
        {
            WmsLayer layer = new WmsLayer("wms", "http://wms.iter.dk/example_capabilities_1_3_0.xml");
            layer.AddLayer("ROADS_1M");
        }

        [Test]
        [Ignore("http://wms.iter.dk is no longer available")]
        public void Test130()
        {
            Client c = new Client("http://wms.iter.dk/example_capabilities_1_3_0.xml");
            Assert.AreEqual(3, c.ServiceDescription.Keywords.Length);
            Assert.AreEqual("1.3.0", c.Version);
            Assert.AreEqual("http://hostname/path?", c.GetMapRequests[0].OnlineResource);
            Assert.AreEqual("image/gif", c.GetMapOutputFormats[0]);
            Assert.AreEqual(4, c.Layer.ChildLayers.Length);
        }

        [Test, Ignore("Assumption false")]
        public void TestDemisv111()
        {
            Client c = new Client("http://www2.demis.nl/worldmap/wms.asp");
            Assert.AreEqual("World Map", c.ServiceDescription.Title);
            Assert.AreEqual("1.1.1", c.Version);
            Assert.AreEqual("http://www2.demis.nl/wms/wms.asp?wms=WorldMap&", c.GetMapRequests[0].OnlineResource);
            Assert.AreEqual("image/png", c.GetMapOutputFormats[0]);
            Assert.AreEqual(20, c.Layer.ChildLayers.Length);
        }
    }
}
