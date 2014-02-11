using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using GeoAPI.Geometries;

namespace SharpMap.Forms.ToolBar
{
    [DesignTimeVisible(true)]
    public partial class MapZoomToolStrip : MapToolStrip
    {
        System.Windows.Forms.ToolStripButton _zoomToExtents;
        System.Windows.Forms.ToolStripButton _fixedZoomIn;
        System.Windows.Forms.ToolStripButton _fixedZoomOut;
        System.Windows.Forms.ToolStripButton _zoomToWindow;
        System.Windows.Forms.ToolStripButton _pan;
        System.Windows.Forms.ToolStripButton _zoomPrev;
        System.Windows.Forms.ToolStripButton _zoomNext;
        System.Windows.Forms.ToolStripComboBox _predefinedScales;
        private System.Windows.Forms.ToolStripSeparator _sep1;
        private System.Windows.Forms.ToolStripSeparator _sep2;
        private System.Windows.Forms.ToolStripSeparator _sep3;


        public MapZoomToolStrip()
            :base()
        {
            InitializeComponent();

            AddScales();
        }

        public MapZoomToolStrip(IContainer container)
            :base(container)
        {
            InitializeComponent();
            AddScales();
        }

        private void AddScales()
        {
            _predefinedScales.Items.AddRange(new object[]
            {
                "1:100", "1:250", "1:500", "1:1000", "1:2500", "1:5000",
                "1:10000", "1:25000", "1:50000", "1:100000", "1:250000",
                "1:500000", "1:1000000", "1:2500000", "1:5000000", "1:10000000"
            });
        }

        public void InitializeComponent()
        {
            this._zoomToExtents = new System.Windows.Forms.ToolStripButton();
            this._fixedZoomIn = new System.Windows.Forms.ToolStripButton();
            this._fixedZoomOut = new System.Windows.Forms.ToolStripButton();
            this._sep1 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomToWindow = new System.Windows.Forms.ToolStripButton();
            this._pan = new System.Windows.Forms.ToolStripButton();
            this._sep2 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomPrev = new System.Windows.Forms.ToolStripButton();
            this._zoomNext = new System.Windows.Forms.ToolStripButton();
            this._sep3 = new System.Windows.Forms.ToolStripSeparator();
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
            this._fixedZoomOut.ToolTipText = "Zoom out of map";
            this._fixedZoomOut.Click += OnFixedZoom;
            // 
            // sep1
            // 
            this._sep1.Name = "_sep1";
            this._sep1.Size = new System.Drawing.Size(6, 6);
            // 
            // _zoomToWindow
            // 
            this._zoomToWindow.CheckOnClick = true;
            this._zoomToWindow.Enabled = false;
            this._zoomToWindow.Image = global::SharpMap.Properties.Resources.zoom_region;
            this._zoomToWindow.Name = "_zoomToWindow";
            this._zoomToWindow.Size = new System.Drawing.Size(23, 20);
            this._zoomToWindow.ToolTipText = "Specify viewport by mouse selection";
            this._zoomToWindow.CheckedChanged += OnCheckedChanged;
            // 
            // _pan
            // 
            this._pan.CheckOnClick = true;
            this._pan.Enabled = false;
            this._pan.Image = global::SharpMap.Properties.Resources.pan;
            this._pan.Name = "_pan";
            this._pan.Size = new System.Drawing.Size(23, 20);
            this._pan.ToolTipText = "Drag the map\'s content around and scoll by mouse wheel";
            this._pan.CheckedChanged += OnCheckedChanged;
            // 
            // _sep2
            // 
            this._sep2.Name = "_sep2";
            this._sep2.Size = new System.Drawing.Size(6, 6);
            // 
            // _zoomPrev
            // 
            this._zoomPrev.Enabled = false;
            this._zoomPrev.Image = global::SharpMap.Properties.Resources.zoom_last;
            this._zoomPrev.Name = "_zoomPrev";
            this._zoomPrev.Size = new System.Drawing.Size(23, 22);
            this._zoomPrev.ToolTipText = "Zoom to previous viewport";
            this._zoomPrev.Click += OnFixedZoom;
            // 
            // _zoomNext
            // 
            this._zoomNext.Enabled = false;
            this._zoomNext.Image = global::SharpMap.Properties.Resources.zoom_next;
            this._zoomNext.Name = "_zoomNext";
            this._zoomNext.Size = new System.Drawing.Size(23, 22);
            this._zoomNext.ToolTipText = "Restore last viewport";
            this._zoomNext.Click += OnFixedZoom;
            // 
            // sep3
            // 
            this._sep3.Name = "_sep3";
            this._sep3.Size = new System.Drawing.Size(6, 6);
            // 
            // _predefinedScales
            // 
            this._predefinedScales.Name = "_predefinedScales";
            this._predefinedScales.Size = new System.Drawing.Size(121, 21);
            this._predefinedScales.KeyPress += OnScaleEntered;
            this._predefinedScales.SelectedIndexChanged += OnScaleSelected;
            // 
            // MapZoomToolStrip
            // 
            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomToExtents,
            this._fixedZoomIn,
            this._fixedZoomOut,
            this._sep1,
            this._zoomToWindow,
            this._pan,
            this._sep2,
            this._zoomPrev,
            this._zoomNext,
            this._sep3,
            this._predefinedScales});
            this.Name = "MapZoomToolStrip";
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
            else if (sender == _zoomPrev)
            {
                _zoomExtentStack.ZoomPrevious();
            }
            else if (sender == _zoomNext)
            {
                _zoomExtentStack.ZoomNext();
            }
            else
            {
                var scale = sender == _fixedZoomIn ? 1d / 1.2d : 1.2d;
                MapControl.Map.Zoom *= scale;
            }
            MapControl.Refresh();
        }

        #region ZoomExtentStack
        
        /// <summary>
        /// An implementation of zoom previous/zoom next like *** MapControl
        /// </summary>
        [Serializable]
        public class ZoomExtentStack
        {
            private readonly MapBox _mapBox;
            private readonly List<Envelope> _zoomExtentStack = new List<Envelope>();
            private bool _blockStoringWhenPanning;
            private bool _storeExtentsUser;
            private bool _storeExtentsInternal;
            private int _index;

            /// <summary>
            /// Initialisation; no extents will be stored until .StoreExtents = true
            /// </summary>
            /// <param name="mapBox">mapbox control</param>
            public ZoomExtentStack(MapBox mapBox)
            {
                _mapBox = mapBox;
                _mapBox.Map.MapViewOnChange += HandleMapMapViewOnChange;
                _mapBox.MouseDown += HandleMapBoxMouseDown;
                _mapBox.MouseUp += HandleMapBoxMouseUp;
            }

            private void HandleMapBoxMouseDown(Coordinate worldPos, MouseEventArgs imagePos)
            {
                if (_mapBox.ActiveTool == MapBox.Tools.Pan) _blockStoringWhenPanning = false;
            }

            private void HandleMapBoxMouseUp(Coordinate worldPos, MouseEventArgs imagePos)
            {
                if (_mapBox.ActiveTool == MapBox.Tools.Pan) _blockStoringWhenPanning = true;
            }

            private void HandleMapMapViewOnChange()
            {
                if (_storeExtentsUser && _storeExtentsInternal && !_blockStoringWhenPanning)
                    Add(_mapBox.Map.Envelope);
                else
                    _storeExtentsInternal = true;
            }

            /// <summary>
            /// If true extents will be stored, starting with the current;
            /// if false no extents will be stored (paused).
            /// </summary>
            public bool StoreExtents
            {
                get { return _storeExtentsUser; }
                set
                {
                    if (value) Add(_mapBox.Map.Envelope);
                    _storeExtentsUser = value;
                }
            }

            /// <summary>
            /// Erases the extents stack
            /// </summary>
            public void Clear()
            {
                _zoomExtentStack.Clear();
                _index = 0;
            }

            /// <summary>
            /// true if previous zoom is available
            /// </summary>
            public bool CanZoomPrevious
            {
                get { return (_index > 0); }
            }

            /// <summary>
            /// true if 'next' zoom is available
            /// </summary>
            public bool CanZoomNext
            {
                get { return (_index < _zoomExtentStack.Count - 1); }
            }

            /// <summary>
            /// Execute a zoom previous
            /// </summary>
            public void ZoomPrevious()
            {
                if (CanZoomPrevious)
                {
                    _storeExtentsInternal = false;
                    _index--;
                    _mapBox.Map.ZoomToBox(_zoomExtentStack[_index]);
                }
            }

            /// <summary>
            /// Execute a zoom next
            /// </summary>
            public void ZoomNext()
            {
                if (CanZoomNext)
                {
                    _storeExtentsInternal = false;
                    _index++;
                    _mapBox.Map.ZoomToBox(_zoomExtentStack[_index]);
                }
            }

            /// <summary>
            /// Adds the given extent to the stack
            /// </summary>
            /// <param name="newExtent">the extent to be stored</param>
            private void Add(Envelope newExtent)
            {
                // remove all above index
                if (_zoomExtentStack.Count - 1 > _index)
                    _zoomExtentStack.RemoveRange(_index + 1, _zoomExtentStack.Count - _index - 1);
                // add given extent
                _zoomExtentStack.Add(newExtent);
                // correct index
                _index = _zoomExtentStack.Count - 1;
            }
        }
        
        #endregion

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
            //MapControl.MapZoomChanged += OnMapZoomChanged;
            MapControl.MapChanging += HandleMapChanging;
            MapControl.MapChanged += HandleMapChanged;
            MapControl.Map.MapViewOnChange += OnMapMapViewOnChange;
            MapControl.ActiveToolChanged += OnMapControlActiveToolChanged;


            _fixedZoomIn.Enabled =
                _fixedZoomOut.Enabled =
                    _zoomToExtents.Enabled =
                        _pan.Enabled =
                        _zoomToWindow.Enabled = 
                            /*_predefinedScales.Enabled =*/
                                MapControl != null;
            this.Visible = true;
            _zoomExtentStack = new ZoomExtentStack(MapControl);
            _zoomExtentStack.StoreExtents = true;
            

            MapControl.Visible = true;
        }

        private void HandleMapChanged(object sender, EventArgs e)
        {
            if (sender != MapControl)
                return;

            _zoomExtentStack = new ZoomExtentStack(MapControl);
            _zoomExtentStack.StoreExtents = true;

            _predefinedScales.Text = string.Format(NumberFormatInfo.CurrentInfo, "1:{0}", 
                Math.Round(MapControl.Map.GetMapScale(_dpiX), 0, MidpointRounding.AwayFromZero));
            MapControl.Map.MapViewOnChange += OnMapMapViewOnChange;
        }

        private void HandleMapChanging(object sender, CancelEventArgs e)
        {
            if (sender != MapControl)
                return;
            _zoomExtentStack.Clear();
            
            MapControl.Map.MapViewOnChange -= OnMapMapViewOnChange;
        }

        void OnMapMapViewOnChange()
        {
            OnMapZoomChanged(MapControl.Map.Zoom);
        }

        private void OnMapZoomChanged(double zoom)
        {
            if (MapControl == null) return;

            var scale = MapControl.Map.GetMapScale(_dpiX);

            //_zoomExtentStack.StoreExtents

            _zoomPrev.Enabled = _zoomExtentStack.CanZoomPrevious;
            _zoomNext.Enabled = _zoomExtentStack.CanZoomNext;

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)
                            delegate
                                {
                                    _zoomPrev.Enabled = _zoomExtentStack.CanZoomPrevious;
                                    _zoomNext.Enabled = _zoomExtentStack.CanZoomNext;
                                    _predefinedScales.Text = string.Format("1:{0}", 
                                        Math.Round(scale, 0, MidpointRounding.AwayFromZero));
                                });
            else
            {
                _zoomPrev.Enabled = _zoomExtentStack.CanZoomPrevious;
                _zoomNext.Enabled = _zoomExtentStack.CanZoomNext;
                _predefinedScales.Text = string.Format("1:{0}", Math.Round(scale, 0));
            }
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

            var text = _predefinedScales.Text;
            if (!text.StartsWith("1:"))
                 text = "1:" + text;

            double val;
            if (!double.TryParse(text.Substring(2), NumberStyles.Float, NumberFormatInfo.CurrentInfo, out val))
                return;

            //_predefinedScales.Text = text;

            MapControl.Map.MapScale = val;
            MapControl.Refresh();

            BeginInvoke(new MethodInvoker(
            delegate
            {
                _predefinedScales.Text = string.Format("1:{0}",
                    Math.Round(MapControl.Map.GetMapScale(_dpiX), MidpointRounding.AwayFromZero));
                _predefinedScales.SelectionStart = 0;
                _predefinedScales.SelectionLength = _predefinedScales.Text.Length;
            }));
        }

        private int _dpiX, _dpiY;
        private ZoomExtentStack _zoomExtentStack;

        protected override void OnCreateControl()
        {
            using (var g = CreateGraphics())
            {
                _dpiX = (int)g.DpiX;
                _dpiY = (int)g.DpiY;
            }
            base.OnCreateControl();
        }
    }
}
