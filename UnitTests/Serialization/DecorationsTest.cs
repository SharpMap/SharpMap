using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpMap.Rendering.Decoration;

namespace UnitTests.Serialization
{
    public class DecorationsTest : BaseSerializationTest
    {
        [Test]
        public void NorthArrowTest()
        {
            var northArrowS = new NorthArrow();
            NorthArrow northArrowD = null;
            
            Assert.DoesNotThrow(() => northArrowD = SandD(northArrowS, GetFormatter()));
            Assert.IsNotNull(northArrowD);

            var eqc = new NorthArrowEqualityComparer();
            Assert.IsTrue(eqc.Equals(northArrowS, northArrowD), eqc.ToString());

        }

        [Test]
        public void DisclaimerTest()
        {
            var northArrowS = new Disclaimer();
            Disclaimer northArrowD = null;

            Assert.DoesNotThrow(() => northArrowD = SandD(northArrowS, GetFormatter()));
            Assert.IsNotNull(northArrowD);

            var eqc = new DisclaimerEqualityComparer();
            Assert.IsTrue(eqc.Equals(northArrowS, northArrowD), eqc.ToString());

        }

        [Test]
        public void EyeOfSightTest()
        {
            var northArrowS = new EyeOfSight();
            EyeOfSight northArrowD = null;

            Assert.DoesNotThrow(() => northArrowD = SandD(northArrowS, GetFormatter()));
            Assert.IsNotNull(northArrowD);

            var eqc = new EyeOfSightEqualityComparer();
            Assert.IsTrue(eqc.Equals(northArrowS, northArrowD), eqc.ToString());

        }

        private class DecorationEqualityComparer<T> : EqualityComparer<T> 
            where T:MapDecoration
        {
            
            protected readonly List<string> DifferAt = new List<string>();

            public override bool Equals(T lhs, T rhs)
            {
                var equal = true;
                if (!(rhs.GetType() == lhs.GetType()))
                {
                    equal = false;
                    DifferAt.Add("Type");
                }

                if (lhs.Anchor != rhs.Anchor)
                {
                    equal = false;
                    DifferAt.Add("Anchor");
                }

                if (lhs.BackgroundColor != rhs.BackgroundColor)
                {
                    equal = false;
                    DifferAt.Add("BackgroundColor");
                }

                if (lhs.BorderColor != rhs.BorderColor)
                {
                    equal = false;
                    DifferAt.Add("BorderColor");
                }

                if (lhs.BorderMargin != rhs.BorderMargin)
                {
                    equal = false;
                    DifferAt.Add("BorderMargin");
                }

                if (lhs.BorderWidth != rhs.BorderWidth)
                {
                    equal = false;
                    DifferAt.Add("BorderWidth");
                }

                if (lhs.Enabled != rhs.Enabled)
                {
                    equal = false;
                    DifferAt.Add("Enabled");
                }

                if (lhs.Location != rhs.Location)
                {
                    equal = false;
                    DifferAt.Add("Location");
                }

                if (lhs.Opacity != rhs.Opacity)
                {
                    equal = false;
                    DifferAt.Add("Opacity");
                }

                if (lhs.Padding != rhs.Padding)
                {
                    equal = false;
                    DifferAt.Add("Padding");
                }

                if (lhs.Padding != rhs.Padding)
                {
                    equal = false;
                    DifferAt.Add("Padding");
                }
                return equal;
            }

            public sealed override int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }

            public override string ToString()
            {
                if (DifferAt.Count == 0)
                    return typeof(T).Name + "s are equal!";
                return string.Format("Decorations differ at \n - {0}", string.Join(", \n - ", DifferAt));
            }
        }

        private class NorthArrowEqualityComparer : DecorationEqualityComparer<NorthArrow> 
        {
            public override bool Equals(NorthArrow x, NorthArrow y)
            {
                var result = base.Equals(x, y);
                if (x.ForeColor != y.ForeColor)
                {
                    DifferAt.Add("ForeColor");
                    result = false;
                }

                if (x.NorthArrowImage == null ^ y.NorthArrowImage == null)
                {
                    DifferAt.Add("NorthArrowImage (== null)");
                    result = false;
                }

                if (x.NorthArrowImage != null && x.GetHashCode() != y.GetHashCode())
                {
                    if (x.GetHashCode() != y.GetHashCode())
                    DifferAt.Add("NorthArrowImage (GetHashCode)");
                    result = false;
                }

                if (x.Size != y.Size)
                {
                    DifferAt.Add("Size");
                    result = false;
                }

                return result;
            }
        }

        private class DisclaimerEqualityComparer : DecorationEqualityComparer<Disclaimer>
        {
            public override bool Equals(Disclaimer lhs, Disclaimer rhs)
            {
                var result = base.Equals(lhs, rhs);
                
                if (!lhs.Font.Equals(rhs.Font))
                {
                    DifferAt.Add("Font");
                    result = false;
                }

                if (lhs.ForeColor != rhs.ForeColor)
                {
                    DifferAt.Add("ForeColor");
                    result = false;
                }
                if (!StringFormatsEqual(lhs.Format, rhs.Format))
                {
                    DifferAt.Add("Format");
                    result = false;
                }

                if (lhs.Halo != rhs.Halo)
                {
                    DifferAt.Add("Halo");
                    result = false;
                }
                if (lhs.HaloColor != rhs.HaloColor)
                {
                    DifferAt.Add("HaloColor");
                    result = false;
                }

                if (!string.Equals(lhs.Text, rhs.Text))
                {
                    DifferAt.Add("Text");
                    result = false;
                }
                return result;
            }

            private static bool StringFormatsEqual(StringFormat lhs, StringFormat rhs)
            {
                if (lhs == rhs)
                    return true;

                if (lhs.Alignment != rhs.Alignment) return false;
                if (lhs.DigitSubstitutionLanguage != rhs.DigitSubstitutionLanguage) return false;
                if (lhs.DigitSubstitutionMethod != rhs.DigitSubstitutionMethod) return false;
                if (lhs.FormatFlags != rhs.FormatFlags) return false;
                if (lhs.HotkeyPrefix!= rhs.HotkeyPrefix) return false;
                if (lhs.LineAlignment != rhs.LineAlignment) return false;
                if (lhs.Trimming != rhs.Trimming) return false;

                float flhs;
                var lhst = lhs.GetTabStops(out flhs);
                float frhs;
                var rhst = lhs.GetTabStops(out frhs);
                if (flhs != frhs) return false;

                if (lhst.Length != rhst.Length) return false;
                for (var i = 0; i < lhst.Length; i++)
                    if (lhst[i] != rhst[i]) return false;

                return true;
            }
        }

        private class EyeOfSightEqualityComparer : DecorationEqualityComparer<EyeOfSight>
        {
            public override bool Equals(EyeOfSight lhs, EyeOfSight rhs)
            {
                var result = base.Equals(lhs, rhs);

                if (lhs.NeedleFillColor != rhs.NeedleFillColor)
                {
                    DifferAt.Add("NeedleFillColor");
                    result = false;
                }
                if (lhs.NeedleOutlineColor != rhs.NeedleOutlineColor)
                {
                    DifferAt.Add("NeedleOutlineColor");
                    result = false;
                }
                if (lhs.NeedleOutlineWidth != rhs.NeedleOutlineWidth)
                {
                    DifferAt.Add("NeedleOutlineWidth");
                    result = false;
                }
                return result;
            }
        }
    }
}