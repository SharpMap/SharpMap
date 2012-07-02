using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormSamples
{
    public partial class FormCreateTilesSample : Form
    {
        public FormCreateTilesSample()
        {
            InitializeComponent();
        }

        public SharpMap.Map Map { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            lblFolder.Text = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tiles");
            var asr = new AppSettingsReader();
            txtGoogleMapsApiKey.Text = (string) asr.GetValue("GoogleMapsApiKey", typeof(string));
            
            base.OnLoad(e);
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            fbdFolder.SelectedPath = lblFolder.Text;
            if (fbdFolder.ShowDialog() == DialogResult.OK)
            {
                lblFolder.Text = fbdFolder.SelectedPath;
            }
        }

        private void chkSampleWebPage_CheckedChanged(object sender, EventArgs e)
        {
            txtGoogleMapsApiKey.Enabled = chkSampleWebPage.Checked;
        }

        private void btnDo_Click(object sender, EventArgs e)
        {
            if (sender == btnDo)
            {
                var tileRange = txtZoomLevels.Text.Trim();
                if (string.IsNullOrEmpty(tileRange))
                    return;

                var parts = tileRange.Split(';');
                using (var cts = new Samples.CreateTilesSample(Map, chkMercator.Checked, lblFolder.Text))
                {
                    cts.Opacity = (float)tbOpacity.Value / 100f;

                    foreach (var part in parts)
                    {
                        var subParts = part.Split('-');
                        if (subParts.Length == 2)
                        {
                            int fromLevel;
                            if (!int.TryParse(subParts[0], out fromLevel))
                                return;
                            int toLevel;
                            if (!int.TryParse(subParts[1], out toLevel))
                                return;
                            for (var i = fromLevel; i <= toLevel; i++)
                                cts.SaveImagesAtLevel(i);
                        }
                        else
                        {
                            int level;
                            if (int.TryParse(part, out level))
                                cts.SaveImagesAtLevel(level);

                        }
                    }
                }

                if (chkSampleWebPage.Checked)
                {
                    var htmlPath = System.IO.Path.Combine(lblFolder.Text, "SharpMapTileDemo.html");
                    Samples.CreateTilesSample.CreateHtmlSamplePage(htmlPath, txtGoogleMapsApiKey.Text);
                    Process.Start(new Uri(htmlPath).AbsolutePath);
                }
            }
            Close();
        }
    }
}
