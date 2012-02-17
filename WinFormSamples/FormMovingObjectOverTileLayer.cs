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
using WinFormSamples.Properties;

#if DotSpatialProjections
using GeometryTransform = DotSpatial.Projections.GeometryTransform;
#else
using GeometryTransform = ProjNet.CoordinateSystems.Transformations.GeometryTransform;
#endif

namespace WinFormSamples
{
    public partial class FormMovingObjectOverTileLayer : Form
    {

        private List<SharpMap.Geometries.Geometry> geos = new List<SharpMap.Geometries.Geometry>();

        private bool movingUp = true;
        private bool movingLeft = true;
        SharpMap.Geometries.Point position;

        public FormMovingObjectOverTileLayer()
        {
   
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        private void FormMovingObjectOverTileLayer_Load(object sender, EventArgs e)
        {

            //Lisbon...
#if DotSpatialProjections
            var mathTransform = LayerTools.Wgs84toGoogleMercator;
            SharpMap.Geometries.BoundingBox geom = GeometryTransform.TransformBox(
                new SharpMap.Geometries.BoundingBox(-9.205626, 38.690993, -9.123736, 38.740837),
                mathTransform.Source, mathTransform.Target);
#else
            var mathTransform = LayerTools.Wgs84toGoogleMercator.MathTransform;
            SharpMap.Geometries.BoundingBox geom = GeometryTransform.TransformBox(
                new SharpMap.Geometries.BoundingBox(-9.205626, 38.690993, -9.123736, 38.740837),
                mathTransform);
#endif


            //Google Background
            TileAsyncLayer layer2 = new TileAsyncLayer(new OsmTileSource(), "TileLayer - OSM");


            this.mapBox1.Map.BackgroundLayer.Add(layer2);

            //Adds a static layer
            VectorLayer staticLayer = new VectorLayer("Fixed Marker");
            //position = geom.GetCentroid();
            List<SharpMap.Geometries.Geometry> aux = new List<SharpMap.Geometries.Geometry>();
            aux.Add(geom.GetCentroid());
            staticLayer.Style.Symbol = Resources.PumpSmall;
            SharpMap.Data.Providers.GeometryProvider geoProviderFixed = new SharpMap.Data.Providers.GeometryProvider(aux);
            staticLayer.DataSource = geoProviderFixed;
            this.mapBox1.Map.Layers.Add(staticLayer);

            
            //Adds a moving variable layer
            VectorLayer pushPinLayer = new VectorLayer("PushPins");
            position = geom.GetCentroid();
            geos.Add(position);
            pushPinLayer.Style.Symbol = Resources.OutfallSmall;
            SharpMap.Data.Providers.GeometryProvider geoProvider = new SharpMap.Data.Providers.GeometryProvider(geos);
            pushPinLayer.DataSource = geoProvider;
            this.mapBox1.Map.VariableLayers.Add(pushPinLayer);

            this.mapBox1.Map.ZoomToBox(geom);
            this.mapBox1.Refresh();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double dx, dy;
            if (movingLeft)
                dx = -100;
            else
                dx = 100;

            if (movingUp)
                dy = 100;
            else
                dy = -100;

            position.X = position.X + dx;
            position.Y = position.Y + dy;

            if (position.X < this.mapBox1.Map.Envelope.Left)
                movingLeft = false;
            else if (position.X > this.mapBox1.Map.Envelope.Right)
                movingLeft = true;

            if (position.Y < this.mapBox1.Map.Envelope.Bottom)
                movingUp = true;
            else if (position.Y > this.mapBox1.Map.Envelope.Top)
                movingUp = false;

            VariableLayerCollection.TouchTimer();
            //this.mapBox1.Refresh();

        }


        private void button4_Click(object sender, EventArgs e)
        {
            this.timer1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.timer1.Enabled = false;
        }


        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            this.mapBox1.Refresh();
        }

        private void FormMovingObjectOverTileLayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.timer1.Stop();
        }


    }
}
