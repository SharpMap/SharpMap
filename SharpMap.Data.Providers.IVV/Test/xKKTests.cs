using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

#if DEBUG

namespace SharpMap.Test
{
    public class xKKTests
    {
        [TestCase(@"D:\temp\BAM\P_2025_151217\BAM_p0.zkk")]
        public void TestxKK(string filename)
        {
            xKKProvider p = null;

            Assert.DoesNotThrow(() => p = xKKProvider.Load(filename));
            Assert.That(p != null, "p != null");
            Assert.That(p.ConnectionID == filename);

            var m = new Map();
            m.Layers.Add(new VectorLayer(System.IO.Path.GetFileName(filename), p)
                { Style = new VectorStyle{PointSize = 2f}});
            
            m.ZoomToExtents();
            using (var img = m.GetMap())
            { img.Save(System.IO.Path.ChangeExtension(filename, "png"));}
        }

        [TestCase(@"D:\temp\BAM\P_2025_151217\BAM_p0.zpz")]
        public void TestZPZ(string filename)
        {
            xKKProvider p = null;

            Assert.DoesNotThrow(() => p = xKKProvider.Load(System.IO.Path.ChangeExtension(filename, "zkk")));
            Assert.That(p != null, "p != null");

            ZPZProvider p2 = null;
            Assert.DoesNotThrow(() => p2 = ZPZProvider.Load(filename, p));
            Assert.That(p != null, "p2 != null");

            var m = new Map();
            m.Layers.Add(new VectorLayer(System.IO.Path.GetFileName(filename), p2) { Style = new VectorStyle
            {
                Fill = new SolidBrush(Color.Lavender),
                Outline = Pens.DodgerBlue, EnableOutline = true
            } });

            m.ZoomToExtents();
            using (var img = m.GetMap())
            { img.Save(System.IO.Path.ChangeExtension(filename, "png")); }
        }


        [TestCase(@"D:\temp\BAM\P_2025_151217\BAM_p0.zgs")]
        public void TestZGS(string filename)
        {
            xKKProvider p = null;

            Assert.DoesNotThrow(() => p = xKKProvider.Load(System.IO.Path.ChangeExtension(filename, "zkk")));
            Assert.That(p != null, "p != null");
            //Assert.That(p.ConnectionID == filename);

            xxSProvider<ZGS> p2 = null;
            Assert.DoesNotThrow(() => p2 = xxSProvider<ZGS>.Load(filename, ZGS.Create, p));
            Assert.That(p2 != null, "p != null");


            var m = new Map();
            var styles = new Dictionary<short, IStyle>();
            styles.Add(1, new VectorStyle{Line = Pens.Gainsboro });
            styles.Add(2, new VectorStyle{Line = Pens.Gainsboro });
            styles.Add(3, new VectorStyle{Line = Pens.DarkGray});
            styles.Add(4, new VectorStyle{Line = Pens.Black });
            styles.Add(5, new VectorStyle{Line = new Pen(Color.Green, 2)});
            styles.Add(6, new VectorStyle{Line = new Pen(Color.OrangeRed, 3) });
            styles.Add(7, new VectorStyle{Line = new Pen(Color.Blue, 4) {EndCap = LineCap.Round, StartCap = LineCap.Round}});
            m.Layers.Add(new VectorLayer(System.IO.Path.GetFileName(filename), p2)
            {
                Theme = new UniqueValuesTheme<short>("ART",styles, new VectorStyle { Line = new Pen(Color.Aqua, 1) { DashStyle = DashStyle.Dot}})
            });

            m.ZoomToExtents();
            using (var img = m.GetMap())
            { img.Save(System.IO.Path.ChangeExtension(filename, "png")); }
        }
    }
}

#endif