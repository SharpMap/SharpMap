using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Layers;
using BruTile.Predefined;

using WinFormSamples.Properties;

using GeometryTransform = GeoAPI.CoordinateSystems.Transformations.GeometryTransform;

namespace WinFormSamples
{
    public partial class FormMovingObjectOverTileLayer : Form
    {

        private List<IGeometry> geos = new List<IGeometry>();

        private bool movingUp = true;
        private bool movingLeft = true;
        GeoAPI.Geometries.Coordinate position;

        public FormMovingObjectOverTileLayer()
        {

            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        private void FormMovingObjectOverTileLayer_Load(object sender, EventArgs e)
        {

            //Lisbon...
            var mathTransform = LayerTools.Wgs84toGoogleMercator.MathTransform;
            GeoAPI.Geometries.Envelope geom = GeometryTransform.TransformBox(
                new Envelope(-9.205626, -9.123736, 38.690993, 38.740837),
                mathTransform);

            var map = new SharpMap.Map();
            //Google Background
            TileAsyncLayer layer2 = new TileAsyncLayer(KnownTileSources.Create(KnownTileSource.BingRoads), "TileLayer - Bing");
            map.BackgroundLayer.Add(layer2);

            var gf = new GeometryFactory(new PrecisionModel(), 3857);

            //Adds a static layer
            var staticLayer = new VectorLayer("Fixed Marker");
            //position = geom.GetCentroid();
            var aux = new List<IGeometry>();
            aux.Add(gf.CreatePoint(geom.Centre));
            staticLayer.Style.Symbol = Resources.PumpSmall;
            var geoProviderFixed = new SharpMap.Data.Providers.GeometryProvider(aux);
            staticLayer.DataSource = geoProviderFixed;
            map.Layers.Add(staticLayer);


            //Adds a moving variable layer
            VectorLayer pushPinLayer = new VectorLayer("PushPins");
            position = geom.Centre;
            geos.Add(gf.CreatePoint(position));
            pushPinLayer.Style.Symbol = Resources.OutfallSmall;
            var geoProvider = new SharpMap.Data.Providers.GeometryProvider(geos);
            pushPinLayer.DataSource = geoProvider;
            map.VariableLayers.Add(pushPinLayer);

            map.ZoomToBox(geom);


            this.mapBox1.Map = map;

            this.mapBox1.Refresh();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            const double step = 25;
            
            double dx, dy;
            if (movingLeft)
                dx = -step;
            else
                dx = step;

            if (movingUp)
                dy = step;
            else
                dy = -step;

            position.X += dx;
            position.Y += dy;

            if (position.X < this.mapBox1.Map.Envelope.MinX)
                movingLeft = false;
            else if (position.X > this.mapBox1.Map.Envelope.MaxX)
                movingLeft = true;

            if (position.Y < this.mapBox1.Map.Envelope.MinY)
                movingUp = true;
            else if (position.Y > this.mapBox1.Map.Envelope.MaxY)
                movingUp = false;

            geos[0].GeometryChanged();

            //static method replaced by instance method
            //VariableLayerCollection.TouchTimer();
            this.mapBox1.Map.VariableLayers.TouchTimer();

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
