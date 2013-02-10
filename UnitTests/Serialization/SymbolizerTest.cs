using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpMap.Rendering.Symbolizer;

namespace UnitTests.Serialization
{
    [TestFixture]
    public class SymbolizerTest : BaseSerializationTest
    {
        [Test]
        public void TestCharacterPointSymbolizer()
        {
            var cps = new CharacterPointSymbolizer();
            cps.CharacterIndex = (int) 'q';
            cps.Font = new Font(FontFamily.GenericSansSerif, 12f, FontStyle.Bold);
            cps.Foreground = new SolidBrush(Color.BlueViolet);
            cps.Halo = 2;
            cps.HaloBrush = new SolidBrush(Color.BlanchedAlmond);
            cps.Offset = new PointF(6f, 6f);

            CharacterPointSymbolizer cpsD = null;
            Assert.DoesNotThrow(() => cpsD = SandD(cps, GetFormatter()));

            var e = new CharacterPointSymbolizerEqualityComparer();
            Assert.IsTrue(e.Equals(cps, cpsD));
        }
    }

    internal class SymbolizerEqualityComparer<T> : EqualityComparer<T> where T : ISymbolizer
    {
        public override bool Equals(T lhs, T rhs)
        {
            if (lhs.PixelOffsetMode != rhs.PixelOffsetMode)
                return false;
            if (lhs.SmoothingMode != rhs.SmoothingMode)
                return false;
            return true;
        }

        public override int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class PointSymbolizerEqualityComparer<T> : SymbolizerEqualityComparer<T>
        where T: PointSymbolizer
    {
        public override bool Equals(T lhs, T rhs)
        {
            if (lhs.Offset != rhs.Offset)
                return false;

            if (lhs.Rotation != rhs.Rotation)
                return false;

            if (lhs.Scale != rhs.Scale)
                return false;

            if (lhs.Size != rhs.Size)
                return false;

            return base.Equals(lhs, rhs);
        }
    }

    internal class CharacterPointSymbolizerEqualityComparer : PointSymbolizerEqualityComparer<CharacterPointSymbolizer>
    {
        public override bool Equals(CharacterPointSymbolizer lhs, CharacterPointSymbolizer rhs)
        {
            if (lhs.CharacterIndex != rhs.CharacterIndex)
                return false;

            if (!lhs.Font.Equals(rhs.Font))
                return false;

            if (((SolidBrush)lhs.Foreground).Color != ((SolidBrush)rhs.Foreground).Color)
                return false;

            if (lhs.Halo != rhs.Halo)
                return false;

            if (lhs.HaloBrush != null ^ rhs.HaloBrush != null)
                return false;

            if (lhs.HaloBrush != null)
            {
                if (((SolidBrush) lhs.HaloBrush).Color != ((SolidBrush) rhs.HaloBrush).Color)
                    return false;
            }

            if (lhs.StringFormat.FormatFlags != rhs.StringFormat.FormatFlags)
                return false;

            return base.Equals(lhs, rhs);
        }
    }
}