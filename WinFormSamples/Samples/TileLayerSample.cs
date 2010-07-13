using System;
using System.Collections.Generic;
using System.Text;
using BruTile.PreDefined;
using SharpMap.Layers;
using BruTile;
using BruTile.Web;
using SharpMap;
using SharpMap.Geometries;

namespace WinFormSamples.Samples
{
    class TileLayerSample
    {
        private static Int32 _num = 0;
        
        public static Map InitializeMap()
        {
            switch (_num++ % 9)
            {
                case 0:
                    return InitializeMapOsm();
                case 1:
                    return InitializeMapBing(BingMapType.Roads);
                case 2:
                    return InitializeMapBing(BingMapType.Aerial);
                case 3:
                    return InitializeMapBing(BingMapType.Hybrid);
                case 4:
                    return InitializeMapGoogle(GoogleMapType.GoogleMap);
                case 5:
                    return InitializeMapGoogle(GoogleMapType.GoogleSatellite);
                case 6:
                    return InitializeMapGoogle(GoogleMapType.GoogleSatellite | GoogleMapType.GoogleLabels);
                case 7:
                    return InitializeMapGoogle(GoogleMapType.GoogleTerrain);
                case 8:
                    _num = 0;
                    return InitializeMapGoogle(GoogleMapType.GoogleLabels);

            }
            return InitializeMapOsm();
        }

        private static Map InitializeMapOsm()
        {
            Map map = new Map();

            TileLayer tileLayer = new TileLayer(new OsmTileSource(), "TileLayer - OSM");
            map.Layers.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }

        private static Map InitializeMapBing(BingMapType mt)
        {
            Map map = new Map();

            TileLayer tileLayer = new TileLayer(new BingTileSource(BingRequest.UrlBingStaging, "", mt) , "TileLayer - Bing " + mt);
            map.Layers.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }

        private static Map InitializeMapGoogle(GoogleMapType mt)
        {
            Map map = new Map();

            GoogleRequest req;
            ITileSource tileSource;
            TileLayer tileLayer;
            if (mt == (GoogleMapType.GoogleSatellite | GoogleMapType.GoogleLabels))
            {
                req = new GoogleRequest(GoogleMapType.GoogleSatellite);
                tileSource = new GoogleTileSource(req);
                tileLayer = new TileLayer(tileSource, "TileLayer - " + GoogleMapType.GoogleSatellite);
                map.Layers.Add(tileLayer);
                req = new GoogleRequest(GoogleMapType.GoogleLabels);
                tileSource = new GoogleTileSource(req);
                mt = GoogleMapType.GoogleLabels;
            }
            else
            {
                req = new GoogleRequest(mt);
                tileSource = new GoogleTileSource(req);
            }

            tileLayer = new TileLayer(tileSource, "TileLayer - " + mt);
            map.Layers.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }

    }
}
