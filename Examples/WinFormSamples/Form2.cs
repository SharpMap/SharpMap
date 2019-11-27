using System.Data;
using System.Linq;
using BruTile.Predefined;
using SharpMap.Data;
using SharpMap.Rendering.Decoration;
using SharpMap.Rendering.Decoration.Graticule;
using SharpMap.Rendering.Decoration.ScaleBar;
using WinFormSamples.Samples;

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
        public Form2()
        {
            this.InitializeComponent();
            // this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.mapBox1.ShowProgressUpdate = true;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var tileLayer = new TileAsyncLayer(KnownTileSources.Create(KnownTileSource.BingRoadsStaging, userAgent:TileLayerSample.DefaultUserAgent), "TileLayer");

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

            // ScaleBar
            this.mapBox1.Map.SRID = 3857;
            this.mapBox1.Map.Decorations.Add(
                new ScaleBar
                {
                    Anchor = MapDecorationAnchor.RightCenter,
                    Enabled = chkScaleBar.Checked
                });

            // Graticule
            this.mapBox1.Map.Decorations.Add(new Graticule()
            {
                Enabled =  chkGraticule.Checked,
                PcsGraticuleMode = PcsGraticuleMode.WebMercatorScaleLines,
                PcsGraticuleStyle =
                {
                    NumSubdivisions = 2
                }
            });
            
            this.mapBox1.Map.ZoomToBox(geom);
            this.mapBox1.Map.Zoom = 8500;
            this.mapBox1.Refresh();
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
            var graticule = mapBox1.Map.Decorations[1] as Graticule;
            if (graticule == null) return;
            graticule.Enabled = chkGraticule.Checked;
            this.mapBox1.Refresh();
        }
    }
}
