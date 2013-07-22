using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using BruTile;
using BruTile.Cache;
using System.IO;
using System.Net;
using GeoAPI.Geometries;
using System.ComponentModel;
using Common.Logging;

namespace SharpMap.Layers
{
    /// <summary>
    /// Tile layer class that gets and serves tiles asynchonously
    /// </summary>
    [Serializable]
    public class TileAsyncLayer : TileLayer, ITileAsyncLayer
    {
        class DownloadTask
        {
            public CancellationTokenSource CancellationToken;
            public System.Threading.Tasks.Task Task;
        }

        static readonly ILog Logger = LogManager.GetLogger(typeof(TileAsyncLayer));
        private readonly List<DownloadTask> _currentTasks = new List<DownloadTask>();

        int _numPendingDownloads = 0;
        bool _onlyRedrawWhenComplete = false;

        /// <summary>
        /// Gets or Sets a value indicating if to redraw the map only when all tiles are downloaded
        /// </summary>
        public bool OnlyRedrawWhenComplete
        {
            get { return _onlyRedrawWhenComplete; }
            set { _onlyRedrawWhenComplete = value; }
        }
        
        /// <summary>
        /// Returns the number of tiles that are in queue for download
        /// </summary>
        public int NumPendingDownloads { get { return _numPendingDownloads; } }

        /// <summary>
        /// Event raised when tiles are downloaded
        /// </summary>
        public event DownloadProgressHandler DownloadProgressChanged;

        
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="tileSource">The tile source</param>
        /// <param name="layerName">The layers name</param>
        public TileAsyncLayer(ITileSource tileSource, string layerName)
            : base(tileSource, layerName, new Color(), true, null)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="tileSource">The tile source</param>
        /// <param name="layerName">The layers name</param>
        /// <param name="transparentColor">The color that should be treated as <see cref="Color.Transparent"/></param>
        /// <param name="showErrorInTile">Value indicating that an error tile should be generated for non-existent tiles</param>
        public TileAsyncLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile)
            : base(tileSource, layerName, transparentColor, showErrorInTile, null)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="tileSource">The tile source</param>
        /// <param name="layerName">The layers name</param>
        /// <param name="transparentColor">The color that should be treated as <see cref="Color.Transparent"/></param>
        /// <param name="showErrorInTile">Value indicating that an error tile should be generated for non-existent tiles</param>
        /// <param name="fileCacheDir">The directories where tiles should be stored</param>
        public TileAsyncLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, string fileCacheDir)
            : base(tileSource, layerName, transparentColor, showErrorInTile, fileCacheDir)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="tileSource">The tile source</param>
        /// <param name="layerName">The layers name</param>
        /// <param name="transparentColor">The color that should be treated as <see cref="Color.Transparent"/></param>
        /// <param name="showErrorInTile">Value indicating that an error tile should be generated for non-existent tiles</param>
        /// <param name="fileCache">If the layer should use a file-cache so store tiles, set this to a fileCacheProvider. Set to null to avoid filecache</param>
        /// <param name="imgFormat">Set the format of the tiles to be used</param>
        public TileAsyncLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, FileCache fileCache, ImageFormat imgFormat)
            : base(tileSource, layerName, transparentColor, showErrorInTile, fileCache,imgFormat)
        {
        }

        /// <summary>
        /// EventHandler for event fired when a new Tile is available for rendering
        /// </summary>
        public event MapNewTileAvaliabledHandler MapNewTileAvaliable;

        /// <summary>
        /// Method to cancel the async layer
        /// </summary>
        public void Cancel()
        {
            lock (_currentTasks)
            {
                foreach (var t in _currentTasks)
                {
                    if (!t.Task.IsCompleted)
                        t.CancellationToken.Cancel();
                }
                _currentTasks.Clear();
                _numPendingDownloads = 0;
            }
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="graphics">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics graphics, Map map)
        {
            var bbox = map.Envelope;
            var extent = new Extent(bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY);
            int level = BruTile.Utilities.GetNearestLevel(_source.Schema.Resolutions, map.PixelSize);
            var tiles = _source.Schema.GetTilesInView(extent, level);

            //Abort previous running Threads
            Cancel();

            using (var ia = new ImageAttributes())
            {
                if (!_transparentColor.IsEmpty)
                    ia.SetColorKey(_transparentColor, _transparentColor);
#if !PocketPC
                ia.SetWrapMode(WrapMode.TileFlipXY);
#endif
                foreach (TileInfo info in tiles)
                {
                    if (_bitmaps.Find(info.Index) != null)
                    {
                        //draws directly the bitmap
                        var bb = new Envelope(new Coordinate(info.Extent.MinX, info.Extent.MinY),
                                              new Coordinate(info.Extent.MaxX, info.Extent.MaxY));
                        HandleMapNewTileAvaliable(map, graphics, bb, _bitmaps.Find(info.Index), _source.Schema.Width,
                                                  _source.Schema.Height, ia);
                    }
                    else if (_fileCache != null && _fileCache.Exists(info.Index))
                    {

                        Bitmap img = GetImageFromFileCache(info) as Bitmap;
                        _bitmaps.Add(info.Index, img);

                        //draws directly the bitmap
                        var btExtent = info.Extent;
                        var bb = new Envelope(new Coordinate(btExtent.MinX, btExtent.MinY),
                                              new Coordinate(btExtent.MaxX, btExtent.MaxY));
                        HandleMapNewTileAvaliable(map, graphics, bb, _bitmaps.Find(info.Index), _source.Schema.Width,
                                                  _source.Schema.Height, ia);
                    }
                    else
                    {
                        var cancelToken = new CancellationTokenSource();
                        var token = cancelToken.Token;
                        var l_info = info;
                        if (Logger.IsDebugEnabled)
                            Logger.DebugFormat("Starting new Task to download tile {0},{1},{2}", info.Index.Level, info.Index.Col, info.Index.Row);
                        var t = new System.Threading.Tasks.Task(delegate
                        {
                            if (token.IsCancellationRequested)
                                token.ThrowIfCancellationRequested();

                            if (Logger.IsDebugEnabled)
                                Logger.DebugFormat("Task started for download of tile {0},{1},{2}", info.Index.Level, info.Index.Col, info.Index.Row);

                            var res = GetTileOnThread(token, _source.Provider, l_info, _bitmaps, true);
                            if (res)
                            {
                                Interlocked.Decrement(ref _numPendingDownloads);
                                var e = DownloadProgressChanged;
                                if (e != null)
                                    e(_numPendingDownloads);
                            }

                        }, token);
                        var dt = new DownloadTask() { CancellationToken = cancelToken, Task = t };
                        lock (_currentTasks)
                        {
                            _currentTasks.Add(dt);
                            _numPendingDownloads++;
                        }
                        t.Start();
                    }
                }
            }

        }

        static void HandleMapNewTileAvaliable(Map map, Graphics g, Envelope box, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes)
        {

            try
            {
                var min = map.WorldToImage(box.Min());
                var max = map.WorldToImage(box.Max());

                min = new PointF((float)Math.Round(min.X), (float)Math.Round(min.Y));
                max = new PointF((float)Math.Round(max.X), (float)Math.Round(max.Y));

                g.DrawImage(bm,
                    new Rectangle((int)min.X, (int)max.Y, (int)(max.X - min.X), (int)(min.Y - max.Y)),
                    0, 0,
                    sourceWidth, sourceHeight,
                    GraphicsUnit.Pixel,
                    imageAttributes);

                // g.Dispose();

            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
                //this can be a GDI+ Hell Exception...
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <param name="tileProvider"></param>
        /// <param name="tileInfo"></param>
        /// <param name="bitmaps"></param>
        /// <param name="retry"></param>
        /// <returns>true if thread finished without getting cancellation signal, false = cancelled</returns>
        private bool GetTileOnThread(CancellationToken cancelToken, ITileProvider tileProvider, TileInfo tileInfo, MemoryCache<Bitmap> bitmaps, bool retry)
        {
            byte[] bytes;
            try
            {
                if (cancelToken.IsCancellationRequested)
                    cancelToken.ThrowIfCancellationRequested();


                //We may have gotten the tile from another thread now..
                if (bitmaps.Find(tileInfo.Index) != null)
                {
                    return true;
                }

                if (Logger.IsDebugEnabled)
                    Logger.DebugFormat("Calling gettile on provider for tile {0},{1},{2}", tileInfo.Index.Level, tileInfo.Index.Col, tileInfo.Index.Row);

                bytes = tileProvider.GetTile(tileInfo);
                if (cancelToken.IsCancellationRequested)
                    cancelToken.ThrowIfCancellationRequested();

                using (var ms = new MemoryStream(bytes))
                {
                    Bitmap bitmap = new Bitmap(ms);
                    bitmaps.Add(tileInfo.Index, bitmap);
                    if (_fileCache != null && !_fileCache.Exists(tileInfo.Index))
                    {
                        AddImageToFileCache(tileInfo, bitmap);
                    }


                    if (cancelToken.IsCancellationRequested)
                        cancelToken.ThrowIfCancellationRequested();
                    OnMapNewTileAvaliable(tileInfo, bitmap);
                }
                return true;
            }
            catch (WebException ex)
            {
                if (Logger.IsDebugEnabled)
                    Logger.DebugFormat("Exception downloading tile {0},{1},{2} {3}", tileInfo.Index.Level, tileInfo.Index.Col, tileInfo.Index.Row, ex.Message);
                
                if (retry)
                {
                    return GetTileOnThread(cancelToken, tileProvider, tileInfo, bitmaps, false);
                }
                else
                {
                    if (_showErrorInTile)
                    {
                        //an issue with this method is that one an error tile is in the memory cache it will stay even
                        //if the error is resolved. PDD.
                        var bitmap = new Bitmap(_source.Schema.Width, _source.Schema.Height);
                        using (var graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.DrawString(ex.Message, new Font(FontFamily.GenericSansSerif, 12), new SolidBrush(Color.Black),
                                new RectangleF(0, 0, _source.Schema.Width, _source.Schema.Height));
                        }
                        //Draw the Timeout Tile
                        OnMapNewTileAvaliable(tileInfo, bitmap);
                        //With timeout we don't add to the internal cache
                        //bitmaps.Add(tileInfo.Index, bitmap);
                    }
                    return true;
                }
            }
            catch (System.OperationCanceledException tex)
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.InfoFormat("TileAsyncLayer - Thread aborting: {0}", Thread.CurrentThread.Name);
                    Logger.InfoFormat(tex.Message);
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn("TileAsyncLayer - GetTileOnThread Exception", ex);
                //This is not due to cancellation so return true
                return true;
            }
        }

        private void OnMapNewTileAvaliable(TileInfo tileInfo, Bitmap bitmap)
        {
            if (_onlyRedrawWhenComplete)
                return;

            if (MapNewTileAvaliable != null)
            {
                var bb = new Envelope(new Coordinate(tileInfo.Extent.MinX, tileInfo.Extent.MinY), new Coordinate(tileInfo.Extent.MaxX, tileInfo.Extent.MaxY));
                using (var ia = new ImageAttributes())
                {
                    if (!_transparentColor.IsEmpty)
                        ia.SetColorKey(_transparentColor, _transparentColor);
#if !PocketPC
                    ia.SetWrapMode(WrapMode.TileFlipXY);
#endif
                    MapNewTileAvaliable(this, bb, bitmap, _source.Schema.Width, _source.Schema.Height, ia);
                }
            }
        }


    }


}
