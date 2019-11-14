using System.Drawing;
using System.Drawing.Imaging;
using BruTile.Web;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Layers;
using BruTile.Predefined;

namespace UnitTests.Layers
{
    [TestFixture]
    public class TileLayerIssues
    {
        private string _fileCacheRoot;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _fileCacheRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BruTileFileCache");
        }

        [Test, Ignore("Could not reproduce issue"), Description("Map.GetMap() returns incomplete image")]
        public void TestIncompleteImage()
        {
            using (var map = new SharpMap.Map(new Size(2500, 2500)))
            {
                map.BackColor = Color.Magenta;
                var fc = new BruTile.Cache.FileCache(System.IO.Path.Combine(_fileCacheRoot, "BingStaging"), "png"); 
                var bts = KnownTileSources.Create(KnownTileSource.BingHybridStaging, null, fc);
                var tl = new TileLayer(bts, "TileLayer - " + KnownTileSource.BingHybridStaging, Color.Transparent, true);
                                       
                map.Layers.Add(tl);
                
                map.ZoomToBox(new Envelope(829384.331338522, 837200.785470394, 7068020.31417922, 7072526.73926545)
                            /*new Envelope(-239839.49199841652, 78451.759683380573, -37033.0152981899, 106723.52879865949)*/);
                using (var image = map.GetMap())
                    image.Save(
                        System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "TestIncompleteImage.png"),
                        ImageFormat.Png);
            }
        }
    }
}
