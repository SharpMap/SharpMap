using NUnit.Framework;

namespace UnitTests.WMS
{
    public class SpatialReferencedBoundingBoxTest
    {
        [Test]
        public void TestConstructor()
        {
            var srbox = new SharpMap.Web.Wms.SpatialReferencedBoundingBox(0, 10, 20, 30, 4326);
            Assert.AreEqual(0d, srbox.MinX);
            Assert.AreEqual(10d, srbox.MinY);
            Assert.AreEqual(20d, srbox.MaxX);
            Assert.AreEqual(30d, srbox.MaxY);
            Assert.AreEqual(4326, srbox.SRID);
        }
    }
}