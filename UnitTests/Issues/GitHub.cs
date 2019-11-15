using System;
using System.Net;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace UnitTests.Issues
{
    public class GitHub
    {
        [TestCase("https://www.wms.nrw.de/geobasis/wms_nw_dop", SecurityProtocolType.Ssl3, SecurityProtocolType.Tls12)]
        [Description("https request with unmatching security protocol type")]
        public void TestIssue156(string url, SecurityProtocolType sptFail, SecurityProtocolType sptSucceed)
        {
            var spDefault = ServicePointManager.SecurityProtocol;

            SharpMap.Layers.WmsLayer wmsLayer = null;
            ServicePointManager.SecurityProtocol = sptFail;
            Assert.That(() => wmsLayer = new SharpMap.Layers.WmsLayer("WMSFAIL", url), Throws.InstanceOf<ApplicationException>() );
            ServicePointManager.SecurityProtocol = sptSucceed;
            Assert.That(() => wmsLayer = new SharpMap.Layers.WmsLayer("WMSSUCCED", url), Throws.Nothing);
            Assert.That(wmsLayer, Is.Not.Null, "wmsLayer null");

            ServicePointManager.SecurityProtocol = spDefault;
        }
    }
}
