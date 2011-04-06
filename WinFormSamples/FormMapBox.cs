using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SharpMap.Forms;
using SharpMap.Layers;
using WinFormSamples.Samples;

namespace WinFormSamples
{
    public partial class FormMapBox : Form
    {
        public FormMapBox()
        {
            InitializeComponent();
            mapBox1.ActiveTool = MapBox.Tools.Pan;
        }

        private void radioButton_Click(object sender, EventArgs e)
        {
            Cursor mic = mapBox1.Cursor;
            mapBox1.Cursor = Cursors.WaitCursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                //mapImage.ActiveTool = MapImage.Tools.None;
                string text = ((RadioButton)sender).Text;

                switch (text)
                {
                    case "Shapefile":
                        mapBox1.Map = ShapefileSample.InitializeMap(tbAngle.Value);
                        break;
                    case "GradientTheme":
                        mapBox1.Map = GradiantThemeSample.InitializeMap(tbAngle.Value);
                        break;
                    case "WFS Client":
                        mapBox1.Map = WfsSample.InitializeMap(tbAngle.Value);
                        break;
                    case "WMS Client":
                        mapBox1.Map = TiledWmsSample.InitializeMap();
                        //mapBox1.Map = WmsSample.InitializeMap(tbAngle.Value);
                        break;
                    case "OGR - MapInfo":
                    case "OGR - S-57":
                        mapBox1.Map = OgrSample.InitializeMap(tbAngle.Value);
                        break;
                    case "GDAL - GeoTiff":
                    case "GDAL - '.DEM'":
                    case "GDAL - '.ASC'":
                    case "GDAL - '.VRT'":
                        mapBox1.Map = GdalSample.InitializeMap(tbAngle.Value);
                        mapBox1.ActiveTool = MapBox.Tools.Pan;
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
                        mapBox1.Map = TileLayerSample.InitializeMap(tbAngle.Value);
                        ((RadioButton)sender).Text = mapBox1.Map.Layers[0].LayerName;
                        break;
                    case "PostGis":
                        mapBox1.Map = PostGisSample.InitializeMap(tbAngle.Value);
                        break;
                    case "SpatiaLite":
                        mapBox1.Map = SpatiaLiteSample.InitializeMap(tbAngle.Value);
                        break;
                    case "Oracle":
                        mapBox1.Map = OracleSample.InitializeMap(tbAngle.Value);
                        break;
                    default:
                        break;
                }
                mapBox1.Map.Size = Size;
                mapBox1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }
            Cursor = Cursors.Default;
            mapBox1.Cursor = mic;
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
            mapBox1.Refresh();
        }

        private void tbAngle_Scroll(object sender, EventArgs e)
        {
            System.Drawing.Drawing2D.Matrix matrix = new Matrix(); 
            if (tbAngle.Value != 0f)
                matrix.RotateAt(tbAngle.Value, new PointF(mapBox1.Width * 0.5f, mapBox1.Height * 0.5f));

            mapBox1.Map.MapTransform = matrix;
            AdjustRotation(mapBox1.Map.Layers, tbAngle.Value);
            AdjustRotation(mapBox1.Map.VariableLayers, tbAngle.Value);

            mapBox1.Refresh();
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

    }
}