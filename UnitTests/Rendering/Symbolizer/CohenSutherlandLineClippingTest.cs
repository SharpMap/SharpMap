﻿using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace UnitTests.Rendering.Symbolizer
{
    [TestFixture]
    public class CohenSutherlandLineClippingTest
    {
        private static readonly GeometryFactory Factory = new GeometryFactory();

        [Test]
        public void SimpleTest()
        {
            var lc = new SharpMap.Rendering.Symbolizer.CohenSutherlandLineClipping(0, 0, 10, 10);
            var l = Factory.CreateLineString(new[] { new Coordinate(-5, 5), new Coordinate(15, 5), });
            var res = lc.ClipLineString(l);
            Assert.IsNotNull(res);
            Assert.IsTrue(res is MultiLineString);
            Assert.AreEqual("MULTILINESTRING ((0 5, 10 5))", res.ToString());

            l = Factory.CreateLineString(new[] { new Coordinate(5, -5), new Coordinate(5, 15), });
            res = lc.ClipLineString(l);
            Assert.IsNotNull(res);
            Assert.IsTrue(res is MultiLineString);
            Assert.AreEqual("MULTILINESTRING ((5 0, 5 10))", res.ToString());

            l = Factory.CreateLineString(new[] { new Coordinate(5, -5), new Coordinate(5, 5), new Coordinate(5, 15), });
            res = lc.ClipLineString(l);
            Assert.IsNotNull(res);
            Assert.IsTrue(res is MultiLineString);
            Assert.AreEqual("MULTILINESTRING ((5 0, 5 5, 5 10))", res.ToString());
        }

        [Test]
        public void InOutInTest()
        {
            var lc = new SharpMap.Rendering.Symbolizer.CohenSutherlandLineClipping(0, 0, 10, 10);
            var l = Factory.CreateLineString(new[] { new Coordinate(-5, 4), new Coordinate(15, 4), new Coordinate(15, 6), new Coordinate(-5, 6), });
            var res = lc.ClipLineString(l);
            Assert.IsNotNull(res);
            Assert.IsTrue(res is MultiLineString);
            Assert.AreEqual("MULTILINESTRING ((0 4, 10 4), (10 6, 0 6))", res.ToString());
        }

        //Any more
    }
}