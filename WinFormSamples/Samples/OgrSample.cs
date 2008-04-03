using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpMap;

namespace WinFormSamples.Samples
{
  public static class OgrSample
  {
    public static SharpMap.Map InitializeMap()
    {
      //Initialize a new map of size 'imagesize'
      SharpMap.Map map = new Map();

      //Set up the countries layer
      SharpMap.Layers.VectorLayer layCountries = new SharpMap.Layers.VectorLayer("Countries");
      //Set the datasource to a shapefile in the App_data folder
      try
      {
        layCountries.DataSource = new SharpMap.Data.Providers.Ogr("GeoData/MapInfo/countriesMapInfo.tab");
      }
      catch (Exception ex)
      {
        if (ex.GetType() == typeof(TypeInitializationException))
          throw new Exception(String.Format("The application threw a PINVOKE exception. You probably need to copy the unmanaged dll's to your bin directory. They are a part of fwtools {0}. You can download it from: http://home.gdal.org/fwtools/", SharpMap.Data.Providers.Ogr.FWToolsVersion));
      }

      //Set fill-style to green
      layCountries.Style.Fill = new SolidBrush(Color.Green);
      //Set the polygons to have a black outline
      layCountries.Style.Outline = System.Drawing.Pens.Black;
      layCountries.Style.EnableOutline = true;
      layCountries.SRID = 4326;

      //Set up a river layer
      SharpMap.Layers.VectorLayer layRivers = new SharpMap.Layers.VectorLayer("Rivers");
      //Set the datasource to a shapefile in the App_data folder
      layRivers.DataSource = new SharpMap.Data.Providers.Ogr("GeoData/MapInfo/riversMapInfo.tab");
      //Define a blue 1px wide pen
      layRivers.Style.Line = new Pen(Color.Blue, 1);
      layRivers.SRID = 4326;

      //Set up a river layer
      SharpMap.Layers.VectorLayer layCities = new SharpMap.Layers.VectorLayer("Cities");
      //Set the datasource to a shapefile in the App_data folder
      layCities.DataSource = new SharpMap.Data.Providers.Ogr("GeoData/MapInfo/citiesMapInfo.tab");
      layCities.Style.SymbolScale = 0.8f;
      layCities.MaxVisible = 40;
      layCities.SRID = 4326;

      //Set up a country label layer
      SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels");
      layLabel.DataSource = layCountries.DataSource;
      layLabel.Enabled = true;
      layLabel.LabelColumn = "Name";
      layLabel.Style = new SharpMap.Styles.LabelStyle();
      layLabel.Style.ForeColor = Color.White;
      layLabel.Style.Font = new Font(FontFamily.GenericSerif, 12);
      layLabel.Style.BackColor = new System.Drawing.SolidBrush(Color.FromArgb(128, 255, 0, 0));
      layLabel.MaxVisible = 90;
      layLabel.MinVisible = 30;
      layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
      layLabel.SRID = 4326;
      layLabel.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

      //Set up a city label layer
      SharpMap.Layers.LabelLayer layCityLabel = new SharpMap.Layers.LabelLayer("City labels");
      layCityLabel.DataSource = layCities.DataSource;
      layCityLabel.Enabled = true;
      layCityLabel.LabelColumn = "Name";
      layCityLabel.Style = new SharpMap.Styles.LabelStyle();
      layCityLabel.Style.ForeColor = Color.Black;
      layCityLabel.Style.Font = new Font(FontFamily.GenericSerif, 11);
      layCityLabel.MaxVisible = layLabel.MinVisible;
      layCityLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left;
      layCityLabel.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Bottom;
      layCityLabel.Style.Offset = new PointF(3, 3);
      layCityLabel.Style.Halo = new Pen(Color.Yellow, 2);
      layCityLabel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
      layCityLabel.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
      layCityLabel.SRID = 4326;
      layCityLabel.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
      layCityLabel.Style.CollisionDetection = true;

      //Add the layers to the map object.
      //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
      map.Layers.Add(layCountries);
      map.Layers.Add(layRivers);
      map.Layers.Add(layCities);
      map.Layers.Add(layLabel);
      map.Layers.Add(layCityLabel);

      //limit the zoom to 360 degrees width
      map.MaximumZoom = 360;
      map.BackColor = Color.LightBlue;

      map.ZoomToExtents(); // = 360;
      map.Center = new SharpMap.Geometries.Point(0, 0);

      return map;
    }
  }
}
