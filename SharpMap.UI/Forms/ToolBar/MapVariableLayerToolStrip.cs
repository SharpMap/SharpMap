using System;
using System.ComponentModel;
using Common.Logging;

namespace SharpMap.Forms.ToolBar
{
    [System.ComponentModel.DesignTimeVisible(true)]
    public class MapVariableLayerToolStrip : MapToolStrip
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        private System.Windows.Forms.ToolStripButton _enableVariableLayers;
        private System.Windows.Forms.ToolStripTextBox _interval;
        private System.Timers.Timer _timer;

        public MapVariableLayerToolStrip()
            :base()
        {
            InitializeComponent();
        }

        public MapVariableLayerToolStrip(IContainer container)
            : base(container)
        {
            InitializeComponent();
        }


        public void InitializeComponent()
        {
            this.SuspendLayout();

            _enableVariableLayers = new System.Windows.Forms.ToolStripButton();
            _enableVariableLayers.Name = "_enableVariableLayers";
            _enableVariableLayers.Image = Properties.Resources.hide;
            _enableVariableLayers.CheckOnClick = true;
            _enableVariableLayers.CheckedChanged += OnCheckedChanged;

            _interval = new System.Windows.Forms.ToolStripTextBox();
            _interval.Text = "500";
            _interval.Enabled = false;
            _interval.TextChanged += OnTextChanged;

            _timer = new System.Timers.Timer();
            _timer.Interval = 500;
            _timer.Elapsed += OnTouchTimer;

            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            { _enableVariableLayers, _interval });

            this.ResumeLayout();
            this.PerformLayout();
            this.Visible = true;
        }

        protected override void OnMapControlChangingInternal(System.ComponentModel.CancelEventArgs e)
        {
            base.OnMapControlChangingInternal(e);

            _enableVariableLayers.Enabled = MapControl != null;
            _timer.Enabled = MapControl != null && _enableVariableLayers.Enabled;
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            int val;
            if (int.TryParse(_interval.Text, System.Globalization.NumberStyles.Integer, 
                System.Globalization.NumberFormatInfo.InvariantInfo, out val))
            {
                if (val < 500) val = 500;
                _timer.Interval = val;
            }
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
            if (Logger.IsDebugEnabled)
                Logger.DebugFormat("Touching timer at {0}", e.SignalTime);
            SharpMap.Layers.VariableLayerCollection.TouchTimer();
        }
    }
}
