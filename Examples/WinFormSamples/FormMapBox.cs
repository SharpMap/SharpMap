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
using System.Drawing.Drawing2D;
using SharpMap.Data;
using SharpMap.Rendering.Decoration;
using WinFormSamples.Samples;

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

            foreach (var name in MapDecorationTypes.Keys)
            {
                lvwDecorations.Items.Add(name);
            }
            
            AddToListView = true;
            
            AppDomain.CurrentDomain.AssemblyLoad += HandleAssemblyLoad;

            pgMapDecoration.SelectedObject = null;
            radioButton2.Checked = true;
            radioButton_Click(radioButton2, EventArgs.Empty);
        }

        private void HandleAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            LoadMapDecorationTypes(args.LoadedAssembly);
        }

        private void LoadMapDecorationTypes(System.Reflection.Assembly a)
        {
            var mdtype = typeof(SharpMap.Rendering.Decoration.IMapDecoration);
            try
            {
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
                            MapDecorationTypes[type.Name] = type;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                
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
        
        void mapBox1_MapQueryStarted(object sender, EventArgs e)
        {
            dataGridView1.DataSource = _queriedData = null;
        }

        void mapBox1_MapQueryEnded(object sender, EventArgs e)
        {
            if (_queriedData != null && _queriedData.Tables.Count > 0)
                dataGridView1.DataSource = _queriedData.Tables[0];
        }

        private FeatureDataSet _queriedData;
        private void mapBox1_OnMapQueried(FeatureDataTable data)
        {
            if (_queriedData == null)
                _queriedData = new FeatureDataSet();
            _queriedData.Tables.Add(data);
        }
        
        private void btnCreateTiles_Click(object sender, EventArgs e)
        {
            if (mapBox1.Map == null)
                return;

            if (mapBox1.Map.Layers.Count == 0)
                return;

            using (var f = new FormCreateTilesSample())
            {
                f.Map = mapBox1.Map;
                f.ShowDialog();
            }
        }
        
                private void radioButton_Click(object sender, EventArgs e)
        {
            Cursor mic = mapBox1.Cursor;
            mapBox1.Cursor = Cursors.WaitCursor;
            Cursor = Cursors.WaitCursor;

            if (formSqlServerOpts != null)
            {
                formSqlServerOpts.Close();
                formSqlServerOpts = null;
            }

            try
            {
                //mapImage.ActiveTool = MapImage.Tools.None;
                string text = ((RadioButton)sender).Text;

                switch (text)
                {
                    case "Shapefile":
                        mapBox1.Map = ShapefileSample.InitializeMap(tbAngle.Value);
                        break;
                    case "GradientTheme":
                        mapBox1.Map = GradiantThemeSample.InitializeMap(tbAngle.Value);
                        break;
                    case "WFS Client":
                        mapBox1.Map = WfsSample.InitializeMap(tbAngle.Value);
                        break;
                    case "WMS Client":
                        //mapBox1.Map = TiledWmsSample.InitializeMap();
                        mapBox1.Map = WmsSample.InitializeMap(tbAngle.Value);
                        break;
                    case "OGR - MapInfo":
                    case "OGR - S-57":
                        mapBox1.Map = OgrSample.InitializeMap(tbAngle.Value);
                        break;
                    case "GDAL - GeoTiff":
                    case "GDAL - '.DEM'":
                    case "GDAL - '.ASC'":
                    case "GDAL - '.VRT'":
                        mapBox1.Map = GdalSample.InitializeMap(tbAngle.Value);
                        mapBox1.ActiveTool = MapBox.Tools.Pan;
                        break;
                    case "TileLayer - OSM":
                    case "TileLayer - OSM with XLS":
                    case "TileLayer - Bing Roads":
                    case "TileLayer - Bing Aerial":
                    case "TileLayer - Bing Hybrid":
                    case "TileLayer - GoogleMap":
                    case "TileLayer - GoogleSatellite":
                    case "TileLayer - GoogleTerrain":
                    case "TileLayer - GoogleLabels":
                    case "Eniro":
                        /*
                        tbAngle.Visible = text.Equals("TileLayer - GoogleLabels");
                        if (!tbAngle.Visible) tbAngle.Value = 0;
                         */
                        mapBox1.Map = TileLayerSample.InitializeMap(tbAngle.Value);
                        ((RadioButton)sender).Text = (mapBox1.Map.BackgroundLayer.Count > 0)
                            ? ((RadioButton)sender).Text = mapBox1.Map.BackgroundLayer[0].LayerName
                            : ((RadioButton)sender).Text = mapBox1.Map.Layers[0].LayerName;
                        break;
                    case "PostGis":
                        mapBox1.Map = PostGisSample.InitializeMap(tbAngle.Value);
                        break;
                    case "SpatiaLite":
                        mapBox1.Map = SpatiaLiteSample.InitializeMap(tbAngle.Value);
                        break;
                    case "SqlServer":
                        // create empty map with BruTile basemap
                        mapBox1.Map = SqlServerSample.InitializeMap(tbAngle.Value);
                        // check conn string
                        var connStrBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(Properties.Settings.Default.SqlServerConnectionString);
                        if (string.IsNullOrEmpty(connStrBuilder.DataSource) || string.IsNullOrEmpty(connStrBuilder.InitialCatalog))
                        {
                            mapBox1.Refresh();

                            MessageBox.Show("Requires SqlServer connection string to be defined (Project / Settings)", 
                                "Sql Server", MessageBoxButtons.OK,MessageBoxIcon.Information);
                            return;
                        }
                        // now show SqlServer dialog
                        formSqlServerOpts = new FormSqlServerOpts()
                        {
                            MapBox = mapBox1,
                            ConnectionString = connStrBuilder.ToString()
                        };
                        formSqlServerOpts.Show(this);
                        break;
                    case "Oracle":
                        mapBox1.Map = OracleSample.InitializeMap(tbAngle.Value);
                        break;
                    case "shp_TextOnPath":
                        mapBox1.Map = TextOnPathSample.InitializeMapOrig(tbAngle.Value);
                        break;
                    case "GdiImageLayer":
                        mapBox1.Map = GdiImageLayerSample.InitializeMap(tbAngle.Value);
                        break;
                    default:
                        break;
                }

                //Add checked Map decorations
                foreach (ListViewItem checkedItem in lvwDecorations.CheckedItems)
                {
                    Type mdType;
                    if (MapDecorationTypes.TryGetValue(checkedItem.Text, out mdType))
                    {
                        IMapDecoration md = Activator.CreateInstance(mdType) as IMapDecoration;
                        mapBox1.Map.Decorations.Add(md);
                    }
                }

                mapBox1.Map.Size = Size;
                mapBox1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }
            finally
            {
                Cursor = Cursors.Default;
                mapBox1.Cursor = mic;
            }
        }

        private void mapImage_ActiveToolChanged(MapBox.Tools tool)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapCenterChanged(GeoAPI.Geometries.Coordinate center)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapRefreshed(object sender, EventArgs e)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapZoomChanged(double zoom)
        {
            UpdatePropertyGrid();
            Console.WriteLine("Current Extents: {0}", mapBox1.Map.Envelope);
        }

        private void mapImage_MapZooming(double zoom)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_SizeChanged(object sender, EventArgs e)
        {
            mapBox1.Refresh();
        }

        private void tbAngle_Scroll(object sender, EventArgs e)
        {
            System.Drawing.Drawing2D.Matrix matrix = new Matrix();
            if (tbAngle.Value != 0f)
                matrix.RotateAt(tbAngle.Value, new PointF(mapBox1.Width * 0.5f, mapBox1.Height * 0.5f));

            mapBox1.Map.MapTransform = matrix;
            AdjustRotation(mapBox1.Map.Layers, tbAngle.Value);
            AdjustRotation(mapBox1.Map.VariableLayers, tbAngle.Value);

            mapBox1.Refresh();
        }

        private void radioButton2_MouseUp(object sender, MouseEventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb == null) return;

            if (e.Button != MouseButtons.Right) return;

            SharpMap.Map map = null;
            switch (rb.Name)
            {
                case "radioButton2": // ShapeFile
                    map = Samples.ShapefileSample.InitializeMap(tbAngle.Value, GetOpenFileName("Shapefile|*.shp"));
                    break;
                case "radioButton5": // Ogr
                    map = Samples.OgrSample.InitializeMap(tbAngle.Value, GetOpenFileName("Ogr Datasource|*.*"));
                    break;
                case "radioButton6": // Gdal
                    map = Samples.GdalSample.InitializeMap(tbAngle.Value, GetOpenFileName("Gdal Datasource|*.*"));
                    break;
                case "radioButton9": // spatialite
                    map = Samples.SpatiaLiteSample.InitializeMap(tbAngle.Value, GetOpenFileName("SpatiaLite 2|*.db;*.sqlite"));
                    break;
            }

            if (map == null)
                return;


            //Add checked Map decorations
            foreach (ListViewItem checkedItem in lvwDecorations.CheckedItems)
            {
                Type mdType;
                if (MapDecorationTypes.TryGetValue(checkedItem.Text, out mdType))
                {
                    IMapDecoration md = Activator.CreateInstance(mdType) as IMapDecoration;
                    map.Decorations.Add(md);
                }
            }

            mapBox1.Map = map;
        }

        private void lvwDecorations_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Type mdType;
            if (!MapDecorationTypes.TryGetValue(e.Item.Text, out mdType))
                return;

            if (e.Item.Checked)
            {
                IMapDecoration ins = Activator.CreateInstance(mdType) as IMapDecoration;
                if (ins != null)
                {
                    mapBox1.Map.Decorations.Add(ins);
                }
            }
            else
            {
                foreach (var item in mapBox1.Map.Decorations)
                {
                    var mdTmpType = item.GetType();
                    if (mdType.Equals(mdTmpType))
                    {
                        mapBox1.Map.Decorations.Remove(item);
                        break;
                    }
                }
            }
            e.Item.Selected = true;
            mapBox1.Refresh();
        }

        private void lvwDecorations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvwDecorations.SelectedItems.Count == 0)
            {
                pgMapDecoration.SelectedObject = null;
                return;
            }

            var lvwi = (ListViewItem)lvwDecorations.SelectedItems[0];
            if (!lvwi.Checked)
            {
                pgMapDecoration.SelectedObject = null;
                pgMapDecoration.Visible = false;
                return;
            }

            Type mdType;
            if (MapDecorationTypes.TryGetValue(lvwi.Text, out mdType))
            {
                foreach (IMapDecoration mapDecoration in mapBox1.Map.Decorations)
                {
                    if (mapDecoration.GetType().Equals(mdType))
                    {
                        pgMapDecoration.SelectedObject = mapDecoration;
                        pgMapDecoration.Visible = true;
                        return;
                    }
                }
            }
            pgMapDecoration.SelectedObject = null;
            pgMapDecoration.Visible = false;

        }

        
        
    }
}
