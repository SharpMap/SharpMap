using System.Collections.Concurrent;
using NUnit.Framework;
using SharpMap.Geometries;

namespace UnitTests.Rendering
{
    [TestFixture]
    public class CohenSutherlandLineClippingTest
    {
        [Test]
        public void SimpleTest()
        {
            var lc = new SharpMap.Rendering.Symbolizer.CohenSutherlandLineClipping(0, 0, 10, 10);
            var l = new LineString(new[] { new Point(-5, 5), new Point(15, 5), });
            var res = lc.ClipLineString(l);
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(typeof(MultiLineString), res);
            Assert.AreEqual("MULTILINESTRING ((0 5, 10 5))", res.ToString());

            l = new LineString(new[] { new Point(5, -5), new Point(5, 15), });
            res = lc.ClipLineString(l);
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(typeof(MultiLineString), res);
            Assert.AreEqual("MULTILINESTRING ((5 0, 5 10))", res.ToString());

            l = new LineString(new[] { new Point(5, -5), new Point(5, 5), new Point(5, 15), });
            res = lc.ClipLineString(l);
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(typeof(MultiLineString), res);
            Assert.AreEqual("MULTILINESTRING ((5 0, 5 5, 5 10))", res.ToString());
        }

        [Test]
        public void InOutInTest()
        {
            var lc = new SharpMap.Rendering.Symbolizer.CohenSutherlandLineClipping(0, 0, 10, 10);
            var l = new LineString(new[] { new Point(-5, 4), new Point(15, 4), new Point(15, 6), new Point(-5, 6), });
            var res = lc.ClipLineString(l);
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(typeof (MultiLineString), res);
            Assert.AreEqual("MULTILINESTRING ((0 4, 10 4), (10 6, 0 6))", res.ToString());
        }

        //Any more
    }
}