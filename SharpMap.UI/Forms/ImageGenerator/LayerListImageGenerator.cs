using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Logging;
using GeoAPI.Geometries;
using SharpMap.Layers;

namespace SharpMap.Forms.ImageGenerator
{
    public class LayerListImageGenerator : IMapBoxImageGenerator
    {
        private static readonly ILog _logger = LogManager.GetLogger<LayerListImageGenerator>();

        private ProgressBar _progressBar;
        private readonly ConcurrentDictionary<ILayer, Tuple<object, Bitmap>> _imageLayers;
        private readonly List<ILayer> _layers;

        private readonly object _paintImageLock = new object();
        private Bitmap _paintImage;

        private CancellationTokenSource _cts = 
            new CancellationTokenSource();

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
            _imageLayers = new ConcurrentDictionary<ILayer, Tuple<object, Bitmap>>();

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
            WireMap();

        }

        private void WireMap()
        {
            if (Map == null)
                return;

                foreach (var lyr in EnumerateLayers(Map))
                    _layers.Add(lyr);

            Map.BackgroundLayer.CollectionChanged += HandleMapLayerCollectionChanged;
            Map.Layers.CollectionChanged += HandleMapLayerCollectionChanged;
            Map.VariableLayers.CollectionChanged += HandleMapLayerCollectionChanged;

            Map.MapNewTileAvaliable += HandleMapNewTileAvaliable;
            Map.VariableLayers.VariableLayerCollectionRequery += HandleVariableLayersRequery;
            Map.RefreshNeeded += HandleMapRefreshNeeded;
        }

        private void HandleMapLayerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NeedsUpdate = true;

            /*
            ClearCache();
            foreach (var lyr in EnumerateLayers(Map))
                _layers.Add(lyr);
             */
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
            //using (var map = Map.Clone())
            //{
                var map = Map;
                if (map.VariableLayers.Count == 0)
                    return;

            //  map.DisposeLayersOnDispose = false;

            var mvp = new MapViewport(map.ID, map.SRID, new Envelope(map.Envelope),
                  map.Size, map.PixelAspectRatio, map.MapTransform);
            foreach (var lyr in Map.VariableLayers)
            {
                //new Task<Rectangle>(RenderLayerImage, new object[] {lyr, mvp, _cts.Token }).Start();
                ThreadPool.QueueUserWorkItem(delegate { RenderLayerImage(new object[] { lyr, mvp, _cts.Token }); });
            }
            //}

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

            if (!_imageLayers.TryGetValue((ILayer)sender, out var lockImg))
                return;

            if (MapBox.IsDisposed == false && IsDisposed == false)
            {
                var min = Point.Round(Map.WorldToImage(box.Min()));
                var max = Point.Round(Map.WorldToImage(box.Max()));
                var rect = new Rectangle(min.X, max.Y, (max.X - min.X), (min.Y - max.Y));
                lock (lockImg.Item1)
                {
                    using (var g = Graphics.FromImage(lockImg.Item2))
                    {
                        g.DrawImage(bm, rect, 0, 0,
                            sourceWidth, sourceHeight,
                            GraphicsUnit.Pixel,
                            imageAttributes);
                    }
                }

                InvalidateCacheImage();

                MapBox.Invoke(new MethodInvoker(MapBox.Invalidate), rect);
                MapBox.Invoke(new MethodInvoker(MapBox.Update));
            }
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
                    lock (lockImg.Item1)
                        lockImg.Item2.Dispose();
                }
            }
            _imageLayers.Clear();
            _layers.Clear();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }



        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                IsDisposed = true;
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
                    for (var i = 0; i < _layers.Count; i++)
                    {
                        lock(_imageLayers)
                        if (_imageLayers.TryGetValue(_layers[i], out var lockImg))
                        {
                            lock (lockImg.Item1)
                                gr.DrawImageUnscaled(lockImg.Item2, 0, 0);
                        }
                    }
                }

                lock (_paintImageLock)
                    _paintImage = res;

                return res;
            }
        }

        public Envelope ImageEnvelope { get; private set; }

        public bool IsDisposed { get; private set; }
        public Map Map { get; private set; }

        /// <summary>
        /// Method to generate a new set of images
        /// </summary>
        public void Generate()
        {
            if ( !MapBox.IsHandleCreated || IsDisposed || MapBox.IsDisposed || !MapBox.IsHandleCreated)
                return;

            _logger.Debug(t => t("\n{0}> Enter Generate", Thread.CurrentThread.ManagedThreadId));
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            if (NeedsUpdate)
            {
                ClearCache();
                foreach (var lyr in EnumerateLayers(Map))
                    _layers.Add(lyr);
            }

            var map = MapBox.Map;
            //using (var map = MapBox.Map)
            //{
            //    map.DisposeLayersOnDispose = false;

                var mvp = new MapViewport(map.ID, map.SRID, new Envelope(map.Envelope),
                    map.Size, map.PixelAspectRatio, map.MapTransform);

                //var tasks = new List<Task>();
                var tf = new TaskFactory();
                for (var i = 0; i < _layers.Count; i++)
                {
                    // Has an cancellation ben requested
                    if (_cts.IsCancellationRequested)
                        break;

                    // Get the layer
                    var lyr = _layers[i];

                    // Does the layer need rendering
                    if (!lyr.Enabled) continue;

                    // Add task to list
                    //tasks.Add(tf.StartNew(RenderLayerImage, new object[] {lyr, mvp, _cts.Token}));
                    //new Task<Rectangle>(RenderLayerImage, new object[] { lyr, mvp, _cts.Token }).Start();
                    ThreadPool.QueueUserWorkItem(delegate { RenderLayerImage(new object[] {lyr, mvp, _cts.Token}); });
                }

                ImageEnvelope = mvp.Envelope;
            //}

            _logger.Debug(t => t("\n{0}> Exit Generate", Thread.CurrentThread.ManagedThreadId));
        }

        private static Random Rnd = new Random(445861);

        /// <summary>
        /// Function to render a layer to an image
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private Rectangle RenderLayerImage(object param)
        {
            var parameters = (object[]) param;
            var lyr = (ILayer)parameters[0];
            var mvp = (MapViewport) parameters[1];

            var sw = new Stopwatch();
            _logger.Debug(t => t("\n{0}> Enter RenderLayerImage {1}\n{0}>\t{2} => {3}",
                Thread.CurrentThread.ManagedThreadId, lyr.LayerName, mvp.Envelope, mvp.Size));
            sw.Start();

            var isAsync = false;
            Tuple<object, Bitmap> img = null;
            if (lyr is ITileAsyncLayer)
            {
                _imageLayers.TryGetValue(lyr, out img);
                //img = _imageLayers[lyr];
                isAsync = true;
            }

            // Update size if necessary
            if (img != null)
            {
                lock (img.Item1)
                    if (img.Item2 != null && img.Item2.Size != Map.Size)
                    {
                        var lockObj = img.Item1;
                        var imageObj = img.Item2;
                        var newLockImg = Tuple.Create(lockObj,
                            new Bitmap(mvp.Size.Width, mvp.Size.Height, PixelFormat.Format32bppArgb));
                        _imageLayers.TryUpdate(lyr, newLockImg, img);
                        //imageObj.Dispose();
                    }
            }

            if (img == null)
            {

                img = Tuple.Create(new object(), new Bitmap(mvp.Size.Width, mvp.Size.Height, PixelFormat.Format32bppArgb));
                _imageLayers.AddOrUpdate(lyr, img, (u,v) => img);
                //lock (((IDictionary)_imageLayers).SyncRoot)
                //_imageLayers[lyr] = img;
            }

            //img = _imageLayers[lyr];
            lock (img.Item1)
            {
                using (var gr = Graphics.FromImage(img.Item2))
                {
                    gr.Clear(Color.Transparent);
                    lyr.Render(gr, mvp);
                    //if (Rnd.NextDouble() < 0.15)
                    //  Thread.Sleep(2000);
                }
            }

            var updateArea = mvp.Envelope.Intersection(lyr.Envelope);
            var lt = Point.Truncate( mvp.WorldToImage(updateArea.TopLeft()));
            var rb = Point.Ceiling(mvp.WorldToImage(updateArea.BottomRight()));
            var updateRect = Rectangle.FromLTRB(lt.X, lt.Y, rb.X, rb.Y);

            if (MapBox.IsHandleCreated)
            {
                _logger.Debug(t => t("\n{0}> Invalidating rectangle {1}",Thread.CurrentThread.ManagedThreadId, updateRect));
                InvalidateCacheImage();

                MapBox.Invalidate(updateRect);
                //MapBox.Invoke(new MethodInvoker(() => MapBox.Invalidate(updateRect)));
                MapBox.Invoke(new MethodInvoker(() => MapBox.Update()));

                /*
                InvalidateCacheImage();
                MapBox.Invoke(new MethodInvoker(MapBox.Invalidate), updateRect);
                MapBox.Invoke(new MethodInvoker(MapBox.Update));
                */
            }

            sw.Stop();
            _logger.Debug(t => t("\n{0}> Exit RenderLayerImage {1} after {4}ms \n{0}>\t{2} => {3}",
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
        }
    }
}
