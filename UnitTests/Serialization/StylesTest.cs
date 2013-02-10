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

            AreStylesEqual(styleS, styleD);
        }

        [Test]
        public void TestVectorStyle()
        {
            var vsS = VectorStyle.CreateRandomStyle();
            var vsD = SandD(vsS, GetFormatter());

            Assert.IsTrue(AreVectorStylesEqual(vsS, vsD));
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

            Assert.IsTrue(AreLabelStylesEqual(lsS, lsD));
        }

        public static bool AreStylesEqual(IStyle lhs, IStyle rhs)
        {
            Assert.AreEqual(lhs.MinVisible, rhs.MinVisible, "MinVisible differs");
            Assert.AreEqual(lhs.MaxVisible, rhs.MaxVisible, "MaxVisible differs");
            Assert.AreEqual(lhs.Enabled, rhs.Enabled, "Enabled differs");
            return true;
        }

        public static bool AreVectorStylesEqual(VectorStyle lhs, VectorStyle rhs)
        {
            AreStylesEqual(lhs, rhs);

            Assert.AreEqual(lhs.PointSize, rhs.PointSize);
            Assert.AreEqual(((SolidBrush)lhs.PointColor).Color, ((SolidBrush)rhs.PointColor).Color);

            SurrogatesTest.ComparePens(lhs.Line, rhs.Line, "Line");
            Assert.AreEqual(lhs.Line.PenType, rhs.Line.PenType);
            Assert.AreEqual(lhs.Line.Color, rhs.Line.Color);
            Assert.AreEqual(lhs.EnableOutline, rhs.EnableOutline);
            Assert.AreEqual(lhs.Outline.PenType, rhs.Outline.PenType);
            Assert.AreEqual(lhs.Outline.Color, rhs.Outline.Color);

            Assert.AreEqual(lhs.Fill.GetType(), rhs.Fill.GetType());

            //ToDo: Test more properties
            return true;
        }

        public static bool AreLabelStylesEqual(LabelStyle lhs, LabelStyle rhs)
        {
            Assert.AreEqual(lhs.CollisionBuffer, rhs.CollisionBuffer, "CollisionBuffer differs");
            Assert.AreEqual(lhs.CollisionDetection, rhs.CollisionDetection, "CollisionDetection differs");
            Assert.AreEqual(lhs.Font, rhs.Font, "Font differs");
            Assert.AreEqual(lhs.ForeColor, rhs.ForeColor, "ForeColor differs");
            Assert.AreEqual(((SolidBrush)lhs.BackColor).Color, ((SolidBrush)rhs.BackColor).Color, "BackColor differs");
            SurrogatesTest.ComparePens(lhs.Halo, rhs.Halo);

            //ToDo Test more properties
            return true;
        }

    }
}