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

        protected SpatialReferenceBaseTest(IReprojectorCore core, ISpatialReference @fromSr, ISpatialReference toSr)
        {
            _reprojector = new Reprojector(core);
            _fromSr = @fromSr;
            _toSr = toSr;
        }

        [Test]
        public void TestCoordinate()
        {
            Coordinate c1 = null, c2 = null;
            Assert.DoesNotThrow(() => c1 = _reprojector.Reproject(_c0, _fromSr, _toSr));
            Assert.DoesNotThrow(() => c2 = _reprojector.Reproject(c1, _fromSr, _toSr));
            //Assert.AreNotEqual(_c0, c1);
            //Assert.AreNotEqual(c1, c2);
            Assert.AreEqual(0d, c2.Distance(_c0), 1.0E-7);
        }


    }

    public class DotSpatialSpatialRefernceTest : SpatialReferenceBaseTest
    {
        private static readonly DotSpatialProjectionsSpatialReferenceFactory Factory =
            new DotSpatialProjectionsSpatialReferenceFactory();
        
        public DotSpatialSpatialRefernceTest()
            :base(new DotSpatialReprojector(), Factory.Create("EPSG:4326"), Factory.Create("EPSG:3758"))
        {
        }
    }

    public class ProjNetSpatialRefernceTest : SpatialReferenceBaseTest
    {
        private static readonly ProjNetSpatialReferenceFactory Factory =
            new ProjNetSpatialReferenceFactory();

        public ProjNetSpatialRefernceTest()
            : base(new ProjNetReprojector(), Factory.Create("EPSG:4326"), Factory.Create("EPSG:3758"))
        {
        }
    }

}