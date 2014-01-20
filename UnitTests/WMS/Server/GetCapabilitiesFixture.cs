using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Web.Wms;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;

namespace UnitTests.WMS.Server
{
    [TestFixture]
    public class GetCapabilitiesFixture
    {
        [Test]
        public void trying_to_build_a_test_that_at_least_starts()
        {
            Capabilities.WmsServiceDescription description = Helper.Description();
            Assert.That(description, Is.Not.Null);
            Map map = Helper.Default();
            Assert.That(map, Is.Not.Null);

            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("SERVICE")).Return("WMS");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetCapabilities(description);
                    IHandlerResponse response = handler.Handle(map, req);
                    Assert.That(response, Is.Not.Null);
                });
        }
    }
}
