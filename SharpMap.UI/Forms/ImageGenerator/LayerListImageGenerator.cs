using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using SharpMap.Styles;

namespace SharpMap.Forms.ImageGenerator
{
    public class LayerListImageGenerator : IMapBoxImageGenerator
    {
        private class LockedBitmap
        {
            public LockedBitmap(object sync = null)
            {
                Sync = sync ?? new object();
                Extent = new Envelope();
            }

            public object Sync { get; }

            public Envelope Extent { get; set; }

            public Bitmap Bitmap { get; set; }

            public override int GetHashCode()
            {
                return 17 ^ Sync.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                bool res = ReferenceEquals(Sync, (obj as LockedBitmap)?.Sync);
                return res;
            }
        }
        private class PendingDownloadTracker : IDisposable
        {
            private readonly ConcurrentBag<ITileAsyncLayer> _asyncLayers = new ConcurrentBag<ITileAsyncLayer>();
            public event DownloadProgressHandler ProgressChanged;

            public int NumPendingDownloads
            {
                get
                {
                    int num = 0;
                    foreach (var lyr in _asyncLayers)
                        num += lyr.NumPendingDownloads;
                    return num;
                }
            }

            public void Add(ITileAsyncLayer asyncLayer)
            {
                if (asyncLayer.OnlyRedrawWhenComplete)
                    return;

                _asyncLayers.Add(asyncLayer);
                asyncLayer.DownloadProgressChanged += HandleDownloadProgressChanged;
            }

            private void HandleDownloadProgressChanged(int tilesremaining)
            {
                ProgressChanged?.Invoke(NumPendingDownloads);
            }

            public void Dispose()
            {
                foreach (var tileAsyncLayer in _asyncLayers)
                    tileAsyncLayer.DownloadProgressChanged -= HandleDownloadProgressChanged;
            }
        }

        private static readonly ILog _logger = LogManager.GetLogger<LayerListImageGenerator>();

        private ProgressBar _progressBar;
        private readonly ConcurrentDictionary<ILayer, LockedBitmap> _imageLayers;
        private readonly List<ILayer> _layers;

        private readonly object _paintImageLock = new object();
        private Bitmap _paintImage;

        private CancellationTokenSource _cts = 
            new CancellationTokenSource();

        private bool _isDisposed;
        private PendingDownloadTracker _pendingDownloadTracker;
        
        private MapBox MapBox { get; set; }
        
        /// <summary>
        /// Creates an instance for this class
        /// </summary>
        /// <param name="mapBox">The map box</param>
        /// <param name="progressBar">A progress bar</param>
        public LayerListImageGenerator(MapBox mapBox, ProgressBar progressBar)
        {
            _progressBar = progressBar;
            _layers = new List<ILayer>();
            _imageLayers = new ConcurrentDictionary<ILayer, LockedBitmap>();
            NeedsUpdate = true;
            MapBox = mapBox;
            WireMapBox();
        }

        private void WireMapBox()
        {
            MapBox.MapChanged += HandleMapChanged;
            Map = MapBox.Map;
            WireMap();
        }

        private void HandleMapChanged(object sender, EventArgs e)
        {
            UnwireMap();
            Map = MapBox.Map;
            _paintImage?.Dispose();
            _paintImage = null;
            WireMap();

        }

        private void WireMap()
        {
            if (Map == null)
                return;

            /*
            _pendingDownloadTracker = new PendingDownloadTracker();
            foreach (var lyr in EnumerateLayers(Map))
            {
                if (lyr is ITileAsyncLayer asyncLyr)
                    _pendingDownloadTracker.Add(asyncLyr);
                _layers.Add(lyr);
            }
            */

            Map.BackgroundLayer.CollectionChanged += HandleMapLayerCollectionChanged;
            Map.Layers.CollectionChanged += HandleMapLayerCollectionChanged;
            Map.VariableLayers.CollectionChanged += HandleMapLayerCollectionChanged;

            Map.MapNewTileAvaliable += HandleMapNewTileAvaliable;
            Map.VariableLayers.VariableLayerCollectionRequery += HandleVariableLayersRequery;
            Map.RefreshNeeded += HandleMapRefreshNeeded;

            NeedsUpdate = true;
        }

        private void HandleMapLayerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NeedsUpdate = true;
        }

        private bool NeedsUpdate { get; set; }

        IEnumerable<ILayer> EnumerateLayers(Map map)
        {
            foreach (var lyr in EnumerateLayers(map.BackgroundLayer))
                yield return lyr;
            foreach (var lyr in EnumerateLayers(map.Layers))
                yield return lyr;
            foreach (var lyr in EnumerateLayers(map.VariableLayers))
                yield return lyr;
        }

        IEnumerable<ILayer> EnumerateLayers(LayerCollection collection)
        {
            var coll = (ICollection) collection;
            lock (coll.SyncRoot)
            {
                foreach (var lyr in collection)
                {
                    foreach (var tmpLyr in EnumerateLayers(lyr))
                        yield return tmpLyr;
                }
            }
        }

        IEnumerable<ILayer> EnumerateLayers(ILayer lyr)
        {
            if (lyr is LayerGroup lyrGroup)
            {
                foreach (var tmpLyr in lyrGroup.Layers)
                foreach (var tmpLyr2 in EnumerateLayers(tmpLyr))
                    yield return tmpLyr2;
            }
            else
            {
                yield return lyr;
            }
        }

        private void HandleVariableLayersRequery(object sender, EventArgs e)
        {
            if (IsDisposed)
                return;

            var map = Map;
            if (map.VariableLayers.Count == 0)
                return;

            var mvp = new MapViewport(map);
            foreach (var lyr in Map.VariableLayers)
            {
                ThreadPool.QueueUserWorkItem(delegate { RenderLayerImage(new object[] { lyr, mvp, _cts.Token, false }); });
            }
        }

        private void HandleMapRefreshNeeded(object sender, EventArgs e)
        {
            Generate();
        }

        private void HandleMapNewTileAvaliable(ITileAsyncLayer sender, Envelope box, Bitmap bm, int sourceWidth,
            int sourceHeight, ImageAttributes imageAttributes)
        {
            if (sender == null)
                return;

            if (IsDisposed) 
                return;

            if (!_imageLayers.TryGetValue((ILayer)sender, out var lockImg))
                return;

            var min = Point.Round(Map.WorldToImage(box.Min()));
            var max = Point.Round(Map.WorldToImage(box.Max()));
            var rect = new Rectangle(min.X, max.Y, (max.X - min.X), (min.Y - max.Y));
            lock (lockImg.Sync)
            {
                using (var g = Graphics.FromImage(lockImg.Bitmap))
                {
                    g.DrawImage(bm, rect, 0, 0,
                        sourceWidth, sourceHeight,
                        GraphicsUnit.Pixel,
                        imageAttributes);
                }
            }

            InvalidateCacheImage();

            if (IsDisposed) return;
            MapBox.Invoke(new MethodInvoker(MapBox.Invalidate), rect);
            if (IsDisposed) return;
            MapBox.Invoke(new MethodInvoker(MapBox.Update));
        }

        private void UnwireMap()
        {
            if (Map == null) return;

            Map.VariableLayers.CollectionChanged -= HandleMapLayerCollectionChanged;
            Map.Layers.CollectionChanged -= HandleMapLayerCollectionChanged;
            Map.BackgroundLayer.CollectionChanged -= HandleMapLayerCollectionChanged;

            Map.MapNewTileAvaliable -= HandleMapNewTileAvaliable;
            Map.VariableLayers.VariableLayerCollectionRequery -= HandleVariableLayersRequery;
            Map.RefreshNeeded -= HandleMapRefreshNeeded;

            ClearCache();

            Map = null;
        }

        private void ClearCache()
        {
            foreach (var layer in _layers)
            {
                if (_imageLayers.TryRemove(layer, out var lockImg))
                {
                    lock (lockImg.Sync)
                    {
                        var bitmap = lockImg.Bitmap;
                        lockImg.Bitmap = null;
                        bitmap.Dispose();
                    }
                }
            }

            if (_pendingDownloadTracker != null)
            {
                _pendingDownloadTracker.ProgressChanged -= HandleProgressChanged;
                _pendingDownloadTracker.Dispose();
            }

            _pendingDownloadTracker = new PendingDownloadTracker();
            _pendingDownloadTracker.ProgressChanged += HandleProgressChanged;

            _imageLayers.Clear();
            _layers.Clear();
        }

        private void HandleProgressChanged(int tilesRemaining)
        {
            if (IsDisposed)
                return;

            if (!MapBox.ShowProgressUpdate)
                return;

            _progressBar.BeginInvoke(new Action<ProgressBar>(p =>
            {
                p.Visible = tilesRemaining > 0;
                p.Enabled = p.Visible;
            }), _progressBar);
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
                SetDisposed();
                UnwireMap();
                ImageEnvelope = new Envelope();
            }
        }


        public Image Image
        {
            get { return ImageValue; }
        }

        public Image ImageValue {
            get
            {
                Bitmap res;
                lock (_paintImageLock)
                {
                    res = _paintImage;
                    if (res != null) return res;
                }

                res = new Bitmap(MapBox.Width, MapBox.Height);

                using (var gr = Graphics.FromImage(res))
                {
                    gr.Clear(Map.BackColor);

                    double mapZoom = Map.Zoom;
                    double mapScale = Map.MapScale;

                    for (int i = 0; i < _layers.Count; i++)
                    {
                        var lyr = _layers[i];

                        // Does the layer need rendering
                        if (!lyr.Enabled) continue;

                        // Does it fit in the visibility constraints
                        double mapCompare = lyr.VisibilityUnits == VisibilityUnits.Scale ? mapScale : mapZoom;
                        if (mapCompare < lyr.MinVisible || lyr.MaxVisible < mapCompare)
                            continue;

                        //lock (_imageLayers)
                        //{
                            if (_imageLayers.TryGetValue(lyr, out var lockImg))
                            {
                                lock (lockImg.Sync)
                                    gr.DrawImageUnscaled(lockImg.Bitmap, 0, 0);
                            }
                        //}
                    }
                }

                lock (_paintImageLock)
                    _paintImage = res;

                return res;
            }
        }

        public Envelope ImageEnvelope { get; private set; }

        public bool IsDisposed
        {
            get => _isDisposed || MapBox.IsDisposed || !MapBox.IsHandleCreated;
            //private set => _isDisposed = value;
        }

        private bool SetDisposed() => _isDisposed = true;

        public Map Map { get; private set; }

        /// <summary>
        /// Method to generate a new set of images
        /// </summary>
        public void Generate()
        {
            if (IsDisposed) return;

            _logger.Debug(t => t("\n{0}> Enter Generate", Thread.CurrentThread.ManagedThreadId));
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            if (NeedsUpdate)
            {
                ClearCache();
                foreach (var lyr in EnumerateLayers(Map))
                {
                    if (lyr is ITileAsyncLayer asyncLyr)
                        _pendingDownloadTracker.Add(asyncLyr);
                    _layers.Add(lyr);
                }
                NeedsUpdate = false;
            }

            var map = MapBox.Map;
            double mapZoom = map.Zoom;
            double mapScale = map.MapScale;

            var mvp = new MapViewport(map);

            for (int i = 0; i < _layers.Count; i++)
            {
                // Has an cancellation ben requested
                if (_cts.IsCancellationRequested)
                    break;

                // Get the layer
                var lyr = _layers[i];

                // Does the layer need rendering
                if (!lyr.Enabled) continue;

                // Layer has no envelope, it has no data
                if (lyr.Envelope?.IsNull ?? false) continue;
                
                // Does it fit in the visibility constraints
                double mapCompare = lyr.VisibilityUnits == VisibilityUnits.Scale ? mapScale : mapZoom;    
                if (mapCompare < lyr.MinVisible || lyr.MaxVisible < mapCompare)
                    continue;

                // Add task to list
                bool invalidateAll = i == _layers.Count - 1;
                ThreadPool.QueueUserWorkItem(delegate { RenderLayerImage(new object[] {lyr, mvp, _cts.Token, invalidateAll}); });
            }

            ImageEnvelope = mvp.Envelope;

            _logger.Debug(t => t("\n{0}> Exit Generate", Thread.CurrentThread.ManagedThreadId));
        }

        /// <summary>
        /// Function to render a layer to an image
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private Rectangle RenderLayerImage(object param)
        {
            if (IsDisposed) return Rectangle.Empty;


            object[] parameters = (object[]) param;
            var lyr = (ILayer)parameters[0];
            var mvp = (MapViewport) parameters[1];
            var token = (CancellationToken) parameters[2];

            //if ((bool)parameters[3]) MapBox.Invalidate();

            if (token.IsCancellationRequested)
                return Rectangle.Empty;

            var updateRect = Rectangle.Empty;

            var sw = new Stopwatch();
            _logger.Debug(t => t("\n{0}> Enter RenderLayerImage {1}\n{0}>\t{2} => {3}",
                Thread.CurrentThread.ManagedThreadId, lyr.LayerName, mvp.Envelope, mvp.Size));
            sw.Start();

            LockedBitmap img = null;
            if (lyr is ITileAsyncLayer)
            {
                _imageLayers.TryGetValue(lyr, out img);
                //return Rectangle.Empty;
            }

            // Update size if necessary
            if (img != null)
            {
                lock (img.Sync)
                {
                    var imageObj = img.Bitmap;
                    if (img.Bitmap != null && img.Bitmap.Size != Map.Size)
                    {
                        object lockObj = img.Sync;
                        var newLockImg = new LockedBitmap(lockObj) { 
                            Bitmap = new Bitmap(mvp.Size.Width, mvp.Size.Height, PixelFormat.Format32bppArgb)
                        };
                        while (!_imageLayers.TryUpdate(lyr, newLockImg, img))
                            Thread.Sleep(25);
                        img = newLockImg;
                        //imageObj.Dispose();
                    }
                }
            }
            else //if (img == null)
            {
                img = new LockedBitmap {
                    Bitmap = new Bitmap(mvp.Size.Width, mvp.Size.Height, PixelFormat.Format32bppArgb)
                };
                img = _imageLayers.AddOrUpdate(lyr, img, (u,v) => img);
            }

            lock (img.Sync)
            {
                try
                {
                    using (var gr = Graphics.FromImage(img.Bitmap))
                    {
                        gr.Clear(Color.Transparent);
                        lyr.Render(gr, mvp);
                    }
                }
                catch (Exception)
                {
                    goto Exit;
                }
            }

            if (token.IsCancellationRequested)
                goto Exit;

            // Envelope of the current layer
            var lyrEnvelope = lyr.Envelope;

            if (lyrEnvelope.Width == 0)
            {
                if (lyr is VectorLayer vlyr)
                {
                    double pointSize = vlyr.Style.PointSize;
                    if (vlyr.Style.Symbol != null)
                        pointSize =  vlyr.Style.Symbol.Size.Width * vlyr.Style.SymbolScale;
                    pointSize = 1.05d * Map.PixelWidth * pointSize;
                    lyrEnvelope.ExpandBy(pointSize);
                }
            }

            // Envelope of the last rendered image
            var imgEnvelope = img.Extent.Copy();
            imgEnvelope.ExpandToInclude(lyrEnvelope);

            var updateArea = mvp.Envelope.Intersection(imgEnvelope);
            var lt = Point.Truncate( mvp.WorldToImage(updateArea.TopLeft()));
            var rb = Point.Ceiling(mvp.WorldToImage(updateArea.BottomRight()));

            if (token.IsCancellationRequested)
                goto Exit;
            
            updateRect = Rectangle.FromLTRB(lt.X, lt.Y, rb.X, rb.Y);

            // Set the image envelope
            img.Extent = mvp.Envelope.Intersection(lyr.Envelope);


            var mapBox = MapBox;
            if (mapBox.IsHandleCreated)
            {
                _logger.Debug(t => t("\n{0}> Invalidating rectangle {1}",Thread.CurrentThread.ManagedThreadId, updateRect));
                InvalidateCacheImage();
                if ((bool) parameters[3])
                    mapBox.Invalidate();
                else
                    mapBox.Invalidate(updateRect);

                //MapBox.Invoke(new MethodInvoker(() => MapBox.Invalidate(updateRect)));
                if (!IsDisposed)
                    mapBox.Invoke(new MethodInvoker(() => { if (!mapBox.IsDisposed) mapBox.Update();}));
            }

            Exit:
            sw.Stop();
            _logger.Debug(t => t("\n{0}> Exit RenderLayerImage {1} after {4}ms \n{0}>\t{2} => {3}",
                Thread.CurrentThread.ManagedThreadId, lyr.LayerName, mvp.Envelope, mvp.Size,
                sw.ElapsedMilliseconds));
            if (updateRect.IsEmpty)
                _logger.Debug(t => t("\n{0}> Exit RenderLayerImage because of cancellation",
                    Thread.CurrentThread.ManagedThreadId, lyr.LayerName, mvp.Envelope, mvp.Size,
                    sw.ElapsedMilliseconds));

            return updateRect;
        }
        
        private void InvalidateCacheImage()
        {
            if (Monitor.TryEnter(_paintImageLock, 25))
            {
                _paintImage?.Dispose();
                _paintImage = null;
                Monitor.Exit(_paintImageLock);
            }
            else
            {
                _logger.Debug(t => t("\n{0}> Couldn't invalidate cache image within 25ms!"));
            }
        }
    }
}
