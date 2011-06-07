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

    public static class MapHelper
    {
        private class LayerData
        {
            public string LabelColumn { get; set; }
            public ICoordinateTransformation Transformation { get; set; }
        }

        private static readonly IDictionary<string, LayerData> layers;

        static MapHelper()
        {
            ICoordinateTransformation transformation = LatLonToGoogle();
            layers = new Dictionary<string, LayerData>
            {
                { "poly_landmarks", new LayerData { LabelColumn = "LANAME", Transformation = transformation } },
                { "tiger_roads", new LayerData { LabelColumn = "NAME", Transformation = transformation } } ,
                { "poi", new LayerData { LabelColumn = "NAME", Transformation = transformation} }
            };
        }

        public static Map InitializeMap()
        {
            HttpContext context = HttpContext.Current;
            Map map = new Map(new Size(1, 1));
            foreach (string layer in layers.Keys)
            {
                string format = String.Format("~/App_Data/nyc/{0}.shp", layer);
                string path = context.Server.MapPath(format);
                if (!File.Exists(path))
                    throw new FileNotFoundException("file not found", path);

                LayerData data = layers[layer];
                ShapeFile dataSource = new ShapeFile(path, true) { SRID = 900913 };
                VectorLayer item = new VectorLayer(layer, dataSource) { CoordinateTransformation = data.Transformation };
                map.Layers.Add(item);

                // LabelLayer labels = CreateLabelLayer(item, data.LabelColumn);
                // map.Layers.Add(labels);
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

            List<ProjectionParameter> parameters = new List<ProjectionParameter>();
            parameters.Add(new ProjectionParameter("semi_major", 6378137.0));
            parameters.Add(new ProjectionParameter("semi_minor", 6378137.0));
            parameters.Add(new ProjectionParameter("latitude_of_origin", 0.0));
            parameters.Add(new ProjectionParameter("central_meridian", 0.0));
            parameters.Add(new ProjectionParameter("scale_factor", 1.0));
            parameters.Add(new ProjectionParameter("false_easting", 0.0));
            parameters.Add(new ProjectionParameter("false_northing", 0.0));
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
            LabelLayer layer = new LabelLayer(name);
            layer.DataSource = src.DataSource;
            layer.LabelColumn = column;
            layer.Style.CollisionDetection = true;
            layer.Style.CollisionBuffer = new SizeF(10F, 10F);
            layer.LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection;
            layer.Style.Offset = new PointF(0, -5F);
            layer.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.CommonCenter;
            layer.Style.Font = new Font(FontFamily.GenericSansSerif, 12);
            layer.MaxVisible = src.MaxVisible;
            layer.MinVisible = src.MinVisible;
            layer.Style.Halo = new Pen(Color.White, 2);
            layer.SmoothingMode = SmoothingMode.HighQuality;
            layer.CoordinateTransformation = src.CoordinateTransformation;
            return layer;
        }
    }
}