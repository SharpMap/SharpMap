using System;
using System.Collections.Generic;
using System.Text;
using SharpMap;
using SharpMap.Layers;

namespace WinFormSamples.Samples
{
  class TiledWmsSample
  {
    public static Map InitializeMap()
    {
      Map map = new Map();
      
      string url = "http://labs.metacarta.com/wms-c/tilecache.py?version=1.1.1&amp;request=GetCapabilities&amp;service=wms-c";
      TiledWmsLayer tiledWmsLayer = new TiledWmsLayer("Metacarta", url);
      tiledWmsLayer.TileSetsActive.Add(tiledWmsLayer.TileSets["basic"].Name);
      map.Layers.Add(tiledWmsLayer);
      map.ZoomToBox(new SharpMap.Geometries.BoundingBox(-180.0, -90.0, 180.0, 90.0));

      return map;
    }
  }
}
