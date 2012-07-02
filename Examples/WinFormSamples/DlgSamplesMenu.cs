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
            new FormMapBox().ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new DockAreaForm().ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            new FormDemoDrawGeometries().ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            new FormAnimation().ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new FormMovingObjectOverTileLayer().ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new MainForm().ShowDialog();
        }
    }
}
