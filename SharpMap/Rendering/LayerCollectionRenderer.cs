using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common.Logging;
using SharpMap.Layers;
using SharpMap.Styles;

namespace SharpMap.Rendering
{
    /// <summary>
    /// A layer collection renderer
    /// </summary>
    public class LayerCollectionRenderer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger<LayerCollectionRenderer>();

        private readonly ILayer[] _layers;
        private MapViewport _mapViewPort;
        private double _mapScale;

        private Image[] _images;
        private static Func<Size, float, int, bool> _parallelHeuristic;
        //private Matrix _transform;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layers">The layer collection to render</param>
        public LayerCollectionRenderer(ICollection<ILayer> layers)
        {
            _layers = new ILayer[layers.Count];
            layers.CopyTo(_layers, 0);
        }

        /// <summary>
        /// Method to render the layer collection
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        /// <param name="allowParallel"></param>
        [Obsolete("Use Render(Graphics, MapViewport, allowParallel)")]
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Render(Graphics g, Map map, bool allowParallel)
        {
            Render(g, (MapViewport)map, allowParallel);
        }

        /// <summary>
        /// Method to render the layer collection
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="mapViewPort">Rendering parameters snapshot of current map</param>
        /// <param name="allowParallel"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Render(Graphics g, MapViewport mapViewPort, bool allowParallel)
        {
            _mapViewPort = mapViewPort;
            _mapScale = _mapViewPort.GetMapScale((int)g.DpiX);
            //_transform = _map.MapTransform;
            g.PageUnit = GraphicsUnit.Pixel;
            if (AllowParallel && allowParallel && ParallelHeuristic(mapViewPort.Size, g.DpiX, _layers.Length))
            {
                RenderParellel(g);
            }
            else
            {
                RenderSequenial(g);
            }
        }

        /// <summary>
        /// Method to determine the synchronization method
        /// </summary>
        public static Func<Size, float, int, bool> ParallelHeuristic
        {
            get { return _parallelHeuristic ?? StdHeuristic; }
            set { _parallelHeuristic = value; }
        }

        /// <summary>
        /// Method to enable parallel rendering of layers
        /// </summary>
        public static bool AllowParallel { get; set; }

        /// <summary>
        /// Standard implementation for <see cref="ParallelHeuristic"/>
        /// </summary>
        /// <param name="size">The size of the map</param>
        /// <param name="dpi">The dots per inch</param>
        /// <param name="numLayers">The number of layers</param>
        /// <returns><c>true</c> if the map's width and height are less or equal 1920 and the collection has less than 10 entries</returns>
        private static bool StdHeuristic(Size size, float dpi, int numLayers)
        {
            return (size.Width < 1920 && size.Height <= 1920 && numLayers < 100);
        }

        private void RenderSequenial(Graphics g)
        {
            for (var layerIndex = 0; layerIndex < _layers.Length; layerIndex++)
            {
                var layer = _layers[layerIndex];
                if (layer.Enabled)
                {
                    double compare = layer.VisibilityUnits == VisibilityUnits.ZoomLevel ? _mapViewPort.Zoom : _mapScale;
                    if (layer.MaxVisible >= compare && layer.MinVisible < compare)
                    {
                        RenderLayer(layer, g, _mapViewPort);
                    }
                }
            }
        }

        private void RenderParellel(Graphics g)
        {
            _images = new Image[_layers.Length];

            var res = Parallel.For(0, _layers.Length, RenderToImage);
            
            var tmpTransform = g.Transform;
            g.Transform = new Matrix();
            if (res.IsCompleted)
            {
                for (var i = 0; i < _images.Length; i++)
                {
                    if (_images[i] != null)
                    {
                        g.DrawImageUnscaled(_images[i], 0, 0);
                        //break;
                    }
                }
            }
            g.Transform = tmpTransform;
        }

        private void RenderToImage(int layerIndex, ParallelLoopState pls)
        {
            if (pls.ShouldExitCurrentIteration)
                return;

            var layer = _layers[layerIndex];
            
            if (layer.Enabled)
            {
                double compare = layer.VisibilityUnits == VisibilityUnits.ZoomLevel ? _mapViewPort.Zoom : _mapScale;
                if (layer.MaxVisible >= compare && layer.MinVisible < compare)
                {
                    var image = _images[layerIndex] = new Bitmap(_mapViewPort.Size.Width, _mapViewPort.Size.Height, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(image))
                    {
                        g.PageUnit = GraphicsUnit.Pixel;
                        ApplyTransform(_mapViewPort.MapTransform , g);

                        g.Clear(Color.Transparent);
                        RenderLayer(layer, g, _mapViewPort );
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the rendering of the layer, a red X is drawn if it fails.
        /// </summary>
        /// <param name="layer">The layer to render</param>
        /// <param name="g">The graphics object to use</param>
        /// <param name="mapViewport">The viewport</param>
        public static void RenderLayer(ILayer layer, Graphics g, MapViewport mapViewport)
        {
            try
            {
                layer.Render(g, mapViewport);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);

                using (var pen = new Pen(Color.Red, 4f))
                {
                    var size = mapViewport.Size;

                    g.DrawLine(pen, 0, 0, size.Width, size.Height);
                    g.DrawLine(pen, size.Width,0, 0, size.Height);
                    g.DrawRectangle(pen, 0, 0, size.Width, size.Height);
                }
                
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void ApplyTransform(Matrix transform, Graphics g)
        {
            g.Transform = transform.Clone();
        }

        public void Dispose()
        {
            if (_images != null)
            {
                foreach (var image in _images)
                {
                    if (image != null)image.Dispose();
                }
            }
        }
    }
}
