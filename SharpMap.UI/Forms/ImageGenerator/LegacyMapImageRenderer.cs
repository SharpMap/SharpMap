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
        private Bitmap _imageBackground = new Bitmap(1, 1);
        private Bitmap _imageStatic = new Bitmap(1, 1);
        private Bitmap _imageVariable = new Bitmap(1, 1);
        private Envelope _imageEnvelope = new Envelope(0, 1, 0, 1);

        private long _idImageGeneration;
        private long _imageGeneration = long.MinValue;
        private Envelope _imageGenerationEnvelope;

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

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _isDisposed = true;

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
        }

        /// <summary>
        /// Gets access to the map
        /// </summary>
        private MapBox MapBox
        {
            get { return _mapBox; }
        }

        /// <inheritdoc cref="IMapBoxImageGenerator.Image"/>
        public Image Image
        {
            get
            {
                GetImagesAsyncEnd(null);
                return ImageValue;
            }
        }

        /// <inheritdoc cref="IMapBoxImageGenerator.ImageValue"/>
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

        /// <inheritdoc cref="IMapBoxImageGenerator.IsDisposed"/>
        public bool IsDisposed { get => _isDisposed; }

        /// <inheritdoc cref="IMapBoxImageGenerator.ImageEnvelope"/>
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
            if (IsDisposed) return;

            // working on local copies
            var mapBox = _mapBox;
            var map = _map;

            if (mapBox.IsDisposed || _isDisposed )
                return;

            Image oldRef;
            lock (mapBox.MapLocker)
            {
                if (mapBox.Dragging) return;
                oldRef = _imageVariable;
                if (map == null) return;
                _imageVariable = GetMap(map, map?.VariableLayers, LayerCollectionType.Variable, map?.Envelope);
            }

            var rect = new Rectangle(Point.Empty, mapBox.Size);
            UpdateImage(false, rect);
            if (oldRef != null)
                oldRef.Dispose();

            mapBox.Invalidate(rect);
            if (!(mapBox.IsDisposed || _isDisposed))
                mapBox.Invoke(new MethodInvoker(() => { if (!mapBox.IsDisposed) mapBox.Update(); }));

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
                int width = map?.Size.Width ?? 0;
                int height = map?.Size.Height ?? 0;

                if (map == null || layers == null || layers.Count == 0 || width <= 0 || height <= 0)
                {
                    if (layerCollectionType == LayerCollectionType.Background)
                        return new Bitmap(1, 1);
                    return null;
                }

                var res = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(res))
                {
                    g.Clear(Color.Transparent);
                    map.RenderMap(g, layerCollectionType, false, true);
                }

                return res;
            }
            catch (Exception ee)
            {
                _logger.Error("Error while rendering map", ee);

                if (layerCollectionType == LayerCollectionType.Background)
                    return new Bitmap(1, 1);
                return null;
            }
        }

        private void GetImagesAsync(Envelope extent, long imageGeneration)
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
            public long Generation { get; set; }
            public Rectangle UpdateArea { get; set; }

#if DEBUG
            public Stopwatch Watch { get; set; }
#endif
        }

        private void GetImagesAsyncEnd(GetImageEndResult res)
        {
            // draw only if generation is larger than the current, else we have already drawn something newer
            // we must to check also IsHandleCreated because during disposal, the handle of the parent is destroyed
            // sooner than progress bar's handle, this leads to cross thread operation and exception because
            // InvokeRequired returns false, but for the progress bar it is true.
            if (res == null || res.Generation < _imageGeneration || _isDisposed || !_mapBox.IsHandleCreated)
                return;

            //if (_imageGenerationEnvelope != null)
            //{
            //    if (!_imageGenerationEnvelope.Equals(res.bbox))
            //        return;
            //}

            if (_logger.IsDebugEnabled)
                _logger.DebugFormat("{0} - {1} / {2}", res.Generation, res.bbox, res.UpdateArea);

            var mapBox = _mapBox;
            if ((mapBox.SetToolsNoneWhileRedrawing|| mapBox.ShowProgressUpdate) && mapBox.InvokeRequired)
            {
                try
                {
                    mapBox.BeginInvoke(new MethodInvoker(() => GetImagesAsyncEnd(res)));
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
                    int width = mapBox.Width;
                    int height = mapBox.Height;
                    if (width > 0 && height > 0)
                    {
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
                        if (mapBox.SetToolsNoneWhileRedrawing)
                            mapBox.ActiveTool = res.Tool.Value;

                        mapBox.ClearDrag();
                        //_isRefreshing = false;

                        if (mapBox.SetToolsNoneWhileRedrawing)
                            mapBox.Enabled = true;

                        if (mapBox.ShowProgressUpdate)
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

                    mapBox.Invalidate(res.UpdateArea);
                    if (!(IsDisposed || mapBox.IsDisposed))
                        mapBox.Invoke(new MethodInvoker(() => { if (!mapBox.IsDisposed) mapBox.Update(); }));
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message, ex);
                }

#if DEBUG
                if (res.Watch != null)
                { 
                    res.Watch.Stop();
                    mapBox.LastRefreshTime = res.Watch.Elapsed;
                }
#endif

                try
                {
                    mapBox.OnMapRefreshed(EventArgs.Empty);
                }
                catch (Exception ee)
                {
                    // Trap errors that occured when calling the event-handlers
                    _logger.Warn("Exception while calling OnMapRefreshed", ee);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private Bitmap MergeImages(long generation, Rectangle rectangle)
        {
            var res = generation == _idImageGeneration
                ? (Bitmap) _image
                : new Bitmap(MapBox.Size.Width, MapBox.Size.Height);

            int counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    using (var g = Graphics.FromImage(res))
                    {
                        if (_map.BackgroundLayer.Count == 0)
                        {
                            g.Clear(_map.BackColor);
                        }
                        else
                        {
                            lock (_backgroundImagesLocker)
                                TryDrawImage(g, _imageBackground, rectangle);
                        }

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


            int width = _mapBox.Width;
            int height = _mapBox.Height;
            if (_imageStatic == null && _imageVariable == null && _imageBackground == null
                && !forceRefresh 
                || width == 0 || height == 0) return;

            var bbox = _map.Envelope;
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
                if (_imageGeneration == long.MaxValue)
                    _imageGeneration = 0;

                Interlocked.Increment(ref _imageGeneration);
                long generation = _imageGeneration;
                _imageGenerationEnvelope = bbox;
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
