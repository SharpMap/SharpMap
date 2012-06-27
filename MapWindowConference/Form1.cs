using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MapWindowConference
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var map = mapBox1.Map = new SharpMap.Map();
            map.BackColor = System.Drawing.Color.White;
            map.BackgroundLayer.Add(new SharpMap.Layers.TileAsyncLayer(
                new BruTile.Web.OsmTileSource(
                    new BruTile.Web.OsmRequest(BruTile.Web.KnownOsmTileServers.Mapnic), 
                    new BruTile.Cache.FileCache(Application.CommonAppDataPath, "png")), "Mapnic"));
            map.ZoomToExtents();
        }

        private void mapBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
