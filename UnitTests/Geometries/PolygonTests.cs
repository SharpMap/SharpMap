using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpMap.Geometries;

namespace UnitTests.Geometries
{
	[TestFixture]
	public class PolygonTests
	{
		[Test]
		public void PolygonTest()
		{
			Polygon p = new Polygon();
			Assert.IsTrue(p.IsEmpty());
			Assert.AreEqual(0, p.NumInteriorRing);
			Assert.AreEqual(p.NumInteriorRing, p.InteriorRings.Count);
			Assert.IsFalse(p.Equals(null));
			Assert.IsTrue(p.Equals(new Polygon()));
			Assert.IsNull(p.GetBoundingBox());
			LinearRing ring = new LinearRing();
			ring.Vertices.Add(new Point(10, 10));
			ring.Vertices.Add(new Point(20, 10));
			ring.Vertices.Add(new Point(20, 20));
            Assert.IsFalse(ring.IsCCW());
			ring.Vertices.Add(new Point(10, 20));
            ring.Vertices.Add(ring.Vertices[0].Clone());
            Assert.IsTrue(ring.IsPointWithin(new Point(15, 15)));
            Assert.AreNotSame(ring.Clone(), ring);
			p.ExteriorRing = ring;
            
            

			Assert.AreEqual(100, p.Area);
			LinearRing ring2 = new LinearRing();
			ring2.Vertices.Add(new Point(11, 11));
			ring2.Vertices.Add(new Point(19, 11));
			ring2.Vertices.Add(new Point(19, 19));
			ring2.Vertices.Add(new Point(11, 19));
			ring2.Vertices.Add(ring2.Vertices[0].Clone());			
			p.InteriorRings.Add(ring2);
			Assert.AreEqual(100 + 64, p.Area);
            // Reverse() doesn't exist for Collections
			//ring2.Vertices.Reverse();
			//Assert.AreEqual(100 - 64, p.Area);
			Assert.AreEqual(1, p.NumInteriorRing);
			Assert.AreEqual(new BoundingBox(10, 10, 20, 20), p.GetBoundingBox());

			Polygon p2 = p.Clone();
			Assert.AreEqual(p, p2);
			Assert.AreNotSame(p, p2);
			p2.InteriorRings.RemoveAt(0);
			Assert.AreNotEqual(p, p2);
		}
	}
}
