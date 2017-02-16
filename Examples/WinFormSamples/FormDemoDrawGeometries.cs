using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GeoAPI.Geometries;
using SharpMap.Layers;
using SharpMap.Data;
using SharpMap.Styles;
using SharpMap.Rendering.Thematics;
using BruTile.Web;
using BruTile.Predefined;
using SharpMap.Data.Providers;

using GeometryTransform = GeoAPI.CoordinateSystems.Transformations.GeometryTransform;

namespace WinFormSamples
{
    public partial class FormDemoDrawGeometries : Form
    {

        private SharpMap.Data.Providers.GeometryProvider geoProvider;

        public FormDemoDrawGeometries()
        {
   
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        private void FormDemoDrawGeometries_Load(object sender, EventArgs e)
        {



            //this.mapBox1.Map = ShapefileSample.InitializeMap(0);
            //Google Background
            TileAsyncLayer bingLayer = new TileAsyncLayer(KnownTileSources.Create(KnownTileSource.BingRoadsStaging), "TileLayer - Bing");
            this.mapBox1.Map.BackgroundLayer.Add(bingLayer);


            SharpMap.Layers.VectorLayer vl = new VectorLayer("My Geometries");
            geoProvider = new SharpMap.Data.Providers.GeometryProvider(new List<IGeometry>());
            vl.DataSource = geoProvider;
            this.mapBox1.Map.Layers.Add(vl);

            var mathTransform = LayerTools.Wgs84toGoogleMercator.MathTransform;
            var geom = GeometryTransform.TransformBox(
                new Envelope(-9.205626, -9.123736, 38.690993, 38.740837),
                mathTransform);

            this.mapBox1.Map.ZoomToExtents(); //(geom);
            this.mapBox1.Refresh();

            this.mapBox1.GeometryDefined += new SharpMap.Forms.MapBox.GeometryDefinedHandler(mapBox1_GeometryDefined);

            this.mapBox1.ActiveToolChanged += new SharpMap.Forms.MapBox.ActiveToolChangedHandler(mapBox1_ActiveToolChanged);

            this.mapBox1.MouseMove += new SharpMap.Forms.MapBox.MouseEventHandler(mapBox1_MouseMove);
        }

        void mapBox1_MouseMove(GeoAPI.Geometries.Coordinate worldPos, MouseEventArgs imagePos)
        {
            this.label2.Text = worldPos.X.ToString("N4") + "/" + worldPos.Y.ToString("N4");
        }

        void mapBox1_ActiveToolChanged(SharpMap.Forms.MapBox.Tools tool)
        {
            this.label1.Text = this.mapBox1.ActiveTool.ToString();
        }

        void mapBox1_GeometryDefined(GeoAPI.Geometries.IGeometry geometry)
        {
            MessageBox.Show("Geometry defined!\r\n"+geometry);

            geoProvider.Geometries.Add(geometry);

            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.Pan;
            this.mapBox1.Refresh();

        }



        private void button4_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.Pan;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.ZoomIn;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.ZoomOut;
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            this.mapBox1.Refresh();
        }

          private void button7_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.DrawPoint;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.DrawLine;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.DrawPolygon;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.geoProvider.Geometries.Clear();
            this.mapBox1.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.ZoomWindow;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
        }
    }
}
