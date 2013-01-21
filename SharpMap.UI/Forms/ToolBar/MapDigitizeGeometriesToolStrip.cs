using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpMap.Forms.ToolBar
{
    /// <summary>
    /// Map tool strip that contains geometry edit controls
    /// </summary>
    [System.ComponentModel.DesignTimeVisible(true)]
    public class MapDigitizeGeometriesToolStrip : MapToolStrip
    {
        private static readonly Common.Logging.ILog Logger = Common.Logging.LogManager.GetCurrentClassLogger();

        public MapDigitizeGeometriesToolStrip()
            :base()
        {
            InitializeComponent();
        }

        public MapDigitizeGeometriesToolStrip(IContainer container)
            : base(container)
        {
            InitializeComponent();
        }

        private System.Windows.Forms.ToolStripButton _clear;
        private System.Windows.Forms.ToolStripSeparator _sep1;
        private System.Windows.Forms.ToolStripButton _addPoint;
        private System.Windows.Forms.ToolStripButton _addLineString;
        private System.Windows.Forms.ToolStripButton _addPolygon;
        /*
                private System.Windows.Forms.ToolStripButton _addPolygonRing;
                private System.Windows.Forms.ToolStripSeparator _sep2;
                private System.Windows.Forms.ToolStripButton _moveFeature;
                private System.Windows.Forms.ToolStripButton _moveVertex;
        */

        private SharpMap.Data.Providers.GeometryProvider _geometryProvider;
        private SharpMap.Layers.VectorLayer _layer;

        public void InitializeComponent()
        {
            this.SuspendLayout();

            _clear = new System.Windows.Forms.ToolStripButton();
            _clear.Name = "_clear";
            _clear.Image = Properties.Resources.layer_delete;
            _clear.Click += OnRemoveFeatures;
            _clear.ToolTipText += "Removes all geometries from the geometry layer";

            _sep1 = new System.Windows.Forms.ToolStripSeparator();
            _sep1.Size = new System.Drawing.Size(6,6);

            _addPoint = new System.Windows.Forms.ToolStripButton();
            _addPoint.Name = "_addPoint";
            _addPoint.Image = Properties.Resources.point_create;
            _addPoint.CheckOnClick = true;
            _addPoint.CheckedChanged += OnCheckedChanged;
            _addPoint.MouseDown += new MouseEventHandler(OnMouseDown);
            _addPoint.ToolTipText += "Adds a point to the geometry layer";

            _addLineString = new System.Windows.Forms.ToolStripButton();
            _addLineString.Name = "_addLineString";
            _addLineString.Image = Properties.Resources.line_create;
            _addLineString.MouseDown += OnMouseDown;
            _addLineString.CheckOnClick = true;
            _addLineString.CheckedChanged += OnCheckedChanged;
            _addLineString.ToolTipText += "Adds a linestring to the geometry layer";

            _addPolygon = new System.Windows.Forms.ToolStripButton();
            _addPolygon.Name = "_addPolygon";
            _addPolygon.Image = Properties.Resources.polygon_create;
            _addPolygon.CheckOnClick = true;
            _addPolygon.MouseDown += OnMouseDown;
            _addPolygon.CheckedChanged += OnCheckedChanged;
            _addPolygon.ToolTipText += "Adds a linestring to the geometry layer";

            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _clear, _sep1, _addPoint, _addLineString, _addPolygon,
                /* _addPolygonRing, _sep2, _moveFeature, _moveVertex */
                });

            this.ResumeLayout();
            this.PerformLayout();

            this.Visible = true;

            this.GeometryDefinedHandler = DefaultGeometryDefinedMethod;
        }

        protected virtual void OnMouseDown(object sender, MouseEventArgs e)
        {
            Debug.WriteLine(string.Format("\nButtonClicked '{0}'", ((ToolStripButton)sender).Name));
            if (Logger.IsDebugEnabled)
                Logger.DebugFormat("\nButtonClicked '{0}'",((ToolStripButton)sender).Name);
        }

        protected override void OnMapControlChangingInternal(System.ComponentModel.CancelEventArgs e)
        {
            base.OnMapControlChangingInternal(e);

            if (MapControl == null) return;

            MapControl.ActiveToolChanged -= OnMapControlActiveToolChanged;
            MapControl.GeometryDefined -= OnGeometryDefined;
        }

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

            _geometryProvider = new SharpMap.Data.Providers.GeometryProvider((GeoAPI.Geometries.IGeometry)null);
            _layer = new SharpMap.Layers.VectorLayer("_tmp_Geometries", _geometryProvider);

            MapControl.ActiveToolChanged += OnMapControlActiveToolChanged;
            MapControl.GeometryDefined += OnGeometryDefined;


            
        }

        private void OnRemoveFeatures(object sender, EventArgs e)
        {
            if (_geometryProvider != null)
                _geometryProvider.Geometries.Clear();
            if (MapControl != null)
                MapControl.Refresh();
        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            if (MapControl == null) return;

            var checkedButton = (System.Windows.Forms.ToolStripButton)sender;
            
            MapBox.Tools newTool;
            if (sender == _addPoint)
                newTool = MapBox.Tools.DrawPoint;
            else if (sender == _addLineString)
                newTool = MapBox.Tools.DrawLine;
            else if (sender == _addPolygon)
                newTool = MapBox.Tools.DrawPolygon;
            else
            {
                if (Logger.IsWarnEnabled)
                    Logger.Warn("Unknown object invoking OnCheckedChanged()");
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

        private void OnGeometryDefined(GeoAPI.Geometries.IGeometry geometry)
        {
            if (geometry == null)
                return;

            if (GeometryDefinedHandler != null)
            {
                GeometryDefinedHandler(geometry);
                return;
            }

        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public SharpMap.Forms.MapBox.GeometryDefinedHandler GeometryDefinedHandler{ get; set; }

        private void DefaultGeometryDefinedMethod(GeoAPI.Geometries.IGeometry geom)
        {
            using (var frm = new WktGeometryCreator())
            {
                frm.Geometry = geom;
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
