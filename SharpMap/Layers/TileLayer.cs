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
using Common.Logging;
using GeoAPI.Geometries;

namespace SharpMap.Layers
{
    //ReSharper disable InconsistentNaming
    ///<summary>
    /// Tile layer class
    ///</summary>
    [Serializable]
    public class TileLayer : Layer, System.Runtime.Serialization.IDeserializationCallback
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
        
        #region Fields
        
        //string layerName;
        /// <summary>
        /// The <see cref="ImageAttributes"/> used when rendering the tiles
        /// </summary>
        //protected readonly ImageAttributes _imageAttributes = new ImageAttributes();

        /// <summary>
        /// The tile source for this layer
        /// </summary>
        protected readonly ITileSource _source;

        /// <summary>
        /// An in-memory tile cache
        /// </summary>
        [NonSerialized]
        protected MemoryCache<Bitmap> _bitmaps = new MemoryCache<Bitmap>(100, 200);

        /// <summary>
        /// A file cache
        /// </summary>
        protected FileCache _fileCache = null;
        
        /// <summary>
        /// The format of the images
        /// </summary>
        protected ImageFormat _ImageFormat = null;
        
        /// <summary>
        /// Value indicating if "error" tiles should be generated or not.
        /// </summary>
        protected readonly bool _showErrorInTile = true;

        InterpolationMode _interpolationMode = InterpolationMode.HighQualityBicubic;
        protected Color _transparentColor;
        //System.Collections.Hashtable _cacheTiles = new System.Collections.Hashtable();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the boundingbox of the entire layer
        /// </summary>
        public override Envelope Envelope
        {
            get
            {
                var extent = _source.Schema.Extent;
                return new Envelope(
                    extent.MinX, extent.MaxX, 
                    extent.MinY, extent.MaxY);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileSource">The source to get the tiles from</param>
        /// <param name="layerName">The name of the layer</param>
        /// <param name="transparentColor">The color to be treated as transparent color</param>
        /// <param name="showErrorInTile">Flag indicating that an error tile should be generated for <see cref="WebException"/>s</param>
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
        /// <remarks>If <paramref name="showErrorInTile"/> is set to false, tile source keeps trying to get the tile in every request</remarks>
        public TileLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, string fileCacheDir)
        {
            _source = tileSource;
            LayerName = layerName;
            _transparentColor = transparentColor;
            _showErrorInTile = showErrorInTile;

            if (!string.IsNullOrEmpty(fileCacheDir))
            {
                _fileCache = new FileCache(fileCacheDir, "png");
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
        public TileLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, FileCache fileCache, ImageFormat imgFormat)
        {
            _source = tileSource;
            LayerName = layerName;
            _transparentColor = transparentColor;
            _showErrorInTile = showErrorInTile;

            _fileCache = fileCache;
            _ImageFormat = imgFormat;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="graphics">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics graphics, Map map)
        {
            if (!map.Size.IsEmpty && map.Size.Width > 0 && map.Size.Height > 0)
            {
                var bmp = new Bitmap(map.Size.Width, map.Size.Height, PixelFormat.Format32bppArgb);
                
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode;
                    g.Transform = graphics.Transform.Clone();

                    var extent = new Extent(map.Envelope.MinX, map.Envelope.MinY, 
                                            map.Envelope.MaxX, map.Envelope.MaxY);
                    int level = BruTile.Utilities.GetNearestLevel(_source.Schema.Resolutions, map.PixelSize);
                    var tiles = new List<TileInfo>(_source.Schema.GetTilesInView(extent, level));

                    IList<WaitHandle> waitHandles = new List<WaitHandle>();
                    Dictionary<TileIndex, Bitmap> toRender = new Dictionary<TileIndex, Bitmap>();
                    Dictionary<TileIndex, bool> takenFromCache = new Dictionary<TileIndex,bool>();
                    foreach (TileInfo info in tiles)
                    {
                        var image = _bitmaps.Find(info.Index);
                        if (image != null)
                        {
                            toRender.Add(info.Index, image);
                            takenFromCache.Add(info.Index,true);
                            continue;
                        }
                        if (_fileCache != null && _fileCache.Exists(info.Index))
                        {
                            _bitmaps.Add(info.Index, GetImageFromFileCache(info) as Bitmap);
                            toRender.Add(info.Index, _bitmaps.Find(info.Index));
                            takenFromCache.Add(info.Index,true);
                            continue;
                        }

                        var waitHandle = new AutoResetEvent(false);
                        waitHandles.Add(waitHandle);
                        ThreadPool.QueueUserWorkItem(GetTileOnThread,
                                                     new object[] { _source.Provider, info, toRender, waitHandle, true });
                    }

                    foreach (var handle in waitHandles)
                        handle.WaitOne();

                    using (var ia = new ImageAttributes())
                    {
                        if (!_transparentColor.IsEmpty)
                            ia.SetColorKey(_transparentColor, _transparentColor);
#if !PocketPC
                        ia.SetWrapMode(WrapMode.TileFlipXY);
#endif

                        foreach (var info in tiles)
                        {
                            if (!toRender.ContainsKey(info.Index))
                                continue;

                            var bitmap = toRender[info.Index];//_bitmaps.Find(info.Index);
                            if (bitmap == null) continue;

                            var min = map.WorldToImage(new Coordinate(info.Extent.MinX, info.Extent.MinY));
                            var max = map.WorldToImage(new Coordinate(info.Extent.MaxX, info.Extent.MaxY));

                            min = new PointF((float) Math.Round(min.X), (float) Math.Round(min.Y));
                            max = new PointF((float) Math.Round(max.X), (float) Math.Round(max.Y));

                            try
                            {
                                g.DrawImage(bitmap,
                                            new Rectangle((int) min.X, (int) max.Y, (int) (max.X - min.X),
                                                          (int) (min.Y - max.Y)),
                                            0, 0, _source.Schema.Width, _source.Schema.Height,
                                            GraphicsUnit.Pixel,
                                            ia);
                            }
                            catch (Exception ee)
                            {
                                Logger.Error(ee.Message);
                            }

                        }
                    }

                    //Add rendered tiles to cache
                    foreach (var kvp in toRender)
                    {
                        if (takenFromCache.ContainsKey(kvp.Key) && !takenFromCache[kvp.Key])
                        {
                            _bitmaps.Add(kvp.Key, kvp.Value);
                        }
                    }

                    graphics.Transform = new Matrix();
                    graphics.DrawImageUnscaled(bmp, 0, 0);
                    graphics.Transform = g.Transform;
                }
            }
        }

        #endregion

        #region Private methods

        private void GetTileOnThread(object parameter)
        {
            object[] parameters = (object[])parameter;
            if (parameters.Length != 5) throw new ArgumentException("Five parameters expected");
            ITileProvider tileProvider = (ITileProvider)parameters[0];
            TileInfo tileInfo = (TileInfo)parameters[1];
            //MemoryCache<Bitmap> bitmaps = (MemoryCache<Bitmap>)parameters[2];
            Dictionary<TileIndex, Bitmap> bitmaps = (Dictionary<TileIndex,Bitmap>)parameters[2];
            AutoResetEvent autoResetEvent = (AutoResetEvent)parameters[3];
            bool retry = (bool) parameters[4];

            var setEvent = true;
            try
            {
                byte[] bytes = tileProvider.GetTile(tileInfo);
                Bitmap bitmap = new Bitmap(new MemoryStream(bytes));
                bitmaps.Add(tileInfo.Index, bitmap);
                if (_fileCache != null)
                {
                    AddImageToFileCache(tileInfo, bitmap);
                }
            }
            catch (WebException ex)
            {
                if (retry)
                {
                    GetTileOnThread(new object[] { tileProvider, tileInfo, bitmaps, autoResetEvent, false });
                    setEvent = false;
                    return;
                }
                if (_showErrorInTile)
                {
                    //an issue with this method is that one an error tile is in the memory cache it will stay even
                    //if the error is resolved. PDD.
                    var bitmap = new Bitmap(_source.Schema.Width, _source.Schema.Height);
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.DrawString(ex.Message, new Font(FontFamily.GenericSansSerif, 12),
                                            new SolidBrush(Color.Black),
                                            new RectangleF(0, 0, _source.Schema.Width, _source.Schema.Height));
                    }
                    bitmaps.Add(tileInfo.Index, bitmap);
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                if (setEvent) autoResetEvent.Set();
            }
        }

        /// <summary>
        /// Method to add a tile image to the <see cref="FileCache"/>
        /// </summary>
        /// <param name="tileInfo">The tile info</param>
        /// <param name="bitmap">The tile image</param>
        protected void AddImageToFileCache(TileInfo tileInfo, Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, _ImageFormat);
                ms.Seek(0, SeekOrigin.Begin);
                var data = new byte[ms.Length];
                ms.Read(data, 0, data.Length);
                _fileCache.Add(tileInfo.Index, data);
            }
        }

        /// <summary>
        /// Function to get a tile image from the <see cref="FileCache"/>.
        /// </summary>
        /// <param name="info">The tile info</param>
        /// <returns>The tile-image, if already cached</returns>
        protected Image GetImageFromFileCache(TileInfo info)
        {
            using (var ms = new MemoryStream(_fileCache.Find(info.Index)))
            {
                return Image.FromStream(ms);
            }
        }
        #endregion

        public void OnDeserialization(object sender)
        {
            if (_bitmaps == null)
                _bitmaps = new MemoryCache<Bitmap>(100, 200);
        }

        protected override void ReleaseManagedResources()
        {
            base.ReleaseManagedResources();

            if (_source is IDisposable)
            {
                (_source as IDisposable).Dispose();
            }

        }
    }
}
