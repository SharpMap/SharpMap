using System;
using System.Drawing;
using System.Linq;
using System.Web;
using NetTopologySuite.Geometries;
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

            Assert.That(queryString["BGCOLOR"], Is.EqualTo("FF0000"), 
                "Layer.BgColor does not produce a valid Url");
        }

        [Test(Description = "BgColor output when set or not")]
        public void BgColor_written_when_ne_white()
        {
            var client = CreateClientFromXmlResult("wms.atlantedesertificazione.xml");

            var layer = new WmsLayer("wms", client) { SRID = 0, Transparent = false, BgColor = Color.White };
            var url = layer.GetRequestUrl(new Envelope(1, 1, 1, 1), new Size(1, 1));
            var queryString = HttpUtility.ParseQueryString(url);
            Assert.That(queryString.AllKeys.Contains("BGCOLOR"), Is.False, "Layer.BgColor is white and shall not be in request url");

            layer = new WmsLayer("wms", client) { SRID = 0, Transparent = true, BgColor = Color.Green };
            url = layer.GetRequestUrl(new Envelope(1, 1, 1, 1), new Size(1, 1));
            queryString = HttpUtility.ParseQueryString(url);
            Assert.That(queryString.AllKeys.Contains("BGCOLOR"), Is.True, "Layer.BgColor set to non-white");
            Assert.That(queryString["BGCOLOR"], Is.EqualTo("008000"), "Layer.BgColor translated correctly");
        }

        [Test]
        public void ContactInfo_ContactOrganization_IsParsedCorrectly()
        {
            var client = CreateClientFromXmlResult("wms.atlantedesertificazione.xml");

            Assert.That(client.ServiceDescription.ContactInformation.PersonPrimary.Organisation,
                Is.EqualTo("Geoportale Nazionale - Ministero dell'Ambiente e della Tutela del Territorio e del Mare"));
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
