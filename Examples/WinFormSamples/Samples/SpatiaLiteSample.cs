#if OPENGLSAMPLE
using VectorLayer = SharpMap.Layers.OpenGLVectorLayer;
#else
using VectorLayer = SharpMap.Layers.VectorLayer;
#endif

namespace WinFormSamples.Samples
{
    public static class SpatiaLiteSample
    {
        private static string DataSource
        {
            get { return @"Data Source=GeoData\SpatiaLite\sample.sqlite"; }
        }
        
        
        public static SharpMap.Map InitializeMap(float angle)
        {
            //Initialize a new map of size 'imagesize'
            SharpMap.Map map = new SharpMap.Map();

            //Set up the countries layer
            var layCountries = new VectorLayer("Countries");

            //Set the datasource to a shapefile in the App_data folder
            layCountries.DataSource = new SharpMap.Data.Providers.SpatiaLite(
                DataSource, "countries", "geom", "PK_UID");

            //Set fill-style to green
            layCountries.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Green);
            //Set the polygons to have a black outline
            layCountries.Style.Outline = System.Drawing.Pens.Black;
            layCountries.Style.EnableOutline = true;

            //Set up a river layer
            var layRivers = new SharpMap.Layers.VectorLayer("Rivers");
            //Set the datasource to a shapefile in the App_data folder
            layRivers.DataSource = new SharpMap.Data.Providers.SpatiaLite(
                DataSource, "rivers", "geom", "PK_UID");
            //Define a blue 3px wide pen
            layRivers.Style.Line = new System.Drawing.Pen(System.Drawing.Color.LightBlue, 2);
            layRivers.Style.Line.CompoundArray = new[] { 0.2f, 0.8f };
            layRivers.Style.Outline = new System.Drawing.Pen(System.Drawing.Color.DarkBlue, 3);
            layRivers.Style.EnableOutline = true;

            //Set up a cities layer
            var layCities = new SharpMap.Layers.VectorLayer("Cities");
            //Set the datasource to the spatialite table
            layCities.DataSource = new SharpMap.Data.Providers.SpatiaLite(
                DataSource, "cities", "geom", "PK_UID");
            layCities.Style.SymbolScale = 0.8f;
            layCities.MaxVisible = 40;

            //Set up a country label layer
            SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels") 
            {
                DataSource = layCountries.DataSource,
                LabelColumn = "NAME",
                MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                PriorityColumn = "POPDENS",
                Style = new SharpMap.Styles.LabelStyle
                {
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, 12),
                    BackColor = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0)),
                    HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                    MaxVisible = 90,
                    MinVisible = 30,
                    CollisionDetection = true
                }
            };

            //Set up a city label layer
            SharpMap.Layers.LabelLayer layCityLabel = new SharpMap.Layers.LabelLayer("City labels")
            {
                DataSource = layCities.DataSource,
                LabelColumn = "name",
                PriorityColumn = "population",
                PriorityDelegate = delegate(SharpMap.Data.FeatureDataRow fdr) 
                { 
                    System.Int32 retVal = 10000000 * ( (System.String)fdr["capital"] == "Y" ? 1 : 0 );
                    return  retVal + System.Convert.ToInt32(fdr["population"]);
                },
                TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias,
                SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias,
                LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection,
                Style = new SharpMap.Styles.LabelStyle
                {
                    ForeColor = System.Drawing.Color.Black,
                    Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, 11),
                    MaxVisible = layLabel.MinVisible,
                    HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left,
                    VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Bottom,
                    Offset = new System.Drawing.PointF(3, 3),
                    Halo = new System.Drawing.Pen(System.Drawing.Color.Yellow, 2),
                    CollisionDetection = true
                }
            };

            var layRiverLabels = new SharpMap.Layers.LabelLayer("RiverLabels");
            layRiverLabels.DataSource = layRivers.DataSource;
            layRiverLabels.LabelColumn = "Name";
            layRiverLabels.PriorityDelegate = GetRiverLength;
            layRiverLabels.Style = new SharpMap.Styles.LabelStyle
                                       {
                                           Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                                           Halo = new System.Drawing.Pen(System.Drawing.Color.Azure, 2),
                                           ForeColor = System.Drawing.Color.DarkCyan,
                                           IgnoreLength = false,
                                           Enabled = true,
                                           CollisionDetection = true,
                                          
                                       };
            layRiverLabels.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;

            //Add the layers to the map object.
            //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
            map.Layers.Add(layCountries);
            map.Layers.Add(layRivers);
            map.Layers.Add(layCities);
            map.Layers.Add(layRiverLabels);
            map.Layers.Add(layLabel);
            map.Layers.Add(layCityLabel);

            //limit the zoom to 360 degrees width
            map.MaximumZoom = 360;
            map.BackColor = System.Drawing.Color.LightBlue;

            map.ZoomToExtents(); // = 360;

            var mat = new System.Drawing.Drawing2D.Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;
            return map;

        }
#if !OPENGLSAMPLE
        internal static SharpMap.Map InitializeMap(float angle, string[] filenames)
        {
            if (filenames == null)
                return null;

            var map = new SharpMap.Map();

            try
            {
                foreach (var filename in filenames)
                {
                    var connectionString = string.Format("Data Source={0}", filename);
                    foreach (var provider in SharpMap.Data.Providers.SpatiaLite.GetSpatialTables(connectionString))
                    {
                        map.Layers.Add(
                            new SharpMap.Layers.VectorLayer(
                                string.Format("{0} - {1}", provider.Table, provider.GeometryColumn), provider) { Style = LayerTools.GetRandomVectorStyle() });
                    }
                }
                if (map.Layers.Count > 0)
                {
                    map.ZoomToExtents();

                    System.Drawing.Drawing2D.Matrix mat = new System.Drawing.Drawing2D.Matrix();
                    mat.RotateAt(angle, map.WorldToImage(map.Center));
                    map.MapTransform = mat;
                    return map;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return null;
        }
#endif

        internal static int GetRiverLength(SharpMap.Data.FeatureDataRow row)
        {
            if (row == null) return 0;
            if (row.Geometry == null)
                return 0;
            var ml = row.Geometry as NetTopologySuite.Geometries.MultiLineString;
            if (ml != null)
                return System.Convert.ToInt32(ml.Length);

            var l = row.Geometry as NetTopologySuite.Geometries.LineString;
            if (l != null)
                return System.Convert.ToInt32(l.Length);
            return 0;
        }
    }
}
