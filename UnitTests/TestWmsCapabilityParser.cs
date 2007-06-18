using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class TestWmsCapabilityParser
	{
		[Test]
		public void Test130()
		{
			SharpMap.Web.Wms.Client c = new SharpMap.Web.Wms.Client("http://wms.iter.dk/example_capabilities_1_3_0.xml");
			Assert.AreEqual(3, c.ServiceDescription.Keywords.Length);
			Assert.AreEqual("1.3.0", c.WmsVersion);
			Assert.AreEqual("http://hostname/path?", c.GetMapRequests[0].OnlineResource);
			Assert.AreEqual("image/gif", c.GetMapOutputFormats[0]);
			Assert.AreEqual(4, c.Layer.ChildLayers.Length);
		}
		[Test]
		public void TestDemisv111()
		{
			SharpMap.Web.Wms.Client c = new SharpMap.Web.Wms.Client("http://www2.demis.nl/mapserver/request.asp");
			Assert.AreEqual("Demis World Map", c.ServiceDescription.Title);
			Assert.AreEqual("1.1.1", c.WmsVersion);
			Assert.AreEqual("http://www2.demis.nl/wms/wms.asp?wms=WorldMap&", c.GetMapRequests[0].OnlineResource);
			Assert.AreEqual("image/png", c.GetMapOutputFormats[0]);
			Assert.AreEqual(20, c.Layer.ChildLayers.Length);
		}

		[Test]
		public void AddLayerOK()
		{
			SharpMap.Layers.WmsLayer layer = new SharpMap.Layers.WmsLayer("wms", "http://wms.iter.dk/example_capabilities_1_3_0.xml");
			layer.AddLayer("ROADS_1M");
		}
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddLayerFail()
		{
			SharpMap.Layers.WmsLayer layer = new SharpMap.Layers.WmsLayer("wms", "http://wms.iter.dk/example_capabilities_1_3_0.xml");
			layer.AddLayer("NonExistingLayer");
		}
	}
}
