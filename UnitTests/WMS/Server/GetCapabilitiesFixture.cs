using System;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Web.Wms.Exceptions;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;
using UnitTests.Properties;

namespace UnitTests.WMS.Server
{
    public class GetCapabilitiesFixture : AbstractFixture
    {
        [Test]
        public void request_generates_valid_document()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("SERVICE")).Return("WMS");
                    Expect.Call(req.Url).Return(new Uri(Desc.OnlineResource)).Repeat.AtLeastOnce();
                    Expect.Call(req.Encode(Desc.OnlineResource)).Return(Desc.OnlineResource);
                })
                .Verify(() =>
                {
                    IHandler handler = new GetCapabilities(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetCapabilitiesResponse>(resp);
                    Validate((GetCapabilitiesResponse)resp);
                });
        }

        private static void Validate(GetCapabilitiesResponse cap)
        {
            Assert.That(cap.ContentType, Is.EqualTo("text/xml"));
            XmlDocument doc = cap.Capabilities;
            Assert.That(doc, Is.Not.Null);
            XmlElement el = doc.DocumentElement;
            Assert.That(el.OuterXml, Is.EqualTo(Resources.expectedGetCapabilitiesXml));
        }

        [Test]
        public void version_param_is_optional()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return(null);
                    Expect.Call(req.GetParam("SERVICE")).Return("WMS");
                    Expect.Call(req.Url).Return(new Uri(Desc.OnlineResource)).Repeat.AtLeastOnce();
                    Expect.Call(req.Encode(Desc.OnlineResource)).Return(Desc.OnlineResource);
                })
                .Verify(() =>
                {
                    IHandler handler = new GetCapabilities(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetCapabilitiesResponse>(resp);
                    Validate((GetCapabilitiesResponse)resp);
                });
        }

        [Test, ExpectedException(typeof(WmsParameterNotSpecifiedException))]
        public void service_param_is_mandatory()
        {
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
                    IHandler handler = new GetCapabilities(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                });
        }

        [Test, ExpectedException(typeof(WmsOperationNotSupportedException))]
        public void only_wms_130_is_supported()
        {
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
                    IHandler handler = new GetCapabilities(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                });
        }
    }
}
