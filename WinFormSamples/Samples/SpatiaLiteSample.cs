using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace WinFormSamples.Samples
{
    public static class SpatiaLiteSample
    {
        public static SharpMap.Map InitializeMap()
        {
            //Initialize a new map of size 'imagesize'
            SharpMap.Map map = new Map();

            //Set up the countries layer
            SharpMap.Layers.VectorLayer layCountries = new SharpMap.Layers.VectorLayer("VRS2386");

            //Set the datasource to a shapefile in the App_data folder
            layCountries.DataSource = new SharpMap.Data.Providers.SpatiaLite(
                Properties.Settings.Default.SpatiaLiteConnectionString,
                "regions", "XGeometryX", "OID");

            //Set fill-style to green
            layCountries.Style.Fill = new SolidBrush(Color.Green);
            //Set the polygons to have a black outline
            layCountries.Style.Outline = System.Drawing.Pens.Black;
            layCountries.Style.EnableOutline = true;

            ////Set up a river layer
            //SharpMap.Layers.VectorLayer layRivers = new SharpMap.Layers.VectorLayer("Rivers");
            ////Set the datasource to a shapefile in the App_data folder
            //layRivers.DataSource = new SharpMap.Data.Providers.PostGIS(Properties.Settings.Default.PostGisConnectionString, "rivers", "ogc_fid");
            ////Define a blue 1px wide pen
            //layRivers.Style.Line = new Pen(Color.Blue, 1);

            ////Set up a river layer
            //SharpMap.Layers.VectorLayer layCities = new SharpMap.Layers.VectorLayer("Cities");
            ////Set the datasource to a shapefile in the App_data folder
            //layCities.DataSource = new SharpMap.Data.Providers.PostGIS(Properties.Settings.Default.PostGisConnectionString, "cities", "ogc_fid");
            //layCities.Style.SymbolScale = 0.8f;
            //layCities.MaxVisible = 40;

            //Set up a country label layer
            SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels") 
            {
                DataSource = layCountries.DataSource,
                Enabled = true,
                LabelColumn = "EINWOHNER",
                MaxVisible = 90,
                MinVisible = 30,
                MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                PriorityColumn = "EINWOHNER",
                Style = new SharpMap.Styles.LabelStyle()
                {
                    ForeColor = Color.White,
                    Font = new Font(FontFamily.GenericSerif, 12),
                    BackColor = new System.Drawing.SolidBrush(Color.FromArgb(128, 255, 0, 0)),
                    HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                    CollisionDetection = true
                }
            };

            ////Set up a city label layer
            //SharpMap.Layers.LabelLayer layCityLabel = new SharpMap.Layers.LabelLayer("City labels")
            //{
            //    DataSource = layCities.DataSource,
            //    Enabled = true,
            //    LabelColumn = "name",
            //    PriorityColumn = "population",
            //    PriorityDelegate = delegate(SharpMap.Data.FeatureDataRow fdr) 
            //    { 
            //        Int32 retVal = 10000000 * (Int32)( (String)fdr["capital"] == "Y" ? 1 : 0 );
            //        return  retVal + Convert.ToInt32(fdr["population"]);
            //    },
            //    TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias,
            //    SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias,
            //    LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
            //    Style = new LabelStyle()
            //    {
            //        ForeColor = Color.Black,
            //        Font = new Font(FontFamily.GenericSerif, 11),
            //        MaxVisible = layLabel.MinVisible,
            //        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
            //        VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom,
            //        Offset = new PointF(3, 3),
            //        Halo = new Pen(Color.Yellow, 2),
            //        CollisionDetection = true
            //    }
            //};

            //Add the layers to the map object.
            //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
            map.Layers.Add(layCountries);
            //map.Layers.Add(layRivers);
            //map.Layers.Add(layCities);
            map.Layers.Add(layLabel);
            //map.Layers.Add(layCityLabel);

            //limit the zoom to 360 degrees width
            map.MaximumZoom = 360;
            map.BackColor = Color.LightBlue;

            map.ZoomToExtents(); // = 360;
            //map.Center = new SharpMap.Geometries.Point(0, 0);

            return map;

        }
    }
}
