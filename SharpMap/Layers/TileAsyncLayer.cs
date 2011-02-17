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
        public TileAsyncLayer(ITileSource tileSource, string layerName)
            : base(tileSource, layerName, new Color(), true)
        {
        }

        public TileAsyncLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile)
            : base(tileSource, layerName, transparentColor, showErrorInTile)
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

            foreach (TileInfo info in tiles)
            {
                if (_bitmaps.Find(info.Index) != null)
                {
                    //OnMapNewTileAvaliable(info, _bitmaps.Find(info.Index));
                    ThreadPool.QueueUserWorkItem(OnMapNewtileAvailableHelper, new object[]{info,_bitmaps.Find(info.Index)});
                    
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(GetTileOnThread, new object[] { _source.Provider, info, _bitmaps });
                }
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
                    bitmaps.Add(tileInfo.Index, bitmap);
                }
            }
            catch (Exception ex)
            {
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
