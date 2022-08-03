using System;
using System.ComponentModel;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace UnitTests.Converters.WKT
{
    [TestFixture]
    public class WktGeometryTests
    {
        private static readonly GeometryFactory Factory = new GeometryFactory();
        private static readonly WKTReader WktReader = new WKTReader(Factory);

        public static Geometry GeomFromText(string wkt)
        {
            return WktReader.Read(wkt);
        }

        [Test]
        public void ParseGeometryCollection()
        {
            string geomCollection = "GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))";
            var geom = GeomFromText(geomCollection) as GeometryCollection;
            Assert.IsNotNull(geom);
            Assert.AreEqual(3, geom.NumGeometries);
            Assert.IsTrue(geom[0] is Point);
            Assert.IsTrue(geom[1] is Point);
            Assert.IsTrue(geom[2] is LineString);
            Assert.AreEqual(geomCollection, geom.AsText());
            geom = GeomFromText("GEOMETRYCOLLECTION EMPTY") as GeometryCollection;
            Assert.IsNotNull(geom);
            Assert.AreEqual(0, geom.NumGeometries);
            geomCollection = "GEOMETRYCOLLECTION (POINT (10 10), LINESTRING EMPTY, POINT (20 49))";
            geom = GeomFromText(geomCollection) as GeometryCollection;
            Assert.IsNotNull(geom);
            Assert.IsTrue(geom[1].IsEmpty);
            Assert.AreEqual(3, geom.NumGeometries);
            Assert.AreEqual(geomCollection, geom.AsText());
            Assert.AreEqual("GEOMETRYCOLLECTION EMPTY", Factory.CreateGeometryCollection(null).AsText());
        }

        [Test]
        public void ParseLineString()
        {
            string linestring = "LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)";
            var geom = GeomFromText(linestring) as MultiLineString;
            Assert.IsNotNull(geom);
            Assert.AreEqual(40, geom.Length);
            Assert.IsFalse(geom.IsRing);
            Assert.AreEqual(linestring, geom.AsText());
            Assert.IsTrue((GeomFromText("LINESTRING EMPTY") as MultiLineString).IsEmpty);
            Assert.AreEqual("LINESTRING EMPTY", Factory.CreateLineString((Coordinate[])null).AsText());
            linestring = "LINESTRING (-3.32348670085011 56.3658879665301, 5.72037855874896E-05 53.5353372259589)";
            geom = GeomFromText(linestring) as LineString;
            Assert.IsNotNull(geom);
        }

        [Test]
        public void ParseMultiLineString()
        {
            string multiLinestring = "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))";
            var geom = GeomFromText(multiLinestring) as MultiLineString;
            Assert.IsNotNull(geom);
            Assert.AreEqual(3, geom.NumGeometries);
            Assert.AreEqual(180, geom.Length);
            Assert.AreEqual(120, geom[2].Length);
            Assert.IsFalse(((MultiLineString)geom[0]).IsClosed, "[0].IsClosed");
            Assert.IsFalse(((MultiLineString)geom[1]).IsClosed, "[1].IsClosed");
            Assert.IsTrue(((MultiLineString)geom[2]).IsClosed, "[2].IsClosed");
            Assert.IsTrue(geom[0].IsSimple, "[0].IsSimple");
            Assert.IsTrue(geom[1].IsSimple, "[1].IsSimple");
            Assert.IsTrue(geom[2].IsSimple, "[2].IsSimple");
            Assert.IsTrue(((MultiLineString)geom[2]).IsRing, "Third line is a ring");
            Assert.AreEqual(multiLinestring, geom.AsText());
            Assert.IsTrue(GeomFromText("MULTILINESTRING EMPTY").IsEmpty);
            geom = GeomFromText("MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), EMPTY, (20 20, 50 20, 50 60, 20 20))") as MultiLineString;
            Assert.IsNotNull(geom);
            Assert.IsTrue(geom[2].IsEmpty);
            Assert.AreEqual(4, geom.NumGeometries);
            Assert.AreEqual("MULTILINESTRING EMPTY", Factory.CreateMultiLineString(null).AsText());
        }

        [Test]
        public void ParseMultiPoint()
        {
            string multipoint = "MULTIPOINT ((20.564 346.3493254), (45 32), (23 54))";
            var geom = GeomFromText(multipoint) as MultiPoint;
            Assert.IsNotNull(geom);
            Assert.AreEqual(20.564, ((Point)geom[0]).X);
            Assert.AreEqual(54, ((Point)geom[2]).Y);
            Assert.AreEqual(multipoint, geom.AsText());
            Assert.IsTrue(GeomFromText("MULTIPOINT EMPTY").IsEmpty);
            Assert.AreEqual("MULTIPOINT EMPTY", Factory.CreateMultiPointFromCoords((Coordinate[])null).AsText());
        }

        [Test]
        public void ParseMultipolygon()
        {
            string multipolygon = "MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), ((5 5, 7 5, 7 7, 5 7, 5 5)))";
            var geom = GeomFromText(multipolygon) as MultiPolygon;
            Assert.IsNotNull(geom);
            Assert.AreEqual(2, geom.NumGeometries);
            Assert.AreEqual(new Point(5, 5), geom[0].Centroid);
            Assert.AreEqual(multipolygon, geom.AsText());
            Assert.IsNotNull(GeomFromText("MULTIPOLYGON EMPTY"));
            Assert.IsTrue(GeomFromText("MULTIPOLYGON EMPTY").IsEmpty);
            geom = GeomFromText("MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), EMPTY, ((5 5, 7 5, 7 7, 5 7, 5 5)))") as MultiPolygon;
            Assert.IsNotNull(geom);
            Assert.IsTrue(geom[1].IsEmpty);
            Assert.AreEqual(new Point(5, 5), ((Polygon)geom[2]).ExteriorRing.EndPoint);
            Assert.AreEqual(new Point(5, 5), ((Polygon)geom[2]).ExteriorRing.StartPoint);
            Assert.AreEqual(((Polygon)geom[2]).ExteriorRing.StartPoint, ((Polygon)geom[2]).ExteriorRing.EndPoint);
            Assert.AreEqual(3, geom.NumGeometries);
            Assert.AreEqual("MULTIPOLYGON EMPTY", Factory.CreateMultiPolygon(null).AsText());
        }

        [Test]
        public void ParsePoint()
        {
            string point = "POINT (20.564 346.3493254)";
            var geom = GeomFromText(point) as Point;
            Assert.IsNotNull(geom);
            Assert.AreEqual(20.564, geom.X);
            Assert.AreEqual(346.3493254, geom.Y);
            Assert.AreEqual(point, geom.AsText());
            Assert.IsTrue(GeomFromText("POINT EMPTY").IsEmpty);
            Assert.AreEqual("POINT EMPTY", Factory.CreatePoint((Coordinate)null).AsText());
        }

        [Test]
        public void ParsePolygon()
        {
            string polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20))";
            var geom = GeomFromText(polygon) as Polygon;
            Assert.IsNotNull(geom);
            Assert.AreEqual(40, geom.ExteriorRing.Length);
            Assert.AreEqual(100, geom.Area);
            Assert.AreEqual(polygon, geom.AsText());
            //Test interior rings
            polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 29 21, 29 29, 21 29, 21 21), (23 23, 23 27, 27 27, 27 23, 23 23))";
            geom = GeomFromText(polygon) as Polygon;
            Assert.IsNotNull(geom);
            Assert.AreEqual(88, geom.Length);            
            Assert.AreEqual(20, geom.Area);            
            LinearRing exteriorRing = geom.ExteriorRing as LinearRing;
            Assert.IsNotNull(exteriorRing);
            Assert.AreEqual(40, exteriorRing.Length);
            Assert.AreEqual(0, exteriorRing.Area);
            Polygon exteriorPoly = exteriorRing.Factory.CreatePolygon(exteriorRing, null);
            Assert.IsNotNull(exteriorPoly);
            Assert.AreEqual(100, exteriorPoly.Area);
            Assert.AreEqual(2, geom.NumInteriorRings);
            LinearRing interiorRingOne = geom.InteriorRings[0] as LinearRing;
            Assert.IsNotNull(interiorRingOne);
            Assert.AreEqual(32, interiorRingOne.Length);
            Assert.AreEqual(0, interiorRingOne.Area);
            Polygon interiorPolyOne = interiorRingOne.Factory.CreatePolygon(interiorRingOne, null);
            Assert.IsNotNull(interiorPolyOne);
            Assert.AreEqual(64, interiorPolyOne.Area);
            LinearRing interiorRingTwo = geom.InteriorRings[1] as LinearRing;
            Assert.IsNotNull(interiorRingTwo);
            Assert.AreEqual(16, interiorRingTwo.Length);
            Assert.AreEqual(0, interiorRingTwo.Area);
            Polygon interiorPolyTwo = interiorRingTwo.Factory.CreatePolygon(interiorRingTwo, null);
            Assert.IsNotNull(interiorPolyTwo);
            Assert.AreEqual(16, interiorPolyTwo.Area);
            Assert.AreEqual(exteriorPoly.Area - interiorPolyOne.Area - interiorPolyTwo.Area, geom.Area);
            Assert.AreEqual(polygon, geom.AsText());
            //Test empty geometry WKT
            Assert.IsTrue(GeomFromText("POLYGON EMPTY").IsEmpty);
            Assert.AreEqual("POLYGON EMPTY", Factory.CreatePolygon(null, null).AsText());
        }

        
    }
}
