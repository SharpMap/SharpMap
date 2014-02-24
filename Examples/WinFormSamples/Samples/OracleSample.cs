using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap;
using SharpMap.Layers;
using SharpMap.Data.Providers;
using SharpMap.Styles;

namespace WinFormSamples.Samples
{
    public class OracleSample
    {
        public static Map InitializeMap(float angle)
        {
            //Initialize a new map of size 'imagesize'
            Map map = new Map();

            //Set up the countries layer
            VectorLayer layCountries = new VectorLayer("OracleSample");

            //Set the datasource to a shapefile in the App_data folder
            layCountries.DataSource = new SharpMap.Data.Providers.Oracle("system", "metsys", "127.0.0.1",
                                                                         "COUNTRIES", "GEOM", "ID");
            //System.Diagnostics.Debug.WriteLine(layCountries.DataSource.GetGeometryByID(101).ToString());

            //Set fill-style to green
            layCountries.Style.Fill = new SolidBrush(Color.Green);
            //Set the polygons to have a black outline
            layCountries.Style.Outline = Pens.Black;
            layCountries.Style.EnableOutline = true;

            //Set up a river layer
            VectorLayer layRivers = new SharpMap.Layers.VectorLayer("Rivers");
            //Set the datasource to a shapefile in the App_data folder
            layRivers.DataSource = new SharpMap.Data.Providers.Oracle("system", "metsys", "127.0.0.1", "RIVERS", "GEOM", "ID");
            //Define a blue 1px wide pen
            layRivers.Style.Line = new Pen(Color.Blue, 1);

            //Set up a river layer
            VectorLayer layCities = new VectorLayer("Cities");
            //Set the datasource to a shapefile in the App_data folder
            layCities.DataSource = new SharpMap.Data.Providers.Oracle("system", "metsys", "127.0.0.1", "CITIES", "GEOM", "ID");
            layCities.Style.SymbolScale = 0.8f;
            layCities.MaxVisible = 40;

            //Set up a country label layer
            LabelLayer layLabel = new LabelLayer("Country labels")
            {
                DataSource = layCountries.DataSource,
                Enabled = true,
                LabelColumn = "NAME",
                MaxVisible = 90,
                MinVisible = 30,
                MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                PriorityColumn = "POPDENS",
                Style = new LabelStyle()
                {
                    ForeColor = Color.White,
                    Font = new Font(FontFamily.GenericSerif, 12),
                    BackColor = new SolidBrush(Color.FromArgb(128, 255, 0, 0)),
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    CollisionDetection = true
                }
            };

            ////Set up a city label layer
            LabelLayer layCityLabel = new LabelLayer("City labels")
            {
                DataSource = layCities.DataSource,
                Enabled = true,
                LabelColumn = "NAME",
                PriorityColumn = "POPULATION",
                PriorityDelegate = delegate(SharpMap.Data.FeatureDataRow fdr)
                {
                    Int32 retVal = 10000000 * ((String)fdr["capital"] == "Y" ? 1 : 0);
                    return retVal + Convert.ToInt32(fdr["population"]);
                },
                TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias,
                SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias,
                LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                Style = new LabelStyle()
                {
                    ForeColor = Color.Black,
                    Font = new Font(FontFamily.GenericSerif, 11),
                    MaxVisible = layLabel.MinVisible,
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
                    VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom,
                    Offset = new PointF(3, 3),
                    Halo = new Pen(Color.Yellow, 2),
                    CollisionDetection = true
                }
            };

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

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;

        }
    }
}