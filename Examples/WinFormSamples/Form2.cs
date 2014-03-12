#if DotSpatialProjections
using GeometryTransform = DotSpatial.Projections.GeometryTransform;
#else

#endif
using BruTile.Predefined;

namespace WinFormSamples
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Windows.Forms;

    using BruTile;
    using BruTile.Cache;
    //using BruTile.PreDefined;
    using BruTile.Web;

    using GeoAPI.Geometries;

    using NetTopologySuite.Geometries;

#if !DotSpatialProjections
    using GeoAPI.CoordinateSystems.Transformations;
#endif

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
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //TileAsyncLayer osmLayer= new TileAsyncLayer(new OsmTileSource(), "TileLayer - OSM");
            TileAsyncLayer bingLayer = new TileAsyncLayer(new BingTileSource(BingRequest.UrlBing, "", BingMapType.Roads), "TileLayer - Bing");

            this.mapBox1.Map.BackgroundLayer.Add(bingLayer);
            GeometryFactory gf = new GeometryFactory(new PrecisionModel(), 3857);

#if DotSpatialProjections
            var mathTransform = LayerTools.Wgs84toGoogleMercator;
            var geom = GeometryTransform.TransformBox(
                new Envelope(-9.205626, -9.123736, 38.690993, 38.740837), 
                mathTransform.Source, mathTransform.Target);
#else
            IMathTransform mathTransform = LayerTools.Wgs84toGoogleMercator.MathTransform;
            Envelope geom = GeometryTransform.TransformBox(
                new Envelope(-9.205626, -9.123736, 38.690993, 38.740837),
                mathTransform);
#endif

            //Adds a pushpin layer
            VectorLayer pushPinLayer = new VectorLayer("PushPins");
            List<IGeometry> geos = new List<IGeometry>();
            geos.Add(gf.CreatePoint(geom.Centre));
            GeometryProvider geoProvider = new GeometryProvider(geos);
            pushPinLayer.DataSource = geoProvider;
            //this.mapBox1.Map.Layers.Add(pushPinLayer);

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
            TileLayer googleLayer = new TileAsyncLayer(new BingTileSource(new BingRequest(BingRequest.UrlBingStaging, string.Empty, BingMapType.Hybrid )), "TileLayer - Bing");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(googleLayer);
            this.mapBox1.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {

            TileAsyncLayer bingLayer = new TileAsyncLayer(new BingTileSource(BingRequest.UrlBing, "", BingMapType.Roads), "TileLayer - Bing");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(bingLayer);
            this.mapBox1.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TileAsyncLayer osmLayer = new TileAsyncLayer(new OsmTileSource(), "TileLayer - OSM");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(osmLayer);
            this.mapBox1.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ITileSchema schema = new SphericalMercatorInvertedWorldSchema();
            ILayer[] layers = CreateLayers();
            SharpMapTileSource source = new SharpMapTileSource(schema, layers);
            TileAsyncLayer osmLayer = new TileAsyncLayer(source, "TileLayer - SharpMap");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(osmLayer);
            this.mapBox1.Refresh();
        }

        private static ILayer[] CreateLayers()
        {
            ILayer countries = CreateLayer("GeoData/World/countries.shp", new VectorStyle { EnableOutline = true, Fill = new SolidBrush(Color.Green) });
            ILayer rivers = CreateLayer("GeoData/World/rivers.shp", new VectorStyle { Line = new Pen(Color.Blue) });
            ILayer cities = CreateLayer("GeoData/World/cities.shp", new VectorStyle());
            return new[] { countries, rivers, cities };
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
                SRID = 900913,
                CoordinateTransformation = LayerTools.Wgs84toGoogleMercator,
                Style = style,
                SmoothingMode = SmoothingMode.AntiAlias
            };
            return layer;
        }
    }
}
