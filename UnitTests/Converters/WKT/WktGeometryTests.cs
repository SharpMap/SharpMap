using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;
using SharpMap.Geometries;
namespace UnitTests.Converters.WKT
{
	[TestFixture]
	public class WktGeometryTests
	{
		[Test]
		public void ParseGeometryCollection()
		{
			string geomCollection = "GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))";
			GeometryCollection geom = Geometry.GeomFromText(geomCollection) as GeometryCollection;
			Assert.IsNotNull(geom);
			Assert.AreEqual(3, geom.NumGeometries);
			Assert.IsTrue(geom[0] is Point);
			Assert.IsTrue(geom[1] is Point);
			Assert.IsTrue(geom[2] is LineString);
			Assert.AreEqual(geomCollection, geom.AsText());
			geom = Geometry.GeomFromText("GEOMETRYCOLLECTION EMPTY") as GeometryCollection;
			Assert.IsNotNull(geom);
			Assert.AreEqual(0, geom.NumGeometries);
			geomCollection = "GEOMETRYCOLLECTION (POINT (10 10), LINESTRING EMPTY, POINT (20 49))";
			geom = Geometry.GeomFromText(geomCollection) as GeometryCollection;
			Assert.IsNotNull(geom);
			Assert.IsTrue(geom[1].IsEmpty());
			Assert.AreEqual(3, geom.NumGeometries);
			Assert.AreEqual(geomCollection, geom.AsText());
			Assert.AreEqual("GEOMETRYCOLLECTION EMPTY", new GeometryCollection().AsText());
		}

		[Test]
		public void ParseMultipolygon()
		{
			string multipolygon = "MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), ((5 5, 7 5, 7 7, 5 7, 5 5)))";
			MultiPolygon geom = Geometry.GeomFromText(multipolygon) as MultiPolygon;
			Assert.IsNotNull(geom);
			Assert.AreEqual(2, geom.NumGeometries);
			Assert.AreEqual(new Point(5,5), geom[0].Centroid);
			Assert.AreEqual(multipolygon, geom.AsText());
			Assert.IsNotNull(Geometry.GeomFromText("MULTIPOLYGON EMPTY"));
			Assert.IsTrue(Geometry.GeomFromText("MULTIPOLYGON EMPTY").IsEmpty());
			geom = Geometry.GeomFromText("MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), EMPTY, ((5 5, 7 5, 7 7, 5 7, 5 5)))") as MultiPolygon;
			Assert.IsNotNull(geom);
			Assert.IsTrue(geom[1].IsEmpty());
			Assert.AreEqual(new Point(5, 5), geom[2].ExteriorRing.EndPoint);
			Assert.AreEqual(new Point(5, 5), geom[2].ExteriorRing.StartPoint);
			Assert.AreEqual(geom[2].ExteriorRing.StartPoint, geom[2].ExteriorRing.EndPoint);
			Assert.AreEqual(3, geom.NumGeometries);
			Assert.AreEqual("MULTIPOLYGON EMPTY", new MultiPolygon().AsText());
		}

		[Test]
		public void ParseLineString()
		{
			string linestring = "LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)";
			LineString geom = Geometry.GeomFromText(linestring) as LineString;
			Assert.IsNotNull(geom);
			Assert.AreEqual(40, geom.Length);
			Assert.IsFalse(geom.IsRing);
			Assert.AreEqual(linestring, geom.AsText());
			Assert.IsTrue((Geometry.GeomFromText("LINESTRING EMPTY") as LineString).IsEmpty());
			Assert.AreEqual("LINESTRING EMPTY", new LineString().AsText());
		}
		[Test]
		public void ParseMultiLineString()
		{
			string multiLinestring = "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))";
			MultiLineString geom = Geometry.GeomFromText(multiLinestring) as MultiLineString;
			Assert.IsNotNull(geom);
			Assert.AreEqual(3, geom.NumGeometries);
			Assert.AreEqual(180, geom.Length);
			Assert.AreEqual(120, geom[2].Length);
			Assert.IsFalse(geom[0].IsClosed, "[0].IsClosed");
			Assert.IsFalse(geom[1].IsClosed, "[1].IsClosed");
			Assert.IsTrue(geom[2].IsClosed, "[2].IsClosed");
			Assert.IsTrue(geom[0].IsSimple(), "[0].IsSimple");
			Assert.IsTrue(geom[1].IsSimple(), "[1].IsSimple");
			Assert.IsTrue(geom[2].IsSimple(), "[2].IsSimple");
			Assert.IsTrue(geom[2].IsRing, "Third line is a ring");
			Assert.AreEqual(multiLinestring, geom.AsText());
			Assert.IsTrue(Geometry.GeomFromText("MULTILINESTRING EMPTY").IsEmpty());
			geom = Geometry.GeomFromText("MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), EMPTY, (20 20, 50 20, 50 60, 20 20))") as MultiLineString;
			Assert.IsNotNull(geom);
			Assert.IsTrue(geom[2].IsEmpty());
			Assert.AreEqual(4, geom.NumGeometries);
			Assert.AreEqual("MULTILINESTRING EMPTY", new MultiLineString().AsText());
		}
		[Test]
		public void ParsePolygon()
		{
			string polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20))"; 
			Polygon geom = Geometry.GeomFromText(polygon) as Polygon;
			Assert.IsNotNull(geom);
			Assert.AreEqual(40, geom.ExteriorRing.Length);
			Assert.AreEqual(100, geom.Area);
			Assert.AreEqual(polygon, geom.AsText());
			//Test interior rings
			polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 29 21, 29 29, 21 29, 21 21), (23 23, 23 27, 27 27, 27 23, 23 23))";
			geom = Geometry.GeomFromText(polygon) as Polygon;
			Assert.IsNotNull(geom);
			Assert.AreEqual(40, geom.ExteriorRing.Length);
			Assert.AreEqual(2, geom.InteriorRings.Count);
			Assert.AreEqual(52, geom.Area);
			Assert.AreEqual(geom.ExteriorRing.Area - geom.InteriorRings[0].Area + geom.InteriorRings[1].Area, geom.Area);
			Assert.AreEqual(polygon, geom.AsText());			
			//Test empty geometry WKT
			Assert.IsTrue(Geometry.GeomFromText("POLYGON EMPTY").IsEmpty());
			Assert.AreEqual("POLYGON EMPTY", new Polygon().AsText());
		}
		[Test]
		public void ParsePoint()
		{
			string point = "POINT (20.564 346.3493254)";
			Point geom = Geometry.GeomFromText(point) as Point;
			Assert.IsNotNull(geom);
			Assert.AreEqual(20.564, geom.X);
			Assert.AreEqual(346.3493254, geom.Y);
			Assert.AreEqual(point, geom.AsText());
			Assert.IsTrue(Geometry.GeomFromText("POINT EMPTY").IsEmpty());
			Assert.AreEqual("POINT EMPTY", new Point().AsText());
		}
		[Test]
		public void ParseMultiPoint()
		{
			string multipoint = "MULTIPOINT (20.564 346.3493254, 45 32, 23 54)";
			MultiPoint geom = Geometry.GeomFromText(multipoint) as MultiPoint;
			Assert.IsNotNull(geom);
			Assert.AreEqual(20.564, geom[0].X);
			Assert.AreEqual(54, geom[2].Y);
			Assert.AreEqual(multipoint, geom.AsText());
			Assert.IsTrue(Geometry.GeomFromText("MULTIPOINT EMPTY").IsEmpty());
			Assert.AreEqual("MULTIPOINT EMPTY", new MultiPoint().AsText());
		}
	}
}
