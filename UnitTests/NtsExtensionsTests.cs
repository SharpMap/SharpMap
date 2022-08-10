using NetTopologySuite.Geometries;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class NtsExtensionsTests
    {
        [Test]
        public void TestCloseRing()
        {
            var coords = new List<Coordinate>(new Coordinate[] {
                new CoordinateZ(0,0,5),
                new CoordinateZ(0,100,10),
                new CoordinateZ(100,100,20),
                new CoordinateZ(100,0,30)
            });

            coords.EnsureValidRing();
            Assert.AreEqual(5, coords.Count);

            Assert.AreEqual(5, coords[4].Z);

        }

        [Test]
        public void TestCloseRing2()
        {
            var coords = new List<Coordinate>(new Coordinate[] {
                new Coordinate(0,0),
                new Coordinate(0,100),
                new Coordinate(100,100),
                new Coordinate(100,0)
            });

            coords.EnsureValidRing();
            Assert.AreEqual(5, coords.Count);
        }
    }
}
