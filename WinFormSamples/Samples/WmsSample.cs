using System.Drawing;
using SharpMap;

namespace WinFormSamples.Samples
{
  public static class WmsSample
  {
    public static Map InitializeMap()
    {
      string wmsUrl = "http://www2.demis.nl/mapserver/request.asp";
            
      SharpMap.Map map = new SharpMap.Map();

      SharpMap.Layers.WmsLayer layWms = new SharpMap.Layers.WmsLayer("Demis Map", wmsUrl);
      layWms.SpatialReferenceSystem = "EPSG:4326";

      layWms.AddLayer("Bathymetry");
      layWms.AddLayer("Topography");
      layWms.AddLayer("Hillshading");
      
      layWms.SetImageFormat(layWms.OutputFormats[0]);
      layWms.ContinueOnError = true; //Skip rendering the WMS Map if the server couldn't be requested (if set to false such an event would crash the app)
      layWms.TimeOut = 5000; //Set timeout to 5 seconds
      layWms.SRID = 4326;
      map.Layers.Add(layWms);
      
      //limit the zoom to 360 degrees width
      map.MaximumZoom = 360;
      map.BackColor = Color.LightBlue;

      map.Zoom = 360;
      map.Center = new SharpMap.Geometries.Point(0, 0);

      return map;
    }
  }
}
