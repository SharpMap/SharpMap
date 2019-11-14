using System;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace UnitTests.Converters.WKT
{
    public class SpatialReferenceTests
    {
        [Test]
        public void TestWkt()
        {
            string wkt = null;
            Assert.DoesNotThrow(() => wkt = SharpMap.Converters.WellKnownText.SpatialReference.SridToWkt(31466));
            Assert.IsFalse(string.IsNullOrWhiteSpace(wkt));
            System.Diagnostics.Trace.WriteLine("SridToWkt(31466):\n{0}", wkt);

        }

        [Test]
        public void TestProj4()
        {
            string proj4 = null;
            Assert.DoesNotThrow(() => proj4 = SharpMap.Converters.WellKnownText.SpatialReference.SridToProj4(31466));
            Assert.IsFalse(string.IsNullOrWhiteSpace(proj4));
            System.Diagnostics.Trace.WriteLine("SridToProj4(31466):\n{0}", proj4);
        }

        [TestCase(900913)]
        [TestCase(3857)]
        public void WebMercatorCanBeTranformed(int srid)
        {
            var wkt = SharpMap.Converters.WellKnownText.SpatialReference.SridToWkt(srid);

            var csf = new CoordinateSystemFactory();
            var cs = csf.CreateFromWkt(wkt);
            
            var ctf = new CoordinateTransformationFactory();
            Assert.DoesNotThrow(() => ctf.CreateFromCoordinateSystems(cs, GeographicCoordinateSystem.WGS84),
                "Could not reproject SRID:" + srid);
        }
    }
}
