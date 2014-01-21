using System.Drawing.Imaging;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Web.Wms.Exceptions;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;

namespace UnitTests.WMS.Server
{
    public class GetMapFixture : AbstractFixture
    {
        [Test]
        public void request_generates_valid_image()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("FALSE");
                    Expect.Call(req.GetParam("BGCOLOR")).Return(null);
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                    GetMapResponse img = (GetMapResponse)resp;
                    ImageCodecInfo codec = img.CodecInfo;
                    Assert.That(codec, Is.Not.Null);
                    Assert.That(codec.MimeType, Is.EqualTo("image/png"));
                    Assert.That(img.Image, Is.Not.Null);
                });
        }

        [Test]
        public void request_generates_transparent_image()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("TRUE");
                    DoNotExpect.Call(req.GetParam("BGCOLOR"));
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                    GetMapResponse img = (GetMapResponse)resp;
                    ImageCodecInfo codec = img.CodecInfo;
                    Assert.That(codec, Is.Not.Null);
                    Assert.That(codec.MimeType, Is.EqualTo("image/png"));
                    Assert.That(img.Image, Is.Not.Null);
                });
        }

        [Test]
        public void bgcolor_html_known_name()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("FALSE");
                    Expect.Call(req.GetParam("BGCOLOR")).Return("lightgrey");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                });
        }

        [Test]
        public void bgcolor_hex_format()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("FALSE");
                    Expect.Call(req.GetParam("BGCOLOR")).Return("#CFCFCF");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);                    
                });
        }

        [Test, ExpectedException(typeof(WmsInvalidParameterException))]
        public void bgcolor_rgb_not_supported()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("FALSE");
                    Expect.Call(req.GetParam("BGCOLOR")).Return("rgb(0,10,20)");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                });
        }

        [Test, ExpectedException(typeof(WmsInvalidParameterException))]
        public void bgcolor_rgba_not_supported()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("FALSE");
                    Expect.Call(req.GetParam("BGCOLOR")).Return("rgba(0,10,20,0)");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                });
        }

        [Test, ExpectedException(typeof(WmsInvalidParameterException))]
        public void bgcolor_must_be_valid()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("FALSE");
                    Expect.Call(req.GetParam("BGCOLOR")).Return("invalid");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                });
        }

        [Test, ExpectedException(typeof(WmsLayerNotDefinedException))]
        public void layer_names_must_be_valid()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_err,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("35,65,45,75");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("TRUE");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                });
        }

        [Test, ExpectedException(typeof(WmsInvalidBboxException))]
        public void bbox_must_be_valid()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("45,75,35,65"); // min-max flipped!
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("TRANSPARENT")).Return("TRUE");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                });
        }

        [Test, ExpectedException(typeof(WmsParameterNotSpecifiedException))]
        public void version_is_mandatory()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() => Expect.Call(req.GetParam("VERSION")).Return(null))
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                });
        }

        [Test, ExpectedException(typeof(WmsInvalidParameterException))]
        public void only_wms_130_is_supported()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() => Expect.Call(req.GetParam("VERSION")).Return("1.1.1"))
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                });
        }

        [Test, ExpectedException(typeof(WmsInvalidCRSException))]
        public void crs_support_is_checked()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks");
                    Expect.Call(req.GetParam("STYLES")).Return("WMS");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:900913");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetMap(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetMapResponse>(resp);
                });
        }
    }
}