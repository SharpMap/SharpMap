using System;
using System.Windows.Forms;
using SharpMap.Forms;
using SharpMap.Layers;

namespace WinFormSamples
{
    public partial class FormMapBox : Form
    {
        public FormMapBox()
        {
            InitializeComponent();
            mapBox1.ActiveTool = MapBox.Tools.Pan;
        }


        private void UpdatePropertyGrid()
        {
            pgMap.Update();
        }

        private static void AdjustRotation(LayerCollection lc, float angle)
        {
            foreach (ILayer layer in lc)
            {
                if (layer is VectorLayer)
                    ((VectorLayer) layer).Style.SymbolRotation = -angle;
                else if (layer is LabelLayer)
                    ((LabelLayer)layer).Style.Rotation = -angle;
            }
        }

        private static string[] GetOpenFileName(string filter)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.ShowReadOnly = false;
                ofd.Multiselect = true;

                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                    return ofd.FileNames;
                return null;
            }
        }

        private void radioButton2_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}