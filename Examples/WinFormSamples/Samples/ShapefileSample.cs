using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using SharpMap;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Symbolizer;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using Point = GeoAPI.Geometries.Coordinate;

namespace WinFormSamples.Samples
{
    public static partial class ShapefileSample
    {
        private static int _mapId = 0;
        public static Map InitializeMap(float angle)
        {
            switch (_mapId)
            {
                case 0:
                    _mapId++;
                    return InitializeMapOrig(angle);
                case 1:
                    _mapId++;
                    return InitializeMapOsm(angle);
                case 2:
                    _mapId++;
                    return InitializeMapOsm(angle);
                    //return InitializeMapSharpDX(angle);
                case 3:
                    _mapId -= 3;
                    return InitializeMapWithSymbolizerLayers(angle);
                default:
                    _mapId = 0;
                    return InitializeMapOrig(angle);
            }
        }

        private static Map InitializeMapWithSymbolizerLayers(float angle)
        {
            //Initialize a new map of size 'imagesize'
            Map map = new Map();

            //Set up the countries layer
            var layCountries = new SharpMap.Layers.Symbolizer.PolygonalVectorLayer(
                "Countries",
                new ShapeFile("GeoData/World/countries.shp", true),
                new BasicPolygonSymbolizer {Fill = new SolidBrush(Color.Green), Outline = Pens.Black,}
                ) {SRID = 4326};

            //Set up a river layer
            var symbolizer = new CachedLineSymbolizer();
            symbolizer.LineSymbolizeHandlers.AddRange( new [] {
                new PlainLineSymbolizeHandler { Line = new Pen(Color.Blue, 3) { LineJoin = LineJoin.Round } },
                new PlainLineSymbolizeHandler{ Line = new Pen(Color.Aqua, 1)}, });

            var layRivers = new SharpMap.Layers.Symbolizer.LinealVectorLayer("Rivers")
                                {
                                    //Set the datasource to a shapefile in the App_data folder
                                    DataSource = new ShapeFile("GeoData/World/rivers.shp", true),
                                    //Define a blue 2px wide pen
                                    Symbolizer = symbolizer,
                                    SRID = 4326
                                };

            //Set up a cities layer
            var layCities = new SharpMap.Layers.Symbolizer.PuntalVectorLayer("Cities")
                                {
                                    //Set the datasource to a shapefile in the App_data folder
                                    DataSource = new ShapeFile("GeoData/World/cities.shp", true),
                                    Symbolizer = new RasterPointSymbolizer() { Scale = 0.8f },
                                    MaxVisible = 40
                                } ;

            //Set up a country label layer
            var layLabel = new LabelLayer("Country labels")
                               {
                                   DataSource = layCountries.DataSource,
                                   Enabled = true,
                                   LabelColumn = "Name",
                                   LabelFilter = LabelCollisionDetection.QuickAccurateCollisionDetectionMethod,
                                   Style =
                                       new LabelStyle
                                           {
                                               ForeColor = Color.White,
                                               Font = new Font(FontFamily.GenericSerif, 12),
                                               BackColor = new SolidBrush(Color.FromArgb(128, 255, 0, 0)),
                                               HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center
                                           },
                                   MaxVisible = 90,
                                   MinVisible = 30,
                                   SRID = 4326,
                                   MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                                   
                               };

            //Set up a city label layer
            var layCityLabel = new LabelLayer("City labels")
                                   {
                                       DataSource = layCities.DataSource,
                                       Enabled = true,
                                       LabelColumn = "Name",
                                       TextRenderingHint = TextRenderingHint.AntiAlias,
                                       SmoothingMode = SmoothingMode.AntiAlias,
                                       SRID = 4326,
                                       LabelFilter = LabelCollisionDetection.QuickAccurateCollisionDetectionMethod,
                                       Style =
                                           new LabelStyle
                                               {
                                                   ForeColor = Color.Black,
                                                   Font = new Font(FontFamily.GenericSerif, 11),
                                                   HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
                                                   VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom,
                                                   Offset = new PointF(3, 3),
                                                   CollisionDetection = true,
                                                   Halo = new Pen(Color.Yellow, 2)
                                               }, 
                                       MaxVisible = layLabel.MinVisible,
                                   };

            //Setup River label
            var layRiverLabel = new LabelLayer("River labels")
                                   {
                                       DataSource = layRivers.DataSource,
                                       Enabled = true,
                                       LabelColumn = "Name",
                                       TextRenderingHint = TextRenderingHint.AntiAlias,
                                       SmoothingMode = SmoothingMode.AntiAlias,
                                       SRID = 4326,
                                       LabelFilter = LabelCollisionDetection.QuickAccurateCollisionDetectionMethod,
                                       MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.All,
                                       Style =
                                           new LabelStyle
                                               {
                                                   ForeColor = Color.DarkBlue,
                                                   Font = new Font(FontFamily.GenericSansSerif, 11),
                                                   HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                                                   VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Middle,
                                                   CollisionDetection = true,
                                                   Halo = new Pen(Color.Azure, 2), 
                                                   IgnoreLength =  true
                                                   
                                               }, 
                                   };

            //Add the layers to the map object.
            //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
            map.Layers.Add(layCountries);
            map.Layers.Add(layRivers);
            map.Layers.Add(layCities);
            map.Layers.Add(layRiverLabel);
            map.Layers.Add(layLabel);
            map.Layers.Add(layCityLabel);


            //limit the zoom to 360 degrees width
            map.MaximumZoom = 360;
            map.BackColor = Color.LightBlue;

            map.Zoom = 360;
            map.Center = new Point(0, 0);

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }
        private static Map InitializeMapOrig(float angle)
        {
            //Initialize a new map of size 'imagesize'
            Map map = new Map();
            map.SRID = 4326;

            //Set up the countries layer
            VectorLayer layCountries = new VectorLayer("Countries");
            //Set the datasource to a shapefile in the App_data folder
            layCountries.DataSource = new ShapeFile("GeoData/World/countries.shp", true);
            //Set fill-style to green
            layCountries.Style.Fill = new SolidBrush(Color.FromArgb(64, Color.Green));
            //Set the polygons to have a black outline
            layCountries.Style.Outline = Pens.Black;
            layCountries.Style.EnableOutline = true;
            layCountries.SRID = 4326;

            //Set up a river layer
            VectorLayer layRivers = new VectorLayer("Rivers");
            //Set the datasource to a shapefile in the App_data folder
            layRivers.DataSource = new ShapeFile("GeoData/World/rivers.shp", true);
            //Define a blue 1px wide pen
            layRivers.Style.Line = new Pen(Color.Blue, 1);
            layRivers.SRID = 4326;

            //Set up a cities layer
            VectorLayer layCities = new VectorLayer("Cities");
            //Set the datasource to a shapefile in the App_data folder
            layCities.DataSource = new ShapeFile("GeoData/World/cities.shp", true);
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
            layLabel.LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection;
            layLabel.Style.CollisionDetection = true;
            layLabel.LabelPositionDelegate = fdr => fdr.Geometry.InteriorPoint.Coordinate;
            layLabel.PriorityColumn = "POPDENS";

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
            layCityLabel.PriorityColumn = "POPULATION";
            layCityLabel.Theme = new GradientTheme(layCityLabel.PriorityColumn, 250000, 5000000,
                new LabelStyle
                {
                    MaxVisible = 10,
                    CollisionBuffer = new Size(0, 0), CollisionDetection = true, Enabled = true,
                    ForeColor = Color.LightSlateGray, Halo = new Pen(Color.Silver, 1), 
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Middle,
                    Font = new Font(GenericFontFamilies.SansSerif.ToString(), 8f, FontStyle.Regular)
                } ,
                new LabelStyle
                {
                    MaxVisible = layLabel.MinVisible,
                    CollisionBuffer = new Size(3, 3), CollisionDetection = true, Enabled = true,
                    ForeColor = Color.LightSlateGray, Halo = new Pen(Color.Silver, 5), 
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Middle,
                    Font = new Font(GenericFontFamilies.SansSerif.ToString(), 16f, FontStyle.Bold)
                });

            bool ignoreLength = false;

            var layRiverLabel = new LabelLayer("River labels")
            {
                DataSource = layRivers.DataSource,
                Enabled = true,
                LabelColumn = "Name",
                TextRenderingHint = TextRenderingHint.AntiAlias,
                SmoothingMode = SmoothingMode.AntiAlias,
                SRID = 4326,
                LabelFilter = LabelCollisionDetection.QuickAccurateCollisionDetectionMethod,
                MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.CommonCenter,
                Style =
                                           new LabelStyle
                                           {
                                               ForeColor = Color.DarkBlue,
                                               Font = new Font(FontFamily.GenericSansSerif, 11),
                                               HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                                               VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Middle,
                                               CollisionDetection = true,
                                               Halo = new Pen(Color.Azure, 2),
                                               IgnoreLength = ignoreLength,
                                               Offset = new PointF(0, -10)

                                           },
            };

            //Add the layers to the map object.
            //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
            //map.BackgroundLayer.Add(AsyncLayerProxyLayer.Create(layCountries));
            map.Layers.Add(layCountries); 
            map.Layers.Add(layRivers);
            map.Layers.Add(layCities);
            map.Layers.Add(layLabel);
            map.Layers.Add(layCityLabel);
            map.Layers.Add(layRiverLabel);


            //limit the zoom to 360 degrees width
            map.MaximumZoom = 360;
            map.BackColor = Color.LightBlue;

            map.Zoom = 360;
            map.Center = new Point(0, 0);

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }

        /*
        private static Map InitializeMapSharpDX(float angle)
        {
            //Initialize a new map of size 'imagesize'
            Map map = new Map();

            //Set up the countries layer
            VectorLayer layCountries = new SharpDXVectorLayer("Countries");
            //Set the datasource to a shapefile in the App_data folder
            layCountries.DataSource = new ShapeFile("GeoData/World/countries.shp", true);
            //Set fill-style to green
            layCountries.Style.Fill = new SolidBrush(Color.FromArgb(64, Color.LimeGreen));
            //Set the polygons to have a black outline
            layCountries.Style.Outline = Pens.Black;
            layCountries.Style.EnableOutline = true;
            layCountries.SRID = 4326;

            //Set up a river layer
            VectorLayer layRivers = new SharpDXVectorLayer("Rivers");
            //Set the datasource to a shapefile in the App_data folder
            layRivers.DataSource = new ShapeFile("GeoData/World/rivers.shp", true);
            //Define a blue 1px wide pen
            layRivers.Style.Line = new Pen(Color.Blue, 1);
            layRivers.SRID = 4326;

            //Set up a cities layer
            VectorLayer layCities = new SharpDXVectorLayer("Cities");
            //Set the datasource to a shapefile in the App_data folder
            layCities.DataSource = new ShapeFile("GeoData/World/cities.shp", true);
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
            //map.BackgroundLayer.Add(AsyncLayerProxyLayer.Create(layCountries));
            map.Layers.Add(layCountries);
            map.Layers.Add(layRivers);
            map.Layers.Add(layCities);
            map.Layers.Add(layLabel);
            map.Layers.Add(layCityLabel);


            //limit the zoom to 360 degrees width
            map.MaximumZoom = 360;
            map.BackColor = Color.LightBlue;

            map.Zoom = 360;
            map.Center = new Point(0, 0);

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }
        */
        internal static Map InitializeMap(float angle, string[] filenames)
        {
            if (filenames == null)
                return null;

            var providers = new SharpMap.Data.Providers.ShapeFile[filenames.Length];
            for (int i = 0; i < filenames.Length; i++)
            {
                providers[i] = new ShapeFile(filenames[i], true, false);
                providers[i].Open();
            }

            var map = LayerTools.GetMapForProviders(providers);

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }
    }
}
