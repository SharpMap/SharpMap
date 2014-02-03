using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Layers;

namespace UnitTests.Layers
{
    public class GdiImageLayerTest
    {
        private static string CreateImage(Size size, Point? origin = null)
        {
            var img = new Bitmap(size.Width, size.Height);
            var tmpFile = Path.ChangeExtension(Path.GetTempFileName(), ".png");
            img.Save(tmpFile, ImageFormat.Png);

            CreateWorldFile(tmpFile, size, origin);

            return tmpFile;
        }

        private static void CreateWorldFile(string imageFile, Size size, Point? origin)
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

        private void DeleteTmpFiles(string imageFile)
        {
            if (File.Exists(imageFile))
            {
                foreach (var file in Directory.GetFiles(Path.GetTempPath(), 
                    Path.GetFileNameWithoutExtension(imageFile) + ".*"))
                {
                    File.Delete(file);
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
            DeleteTmpFiles(imageFile);
        }

        
    }
}