using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpMap.Forms
{
    [DesignTimeVisible(true)]
    public partial class MapZoomToolStrip : MapToolStrip
    {
        System.Windows.Forms.ToolStripButton _zoomToExtents;
        System.Windows.Forms.ToolStripButton _fixedZoomIn;
        System.Windows.Forms.ToolStripButton _fixedZoomOut;
        System.Windows.Forms.ToolStripButton _zoomToWindow;
        System.Windows.Forms.ToolStripButton _pan;
        System.Windows.Forms.ToolStripComboBox _predefinedScales;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripSeparator sep2;


        public MapZoomToolStrip()
            :base()
        {
            _predefinedScales.Items.AddRange(new string[] {"1:100", "1:250", "1:500", "1:1000", "1:2500", "1:5000", 
                "1:10000", "1:25000", "1:50000", "1:100000"});
        }

        public MapZoomToolStrip(IContainer container)
            :base(container)
        {
            _predefinedScales.Items.AddRange(new string[] {"1:100", "1:250", "1:500", "1:1000", "1:2500", "1:5000", 
                "1:10000", "1:25000", "1:50000", "1:100000"});
        }

        protected override void InitializeComponent()
        {
            this._zoomToExtents = new System.Windows.Forms.ToolStripButton();
            this._fixedZoomIn = new System.Windows.Forms.ToolStripButton();
            this._fixedZoomOut = new System.Windows.Forms.ToolStripButton();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomToWindow = new System.Windows.Forms.ToolStripButton();
            this._pan = new System.Windows.Forms.ToolStripButton();
            this.sep2 = new System.Windows.Forms.ToolStripSeparator();
            this._predefinedScales = new System.Windows.Forms.ToolStripComboBox();
            this.SuspendLayout();
            // 
            // _zoomToExtents
            // 
            this._zoomToExtents.Enabled = false;
            this._zoomToExtents.Image = global::SharpMap.Properties.Resources.zoom_extent;
            this._zoomToExtents.Name = "_zoomToExtents";
            this._zoomToExtents.Size = new System.Drawing.Size(23, 22);
            this._zoomToExtents.ToolTipText = "Zoom to the map\'s extent";
            this._zoomToExtents.Click += OnFixedZoom;
            // 
            // _fixedZoomIn
            // 
            this._fixedZoomIn.Enabled = false;
            this._fixedZoomIn.Image = global::SharpMap.Properties.Resources.zoom_in;
            this._fixedZoomIn.Name = "_fixedZoomIn";
            this._fixedZoomIn.Size = new System.Drawing.Size(23, 22);
            this._fixedZoomIn.ToolTipText = "Zoom into map";
            this._fixedZoomIn.Click += OnFixedZoom;
            // 
            // _fixedZoomOut
            // 
            this._fixedZoomOut.Enabled = false;
            this._fixedZoomOut.Image = global::SharpMap.Properties.Resources.zoom_out;
            this._fixedZoomOut.Name = "_fixedZoomOut";
            this._fixedZoomOut.Size = new System.Drawing.Size(23, 22);
            this._fixedZoomOut.ToolTipText = "Zoom into map";
            this._fixedZoomOut.Click += OnFixedZoom;
            // 
            // sep1
            // 
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(6, 6);
            // 
            // _zoomToWindow
            // 
            this._zoomToWindow.CheckOnClick = true;
            this._zoomToWindow.CheckedChanged += OnCheckedChanged;
            this._zoomToWindow.Enabled = false;
            this._zoomToWindow.Image = global::SharpMap.Properties.Resources.zoom_region;
            this._zoomToWindow.Name = "_zoomToWindow";
            this._zoomToWindow.Size = new System.Drawing.Size(23, 20);
            this._zoomToWindow.ToolTipText = "Specify viewport by mouse selection";
            // 
            // _pan
            // 
            this._pan.CheckOnClick = true;
            this._pan.CheckedChanged += OnCheckedChanged;
            this._pan.Enabled = false;
            this._pan.Image = global::SharpMap.Properties.Resources.pan;
            this._pan.Name = "_pan";
            this._pan.Size = new System.Drawing.Size(23, 20);
            this._pan.ToolTipText = "Drag the map\'s content around and scoll by mouse wheel";
            // 
            // sep2
            // 
            this.sep2.Name = "sep2";
            this.sep2.Size = new System.Drawing.Size(6, 6);
            // 
            // _predefinedScales
            // 
            this._predefinedScales.Name = "_predefinedScales";
            this._predefinedScales.Size = new System.Drawing.Size(121, 21);
            // 
            // MapZoomToolStrip
            // 
            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomToExtents,
            this._fixedZoomIn,
            this._fixedZoomOut,
            this.sep1,
            this._zoomToWindow,
            this._pan,
            this.sep2,
            this._predefinedScales});
            this.Name = "MatZoomToolStrip";
            this.Text = "MapZoomToolStrip";
            this.ResumeLayout(false);

        }


        private void OnFixedZoom(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            if (sender == _zoomToExtents)
            {
                MapControl.Map.ZoomToExtents();
            }
            else
            {
                var scale = sender == _fixedZoomIn ? 1d / 1.2d : 1.2d;
                MapControl.Map.Zoom *= scale;
            }
            MapControl.Refresh();
        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            var tsb = (System.Windows.Forms.ToolStripButton)sender;

            if (tsb == _pan)
                TrySetActiveTool(tsb, MapBox.Tools.Pan);
            if (tsb == _zoomToWindow)
                TrySetActiveTool(tsb, MapBox.Tools.ZoomWindow);
        }

        protected override void OnMapControlChangedInternal(EventArgs e)
        {
            MapControl.MapZoomChanged += OnMapZoomChanged;
            MapControl.ActiveToolChanged += OnMapControlActiveToolChanged;

            _fixedZoomIn.Enabled =
                _fixedZoomOut.Enabled =
                    _zoomToExtents.Enabled =
                        _pan.Enabled =
                        _zoomToWindow.Enabled = 
                            /*_predefinedScales.Enabled =*/
                                MapControl != null;
            this.Visible = true;
            MapControl.Visible = true;
        }

        private void OnMapZoomChanged(double zoom)
        {
            if (MapControl == null) return;
            var scale = MapControl.Map.Zoom;

            if (InvokeRequired)
                ;//Invoke(zoom => _predefinedScales.Text = string.Format("1:{0}", Math.Round(ZoomToScale(zoom), 0));
            else
                _predefinedScales.Text = string.Format("1:{0}", Math.Round(ZoomToScale(zoom), 0));


        }

        private double ZoomToScale(double zoom)
        {
            return MapControl.Map.Zoom;
        }

        private void OnMapControlActiveToolChanged(MapBox.Tools tool)
        {
            if (MapControl == null) return;
            switch (tool)
            {
                case MapBox.Tools.Pan:
                    _pan.Checked = true;
                    _zoomToWindow.Checked = false;
                    break;
                case MapBox.Tools.ZoomWindow:
                    _pan.Checked = false;
                    _zoomToWindow.Checked = true;
                    break;
                default:
                    _pan.Checked = false;
                    _zoomToWindow.Checked = false;
                    break;
            }
        }

        private void OnScaleEntered(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                OnScaleSelected(sender, e);
        }

        private void OnScaleSelected(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            if (string.IsNullOrEmpty(_predefinedScales.Text))
                return;

            if (!_predefinedScales.Text.StartsWith("1:"))
                _predefinedScales.Text = "1:" + _predefinedScales.Text;

            var val = double.Parse(_predefinedScales.Text.Substring(2), 
                System.Globalization.NumberFormatInfo.InvariantInfo);
            var zoom = ScaleToZoom(val);
            MapControl.Map.Zoom = zoom;
            MapControl.Refresh();
        }

        private double ScaleToZoom(double scale)
        {
            return MapControl.Map.Zoom;
        }
    }
}
