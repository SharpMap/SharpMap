namespace SharpMap.Demo.Wms.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Web;
    using Data.Providers;
    using Layers;
    using ProjNet.CoordinateSystems;
    using ProjNet.CoordinateSystems.Transformations;
    using Rendering;
    using Styles;

    public static class MapHelper
    {
        private class LayerData
        {
            public string LabelColumn { get; set; }
            public IStyle Style { get; set; }
        }

        private static readonly IDictionary<string, LayerData> Nyc;

        static MapHelper()
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
                { "poly_landmarks", landmarks },
                { "tiger_roads", roads },
                { "poi", pois }
            };
        }

        public static Map OpenLayers()
        {
            ICoordinateTransformation transformation = LatLonToGoogle();
            HttpContext context = HttpContext.Current;
            Map map = new Map(new Size(1, 1));

            IDictionary<string, LayerData> dict = Nyc;
            foreach (string layer in dict.Keys)
            {
                string format = String.Format("~/App_Data/nyc/{0}.shp", layer);
                string path = context.Server.MapPath(format);
                if (!File.Exists(path))
                    throw new FileNotFoundException("file not found", path);

                LayerData data = dict[layer];
                ShapeFile dataSource = new ShapeFile(path, true) { SRID = 900913 };
                VectorLayer item = new VectorLayer(layer, dataSource)
                {
                    Style = (VectorStyle)data.Style,
                    SmoothingMode = SmoothingMode.AntiAlias,
                    CoordinateTransformation = transformation
                };
                map.Layers.Add(item);
            }
            return map;
        }

        public static Map PolyMaps()
        {
            HttpContext context = HttpContext.Current;
            Map map = new Map(new Size(1, 1));

            IDictionary<string, LayerData> dict = Nyc;
            foreach (string layer in dict.Keys)
            {
                string format = String.Format("~/App_Data/nyc/{0}.shp", layer);
                string path = context.Server.MapPath(format);
                if (!File.Exists(path))
                    throw new FileNotFoundException("file not found", path);

                LayerData data = dict[layer];
                ShapeFile dataSource = new ShapeFile(path, true) { SRID = 4326 };
                VectorLayer item = new VectorLayer(layer, dataSource) { Style = (VectorStyle)data.Style };
                map.Layers.Add(item);
            }
            return map;
        }

        public static ICoordinateTransformation LatLonToGoogle()
        {
            CoordinateSystemFactory csFac = new CoordinateSystemFactory();
            CoordinateTransformationFactory ctFac = new CoordinateTransformationFactory();
            IGeographicCoordinateSystem sourceCs = csFac.CreateGeographicCoordinateSystem(
                "WGS 84",
                AngularUnit.Degrees,
                HorizontalDatum.WGS84,
                PrimeMeridian.Greenwich,
                new AxisInfo("north", AxisOrientationEnum.North),
                new AxisInfo("east", AxisOrientationEnum.East));

            List<ProjectionParameter> parameters = new List<ProjectionParameter>
            {
                new ProjectionParameter("semi_major", 6378137.0),
                new ProjectionParameter("semi_minor", 6378137.0),
                new ProjectionParameter("latitude_of_origin", 0.0),
                new ProjectionParameter("central_meridian", 0.0),
                new ProjectionParameter("scale_factor", 1.0),
                new ProjectionParameter("false_easting", 0.0),
                new ProjectionParameter("false_northing", 0.0)
            };
            IProjection projection = csFac.CreateProjection("Google Mercator", "mercator_1sp", parameters);
            IProjectedCoordinateSystem targetCs = csFac.CreateProjectedCoordinateSystem(
                "Google Mercator",
                sourceCs,
                projection,
                LinearUnit.Metre,
                new AxisInfo("East", AxisOrientationEnum.East),
                new AxisInfo("North", AxisOrientationEnum.North));
            return ctFac.CreateFromCoordinateSystems(sourceCs, targetCs);
        }

        public static LabelLayer CreateLabelLayer(VectorLayer src, string column)
        {
            string name = String.Format("{0}:Labels", src.LayerName);
            LabelLayer layer = new LabelLayer(name)
            {
                DataSource = src.DataSource,
                LabelColumn = column,
                LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection,
                MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.CommonCenter,
                MaxVisible = src.MaxVisible,
                MinVisible = src.MinVisible,
                Style =
                {
                    CollisionDetection = true,
                    CollisionBuffer = new SizeF(10F, 10F),
                    Offset = new PointF(0, -5F),
                    Font = new Font(FontFamily.GenericSansSerif, 12),
                    Halo = new Pen(Color.White, 2)
                },
                SmoothingMode = SmoothingMode.HighQuality,
                CoordinateTransformation = src.CoordinateTransformation
            };
            return layer;
        }
    }
}