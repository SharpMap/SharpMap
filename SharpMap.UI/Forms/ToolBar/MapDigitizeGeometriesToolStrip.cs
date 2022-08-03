using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace SharpMap.Forms.ToolBar
{
    /// <summary>
    /// Map tool strip that contains geometry edit controls
    /// </summary>
    [DesignTimeVisible(true)]
    public class MapDigitizeGeometriesToolStrip : MapToolStrip
    {
        private static readonly Common.Logging.ILog _logger = Common.Logging.LogManager.GetLogger(typeof(MapDigitizeGeometriesToolStrip));

        /// <summary>
        /// Creates an instance of this control
        /// </summary>
        public MapDigitizeGeometriesToolStrip()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates an instance of this control
        /// </summary>
        /// <param name="container">A container</param>
        public MapDigitizeGeometriesToolStrip(IContainer container)
            : base(container)
        {
            InitializeComponent();
        }

        private ToolStripButton _clear;
        private ToolStripSeparator _sep1;
        private ToolStripButton _addPoint;
        private ToolStripButton _addLineString;
        private ToolStripButton _addPolygon;
        /*
        private System.Windows.Forms.ToolStripButton _addPolygonRing;
        private System.Windows.Forms.ToolStripSeparator _sep2;
        private System.Windows.Forms.ToolStripButton _moveFeature;
        private System.Windows.Forms.ToolStripButton _moveVertex;
        */

        private Data.Providers.GeometryProvider _geometryProvider;
        private Layers.VectorLayer _layer;

        /// <summary>
        /// Initialize this component
        /// </summary>
        public void InitializeComponent()
        {
            SuspendLayout();

            _clear = new ToolStripButton();
            _clear.Name = "_clear";
            _clear.Image = Properties.Resources.layer_delete;
            _clear.Click += OnRemoveFeatures;
            _clear.ToolTipText += @"Removes all geometries from the geometry layer";

            _sep1 = new ToolStripSeparator();
            _sep1.Size = new System.Drawing.Size(6, 6);

            _addPoint = new ToolStripButton();
            _addPoint.Name = "_addPoint";
            _addPoint.Image = Properties.Resources.point_create;
            _addPoint.CheckOnClick = true;
            _addPoint.CheckedChanged += OnCheckedChanged;
            _addPoint.MouseDown += new MouseEventHandler(OnMouseDown);
            _addPoint.ToolTipText += @"Adds a point to the geometry layer";

            _addLineString = new ToolStripButton();
            _addLineString.Name = "_addLineString";
            _addLineString.Image = Properties.Resources.line_create;
            _addLineString.MouseDown += OnMouseDown;
            _addLineString.CheckOnClick = true;
            _addLineString.CheckedChanged += OnCheckedChanged;
            _addLineString.ToolTipText += @"Adds a linestring to the geometry layer";

            _addPolygon = new ToolStripButton();
            _addPolygon.Name = "_addPolygon";
            _addPolygon.Image = Properties.Resources.polygon_create;
            _addPolygon.CheckOnClick = true;
            _addPolygon.MouseDown += OnMouseDown;
            _addPolygon.CheckedChanged += OnCheckedChanged;
            _addPolygon.ToolTipText += @"Adds a linestring to the geometry layer";

            Items.AddRange(new ToolStripItem[] { _clear, _sep1, _addPoint, _addLineString, _addPolygon,
                /* _addPolygonRing, _sep2, _moveFeature, _moveVertex */
                });

            ResumeLayout();
            PerformLayout();

            Visible = true;

            GeometryDefinedHandler = DefaultGeometryDefinedMethod;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMouseDown(object sender, MouseEventArgs e)
        {
            Debug.WriteLine(string.Format("\nButtonClicked '{0}'", ((ToolStripButton)sender).Name));
            if (_logger.IsDebugEnabled)
                _logger.DebugFormat("\nButtonClicked '{0}'", ((ToolStripButton)sender).Name);
        }

        /// <inheritdoc/>
        protected override void OnMapControlChangingInternal(CancelEventArgs e)
        {
            base.OnMapControlChangingInternal(e);

            if (MapControl == null) return;

            MapControl.ActiveToolChanged -= OnMapControlActiveToolChanged;
            MapControl.GeometryDefined -= OnGeometryDefined;
        }

        /// <inheritdoc/>
        protected override void OnMapControlChangedInternal(EventArgs e)
        {
            base.OnMapControlChangedInternal(e);

            if (MapControl == null)
            {
                _layer.Dispose();
                _layer = null;
                Enabled = false;
                return;
            }

            _geometryProvider = new Data.Providers.GeometryProvider((NetTopologySuite.Geometries.Geometry)null);
            _layer = new Layers.VectorLayer("_tmp_Geometries", _geometryProvider);

            MapControl.ActiveToolChanged += OnMapControlActiveToolChanged;
            MapControl.GeometryDefined += OnGeometryDefined;



        }

        private void OnRemoveFeatures(object sender, EventArgs e)
        {
            _geometryProvider?.Geometries.Clear();
            MapControl?.Refresh();
        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            var checkedButton = (ToolStripButton)sender;

            MapBox.Tools newTool;
            if (sender == _addPoint)
                newTool = MapBox.Tools.DrawPoint;
            else if (sender == _addLineString)
                newTool = MapBox.Tools.DrawLine;
            else if (sender == _addPolygon)
                newTool = MapBox.Tools.DrawPolygon;
            else
            {
                if (_logger.IsWarnEnabled)
                    _logger.Warn("Unknown object invoking OnCheckedChanged()");
                return;
            }
            TrySetActiveTool(checkedButton, newTool);
        }

        private void OnMapControlActiveToolChanged(MapBox.Tools tool)
        {
            if (MapControl == null) return;
            switch (tool)
            {
                case MapBox.Tools.DrawPoint:
                    _addLineString.Checked = false;
                    _addPolygon.Checked = false;
                    _addPoint.Checked = true;
                    break;
                case MapBox.Tools.DrawLine:
                    _addPoint.Checked = false;
                    _addPolygon.Checked = false;
                    _addLineString.Checked = true;
                    break;
                case MapBox.Tools.DrawPolygon:
                    _addPoint.Checked = false;
                    _addLineString.Checked = false;
                    _addPolygon.Checked = true;
                    break;
                default:
                    _addPoint.Checked = false;
                    _addLineString.Checked = false;
                    _addPolygon.Checked = false;
                    break;

            }
        }

        private void OnGeometryDefined(NetTopologySuite.Geometries.Geometry geometry)
        {
            if (geometry == null)
                return;

            GeometryDefinedHandler?.Invoke(geometry);

        }

        /// <summary>
        /// A handler that is invoked when a new geometry was defined.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapBox.GeometryDefinedHandler GeometryDefinedHandler { get; set; }

        private void DefaultGeometryDefinedMethod(NetTopologySuite.Geometries.Geometry geom)
        {
            using (var frm = new WktGeometryCreator())
            {
                frm.Geometry = geom;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    _geometryProvider.Geometries.Add(frm.Geometry);
                    if (MapControl != null)
                    {
                        var map = MapControl.Map ?? new Map();
                        if (!map.Layers.Contains(_layer))
                        {
                            map.Layers.Add(_layer);
                        }
                        MapControl.Refresh();
                    }
                }
            }
        }

    }
}
