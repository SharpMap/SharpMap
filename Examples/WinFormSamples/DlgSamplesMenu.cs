using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinFormSamples
{
    public partial class DlgSamplesMenu : Form
    {
        public DlgSamplesMenu()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using(var f = new FormMapBox())
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

    }
}
