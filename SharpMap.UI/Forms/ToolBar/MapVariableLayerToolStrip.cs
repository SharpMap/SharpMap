using System;
using System.ComponentModel;
using Common.Logging;
using SharpMap.Forms.ImageGenerator;

namespace SharpMap.Forms.ToolBar
{
    /// <summary>
    /// A pre-configured tool strip for the handling of Map's VariableLayers collection
    /// </summary>
    [DesignTimeVisible(true)]
    public class MapVariableLayerToolStrip : MapToolStrip
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MapVariableLayerToolStrip));

        private System.Windows.Forms.ToolStripButton _enableVariableLayers;
        private System.Windows.Forms.ToolStripTextBox _interval;
        private System.Timers.Timer _timer;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public MapVariableLayerToolStrip()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="container">A container for components</param>
        public MapVariableLayerToolStrip(IContainer container)
            : base(container)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this tool strip
        /// </summary>
        public void InitializeComponent()
        {
            SuspendLayout();

            _enableVariableLayers = new System.Windows.Forms.ToolStripButton();
            _enableVariableLayers.Name = "_enableVariableLayers";
            _enableVariableLayers.Image = Properties.Resources.hide;
            _enableVariableLayers.CheckOnClick = true;
            _enableVariableLayers.CheckedChanged += OnCheckedChanged;

            _interval = new System.Windows.Forms.ToolStripTextBox();
            _interval.Text = @"500";
            _interval.Enabled = false;
            _interval.TextChanged += OnTextChanged;

            _timer = new System.Timers.Timer();
            _timer.Interval = 500;
            _timer.Elapsed += OnTouchTimer;

            Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            { _enableVariableLayers, _interval });

            ResumeLayout();
            PerformLayout();
            Visible = true;
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Enabled = false;
                _timer = null;
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc cref="OnMapControlChangingInternal"/>
        protected override void OnMapControlChangingInternal(CancelEventArgs e)
        {
            base.OnMapControlChangingInternal(e);

            _enableVariableLayers.Enabled = MapControl != null;
            _timer.Enabled = MapControl != null && _enableVariableLayers.Enabled;
        }

        /// <inheritdoc cref="OnMapControlChangedInternal"/>
        protected override void OnMapControlChangedInternal(EventArgs e)
        {
            base.OnMapControlChangedInternal(e);

            if (MapControl?.ImageRenderer is LayerListImageRenderer llig)
                _interval.Text = llig.RefreshInterval.ToString("D");
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(_interval.Text, System.Globalization.NumberStyles.Integer,
                System.Globalization.NumberFormatInfo.InvariantInfo, out int val)) return;

            if (val < 50) val = 50;
            if (val == _timer.Interval)
                return;

            _timer.Interval = val;
            if (MapControl.ImageRenderer is LayerListImageRenderer llig)
                llig.RefreshInterval = val;
        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            if (_enableVariableLayers.Checked)
            {
                _enableVariableLayers.Image = Properties.Resources.show;
                _timer.Enabled = true;
                _interval.Enabled = true;
                return;
            }

            _enableVariableLayers.Image = Properties.Resources.hide;
            _interval.Enabled = false;
            _timer.Enabled = false;


        }

        private void OnTouchTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_logger.IsDebugEnabled)
                _logger.DebugFormat("Touching timer at {0}", e.SignalTime);

            MapControl.Map.VariableLayers.TouchTimer();

        }
    }
}
