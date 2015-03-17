using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using WICBitmap = SharpDX.WIC.Bitmap;
using D2D1PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace SharpMap.Rendering
{
    internal class WICRenderTargetFactory : IRenderTargetFactory, IDisposable
    {
        private readonly ImagingFactory _wicFactory;

        public WICRenderTargetFactory()
        {
            _wicFactory = new ImagingFactory();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                _wicFactory.Dispose();
        }

        public RenderTarget Create(Factory factory, Graphics g, Map map)
        {
            var wicBitmap = new WICBitmap(_wicFactory, map.Size.Width, map.Size.Height,
                SharpDX.WIC.PixelFormat.Format32bppPBGRA,
                BitmapCreateCacheOption.CacheOnDemand);

            var rtp = new RenderTargetProperties(RenderTargetType.Default,
                new D2D1PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 0, 0,
                RenderTargetUsage.None,
                FeatureLevel.Level_DEFAULT);

            var res = new WicRenderTarget(factory, wicBitmap, rtp) {Tag = wicBitmap};
            res.BeginDraw();
            res.Clear(SharpDX.Color.Transparent);

            return res;
        }

        public void CleanUp(RenderTarget target, Graphics g, Map map)
        {
            target.EndDraw();
            
            var wicBitmap = (WICBitmap) target.Tag;
            using (var image = ConvertToBitmap(wicBitmap))
                g.DrawImageUnscaled(image, 0, 0);

            wicBitmap.Dispose();
            target.Dispose();
        }

        private static System.Drawing.Bitmap ConvertToBitmap(WICBitmap wicBitmap)
        {
            var width = wicBitmap.Size.Width;
            var height = wicBitmap.Size.Height;
            var gdiBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            var gdiBitmapData = gdiBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, gdiBitmap.Width, gdiBitmap.Height),
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            var buffer = new int[width * height];
            wicBitmap.CopyPixels(buffer);
            Marshal.Copy(buffer, 0, gdiBitmapData.Scan0, buffer.Length);

            gdiBitmap.UnlockBits(gdiBitmapData);
            return gdiBitmap;
        }
    }
}