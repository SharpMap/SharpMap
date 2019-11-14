// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reactive.Linq;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;

namespace UnitTests
{
    public static class Extenders
    {
        #region MapNewTileAvailableEventArgs class
        public class MapNewTileAvailableEventArgs : EventArgs
        {
            public MapNewTileAvailableEventArgs(TileLayer sender, Envelope bbox, Bitmap bm, int sourceWidth,
                int sourceHeight, ImageAttributes imageAttributes)
            {
                Sender = sender;
                BoundingBox = bbox;
                Bitmap = bm;
                SourceWidth = sourceWidth;
                SourceHeight = sourceHeight;
                ImageAttributes = imageAttributes;
            }

            public TileLayer Sender { get; private set; }

            public Envelope BoundingBox { get; private set; }

            public Bitmap Bitmap { get; private set; }

            public int SourceWidth { get; private set; }

            public int SourceHeight { get; private set; }

            public ImageAttributes ImageAttributes { get; private set; }
        } 
        #endregion

        #region DownloadProgressEventArgs class
        public class DownloadProgressEventArgs : EventArgs
        {
            public DownloadProgressEventArgs(int tilesRemaining)
            {
                TilesRemaining = tilesRemaining;
            }

            public int TilesRemaining { get; private set; }
        } 
        #endregion

        #region Map extenders

        public static LayerCollection GetCollection(this Map map, LayerCollectionType collectionType)
        {
            switch (collectionType)
            {
                case LayerCollectionType.Background:
                    return map.BackgroundLayer;
                    
                case LayerCollectionType.Static:
                    return map.Layers;
                
                case LayerCollectionType.Variable:
                    return  map.VariableLayers;
                default:
                    throw new Exception();
            }
        }
        
        public static IObservable<MapNewTileAvailableEventArgs> GetMapNewTileAvailableAsObservable(this Map map)
        {
            var listener = new EventListener(map, "MapNewTileAvaliable");
            return listener.SavedArgs.ToObservable()
                .Select(dict => new MapNewTileAvailableEventArgs(
                    (TileLayer) dict["sender"],
                    (Envelope) dict["bbox"],
                    (Bitmap) dict["bm"],
                    (int) dict["sourceWidth"],
                    (int) dict["sourceHeight"],
                    (ImageAttributes) dict["imageAttributes"]));
        }

        #endregion

        public static IObservable<MapNewTileAvailableEventArgs> GetMapNewTileAvailableAsObservable(
            this ITileAsyncLayer layer)
        {
            var listener = new EventListener(layer, "MapNewTileAvaliable");
            return listener.SavedArgs.ToObservable()
                .Select(dict => new MapNewTileAvailableEventArgs(
                    (TileLayer)dict["sender"],
                    (Envelope)dict["bbox"],
                    (Bitmap)dict["bm"],
                    (int)dict["sourceWidth"],
                    (int)dict["sourceHeight"],
                    (ImageAttributes)dict["imageAttributes"]));
        }

        public static IObservable<DownloadProgressEventArgs> GetDownloadProgressAsObservable(this ITileAsyncLayer layer)
        {
            var listener = new EventListener(layer, "DownloadProgressChanged");

            return listener.SavedArgs.ToObservable()
                .Select(dict => new DownloadProgressEventArgs((int)dict["tilesRemaining"]));
        }

        public static string GetDescription(this TestContext.TestAdapter adapter)
        {
            return (string)adapter.Properties["_DESCRIPTION"].FirstOrDefault();
        }
    }
}
