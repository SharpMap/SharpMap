using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace SharpMap.Forms.ToolBar
{

    /// <summary>
    /// A tool strip for enabling querying on the map
    /// </summary>
    [DesignTimeVisible(true)]
    public class MapQueryToolStrip : MapToolStrip
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public MapQueryToolStrip()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="container">A container to add components to</param>
        public MapQueryToolStrip(IContainer container)
            : base(container)
        {
            InitializeComponent();
        }

        private static readonly Common.Logging.ILog _logger = Common.Logging.LogManager.GetLogger(typeof(MapQueryToolStrip));

        private ToolStripButton _clear;
        private ToolStripSeparator _sep1;
        private ToolStripButton _queryWindow;
        private ToolStripButton _queryGeometry;
        private ToolStripSeparator _sep2;
        private ToolStripComboBox _queryLayerPicker;

        private Data.Providers.GeometryFeatureProvider _geometryProvider;
        private Layers.VectorLayer _layer;

        private readonly Dictionary<string, int> _dictLayerNameToIndex
            = new Dictionary<string, int>();

        /// <summary>
        /// Method to initialize the tool strip
        /// </summary>
        public void InitializeComponent()
        {
            _clear = new ToolStripButton();
            _sep1 = new ToolStripSeparator();
            _queryWindow = new ToolStripButton();
            _queryGeometry = new ToolStripButton();
            _sep2 = new ToolStripSeparator();
            _queryLayerPicker = new ToolStripComboBox();
            SuspendLayout();
            // 
            // _clear
            // 
            _clear.Image = Properties.Resources.layer_delete;
            _clear.Name = "_clear";
            _clear.Size = new System.Drawing.Size(23, 22);
            // 
            // _sep1
            // 
            _sep1.Name = "_sep1";
            _sep1.Size = new System.Drawing.Size(6, 25);
            // 
            // _queryWindow
            // 
            _queryWindow.CheckOnClick = true;
            _queryWindow.Image = Properties.Resources.rectangle_edit;
            _queryWindow.Name = "_queryWindow";
            _queryWindow.CheckedChanged += OnCheckedChanged;
            _queryWindow.Size = new System.Drawing.Size(23, 22);
            // 
            // _queryGeometry
            // 
            _queryGeometry.CheckOnClick = true;
            _queryGeometry.Image = Properties.Resources.query_spatial_vector;
            _queryGeometry.Name = "_queryGeometry";
            _queryGeometry.Size = new System.Drawing.Size(23, 20);
            _queryGeometry.CheckedChanged += OnCheckedChanged;
            // 
            // _sep2
            // 
            _sep2.Name = "_sep2";
            _sep2.Size = new System.Drawing.Size(6, 6);
            // 
            // _queryLayerPicker
            // 
            _queryLayerPicker.Name = "_queryLayerPicker";
            _queryLayerPicker.Size = new System.Drawing.Size(121, 21);
            _queryLayerPicker.DropDownStyle = ComboBoxStyle.DropDownList;
            _queryLayerPicker.SelectedIndexChanged += OnSelectedIndexChanged;

            // 
            // MapQueryToolStrip
            // 
            Items.AddRange(new ToolStripItem[] {
            _clear,
            _sep1,
            _queryWindow,
            _queryGeometry,
            _sep2,
            _queryLayerPicker});
            ResumeLayout(false);

        }

        /// <inheritdoc cref="MapToolStrip.OnMapControlChangingInternal"/>
        protected override void OnMapControlChangingInternal(CancelEventArgs e)
        {
            base.OnMapControlChangingInternal(e);
            if (MapControl == null) return;

            OnClear();

            MapControl.ActiveToolChanged -= OnMapControlActiveToolChanged;
            MapControl.MapQueried -= OnMapQueried;
            MapControl.MapChanging -= OnMapChanging;
            MapControl.MapChanged -= OnMapChanged;
            MapControl.Map.Layers.ListChanged -= OnListChanged;
        }

        /// <inheritdoc cref="MapToolStrip.OnMapControlChangedInternal"/>
        protected override void OnMapControlChangedInternal(EventArgs e)
        {
            base.OnMapControlChangedInternal(e);

            if (MapControl == null)
            {
                Enabled = false;
                return;
            }
            MapControl.ActiveToolChanged += OnMapControlActiveToolChanged;
            MapControl.MapQueried += OnMapQueried;
            MapControl.MapChanging += OnMapChanging;
            MapControl.MapChanged += OnMapChanged;
            MapControl.Map.Layers.ListChanged += OnListChanged;
        }

        private void OnMapChanged(object sender, EventArgs e)
        {
            MapControl.Map.Layers.ListChanged += OnListChanged;
            MapControl.Map.BackgroundLayer.ListChanged += OnListChanged;
            OnListChanged(MapControl.Map.Layers, new ListChangedEventArgs(ListChangedType.Reset, 0));
        }

        private void OnMapChanging(object sender, CancelEventArgs e)
        {
            MapControl.Map.Layers.ListChanged -= OnListChanged;
            MapControl.Map.BackgroundLayer.ListChanged -= OnListChanged;
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            int index = _queryLayerPicker.SelectedIndex;
            if (index >= 0)
            {
                string lyrName = (string)_queryLayerPicker.Items[index];
                if (_dictLayerNameToIndex.TryGetValue(lyrName, out int lyrIndex))
                {
                    MapControl.QueryLayerIndex = lyrIndex;
                }
            }
        }

        private void OnMapControlActiveToolChanged(MapBox.Tools tool)
        {
            if (MapControl == null) return;
            switch (tool)
            {
                case MapBox.Tools.QueryPoint:
                    _queryGeometry.Checked = true;
                    _queryWindow.Checked = false;
                    break;
                case MapBox.Tools.QueryBox:
                    _queryGeometry.Checked = false;
                    _queryWindow.Checked = true;
                    break;
                default:
                    _queryGeometry.Checked = false;
                    _queryWindow.Checked = false;
                    break;
            }
        }

        private void OnListChanged(object sender, ListChangedEventArgs e)
        {
            _queryLayerPicker.Items.Clear();
            if (MapControl == null)
            {
                return;
            }

            _dictLayerNameToIndex.Clear();
            int queryLayerIndex = MapControl.QueryLayerIndex;
            int i = 0;
            int j = 0;
            int k = -1;
            foreach (var lyr in MapControl.Map.Layers)
            {
                if (lyr.LayerName == "QueriedFeatures") continue;

                if (lyr is Layers.ICanQueryLayer)
                {
                    if (i == queryLayerIndex) k = j;

                    j += 1;
                    _dictLayerNameToIndex.Add(lyr.LayerName, i);
                    _queryLayerPicker.Items.Add(lyr.LayerName);
                }
                i++;

            }
            if (k > -1)
                _queryLayerPicker.SelectedIndex = k;
        }



        private void OnClear()
        {
            if (MapControl == null) return;

            var map = MapControl.Map;
            if (_layer != null && map.Layers.Contains(_layer))
            {
                map.Layers.Remove(_layer);
                _layer.Dispose();
                _layer = null;
            }
        }

        private void OnMapQueried(Data.FeatureDataTable features)
        {
            OnClear();

            if (MapControl == null) return;

            _geometryProvider = new Data.Providers.GeometryFeatureProvider(features);
            _layer = new Layers.VectorLayer("QueriedFeatures", _geometryProvider);
            _layer.IsQueryEnabled = false;

            var map = MapControl.Map;
            map.Layers.Add(_layer);

            MapControl.Refresh();

        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            if (_queryLayerPicker.SelectedItem == null)
            {
                MessageBox.Show(@"No layer to query selected");
                return;
            }

            var checkedButton = (ToolStripButton)sender;

            MapBox.Tools newTool;
            if (sender == _queryWindow)
                newTool = MapBox.Tools.QueryBox;
            else if (sender == _queryGeometry)
                newTool = MapBox.Tools.QueryPoint;
            else
            {
                if (_logger.IsWarnEnabled)
                    _logger.Warn("Unknown object invoking OnCheckedChanged()");
                return;
            }
            TrySetActiveTool(checkedButton, newTool);

        }
    }
}
