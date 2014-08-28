// Copyright 2014 - Spartaco Giubbolini, portions by Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.UI.
// SharpMap.UI is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.UI is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Logging;
using GeoAPI.Geometries;

namespace SharpMap.Forms
{
    /// <summary>
    /// This control displays a minimap of the whole extension of a map, and let the user drag the viewport.
    /// </summary>
    [Serializable]
    public partial class MiniMapControl : Control
    {
        #region fields
        
        private MapBox _mapBoxControl;

        [NonSerialized]
        private Timer _resizeTimer;
        private int _resizeInterval = 500;

        private volatile int _generation;
        private volatile Map _currentMap;

        private Rectangle _frame;
        private bool _mouseDown;
        private Point _mouseDownLocation;
        private BorderStyle _borderStyle = BorderStyle.Fixed3D;
        private DashStyle _framePenDashStyle = DashStyle.Solid;
        private PenAlignment _framePenAlignment = PenAlignment.Center;
        private int _frameHalo;
        private Color _frameBrushColor = Color.Red;
        private Color _framePenColor = Color.Red;
        private int _framePenWidth = 2;
        private float _opacity = 0.0f;
        private Color _frameHaloColor;

        #endregion

        #region ctor

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public MiniMapControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.UserPaint, true);
            base.DoubleBuffered = true;

            _resizeTimer = new Timer { Interval = ResizeInterval };
            _resizeTimer.Tick += HandleResizeTimerTick;
        } 
        #endregion

        #region properties
        
        /// <summary>
        /// Gets or sets a value indicating the <see cref="MapBox"/> control linked to the mini map.
        /// </summary>
        [DefaultValue(null)]
        public MapBox MapControl
        {
            get { return _mapBoxControl; }
            set
            {
                UnhookEvents();

                _mapBoxControl = value;

                HookEvents();

                Cursor = value == null ? Cursors.Default : Cursors.Hand;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the border style
        /// </summary>
        [Category("Appearance")]
        [Description("Defines the border style")]
        [DefaultValue(BorderStyle.Fixed3D)]
        public BorderStyle BorderStyle  
        {
            get { return _borderStyle; }
            set
            {
                if (_borderStyle == value)
                    return;
                _borderStyle = value;
                UpdateStyles();
                //OnBorderStyleChanged(EventArgs.Empty);
            }
        }

        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 65536;
                createParams.ExStyle &= -513;
                createParams.Style &= -8388609;
                switch (_borderStyle)
                {
                    case BorderStyle.FixedSingle:
                        createParams.Style |= 8388608;
                        break;
                    case BorderStyle.Fixed3D:
                        createParams.ExStyle |= 512;
                        break;
                }
                return createParams;
            }
        }

        ///// <summary>
        ///// Event raised when <see cref="BorderStyle"/> has changed
        ///// </summary>
        //public event EventHandler BorderStyleChanged;

        ///// <summary>
        ///// Event invoker for the <see cref="BorderStyleChanged"/> event.
        ///// </summary>
        ///// <param name="e">The event's arguments</param>
        //protected virtual void OnBorderStyleChanged(EventArgs e)
        //{
        //    var h = BorderStyleChanged;
        //    if (h != null) h(this, e);
        //}

        /// <summary>
        /// Gets or sets a value indicating the color of the pen that draws the frame
        /// </summary>
        [Description("The color of the pen that draws the frame")]
        [Category("Frame Appearance")]
        [DefaultValue(typeof (Color), "Red")]
        public Color FramePenColor
        {
            get { return _framePenColor; }
            set
            {
                if (value == _framePenColor)
                    return; 
                _framePenColor = value;
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the color of the brush that fills the frame
        /// </summary>
        [Description("The color of the brush that fills the frame")]
        [Category("Frame Appearance")]
        [DefaultValue(typeof(Color), "Transparent")]
        public Color FrameBrushColor        
        {
            get { return _frameBrushColor; }
            set
            {
                if (value == _frameBrushColor)
                    return;
                _frameBrushColor = value;
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the width of the pen that draws the frame
        /// </summary>
        [Description("The width of the pen that draws the frame")]
        [Category("Frame Appearance")]
        [DefaultValue(2)]
        public int FramePenWidth
        {
            get { return _framePenWidth; }
            set
            {
                if (value < 1) value = 1;
                if (value == _framePenWidth)
                    return;
                
                _framePenWidth = value;
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the opacity of the brush used to fill the frame
        /// </summary>
        [Description("The opacity of the brush used to fill the frame")]
        [Category("Frame Appearance")]
        [DefaultValue(0f)]
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                if (value < 0f) value = 0f;
                if (value > 1f) value = 1f;
                if (value == _opacity)
                    return;
                _opacity = value;
                Refresh();
            }
        }

        #endregion

        #region protected members

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
                return;

            _mouseDown = true;
            _mouseDownLocation = e.Location;

            Cursor.Clip = RectangleToScreen(ClientRectangle);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouseDown)
            {
                _frame = CalculateNewFrame(e);

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (MapControl != null && _currentMap != null && _mouseDown)
            {
                var x = e.X;
                var y = e.Y;

                var newCenter = _currentMap.ImageToWorld(new PointF(x, y));

                MapControl.Map.Center = newCenter;
                MapControl.Refresh();

                _frame = CalculateNewFrame(e);
                Invalidate();

                Cursor.Clip = Rectangle.Empty;
                _mouseDown = false;
                _mouseDownLocation = Point.Empty;
            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DesignMode)
            {
                var width = Convert.ToInt32(ClientSize.Width*0.5);
                var height = width * (MapControl != null
                    ? (double)MapControl.ClientSize.Height/MapControl.ClientSize.Width
                    : (double)ClientSize.Height/ClientSize.Width);

                DrawFrame(e.Graphics, new Rectangle(
                    Convert.ToInt32(ClientSize.Width*0.2), Convert.ToInt32(ClientSize.Height*0.3), 
                    width, Convert.ToInt32(height)));
                
                //DrawFrame(e.Graphics, new Rectangle(
                //    Convert.ToInt32(ClientSize.Width * 0.2), Convert.ToInt32(ClientSize.Height * 0.3),
                //    Convert.ToInt32(ClientSize.Width * 0.5), Convert.ToInt32(ClientSize.Height * 0.7)));
            }
            else
            {
                DrawFrame(e.Graphics, _frame);
            }
        }

        #endregion

        #region private members

        private Rectangle CalculateNewFrame(MouseEventArgs e)
        {
            var x = e.X;
            var y = e.Y;

            return new Rectangle(
                x - Convert.ToInt32(_frame.Width * 0.5),
                y - Convert.ToInt32(_frame.Height * 0.5),
                Convert.ToInt32(_frame.Width),
                Convert.ToInt32(_frame.Height));
        }

        private void HookEvents()
        {
            if (_mapBoxControl == null)
                return;

            _mapBoxControl.MapRefreshed += OnMapBoxRendered;
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, EventArgs eventArgs)
        {
            LogManager.GetCurrentClassLogger().Debug(fmh => fmh("SizeChanged"));
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void OnMapBoxRendered(object sender, EventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(CreateMiniMap));
            else
                CreateMiniMap();
        }

        private void CreateMiniMap()
        {
            var clonedMap = _mapBoxControl.Map.Clone();
            clonedMap.Decorations.Clear();

            var tsk = Task.Factory.StartNew(new Func<object, Tuple<Image, int, Rectangle>>(GenerateMap), new object[] { ++_generation, clonedMap });
            tsk.ContinueWith(t =>
            {
                if (t.Result != null)
                {
                    if (t.Result.Item2 == _generation)
                    {
                        _currentMap = clonedMap;
                        BackgroundImage = t.Result.Item1;
                        _frame = t.Result.Item3;
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UnhookEvents()
        {
            if (_mapBoxControl == null)
                return;

            _mapBoxControl.MapRefreshed -= OnMapBoxRendered;
            SizeChanged -= OnMapBoxRendered;
        }

        private void DrawFrame(Graphics g, Rectangle rect)
        {
            if (rect == Rectangle.Empty)
                return;

            var oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (FrameBrushColor != Color.Transparent && Opacity != 0)
            {
                using (var b = new SolidBrush(Color.FromArgb(Convert.ToInt32(Opacity*255), FrameBrushColor))) 
                    g.FillRectangle(b, rect);
            }

            if (FrameHalo > 0)
            {
                using (var newPen = new Pen(FrameHaloColor, FramePenWidth + _frameHalo))
                {
                    g.DrawRectangle(newPen, rect);
                }
            }

            using (var newPen = new Pen(FramePenColor, FramePenWidth))
            {
                newPen.DashStyle = FramePenDashStyle;
                newPen.Alignment = FramePenAlignment;
                g.DrawRectangle(newPen, rect);
            }

            g.SmoothingMode = oldSmoothingMode;
        }
        
        /// <summary>
        /// Gets or sets a value indicating if the viewport frame should have a halo
        /// </summary>
        [Description("If set, a halo effect is drawn around the viewport frame")]
        [Category("Frame Appearance")]
        [DefaultValue(0)]
        public int FrameHalo
        {
            get { return _frameHalo; }
            set
            {
                if (value == _frameHalo)
                    return; 
                _frameHalo = value;
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the color of the the viewport frame halo
        /// </summary>
        [Description("If set, a halo effect is drawn around the viewport frame")]
        [Category("Frame Appearance")]
        [DefaultValue(typeof (Color), "System.Drawing.Color.White")]
        public Color FrameHaloColor
        {
            get { return _frameHaloColor; }
            set
            {
                if (_frameHaloColor == value)
                    return;
                _frameHaloColor = value;
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the pen alignment used when drawing the frame
        /// </summary>
        [Description("The pen alignment used when drawing the frame")]
        [Category("Frame Appearance")]
        [DefaultValue(typeof(PenAlignment), "System.Drawing.PenAlignment.Center")]
        public PenAlignment FramePenAlignment
        {
            get { return _framePenAlignment; }
            set
            {
                if (value == _framePenAlignment)
                    return;
                _framePenAlignment = value;
                Refresh();
                //OnFramePenAlignmentChanged(EventArgs.Empty);
            }
        }

        ///// <summary>
        ///// Event raised when the <see cref="FramePenAlignment"/> has changed
        ///// </summary>
        //public event EventHandler FramePenAlignmentChanged;

        ///// <summary>
        ///// Event invoker for the <see cref="FramePenAlignmentChanged"/> event.
        ///// </summary>
        ///// <param name="e">The events arguments</param>
        //protected virtual void OnFramePenAlignmentChanged(EventArgs e)
        //{
        //    Refresh();
        //    var h = FramePenAlignmentChanged;
        //    if (h != null) h(this, e);
        //}

        /// <summary>
        /// Gets or sets a value indicating the pen alignment used when drawing the frame
        /// </summary>
        [Description("The dash style used when drawing the frame")]
        [Category("Frame Appearance")]
        [DefaultValue(typeof(DashStyle), "System.Drawing.DashStyle.Solid")]
        public DashStyle FramePenDashStyle
        {
            get { return _framePenDashStyle; }
            set
            {
                if (value == _framePenDashStyle)
                    return;
                if (value == DashStyle.Custom)
                    throw new NotSupportedException("DashStyle.Custom is not supported");
                _framePenDashStyle = value;

                Refresh();
                //OnFramePenDashStyleChanged(EventArgs.Empty);
            }
        }

        ///// <summary>
        ///// Event raised when the <see cref="FramePenDashStyle"/> has changed
        ///// </summary>
        //public event EventHandler FramePenDashStyleChanged;

        ///// <summary>
        ///// Event invoker for the <see cref="FramePenDashStyleChanged"/> event.
        ///// </summary>
        ///// <param name="e">The events arguments</param>
        //protected virtual void OnFramePenDashStyleChanged(EventArgs e)
        //{
        //    Refresh();
        //    var h = FramePenDashStyleChanged;
        //    if (h != null) h(this, e);
        //}

        private Tuple<Image, int, Rectangle> GenerateMap(object state)
        {
            try
            {
                var args = (object[])state;

                var currentGeneration = (int)args[0];
                var map = (Map)args[1];

                Rectangle frame;

                Image img = null;
                if (map.Layers.Count > 0)
                {
                    var originalCenter = map.Center;
                    var originalWidth = map.Zoom;
                    var originalHeight = map.MapHeight;

                    var wx1 = originalCenter.X - originalWidth*0.5;
                    var wy1 = originalCenter.Y - originalHeight*0.5;

                    var wx2 = originalCenter.X + originalWidth*0.5;
                    var wy2 = originalCenter.Y + originalHeight*0.5;

                    map.Size = Size;
                    map.ZoomToExtents();

                    img = map.GetMap();

                    var np1 = map.WorldToImage(new Coordinate(wx1, wy1));
                    var np2 = map.WorldToImage(new Coordinate(wx2, wy2));

                    frame = new Rectangle(
                        Convert.ToInt32(np1.X),
                        Convert.ToInt32(np2.Y),
                        Convert.ToInt32(Math.Abs(np2.X - np1.X)),
                        Convert.ToInt32(Math.Abs(np1.Y - np2.Y)));
                }
                else
                {
                    frame = Rectangle.Empty;
                }

                return new Tuple<Image, int, Rectangle>(img, currentGeneration, frame);
            }
            catch (Exception)
            {
                return null;
            }

        }

        private void HandleResizeTimerTick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            OnMapBoxRendered(sender, e);
        }

        //public event EventHandler ResizeIntervalChanged;

        //protected virtual void OnResizeIntervalChanged(EventArgs e)
        //{
        //    _resizeTimer.Stop();
        //    _resizeTimer.Interval = _resizeInterval;
        //    _resizeTimer.Start();

        //    var handler = ResizeIntervalChanged;
        //    if (handler != null) handler(this, e);
        //}

        /// <summary>
        /// Gets or sets a value indicating the interval between two MapControl.Resize events
        /// </summary>
        public int ResizeInterval
        {
            get { return _resizeInterval; }
            set
            {
                if (value == _resizeInterval)
                if (value < 1)
                    throw new ArgumentException("The resize interval must be a positive value");
                _resizeInterval = value;

                _resizeTimer.Stop();
                _resizeTimer.Interval = _resizeInterval;
                _resizeTimer.Start();

                //OnResizeIntervalChanged(EventArgs.Empty);
            }
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            _resizeTimer = new Timer { Interval = _resizeInterval };
            _resizeTimer.Tick += HandleResizeTimerTick;
            _resizeTimer.Start();
        }

        #endregion
    }
}
