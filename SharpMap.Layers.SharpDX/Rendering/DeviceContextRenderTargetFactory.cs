using System;
using System.Drawing;
using SharpDX.Direct2D1;

namespace SharpMap.Rendering
{
    public class DeviceContextRenderTargetFactory : IRenderTargetFactory
    {
        public RenderTarget Create(Factory factory, Graphics g, Map map)
        {
            var hdc = g.GetHdc();
            var rtp = new RenderTargetProperties(
                RenderTargetType.Default,
                new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                0, 0, RenderTargetUsage.None,
                FeatureLevel.Level_DEFAULT);

            var rt = new DeviceContextRenderTarget(factory, rtp);
            rt.Tag = hdc;
            rt.BindDeviceContext(hdc, new SharpDX.Rectangle(0, 0, map.Size.Width, map.Size.Height));
            rt.BeginDraw();

            return rt;
        }

        public void CleanUp(RenderTarget target, Graphics g, Map map)
        {
            target.EndDraw();

            var hdc = (IntPtr)target.Tag;
            g.ReleaseHdc(hdc);
        }
    }
}