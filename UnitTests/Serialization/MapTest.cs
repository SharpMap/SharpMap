using System.Drawing;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace UnitTests.Serialization
{
    public class MapTest : BaseSerializationTest
    {
        [Test] 
        public void TestMap()
        {
            var m = new Map(new Size(640, 480));
            m.Layers.Add(new VectorLayer("tmp", new GeometryProvider(
                new LineString(new [] {new Coordinate(0, 0), new Coordinate(10, 10), }))));

            m.ZoomToExtents();

            Map mD = null;
            Assert.DoesNotThrow(()=>mD=SandD(m, GetFormatter()));
            Assert.NotNull(mD);

            Assert.AreEqual(m.Size, mD.Size);
            Assert.AreEqual(m.Layers.Count, mD.Layers.Count);
            var c = new LayerTest.VectorLayerEqualityComparer();
            Assert.IsTrue(c.Equals((VectorLayer)m.Layers[0], (VectorLayer)mD.Layers[0]));

            Assert.AreEqual(m.PixelAspectRatio, mD.PixelAspectRatio);
            Assert.AreEqual(m.PixelHeight, mD.PixelHeight);
            Assert.AreEqual(m.PixelWidth, mD.PixelWidth);
            Assert.AreEqual(m.PixelSize, mD.PixelSize);

            Assert.AreEqual(m.BackColor, mD.BackColor);
            Assert.IsTrue(m.Center.Equals(mD.Center));
            Assert.IsTrue(m.GetExtents().Equals(mD.GetExtents()));
            Assert.IsTrue(m.Envelope.Equals(mD.Envelope));

            Assert.AreEqual(m.Decorations.Count, mD.Decorations.Count);
            Assert.AreEqual(m.SRID, mD.SRID);
            Assert.AreEqual(m.Zoom, mD.Zoom);

            Assert.DoesNotThrow(() => mD.GetMap().Save("test.bmp"));
        }
    }
}