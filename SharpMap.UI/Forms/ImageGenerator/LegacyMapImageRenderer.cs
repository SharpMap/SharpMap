using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Logging;
using GeoAPI.Geometries;
using SharpMap.Layers;

namespace SharpMap.Forms.ImageGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class LegacyMapBoxImageGenerator : IMapBoxImageGenerator
    {
        private static readonly ILog _logger = LogManager.GetLogger<LegacyMapBoxImageGenerator>();

        private readonly MapBox _mapBox;
        private readonly ProgressBar _progressBar;

        private Map _map;

        private volatile bool _isDisposed;
        //private bool _isRefreshing;
        private readonly object _staticImagesLocker = new object();
        private readonly object _backgroundImagesLocker = new object();
        private readonly object _paintImageLocker = new object();

        private Image _image = new Bitmap(1, 1);
        private uint _idImageGeneration;
        private Bitmap _imageBackground = new Bitmap(1, 1);
        private Bitmap _imageStatic = new Bitmap(1, 1);
        private Bitmap _imageVariable = new Bitmap(1, 1);
        private Envelope _imageEnvelope = new Envelope(0, 1, 0, 1);

        private uint _imageGeneration;

        /// <summary>
        /// Creates an instance for this class
        /// </summary>
        /// <param name="mapBox">The map box</param>
        /// <param name="progressBar">A progress bar</param>
        public LegacyMapBoxImageGenerator(MapBox mapBox, ProgressBar progressBar)
        {
            _progressBar = progressBar;
            _mapBox = mapBox;
            WireMapBox();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                UnwireMap();

                _imageBackground?.Dispose();
                _imageBackground = null;
                _imageStatic?.Dispose();
                _imageStatic = null;
                _imageVariable?.Dispose();
                _imageVariable = null;
                _image?.Dispose();
                _image = null;

                _imageEnvelope = new Envelope();
            }
            _isDisposed = true;
        }

        /// <summary>
        /// Gets access to the map
        /// </summary>
        private MapBox MapBox
        {
            get { return _mapBox; }
        }




        public Image Image
        {
            get
            {
                GetImagesAsyncEnd(null);
                return ImageValue;
            }
        }

        public Image ImageValue
        {
            get
            {
                Image res;
                lock (_paintImageLocker)
                    res = _image;
                return res;
            }
        }

        public bool IsDisposed { get => _isDisposed; }

        public Envelope ImageEnvelope { get => new Envelope(_imageEnvelope); }

        #region Necessary event handlers

        private void HandleMapChanged(object sender, EventArgs e)
        {
            UnwireMap();
            _map = _mapBox.Map;
            WireMap();
        }
        void HandleMapRefreshNeeded(object sender, EventArgs e)
        {
            UpdateImage(true, new Rectangle(Point.Empty, MapBox.Size));
        }


        private void HandleMapNewTileAvaliable(ITileAsyncLayer sender, Envelope box, Bitmap bm, int sourceWidth,
            int sourceHeight, ImageAttributes imageAttributes)
        {
            lock (_backgroundImagesLocker)
            {
                try
                {
                    var min = Point.Round(_map.WorldToImage(box.Min()));
                    var max = Point.Round(_map.WorldToImage(box.Max()));

                    if (MapBox.IsDisposed == false && _isDisposed == false)
                    {
                        var rect = new Rectangle(min.X, max.Y, (max.X - min.X), (min.Y - max.Y));
                        using (var g = Graphics.FromImage(_imageBackground))
                        {

                            g.DrawImage(bm, rect, 0, 0,
                                sourceWidth, sourceHeight,
                                GraphicsUnit.Pixel,
                                imageAttributes);

                        }

                        UpdateImage(false, rect);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message, ex);
                    //this can be a GDI+ Hell Exception...
                }

            }

        }

        /// <summary>
        /// Handles need to requery of variable layers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleVariableLayersRequery(object sender, EventArgs e)
        {
            if (_mapBox.IsDisposed || _isDisposed)
                return;

            Image oldRef;
            lock (_mapBox.MapLocker)
            {
                if (_mapBox.Dragging) return;
                oldRef = _imageVariable;
                _imageVariable = GetMap(_map, _map.VariableLayers, LayerCollectionType.Variable, _map.Envelope);
            }

            var rect = new Rectangle(Point.Empty, _mapBox.Size);
            UpdateImage(false, rect);
            if (oldRef != null)
                oldRef.Dispose();

            _mapBox.Invalidate(rect);
            _mapBox.Invoke(new MethodInvoker(_mapBox.Update));

            //// TODO Why?
            //Application.DoEvents();
        }


        #endregion

        #region Event (un)subscribe functions

        private void WireMapBox()
        {
            _mapBox.MapChanged += HandleMapChanged;
            _map = _mapBox.Map;
            WireMap();
        }

        private void WireMap()
        {
            if (_map == null)
                return;

            _map.MapNewTileAvaliable += HandleMapNewTileAvaliable;
            _map.VariableLayers.VariableLayerCollectionRequery += HandleVariableLayersRequery;
            _map.RefreshNeeded += HandleMapRefreshNeeded;
        }

        private void UnwireMap()
        {
            if (_map == null) return;
            _map.MapNewTileAvaliable -= HandleMapNewTileAvaliable;
            _map.VariableLayers.VariableLayerCollectionRequery -= HandleVariableLayersRequery;
            _map.RefreshNeeded -= HandleMapRefreshNeeded;

            _map = null;
        }

        #endregion


        private Bitmap GetMap(Map map, LayerCollection layers, LayerCollectionType layerCollectionType,
            Envelope extent)
        {
            try
            {
                var width = _mapBox.Width;
                var height = _mapBox.Height;

                if ((layers == null || layers.Count == 0 || width <= 0 || height <= 0))
                {
                    if (layerCollectionType == LayerCollectionType.Background)
                        return new Bitmap(1, 1);
                    return null;
                }

                var retval = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                using (var g = Graphics.FromImage(retval))
                {
                    g.Clear(Color.Transparent);
                    map.RenderMap(g, layerCollectionType, false, true);
                }

                /*if (layerCollectionType == LayerCollectionType.Variable)
                    retval.MakeTransparent(_map.BackColor);
                else if (layerCollectionType == LayerCollectionType.Static && map.BackgroundLayer.Count > 0)
                    retval.MakeTransparent(_map.BackColor);*/
                return retval;
            }
            catch (Exception ee)
            {
                _logger.Error("Error while rendering map", ee);

                if (layerCollectionType == LayerCollectionType.Background)
                    return new Bitmap(1, 1);
                return null;
            }
        }

        private void GetImagesAsync(Envelope extent, uint imageGeneration)
        {
            lock (_mapBox.MapLocker)
            {
                if (_isDisposed)
                    return;

                if (imageGeneration < _imageGeneration)
                {
                    /*we're to old*/
                    return;
                }
                var safeMap = _map.Clone();
                _imageVariable = GetMap(safeMap, _map.VariableLayers, LayerCollectionType.Variable, extent);

                lock (_staticImagesLocker)
                {
                    _imageStatic = GetMap(safeMap, _map.Layers, LayerCollectionType.Static, extent);
                }
                lock (_backgroundImagesLocker)
                {
                    _imageBackground = GetMap(safeMap, _map.BackgroundLayer, LayerCollectionType.Background, extent);
                }
            }
        }

        private class GetImageEndResult
        {
            public MapBox.Tools? Tool { get; set; }
            public Envelope bbox { get; set; }
            public uint Generation { get; set; }
            public Rectangle UpdateArea { get; set; }

#if DEBUG
            public Stopwatch Watch { get; set; }
#endif
        }

        private void GetImagesAsyncEnd(GetImageEndResult res)
        {
            // draw only if generation is larger than the current, else we have aldready drawn something newer
            // we must to check also IsHandleCreated because during disposal, the handle of the parent is detroyed sooner than progress bar's handle,
            // this leads to cross thread operation and exception because InvokeRequired returns false, but for the progress bar it is true.
            if (res == null || res.Generation < _imageGeneration || _isDisposed || !_mapBox.IsHandleCreated)
                return;


            if (_logger.IsDebugEnabled)
                _logger.DebugFormat("{0} - {1} / {2}", res.Generation, res.bbox, res.UpdateArea);


            if ((_mapBox.SetToolsNoneWhileRedrawing|| _mapBox.ShowProgressUpdate) && _mapBox.InvokeRequired)
            {
                try
                {
                    _mapBox.BeginInvoke(new MethodInvoker(() => GetImagesAsyncEnd(res)));

                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message, ex);
                }
            }
            else
            {
                try
                {
                    var oldRef = _image;
                    var width = _mapBox.Width;
                    var height = _mapBox.Height;
                    if (width > 0 && height > 0)
                    {


                        /*
                        Bitmap bmp = new Bitmap(width, height);
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.Clear(_map.BackColor);
                            lock (_backgroundImagesLocker)
                            {
                                //Draws the background Image
                                if (_imageBackground != null)
                                {
                                    try
                                    {
                                        g.DrawImageUnscaled(_imageBackground, 0, 0);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Warn(ex.Message, ex);
                                    }
                                }
                            }

                            //Draws the static images
                            if (_staticImagesLocker != null)
                            {
                                try
                                {
                                    if (_imageStatic != null)
                                    {
                                        g.DrawImageUnscaled(_imageStatic, 0, 0);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warn(ex.Message, ex);
                                }

                            }

                            //Draws the variable Images
                            if (_imageVariable != null)
                            {
                                try
                                {
                                    g.DrawImageUnscaled(_imageVariable, 0, 0);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warn(ex.Message, ex);
                                }
                            }

                            g.Dispose();
                        }
                        */


                        var bmp = MergeImages(res.Generation, res.UpdateArea);

                        lock (_paintImageLocker)
                        {
                            if (bmp != _image)
                            {
                                _image = bmp;
                                _imageEnvelope = res.bbox;
                                _idImageGeneration = res.Generation;
                            }
                        }
                    }

                    if (res.Tool.HasValue)
                    {
                        if (_mapBox.SetToolsNoneWhileRedrawing)
                            _mapBox.ActiveTool = res.Tool.Value;

                        _mapBox.ClearDrag();
                        //_isRefreshing = false;

                        if (_mapBox.SetToolsNoneWhileRedrawing)
                            _mapBox.Enabled = true;

                        if (_mapBox.ShowProgressUpdate)
                        {
                            _progressBar.Enabled = false;
                            _progressBar.Visible = false;
                        }
                    }

                    lock (_paintImageLocker)
                    {
                        if (oldRef != null && oldRef != _image)
                            oldRef.Dispose();
                    }

                    _mapBox.Invalidate(res.UpdateArea);
                    _mapBox.Invoke(new MethodInvoker(_mapBox.Update));
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message, ex);
                }

#if DEBUG
                if (res.Watch != null)
                { res.Watch.Stop();
                _mapBox.LastRefreshTime = res.Watch.Elapsed;
                }
#endif

                try
                {
                    _mapBox.OnMapRefreshed(EventArgs.Empty);
                    /*
                    if (_mapBox.MapRefreshed != null)
                    {
                        MapRefreshed(this, null);
                    }
*/
                }
                catch (Exception ee)
                {
                    //Trap errors that occured when calling the eventhandlers
                    _logger.Warn("Exception while calling eventhandler", ee);
                }
            }
        }

        private Bitmap MergeImages(uint generation, Rectangle rectangle)
        {
            var res = generation == _idImageGeneration
                ? (Bitmap) _image 
                : new Bitmap(MapBox.Size.Width, MapBox.Size.Height);

            var counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    using (var g = Graphics.FromImage(res))
                    {
                        lock (_backgroundImagesLocker)
                            TryDrawImage(g, _imageBackground, rectangle);
                        lock (_staticImagesLocker)
                            TryDrawImage(g, _imageStatic, rectangle);
                        TryDrawImage(g, _imageVariable, rectangle);
                    }

                    break;
                }
                catch (Exception e)
                {
                    _logger.Warn($"{counter}. merge at {rectangle} failed.");
                    if (counter == 3)
                    {
                        _logger.Warn("Quit trying to merge.");
                        break;
                    }
                }

                Thread.Sleep(25);
            }

            return res;
        }

        private static void TryDrawImage(Graphics g, Image image, Rectangle rect)
        {
            if (image == null)
                return;

            try
            {
                g.DrawImage(image, rect, rect, GraphicsUnit.Pixel);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex.Message, ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Generate()
        {
            UpdateImage(true, new Rectangle(Point.Empty, MapBox.Size));
        }

        private void UpdateImage(bool forceRefresh, Rectangle updateArea)
        {
            if (_isDisposed || _mapBox.IsDisposed)
                return;

#if DEBUG
            var watch = new Stopwatch();
            watch.Start();
#endif


            var width = _mapBox.Width;
            var height = _mapBox.Height;
            if (((_imageStatic == null && _imageVariable == null && _imageBackground == null) && !forceRefresh) ||
                (width == 0 || height == 0)) return;

            Envelope bbox = _map.Envelope;
            if (forceRefresh) // && _isRefreshing == false)
            {
                //_isRefreshing = true;
                MapBox.Tools oldTool = _mapBox.ActiveTool;
                if (_mapBox.SetToolsNoneWhileRedrawing)
                {
                    _mapBox.ActiveTool = MapBox.Tools.None;
                    _mapBox.Enabled = false;
                }

                if (_mapBox.ShowProgressUpdate)
                {
                    if (_mapBox.InvokeRequired)
                    {
                        _progressBar.BeginInvoke(new Action<ProgressBar>(p =>
                        {
                            p.Visible = true;
                            p.Enabled = true;
                        }), _progressBar);
                    }
                    else
                    {
                        _progressBar.Visible = true;
                        _progressBar.Enabled = true;
                    }
                }

                // Assert we never run into overflow errors
                if (_imageGeneration == uint.MaxValue)
                    _imageGeneration = 0;

                var generation = ++_imageGeneration;
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        GetImagesAsync(bbox, generation);
                        GetImagesAsyncEnd(new GetImageEndResult
                        {
                            Tool = oldTool, bbox = bbox, Generation = generation, UpdateArea = updateArea
#if DEBUG
                            , Watch = watch
#endif
                        });
                    });
            }
            else
            {
                GetImagesAsyncEnd(new GetImageEndResult
                {
                    Tool = null, bbox = bbox, Generation = _imageGeneration,
                    UpdateArea = updateArea
#if DEBUG
                    , Watch = watch
#endif
                });
            }
        }

    }
}
