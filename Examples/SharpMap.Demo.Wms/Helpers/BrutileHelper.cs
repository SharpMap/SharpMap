using System.Drawing;
using BruTile.Web;
using SharpMap.Layers;

namespace SharpMap.Demo.Wms.Helpers
{
    public static class BruTileHelper
    {
        public static Map Osm()
        {
            ILayer layer = new TileLayer(new OsmTileSource(), "BruTile-OSM") { SRID = 900913 };         
            Map map = new Map(new Size(1, 1));            
            map.Layers.Add(layer);            
            return map;
        }
    }
}