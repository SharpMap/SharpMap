using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace UnitTests.Data.Providers
{
    [TestFixture]
    public class KmlProviderTests
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }

        [Test]
        public void TestConstruction()
        {
            KmlProvider p = null;
            Assert.DoesNotThrow(() => p = KmlProvider.FromKml(@"TestData\KML_Samples.kml"));

            Assert.IsNotNull(p);
            Assert.IsTrue(p.ConnectionID.StartsWith("KML Samples"));
        }

        [Test]
        public void TestGetMap()
        {
            var m = new Map(new Size(500, 300));
            m.BackColor = Color.White;

            var p = KmlProvider.FromKml(@"TestData\KML_Samples.kml");
            var l = new VectorLayer(p.ConnectionID, p);
            l.Theme = p.GetKmlTheme();
            
            m.Layers.Add(l);
            m.ZoomToExtents();
            m.Zoom *= 1.1;

            var img = m.GetMap();
            img.Save("KmlProviderImage.png", ImageFormat.Png);


        }
    }
}
