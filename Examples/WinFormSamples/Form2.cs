using System.Data;
using System.Linq;
using BruTile.Predefined;
using SharpMap.Data;
using SharpMap.Rendering.Decoration;
using SharpMap.Rendering.Decoration.ScaleBar;

namespace WinFormSamples
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Windows.Forms;
    using BruTile;
    //using BruTile.PreDefined;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using GeoAPI.CoordinateSystems.Transformations;
    using SharpMap.Data.Providers;
    using SharpMap.Forms;
    using SharpMap.Layers;
    using SharpMap.Styles;

    public partial class Form2 : Form
    {
        private const double WebMercatorRadius = 6378137.0;
        private readonly Envelope _webMercatorEnv = new Envelope(-20037508.34,20037508.34,-20000000,20000000);
        private readonly Polygon  _webMercatorPoly = null;
        
        private const int PowerRangeMin = -5;
        private const int PowerRangeMax = 10;
        private readonly double[] _niceNumberArray = {1, 2, 2.5, 5};
        
        public Form2()
        {
            this.InitializeComponent();
            // this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            
            _webMercatorPoly = EnvToPolygon(_webMercatorEnv);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var tileLayer = new TileAsyncLayer(KnownTileSources.Create(KnownTileSource.BingRoadsStaging), "TileLayer");

            this.mapBox1.Map.BackgroundLayer.Add(tileLayer);
            GeometryFactory gf = new GeometryFactory(new PrecisionModel(), 3857);

            IMathTransform mathTransform = LayerTools.Wgs84toGoogleMercator.MathTransform;
            Envelope geom = GeometryTransform.TransformBox(
                new Envelope(-9.205626, -9.123736, 38.690993, 38.740837),
                mathTransform);

            //Adds a pushpin layer
            VectorLayer pushPinLayer = new VectorLayer("PushPins");
            List<IGeometry> geos = new List<IGeometry>();
            geos.Add(gf.CreatePoint(geom.Centre));
            GeometryProvider geoProvider = new GeometryProvider(geos);
            pushPinLayer.DataSource = geoProvider;
            //this.mapBox1.Map.Layers.Add(pushPinLayer);

            // ScaleGraticule
            this.mapBox1.Map.Layers.AddCollection(CreateGraticuleLayers());

            // ScaleBar
            this.mapBox1.Map.SRID = 3857;
            this.mapBox1.Map.Decorations.Add(
                new ScaleBar
                {
                    Anchor = MapDecorationAnchor.RightCenter,
                    Enabled = chkScaleBar.Checked
                });

            this.mapBox1.Map.ZoomToBox(geom);
            this.mapBox1.Map.Zoom = 8500;
            this.mapBox1.Refresh();
        }

        private LayerCollection CreateGraticuleLayers()
        {
            var layers = new LayerCollection();
            
            // Graticule Layer
            var graticuleLyr = new VectorLayer("Graticule");
            graticuleLyr.DataSource = CreateGraticuleDatasource();
            graticuleLyr.Style.Line.Color = Color.DarkSlateGray;
            var majorIntStyle = new VectorStyle()
            {
                Line = new System.Drawing.Pen(Brushes.DarkBlue, 2)
            };

            var minorIntStyle = new VectorStyle()
            {
                Line = new System.Drawing.Pen(Brushes.DarkSlateGray, 1){DashStyle = DashStyle.Dash}
            };

            //Create the theme items
            var dictStyles = new Dictionary<bool, SharpMap.Styles.IStyle>();
            dictStyles.Add(true, majorIntStyle);
            dictStyles.Add(false, minorIntStyle);

            //Assign the theme
            graticuleLyr.Theme =
                new SharpMap.Rendering.Thematics.UniqueValuesTheme<bool>("Major", dictStyles, minorIntStyle);
            layers.Add(graticuleLyr);

            // labels
            var labelLyr = new LabelLayer("Graticule Labels");
            labelLyr.DataSource = graticuleLyr.DataSource;
            labelLyr.LabelColumn = "Label";
            labelLyr.LabelStringDelegate = row => ((bool)row["Major"] ? (string)row["Label"] : "");

            var labelStyle = (LabelStyle) labelLyr.Style;
            labelStyle.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
            labelStyle.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;

//            labelLyr.Style = new LabelStyle();
//            labelLyr.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.SimpleCollisionDetection;
//            labelLyr.Style.CollisionDetection = true;
//            labelLyr.Style.CollisionBuffer = new SizeF(2, 2);
//            labelLyr.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
//            labelLyr.Styletyle.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;


            layers.Add(labelLyr);
            
            return layers;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = MapBox.Tools.Pan;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = MapBox.Tools.ZoomWindow;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = MapBox.Tools.ZoomOut;
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            this.mapBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var googleLayer = new TileAsyncLayer(KnownTileSources.Create(KnownTileSource.BingHybridStaging),
                "TileLayer - Bing");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(googleLayer);
            this.mapBox1.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TileAsyncLayer bingLayer = new TileAsyncLayer(KnownTileSources.Create(KnownTileSource.BingRoadsStaging),
                "TileLayer - Bing");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(bingLayer);
            this.mapBox1.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TileAsyncLayer osmLayer = new TileAsyncLayer(KnownTileSources.Create(KnownTileSource.OpenStreetMap),
                "TileLayer - OSM");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(osmLayer);
            this.mapBox1.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ITileSchema schema = new GlobalSphericalMercator();
            ILayer[] layers = CreateLayers();
            SharpMapTileSource source = new SharpMapTileSource(schema, layers);
            TileAsyncLayer osmLayer = new TileAsyncLayer(source, "TileLayer - SharpMap");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(osmLayer);
            this.mapBox1.Refresh();
        }

        private static ILayer[] CreateLayers()
        {
            ILayer countries = CreateLayer("GeoData/World/countries.shp",
                new VectorStyle {EnableOutline = true, Fill = new SolidBrush(Color.Green)});
            ILayer rivers = CreateLayer("GeoData/World/rivers.shp", new VectorStyle {Line = new Pen(Color.Blue)});
            ILayer cities = CreateLayer("GeoData/World/cities.shp", new VectorStyle());
            return new[] {countries, rivers, cities};
        }

        private static ILayer CreateLayer(string path, VectorStyle style)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
                throw new FileNotFoundException("file not found", path);

            string full = file.FullName;
            string name = Path.GetFileNameWithoutExtension(full);
            ILayer layer = new VectorLayer(name, new ShapeFile(full, true))
            {
                SRID = 4326,
                //CoordinateTransformation = LayerTools.Wgs84toGoogleMercator,
                TargetSRID = 3857,
                Style = style,
                SmoothingMode = SmoothingMode.AntiAlias
            };
            return layer;
        }

        private void chkScaleBar_Checked(object sender, EventArgs e)
        {
            var scaleBar = mapBox1.Map.Decorations[0] as ScaleBar;
            if (scaleBar == null) return;
            scaleBar.Enabled = chkScaleBar.Checked;
            this.mapBox1.Refresh();
        }

        private void chkGraticule_Checked(object sender, EventArgs e)
        {
            foreach (var lyr in this.mapBox1.Map.Layers)
                if (lyr.LayerName.StartsWith("Graticule"))
                    lyr.Enabled = chkGraticule.Checked;
            
            if (chkGraticule.Checked)
                if (chkGraticule.Checked) DrawGraticule();
            
            this.mapBox1.Refresh();
        }

        private GeometryFeatureProvider CreateGraticuleDatasource()
        {
            var fdt = new FeatureDataTable {TableName = "Graticule"};
            fdt.Columns.Add(new DataColumn("oid", typeof(uint)));
            fdt.Columns[0].AutoIncrement = true;

            fdt.Columns.Add(new DataColumn("Label", typeof(string)));
            fdt.Columns.Add(new DataColumn("Major", typeof(bool))); 

            fdt.PrimaryKey = new[] {fdt.Columns[0]};

            return new GeometryFeatureProvider(fdt);

        }
        
        private void DrawGraticule()
        {
            if (!chkGraticule.Checked) return;

            var mapExtentsEnv = this.mapBox1.Map.Envelope;
            var mapExtentsPoly = EnvToPolygon(mapExtentsEnv); 

            //var minSide = Math.Min(extents.Width, extents.Height);
            var majorInt = CalcGraticuleInterval(mapExtentsEnv.Height / 2);
            var minorInt = majorInt / 10;
            this.chkGraticule.Text = $"Graticule ({minorInt:N0}m)";

            var constrExtents = CalcContructionExtents(mapExtentsEnv, minorInt);
            
            var graticuleLyr = (VectorLayer) mapBox1.Map.Layers.GetLayerByName("Graticule");
            var gfp = (GeometryFeatureProvider) graticuleLyr.DataSource;
            var fdt = gfp.Features;
            lock (fdt.Rows.SyncRoot)
            {
                gfp.Features.Clear();
                gfp.Features.BeginLoadData();

                // horizontal graticule CONSTRAINED to both Map extents AND _webMercatorExtents
                for (var thisY = Math.Max(constrExtents.MinY, _webMercatorEnv.MinY);
                    thisY <= Math.Min(constrExtents.MaxY, _webMercatorEnv.MaxY);
                    thisY += minorInt)
                {
                    if (thisY < mapExtentsEnv.MinY || thisY > mapExtentsEnv.MaxY) continue;
                    
                    var fdr = gfp.Features.NewRow();
                    fdr["Label"] = $"  {thisY:N0}";
                    fdr["Major"] = (thisY % majorInt == 0) ? true: false;
                    fdr.Geometry = new LineString(
                        new[]
                        {
                            // NB intentional use of mapExtentsEnv (instead of constrExtents) 
                            new Coordinate(Math.Max(mapExtentsEnv.MinX, _webMercatorEnv.MinX), thisY),
                            new Coordinate(Math.Min(mapExtentsEnv.MaxX, _webMercatorEnv.MaxX), thisY)
                        }
                    );
                    gfp.Features.AddRow(fdr);
                }
             
                // vertical graticule CLIPPED to _webMercatorPoly AND _mapExtentsPoly
                for (var thisX = constrExtents.MinX; thisX <= constrExtents.MaxX; thisX += minorInt)
                {
                    var geom = ConstructMeridianGraticule(thisX, minorInt, constrExtents, mapExtentsPoly);
                    if (geom == null || geom.IsEmpty) continue;
                    
                    var fdr = gfp.Features.NewRow();
                    fdr["Label"] = $"  {thisX:N0}";
                    fdr["Major"] = (thisX % majorInt == 0) ? true: false;
                    fdr.Geometry = geom;
                    gfp.Features.AddRow(fdr);
                  
                }

                gfp.Features.EndLoadData();
            }
        }

        private Polygon EnvToPolygon(Envelope env)
        {
            var linearRing = new LinearRing(new Coordinate[]
            {
                new Coordinate(env.BottomLeft()),
                new Coordinate(env.TopLeft()),
                new Coordinate(env.TopRight()),
                new Coordinate(env.BottomRight()),
                new Coordinate(env.BottomLeft())
            });
            return new Polygon(linearRing);
        }

        private Envelope CalcContructionExtents(Envelope extents, double minorInt)
        {

            // Y extents expanded to next increment of minorInt
            var minY= Math.Floor(extents.MinY / minorInt) * minorInt;
            if (extents.MinY < 0) minY -= minorInt;

            var maxY = Math.Ceiling(extents.MaxY / minorInt) * minorInt;
            if (extents.MaxY < 0) maxY += minorInt;

            // X extents more complicated
            double lowerLeftX, upperLeftX, upperRightX, lowerRightX;

            if (Math.Max(extents.MinX, _webMercatorEnv.MinX) < 0 && 
                Math.Max(extents.MinY, _webMercatorEnv.MinY) < 0 && 
                Math.Min(extents.MaxY , _webMercatorEnv.MaxY) >= 0)
            {
                // LHS extents lies to the left of central meridian with vt extent spanning equator
                lowerLeftX = upperLeftX = Math.Max(extents.MinX, _webMercatorEnv.MinX);
            }
            else
            {
                lowerLeftX = CalcEquatorialX(extents.BottomLeft());
                upperLeftX = CalcEquatorialX(extents.TopLeft());
            }
            
            if (Math.Min(extents.MaxX, _webMercatorEnv.MaxX) >= 0 && 
                Math.Max(extents.MinY, _webMercatorEnv.MinY) < 0 && 
                Math.Min(extents.MaxY , _webMercatorEnv.MaxY) >= 0)
            {
                // RHS extents lists to right of central meridian with vt extent spanning equator
                lowerRightX = upperRightX = Math.Min(extents.MaxX, _webMercatorEnv.MaxX);
            }
            else
            {
                lowerRightX = CalcEquatorialX(extents.BottomRight());
                upperRightX = CalcEquatorialX(extents.TopRight());
            }

            // X extents expanded to next largest increment of minorInt 
            var minX = Math.Floor(Math.Abs(Math.Min(lowerLeftX, upperLeftX)) / minorInt) * minorInt;
            if (Math.Min(lowerLeftX, upperLeftX) < 0) minX *= -1;
            
            var maxX = Math.Ceiling(Math.Abs(Math.Max(lowerRightX, upperRightX)) / minorInt) * minorInt;
            if (Math.Max(lowerRightX, upperRightX) < 0) maxX *= -1;
            
            // envelope MAY exceed _webMercatorExtents. Dangles will be clipped when drawing geometry. 
            return new Envelope(minX, maxX, minY, maxY);
 
        }

        private double CalcEquatorialX(Coordinate coord)
        {
            var scaleFactor = Math.Cosh(Math.Abs(coord.Y) / WebMercatorRadius);
            return coord.X / scaleFactor;
        }

        private double CalcScaleCorrectedX(double x, double y)
        {
            var scaleFactor = Math.Cosh(Math.Abs(y) / WebMercatorRadius);
            return x * scaleFactor;
        }

        private IGeometry ConstructMeridianGraticule(double thisX, double minorInt, Envelope constrExtents, Polygon mapExtents)
        {
            var coords = new CoordinateList();

            var thisY = constrExtents.MinY;
            while (thisY <= constrExtents.MaxY)
            {
                coords.Add(new Coordinate(CalcScaleCorrectedX(thisX, thisY), thisY));
                thisY += minorInt;
            }

            var graticule = (IGeometry) new LineString(coords.ToArray());
            if (!_webMercatorEnv.Contains(graticule.EnvelopeInternal))
                graticule = _webMercatorPoly.Intersection(graticule);
            
            if (!mapExtents.EnvelopeInternal.Contains(graticule.EnvelopeInternal))
                graticule = mapExtents.Intersection(graticule);

            return graticule;
        }

        private double CalcGraticuleInterval(double maxInterval)
        {
            int y; //power of 10. Range of -5 to 10 gives huge scale range.
            double candidate = 0d; //Candidate value for new interval.
            for (y = PowerRangeMax; y >= PowerRangeMin; y--)
            {
                double multiplier = Math.Pow(10, y); //Mulitiplier, =10^exp, to apply to nice numbers.
                foreach (var niceNumber in _niceNumberArray.Reverse())
                {
                    candidate = niceNumber * multiplier;
                    if (candidate <= maxInterval)
                        return candidate;
                }
            }

            return candidate; //return the maximum
        }
    }
}
