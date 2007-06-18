using System;
using NUnit.Framework;
using SharpMap.Geometries;

namespace UnitTests.Geometries
{
	[TestFixture]
	public class PointTests
	{
		[Test]
		public void Point()
		{
			//Do various Point method calls to cover the point class with sufficient testing
			Point p0 = new Point();
			Point p1 = new Point(0,0);
			Point p2 = new Point(450, 120);
			Assert.IsTrue(p0.IsEmpty());
			Assert.IsFalse(p1.IsEmpty());
			Assert.AreNotEqual(p0, p1);
			Assert.AreEqual(450, p2.X);
			Assert.AreEqual(120, p2.Y);
			Assert.AreNotSame(p2.Clone(), p2);
			p0 = p2.Clone();
			p0.X += 100; p0.Y = 150;
			p0[0] += p0[1];
			Assert.AreEqual(new Point(700, 150),p0);
			Assert.AreEqual(p2, p2.GetBoundingBox().Min);
			Assert.AreEqual(p2, p2.GetBoundingBox().Max);
			Assert.IsTrue(p2.IsSimple());
			Assert.IsFalse(p2.IsEmpty());
			Assert.AreEqual(2, p2.NumOrdinates);
			Assert.AreEqual(new Point(400, 100), p2 + new Point(-50, -20));
			Assert.AreEqual(new Point(500, 100), p2 - new Point(-50, 20));
			Assert.AreEqual(new Point(900, 240), p2 * 2);
			Assert.AreEqual(0, p2.Dimension);
			Assert.AreEqual(450, p2[0]);
			Assert.AreEqual(120, p2[1]);
			Assert.IsNull(p2.Boundary());
			Assert.AreEqual(p2.X.GetHashCode() ^ p2.Y.GetHashCode() ^ p2.IsEmpty().GetHashCode(), p2.GetHashCode());
			Assert.Greater(p2.CompareTo(p1), 0);
			Assert.Less(p1.CompareTo(p2), 0);
			Assert.AreEqual(p2.CompareTo(new Point(450,120)), 0);
		}
		[Test]
		public void Point3D()
		{
			//Do various Point method calls to cover the point class with sufficient testing
			Point3D p0 = new Point3D();
			Point p = new Point(23,21);
			Point3D p1 = new Point3D(450, 120, 34);
			Point3D p2 = new Point3D(p, 94);
			Assert.IsTrue(p0.IsEmpty());
			Assert.IsFalse(p1.IsEmpty());
			Assert.IsFalse(p2.IsEmpty());
			
			Assert.AreNotEqual(p, p2);
			Assert.AreEqual(94, p2.Z);
			
			Assert.AreNotSame(p1.Clone(), p1);
			p0 = p1.Clone();
			p0.X += 100; p0.Y = 150; p0.Z += 499;
			p0[2] += p0[2];
			Assert.AreEqual(new Point3D(550, 150, 1066), p0);
			Assert.AreEqual(p2.AsPoint(), p2.GetBoundingBox().Min);
			Assert.AreEqual(p2.AsPoint(), p2.GetBoundingBox().Max);
			Assert.AreEqual(3, p2.NumOrdinates);
			Assert.AreEqual(new Point3D(-27, 1, 123), p2 + new Point3D(-50, -20, 29));
			Assert.AreEqual(new Point(73, 1), p2 - new Point(-50, 20));
			Assert.AreEqual(new Point3D(46, 42, 188), p2 * 2);
			Assert.AreEqual(0, p2.Dimension);
			Assert.AreEqual(23, p2[0]);
			Assert.AreEqual(21, p2[1]);
			Assert.AreEqual(94, p2[2]);
			Assert.IsNull(p2.Boundary());
			Assert.AreEqual(p2.X.GetHashCode() ^ p2.Y.GetHashCode() ^ p2.Z.GetHashCode() ^ p2.IsEmpty().GetHashCode(), p2.GetHashCode());
			Assert.Less(p2.CompareTo(p1), 0);
			Assert.Greater(p1.CompareTo(p2), 0);
			Assert.AreEqual(0, p2.CompareTo(new Point3D(23, 21, 94)));
			Assert.AreEqual(0, p2.CompareTo(new Point(23, 21)));
		}
	}
}
