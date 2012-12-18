using System.Drawing;
using NUnit.Framework;
using SharpMap.Styles;

namespace UnitTests.Serialization
{
    public class StylesTest : BaseSerializationTest
    {
        [Test]
        public void TestStyle()
        {
            var styleS = new Style();
            styleS.MinVisible = 2;
            styleS.MaxVisible = 100;
            styleS.Enabled = false;

            var styleD = SandD(styleS, GetFormatter());

            Assert.AreEqual(2d, styleD.MinVisible);
            Assert.AreEqual(100, styleD.MaxVisible);
            Assert.AreEqual(false, styleD.Enabled);
        }

        [Test]
        public void TestVectorStyle()
        {
            var vsS = VectorStyle.CreateRandomStyle();
            var vsD = SandD(vsS, GetFormatter());

            Assert.AreEqual(vsS.PointSize, vsD.PointSize);
            Assert.AreEqual(((SolidBrush)vsD.PointColor).Color, ((SolidBrush)vsD.PointColor).Color);

            Assert.AreEqual(vsS.Line.PenType, vsD.Line.PenType);
            Assert.AreEqual(vsS.Line.Color, vsD.Line.Color);
            Assert.AreEqual(vsS.EnableOutline, vsD.EnableOutline);
            Assert.AreEqual(vsS.Outline.PenType, vsD.Outline.PenType);
            Assert.AreEqual(vsS.Outline.Color, vsD.Outline.Color);
            
            Assert.AreEqual(vsS.Fill.GetType(), vsD.Fill.GetType());

            //ToDo: Test more properties
        }

        [Test]
        public void TestLabelStyle()
        {
            var lsS = new LabelStyle();
            lsS.CollisionBuffer = new SizeF(3, 3);
            lsS.CollisionDetection = true;
            lsS.Font = new Font(FontFamily.GenericMonospace, 14);
            lsS.ForeColor = Color.Brown;
            lsS.BackColor = new SolidBrush(Color.AntiqueWhite);
            lsS.Halo = new Pen(Color.Aquamarine, 2f);

            var lsD = SandD(lsS, GetFormatter());

            Assert.AreEqual(lsS.CollisionBuffer, lsD.CollisionBuffer);
            Assert.AreEqual(lsS.CollisionDetection, lsD.CollisionDetection);
            Assert.AreEqual(lsS.Font, lsD.Font);
            Assert.AreEqual(lsS.ForeColor, lsD.ForeColor);
            Assert.AreEqual(((SolidBrush)lsS.BackColor).Color, ((SolidBrush)lsD.BackColor).Color);
            Assert.AreEqual(lsS.Halo.Color, lsD.Halo.Color);
            Assert.AreEqual(lsS.Halo.Width, lsD.Halo.Width);

            //ToDo Test more properties
        }

    }
}