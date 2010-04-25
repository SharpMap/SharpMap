using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Layers;
using BruTile;
using BruTile.Web;
using SharpMap;
using SharpMap.Geometries;

namespace WinFormSamples.Samples
{
    class TileLayerSample
    {
        public static Map InitializeMap()
        {
            Map map = new Map();
            
            TileLayer tileLayer = new TileLayer(new OsmTileSource(), "OSM");
            map.Layers.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }
    }
}
