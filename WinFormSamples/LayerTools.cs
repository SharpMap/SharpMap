using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Layers;
using System.Drawing;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;

namespace WinFormSamples
{
    static class LayerTools
    {

        private static ICoordinateTransformation wgs84toGoogle;
        private static ICoordinateTransformation googletowgs84;

        /// <summary>
        /// Wgs84 to Google Mercator Coordinate Transformation
        /// </summary>
        public static ICoordinateTransformation Wgs84toGoogleMercator
        {
            get
            {

                if (wgs84toGoogle == null)
                {

                    CoordinateSystemFactory csFac = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
                    CoordinateTransformationFactory ctFac = new CoordinateTransformationFactory();

                    IGeographicCoordinateSystem wgs84 = csFac.CreateGeographicCoordinateSystem(
                      "WGS 84", AngularUnit.Degrees, HorizontalDatum.WGS84, PrimeMeridian.Greenwich,
                      new AxisInfo("north", AxisOrientationEnum.North), new AxisInfo("east", AxisOrientationEnum.East));

                    List<ProjectionParameter> parameters = new List<ProjectionParameter>();
                    parameters.Add(new ProjectionParameter("semi_major", 6378137.0));
                    parameters.Add(new ProjectionParameter("semi_minor", 6378137.0));
                    parameters.Add(new ProjectionParameter("latitude_of_origin", 0.0));
                    parameters.Add(new ProjectionParameter("central_meridian", 0.0));
                    parameters.Add(new ProjectionParameter("scale_factor", 1.0));
                    parameters.Add(new ProjectionParameter("false_easting", 0.0));
                    parameters.Add(new ProjectionParameter("false_northing", 0.0));
                    IProjection projection = csFac.CreateProjection("Google Mercator", "mercator_1sp", parameters);

                    IProjectedCoordinateSystem epsg900913 = csFac.CreateProjectedCoordinateSystem(
                      "Google Mercator", wgs84, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East),
                      new AxisInfo("North", AxisOrientationEnum.North));

                    wgs84toGoogle = ctFac.CreateFromCoordinateSystems(wgs84, epsg900913);
                }

                return wgs84toGoogle;

            }
        }


        public static ICoordinateTransformation GoogleMercatorToWgs84
        {
            get
            {


                if (googletowgs84 == null)
                {

                    CoordinateSystemFactory csFac = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
                    CoordinateTransformationFactory ctFac = new CoordinateTransformationFactory();

                    IGeographicCoordinateSystem wgs84 = csFac.CreateGeographicCoordinateSystem(
                      "WGS 84", AngularUnit.Degrees, HorizontalDatum.WGS84, PrimeMeridian.Greenwich,
                      new AxisInfo("north", AxisOrientationEnum.North), new AxisInfo("east", AxisOrientationEnum.East));

                    List<ProjectionParameter> parameters = new List<ProjectionParameter>();
                    parameters.Add(new ProjectionParameter("semi_major", 6378137.0));
                    parameters.Add(new ProjectionParameter("semi_minor", 6378137.0));
                    parameters.Add(new ProjectionParameter("latitude_of_origin", 0.0));
                    parameters.Add(new ProjectionParameter("central_meridian", 0.0));
                    parameters.Add(new ProjectionParameter("scale_factor", 1.0));
                    parameters.Add(new ProjectionParameter("false_easting", 0.0));
                    parameters.Add(new ProjectionParameter("false_northing", 0.0));
                    IProjection projection = csFac.CreateProjection("Google Mercator", "mercator_1sp", parameters);

                    IProjectedCoordinateSystem epsg900913 = csFac.CreateProjectedCoordinateSystem(
                      "Google Mercator", wgs84, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East),
                      new AxisInfo("North", AxisOrientationEnum.North));

                    googletowgs84 = ctFac.CreateFromCoordinateSystems(epsg900913, wgs84);
                }

                return googletowgs84;

            }
        }

        public static LabelLayer CreateLabelLayer(VectorLayer originalLayer, string labelColumnName)
        {
            SharpMap.Layers.LabelLayer labelLayer = new SharpMap.Layers.LabelLayer(originalLayer.LayerName + ":Labels");
            labelLayer.DataSource = originalLayer.DataSource;
            labelLayer.LabelColumn = labelColumnName;
            labelLayer.Style.CollisionDetection = true;
            labelLayer.Style.CollisionBuffer = new SizeF(10F, 10F);
            labelLayer.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
            labelLayer.Style.Offset = new PointF(0, -5F);
            labelLayer.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.CommonCenter;
            labelLayer.Style.Font = new Font(FontFamily.GenericSansSerif, 12);
            labelLayer.MaxVisible = originalLayer.MaxVisible;
            labelLayer.MinVisible = originalLayer.MinVisible;
            labelLayer.Style.Halo = new Pen(Color.White, 2);
            labelLayer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            labelLayer.CoordinateTransformation = originalLayer.CoordinateTransformation;
            return labelLayer;
        }



    }
}
