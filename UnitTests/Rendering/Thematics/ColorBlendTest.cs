using System.Drawing;
using SharpMap.Rendering.Thematics;
using NUnit.Framework;
namespace UnitTests.Rendering.Thematics
{
    [TestFixture]
    public class ColorBlendTest
    {
        [Test]
        public void ToBrushTest()
        {
            ColorBlend colorBlend = new ColorBlend(new Color[] {Color.Blue, Color.GreenYellow, Color.Red}, new float[] {0f, 500f, 1000f} );
            Assert.IsNotNull(colorBlend);
            System.Drawing.Drawing2D.LinearGradientBrush lgb = colorBlend.ToBrush(new Rectangle(0, 0, 100, 100), 0.5f);
            Assert.IsNotNull(lgb);
            Assert.AreEqual(1.0f, lgb.InterpolationColors.Positions[2]);
        }
    }
}