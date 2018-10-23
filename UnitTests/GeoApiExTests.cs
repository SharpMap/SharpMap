using GeoAPI.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTests
{
    [TestFixture]
    public class GeoApiExTests
    {
        [Test]
        public void TestCloseRing()
        {
            var coords = new List<Coordinate>(new Coordinate[] {
                new Coordinate(0,0,5),
                new Coordinate(0,100,10),
                new Coordinate(100,100,20),
                new Coordinate(100,0,30)
            });

            coords.EnsureValidRing();
            Assert.AreEqual(5,coords.Count);

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
