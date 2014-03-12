using GeoAPI.Geometries;
using GeoAPI.SpatialReference;
using NUnit.Framework;

namespace SharpMap.SpatialReference
{
    [TestFixture]
    public abstract class SpatialReferenceBaseTest
    {
        private readonly Reprojector _reprojector;
        private readonly ISpatialReference _fromSr, _toSr;

        private readonly Coordinate _c0 = new Coordinate();
        
        protected SpatialReferenceBaseTest(IReprojectorCore core)
        {
            _reprojector = new Reprojector(core);

            _fromSr = _reprojector.Factory.Create("EPSG:4326");
            _toSr = _reprojector.Factory.Create("EPSG:3857");
        }

        [Test]
        public void TestCoordinate()
        {
            Coordinate c0 = _c0, c1 = null, c2 = null;
            Assert.DoesNotThrow(() => c1 = _reprojector.Reproject(c0, _fromSr, _toSr));
            Assert.DoesNotThrow(() => c2 = _reprojector.Reproject(c1, _fromSr, _toSr));
            //Assert.AreNotEqual(_c0, c1);
            //Assert.AreNotEqual(c1, c2);
            Assert.AreEqual(0d, c2.Distance(_c0), 1.0E-3);
        }

        [Test, Ignore("Not yet implemented")]
        public void TestCoordinateSequence()
        {
        }

        [Test, Ignore("Not yet implemented")]
        public void TestPoint()
        {
        }

        [Test, Ignore("Not yet implemented")]
        public void TestLineString()
        {
        }

        [Test, Ignore("Not yet implemented")]
        public void TestPolygon()
        {
        }

        [Test, Ignore("Not yet implemented")]
        public void TestMultiPoint()
        {
        }

        [Test, Ignore("Not yet implemented")]
        public void TestMultiLineString()
        {
        }

        [Test, Ignore("Not yet implemented")]
        public void TestMultiPolygon()
        {
        }

    }

    public class DotSpatialSpatialRefernceTest : SpatialReferenceBaseTest
    {
        public DotSpatialSpatialRefernceTest()
            :base(new DotSpatialReprojector())
        {
        }
    }

    public class ProjNetSpatialRefernceTest : SpatialReferenceBaseTest
    {
        private static readonly ProjNetSpatialReferenceFactory Factory =
            new ProjNetSpatialReferenceFactory();

        public ProjNetSpatialRefernceTest()
            : base(new ProjNetReprojector())
        {
        }
    }

}