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
        protected FileCache _fileCache = null;
        protected ImageFormat _ImageFormat = null;
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
            : this(tileSource, layerName, new Color(), true, null)
        {
        }

        public TileLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile)
            : this(tileSource,layerName, transparentColor, showErrorInTile,null)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="tileSource">the source to get the tiles from</param>
        /// <param name="layerName">name of the layer</param>
        /// <param name="transparentColor">transparent color off</param>
        /// <param name="showErrorInTile">generate an error tile if it could not be retrieved from source</param>
        /// <param name="fileCacheDir">If the layer should use a file-cache so store tiles, set this to that directory. Set to null to avoid filecache</param>
        /// <remarks>If <see cref="showErrorInTile"/> is set to false, tile source keeps trying to get the tile in every request</remarks>
        public TileLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, string fileCacheDir)
        {
            _source = tileSource;
            LayerName = layerName;
            if (!transparentColor.IsEmpty)
                _imageAttributes.SetColorKey(transparentColor, transparentColor);
            _showErrorInTile = showErrorInTile;

#if !PocketPC
            _imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
#endif
            if (!string.IsNullOrEmpty(fileCacheDir))
            {
                _fileCache = new BruTile.Cache.FileCache(fileCacheDir, "png");
                _ImageFormat = ImageFormat.Png;
            }
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="tileSource">the source to get the tiles from</param>
        /// <param name="layerName">name of the layer</param>
        /// <param name="transparentColor">transparent color off</param>
        /// <param name="showErrorInTile">generate an error tile if it could not be retrieved from source</param>
        /// <param name="fileCache">If the layer should use a file-cache so store tiles, set this to a fileCacheProvider. Set to null to avoid filecache</param>
        /// <param name="imgFormat">Set the format of the tiles to be used</param>
        public TileLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, BruTile.Cache.FileCache fileCache, ImageFormat imgFormat)
        {
            _source = tileSource;
            LayerName = layerName;
            if (!transparentColor.IsEmpty)
                _imageAttributes.SetColorKey(transparentColor, transparentColor);
            _showErrorInTile = showErrorInTile;

#if !PocketPC
            _imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
#endif
            _fileCache = fileCache;
            _ImageFormat = imgFormat;
        }

        #endregion

        System.Collections.Hashtable _cacheTiles = new System.Collections.Hashtable();

        #region Public methods

        public override void Render(Graphics graphics, Map map)
        {
            if (!map.Size.IsEmpty && map.Size.Width > 0 && map.Size.Height > 0)
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
                    if (_fileCache != null && _fileCache.Exists(info.Index))
                    {
                        _bitmaps.Add(info.Index, GetImageFromFileCache(info) as Bitmap);
                        continue;
                    }

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

                    try
                    {
                        g.DrawImage(bitmap,
                            new Rectangle((int)min.X, (int)max.Y, (int)(max.X - min.X), (int)(min.Y - max.Y)),
                            0, 0, _source.Schema.Width, _source.Schema.Height,
                            GraphicsUnit.Pixel,
                            _imageAttributes);
                    }
                    catch (Exception ee)
                    {
                        /*GDI+ Hell*/
                    }

                }

                graphics.Transform = new Matrix();
                graphics.DrawImageUnscaled(bmp, 0, 0);
                graphics.Transform = g.Transform;

                g.Dispose();
            }
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
                if (_fileCache != null)
                {
                    AddImageToFileCache(tileInfo, bitmap);
                }
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

        protected void AddImageToFileCache(TileInfo tileInfo, Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, _ImageFormat);
            ms.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[ms.Length];
            ms.Read(data, 0, data.Length);
            ms.Dispose();
            _fileCache.Add(tileInfo.Index, data);
        }

        protected Image GetImageFromFileCache(TileInfo info)
        {
            MemoryStream ms = new MemoryStream(_fileCache.Find(info.Index));
            Image img = Image.FromStream(ms);
            ms.Dispose();
            return img;
        }
        #endregion
    }
}
