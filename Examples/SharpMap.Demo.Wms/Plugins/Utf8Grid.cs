// code adapted from: https://github.com/awcoats/mapstache
namespace Mapstache
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    using GeoAPI.Geometries;

    public class Utf8GridResults
    {
        public Utf8GridResults()
        {
            this.Keys = new List<string>();
            this.Data = new Dictionary<string, object>();
            this.Grid = new List<string>();
        }

        public IList<string> Keys { get; set; }

        public IDictionary<string, object> Data { get; set; }

        public IList<string> Grid { get; set; }
    }

    public class Utf8Grid : IDisposable
    {        
        private Bitmap bitmap;
        private Graphics graphics;

        private readonly Utf8GridResults results;        
        private readonly GraphicsPathBuilder graphicsPathBuilder;

        public Utf8Grid(int utfGridResolution, int tileX, int tileY, int zoom)
        {
            Size size = new Size(256 / utfGridResolution, 256 / utfGridResolution);
            this.bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppRgb);
            this.graphics = Graphics.FromImage(this.bitmap);
            RectangleF bbox = this.GetBoundingBoxInLatLngWithMargin(tileX, tileY, zoom);
            this.graphicsPathBuilder = new GraphicsPathBuilder(SphericalMercator.FromLonLat(bbox), size);            
            this.results = new Utf8GridResults();
        }

        public RectangleF GetBoundingBoxInLatLngWithMargin(int tileX, int tileY, int zoom)
        {
            PointF lonlat1 = TileSystemHelper.PixelXYToLatLong(new Point((tileX * 256), (tileY * 256)), zoom);
            PointF lonlat2 = TileSystemHelper.PixelXYToLatLong(new Point(((tileX + 1) * 256), ((tileY + 1) * 256)), zoom);
            return RectangleF.FromLTRB(lonlat1.X, lonlat2.Y, lonlat2.X, lonlat1.Y);
        }

        public void FillPolygon(IGeometry geometry, int i, object data = null)
        {
            using (GraphicsPath gp = this.graphicsPathBuilder.Build(geometry))
            using (Brush brush = CreateBrush(i))
                this.graphics.FillPath(brush, gp);
            if (data != null)
                this.results.Data.Add(i.ToString(), data);
        }

        public Utf8GridResults CreateUtfGridJson()
        {
            BitmapData bitmapData = this.bitmap.LockBits(
                new Rectangle(0, 0, this.bitmap.Width, this.bitmap.Height), 
                ImageLockMode.ReadOnly, this.bitmap.PixelFormat);
            IntPtr ptr = bitmapData.Scan0;
            int bytes = Math.Abs(bitmapData.Stride) * this.bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            List<int> uniqueValues = new List<int>();
            int[,] grid = new int[this.bitmap.Width, this.bitmap.Height];
            for (int row = 0; row < this.bitmap.Height; row++)
            {
                int start = row * bitmapData.Stride;
                for (int x = 0; x < this.bitmap.Width; x++)
                {
                    int value = RgbToInt(start, rgbValues, x);
                    if (uniqueValues.Contains(value) == false)
                        uniqueValues.Add(value);
                    grid[x, row] = value;
                }
            }
            uniqueValues.Sort();
            this.bitmap.UnlockBits(bitmapData);
            for (int y = 0; y < this.bitmap.Height; y++)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < this.bitmap.Width; x++)
                {
                    int key = (grid[x, y]);
                    int id = uniqueValues.IndexOf(key);
                    id = id + 32;
                    if (id >= 34)
                        id = id + 1;
                    if (id >= 92)
                        id = id + 1;
                    sb.Append(char.ConvertFromUtf32(id));
                }
                this.results.Grid.Add(sb.ToString());
            }
            if (uniqueValues.Contains(0))
            {
                // remove 0 since that is taken care of by ""
                uniqueValues.Remove(0);
                this.results.Keys.Add(String.Empty);
            }
            uniqueValues.ForEach(value =>
            {
                string key = value.ToString(CultureInfo.InvariantCulture);
                this.results.Keys.Add(key);
            });
            return this.results;
        }

        public static int RgbToInt(int start, byte[] rgbValues, int x)
        {
            byte[] v = new[] { rgbValues[start], rgbValues[start + 1], rgbValues[start + 2], rgbValues[start + 3] };
            byte r = rgbValues[start + (x * 4) + 2];
            byte g = rgbValues[start + (x * 4) + 1];
            byte b = rgbValues[start + (x * 4)];
            int value = r + (g * 256) + (b * 65536);
            return value;
        }

        public static Color IntToRgb(int p)
        {
            int r = p & 255;
            int g = (p >> 8) & 255;
            int b = (p >> 16) & 255;
            Color color = Color.FromArgb(r, g, b);
            return color;
        }

        public static Brush CreateBrush(int p)
        {
            Color color = IntToRgb(p);
            SolidBrush brush = new SolidBrush(color);
            return brush;
        }

        public static Pen CreatePen(int p)
        {
            Color color = IntToRgb(p);
            Pen pen = new Pen(color);
            return pen;
        }

        public Graphics CreateGraphics()
        {
            return Graphics.FromImage(this.bitmap);
        }

        public void Dispose()
        {
            if (this.bitmap != null)
            {
                this.bitmap.Dispose();
                this.bitmap = null;
            }
            if (this.graphics != null)
            {
                this.graphics.Dispose();
                this.graphics = null;
            }
        }
    }
}
