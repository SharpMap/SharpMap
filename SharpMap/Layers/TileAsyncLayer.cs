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
using SharpMap.Geometries;

namespace SharpMap.Layers
{
    public class TileAsyncLayer : TileLayer, ITileAsyncLayer
    {
        private List<Thread> threadList = new List<Thread>();
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

            Extent extent = new Extent(map.Envelope.Min.X, map.Envelope.Min.Y, map.Envelope.Max.X, map.Envelope.Max.Y);
            int level = BruTile.Utilities.GetNearestLevel(_source.Schema.Resolutions, map.PixelSize);
            IList<TileInfo> tiles = _source.Schema.GetTilesInView(extent, level);

            foreach (Thread t in threadList)
            {
                if (t.IsAlive)
                {
                    t.Abort();
                    t.Join();
                }
            }
            threadList.Clear();

            foreach (TileInfo info in tiles)
            {
                if (_bitmaps.Find(info.Index) != null)
                {
                    //ThreadPool.QueueUserWorkItem(OnMapNewtileAvailableHelper, new object[] { info, _bitmaps.Find(info.Index) });
                    //draws directly the bitmap
                    BoundingBox bb = new BoundingBox(info.Extent.MinX, info.Extent.MinY, info.Extent.MaxX, info.Extent.MaxY);
                    HandleMapNewTileAvaliable(map,graphics, bb, _bitmaps.Find(info.Index), _source.Schema.Width, _source.Schema.Height, _imageAttributes);
                }
                else if (_fileCache != null && _fileCache.Exists(info.Index))
                {
                    
                    Bitmap img = GetImageFromFileCache(info) as Bitmap;
                    _bitmaps.Add(info.Index, img);
                    
                    //ThreadPool.QueueUserWorkItem(OnMapNewtileAvailableHelper, new object[] { info, img });
                    //draws directly the bitmap
                    BoundingBox bb = new BoundingBox(info.Extent.MinX, info.Extent.MinY, info.Extent.MaxX, info.Extent.MaxY);
                    HandleMapNewTileAvaliable(map, graphics, bb, _bitmaps.Find(info.Index), _source.Schema.Width, _source.Schema.Height, _imageAttributes);
                }
                else
                {
                    Thread t = new Thread(new ParameterizedThreadStart(GetTileOnThread));
                    t.Name = info.ToString();
                    t.IsBackground = true;
                    t.Start(new object[] { _source.Provider, info, _bitmaps });
                    threadList.Add(t);
                }
            }

        }


        void HandleMapNewTileAvaliable(Map _map, Graphics g, BoundingBox box, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes)
        {

            try
            {
                PointF min = _map.WorldToImage(new SharpMap.Geometries.Point(box.Min.X, box.Min.Y));
                PointF max = _map.WorldToImage(new SharpMap.Geometries.Point(box.Max.X, box.Max.Y));

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
                Console.WriteLine(ex.ToString());
                //this can be a GDI+ Hell Exception...
            }

        }
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


        private void GetTileOnThread(object parameter)
        {
            object[] parameters = (object[])parameter;
            if (parameters.Length != 3) throw new ArgumentException("Three parameters expected");
            ITileProvider tileProvider = (ITileProvider)parameters[0];
            TileInfo tileInfo = (TileInfo)parameters[1];
            MemoryCache<Bitmap> bitmaps = (MemoryCache<Bitmap>)parameters[2];


            byte[] bytes;
            try
            {
                bytes = tileProvider.GetTile(tileInfo);
                Bitmap bitmap = new Bitmap(new MemoryStream(bytes));
                bitmaps.Add(tileInfo.Index, bitmap);
                if (_fileCache != null && !_fileCache.Exists(tileInfo.Index))
                {
                    AddImageToFileCache(tileInfo, bitmap);
                }

                OnMapNewTileAvaliable(tileInfo, bitmap);
            }
            catch (WebException ex)
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
            catch (ThreadAbortException tex)
            {
                Console.WriteLine("TileAsyncLayer - Thread aborting: " + System.Threading.Thread.CurrentThread.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TileAsyncLayer - GetTileOnThread Exception: " + ex.ToString());
                //todo: log and use other ways to report to user.
            }
        }

        private void OnMapNewTileAvaliable(TileInfo tileInfo, Bitmap bitmap)
        {
            if (this.MapNewTileAvaliable != null)
            {
                BoundingBox bb = new BoundingBox(tileInfo.Extent.MinX, tileInfo.Extent.MinY, tileInfo.Extent.MaxX, tileInfo.Extent.MaxY);
                this.MapNewTileAvaliable(this, bb, bitmap, _source.Schema.Width, _source.Schema.Height, _imageAttributes);
            }
        }


    }


}
