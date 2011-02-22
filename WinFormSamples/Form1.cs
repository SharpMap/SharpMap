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
                        mapImage.Map = ShapefileSample.InitializeMap(tbAngle.Value);
                        break;
                    case "GradientTheme":
                        mapImage.Map = GradiantThemeSample.InitializeMap(tbAngle.Value);
                        break;
                    case "WFS Client":
                        mapImage.Map = WfsSample.InitializeMap(tbAngle.Value);
                        break;
                    case "WMS Client":
                        mapImage.Map = WmsSample.InitializeMap(tbAngle.Value);
                        break;
                    case "OGR - MapInfo":
                    case "OGR - S-57":
                        mapImage.Map = OgrSample.InitializeMap(tbAngle.Value);
                        break;
                    case "GDAL - GeoTiff":
                    case "GDAL - '.DEM'":
                    case "GDAL - '.ASC'":
                    case "GDAL - '.VRT'":
                        mapImage.Map = GdalSample.InitializeMap(tbAngle.Value);
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
                        /*
                        tbAngle.Visible = text.Equals("TileLayer - GoogleLabels");
                        if (!tbAngle.Visible) tbAngle.Value = 0;
                         */
                        mapImage.Map = TileLayerSample.InitializeMap(tbAngle.Value);
                        ((RadioButton) sender).Text = mapImage.Map.Layers[0].LayerName;
                        break;
                    case "PostGis":
                        mapImage.Map = PostGisSample.InitializeMap(tbAngle.Value);
                        break;
                    case "SpatiaLite":
                        mapImage.Map = SpatiaLiteSample.InitializeMap(tbAngle.Value);
                        break;
                    case "Oracle":
                        mapImage.Map = OracleSample.InitializeMap(tbAngle.Value);
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

        private void mapImage_MapQueriedDataSet(SharpMap.Data.FeatureDataSet data)
        {
            dataGridView1.DataSource = data as System.Data.DataSet;
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
            if (tbAngle.Value != 0f)
                matrix.RotateAt(tbAngle.Value, new PointF(mapImage.Width * 0.5f, mapImage.Height*0.5f));

            mapImage.Map.MapTransform = matrix;
            AdjustRotation(mapImage.Map.Layers, tbAngle.Value);
            AdjustRotation(mapImage.Map.VariableLayers, tbAngle.Value);

            mapImage.Refresh();
        }

        private void AdjustRotation(LayerCollection lc, float angle)
        {
            foreach (ILayer layer in lc)
            {
                if (layer is VectorLayer)
                    ((VectorLayer) layer).Style.SymbolRotation = -angle;
                else if (layer is LabelLayer)
                    ((LabelLayer)layer).Style.Rotation = -angle;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new DockAreaForm().Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new FormDemoDrawGeometries().ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new FormAnimation().ShowDialog();
        }

    }
}