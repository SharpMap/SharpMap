using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;

namespace UnitTests.WMS.Server
{
    public class GetFeatureInfoHtmlFixture : AbstractFixture
    {
        [Test]
        public void request_generates_valid_html()
        {
            const string expectedHtml = @"<html>
<head>
<title>GetFeatureInfo output</title>
</head>
<style type='text/css'>
  table.featureInfo, table.featureInfo td, table.featureInfo th {
  border:1px solid #ddd;
  border-collapse:collapse;
  margin:0;
  padding:0;
  font-size: 90%;
  padding:.2em .1em;
}
table.featureInfo th {
  padding:.2em .2em;
  font-weight:bold;
  background:#eee;
}
table.featureInfo td {
  background:#fff;
}
table.featureInfo tr.odd td {
  background:#eee;
}
table.featureInfo caption {
  text-align:left;
  font-size:100%;
  font-weight:bold;
  padding:.2em .2em;
}
</style>
<body>
<table class='featureInfo'>
<caption class='featureInfo'>poly_landmarks</caption>
<tr>
<th>Oid</th>
<th>LAND</th>
<th>CFCC</th>
<th>LANAME</th>
</tr>
<tr>
<td>52</td>
<td>76</td>
<td>D65</td>
<td>City Hall</td>
</tr>
<tr>
<td>47</td>
<td>69</td>
<td>H11</td>
<td>Hudson River</td>
</tr>
</table><br /><table class='featureInfo'>
<caption class='featureInfo'>tiger_roads</caption>
<tr>
<th>Oid</th>
<th>CFCC</th>
<th>NAME</th>
</tr>
<tr>
<td>7664</td>
<td>A41</td>
<td>Broadway</td>
</tr>
<tr>
<td>7667</td>
<td>A41</td>
<td>Broadway</td>
</tr>
<tr>
<td>6016</td>
<td>A41</td>
<td>Barclay St</td>
</tr>
</table><br />
</body>
</html>";
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
                    Expect.Call(req.GetParam("INFO_FORMAT")).Return("text/html");
                    Expect.Call(req.GetParam("X")).Return(null);
                    Expect.Call(req.GetParam("I")).Return("378");
                    Expect.Call(req.GetParam("Y")).Return(null);
                    Expect.Call(req.GetParam("J")).Return("288");
                    Expect.Call(req.GetParam("FEATURE_COUNT")).Return("10");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetFeatureInfoHtml(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetFeatureInfoResponseHtml>(resp);
                    GetFeatureInfoResponseHtml html = (GetFeatureInfoResponseHtml)resp;
                    string contentType = html.ContentType;
                    Assert.That(contentType, Is.Not.Null);
                    Assert.That(contentType, Is.EqualTo("text/html"));
                    string charset = html.Charset;
                    Assert.That(charset, Is.Not.Null);
                    Assert.That(charset, Is.EqualTo("utf-8"));
                    string actual = html.Response;
                    string expected = expectedHtml.Replace(Environment.NewLine, new String('\n', 1));
                    Assert.That(actual, Is.EqualTo(expected));
                });
        }
    }
}