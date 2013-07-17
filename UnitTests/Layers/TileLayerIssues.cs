using System.Drawing;
using System.Drawing.Imaging;
using BruTile.Web;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Layers;

namespace UnitTests.Layers
{
    [TestFixture]
    public class TileLayerIssues
    {
        private string _fileCacheRoot;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _fileCacheRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BruTileFileCache");
        }

        [Test, Ignore("Could not reproduce issue"), Description("Map.GetMap() returns incomplete image")]
        public void TestIncompleteImage()
        {
            using (var map = new SharpMap.Map(new Size(2500, 2500)))
            {
                map.BackColor = Color.Magenta;
                var br = new BingRequest(BingRequest.UrlBing, "", BingMapType.Hybrid);
                var bts = new BingTileSource(br);
                var tl = new TileLayer(bts, "TileLayer - " + BingMapType.Hybrid.ToString(), Color.Transparent, true,
                                       System.IO.Path.Combine(_fileCacheRoot, "BingStaging"));
                map.Layers.Add(tl);
                
                map.ZoomToBox(new Envelope(829384.331338522, 837200.785470394, 7068020.31417922, 7072526.73926545)
                            /*new Envelope(-239839.49199841652, 78451.759683380573, -37033.0152981899, 106723.52879865949)*/);
                using (var image = map.GetMap())
                {
                    image.Save("TestIncompleteImage.png", ImageFormat.Png);
                }
            }
        }
    }
}