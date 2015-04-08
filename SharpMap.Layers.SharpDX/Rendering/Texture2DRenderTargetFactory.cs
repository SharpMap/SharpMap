using System;
using System.Runtime.InteropServices;
using D2D1 = SharpDX.Direct2D1;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using GDI = System.Drawing;

namespace SharpMap.Rendering
{
    public class Texture2DRenderTargetFactory : IRenderTargetFactory, IDisposable
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        private readonly D3D11.Device _d3d11Device;
        private readonly D3D11.DeviceContext _d3d11Context;

        //private readonly object _syncRoot;

        private D3D11.Texture2D _renderTexture;
        private D3D11.Texture2D _copyHelperTextureStaging;

        public Texture2DRenderTargetFactory()
        {
            //_syncRoot = new object();
            _d3d11Device = new D3D11.Device(D3D.DriverType.Hardware,
                D3D11.DeviceCreationFlags.BgraSupport | D3D11.DeviceCreationFlags.SingleThreaded 
#if DEBUG
                | D3D11.DeviceCreationFlags.Debug
#endif
                );
            _d3d11Context = _d3d11Device.ImmediateContext;
        }

        public D2D1.RenderTarget Create(D2D1.Factory factory, GDI.Graphics g, Map map)
        {
            //Monitor.Enter(_syncRoot);

            // Dispose the _renderTexture if it is instantiated and not of the required size
            CheckTexture(ref _renderTexture, map.Size);

            // Create a new render texture if one is needed
            if (_renderTexture == null)
            {
                _renderTexture = CreateRenderTargetTexture(_d3d11Device, map.Size.Width, map.Size.Height);
            }

            // Get the surface
            var surface = _renderTexture.QueryInterface<DXGI.Surface>();
            
            var res = new D2D1.RenderTarget(factory, surface, new D2D1.RenderTargetProperties(
                D2D1.RenderTargetType.Hardware, new D2D1.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, D2D1.AlphaMode.Premultiplied),
                g.DpiX, g.DpiY, D2D1.RenderTargetUsage.None, D2D1.FeatureLevel.Level_DEFAULT));

            res.BeginDraw();

            return res;
        }

        public void CleanUp(D2D1.RenderTarget target, GDI.Graphics g, Map map)
        {
            target.EndDraw();
            using (var sc = TakeScreenshotGdi(map.Size))
                g.DrawImage(sc, new GDI.Point(0, 0));
            
            target.Dispose();

            //Monitor.Exit(_syncRoot);
        }

        public void Dispose()
        {
            _d3d11Device.Dispose();
        }

        /// <summary>
        /// Creates a render target texture with the given width and height.
        /// </summary>
        /// <param name="device">Graphics device.</param>
        /// <param name="width">Width of generated texture.</param>
        /// <param name="height">Height of generated texture.</param>
        public D3D11.Texture2D CreateRenderTargetTexture(D3D11.Device device, int width, int height)
        {
            var textureDescription = new D3D11.Texture2DDescription();

            if (TryEnableAntialiasing)
            {
                var maxCount = 1;
                for (var i = 0; i < 32; i++)
                {
                    if (device.CheckMultisampleQualityLevels(DXGI.Format.B8G8R8A8_UNorm, i) > 0)
                        maxCount = i;
                }

                textureDescription.Width = width;
                textureDescription.Height = height;
                textureDescription.MipLevels = 1;
                textureDescription.ArraySize = 1;
                textureDescription.Format = DXGI.Format.B8G8R8A8_UNorm;
                textureDescription.Usage = D3D11.ResourceUsage.Default;
                textureDescription.SampleDescription = new DXGI.SampleDescription(maxCount, 0);
                textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
            }
            else
            {
                textureDescription.Width = width;
                textureDescription.Height = height;
                textureDescription.MipLevels = 1;
                textureDescription.ArraySize = 1;
                textureDescription.Format = DXGI.Format.B8G8R8A8_UNorm;
                textureDescription.Usage = D3D11.ResourceUsage.Default;
                textureDescription.SampleDescription = new DXGI.SampleDescription(1, 0);
                textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
            }

            return new D3D11.Texture2D(device, textureDescription);
        }

        /// <summary>
        /// Gets or sets a value indicating if AntiAlias should be enabled or not
        /// </summary>
        public bool TryEnableAntialiasing { get; set; }

        /// <summary>
        /// Creates a staging texture which enables copying data from gpu to cpu memory.
        /// </summary>
        /// <param name="device">Graphics device.</param>
        /// <param name="size">The size of generated texture.</param>
        public D3D11.Texture2D CreateStagingTexture(D3D11.Device device, GDI.Size size)
        {
            CheckTexture(ref _copyHelperTextureStaging, size);
            
            if (_copyHelperTextureStaging != null)
                return _copyHelperTextureStaging;
            
            //For handling of staging resource see
            // http://msdn.microsoft.com/en-us/library/windows/desktop/ff476259(v=vs.85).aspx

            D3D11.Texture2DDescription textureDescription = new D3D11.Texture2DDescription();
            textureDescription.Width = size.Width;
            textureDescription.Height = size.Height;
            textureDescription.MipLevels = 1;
            textureDescription.ArraySize = 1;
            textureDescription.Format = DXGI.Format.B8G8R8A8_UNorm;
            textureDescription.Usage = D3D11.ResourceUsage.Staging;
            textureDescription.SampleDescription = new DXGI.SampleDescription(1, 0);
            textureDescription.BindFlags = D3D11.BindFlags.None;
            textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.Read;
            textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;

            return new D3D11.Texture2D(device, textureDescription);
        }

        /// <summary>
        /// Takes a screenshot and returns it as a gdi bitmap.
        /// </summary>
        public GDI.Bitmap TakeScreenshotGdi(GDI.Size size)
        {
            //Get and read data from the gpu (create copy helper texture on demand)
            if (_copyHelperTextureStaging == null)
            {
                _copyHelperTextureStaging = CreateStagingTexture(_d3d11Device, size);
            }
            _d3d11Context.CopyResource(_renderTexture, _copyHelperTextureStaging);

            //Prepare target bitmap
            var result = new GDI.Bitmap(size.Width, size.Height);

            var dataBox = _d3d11Context.MapSubresource(_copyHelperTextureStaging, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
            try
            {
                //Lock bitmap so it can be accessed for texture loading
                System.Drawing.Imaging.BitmapData bitmapData = result.LockBits(
                    new System.Drawing.Rectangle(0, 0, result.Width, result.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                try
                {
                    //Copy bitmap data
                    memcpy(bitmapData.Scan0, dataBox.DataPointer, new UIntPtr((uint)(size.Width * size.Height * 4)));
                }
                finally
                {
                    result.UnlockBits(bitmapData);
                }
            }
            finally
            {
                _d3d11Context.UnmapSubresource(_copyHelperTextureStaging, 0);
            }

            return result;
        }

        public void CheckTexture(ref D3D11.Texture2D tx, GDI.Size size)
        {
            if (tx == null)
                return;

            if (tx.Description.Width != size.Width ||
                tx.Description.Height != size.Height)
            {
                tx.Dispose();
                tx = null;
            }
        }
    }
}