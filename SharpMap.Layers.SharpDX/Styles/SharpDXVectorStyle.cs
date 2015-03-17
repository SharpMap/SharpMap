using System;
using System.Drawing;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using SharpMap.Styles;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Brush = SharpDX.Direct2D1.Brush;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;
using GdiSolidBrush = System.Drawing.SolidBrush;
using GdiTextureBrush = System.Drawing.TextureBrush;

namespace SharpMap.Layers.Styles
{
    internal class SharpDXVectorStyle : Style
    {
        private SharpDXVectorStyle() { }

        public static SharpDXVectorStyle FromVectorStyle(RenderTarget rt, Factory f, VectorStyle vs)
        {
            var res = new SharpDXVectorStyle
            {
                // Global
                Enabled = vs.Enabled,
                MinVisible = vs.MinVisible,
                MaxVisible = vs.MaxVisible,
            };

            // Point
            if (vs.PointColor != null)
            {
                res.PointColor = Converter.ToSharpDXBrush(rt, vs.PointColor);
                res.PointSize = vs.PointSize;
            }
            if (vs.Symbol != null)
            {
                res.Symbol = Converter.ToSharpDXBitmap(rt, vs.Symbol as System.Drawing.Bitmap, vs.SymbolScale);
                res.SymbolOffset = Converter.ToSharpDXPoint(vs.SymbolOffset);
                //res.SymbolScale = vs.SymbolScale;
                res.SymbolRotation = vs.SymbolRotation;
            }
            
            // Line
            if (vs.Line != null)
            {
                res.Line = Converter.ToSharpDXBrush(rt, vs.Line.Brush);
                res.LineWidth = vs.Line.Width;
                res.LineStrokeStyle = Converter.ToSharpDXStrokeStyle(f, vs.Line);
                res.LineOffset = vs.LineOffset;
            }
            if (vs.Outline != null)
            {
                res.EnableOutline = vs.EnableOutline;
                res.Outline = Converter.ToSharpDXBrush(rt, vs.Outline.Brush);
                res.OutlineWidth = vs.Outline.Width;
                res.OutlineStrokeStyle = Converter.ToSharpDXStrokeStyle(f, vs.Line);
            }
            
            // Fill
            if (vs.Fill != null)
            {
                res.Fill = Converter.ToSharpDXBrush(rt, vs.Fill);
            }

            return res;
        }

        public Brush PointColor { get; private set; }
        public float PointSize { get; private set; }
        public Bitmap Symbol { get; private set; }
        public Vector2 SymbolOffset { get; private set; }
        public float SymbolScale { get; private set; }
        public float SymbolRotation { get; private set; }
        public Brush Line { get; private set; }
        public float LineOffset { get; private set; }
        public float LineWidth { get; private set; }
        public StrokeStyle LineStrokeStyle { get; private set; }
        public bool EnableOutline { get; private set; }
        public Brush Outline { get; private set; }
        public float OutlineWidth { get; private set; }
        public StrokeStyle OutlineStrokeStyle { get; private set; }
        public Brush Fill { get; private set; }

    }

    internal static class Converter
    {
        internal static Brush ToSharpDXBrush(RenderTarget rt, System.Drawing.Brush brush)
        {
            if (brush is System.Drawing.SolidBrush)
            {
                var res = new SolidColorBrush(rt,
                    ToSharpDXColor(((System.Drawing.SolidBrush) brush).Color));
                //res.Opacity = ToPortion(((SolidBrush) brush).Color.A);
                return res;
            }

            if (brush is System.Drawing.TextureBrush)
            {
                var tb = (TextureBrush) brush;
                var sharpDXBitmap = Converter.ToSharpDXBitmap(rt, tb.Image as System.Drawing.Bitmap, 1f);
                return new SharpDX.Direct2D1.BitmapBrush(rt, sharpDXBitmap);
            }
            throw new NotSupportedException();
        }

        internal static Bitmap ToSharpDXBitmap(RenderTarget rt, System.Drawing.Bitmap image, float symbolScale)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (image.PixelFormat != GdiPixelFormat.Format32bppPArgb)
                return null;

            var imageData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);


            var dataStream = new DataStream(imageData.Scan0, imageData.Stride*imageData.Height, true, false);
            var properties = new BitmapProperties
            {
                PixelFormat = new SharpDX.Direct2D1.PixelFormat
                {
                    Format =   SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    AlphaMode = AlphaMode.Premultiplied
                }
            };

            // ToDo apply scaling here!
            //var scaler = new BitmapScaler(rt.Factory.NativePointer);
            //scaler.

            //Load the image from the gdi resource
            var result = new Bitmap(rt, new Size2(image.Width, image.Height), dataStream, imageData.Stride, properties);

            image.UnlockBits(imageData);

            return result;
        }

        private static float ToPortion(int value, int max = 255)
        {
            return (float) value/max;
        }

        internal static Color4 ToSharpDXColor4(System.Drawing.Color color)
        {
            return new Color4(ToPortion(color.R), ToPortion(color.G), ToPortion(color.B), ToPortion(color.A));
        }

        internal static SharpDX.Color ToSharpDXColor(System.Drawing.Color color)
        {
            return new SharpDX.Color(color.R, color.G, color.B, (byte)255);
        }
        public static Vector2 ToSharpDXPoint(PointF symbolOffset)
        {
            var tmp = System.Drawing.Point.Round(symbolOffset);
            return new Vector2(tmp.X, tmp.Y);
        }

        public static StrokeStyle ToSharpDXStrokeStyle(Factory factory, System.Drawing.Pen pen)
        {
            var sp = new StrokeStyleProperties
            {
                DashCap = (CapStyle) pen.DashCap,
                DashOffset = pen.DashOffset,
                DashStyle = (DashStyle) pen.DashStyle,
                StartCap = (CapStyle)pen.StartCap,
                EndCap = (CapStyle)pen.EndCap,
                LineJoin = (LineJoin)pen.LineJoin,
                MiterLimit = pen.MiterLimit,
            };
            if (pen.DashStyle == System.Drawing.Drawing2D.DashStyle.Custom)
                return new StrokeStyle(factory, sp, pen.DashPattern);

            return new StrokeStyle(factory, sp);
        }
    }
}