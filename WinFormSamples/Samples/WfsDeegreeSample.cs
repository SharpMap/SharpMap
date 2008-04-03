using System;
using System.Collections.Generic;
using System.Text;
using SharpMap;
using System.Drawing;
using SharpMap.Data.Providers;

namespace WinFormSamples.Samples
{
  static class WfsDeegreeSample
  {
    public static Map CreateMap()
    {
      const string getCapabilitiesURI = "http://192.168.25.120:8080/deegree-wfs/services";

      Map map = new SharpMap.Map(new Size(600, 600));
      map.MinimumZoom = 0.005;
      map.BackColor = Color.White;

      SharpMap.Layers.VectorLayer layer1 = new SharpMap.Layers.VectorLayer("Springs");

      WFS prov1 = new WFS(getCapabilitiesURI, "app1", "Springs", WFS.WFSVersionEnum.WFS1_1_0);

      prov1.QuickGeometries = false;
      // Important when connecting to an UMN MapServer
      prov1.GetFeatureGETRequest = true;
      // Ignore multi-geometries...
      prov1.MultiGeometries = false;

      layer1.DataSource = prov1;
      map.Layers.Add(layer1);

      map.ZoomToBox(new SharpMap.Geometries.BoundingBox(574290, 4275299, 702273, 4386402));
      
      return map;
    }
  }
}
