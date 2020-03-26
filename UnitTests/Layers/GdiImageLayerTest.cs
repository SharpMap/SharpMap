using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;

namespace UnitTests.Layers
{
    public class GdiImageLayerTest
    {
        internal static string CreateImage(Size size, PointF? origin = null)
        {
            var img = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            var tmpFile = Path.ChangeExtension(Path.GetTempFileName(), ".png");

            using (var g = Graphics.FromImage(img))
            {
                g.FillRectangle(Brushes.Red, new Rectangle(0, 0, size.Width, size.Height));
            }

            img.Save(tmpFile, ImageFormat.Png);

            CreateWorldFile(tmpFile, size, origin);

            return tmpFile;
        }

        private static void CreateWorldFile(string imageFile, Size size, PointF? origin)
        {
            if (origin == null) return;
            
            var ext = Path.GetExtension(imageFile);
            if (ext == null)
                throw new ArgumentException("imageFile");
            ext = ext.Substring(0, 2) + ext[3] + 'w';

            var worldfile = Path.ChangeExtension(imageFile, ext);
            using (var sw = new StreamWriter(worldfile))
            {
                sw.WriteLine(1d.ToString(NumberFormatInfo.InvariantInfo));
                sw.WriteLine(0d.ToString(NumberFormatInfo.InvariantInfo));
                sw.WriteLine(0d.ToString(NumberFormatInfo.InvariantInfo));
                sw.WriteLine((-1d).ToString(NumberFormatInfo.InvariantInfo));
                sw.WriteLine(origin.Value.X.ToString(NumberFormatInfo.InvariantInfo));
                sw.WriteLine((origin.Value.Y + size.Height).ToString(NumberFormatInfo.InvariantInfo));
            }
        }

        internal static void DeleteTmpFiles(string imageFile)
        {
            if (File.Exists(imageFile))
            {
                foreach (var file in Directory.GetFiles(Path.GetTempPath(), 
                    Path.GetFileNameWithoutExtension(imageFile) + ".*"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.Write(ex);
                        System.Diagnostics.Trace.WriteLine("");
                        //throw;
                    }
                }
            }
        }

        [Test]
        public void TestGdiImageLayer1()
        {
            var imageFile = CreateImage(new Size(512, 256));
            GdiImageLayer l = null;
            
            Assert.DoesNotThrow( () => l = new GdiImageLayer(imageFile));

            Assert.NotNull(l);
            Assert.AreEqual(Path.GetFileName(imageFile), l.LayerName);
            var e = new Envelope(0, 512, 0, 256);
            Assert.AreEqual(e, l.Envelope);
            l.Dispose();

            DeleteTmpFiles(imageFile);
        }

        [Test]
        public void TestGdiImageLayer2()
        {
            var imageFile = CreateImage(new Size(512, 256), new Point(10, 10));
            GdiImageLayer l = null;

            Assert.DoesNotThrow(() => l = new GdiImageLayer(imageFile));

            Assert.NotNull(l);
            Assert.AreEqual(Path.GetFileName(imageFile), l.LayerName);
            var e = new Envelope(10, 522, 10, 266);
            Assert.AreEqual(e, l.Envelope);
            Assert.AreEqual(0d, l.Transparency);
            l.Dispose();

            DeleteTmpFiles(imageFile);
        }
    }

    public class GdiImageLayerProxyTest
    {
        [Test]
        public void TestTransparency()
        {
            var tmpFile = GdiImageLayerTest.CreateImage(new Size(300, 300), new PointF(10, 10));
            using(var l = new GdiImageLayer(tmpFile))
            using (var pl = new GdiImageLayerProxy<GdiImageLayer>(l, 0.3f))
            using (var m = new Map(new Size(450, 450)))
            {
                m.Layers.Add(pl);
                m.ZoomToExtents();
                using (var img = (Bitmap)m.GetMap())
                {
                    var color = img.GetPixel(225, 225);
                    Assert.LessOrEqual(Math.Abs((int)Math.Round(0.3f*255, MidpointRounding.AwayFromZero) - color.A),1);
                }
            }
            GdiImageLayerTest.DeleteTmpFiles(tmpFile);
        }

        [Test, Ignore("not yet implemented")]
        public void TestColorMatrix()
        {
            var tmpFile = GdiImageLayerTest.CreateImage(new Size(300, 300), new PointF(10, 10));
            using (var l = new GdiImageLayer(tmpFile))
            using (var pl = new GdiImageLayerProxy<GdiImageLayer>(l, 0.3f))
            using (var m = new Map(new Size(450, 450)))
            {
                m.Layers.Add(pl);
                m.ZoomToExtents();
                using (var img = (Bitmap)m.GetMap())
                {
                    var color = img.GetPixel(225, 225);
                    Assert.AreEqual((byte)Math.Round(0.3f * 255, MidpointRounding.AwayFromZero), color.A);
                }
            }
            GdiImageLayerTest.DeleteTmpFiles(tmpFile);
        }

        [Test]
        public void TestColorMap()
        {
            var tmpFile = GdiImageLayerTest.CreateImage(new Size(300, 300), new PointF(10, 10));
            using (var l = new GdiImageLayer(tmpFile))
            using (var pl = new GdiImageLayerProxy<GdiImageLayer>(l, new ColorMap{OldColor = Color.Red, NewColor = Color.MistyRose}))
            using (var m = new Map(new Size(450, 450)))
            {
                m.Layers.Add(pl);
                m.ZoomToExtents();
                using (var img = (Bitmap)m.GetMap())
                {
                    var color = img.GetPixel(225, 225);
                    Assert.AreEqual(Color.MistyRose.ToArgb(), color.ToArgb());
                }
            }
            GdiImageLayerTest.DeleteTmpFiles(tmpFile);
        }
    }
}
