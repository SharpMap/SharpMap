using System;
using System.Drawing;
using System.Web;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMap.Web.Wms;

namespace UnitTests.Layers
{
    [TestFixture]
    public class WmsLayerTest
    {
        [Test(Description = "BgColor produces a correct url when used with a named color")]
        public void BgColor_NamedColor_SetUrlPropertyCorrectly()
        {
            var client = CreateClientFromXmlResult("wms.atlantedesertificazione.xml");

            var layer = new WmsLayer("wms", client) {BgColor = Color.Red, SRID = 0, Transparent = false};

            var url = layer.GetRequestUrl(new Envelope(1, 1, 1, 1), new Size(1, 1));
            var queryString = HttpUtility.ParseQueryString(url);

            Assert.That(queryString["BGCOLOR"], Is.EqualTo("#FF0000"), 
                "Layer.BgColor does not produce a valid Url");
        }

        private Client CreateClientFromXmlResult(string xmlFilename)
        {
            var resourceName = typeof (WmsLayerTest).Namespace + "." + xmlFilename;
            var resourceStream = typeof(WmsLayerTest).Assembly.GetManifestResourceStream(resourceName);
            Assert.That(resourceStream, Is.Not.Null, "Wrong test: Invalid resource name");

            var buffer = new byte[resourceStream.Length];
            resourceStream.Read(buffer, 0, Convert.ToInt32(resourceStream.Length));

            var client = new Client(buffer);
            return client;
        }
    }
}
