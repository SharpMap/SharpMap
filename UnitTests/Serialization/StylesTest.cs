using System;
using System.Collections.Generic;
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

#if LINUX
        [Ignore("Might fail on ColorBlend")]
#endif
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
            Assert.IsTrue(new FontEqualityComparer().Equals(lhs.Font, rhs.Font), "Font differs");
            Assert.AreEqual(lhs.ForeColor, rhs.ForeColor, "ForeColor differs");
            Assert.AreEqual(((SolidBrush)lhs.BackColor).Color, ((SolidBrush)rhs.BackColor).Color, "BackColor differs");
            SurrogatesTest.ComparePens(lhs.Halo, rhs.Halo);

            //ToDo Test more properties
            return true;
        }

    }

    internal class FontEqualityComparer : EqualityComparer<Font>
    {
        public override bool Equals(Font x, Font y)
        {
            if (x == null && y == null)
                return true;

            if (x == null) return false;
            if (y == null) return false;

            if (x.Bold != y.Bold)
                return false;

            if (x.IsSystemFont != y.IsSystemFont)
                return false;

            if (x.Italic != y.Italic)
                return false;

            if (x.Strikeout != y.Strikeout)
                return false;

            if (x.Style != y.Style)
                return false;

            if (x.Underline != y.Underline)
                return false;

            if (x.FontFamily.Name != y.FontFamily.Name)
                return false;

            if (x.GdiCharSet != y.GdiCharSet)
                return false;

            if (x.Height != y.Height)
                return false;
            if (x.Unit != y.Unit)
                return false;
            if (x.Name != y.Name)
                return false;
            if (!string.IsNullOrEmpty(x.OriginalFontName))
                if (x.OriginalFontName != y.OriginalFontName)
                    return false;
            if (x.SystemFontName != y.SystemFontName)
                return false;
            if (x.Size != y.Size)
                return false;
            if (x.SizeInPoints != y.SizeInPoints)
                return false;

            return true;
        }

        public override int GetHashCode(Font obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return obj.GetHashCode();
        }
    }


}
