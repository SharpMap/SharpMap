using System;
using System.Windows.Forms;
using SharpMap.Forms;
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
                string text = ((RadioButton) sender).Text;
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
                        mapImage.Map = OgrSample.InitializeMap();
            break;
          case "GDAL - GeoTiff":
                        mapImage.Map = GdalSample.InitializeMap();
            break;
          case "Tiled WMS":
                        mapImage.Map = TiledWmsSample.InitializeMap();
            break;
          case "PostGis":
            this.mapImage.Map = Samples.PostGisSample.InitializeMap();
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
  }
}