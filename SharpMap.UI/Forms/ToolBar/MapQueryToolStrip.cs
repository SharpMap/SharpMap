using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SharpMap.Forms.ToolBar
{
    using System.ComponentModel;

    [DesignTimeVisible(true)]
    public class MapQueryToolStrip : MapToolStrip
    {
        public MapQueryToolStrip()
            :base()
        {
            InitializeComponent();
        }

        public MapQueryToolStrip(IContainer container)
            :base(container)
        {
            InitializeComponent();
        }

        private static readonly Common.Logging.ILog Logger = Common.Logging.LogManager.GetCurrentClassLogger();

        private ToolStripButton _clear;
        private ToolStripSeparator _sep1;
        private ToolStripButton _queryWindow;
        private ToolStripButton _queryGeometry;
        private ToolStripSeparator _sep2;
        private ToolStripComboBox _queryLayerPicker;

        private Data.Providers.GeometryFeatureProvider _geometryProvider;
        private Layers.VectorLayer _layer;

        private readonly Dictionary<string, int> _dictLayerNameToIndex
            = new Dictionary<string,int>();

        public void InitializeComponent()
        {
            this._clear = new System.Windows.Forms.ToolStripButton();
            this._sep1 = new System.Windows.Forms.ToolStripSeparator();
            this._queryWindow = new System.Windows.Forms.ToolStripButton();
            this._queryGeometry = new System.Windows.Forms.ToolStripButton();
            this._sep2 = new System.Windows.Forms.ToolStripSeparator();
            this._queryLayerPicker = new System.Windows.Forms.ToolStripComboBox();
            this.SuspendLayout();
            // 
            // _clear
            // 
            this._clear.Image = global::SharpMap.Properties.Resources.layer_delete;
            this._clear.Name = "_clear";
            this._clear.Size = new System.Drawing.Size(23, 22);
            // 
            // _sep1
            // 
            this._sep1.Name = "_sep1";
            this._sep1.Size = new System.Drawing.Size(6, 25);
            // 
            // _queryWindow
            // 
            this._queryWindow.CheckOnClick = true;
            this._queryWindow.Image = global::SharpMap.Properties.Resources.rectangle_edit;
            this._queryWindow.Name = "_queryWindow";
            this._queryWindow.CheckedChanged += OnCheckedChanged;
            this._queryWindow.Size = new System.Drawing.Size(23, 22);
            // 
            // _queryGeometry
            // 
            this._queryGeometry.CheckOnClick = true;
            this._queryGeometry.Image = global::SharpMap.Properties.Resources.query_spatial_vector;
            this._queryGeometry.Name = "_queryGeometry";
            this._queryGeometry.Size = new System.Drawing.Size(23, 20);
            this._queryGeometry.CheckedChanged += OnCheckedChanged;
            // 
            // _sep2
            // 
            this._sep2.Name = "_sep2";
            this._sep2.Size = new System.Drawing.Size(6, 6);
            // 
            // _queryLayerPicker
            // 
            this._queryLayerPicker.Name = "_queryLayerPicker";
            this._queryLayerPicker.Size = new System.Drawing.Size(121, 21);
            this._queryLayerPicker.DropDownStyle = ComboBoxStyle.DropDownList;
            this._queryLayerPicker.SelectedIndexChanged += OnSelectedIndexChanged;

            // 
            // MapQueryToolStrip
            // 
            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._clear,
            this._sep1,
            this._queryWindow,
            this._queryGeometry,
            this._sep2,
            this._queryLayerPicker});
            this.ResumeLayout(false);

        }

        protected override void  OnMapControlChangingInternal(System.ComponentModel.CancelEventArgs e)
        {
 	        base.OnMapControlChangingInternal(e);
            if (MapControl == null) return;

            OnClear(this, EventArgs.Empty);
            this.MapControl.ActiveToolChanged -= OnMapControlActiveToolChanged;
            this.MapControl.MapQueried -= OnMapQueried;
            this.MapControl.MapChanging -= OnMapChanging;
            this.MapControl.MapChanged -= OnMapChanged;
            MapControl.Map.Layers.ListChanged -= OnListChanged;
        }

        protected override void  OnMapControlChangedInternal(EventArgs e)
        {
 	        base.OnMapControlChangedInternal(e);

            if (MapControl == null)
            {
                Enabled =false;
                return;
            }
            this.MapControl.ActiveToolChanged += OnMapControlActiveToolChanged;
            this.MapControl.MapQueried += OnMapQueried;
            this.MapControl.MapChanging += OnMapChanging;
            this.MapControl.MapChanged += OnMapChanged;
            MapControl.Map.Layers.ListChanged += OnListChanged;
        }

        private void OnMapChanged(object sender, EventArgs e)
        {
            MapControl.Map.Layers.ListChanged += OnListChanged;
            OnListChanged(MapControl.Map.Layers, new ListChangedEventArgs(ListChangedType.Reset, 0));
        }

        private void OnMapChanging(object sender, CancelEventArgs e)
        {
            MapControl.Map.Layers.ListChanged -= OnListChanged;
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            var index = _queryLayerPicker.SelectedIndex;
            if (index >= 0)
            {
                var lyrName = (string)_queryLayerPicker.Items[index];
                int lyrIndex;
                if (_dictLayerNameToIndex.TryGetValue(lyrName, out lyrIndex))
                {
                    MapControl.QueryLayerIndex = lyrIndex;
                }
            }
            else
            { }
        }

        private void OnMapControlActiveToolChanged(MapBox.Tools tool)
        {
            if (MapControl == null) return;
            switch (tool)
            {
                case MapBox.Tools.QueryGeometry:
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

        private void OnListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            _queryLayerPicker.Items.Clear();
            if (MapControl == null)
            {
                return;
            }

            _dictLayerNameToIndex.Clear();
            var queryLayerIndex = MapControl.QueryLayerIndex;
            var i = 0;
            var j = 0;
            var k = -1;
            foreach(var lyr in MapControl.Map.Layers)
            {
                if (lyr.LayerName == "QueriedFeatures") continue;

                if (lyr is SharpMap.Layers.ICanQueryLayer)
                {
                    if (i == queryLayerIndex) k = j;

                    j = j + 1;
                    _dictLayerNameToIndex.Add(lyr.LayerName, i);
                    _queryLayerPicker.Items.Add(lyr.LayerName);
                }
                i++;

            }
            if (k > -1)
            _queryLayerPicker.SelectedIndex = k;
        }



        private void OnClear(object sender, EventArgs e)
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

        private void OnMapQueried(SharpMap.Data.FeatureDataTable features)
        {
            OnClear(this, EventArgs.Empty);

            if (MapControl == null) return;

            _geometryProvider = new SharpMap.Data.Providers.GeometryFeatureProvider(features);
            _layer = new SharpMap.Layers.VectorLayer("QueriedFeatures", _geometryProvider);
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
                System.Windows.Forms.MessageBox.Show("No layer to query selected");
                return;
            }

            var checkedButton = (System.Windows.Forms.ToolStripButton)sender;

            MapBox.Tools newTool;
            if (sender == _queryWindow)
                newTool = MapBox.Tools.QueryBox;
            else if (sender == _queryGeometry)
                newTool = MapBox.Tools.QueryGeometry;
            else
            {
                if (Logger.IsWarnEnabled)
                    Logger.Warn("Unknown object invoking OnCheckedChanged()");
                return;
            }
            TrySetActiveTool(checkedButton, newTool);
            
        }
    }
}
