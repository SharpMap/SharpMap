using System.IO;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Converters.WellKnownText;
namespace UnitTests.Converters.WKB
{
    [TestFixture]
    public class WKBTests
    {
        private static readonly IGeometryFactory Factory = new GeometryFactory();
        
        private string point = "POINT (20.564 346.3493254)";
        private string multipoint = "MULTIPOINT (20.564 346.3493254, 45 32, 23 54)";
        private string linestring = "LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)";

        private string multiLinestring =
            "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))";

        private string polygon = "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 21 29, 29 29, 29 21, 21 21))";

        private IGeometry ToBinaryAndBack(IGeometry gIn, WkbByteOrder byteOrder)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(GeometryToWKB.Write(gIn, byteOrder));
            BinaryReader reader = new BinaryReader(stream);
            stream.Position = 0;
            var gOut = GeometryFromWKB.Parse(reader, Factory);
            return gOut;
        }

        [Test]
        public void BinaryStream()
        {
            //Prepare data
            var gPn = GeometryFromWKT.Parse(point);
            var gMp = GeometryFromWKT.Parse(multipoint);
            var gLi = GeometryFromWKT.Parse(linestring);
            var gML = GeometryFromWKT.Parse(multiLinestring);
            var gPl = GeometryFromWKT.Parse(polygon);

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
            var gPn0 = GeometryFromWKT.Parse(point);
            var gMp0 = GeometryFromWKT.Parse(multipoint);
            var gLi0 = GeometryFromWKT.Parse(linestring);
            var gML0 = GeometryFromWKT.Parse(multiLinestring);
            var gPl0 = GeometryFromWKT.Parse(polygon);

            var gPn1 = GeometryFromWKB.Parse(gPn0.AsBinary(), gPn0.Factory);
            var gMp1 = GeometryFromWKB.Parse(gMp0.AsBinary(), gPn0.Factory);
            var gLi1 = GeometryFromWKB.Parse(gLi0.AsBinary(), gPn0.Factory);
            var gML1 = GeometryFromWKB.Parse(gML0.AsBinary(), gPn0.Factory);
            var gPl1 = GeometryFromWKB.Parse(gPl0.AsBinary(), gPn0.Factory);

            Assert.AreEqual(gPn0, gPn1);
            Assert.AreEqual(gMp0, gMp1);
            Assert.AreEqual(gLi0, gLi1);
            Assert.AreEqual(gML0, gML1);
            Assert.AreEqual(gPl0, gPl1);
        }
    }
}