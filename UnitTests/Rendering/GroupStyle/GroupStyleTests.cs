using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using SharpMap.Layers;
using SharpMap;

namespace UnitTests.Rendering.GroupStyle
{
     [TestFixture]
    public class GroupStyleTests
    {
         [Test]
         public void TestAddRemove()
         {
             var style = new SharpMap.Styles.GroupStyle();
             var vStyle = new SharpMap.Styles.VectorStyle()
             {
                 Enabled = true,
                 PointColor = Brushes.Red,
                 PointSize = 3
             };
             style.AddStyle(vStyle);

             vStyle = new SharpMap.Styles.VectorStyle()
             {
                 Enabled = true,
                 PointColor = Brushes.White,
                 PointSize = 1
             };
             style.AddStyle(vStyle);

             Assert.AreEqual(2, style.Count);
             Assert.AreEqual(Color.Red, (style[0].PointColor as SolidBrush).Color);
         }

         [Test]
         public void TestRender()
         {
             var style = new SharpMap.Styles.GroupStyle();
             var vStyle = new SharpMap.Styles.VectorStyle()
             {
                 Enabled = true,
                 PointColor = Brushes.Red,
                 PointSize = 6
             };
             style.AddStyle(vStyle);

             vStyle = new SharpMap.Styles.VectorStyle()
             {
                 Enabled = true,
                 PointColor = Brushes.White,
                 PointSize = 2
             };
             style.AddStyle(vStyle);

             Assert.AreEqual(2, style.Count);
             Assert.AreEqual(Color.Red, (style[0].PointColor as SolidBrush).Color);

             VectorLayer vLay = new VectorLayer("test");
             vLay.Style = style;

             vLay.DataSource = new SharpMap.Data.Providers.GeometryProvider("POINT(0 0)");

             Map m = new Map(new Size(11, 11));
             m.BackColor = Color.White;
             m.ZoomToBox(new GeoAPI.Geometries.Envelope(-5, 5, -5, 5));
             m.Layers.Add(vLay);
             var img = m.GetMap();

             //img.Save(@"c:\\temp\ren.png");

             Bitmap bmp = img as Bitmap;
             Color c1 = bmp.GetPixel(5, 5);
             Assert.AreEqual(Color.White.ToArgb(), c1.ToArgb());
             c1 = bmp.GetPixel(3, 5);
             Assert.AreEqual(Color.Red.ToArgb(), c1.ToArgb());
             c1 = bmp.GetPixel(7, 5);
             Assert.AreEqual(Color.Red.ToArgb(), c1.ToArgb());
             c1 = bmp.GetPixel(5, 3);
             Assert.AreEqual(Color.Red.ToArgb(), c1.ToArgb());
             c1 = bmp.GetPixel(5, 7);
             Assert.AreEqual(Color.Red.ToArgb(), c1.ToArgb());
         
         }
    }
}
