using System.IO;
using NUnit.Framework;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Converters.WellKnownText;
using SharpMap.Geometries;

namespace UnitTests.Converters.WKB
{
    [TestFixture]
    public class WKBTests
    {
        private string point = "POINT (20.564 346.3493254)";
        private string multipoint = "MULTIPOINT (20.564 346.3493254, 45 32, 23 54)";
        private string linestring = "LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)";

        private string multiLinestring =
            "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))";

        private string polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 21 29, 29 29, 29 21, 21 21))";

        private Geometry ToBinaryAndBack(Geometry gIn, WkbByteOrder byteOrder)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(GeometryToWKB.Write(gIn, byteOrder));
            BinaryReader reader = new BinaryReader(stream);
            stream.Position = 0;
            Geometry gOut = GeometryFromWKB.Parse(reader);
            return gOut;
        }

        [Test]
        public void BinaryStream()
        {
            //Prepare data
            Geometry gPn = GeometryFromWKT.Parse(point);
            Geometry gMp = GeometryFromWKT.Parse(multipoint);
            Geometry gLi = GeometryFromWKT.Parse(linestring);
            Geometry gML = GeometryFromWKT.Parse(multiLinestring);
            Geometry gPl = GeometryFromWKT.Parse(polygon);

            //Test with Xdr
            Assert.AreEqual(gPn, ToBinaryAndBack(gPn, WkbByteOrder.Xdr));
            Assert.AreEqual(gMp, ToBinaryAndBack(gMp, WkbByteOrder.Xdr));
            Assert.AreEqual(gLi, ToBinaryAndBack(gLi, WkbByteOrder.Xdr));
            Assert.AreEqual(gML, ToBinaryAndBack(gML, WkbByteOrder.Xdr));
            Assert.AreEqual(gPl, ToBinaryAndBack(gPl, WkbByteOrder.Xdr));

            //Test with Ndr
            Assert.AreEqual(gPn, ToBinaryAndBack(gPn, WkbByteOrder.Ndr));
            Assert.AreEqual(gMp, ToBinaryAndBack(gMp, WkbByteOrder.Ndr));
            Assert.AreEqual(gLi, ToBinaryAndBack(gLi, WkbByteOrder.Ndr));
            Assert.AreEqual(gML, ToBinaryAndBack(gML, WkbByteOrder.Ndr));
            Assert.AreEqual(gPl, ToBinaryAndBack(gPl, WkbByteOrder.Ndr));
        }

        [Test]
        public void Convert()
        {
            Geometry gPn0 = Geometry.GeomFromText(point);
            Geometry gMp0 = Geometry.GeomFromText(multipoint);
            Geometry gLi0 = Geometry.GeomFromText(linestring);
            Geometry gML0 = Geometry.GeomFromText(multiLinestring);
            Geometry gPl0 = Geometry.GeomFromText(polygon);

            Geometry gPn1 = Geometry.GeomFromWKB(gPn0.AsBinary());
            Geometry gMp1 = Geometry.GeomFromWKB(gMp0.AsBinary());
            Geometry gLi1 = Geometry.GeomFromWKB(gLi0.AsBinary());
            Geometry gML1 = Geometry.GeomFromWKB(gML0.AsBinary());
            Geometry gPl1 = Geometry.GeomFromWKB(gPl0.AsBinary());

            Assert.AreEqual(gPn0, gPn1);
            Assert.AreEqual(gMp0, gMp1);
            Assert.AreEqual(gLi0, gLi1);
            Assert.AreEqual(gML0, gML1);
            Assert.AreEqual(gPl0, gPl1);
        }
    }
}