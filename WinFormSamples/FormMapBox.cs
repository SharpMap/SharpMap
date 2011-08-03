using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpMap.Forms;
using SharpMap.Layers;

namespace WinFormSamples
{
    public partial class FormMapBox : Form
    {
        private static readonly Dictionary<string, Type> MapDecorationTypes = new Dictionary<string, Type>();
        private static bool AddToListView;

        public FormMapBox()
        {
            AddToListView = false;
            AppDomain.CurrentDomain.AssemblyLoad += HandleAssemblyLoad;

            InitializeComponent();
            mapBox1.ActiveTool = MapBox.Tools.Pan;

            AddToListView = true;
            foreach (var name in MapDecorationTypes.Keys)
            {
                lvwDecorations.Items.Add(name);
            }
            pgMapDecoration.SelectedObject = null;

        }

        private void HandleAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var mdtype = typeof (SharpMap.Rendering.Decoration.IMapDecoration);
            foreach (Type type in args.LoadedAssembly.GetTypes())
            {
                //if (type.FullName.StartsWith("SharpMap.Decoration"))
                //    Console.WriteLine(type.FullName);
                if (mdtype.IsAssignableFrom(type))
                {
                    if (!type.IsAbstract)
                    {
                        if (AddToListView)
                            lvwDecorations.Items.Add(new ListViewItem(type.Name));
                        MapDecorationTypes.Add(type.Name, type);
                    }
                }
            }
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
    }
}