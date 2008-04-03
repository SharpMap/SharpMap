using System;
using System.Collections.Generic;
using System.Text;
using SharpMap;
using System.IO;
using System.Drawing;

namespace WinFormSamples.Samples
{
  public static class GdalSample
  {
    public static Map InitializeMap()
    {
      try
      {
        //Sample provided by Dan Brecht and Joel Wilson
        Map map = new Map();
        map.BackColor = Color.White;

        string relativePath = "GeoData/GeoTiff/";

        SharpMap.Layers.GdalRasterLayer layer;

        if (!File.Exists(relativePath + "format01-image_a.tif"))
        {
          throw new Exception("Make sure the data is in the relative directory: " + relativePath);
        }

        layer = new SharpMap.Layers.GdalRasterLayer("GeoTiffA", relativePath + "format01-image_a.tif");
        map.Layers.Add(layer);
        layer = new SharpMap.Layers.GdalRasterLayer("GeoTiffB", relativePath + "format01-image_b.tif");
        map.Layers.Add(layer);
        layer = new SharpMap.Layers.GdalRasterLayer("GeoTiffC", relativePath + "format01-image_c.tif");
        map.Layers.Add(layer);
        layer = new SharpMap.Layers.GdalRasterLayer("GeoTiffD", relativePath + "format01-image_d.tif");
        map.Layers.Add(layer);

        SharpMap.Layers.VectorLayer shapeLayer;

        if (!File.Exists(relativePath + "outline.shp"))
        {
          throw new Exception("Make sure the data is in the relative directory: " + relativePath);
        }

        shapeLayer = new SharpMap.Layers.VectorLayer("outline", new SharpMap.Data.Providers.ShapeFile(relativePath + "outline.shp"));
        shapeLayer.Style.Fill = Brushes.Transparent;
        shapeLayer.Style.Outline = Pens.Black;
        shapeLayer.Style.EnableOutline = true;
        shapeLayer.Style.Enabled = true;
        map.Layers.Add(shapeLayer);

        map.ZoomToExtents();

        return map;
      }
      catch (Exception ex)
      {
        if (ex.Message == "The type initializer for 'OSGeo.GDAL.GdalPINVOKE' threw an exception.")
        {
          throw new Exception(String.Format("The application threw a PINVOKE exception. You probably need to copy the unmanaged dll's to your bin directory. They are a part of fwtools {0}. You can download it from: http://home.gdal.org/fwtools/", SharpMap.Layers.GdalRasterLayer.FWToolsVersion));
        }
        throw;
      }

    }
  }
}
