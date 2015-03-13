using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using GeoAPI.Geometries;
using SharpMap;
using SharpMap.Forms;
using SharpMap.Forms.Tools;

namespace WinFormSamples
{
    public class MagnifierTool : MapTool
    {
        
        private readonly MapBox _parentMapBox;
        private readonly PictureBox _magnified;
        private Map _map;
        private double _magnification = 2;
        private Size _offset;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public MagnifierTool(MapBox parentMapBox) 
            : base("Magnifier", "A tool to magnify the portion of the map below the cursor")
        {
            _parentMapBox = parentMapBox;
            _parentMapBox.MapChanged += HandleMapChanged;
            Map = _parentMapBox.Map;

            MagnificationFactor = 1.10;

            Offset = new Size(5,5);

            _magnified = new PictureBox();
            _magnified.Size = new Size(75, 75);
            _magnified.BorderStyle = BorderStyle.FixedSingle;
            _magnified.Visible = false;

            _parentMapBox.Controls.Add(_magnified);

            Map = _parentMapBox.Map;
            _map = Map.Clone();
            _map.Size = _magnified.Size;
            _map.Zoom = _map.Size.Width*(Map.Envelope.Width/Map.Size.Width) / _magnification;
            _map.Center = _map.Center;
            _magnified.Image = _map.GetMap();

            Enabled = true;

            var ms = Assembly.GetExecutingAssembly().GetManifestResourceStream("WinFormSamples.Magnifier.cur");
            if (ms != null)
                Cursor = new Cursor(ms);
        }

        private void HandleMapChanged(object sender, EventArgs e)
        {
            _map = Map.Clone();
            _map.Size = _magnified.Size;
            _map.Zoom = _map.Size.Width * (Map.Envelope.Width / Map.Size.Width) / _magnification;
            _map.Center = _map.Center;
            //_magnified.Image = _map.GetMap();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MagnifierTool()
        {
            if (_magnified.IsHandleCreated)
            {
                var mb = _magnified;
                if (!_parentMapBox.IsDisposed)
                {
                    _parentMapBox.Invoke(new MethodInvoker(() => _parentMapBox.Controls.Remove(mb)));
                }
                mb.Dispose();
            }

        }

        public override bool DoKeyUp(Coordinate mapPosition, KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.KeyCode)
            {
                case Keys.Add:
                    Magnification = Magnification * MagnificationFactor;
                    return true;
                case Keys.Subtract:
                    Magnification = Magnification / MagnificationFactor;
                    return true;
            }
            return base.DoKeyUp(mapPosition, keyEventArgs);
        }

        public override bool DoMouseLeave()
        {
            if (!Enabled) return false;

            _magnified.Visible = false;
            return true;
        }

        public override bool DoMouseWheel(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            var factor = 1d;
            if (Math.Sign(_parentMapBox.WheelZoomMagnitude)*mouseEventArgs.Delta > 0)
                factor = MagnificationFactor;
            else
                factor = 1/MagnificationFactor;
            Magnification *= factor;
            //_map.Zoom *= Math.Sign(mouseEventArgs.Delta)*MagnificationFactor * Math.Sign(_parentMapBox.WheelZoomMagnitude);
            //_magnified.Image = _map.GetMap();
            
            return base.DoMouseWheel(mapPosition, mouseEventArgs);
        }

        public override bool DoMouseEnter()
        {
            if (!Enabled) return false;

            _magnified.Visible = true;
            return true;
        }

        public double Magnification
        {
            get { return _magnification; }
            set
            {
                if (value <= 1d) value = 1d;
                if (value == _magnification)
                    return;

                var old = _magnification;
                _magnification = value;
                OnMagnificationChanged(_magnification/old);
            }
        }
        
        /// <summary>
        /// Event raised when the magnification factor has changed
        /// </summary>
        public event EventHandler MagnificationChanged;

        /// <summary>
        /// Event invoker for the <see cref="MagnificationChanged"/> event
        /// </summary>
        /// <param name="change"></param>
        private void OnMagnificationChanged(double change)
        {
            _map.Zoom /= change;
            
            var h = MagnificationChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        
        /// <summary>
        /// Gets or sets a value indicating by which factor the magnification has to change on scroll or key
        /// </summary>
        public double MagnificationFactor { get; set; }
        
        public override bool DoMouseMove(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            if (!Enabled) return false;

            _map.Center = mapPosition;
            _magnified.Image = _map.GetMap();
            _magnified.Visible = true;
            
            var newLocation = Point.Add(mouseEventArgs.Location, Offset);
            if (newLocation.X + _magnified.Size.Width > _parentMapBox.ClientSize.Width)
                newLocation.X = mouseEventArgs.Location.X - Offset.Width - _magnified.Size.Width;
            if (newLocation.Y + _magnified.Size.Height > _parentMapBox.ClientSize.Height)
                newLocation.Y = mouseEventArgs.Location.Y - Offset.Height - _magnified.Size.Height;

            _magnified.Location = newLocation;
            _parentMapBox.Refresh();
            return base.DoMouseMove(mapPosition, mouseEventArgs);
        }

        public Size Offset
        {
            get { return _offset; }
            set
            {
                if (value.Width < 0 || value.Height < 0)
                    throw new ArgumentException("value");
                
                if (value == _offset)
                    return;

                var old = _offset;
                _offset = value;
                if (Enabled)
                    _magnified.Location = Point.Add(Point.Subtract(_magnified.Location, old), _offset);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (!Enabled)
            {
                _magnified.Visible = false;
            }
            else
            {
                _magnified.BringToFront();
            }
        }
    }
}
