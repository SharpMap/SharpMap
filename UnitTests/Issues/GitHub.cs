using System;
using System.Net;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NUnit.Framework;

namespace UnitTests.Issues
{
    public class GitHub
    {
        [Test]
        [Description("Shape is not getting plotted on the map image generated")]
        public void TestIssue78()
        {
            string jsonFile = TestUtility.GetPathToTestFile("FeatureCollection.json");
            if (!System.IO.File.Exists(jsonFile))
                Assert.Ignore("Test file {0} not present.", jsonFile);

            string json = System.IO.File.ReadAllText(jsonFile);
            var env = new Envelope();

            using (var map = new SharpMap.Map(new System.Drawing.Size(800, 400)))
            {
                map.Layers.Add(new SharpMap.Layers.TileLayer(
                    BruTile.Predefined.KnownTileSources.Create(BruTile.Predefined.KnownTileSource.BingRoadsStaging, string.Empty), "BingRoad"));

                var rss = Newtonsoft.Json.Linq.JObject.Parse(json);
                var jsonReader = new NetTopologySuite.IO.GeoJsonReader();

                foreach (var shape in rss["features"])
                {
                    var feature = jsonReader.Read<IFeature>(shape.ToString(Newtonsoft.Json.Formatting.None));
                    var geom = feature.Geometry;

                    var fp = new SharpMap.Data.Providers.GeometryFeatureProvider(geom);
                    var layer = new SharpMap.Layers.VectorLayer("geojson", fp);

                    layer.CoordinateTransformation = new
                        ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory().CreateFromCoordinateSystems(
                        ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84,
                        ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator);

                    layer.Style = new SharpMap.Styles.VectorStyle
                    {
                        Fill = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, 255, 0, 0)),
                        Outline = new System.Drawing.Pen(System.Drawing.Color.Red, 1.5f),
                        EnableOutline = true
                    };

                    env.ExpandToInclude(layer.Envelope);
                    map.Layers.Add(layer);
                }

                map.ZoomToBox(env);
                map.Zoom *= 1.1;

                using (var img = map.GetMap())
                    img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), $"TestIssue78.png"));

            }
        }

#if !LINUX
        [TestCase]
        [Description("Raster Layer removed when apply rotation on map")]
        public void TestIssue116()
        {
            string rasterFile = TestUtility.GetPathToTestFile("world.topo.bathy.200412.3x21600x10800.jpg");
            if (!System.IO.File.Exists(rasterFile))
                Assert.Ignore("Test file {0} not present.", rasterFile);

            using (var map = new SharpMap.Map())
            {
                var rasterLyr = new SharpMap.Layers.GdalRasterLayer("Raster", rasterFile);
                map.Layers.Add(rasterLyr);

                var linePoints = new[] { new GeoAPI.Geometries.Coordinate(0, 0), new GeoAPI.Geometries.Coordinate(10, 10) };
                var line = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreateLineString(linePoints);
                var linealDs = new SharpMap.Data.Providers.GeometryProvider(line);
                var linealLyr = new SharpMap.Layers.VectorLayer("Lineal", linealDs) { SRID = 4326 };
                linealLyr.Style.Line = new System.Drawing.Pen(System.Drawing.Color.Red, 2f) { EndCap = System.Drawing.Drawing2D.LineCap.Round };
                map.Layers.Add(linealLyr);

                map.ZoomToExtents();
                var centerMap = new System.Drawing.PointF(map.Size.Width / 2f, map.Size.Height / 2f);
                for (float f = -180; f <= 180; f += 5)
                {
                    var mapTransform = new System.Drawing.Drawing2D.Matrix();
                    mapTransform.RotateAt(f, centerMap);

                    map.MapTransform = mapTransform;

                    using (var img = map.GetMap())
                        img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), $"TestIssue116.{(f < 0 ? "N" : "P")}{((int)Math.Abs(f)):D3}deg.png"));
                }
            }
        }
#endif

        [TestCase("https://www.wms.nrw.de/geobasis/wms_nw_dop", SecurityProtocolType.Ssl3, SecurityProtocolType.Tls12)]
        [Description("https request with unmatching security protocol type")]
        [Category("RequiresWindows")]
        public void TestIssue156(string url, SecurityProtocolType sptFail, SecurityProtocolType sptSucceed)
        {
            var spDefault = ServicePointManager.SecurityProtocol;

            SharpMap.Layers.WmsLayer wmsLayer = null;
            ServicePointManager.SecurityProtocol = sptFail;
            Assert.That(() => wmsLayer = new SharpMap.Layers.WmsLayer("WMSFAIL", url), Throws.InstanceOf<System.ApplicationException>() );
            ServicePointManager.SecurityProtocol = sptSucceed;
            Assert.That(() => wmsLayer = new SharpMap.Layers.WmsLayer("WMSSUCCED", url), Throws.Nothing);
            Assert.That(wmsLayer, Is.Not.Null, "wmsLayer null");

            ServicePointManager.SecurityProtocol = spDefault;
        }
    }
}
