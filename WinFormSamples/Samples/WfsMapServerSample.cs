using System;
using System.Collections.Generic;
using System.Text;
using SharpMap;
using System.Drawing;
using SharpMap.Data.Providers;

namespace WinFormSamples.Samples
{
  static class WfsMapServerSample
  {
    public static Map CreateMap()
    {
      const string getCapabilitiesURI = "http://www2.dmsolutions.ca/cgi-bin/mswfs_gmap?request=GetCapabilities&service=WFS&VERSION=1.0.0";

      Map demoMap = new SharpMap.Map(new Size(600, 600));
      demoMap.MinimumZoom = 0.005;
      demoMap.BackColor = Color.White;

      SharpMap.Layers.VectorLayer layer1 = new SharpMap.Layers.VectorLayer("prov_land");
      WFS prov1 = new WFS(getCapabilitiesURI, "", "prov_land", WFS.WFSVersionEnum.WFS1_1_0);
      layer1.Style.Fill = new SolidBrush(Color.IndianRed);    // States
      //// Options 
      // Defaults: MultiGeometries: true, QuickGeometries: false, GetFeatureGETRequest: false
      // Render with validation...
      prov1.QuickGeometries = false;
      // Important when connecting to an UMN MapServer
      //!!!prov1.GetFeatureGETRequest = true;
      // Ignore multi-geometries...
      prov1.MultiGeometries = false;
      layer1.DataSource = prov1;
      demoMap.Layers.Add(layer1);

      demoMap.ZoomToBox(new SharpMap.Geometries.BoundingBox(-3546709.473205, -869773.355764, 4444893.074820, 3923457.184829));

      return demoMap;
    }

  }
}
