using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using BruTile;
using BruTile.Cache;
using SharpMap.Geometries;

namespace SharpMap.Layers
{
    public class TileLayer : ILayer
    {
        #region Fields
        
        double minVisible = Double.MinValue;
        double maxVisible = Double.MaxValue;
        bool enabled = true;
        string layerName;
        int srid;
        ImageAttributes imageAttributes = new ImageAttributes();
        ITileSource source;
        MemoryCache<Bitmap> bitmaps = new MemoryCache<Bitmap>(100, 200);
        bool showErrorInTile = true;

        #endregion

        #region Properties

        public double MinVisible
        {
            get { return minVisible; }
            set { minVisible = value; }
        }

        public double MaxVisible
        {
            get { return maxVisible; }
            set { maxVisible = value; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public string LayerName
        {
            get { return layerName; }
            set { layerName = value; }
        }

        public BoundingBox Envelope
        {
            get 
            {
                return new BoundingBox(
                    source.Schema.Extent.MinX, 
                    source.Schema.Extent.MinY, 
                    source.Schema.Extent.MaxX, 
                    source.Schema.Extent.MaxY);
            }
        }

        public int SRID
        {
            get { return srid; }
            set { srid = value; }
        }

        #endregion

        #region Constructors 

        public TileLayer(ITileSource tileSource, string layerName)
            : this(tileSource, layerName, new Color(), true)
        {
        }

        public TileLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile)
        {
            this.source = tileSource;
            this.layerName = layerName;
            if (!transparentColor.IsEmpty)
                imageAttributes.SetColorKey(transparentColor, transparentColor);
            this.showErrorInTile = showErrorInTile;

#if !PocketPC
            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
#endif
        }

        #endregion

        #region Public methods

        public void Render(Graphics graphics, Map map)
        {
            Extent extent = new Extent(map.Envelope.Min.X, map.Envelope.Min.Y, map.Envelope.Max.X, map.Envelope.Max.Y);
            int level = BruTile.Utilities.GetNearestLevel(source.Schema.Resolutions, map.PixelSize);
            IList<TileInfo> tiles = source.Schema.GetTilesInView(extent, level);
            
            IList<WaitHandle> waitHandles = new List<WaitHandle>();

            foreach (TileInfo info in tiles)
            {
                if (bitmaps.Find(info.Key) != null) continue;
                AutoResetEvent waitHandle = new AutoResetEvent(false);
                waitHandles.Add(waitHandle);
                ThreadPool.QueueUserWorkItem(GetTileOnThread, new object[] { source.Provider, info, bitmaps, waitHandle });
            }

            foreach (WaitHandle handle in waitHandles)
                handle.WaitOne();

            foreach (TileInfo info in tiles)
            {
                Bitmap bitmap = bitmaps.Find(info.Key);
                if (bitmap == null) continue;

                PointF min = map.WorldToImage(new SharpMap.Geometries.Point(info.Extent.MinX, info.Extent.MinY));
                PointF max = map.WorldToImage(new SharpMap.Geometries.Point(info.Extent.MaxX, info.Extent.MaxY));

                min = new PointF((float)Math.Round(min.X), (float)Math.Round(min.Y));
                max = new PointF((float)Math.Round(max.X), (float)Math.Round(max.Y));

                graphics.DrawImage(bitmap,
                    new Rectangle((int)min.X, (int)max.Y, (int)(max.X - min.X), (int)(min.Y - max.Y)),
                    0, 0, source.Schema.Width, source.Schema.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }
        }

#endregion

        #region Private methods

        private void GetTileOnThread(object parameter)
        {
            object[] parameters = (object[])parameter;
            if (parameters.Length != 4) throw new ArgumentException("Four parameters expected");
            ITileProvider tileProvider = (ITileProvider)parameters[0];
            TileInfo tileInfo = (TileInfo)parameters[1];
            MemoryCache<Bitmap> bitmaps = (MemoryCache<Bitmap>)parameters[2];
            AutoResetEvent autoResetEvent = (AutoResetEvent)parameters[3];

            byte[] bytes;
            try
            {
                bytes = tileProvider.GetTile(tileInfo);
                Bitmap bitmap = new Bitmap(new MemoryStream(bytes));
                bitmaps.Add(tileInfo.Key, bitmap);
            }
            catch (WebException ex)
            {
                if (showErrorInTile)
                {
                    //an issue with this method is that one an error tile is in the memory cache it will stay even
                    //if the error is resolved. PDD.
                    Bitmap bitmap = new Bitmap(this.source.Schema.Width, this.source.Schema.Height);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.DrawString(ex.Message, new Font(FontFamily.GenericSansSerif, 12), new SolidBrush(Color.Black),
                        new RectangleF(0, 0, this.source.Schema.Width, this.source.Schema.Height));
                    bitmaps.Add(tileInfo.Key, bitmap);
                }
            }
            catch (Exception ex)
            {
                //todo: log and use other ways to report to user.
            }
            finally
            {
                autoResetEvent.Set();
            }
        }

        #endregion
    }
}
