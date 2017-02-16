using BruTile.Wmsc;
using SharpMap;
using SharpMap.Layers;
using BruTile.Web;
using BruTile;
using System.Collections.Generic;

namespace WinFormSamples.Samples
{
    internal class TiledWmsSample
    {
        public static Map InitializeMap()
        {
            Map map = new Map();


            //string url = "http://labs.metacarta.com/wms-c/tilecache.py?version=1.1.1&amp;request=GetCapabilities&amp;service=wms-c";
            string url = "http://resource.sgu.se/service/wms/130/brunnar?SERVICE=WMS&VERSION=1.3&REQUEST=getcapabilities&TILED=true";
            //string url = "http://dev:8080/geoserver/gwc/service/wms?SERVICE=WMS&VERSION=1.1.1&REQUEST=getcapabilities&TILED=true";
            //string url = "http://dev:8080/geoserver/ows?service=wms&version=1.3.0&request=GetCapabilities&tiled=true";
            //TiledWmsLayer tiledWmsLayer = new TiledWmsLayer("Metacarta", url);
            //tiledWmsLayer.TileSetsActive.Add(tiledWmsLayer.TileSets["avalon"].Name);
            //map.Layers.Add(tiledWmsLayer);
            //map.ZoomToBox(new BoundingBox(-180.0, -90.0, 180.0, 90.0));

            //WmscRequest req;
            //ITileSource tileSource;
            TileAsyncLayer tileLayer;
            //BruTile.Web.TmsTileSource source2 = new TmsTileSource(url);

            var source = new List<ITileSource>(WmscTileSource.CreateFromWmscCapabilties(new System.Uri(url)));

//            foreach (ITileSource src in source)
//            {
                tileLayer = new TileAsyncLayer(source[16], "tileLayer" + source[16]);
                tileLayer.MapNewTileAvaliable += map.MapNewTileAvaliableHandler;
                map.BackgroundLayer.Add(tileLayer);
//            }
            map.ZoomToExtents();


            return map;
        }
    }
}