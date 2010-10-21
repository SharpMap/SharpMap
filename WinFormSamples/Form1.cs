using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SharpMap.Forms;
using SharpMap.Layers;
using WinFormSamples.Samples;

namespace WinFormSamples
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            mapImage.ActiveTool = MapImage.Tools.Pan;
        }

        private void radioButton_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor mic = mapImage.Cursor;
                mapImage.Cursor = Cursors.WaitCursor;
                Cursor = Cursors.WaitCursor;
                //mapImage.ActiveTool = MapImage.Tools.None;
                string text = ((RadioButton)sender).Text;
                switch (text)
                {
                    case "Shapefile":
                        mapImage.Map = ShapefileSample.InitializeMap();
                        break;
                    case "GradientTheme":
                        mapImage.Map = GradiantThemeSample.InitializeMap();
                        break;
                    case "WFS Client":
                        mapImage.Map = WfsSample.InitializeMap();
                        break;
                    case "WMS Client":
                        mapImage.Map = WmsSample.InitializeMap();
                        break;
                    case "OGR - MapInfo":
                    case "OGR - S-57":
                        mapImage.Map = OgrSample.InitializeMap();
                        break;
                    case "GDAL - GeoTiff":
                    case "GDAL - '.DEM'":
                    case "GDAL - '.ASC'":
                    case "GDAL - '.VRT'":
                        mapImage.Map = GdalSample.InitializeMap();
                        mapImage.ActiveTool = MapImage.Tools.Pan;
                        break;
                    case "TileLayer - OSM":
                    case "TileLayer - Bing Roads":
                    case "TileLayer - Bing Aerial":
                    case "TileLayer - Bing Hybrid":
                    case "TileLayer - GoogleMap":
                    case "TileLayer - GoogleSatellite":
                    case "TileLayer - GoogleTerrain":
                    case "TileLayer - GoogleLabels":
                        tbAngle.Visible = text.Equals("TileLayer - GoogleLabels");
                        if (!tbAngle.Visible) tbAngle.Value = 0;
                        mapImage.Map = TileLayerSample.InitializeMap(tbAngle.Value);
                        ((RadioButton) sender).Text = mapImage.Map.Layers[0].LayerName;
                        break;
                    case "PostGis":
                        mapImage.Map = PostGisSample.InitializeMap();
                        break;
                    case "SpatiaLite":
                        mapImage.Map = SpatiaLiteSample.InitializeMap();
                        break;
                    case "Oracle":
                        mapImage.Map = OracleSample.InitializeMap();
                        break;
                    default:
                        break;
                }
                mapImage.Map.Size = Size;
                mapImage.Refresh();
                Cursor = Cursors.Default;
                mapImage.Cursor = mic;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }
        }

        private void mapImage_MapQueried(SharpMap.Data.FeatureDataTable data)
        {
            dataGridView1.DataSource = data as System.Data.DataTable;
        }

        private void UpdatePropertyGrid()
        {
            pgMap.Update();
        }
        private void mapImage_ActiveToolChanged(MapImage.Tools tool)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapCenterChanged(SharpMap.Geometries.Point center)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapRefreshed(object sender, EventArgs e)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapZoomChanged(double zoom)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapZooming(double zoom)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_SizeChanged(object sender, EventArgs e)
        {
            mapImage.Refresh();
        }

        private void tbAngle_Scroll(object sender, EventArgs e)
        {
            System.Drawing.Drawing2D.Matrix matrix = new Matrix(); 
            matrix.RotateAt(tbAngle.Value, new PointF(mapImage.Width * 0.5f, mapImage.Height*0.5f));
            mapImage.Map.MapTransform = matrix;
            mapImage.Refresh();
        }

    }
}