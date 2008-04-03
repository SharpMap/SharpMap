using System.Drawing;
using SharpMap;

namespace WinFormSamples.Samples
{
  public static class WmsSample
  {
    public static Map InitializeMap()
    {
      //Initialize a new map of size 'imagesize'
      SharpMap.Map map = new SharpMap.Map();
      SharpMap.Layers.WmsLayer layWms = GetWmsLayer();
      //Set up the countries layer
      SharpMap.Layers.VectorLayer layCountries = new SharpMap.Layers.VectorLayer("Countries");
      //Set the datasource to a shapefile in the App_data folder
      layCountries.DataSource = new SharpMap.Data.Providers.ShapeFile("GeoData/World/countries.shp", true);
      //Set fill-style to green
      layCountries.Style.Fill = new SolidBrush(Color.Green);
      //Set the polygons to have a black outline
      layCountries.Style.Outline = System.Drawing.Pens.Yellow;
      layCountries.Style.EnableOutline = true;
      layCountries.SRID = 4326;

      //Set up a country label layer
      SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels");
      layLabel.DataSource = layCountries.DataSource;
      layLabel.Enabled = true;
      layLabel.LabelColumn = "Name";
      layLabel.Style = new SharpMap.Styles.LabelStyle();
      layLabel.Style.ForeColor = Color.White;
      layLabel.Style.Font = new Font(FontFamily.GenericSerif, 8);
      layLabel.Style.BackColor = new System.Drawing.SolidBrush(Color.FromArgb(128, 255, 0, 0));
      layLabel.MaxVisible = 90;
      layLabel.MinVisible = 30;
      layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
      layLabel.SRID = 4326;

      //Add the layers to the map object.
      //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
      map.Layers.Add(layWms);
      map.Layers.Add(layCountries);
      map.Layers.Add(layLabel);

      //limit the zoom to 360 degrees width
      map.MaximumZoom = 360;
      map.BackColor = Color.LightBlue;

      map.Zoom = 360;
      map.Center = new SharpMap.Geometries.Point(0, 0);

      return map;
    }

    private static SharpMap.Layers.WmsLayer GetWmsLayer()
    {
      string wmsUrl = "http://www2.demis.nl/mapserver/request.asp";
      SharpMap.Layers.WmsLayer layWms = new SharpMap.Layers.WmsLayer("Demis Map", wmsUrl);
      layWms.SpatialReferenceSystem = "EPSG:4326";
      layWms.AddLayer("Bathymetry");
      layWms.AddLayer("Ocean features");
      layWms.SetImageFormat(layWms.OutputFormats[0]);
      layWms.ContinueOnError = true; //Skip rendering the WMS Map if the server couldn't be requested (if set to false such an event would crash the app)
      layWms.TimeOut = 5000; //Set timeout to 5 seconds
      layWms.SRID = 4326;
      return layWms;
    }
  }
}
