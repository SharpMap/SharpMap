using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpMap.Forms;

namespace WinFormSamples
{
    public partial class DlgSamplesMenu : Form
    {
        public DlgSamplesMenu()
        {
            InitializeComponent();
            checkBox1.Checked = true;
            button2.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ds = sender == button3;
            FormMapBox.UseDotSpatial = ds;
            using (var f = new FormMapBox())
                f.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using(var f = new  DockAreaForm())
                f.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using(var f = new FormDemoDrawGeometries())
                f.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using(var f = new FormAnimation())
                f.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using(var f = new FormMovingObjectOverTileLayer())
                f.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (var f = new FormLayerListImageGenerator())
                f.ShowDialog();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                MapBox.MapImageGeneratorFunction = null;
            else 
                MapBox.MapImageGeneratorFunction = MapBox.LayerListImageGenerator;
        }


    }
}
