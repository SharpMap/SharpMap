using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BruTile;
using NetTopologySuite.Geometries;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using SharpMap.Layers;
using BruTile.Web;
using BruTile.Predefined;

namespace XYZMap
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            FormAnimation_Load(this, EventArgs.Empty);
        }


        private void FormAnimation_Load(object sender, EventArgs e)
        {

            //string url = "http://192.168.200.228:7080/PBS/rest/services/MyPBSService1/MapServer";
            //TiledWmsLayer tiledWmsLayer = new TiledWmsLayer("MyPBSService1", url);
            //tiledWmsLayer.TileSetsActive.Add(tiledWmsLayer.TileSets["satellite"].Name);
            //mapBox1.Map.Layers.Add(tiledWmsLayer);

            //string url = "http://localhost:3000/GeoIndex_en/{z}/{x}/{y}.png";
            //string url = "http://192.168.200.228:7080/PBS/rest/services/MyPBSService1/MapServer";

            //HttpTileProvider provider = new  HttpTileProvider(new BasicRequest(url));
            //TileSchema schema = new TileSchema();
            //schema.Srs = "EPSG:3587";
            //schema.Format = "image/png";

            //TileSource src = new TileSource(provider, schema);
            //TileAsyncLayer yy = new TileAsyncLayer(src, "GeoIndex_en");                 
            ////TileAsyncLayer bingLayer = new TileAsyncLayer(tms, "TileLayer - Bing");




            //var tms = new ArcGisTileRequest(new Uri("http://192.168.200.228:7080/PBS/rest/services/MyPBSService1/MapServer"), ".png");
            //mapBox1.Map.BackgroundLayer.Add(new SharpMap.Layers.TileAsyncLayer(tms, "MyPBSService1"));

            //this.mapBox1.Map.BackgroundLayer.Add(tileLayer);
            //GeometryFactory gf = new GeometryFactory(new PrecisionModel(), 3857);

            //IMathTransform mathTransform = LayerTools.Wgs84toGoogleMercator.MathTransform;
            //Envelope geom = GeometryTransform.TransformBox(
            //    new Envelope(-9.205626, -9.123736, 38.690993, 38.740837),
            //    mathTransform);


            string url = "http://localhost:3000/GeoIndex_en/{z}/{x}/{y}.png";
            //string url = "http://192.168.200.228:7080/PBS/rest/services/MyPBSService1/MapServer";
            //string url = "http://192.168.200.228:7080/PBS/rest/services/MyPBSService1/MapServer/WMTS";

            //var request = new ArcGisTileRequest(new Uri(url), ".png");
            var request = new BasicRequest(url);


            HttpTileProvider provider = new HttpTileProvider(request);
            //var src = new TileSource(provider, new GlobalSphericalMercator("", YAxis.TMS, 7, 12, "MyPBSService1"));
            //var src = new TileSource(provider, new GlobalSphericalMercator());
            //HttpTileSource src = new HttpTileSource(new GlobalSphericalMercator(), request);

            //var src = new ArcGisTileSource(url, new GlobalSphericalMercator("png", YAxis.TMS, 7, 12, "MyPBSService1")) ;






            //var tileLayer = new TileAsyncLayer(src, "MyPBSService1");

            //this.mapBox1.Map.BackgroundLayer.Add(tileLayer);
            


        }
    }
}
