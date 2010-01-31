using System;
using System.Drawing;
using System.IO;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;

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

                GdalRasterLayer layer;

                if (!File.Exists(relativePath + "format01-image_a.tif"))
                {
                    throw new Exception("Make sure the data is in the relative directory: " + relativePath);
                }

                layer = new GdalRasterLayer("GeoTiffA", relativePath + "format01-image_a.tif");
                map.Layers.Add(layer);
                layer = new GdalRasterLayer("GeoTiffB", relativePath + "format01-image_b.tif");
                map.Layers.Add(layer);
                layer = new GdalRasterLayer("GeoTiffC", relativePath + "format01-image_c.tif");
                map.Layers.Add(layer);
                layer = new GdalRasterLayer("GeoTiffD", relativePath + "format01-image_d.tif");
                map.Layers.Add(layer);

                VectorLayer shapeLayer;

                if (!File.Exists(relativePath + "outline.shp"))
                {
                    throw new Exception("Make sure the data is in the relative directory: " + relativePath);
                }

                shapeLayer = new VectorLayer("outline", new ShapeFile(relativePath + "outline.shp"));
                shapeLayer.Style.Fill = Brushes.Transparent;
                shapeLayer.Style.Outline = Pens.Black;
                shapeLayer.Style.EnableOutline = true;
                shapeLayer.Style.Enabled = true;
                map.Layers.Add(shapeLayer);

                map.ZoomToExtents();

                return map;
            }
            catch (TypeInitializationException ex)
            {
                if (ex.Message == "The type initializer for 'OSGeo.GDAL.GdalPINVOKE' threw an exception.")
                {
                    throw new Exception(
                        String.Format(
                            "The application threw a PINVOKE exception. You probably need to copy the unmanaged dll's to your bin directory. They are a part of fwtools {0}. You can download it from: http://home.gdal.org/fwtools/",
                            GdalRasterLayer.FWToolsVersion));
                }
                throw;
            }
        }
    }
    public static class GdalSample2
    {
        private static readonly string[] Vrts = new string[] { @"..\DEM\Golden_CO.dem", "contours_sample_polyline_play_polyline.asc", "contours_sample_polyline_play1_polyline.vrt", "contours_sample_polyline_play2_polyline.vrt", "contours_sample_polyline_play3_polyline.vrt", "contours_sample_polyline_play3_polyline.vrt" };
        private static int _index = 1;
        private const string RelativePath = "GeoData/VRT/";

        public static Map InitializeMap()
        {
            try
            {
                _index = -1;
                //Sample provided by Dan Brecht and Joel Wilson
                Map map = new Map();
                map.BackColor = Color.White;

                Next(map);
                map.ZoomToExtents();

                return map;
            }
            catch (TypeInitializationException ex)
            {
                if (ex.Message == "The type initializer for 'OSGeo.GDAL.GdalPINVOKE' threw an exception.")
                {
                    throw new Exception(
                        String.Format(
                            "The application threw a PINVOKE exception. You probably need to copy the unmanaged dll's to your bin directory. They are a part of fwtools {0}. You can download it from: http://home.gdal.org/fwtools/",
                            GdalRasterLayer.FWToolsVersion));
                }
                throw;
            }
        }

        public static void Next(Map map)
        {
            _index++;
            if (_index >= Vrts.Length) _index = 0;

            if (!File.Exists(RelativePath + Vrts[_index]))
            {
                throw new Exception("Make sure the data is in the relative directory: " + RelativePath);
            }

            GdalRasterLayer layer = new GdalRasterLayer("VirtualRasterTable", RelativePath + Vrts[_index]);
            map.Layers.Add(layer);

            map.ZoomToExtents();

        }
    }
}