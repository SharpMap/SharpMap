using System;
using NUnit.Framework;
using SharpMap.Geometries;

namespace UnitTests.Geometries
{
	[TestFixture]
	public class BoundingBoxTests
	{
		[Test]
		public void TestJoin()
		{
			BoundingBox b1 = new BoundingBox(20, 30, 40, 50);
			BoundingBox b2 = new BoundingBox(-20, 56, 70, 75);
			BoundingBox bJoined = new BoundingBox(-20, 30, 70, 75);
			Assert.AreEqual(bJoined,b1.Join(b2));
			BoundingBox box = null;
			Assert.AreEqual(b1, b1.Join(box));
			Assert.AreEqual(bJoined, BoundingBox.Join(b1,b2));
			Assert.AreEqual(b2, BoundingBox.Join(null,b2));
			Assert.AreEqual(b1, BoundingBox.Join(b1, null));
			Assert.AreEqual(bJoined, BoundingBox.Join(b2, b1));
		}
		[Test]
		public void TestVarious()
		{
			BoundingBox b1 = new BoundingBox(20, 30, 40, 55);
			Assert.AreEqual(20, b1.Left);
			Assert.AreEqual(20, b1.Min.X);
			Assert.AreEqual(40, b1.Right);
			Assert.AreEqual(40, b1.Max.X);
			Assert.AreEqual(30, b1.Bottom);
			Assert.AreEqual(30, b1.Min.Y);
			Assert.AreEqual(55, b1.Top);
			Assert.AreEqual(55, b1.Max.Y);
			Assert.AreEqual(20, b1.Width);
			Assert.AreEqual(25, b1.Height);
			Assert.AreNotSame(b1, b1.Clone());
			Assert.IsTrue(b1.Contains(new Point(30, 40)));
			Assert.IsTrue(b1.Contains(new Point(20, 40)));
			Assert.IsFalse(b1.Contains(new Point(10, 10)));
			Assert.IsFalse(b1.Contains(new Point(50, 60)));
			Assert.IsFalse(b1.Contains(new Point(30, 60)));
			Assert.IsFalse(b1.Contains(new Point(50, 40)));
			Assert.IsFalse(b1.Contains(new Point(30, 15)));
			Assert.IsTrue(b1.Equals(new BoundingBox(20, 30, 40, 55)));
			Assert.AreEqual(new Point(30, 42.5), b1.GetCentroid());
			Assert.AreEqual(1, b1.LongestAxis);
			Assert.AreEqual(new BoundingBox(19, 29, 41, 56), b1.Grow(1));
			Assert.IsFalse(b1.Equals(null));
			Assert.IsFalse(b1.Equals((object)new Polygon()));
			Assert.AreEqual("20,30 40,55", b1.ToString());
			Assert.AreEqual(b1,new BoundingBox(40,55,20,30));
			Assert.AreEqual(Math.Sqrt(200), b1.Distance(new BoundingBox(50,65,60,75)));
		}
		[Test]
		public void TestIntersect()
		{
			//Test disjoint
			BoundingBox b1 = new BoundingBox(0, 0, 10, 10);
			BoundingBox b2 = new BoundingBox(20, 20, 30, 30);
			Assert.IsFalse(b1.Intersects(b2), "Bounding box intersect test 1a failed");
			Assert.IsFalse(b2.Intersects(b1), "Bounding box intersect test 1a failed");
			b1 = new BoundingBox(0, 0, 10, 10);
			b2 = new BoundingBox(0, 20, 10, 30);
			Assert.IsFalse(b1.Intersects(b2), "Bounding box intersect test 1b failed");
			Assert.IsFalse(b2.Intersects(b1), "Bounding box intersect test 1b failed");
			b1 = new BoundingBox(0, 0, 10, 10);
			b2 = new BoundingBox(20, 0, 30, 10);
			Assert.IsFalse(b1.Intersects(b2), "Bounding box intersect test 1c failed");
			Assert.IsFalse(b2.Intersects(b1), "Bounding box intersect test 1c failed");
			//Test intersects
			b1 = new BoundingBox(0, 0, 10, 10);
			b2 = new BoundingBox(5, 5, 15, 15);
			Assert.IsTrue(b1.Intersects(b2), "Bounding box intersect test 2 failed");
			Assert.IsTrue(b2.Intersects(b1), "Bounding box intersect test 2 failed");
			//Test overlaps
			b1 = new BoundingBox(0, 0, 10, 10);
			b2 = new BoundingBox(-5, -5, 15, 15);
			Assert.IsTrue(b1.Intersects(b2), "Bounding box intersect test 3 failed");
			Assert.IsTrue(b2.Intersects(b1), "Bounding box intersect test 3 failed");
			//Test touches
			b1 = new BoundingBox(0, 0, 10, 10);
			b2 = new BoundingBox(10, 0, 20, 10);
			Assert.IsTrue(b1.Intersects(b2), "Bounding box intersect test 4a failed");
			Assert.IsTrue(b2.Intersects(b1), "Bounding box intersect test 4a failed");
			//Test touches 2
			b1 = new BoundingBox(0, 0, 10, 10);
			b2 = new BoundingBox(10, 10, 20, 20);
			Assert.IsTrue(b1.Intersects(b2), "Bounding box intersect test 4b failed");
			Assert.IsTrue(b2.Intersects(b1), "Bounding box intersect test 4b failed");
			//Test equal
			b1 = new BoundingBox(0, 0, 10, 10);
			b2 = new BoundingBox(0, 0, 10, 10);
			Assert.IsTrue(b1.Intersects(b2), "Bounding box intersect test 5 failed");
			Assert.IsTrue(b2.Intersects(b1), "Bounding box intersect test 5 failed");
		}
	}
}
