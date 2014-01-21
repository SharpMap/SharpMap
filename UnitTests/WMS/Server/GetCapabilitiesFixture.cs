using System;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Web.Wms;
using SharpMap.Web.Wms.Exceptions;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;
using UnitTests.Properties;

namespace UnitTests.WMS.Server
{
    [TestFixture]
    public class GetCapabilitiesFixture
    {
        [Test]
        public void request_generates_valid_document()
        {
            Capabilities.WmsServiceDescription desc = Helper.Description();
            Assert.That(desc, Is.Not.Null);
            Map map = Helper.Default();
            Assert.That(map, Is.Not.Null);

            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("SERVICE")).Return("WMS");
                    Expect.Call(req.Url).Return(new Uri(desc.OnlineResource)).Repeat.AtLeastOnce();
                    Expect.Call(req.Encode(desc.OnlineResource)).Return(desc.OnlineResource);
                })
                .Verify(() =>
                {
                    IHandler handler = new GetCapabilities(desc);
                    IHandlerResponse resp = handler.Handle(map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetCapabilitiesResponse>(resp);
                    GetCapabilitiesResponse cap = (GetCapabilitiesResponse)resp;
                    Assert.That(cap.ContentType, Is.EqualTo("text/xml"));
                    XmlDocument doc = cap.Capabilities;
                    Assert.That(doc, Is.Not.Null);
                    Assert.That(doc.DocumentElement.OuterXml, Is.EqualTo(Resources.expectedGetCapabilitiesXml));
                });
        }

        [Test]
        public void version_param_is_optional()
        {
            Capabilities.WmsServiceDescription desc = Helper.Description();
            Assert.That(desc, Is.Not.Null);
            Map map = Helper.Default();
            Assert.That(map, Is.Not.Null);

            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return(null);
                    Expect.Call(req.GetParam("SERVICE")).Return("WMS");
                    Expect.Call(req.Url).Return(new Uri(desc.OnlineResource)).Repeat.AtLeastOnce();
                    Expect.Call(req.Encode(desc.OnlineResource)).Return(desc.OnlineResource);
                })
                .Verify(() =>
                {
                    IHandler handler = new GetCapabilities(desc);
                    IHandlerResponse resp = handler.Handle(map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetCapabilitiesResponse>(resp);
                    GetCapabilitiesResponse cap = (GetCapabilitiesResponse)resp;
                    Assert.That(cap.ContentType, Is.EqualTo("text/xml"));
                    XmlDocument doc = cap.Capabilities;
                    Assert.That(doc, Is.Not.Null);
                    Assert.That(doc.DocumentElement.OuterXml, Is.EqualTo(Resources.expectedGetCapabilitiesXml));
                });
        }

        [Test, ExpectedException(typeof(WmsParameterNotSpecifiedException))]
        public void service_param_is_mandatory()
        {
            Capabilities.WmsServiceDescription desc = Helper.Description();
            Assert.That(desc, Is.Not.Null);
            Map map = Helper.Default();
            Assert.That(map, Is.Not.Null);

            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("SERVICE")).Return(null);
                })
                .Verify(() =>
                {
                    IHandler handler = new GetCapabilities(desc);
                    IHandlerResponse resp = handler.Handle(map, req);
                    Assert.That(resp, Is.Not.Null);
                });
        }

        [Test, ExpectedException(typeof(WmsOperationNotSupportedException))]
        public void only_wms_130_is_supported()
        {
            Capabilities.WmsServiceDescription desc = Helper.Description();
            Assert.That(desc, Is.Not.Null);
            Map map = Helper.Default();
            Assert.That(map, Is.Not.Null);

            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.1.1");
                    Expect.Call(req.GetParam("SERVICE")).Return(null);
                })
                .Verify(() =>
                {
                    IHandler handler = new GetCapabilities(desc);
                    IHandlerResponse resp = handler.Handle(map, req);
                    Assert.That(resp, Is.Not.Null);
                });
        }
    }
}
