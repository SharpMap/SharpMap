using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace UnitTests.Serialization
{
    public class ThemeTest : BaseSerializationTest
    {
        private static Dictionary<T, IStyle> CreateStylesDictionary<T>(IEnumerable<T> items,
                                                                            Func<IStyle> generator = null)
        {
            generator = generator ?? VectorStyle.CreateRandomLinealStyle;
            var res = new Dictionary<T, IStyle>();

            foreach (var item in items) res.Add(item, generator());

            return res;
        }
        
        [Test]
        public void TestUniqueValueThemeInt()
        {
            var ds = VectorStyle.CreateRandomLinealStyle();
            
            var tS = new UniqueValuesTheme<int>("Attribute", CreateStylesDictionary(new[] {1, 2, 3, 4}), ds);
            UniqueValuesTheme<int> tD = null;
            Assert.DoesNotThrow(() => tD = SandD(tS, GetFormatter()));
            
            Assert.IsNotNull(tD, "Desrialization returned null");

            TestUniqueValuesThemesEqual(tS, tD);
        }

        [Test]
        public void TestUniqueValueThemeDouble()
        {
            var ds = VectorStyle.CreateRandomLinealStyle();

            var tS = new UniqueValuesTheme<double>("Attribute", CreateStylesDictionary(new[] { 1d, 2, 3, 4 }), ds);
            UniqueValuesTheme<double> tD = null;
            Assert.DoesNotThrow(() => tD = SandD(tS, GetFormatter()));

            Assert.IsNotNull(tD, "Desrialization returned null");

            TestUniqueValuesThemesEqual(tS, tD);
        }

        [Test]
        public void TestUniqueValueThemeString()
        {
            var ds = VectorStyle.CreateRandomLinealStyle();

            var tS = new UniqueValuesTheme<string>("Attribute", CreateStylesDictionary(new[] { "Alpha", "Beta", "Gamma" }), ds);
            UniqueValuesTheme<string> tD = null;
            Assert.DoesNotThrow(() => tD = SandD(tS, GetFormatter()));

            Assert.IsNotNull(tD, "Desrialization returned null");

            TestUniqueValuesThemesEqual(tS, tD);
        }


        [Test]
        public void TestGradientTheme()
        {
            var tS = new GradientTheme("AttributeName", 10, 50, 
                VectorStyle.CreateRandomLinealStyle(),
                VectorStyle.CreateRandomLinealStyle());
            GradientTheme tD = null;
            Assert.DoesNotThrow(() => tD = SandD(tS, GetFormatter()));
            Assert.IsNotNull(tD);

            Assert.AreEqual(tS.ColumnName, tD.ColumnName);
            Assert.AreEqual(tS.Min, tD.Min, "Min");
            StylesTest.AreVectorStylesEqual((VectorStyle) tS.MinStyle, (VectorStyle) tS.MinStyle);
            Assert.AreEqual(tS.Max, tD.Max, "Max");
            StylesTest.AreVectorStylesEqual((VectorStyle) tS.MaxStyle, (VectorStyle) tD.MaxStyle);
        }

        private static void TestUniqueValuesThemesEqual<T>(UniqueValuesTheme<T> lhs, UniqueValuesTheme<T> rhs)
        {
            Assert.AreEqual(lhs.AttributeName, rhs.AttributeName, "AttributeName differs");
            var uvS = lhs.UniqueValues;
            var uvD = rhs.UniqueValues;
            Assert.AreEqual(uvS.Length, uvD.Length, "UniqueValues length differs");
            for (var i = 0; i < uvS.Length; i++)
                Assert.AreEqual(uvS[i], uvD[i], "Unique values differ at index {0} [{1}:{2}]", i, uvS[i], uvD[i]);

            Assert.IsTrue(StylesTest.AreVectorStylesEqual((VectorStyle)lhs.DefaultStyle, 
                (VectorStyle)rhs.DefaultStyle), "DefaultStyle differs");

            for (var i = 0; i < uvS.Length; i++)
            {
                Assert.IsTrue(StylesTest.AreVectorStylesEqual(
                    (VectorStyle)lhs.GetStyle(uvS.ToString()),
                    (VectorStyle)rhs.GetStyle(uvD.ToString())), "Style at {0} differs", i);
            }
        }
    }
}