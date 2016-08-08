using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;
using Point=GeoAPI.Geometries.Coordinate;

namespace WinFormSamples.Samples
{
    public static class OgrSample
    {
        private static int _num = 0;
        private static String _ogrSampleDataset;
        public static String OgrSampleDataSet
        {
            get { return _ogrSampleDataset; }
        }

        public static Map InitializeMap(float angle)
        {
            switch (_num++ % 3)
            {
                case 0:
                    return InitializeMapinfo(angle);
                case 1:
                    return InitializeS57(angle);
                case 2:
                    return InitializeDXF(angle);
            }
            return InitializeMapinfo(angle);
        }

        private static Map InitializeDXF(float angle)
        {
            Map map = new Map();
            //Set the datasource to a shapefile in the App_data folder
            Ogr provider;
            try
            {
                provider = new Ogr("GeoData/SampleDXF.dxf",0);
            }
            catch (TypeInitializationException ex)
            {
                if (ex.Message == "The type initializer for 'OSGeo.OGR.Ogr' threw an exception.")
                {
                    throw new Exception(
                        String.Format(
                            "The application threw a PINVOKE exception. You probably need to copy the unmanaged dll's to your bin directory. They are a part of fwtools {0}. You can download it from: http://home.gdal.org/fwtools/",
                            GdalRasterLayer.FWToolsVersion));
                }
                throw;
            }
            VectorLayer lay = new VectorLayer("SampleDXF", provider);
            map.Layers.Add(lay);
            map.ZoomToExtents();
            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            _ogrSampleDataset = "SampleDXF";
            return map;

        }

        private static Map InitializeS57(float angle)
        {
            //Initialize a new map of size 'imagesize'
            Map map = new Map();

            //Set the datasource to a shapefile in the App_data folder
            Ogr provider;
            try
            {
                provider = new Ogr("GeoData/S57/US5TX51M.000");
            }
            catch (TypeInitializationException ex)
            {
                if (ex.Message == "The type initializer for 'OSGeo.OGR.Ogr' threw an exception.")
                {
                    throw new Exception(
                        String.Format(
                            "The application threw a PINVOKE exception. You probably need to copy the unmanaged dll's to your bin directory. They are a part of fwtools {0}. You can download it from: http://home.gdal.org/fwtools/",
                            GdalRasterLayer.FWToolsVersion));
                }
                throw;
            }

            VectorLayer lay;
            Random rnd = new Random(9);
            for (Int32 i = provider.NumberOfLayers - 1; i >= 0; i--)
            {
                Ogr prov = new Ogr("GeoData/S57/US5TX51M.000", i);
                if (!prov.IsFeatureDataLayer) continue;
                string name = prov.LayerName;
                System.Diagnostics.Debug.WriteLine(string.Format("Layer {0}: {1}", i, name));
                //if (provider.GeometryType )
                lay = new VectorLayer(string.Format("Layer_{0}", name), prov);
                if (prov.OgrGeometryTypeString.IndexOf("Polygon") > 0)
                {
                    lay.Style.Fill =
                        new SolidBrush(Color.FromArgb(150, Convert.ToInt32(rnd.NextDouble() * 255),
                                                      Convert.ToInt32(rnd.NextDouble() * 255),
                                                      Convert.ToInt32(rnd.NextDouble() * 255)));
                    lay.Style.Outline =
                        new Pen(
                            Color.FromArgb(150, Convert.ToInt32(rnd.NextDouble() * 255),
                                           Convert.ToInt32(rnd.NextDouble() * 255),
                                           Convert.ToInt32(rnd.NextDouble() * 255)),
                            Convert.ToInt32(rnd.NextDouble() * 3));
                    lay.Style.EnableOutline = true;
                }
                else
                {
                    lay.Style.Line =
                        new Pen(
                            Color.FromArgb(150, Convert.ToInt32(rnd.NextDouble()*255),
                                           Convert.ToInt32(rnd.NextDouble()*255), Convert.ToInt32(rnd.NextDouble()*255)),
                            Convert.ToInt32(rnd.NextDouble()*3));
                }
                map.Layers.Add(lay);
            }
            _ogrSampleDataset = "S-57";
            map.ZoomToExtents();

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }

        private static Map InitializeMapinfo(float angle)
        {
            //Initialize a new map of size 'imagesize'
            Map map = new Map();

            //Set up the countries layer
            VectorLayer layCountries = new VectorLayer("Countries");
            //Set the datasource to a shapefile in the App_data folder
            try
            {
                layCountries.DataSource = new Ogr("GeoData/MapInfo/countriesMapInfo.tab");
            }
            catch (TypeInitializationException ex)
            {
                if (ex.Message == "The type initializer for 'OSGeo.OGR.Ogr' threw an exception.")
                {
                    throw new Exception(
                        String.Format(
                            "The application threw a PINVOKE exception. You probably need to copy the unmanaged dll's to your bin directory. They are a part of fwtools {0}. You can download it from: http://home.gdal.org/fwtools/",
                            GdalRasterLayer.FWToolsVersion));
                }
                throw;
            }

            //Set fill-style to green
            layCountries.Style.Fill = new SolidBrush(Color.Green);
            //Set the polygons to have a black outline
            layCountries.Style.Outline = Pens.Black;
            layCountries.Style.EnableOutline = true;
            layCountries.SRID = 4326;

            //Set up a river layer
            VectorLayer layRivers = new VectorLayer("Rivers");
            //Set the datasource to a shapefile in the App_data folder
            layRivers.DataSource = new Ogr("GeoData/MapInfo/riversMapInfo.tab");
            //Define a blue 1px wide pen
            layRivers.Style.Line = new Pen(Color.Blue, 1);
            layRivers.SRID = 4326;

            //Set up a river layer
            VectorLayer layCities = new VectorLayer("Cities");
            //Set the datasource to a shapefile in the App_data folder
            layCities.DataSource = new Ogr("GeoData/MapInfo/citiesMapInfo.tab");
            layCities.Style.SymbolScale = 0.8f;
            layCities.MaxVisible = 40;
            layCities.SRID = 4326;

            //Set up a country label layer
            LabelLayer layLabel = new LabelLayer("Country labels");
            layLabel.DataSource = layCountries.DataSource;
            layLabel.Enabled = true;
            layLabel.LabelColumn = "Name";
            layLabel.Style = new LabelStyle();
            layLabel.Style.ForeColor = Color.White;
            layLabel.Style.Font = new Font(FontFamily.GenericSerif, 12);
            layLabel.Style.BackColor = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
            layLabel.MaxVisible = 90;
            layLabel.MinVisible = 30;
            layLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
            layLabel.SRID = 4326;
            layLabel.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest;

            //Set up a city label layer
            LabelLayer layCityLabel = new LabelLayer("City labels");
            layCityLabel.DataSource = layCities.DataSource;
            layCityLabel.Enabled = true;
            layCityLabel.LabelColumn = "Name";
            layCityLabel.Style = new LabelStyle();
            layCityLabel.Style.ForeColor = Color.Black;
            layCityLabel.Style.Font = new Font(FontFamily.GenericSerif, 11);
            layCityLabel.MaxVisible = layLabel.MinVisible;
            layCityLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
            layCityLabel.Style.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
            layCityLabel.Style.Offset = new PointF(3, 3);
            layCityLabel.Style.Halo = new Pen(Color.Yellow, 2);
            layCityLabel.TextRenderingHint = TextRenderingHint.AntiAlias;
            layCityLabel.SmoothingMode = SmoothingMode.AntiAlias;
            layCityLabel.SRID = 4326;
            layCityLabel.LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection;
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
            map.Center = new Point(0, 0);

            _ogrSampleDataset = "MapInfo";

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }

        internal static Map InitializeMap(float angle, string[] filenames)
        {
            if (filenames == null)
                return null;

            var providers = new SharpMap.Data.Providers.Ogr[filenames.Length];
            for (int i = 0; i < filenames.Length; i++)
            {
                providers[i] = new Ogr(filenames[i]);
            }

            var map = LayerTools.GetMapForProviders(providers);

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;
            map.ZoomToExtents();
            return map;
        }
    }
}