using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using NetTopologySuite.Geometries;
using SharpMap.Properties;

namespace SharpMap.Forms.ToolBar
{
    [DesignTimeVisible(true)]
    public partial class MapZoomToolStrip : MapToolStrip
    {
        private ToolStripButton _zoomToExtents;
        private ToolStripButton _fixedZoomIn;
        private ToolStripButton _fixedZoomOut;
        private ToolStripButton _zoomToWindow;
        private ToolStripButton _pan;
        private ToolStripButton _zoomPrev;
        private ToolStripButton _zoomNext;
        private ToolStripComboBox _predefinedScales;
        private ToolStripButton _minZoom;
        private ToolStripButton _maxZoom;
        private ToolStripButton _maxZoom2;
        private ToolStripButton _lock;
        private ToolStripSeparator _sep1;
        private ToolStripSeparator _sep2;
        private ToolStripSeparator _sep3;
        private ToolStripSeparator _sep4;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public MapZoomToolStrip()
        {
            InitializeComponent();

            AddScales();
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
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

        /// <summary>
        /// Initializes this component
        /// </summary>
        public void InitializeComponent()
        {
            _zoomToExtents = new ToolStripButton();
            _fixedZoomIn = new ToolStripButton();
            _fixedZoomOut = new ToolStripButton();
            _sep1 = new ToolStripSeparator();
            _zoomToWindow = new ToolStripButton();
            _pan = new ToolStripButton();
            _sep2 = new ToolStripSeparator();
            _zoomPrev = new ToolStripButton();
            _zoomNext = new ToolStripButton();
            _sep3 = new ToolStripSeparator();
            _predefinedScales = new ToolStripComboBox();
            _sep4 = new ToolStripSeparator();
            _minZoom = new ToolStripButton();
            _maxZoom = new ToolStripButton();
            _maxZoom2 = new ToolStripButton();
            _lock = new ToolStripButton();
            SuspendLayout();
            // 
            // _zoomToExtents
            // 
            _zoomToExtents.Enabled = false;
            _zoomToExtents.Image = Resources.zoom_extent;
            _zoomToExtents.Name = "_zoomToExtents";
            _zoomToExtents.Size = new System.Drawing.Size(23, 22);
            _zoomToExtents.ToolTipText = @"Zoom to the map\'s extent";
            _zoomToExtents.Click += OnFixedZoom;
            // 
            // _fixedZoomIn
            // 
            _fixedZoomIn.Enabled = false;
            _fixedZoomIn.Image = Resources.zoom_in;
            _fixedZoomIn.Name = "_fixedZoomIn";
            _fixedZoomIn.Size = new System.Drawing.Size(23, 22);
            _fixedZoomIn.ToolTipText = @"Zoom into map";
            _fixedZoomIn.Click += OnFixedZoom;
            // 
            // _fixedZoomOut
            // 
            _fixedZoomOut.Enabled = false;
            _fixedZoomOut.Image = Resources.zoom_out;
            _fixedZoomOut.Name = "_fixedZoomOut";
            _fixedZoomOut.Size = new System.Drawing.Size(23, 22);
            _fixedZoomOut.ToolTipText = @"Zoom out of map";
            _fixedZoomOut.Click += OnFixedZoom;
            // 
            // _sep1
            // 
            _sep1.Name = "_sep1";
            _sep1.Size = new System.Drawing.Size(6, 6);
            // 
            // _zoomToWindow
            // 
            _zoomToWindow.CheckOnClick = true;
            _zoomToWindow.Enabled = false;
            _zoomToWindow.Image = Resources.zoom_region;
            _zoomToWindow.Name = "_zoomToWindow";
            _zoomToWindow.Size = new System.Drawing.Size(23, 20);
            _zoomToWindow.ToolTipText = @"Specify viewport by mouse selection";
            _zoomToWindow.CheckOnClick = true;
            _zoomToWindow.CheckedChanged += OnCheckedChanged;
            // 
            // _pan
            // 
            _pan.CheckOnClick = true;
            _pan.Enabled = false;
            _pan.Image = Resources.pan;
            _pan.Name = "_pan";
            _pan.Size = new System.Drawing.Size(23, 20);
            _pan.ToolTipText = @"Drag the map\'s content around and scoll by mouse wheel";
            _pan.CheckOnClick = true;
            _pan.CheckedChanged += OnCheckedChanged;
            // 
            // _sep2
            // 
            _sep2.Name = "_sep2";
            _sep2.Size = new System.Drawing.Size(6, 6);
            // 
            // _zoomPrev
            // 
            _zoomPrev.Enabled = false;
            _zoomPrev.Image = Resources.zoom_last;
            _zoomPrev.Name = "_zoomPrev";
            _zoomPrev.Size = new System.Drawing.Size(23, 20);
            _zoomPrev.ToolTipText = @"Zoom to previous viewport";
            _zoomPrev.Click += (sender, args) => _zoomExtentStack.ZoomPrevious();
            // 
            // _zoomNext
            // 
            _zoomNext.Enabled = false;
            _zoomNext.Image = Resources.zoom_next;
            _zoomNext.Name = "_zoomNext";
            _zoomNext.Size = new System.Drawing.Size(23, 20);
            _zoomNext.ToolTipText = @"Restore last viewport";
            _zoomNext.Click += (sender, args) => _zoomExtentStack.ZoomNext();
            // 
            // _sep3
            // 
            _sep3.Name = "_sep3";
            _sep3.Size = new System.Drawing.Size(6, 6);
            // 
            // _predefinedScales
            // 
            _predefinedScales.Name = "_predefinedScales";
            _predefinedScales.Size = new System.Drawing.Size(121, 23);
            _predefinedScales.SelectedIndexChanged += OnScaleSelected;
            // 
            // _sep4
            // 
            _sep4.Name = "_sep3";
            _sep4.Size = new System.Drawing.Size(6, 6);
            // 
            // _minZoom
            // 
            _minZoom.Enabled = false;
            _minZoom.Text = "min";
            _minZoom.Name = "_minZoom";
            _minZoom.Size = new System.Drawing.Size(23, 20);
            _minZoom.ToolTipText = @"Set the minimum zoom level";
            _minZoom.CheckOnClick = true;
            _minZoom.CheckedChanged += OnCheckedChanged;
            // 
            // _maxZoom
            // 
            _maxZoom.Enabled = false;
            _maxZoom.Text = "max";
            _maxZoom.Name = "_maxZoom";
            _maxZoom.Size = new System.Drawing.Size(23, 20);
            _maxZoom.ToolTipText = @"Set the maximum zoom level";
            _maxZoom.CheckOnClick = true;
            _maxZoom.CheckedChanged += OnCheckedChanged;
            // 
            // _maxZoom2
            // 
            _maxZoom2.Enabled = false;
            _maxZoom2.Text = "max box";
            _maxZoom2.Name = "_maxZoom2";
            _maxZoom2.Size = new System.Drawing.Size(23, 20);
            _maxZoom2.ToolTipText = @"Set the maximum zoom window";
            _maxZoom2.CheckOnClick = true;
            _maxZoom2.CheckedChanged += OnCheckedChanged;
            // 
            // _lock
            // 
            _lock.Enabled = false;
            _lock.Name = "_lock";
            _lock.Size = new System.Drawing.Size(23, 20);
            _lock.ToolTipText = @"Lock the viewport";
            _lock.CheckOnClick = true;
            _lock.Image = Resources.unlocked;
            _lock.CheckedChanged += OnCheckedChanged;
            // 
            // MapZoomToolStrip
            // 
            Items.AddRange(new ToolStripItem[] {
            _zoomToExtents,
            _fixedZoomIn,
            _fixedZoomOut,
            _sep1,
            _zoomToWindow,
            _pan,
            _sep2,
            _zoomPrev,
            _zoomNext,
            _sep3,
            _predefinedScales,
            _sep4,
            _minZoom,
            _maxZoom,
            _lock,
            });
            Text = @"MapZoomToolStrip";
            ResumeLayout(false);

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
                double scale = sender == _fixedZoomIn ? 1d / 1.2d : 1.2d;
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

            /// <summary>
            /// Value indicating if zoom changes that have been invoked by user interaction should be saved or not
            /// </summary>
            private bool _storeExtentsUser;

            /// <summary>
            /// Value indicating if zoom changes that have been invoked by this class should be stored or not
            /// </summary>
            private bool _storeExtentsInternal;

            private int _index;
            private bool _isPanning;
            private int _skip;

            /// <summary>
            /// Creates an instance of this class. <br/>
            /// No extents will be stored until .StoreExtents = true
            /// </summary>
            /// <param name="mapBox">The MapBox control</param>
            public ZoomExtentStack(MapBox mapBox)
            {
                _mapBox = mapBox;
                _mapBox.Map.MapViewOnChange += HandleMapMapViewOnChange;
                _mapBox.MouseDown += HandleMapBoxMouseDown;
                _mapBox.MouseUp += HandleMapBoxMouseUp;
                _mapBox.MouseWheel += HandleMouseWheel;
            }

            private void HandleMouseWheel(object sender, MouseEventArgs e)
            {
                // Is the map box going to keep the position under the cursor
                // untouched?
                if (_mapBox.ZoomToPointer)
                {
                    /* 
                     * For the computation of the new zoom and center, the
                     * Map.Center is moved, Map.Zoom is changed and Map.Center 
                     * then reset. We only want the last viewport, this we're
                     * going to skip the next two zoom changes.
                     */
                    _skip = 2;
                }
            }

            private void HandleMapBoxMouseDown(Coordinate worldPos, MouseEventArgs imagePos)
            {
                if (_mapBox.ActiveTool == MapBox.Tools.Pan)
                {
                    _isPanning = true;
                    Add(_mapBox.Map.Envelope);
                }
            }

            private void HandleMapBoxMouseUp(Coordinate worldPos, MouseEventArgs imagePos)
            {
                if (_mapBox.ActiveTool == MapBox.Tools.Pan)
                {
                    _isPanning = false;
                }
            }

            private void HandleMapMapViewOnChange()
            {

                if (_storeExtentsUser && _storeExtentsInternal /*&& (!_blockStoringWhenPanning)*/)
                {
                    if (_isPanning)
                        _zoomExtentStack[_index] = _mapBox.Map.Envelope;
                    else
                        Add(_mapBox.Map.Envelope);
                }
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
                get { return _index > 0; }
            }

            /// <summary>
            /// true if 'next' zoom is available
            /// </summary>
            public bool CanZoomNext
            {
                get { return _index < _zoomExtentStack.Count - 1; }
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
                    _mapBox.Refresh();
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
                    _mapBox.Refresh();
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
                if (_skip == 0)
                    _index = _zoomExtentStack.Count - 1;
                else 
                    _skip--;
            }
        }
        
        #endregion

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            var tsb = (ToolStripButton)sender;

            if (tsb == _pan)
                TrySetActiveTool(tsb, MapBox.Tools.Pan);
            
            if (tsb == _zoomToWindow)
                TrySetActiveTool(tsb, MapBox.Tools.ZoomWindow);
            
            if (tsb == _minZoom)
            {
                MapControl.Map.MinimumZoom =
                    _minZoom.Checked ? MapControl.Map.Zoom : Double.Epsilon;
            }
            
            if (tsb == _maxZoom)
            {
                MapControl.Map.MaximumZoom =
                    _maxZoom.Checked ? MapControl.Map.Zoom : Double.MaxValue;
            }

            if (tsb == _lock)
            {
                if (_lock.Checked)
                {
                    _mvpLock.Lock();
                    tsb.Image = Resources.locked;
                }
                else
                {
                    _mvpLock.Unlock();
                    tsb.Image = Resources.unlocked;
                }

            }
        }

        private MapViewportLock _mvpLock;

        private void ResetControls()
        {
            Visible = true;
            
            if (MapControl.Map != null)
            {
                _minZoom.Checked = MapControl.Map.MinimumZoom > Double.Epsilon;
                _maxZoom.Checked = MapControl.Map.MaximumZoom < Double.MaxValue;
                _maxZoom2.Checked = MapControl.Map.MaximumExtents != null;
            }

            _zoomExtentStack = new ZoomExtentStack(MapControl);
            _zoomExtentStack.StoreExtents = true;

            _mvpLock = new MapViewportLock(MapControl.Map);
        }
        
        /// <inheritdoc cref="MapToolStrip.OnMapControlChangedInternal"/>
        protected override void OnMapControlChangedInternal(EventArgs e)
        {
            if (MapControl == null)
                return;

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
                                _minZoom.Enabled =
                                    _maxZoom.Enabled =
                                        _maxZoom2.Enabled =
                                            _lock.Enabled =
                                                /*_predefinedScales.Enabled =*/
                                                MapControl.Map != null;

            Visible = true;

            ResetControls();

            MapControl.Visible = true;
        }

        private void HandleMapChanged(object sender, EventArgs e)
        {
            if (sender != MapControl)
                return;

            ResetControls();
           
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
            _mvpLock = null;

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

        private void OnScaleEntered(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                OnScaleSelected(sender, e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                e.IsInputKey = true;
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

        private int _dpiX;//, _dpiY;
        private ZoomExtentStack _zoomExtentStack;

        /// <inheritdoc cref="Control.OnCreateControl"/>
        protected override void OnCreateControl()
        {
            using (var g = CreateGraphics())
            {
                _dpiX = (int)g.DpiX;
                //_dpiY = (int)g.DpiY;
            }
            base.OnCreateControl();
        }
    }
}
