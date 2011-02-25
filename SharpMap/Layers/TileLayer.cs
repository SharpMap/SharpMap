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

    ///<summary>
    ///</summary>
    public class TileLayer : Layer
    {
        #region Fields
        
        //string layerName;
        protected readonly ImageAttributes _imageAttributes = new ImageAttributes();
        protected readonly ITileSource _source;
        protected readonly MemoryCache<Bitmap> _bitmaps = new MemoryCache<Bitmap>(100, 200);
        protected readonly bool _showErrorInTile = true;
        InterpolationMode _interpolationMode = InterpolationMode.HighQualityBicubic;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the boundingbox of the entire layer
        /// </summary>
        public override BoundingBox Envelope
        {
            get 
            {
                return new BoundingBox(
                    _source.Schema.Extent.MinX, 
                    _source.Schema.Extent.MinY, 
                    _source.Schema.Extent.MaxX, 
                    _source.Schema.Extent.MaxY);
            }
        }

        /// <summary>
        /// The algorithm used when images are scaled or rotated 
        /// </summary>
        public InterpolationMode InterpolationMode
        {
            get { return _interpolationMode; }
            set { _interpolationMode = value; }
        }

        #endregion

        #region Constructors 

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="tileSource">the source to get the tiles from</param>
        /// <param name="layerName">name of the layer</param>
        public TileLayer(ITileSource tileSource, string layerName)
            : this(tileSource, layerName, new Color(), true)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="tileSource">the source to get the tiles from</param>
        /// <param name="layerName">name of the layer</param>
        /// <param name="transparentColor">transparent color off</param>
        /// <param name="showErrorInTile">generate an error tile if it could not be retrieved from source</param>
        /// <remarks>If <see cref="showErrorInTile"/> is set to false, tile source keeps trying to get the tile in every request</remarks>
        public TileLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile)
        {
            _source = tileSource;
            LayerName = layerName;
            if (!transparentColor.IsEmpty)
                _imageAttributes.SetColorKey(transparentColor, transparentColor);
            _showErrorInTile = showErrorInTile;

#if !PocketPC
            _imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
#endif
        }

        #endregion

        System.Collections.Hashtable _cacheTiles = new System.Collections.Hashtable();

        #region Public methods

        public override void Render(Graphics graphics, Map map)
        {
            Bitmap bmp = new Bitmap(map.Size.Width, map.Size.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);

            g.InterpolationMode = InterpolationMode;
            g.Transform = graphics.Transform.Clone();

            Extent extent = new Extent(map.Envelope.Min.X, map.Envelope.Min.Y, map.Envelope.Max.X, map.Envelope.Max.Y);
            int level = BruTile.Utilities.GetNearestLevel(_source.Schema.Resolutions, map.PixelSize);
            IList<TileInfo> tiles = _source.Schema.GetTilesInView(extent, level);
            
            IList<WaitHandle> waitHandles = new List<WaitHandle>();

            foreach (TileInfo info in tiles)
            {
                if (_bitmaps.Find(info.Index) != null) continue;
                AutoResetEvent waitHandle = new AutoResetEvent(false);
                waitHandles.Add(waitHandle);
                ThreadPool.QueueUserWorkItem(GetTileOnThread, new object[] { _source.Provider, info, _bitmaps, waitHandle });
            }

            foreach (WaitHandle handle in waitHandles)
                handle.WaitOne();

            foreach (TileInfo info in tiles)
            {
                Bitmap bitmap = _bitmaps.Find(info.Index);
                if (bitmap == null) continue;

                PointF min = map.WorldToImage(new Geometries.Point(info.Extent.MinX, info.Extent.MinY));
                PointF max = map.WorldToImage(new Geometries.Point(info.Extent.MaxX, info.Extent.MaxY));

                min = new PointF((float)Math.Round(min.X), (float)Math.Round(min.Y));
                max = new PointF((float)Math.Round(max.X), (float)Math.Round(max.Y));

                g.DrawImage(bitmap,
                    new Rectangle((int)min.X, (int)max.Y, (int)(max.X - min.X), (int)(min.Y - max.Y)),
                    0, 0, _source.Schema.Width, _source.Schema.Height,
                    GraphicsUnit.Pixel,
                    _imageAttributes);

            }

            graphics.Transform = new Matrix();
            graphics.DrawImageUnscaled(bmp, 0, 0);
            graphics.Transform = g.Transform;

            g.Dispose();
        }

        #endregion

        #region Private methods

        private void GetTileOnThread(object parameter)
        {
            object[] parameters = (object[])parameter;
            if (parameters.Length != 4) throw new ArgumentException("Three parameters expected");
            ITileProvider tileProvider = (ITileProvider)parameters[0];
            TileInfo tileInfo = (TileInfo)parameters[1];
            MemoryCache<Bitmap> bitmaps = (MemoryCache<Bitmap>)parameters[2];
            AutoResetEvent autoResetEvent = (AutoResetEvent)parameters[3];
            

            byte[] bytes;
            try
            {
                bytes = tileProvider.GetTile(tileInfo);
                Bitmap bitmap = new Bitmap(new MemoryStream(bytes));
                bitmaps.Add(tileInfo.Index, bitmap);
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
                    bitmaps.Add(tileInfo.Index, bitmap);
                }
            }
            catch(Exception ex)
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
