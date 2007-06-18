using System;
using NUnit.Framework;
using SharpMap.Geometries;

namespace UnitTests.Geometries
{
	[TestFixture]
	public class MultiLinestringsTests
	{
		[Test]
		public void MultiLinestring()
		{
			MultiLineString mls = new MultiLineString();
			Assert.IsTrue(mls.IsEmpty());
			mls.LineStrings.Add(new LineString());
			Assert.IsTrue(mls.IsEmpty());
			mls.LineStrings[0].Vertices.Add(new Point(45, 68));
			mls.LineStrings[0].Vertices.Add(new Point(82, 44));
			mls.LineStrings.Add(CreateLineString());
			foreach (LineString ls in mls)
				Assert.IsFalse(ls.IsEmpty());
			Assert.IsFalse(mls.IsEmpty());
			foreach (LineString ls in mls)
				Assert.IsFalse(ls.IsClosed);
			Assert.IsFalse(mls.IsClosed);
			//Close linestrings
			foreach (LineString ls in mls)
				ls.Vertices.Add(ls.StartPoint.Clone());
			foreach (LineString ls in mls)
				Assert.IsTrue(ls.IsClosed);
			Assert.IsTrue(mls.IsClosed);
			Assert.AreEqual(new BoundingBox(1,2,930,123), mls.GetBoundingBox());
		}

		private LineString CreateLineString()
		{
			LineString ls = new LineString();
			ls.Vertices.Add(new Point(1, 2));
			ls.Vertices.Add(new Point(10, 22));
			ls.Vertices.Add(new Point(930, 123));
			return ls;
		}
	}
}
