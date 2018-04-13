using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Forms;
using SharpMap.Forms.Tools;
using SharpMap.Layers;

namespace WinFormSamples
{
    public partial class FormMapBox : Form
    {
        private static readonly Dictionary<string, Type> MapDecorationTypes = new Dictionary<string, Type>();
        private static bool AddToListView;

        public static bool UseDotSpatial
        {
            get
            {
                return Session.Instance.CoordinateSystemServices.GetCoordinateSystem(4326) is DotSpatialCoordinateSystem;
            }
            set
            {
                if (value == UseDotSpatial) return;

                var s = (Session) Session.Instance;
                var css = !value 
                    ? new CoordinateSystemServices(
                        new CoordinateSystemFactory(),
                        new CoordinateTransformationFactory(),
                        SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems())
                    : new CoordinateSystemServices(
                        new DotSpatialCoordinateSystemFactory(), 
                        new DotSpatialCoordinateTransformationFactory(),
                        SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());
                s.SetCoordinateSystemServices(css);
            }
        }

        public FormMapBox()
        {
            AddToListView = false;

            InitializeComponent();
            mapBox1.ActiveTool = MapBox.Tools.Pan;

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly a in loadedAssemblies)
                LoadMapDecorationTypes(a);

            AddToListView = true;
            AppDomain.CurrentDomain.AssemblyLoad += HandleAssemblyLoad;

            foreach (var name in MapDecorationTypes.Keys)
            {
                lvwDecorations.Items.Add(name);
            }
            pgMapDecoration.SelectedObject = null;

        }


    //    Public Shared Sub RegisterIdpMessages(assembly As Assembly)
    //    For Each subclass In assembly.DefinedTypes.Where(Function(t) t.IsSubclassOf(GetType(IdpMessage)) And t.IsAbstract = False)
    //        RegisterIdpMessage(subclass)
    //    Next
    //End Sub

        private void HandleAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            LoadMapDecorationTypes(args.LoadedAssembly);
            //var mdtype = typeof (SharpMap.Rendering.Decoration.IMapDecoration);
            //foreach (Type type in args.LoadedAssembly.GetTypes())
            //{
            //    if (type.FullName.StartsWith("SharpMap"))
            //        Console.WriteLine(type.FullName);
            //    if (mdtype.IsAssignableFrom(type))
            //    {
            //        if (!type.IsAbstract)
            //        {
            //            if (AddToListView)
            //                lvwDecorations.Items.Add(new ListViewItem(type.Name));
            //            MapDecorationTypes.Add(type.Name, type);
            //        }
            //    }
            //}
        }

        private void LoadMapDecorationTypes(System.Reflection.Assembly a)
        {
            var mdtype = typeof(SharpMap.Rendering.Decoration.IMapDecoration);
            foreach (Type type in a.GetTypes())
            {
                if (type.FullName.StartsWith("SharpMap"))
                    Console.WriteLine(type.FullName);
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
            if (pgMap.InvokeRequired)
                pgMap.Invoke(new MethodInvoker(() => pgMap.Update()));
            else
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

        private void btnTool_Click(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            IMapTool tool = null;

            switch (btn.Name)
            {
                case "btnTool":
                    tool = (mapBox1.CustomTool is SampleTool) ? null : new SampleTool(mapBox1);
                    break;
                case "btnTool2":
                    tool = (mapBox1.CustomTool is MagnifierTool) ? null : new MagnifierTool(mapBox1);
                    break;
            }

            var oldCustomTool = mapBox1.CustomTool;
            if (oldCustomTool is SampleTool) btnTool.Font = new Font(btn.Font, FontStyle.Regular);
            if (oldCustomTool is MagnifierTool) btnTool2.Font = new Font(btn.Font, FontStyle.Regular);

            if (oldCustomTool is IDisposable) ((IDisposable) oldCustomTool).Dispose();

            mapBox1.CustomTool = tool;
            btn.Font = new Font(btn.Font, tool == null ? FontStyle.Regular : FontStyle.Bold);
            if (tool == null)
                mapBox1.ActiveTool = MapBox.Tools.Pan;

            //if (mapBox1.CustomTool == null)
            //    mapBox1.CustomTool = new SampleTool(mapBox1);
            //else
            //{
            //    mapBox1.CustomTool = null;
            //    mapBox1.ActiveTool = MapBox.Tools.Pan;
            //}
        }
    }
}
