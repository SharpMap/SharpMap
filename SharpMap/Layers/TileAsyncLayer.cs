using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
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
    public class TileAsyncLayer : TileLayer, ITileAsyncLayer
    {
        static ILog logger = LogManager.GetLogger(typeof(TileAsyncLayer));

        private List<BackgroundWorker> threadList = new List<BackgroundWorker>();
        private Random r = new Random(DateTime.Now.Second);
        public TileAsyncLayer(ITileSource tileSource, string layerName)
            : base(tileSource, layerName, new Color(), true, null)
        {
        }

        public TileAsyncLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile)
            : base(tileSource, layerName, transparentColor, showErrorInTile, null)
        {
        }

        public TileAsyncLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, string fileCacheDir)
            : base(tileSource, layerName, transparentColor, showErrorInTile, fileCacheDir)
        {
        }

        /// <summary>
        /// EventHandler for event fired when a new Tile is available for rendering
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="bm"></param>
        public event MapNewTileAvaliabledHandler MapNewTileAvaliable;

        public override void Render(Graphics graphics, Map map)
        {

            var bbox = map.Envelope;
            var extent = new Extent(bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY);
            int level = BruTile.Utilities.GetNearestLevel(_source.Schema.Resolutions, map.PixelSize);
            var tiles = _source.Schema.GetTilesInView(extent, level);

            //Abort previous running Threads
            lock (threadList)
            {
                foreach (BackgroundWorker t in threadList)
                {
                    if (t.IsBusy)
                    {
                        t.CancelAsync();
                    }
                }
                threadList.Clear();
            }

            foreach (TileInfo info in tiles)
            {
                if (_bitmaps.Find(info.Index) != null)
                {
                    //ThreadPool.QueueUserWorkItem(OnMapNewtileAvailableHelper, new object[] { info, _bitmaps.Find(info.Index) });
                    //draws directly the bitmap
                    var bb = new Envelope(new Coordinate(info.Extent.MinX, info.Extent.MinY), new Coordinate(info.Extent.MaxX, info.Extent.MaxY));
                    HandleMapNewTileAvaliable(map,graphics, bb, _bitmaps.Find(info.Index), _source.Schema.Width, _source.Schema.Height, _imageAttributes);
                }
                else if (_fileCache != null && _fileCache.Exists(info.Index))
                {
                    
                    Bitmap img = GetImageFromFileCache(info) as Bitmap;
                    _bitmaps.Add(info.Index, img);
                    
                    //ThreadPool.QueueUserWorkItem(OnMapNewtileAvailableHelper, new object[] { info, img });
                    //draws directly the bitmap
                    var btExtent = info.Extent;
                    var bb = new Envelope(new Coordinate(btExtent.MinX, btExtent.MinY), new Coordinate(btExtent.MaxX, btExtent.MaxY));
                    HandleMapNewTileAvaliable(map, graphics, bb, _bitmaps.Find(info.Index), _source.Schema.Width, _source.Schema.Height, _imageAttributes);
                }
                else
                {
                    BackgroundWorker b = new BackgroundWorker();
                    b.WorkerSupportsCancellation = true;
                    b.DoWork += new DoWorkEventHandler(b_DoWork);
                    b.RunWorkerAsync(new object[] { _source.Provider, info, _bitmaps, true });
                    //Thread t = new Thread(new ParameterizedThreadStart(GetTileOnThread));
                    //t.Name = info.ToString();
                    //t.IsBackground = true;
                    //t.Start();
                    lock (threadList)
                    {
                        threadList.Add(b);
                    }
                }
            }

        }

        void b_DoWork(object sender, DoWorkEventArgs e)
        {
            this.GetTileOnThread((BackgroundWorker) sender, e.Argument);
            
        }


        static void HandleMapNewTileAvaliable(Map _map, Graphics g, Envelope box, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes)
        {

            try
            {
                var min = _map.WorldToImage(box.Min());
                var max = _map.WorldToImage(box.Max());

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
                logger.Warn(ex.Message, ex);
                //this can be a GDI+ Hell Exception...
            }

        }

        /*
        private void OnMapNewtileAvailableHelper(object parameter)
        {
            //this is to wait for the main UI thread to finalize rendering ... (buggy code here)...
            System.Threading.Thread.Sleep(100);
            object[] paramters = (object[])parameter;
            if (paramters.Length != 2) throw new ArgumentException("Two parameters expected");
            TileInfo tileInfo = (TileInfo)paramters[0];
            Bitmap bm = (Bitmap)paramters[1];
            OnMapNewTileAvaliable(tileInfo, bm);
        }
        */

        private void GetTileOnThread(BackgroundWorker worker, object parameter)
        {
            System.Threading.Thread.Sleep(50 + (r.Next(5)*10));
            object[] parameters = (object[])parameter;
            if (parameters.Length != 4) throw new ArgumentException("Four parameters expected");
            ITileProvider tileProvider = (ITileProvider)parameters[0];
            TileInfo tileInfo = (TileInfo)parameters[1];
            MemoryCache<Bitmap> bitmaps = (MemoryCache<Bitmap>)parameters[2];
            bool retry = (bool)parameters[3];


            byte[] bytes;
            try
            {
                
                if (worker.CancellationPending == true)
                    return;
                bytes = tileProvider.GetTile(tileInfo);
                if (worker.CancellationPending == true)
                    return;
                Bitmap bitmap = new Bitmap(new MemoryStream(bytes));
                bitmaps.Add(tileInfo.Index, bitmap);
                if (_fileCache != null && !_fileCache.Exists(tileInfo.Index))
                {
                    AddImageToFileCache(tileInfo, bitmap);
                }

                if (worker.CancellationPending == true)
                    return;
                OnMapNewTileAvaliable(tileInfo, bitmap);
                if (worker.CancellationPending == true)
                    return;
            }
            catch (WebException ex)
            {
                if (retry == true)
                {
                    parameters[3] = false;
                    GetTileOnThread( worker, parameters);
                }
                else
                {
                    if (_showErrorInTile)
                    {
                        //an issue with this method is that one an error tile is in the memory cache it will stay even
                        //if the error is resolved. PDD.
                        Bitmap bitmap = new Bitmap(_source.Schema.Width, _source.Schema.Height);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.DrawString(ex.Message, new Font(FontFamily.GenericSansSerif, 12), new SolidBrush(Color.Black),
                            new RectangleF(0, 0, _source.Schema.Width, _source.Schema.Height));
                        //Draw the Timeout Tile
                        OnMapNewTileAvaliable(tileInfo, bitmap);

                        //With timeout we don't add to the internal cache
                        //bitmaps.Add(tileInfo.Index, bitmap);
                    }
                }
            }
            catch (ThreadAbortException tex)
            {
                if (logger.IsInfoEnabled)
                    logger.InfoFormat("TileAsyncLayer - Thread aborting: {0}", System.Threading.Thread.CurrentThread.Name);
            }
            catch (Exception ex)
            {
                logger.Warn("TileAsyncLayer - GetTileOnThread Exception", ex);
            }
        }

        private void OnMapNewTileAvaliable(TileInfo tileInfo, Bitmap bitmap)
        {
            if (MapNewTileAvaliable != null)
            {
                var bb = new Envelope(new Coordinate(tileInfo.Extent.MinX, tileInfo.Extent.MinY), new Coordinate(tileInfo.Extent.MaxX, tileInfo.Extent.MaxY));
                MapNewTileAvaliable(this, bb, bitmap, _source.Schema.Width, _source.Schema.Height, _imageAttributes);
            }
        }


    }


}
