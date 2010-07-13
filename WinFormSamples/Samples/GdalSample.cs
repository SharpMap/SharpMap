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
        private static int _num = 0;
        private static String _gdalSampleDataset;
        public static String GdalSampleDataSet
        {
            get { return _gdalSampleDataset; }
        }

        public static Map InitializeMap()
        {
            switch (_num++ % 8)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return InitializeGeoTiff(_num);
                default:
                    return InitializeVRT(ref _num);
            }
        }

        private static Map InitializeGeoTiff(Int32 index)
        {
            try
            {
                //Sample provided by Dan Brecht and Joel Wilson
                Map map = new Map();
                map.BackColor = Color.White;
                const string relativePath = "GeoData/GeoTiff/";

                GdalRasterLayer layer;

                switch (index)
                {
                    case 1:
                        layer = new GdalRasterLayer("GeoTiff", relativePath + "SU83NE.TIF");
                        layer.UseRotation = true;
                        map.Layers.Add(layer);
                        break;
                    case 2:
                        layer = new GdalRasterLayer("GeoTiff", relativePath + "utm.tif");
                        layer.UseRotation = true;
                        map.Layers.Add(layer);
                        break;
                    case 3:
                        layer = new GdalRasterLayer("GeoTiff", relativePath + "utm.jp2");
                        layer.UseRotation = true;
                        map.Layers.Add(layer);
                        break;

                    default:
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
                        break;
                }

                map.ZoomToExtents();
                if (_num > 4) _num = 1;
                _gdalSampleDataset = "GeoTiff" + _num;
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

        private static readonly string[] Vrts = new string[] { @"..\DEM\Golden_CO.dem", "contours_sample_polyline_play_polyline.asc", "contours_sample_polyline_play1_polyline.vrt", "contours_sample_polyline_play2_polyline.vrt", "contours_sample_polyline_play3_polyline.vrt", "contours_sample_polyline_play3_polyline.vrt" };
        private const string RelativePath = "GeoData/VRT/";
        private static Map InitializeVRT(ref Int32 index)
        {
            Map map = new Map();
            Int32 ind = index - 5;
            if (ind >= Vrts.Length) ind = 0;

            if (!File.Exists(RelativePath + Vrts[ind]))
            {
                throw new Exception("Make sure the data is in the relative directory: " + RelativePath);
            }

            GdalRasterLayer layer = new GdalRasterLayer("VirtualRasterTable", RelativePath + Vrts[ind]);
            map.Layers.Add(layer);
            _gdalSampleDataset = string.Format("'{0}'", Path.GetExtension(layer.Filename).ToUpper());
            map.ZoomToExtents();
            //index++;
            return map;
        }

    }

}