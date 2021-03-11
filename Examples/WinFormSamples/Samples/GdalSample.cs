using System;
using System.Linq;
using System.Reflection;

namespace WinFormSamples.Samples
{
    public static class GdalSample
    {
        private static int _num;
        private static string _gdalSampleDataset;
        public static string GdalSampleDataSet
        {
            get { return _gdalSampleDataset; }
        }

        public static SharpMap.Map InitializeMap(float angle)
        {
            _num %= 9;
            int num = _num;
            _num++;

            switch (num)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return InitializeGeoTiff(num, angle);
                default:
                    return InitializeVRT(num, angle);
            }
        }

        private static SharpMap.Map InitializeGeoTiff(int index, float angle)
        {
            try
            {
                //Sample provided by Dan Brecht and Joel Wilson
                var map = new SharpMap.Map();
                map.BackColor = System.Drawing.Color.White;
                const string relativePath = "GeoData/GeoTiff/";

                SharpMap.Layers.GdalRasterLayer layer;

                switch (index)
                {
                    case 0:
                        layer = new SharpMap.Layers.GdalRasterLayer("GeoTiff", relativePath + "utm.tif");
                        map.Layers.Add(layer);
                        break;
                    case 1:
                        layer = new SharpMap.Layers.GdalRasterLayer("GeoTiff", relativePath + "utm.jp2");
                        map.Layers.Add(layer);
                        break;

                    case 2:
                        layer = new SharpMap.Layers.GdalRasterLayer("GeoTiff", relativePath + "world_raster_mod.tif");
                        map.Layers.Add(layer);
                        break;

                    default:
                        if (!System.IO.File.Exists(relativePath + "format01-image_a.tif"))
                        {
                            throw new System.Exception("Make sure the data is in the relative directory: " + relativePath);
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

                        if (!System.IO.File.Exists(relativePath + "outline.shp"))
                        {
                            throw new System.Exception("Make sure the data is in the relative directory: " + relativePath);
                        }

                        shapeLayer = new SharpMap.Layers.VectorLayer("outline", new SharpMap.Data.Providers.ShapeFile(relativePath + "outline.shp"));
                        shapeLayer.Style.Fill = System.Drawing.Brushes.Transparent;
                        shapeLayer.Style.Outline = System.Drawing.Pens.Black;
                        shapeLayer.Style.EnableOutline = true;
                        shapeLayer.Style.Enabled = true;
                        map.Layers.Add(shapeLayer);
                        break;
                }
                
                //if (layer != null)
                //    layer.UseRotation = true;

                map.ZoomToExtents();

                System.Drawing.Drawing2D.Matrix mat = new System.Drawing.Drawing2D.Matrix();
                mat.RotateAt(angle, map.WorldToImage(map.Center));
                map.MapTransform = mat;

                _gdalSampleDataset = "GeoTiff" + _num;
                return map;
            }
            catch (System.TypeInitializationException ex)
            {
                if (ex.Message == "The type initializer for 'OSGeo.GDAL.GdalPINVOKE' threw an exception.")
                {
                    var asm = Assembly.GetAssembly(typeof(OSGeo.GDAL.Gdal)).GetName();
                    throw new System.Exception(
                        string.Format(
                            "The application threw a PINVOKE exception. You probably need to copy the unmanaged dll's to your bin directory. " +
                            "They are a part of the GDAL NuGet package v{0}.",
                            asm.Version.ToString(3)));
                }
                throw;
            }

        }

        private static readonly string[] Vrts =
        {
            @"..\DEM\Golden_CO.dem", 
            "contours_sample_polyline_play_polyline.asc", 
            "contours_sample_polyline_play1_polyline.vrt", 
            "contours_sample_polyline_play2_polyline.vrt", 
            "contours_sample_polyline_play3_polyline.vrt"
        };

        private const string RelativePath = "GeoData/VRT/";
        private static SharpMap.Map InitializeVRT(int index, float angle)
        {
            SharpMap.Map map = new SharpMap.Map();
            int ind = index - 4;
            if (ind >= Vrts.Length) ind = 0;

            if (!System.IO.File.Exists(RelativePath + Vrts[ind]))
            {
                throw new System.Exception("Make sure the data is in the relative directory: " + RelativePath);
            }

            SharpMap.Layers.GdalRasterLayer layer = new SharpMap.Layers.GdalRasterLayer("VirtualRasterTable", RelativePath + Vrts[ind]);

            var ext = System.IO.Path.GetExtension(layer.Filename);
            map.Layers.Add(layer);
            _gdalSampleDataset = string.Format("'{0}'", ext != null ? ext.ToUpper() : string.Empty);
            map.ZoomToExtents();

            System.Drawing.Drawing2D.Matrix mat = new System.Drawing.Drawing2D.Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;
            
            return map;
        }

        public static SharpMap.Map InitializeMap(int angle, string[] filenames)
        {
            if (filenames == null || filenames.Length == 0) return null;

            var map = new SharpMap.Map();
            for (int i = 0; i < filenames.Length; i++)
            {
                if (filenames[i].StartsWith("PG:"))
                {
                    SharpMap.Layers.ILayer lyr = null;
                    try
                    {
                        lyr = new SharpMap.Layers.GdalRasterLayer($"PG_RASTER{i}", filenames[i]);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    map.Layers.Add(lyr);
                }
                else
                    map.Layers.Add(new SharpMap.Layers.GdalRasterLayer(System.IO.Path.GetFileName(filenames[i]), filenames[i]));
            }

            var mat = new System.Drawing.Drawing2D.Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;
            map.ZoomToExtents();
            return map;
        }
    }

}
