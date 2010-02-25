using System;
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
          //mapImage.ActiveTool = MapImage.Tools.None;
          string text = ((RadioButton)sender).Text;
        button1.Enabled = false;
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
          case "TileLayer OpenStreetMap":
            mapImage.Map = TileLayerSample.InitializeMap();
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
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, "Error");
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {

    }

    private void mapImage_MapQueried(SharpMap.Data.FeatureDataTable data)
    {
        dataGridView1.DataSource = data as System.Data.DataTable;
    }

  }
}