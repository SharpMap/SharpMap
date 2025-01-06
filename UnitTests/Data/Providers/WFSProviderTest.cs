//using System;
using NUnit.Framework;
using SharpMap.Data;

namespace UnitTests.Data.Providers
{
    [TestFixture, Ignore("Uses internet connection")]
    public class WFSProviderTest
    {
        //<gml:coordinates xmlns:gml="http://www.opengis.net/gml" decimal="." cs="," ts=" ">51.52672002,25.33049886</gml:coordinates>
        [Test]
        public void TestAxisOrder()
        {
            //WFS.RequestValidationTest
            var p =
                new SharpMap.Data.Providers.WFS("http://geo.vliz.be/geoserver/wfs?service=WFS&request=GetCapabilities",
                    "nsTmp", "elevation_10m", SharpMap.Data.Providers.WFS.WFSVersionEnum.WFS1_1_0);
            Assert.That(p.SRID == 4326);
            Assert.That(p.AxisOrder[0] == 1 && p.AxisOrder[1]==0);
            p.AxisOrder = new[] {0, 1};
            Assert.That(p.AxisOrder[0] == 0 && p.AxisOrder[1] == 1);

            Assert.Throws<System.ArgumentException>(() => p.AxisOrder = new[] { 0, 1, 4 });
            Assert.Throws<System.ArgumentException>(() => p.AxisOrder = new[] { 0, 2 });
            Assert.Throws<System.ArgumentException>(() => p.AxisOrder = new[] { 1, 2 });
        }

        [Test]
        public void TestQuery()
        {
            var p =
                new SharpMap.Data.Providers.WFS("http://geo.vliz.be/geoserver/wfs?service=WFS&request=GetCapabilities",
                    "nsTmp", "elevation_10m", SharpMap.Data.Providers.WFS.WFSVersionEnum.WFS1_1_0);
            p.FeatureTypeInfo.Geometry._GeometryName = "the_geom";
            //p.FeatureTypeInfo.Geometry._GeometryType = 

            var ext = p.GetExtents();
            
            var g = p.GetGeometriesInView(new NetTopologySuite.Geometries.Envelope(-90, 90, -180, 180));
            Assert.That(g.Count > 0);

            var fds = new FeatureDataSet();
            Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(p.GetExtents(), fds));
        }
    }
}