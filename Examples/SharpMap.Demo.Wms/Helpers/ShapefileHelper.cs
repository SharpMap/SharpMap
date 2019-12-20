using ProjNet.CoordinateSystems;

namespace SharpMap.Demo.Wms.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Web;

    using GeoAPI.CoordinateSystems.Transformations;

    using SharpMap.Data.Providers;
    using SharpMap.Demo.Wms.Models;
    using SharpMap.Layers;
    using SharpMap.Styles;

    public static class ShapefileHelper
    {
        private static readonly IDictionary<string, LayerData> Nyc;

        static ShapefileHelper()
        {
            LayerData landmarks = new LayerData
            {
                LabelColumn = "LANAME",
                Style = new VectorStyle
                {
                    EnableOutline = true,
                    Fill = new SolidBrush(Color.FromArgb(192, Color.LightBlue))
                }
            };
            LayerData roads = new LayerData
            {
                LabelColumn = "NAME",
                Style = new VectorStyle
                {
                    EnableOutline = false,
                    Fill = new SolidBrush(Color.FromArgb(192, Color.LightGray)),
                    Line = new Pen(Color.FromArgb(200, Color.DarkBlue), 0.5f)
                }
            };
            LayerData pois = new LayerData
            {
                LabelColumn = "NAME",
                Style = new VectorStyle
                {
                    PointColor = new SolidBrush(Color.FromArgb(200, Color.DarkGreen)),
                    PointSize = 10
                }
            };

            Nyc = new Dictionary<string, LayerData>
            {
                { "nyc/poly_landmarks.shp", landmarks },
                { "nyc/tiger_roads.shp", roads },
                { "nyc/poi.shp", pois }
            };
        }

        public static Map Spherical()
        {
            //ICoordinateTransformation transformation = ProjHelper.LatLonToGoogle();
            HttpContext context = HttpContext.Current;
            Map map = new Map(new Size(1, 1));

            IDictionary<string, LayerData> dict = Nyc;
            var ctFac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var pos = ctFac.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator);
            var neg = ctFac.CreateFromCoordinateSystems(ProjectedCoordinateSystem.WebMercator, GeographicCoordinateSystem.WGS84);

            foreach (string layer in dict.Keys)
            {
                string format = String.Format("~/App_Data/{0}", layer);
                string path = context.Server.MapPath(format);
                if (!File.Exists(path))
                    throw new FileNotFoundException("file not found", path);

                string name = Path.GetFileNameWithoutExtension(layer);
                LayerData data = dict[layer];
                ShapeFile source = new ShapeFile(path, true);
                VectorLayer item = new VectorLayer(name, source)
                {
                    //SRID = 4326,
                    //TargetSRID = 900913,
                    CoordinateTransformation = pos,
                    ReverseCoordinateTransformation = neg,
                    Style = (VectorStyle)data.Style,
                    SmoothingMode = SmoothingMode.AntiAlias,
                    IsQueryEnabled = true
                };
                map.Layers.Add(item);
            }
            return map;
        }

        public static Map Default()
        {
            HttpContext context = HttpContext.Current;
            Map map = new Map(new Size(1, 1));

            IDictionary<string, LayerData> dict = Nyc;
            foreach (string layer in dict.Keys)
            {
                string format = String.Format("~/App_Data/{0}", layer);
                string path = context.Server.MapPath(format);
                if (!File.Exists(path))
                    throw new FileNotFoundException("file not found", path);

                string name = Path.GetFileNameWithoutExtension(layer);
                LayerData data = dict[layer];
                ShapeFile source = new ShapeFile(path, true);
                VectorLayer item = new VectorLayer(name, source)
                {
                    SRID = 4326,
                    Style = (VectorStyle)data.Style,
                    SmoothingMode = SmoothingMode.AntiAlias
                };
                map.Layers.Add(item);
            }
            return map;
        }
    }
}
