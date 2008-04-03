using System;
using System.Collections.Generic;
using System.Text;
using SharpMap;
using SharpMap.Data.Providers;

namespace WinFormSamples.Samples
{
  public static class WfsGeusSample
  {
    public static Map InitializeMap()
    {
      const string getCapabilitiesURI = "http://steven:8080/eWaterWfsConnector/wfs";

      Map map = new SharpMap.Map(new System.Drawing.Size(600, 600));
      map.MinimumZoom = 0.005;
      map.BackColor = System.Drawing.Color.White;

      SharpMap.Layers.VectorLayer layer1 = new SharpMap.Layers.VectorLayer("test");
      
      WFS prov1 = new WFS(getCapabilitiesURI, "", "Well", WFS.WFSVersionEnum.WFS1_0_0);

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
