using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using NUnit.Framework;
using SharpKml.Dom;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Decoration;

namespace UnitTests.Data.Providers
{
    [TestFixture]
    public class KmlProviderTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }

        [Test]
        public void TestConstruction()
        {
            KmlProvider p = null;
            Assert.DoesNotThrow(() => p = KmlProvider.FromKml(TestUtility.GetPathToTestFile("KML_Samples.kml")));

            Assert.IsNotNull(p);
            Assert.IsTrue(p.ConnectionID.StartsWith("KML Samples"));
        }

        [Test]
        public void TestGetMap()
        {
            var m = new Map(new Size(500, 300));
            m.BackColor = Color.White;

            var p = KmlProvider.FromKml(TestUtility.GetPathToTestFile("KML_Samples.kml"));
            var l = new VectorLayer(p.ConnectionID, p);
            l.Theme = p.GetKmlTheme();
            
            m.Layers.Add(l);
            m.ZoomToExtents();
            m.Zoom *= 1.1;

            using (var img = m.GetMap())
                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "KmlProviderImage.png"), ImageFormat.Png);
        }
        
//        [Test]
//        public void TestKmlInternalStyle()
//        {
//            var m = new Map(new Size(500, 300));
//            m.BackColor = Color.White;
//
//            var p = KmlProvider.FromKml(TestUtility.GetPathToTestFile("KML_Samples.kml"));
//            var l = new VectorLayer(p.ConnectionID, p);
//            l.Theme = p.GetKmlTheme();
//            
//            m.Layers.Add(l);
//            m.ZoomToExtents();
//            m.Zoom *= 1.1;
//
//            using (var img = m.GetMap())
//                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "KmlProviderImage.png"), ImageFormat.Png);
//        }
//
//        [Test]
//        public void TestKmzInternalStyle()
//        {
//            var m = new Map(new Size(500, 300));
//            m.BackColor = Color.White;
//
//            var p = KmlProvider.FromKml(TestUtility.GetPathToTestFile("KML_Samples.kml"));
//            var l = new VectorLayer(p.ConnectionID, p);
//            l.Theme = p.GetKmlTheme();
//            
//            m.Layers.Add(l);
//            m.ZoomToExtents();
//            m.Zoom *= 1.1;
//
//            using (var img = m.GetMap())
//                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "KmlProviderImage.png"), ImageFormat.Png);
//        }

        [TestCase("KML_geom_internal_styleurl.kml", "KML file with styles defined internally (see also KMZ equivalent)")]
        [TestCase("KML_geom_external_styleurl_file.kml", "KML file with styles defined in KML_styles.kml (see also KMZ equivalent)")]
        [TestCase("KML_geom_external_styleurl_http.kml", "KML file with icon styles from HTTP resource")]
        [TestCase("KML_geom_external_styleurl_invalid.kml", "KML file with invalid StyleUrls. \nDefault icons/symbology will be used, and any style overrides will be applied")]
        [TestCase("KMZ_geom_internal_styleurl.kmz", "KMZ file with styles defined internally (see also KML equivalent)")]
        [TestCase("KMZ_geom_external_styleurl_file.kmz", "KMZ file with styles defined in KMZ_styles.kmz (see also KML equivalent)")]
        public void TestKmlStyles(string testDataFile, string description)
        {
            // NB additional symbols at http://kml4earth.appspot.com/icons.html
            var m = new Map(new Size(500, 300));
            m.BackColor = Color.AliceBlue;

            KmlProvider p;
            if (testDataFile.ToLower().EndsWith(".kml"))
                p = KmlProvider.FromKml(TestUtility.GetPathToTestFile(testDataFile));
            else
                p = KmlProvider.FromKmz(TestUtility.GetPathToTestFile(testDataFile));
            
            var l = new VectorLayer(p.ConnectionID, p);
            l.Theme = p.GetKmlTheme();
            
            m.Layers.Add(l);
            
            var labelLayer = new LabelLayer("Labels");
            labelLayer.DataSource = p;
            labelLayer.LabelStringDelegate = (fdr) => ((Placemark) fdr["Object"]).Name;
            labelLayer.Style.Font = new Font(labelLayer.Style.Font.FontFamily, 8f);
            m.Layers.Add(labelLayer);

            ZoomToExtentsWithMargin(m);

            var disclaimer = new Disclaimer();
            disclaimer.Text = $"{description} \nOpen **{testDataFile}** in GoogleEarth for comparison";
            disclaimer.Anchor = MapDecorationAnchor.LeftTop;
            m.Decorations.Add(disclaimer);
            
            using (var img = m.GetMap())
                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), testDataFile + ".png"), ImageFormat.Png);
        }

        [TestCase(@"https://developers.google.com/kml/documentation/kmlfiles/animatedupdate_example.kml", "developers.google.com")]
        [TestCase(@"http://data-fairfaxcountygis.opendata.arcgis.com/datasets/533db4de13ae4d729884b5ec41d65034_0.kml", "data-fairfaxcountygis.opendata.arcgis.com")]
        public void TestHttp(string kmlWebResource, string description)
        {
            var m = new Map(new Size(500, 300));
            m.BackColor = Color.White;

            KmlProvider p = null;
            
            var request = WebRequest.Create(kmlWebResource);
            using (var response = request.GetResponse())
            {
                if (response.ContentType == "None" || response.ContentType.Contains("vnd.google-earth.kml"))
                {
                    var s = response.GetResponseStream();
                    p = KmlProvider.FromKml(s);
                }
                    
                else if (response.ContentType.Contains("vnd.google-earth.kmz"))
                    p = KmlProvider.FromKmz(response.GetResponseStream(), "doc.kml");
            }
            
            var l = new VectorLayer(p.ConnectionID, p);
            l.Theme = p.GetKmlTheme();
            m.Layers.Add(l);

            // Takes ages to render
//            var labelLayer = new LabelLayer("Labels");
//            labelLayer.DataSource = p;
//            labelLayer.LabelColumn = "Name";
//            m.Layers.Add(labelLayer);
         
            ZoomToExtentsWithMargin(m);

            var disclaimer = new Disclaimer();
            disclaimer.Text = kmlWebResource;
            disclaimer.Anchor = MapDecorationAnchor.LeftTop;
            m.Decorations.Add(disclaimer);
            
            using (var img = m.GetMap())
                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), description + ".png"), ImageFormat.Png);
        }

        private void ZoomToExtentsWithMargin(Map m)
        {
            m.ZoomToExtents();
            var env = m.GetExtents();
            if (Math.Abs(env.Width) < 0.025)
            {
                env.ExpandBy(0.025);
                m.ZoomToBox(env);
            }
            m.Zoom *= 1.1;

        }
        
        
    }
}
