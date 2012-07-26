using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Styles;
using SharpMap.Rendering.Thematics;
using BruTile;
using BruTile.Cache;
using BruTile.PreDefined;
using BruTile.Web;

#if DotSpatialProjections
using GeometryTransform = DotSpatial.Projections.GeometryTransform;
#else
using GeometryTransform = ProjNet.CoordinateSystems.Transformations.GeometryTransform;
#endif

namespace WinFormSamples
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            // this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //TileAsyncLayer osmLayer= new TileAsyncLayer(new OsmTileSource(), "TileLayer - OSM");
            TileAsyncLayer bingLayer = new TileAsyncLayer(new BingTileSource(BingRequest.UrlBing, "", BingMapType.Roads), "TileLayer - Bing");

            this.mapBox1.Map.BackgroundLayer.Add(bingLayer);
            var gf = new GeometryFactory(new PrecisionModel(), 3857);

#if DotSpatialProjections
            var mathTransform = LayerTools.Wgs84toGoogleMercator;
            var geom = GeometryTransform.TransformBox(
                new Envelope(-9.205626, -9.123736, 38.690993, 38.740837), 
                mathTransform.Source, mathTransform.Target);
#else
            var mathTransform = LayerTools.Wgs84toGoogleMercator.MathTransform;
            var geom = GeometryTransform.TransformBox(
                new Envelope(-9.205626, -9.123736, 38.690993, 38.740837),
                mathTransform);
#endif

            //Adds a pushpin layer
            VectorLayer pushPinLayer = new VectorLayer("PushPins");
            var geos = new List<IGeometry>();
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
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.Pan;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.ZoomWindow;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.ZoomOut;
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            this.mapBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TileLayer googleLayer = new TileAsyncLayer(new GoogleTileSource(new GoogleRequest(GoogleMapType.GoogleMap), new MemoryCache<byte[]>(100, 1000)), "TileLayer - Google");
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
            ILayer countries = new VectorLayer("countries", new ShapeFile("GeoData/World/countries.shp", true))
            {
                SRID = 900913,
                CoordinateTransformation = LayerTools.Wgs84toGoogleMercator,
                Style = new VectorStyle { EnableOutline = true, Fill = new SolidBrush(Color.Green) },
                SmoothingMode = SmoothingMode.AntiAlias
            };
            ILayer rivers = new VectorLayer("countries", new ShapeFile("GeoData/World/rivers.shp", true))
            {
                SRID = 900913,
                CoordinateTransformation = LayerTools.Wgs84toGoogleMercator,
                Style = new VectorStyle { Line = new Pen(Color.Blue) },
                SmoothingMode = SmoothingMode.AntiAlias
            };
            ILayer cities = new VectorLayer("countries", new ShapeFile("GeoData/World/cities.shp", true))
            {
                SRID = 900913,
                CoordinateTransformation = LayerTools.Wgs84toGoogleMercator,                
                SmoothingMode = SmoothingMode.AntiAlias
            };
            return new [] { countries, rivers, cities };
        }
    }
}
