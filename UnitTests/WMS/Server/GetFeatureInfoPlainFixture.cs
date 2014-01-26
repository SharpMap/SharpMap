using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;

namespace UnitTests.WMS.Server
{
    public class GetFeatureInfoPlainFixture : AbstractFixture
    {
        [Test]
        public void request_generates_valid_plain()
        {
            const string expectedPlain = "GetFeatureInfo results:\nLayer: \'poly_landmarks\'\nFeatureinfo:\n\'52\' \'76\' \'D65\' \'City Hall\',\n\'47\' \'69\' \'H11\' \'Hudson River\'\nLayer: \'tiger_roads\'\nFeatureinfo:\n\'7664\' \'A41\' \'Broadway\',\n\'7667\' \'A41\' \'Broadway\',\n\'6016\' \'A41\' \'Barclay St\'\nSearch returned no results on layer: poi\n";

            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.689903,-74.02474,40.724235,-73.98955");
                    Expect.Call(req.GetParam("WIDTH")).Return("800");
                    Expect.Call(req.GetParam("HEIGHT")).Return("820");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("QUERY_LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("INFO_FORMAT")).Return("text/plain");
                    Expect.Call(req.GetParam("X")).Return(null);
                    Expect.Call(req.GetParam("I")).Return("378");
                    Expect.Call(req.GetParam("Y")).Return(null);
                    Expect.Call(req.GetParam("J")).Return("288");
                    Expect.Call(req.GetParam("FEATURE_COUNT")).Return("10");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetFeatureInfoPlain(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetFeatureInfoResponsePlain>(resp);
                    GetFeatureInfoResponsePlain plain = (GetFeatureInfoResponsePlain)resp;
                    string contentType = plain.ContentType;
                    Assert.That(contentType, Is.Not.Null);
                    Assert.That(contentType, Is.EqualTo("text/plain"));
                    string charset = plain.Charset;
                    Assert.That(charset, Is.Not.Null);
                    Assert.That(charset, Is.EqualTo("utf-8"));
                    Assert.That(plain.Response, Is.EqualTo(expectedPlain));
                });
        }
    }
}