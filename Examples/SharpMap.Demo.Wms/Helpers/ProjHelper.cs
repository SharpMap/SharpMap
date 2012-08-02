namespace SharpMap.Demo.Wms.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    using ProjNet.CoordinateSystems;
    using ProjNet.CoordinateSystems.Transformations;

    using SharpMap.Layers;
    using SharpMap.Rendering;

    public static class ProjHelper
    {
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
            ICoordinateTransformation transformation = ctFac.CreateFromCoordinateSystems(sourceCs, targetCs);
            return transformation;
        }
    }

    public static class LabelHelper
    {
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