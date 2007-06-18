using System;
using NUnit.Framework;
using SharpMap.Geometries;

namespace UnitTests.Converters.WKB
{
	[TestFixture]
	public class WKBTests
	{
		string multiLinestring = "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))";
		string linestring = "LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)";
		string polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 21 29, 29 29, 29 21, 21 21))";
		string point = "POINT (20.564 346.3493254)";
		string multipoint = "MULTIPOINT (20.564 346.3493254, 45 32, 23 54)";

		[Test]
		public void Convert()
		{
			Geometry gML0 = Geometry.GeomFromText(multiLinestring);
			Geometry gLi0 = Geometry.GeomFromText(linestring);
			Geometry gPl0 = Geometry.GeomFromText(polygon);
			Geometry gPn0 = Geometry.GeomFromText(point);
			Geometry gMp0 = Geometry.GeomFromText(multipoint);
			Geometry gML1 = Geometry.GeomFromWKB(gML0.AsBinary());
			Geometry gLi1 = Geometry.GeomFromWKB(gLi0.AsBinary());
			Geometry gPl1 = Geometry.GeomFromWKB(gPl0.AsBinary());
			Geometry gPn1 = Geometry.GeomFromWKB(gPn0.AsBinary());
			Geometry gMp1 = Geometry.GeomFromWKB(gMp0.AsBinary());
			Assert.AreEqual(gML0, gML1);
			Assert.AreEqual(gLi0, gLi1);
			Assert.AreEqual(gPl0, gPl1);
			Assert.AreEqual(gPn0, gPn1);
			Assert.AreEqual(gMp0, gMp1);
		}
	}
}
