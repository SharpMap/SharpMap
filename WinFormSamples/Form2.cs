using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpMap.Layers;
using SharpMap.Data;
using SharpMap.Styles;
using SharpMap.Rendering.Thematics;
using BruTile.Web;

namespace WinFormSamples
{
    public partial class Form2 : Form
    {
        public Form2()
        {
   
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

            
            //TileAsyncLayer osmLayer= new TileAsyncLayer(new OsmTileSource(), "TileLayer - OSM");
            TileAsyncLayer bingLayer = new TileAsyncLayer(new BingTileSource(BingRequest.UrlBing, "",BingMapType.Roads), "TileLayer - Bing" );
            this.mapBox1.Map.BackgroundLayer.Add(bingLayer);

            SharpMap.Geometries.BoundingBox geom = ProjNet.CoordinateSystems.Transformations.GeometryTransform.TransformBox(new SharpMap.Geometries.BoundingBox(-9.205626, 38.690993, -9.123736, 38.740837), LayerTools.Wgs84toGoogleMercator.MathTransform);

            //Adds a pushpin layer
            VectorLayer pushPinLayer = new VectorLayer("PushPins");
            List<SharpMap.Geometries.Geometry> geos = new List<SharpMap.Geometries.Geometry>();
            geos.Add(geom.GetCentroid());
            SharpMap.Data.Providers.GeometryProvider geoProvider = new SharpMap.Data.Providers.GeometryProvider(geos);
            pushPinLayer.DataSource = geoProvider;
            this.mapBox1.Map.Layers.Add(pushPinLayer);



            this.mapBox1.Map.ZoomToBox(geom);
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
            TileLayer googleLayer = new TileAsyncLayer(new GoogleTileSource(new BruTile.Web.GoogleRequest(GoogleMapType.GoogleMap),new BruTile.Cache.MemoryCache<byte[]>(100,1000)), "TileLayer - Google");
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
            TileAsyncLayer osmLayer= new TileAsyncLayer(new OsmTileSource(), "TileLayer - OSM");
            this.mapBox1.Map.BackgroundLayer.Clear();
            this.mapBox1.Map.BackgroundLayer.Add(osmLayer);
            this.mapBox1.Refresh();
        }

    }
}
