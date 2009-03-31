using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinFormSamples
{
  public partial class MainForm : Form
  {
    public MainForm()
    {
      InitializeComponent();
      this.mapImage.ActiveTool = SharpMap.Forms.MapImage.Tools.Pan;
    }

    private void radioButton_Click(object sender, EventArgs e)
    {
      try
      {
        string text = ((RadioButton)sender).Text;
        switch (text)
	      {
          case "Shapefile":
            this.mapImage.Map = Samples.ShapefileSample.InitializeMap();
            break;
          case "GradientTheme":
            this.mapImage.Map = Samples.GradiantThemeSample.InitializeMap();
            break;
          case "WFS Client":
            this.mapImage.Map = Samples.WfsSample.InitializeMap();
            break;
          case "WMS Client":
            this.mapImage.Map = Samples.WmsSample.InitializeMap();
            break;
          case "OGR - MapInfo":
            this.mapImage.Map = Samples.OgrSample.InitializeMap();
            break;
          case "GDAL - GeoTiff":
            this.mapImage.Map = Samples.GdalSample.InitializeMap();
            break;
          case "Tiled WMS":
            this.mapImage.Map = Samples.TiledWmsSample.InitializeMap();
            break;
		      default:
            break;
	      }
        this.mapImage.Map.Size = this.Size;
        this.mapImage.Refresh();
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, "Error");
      }
    }



  }
}
