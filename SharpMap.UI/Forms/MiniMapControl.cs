// Copyright 2014 - Spartaco Giubbolini (spartaco@sgsoftware.it), portions by Felix Obermaier (www.ivv-aachen.de)
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
using GeoAPI.Geometries;

namespace SharpMap.Forms
{
    /// <summary>
    /// This control displays a minimap of the whole extension of a map, and let the user drag the viewport.
    /// </summary>
    [Serializable]
    public partial class MiniMapControl : Control
    {
        /// <summary>
        /// Enumeration of possible hit results
        /// </summary>
        protected enum HitResult
        {
            /// <summary>
            /// Not hit anything at all
            /// </summary>
            None,

            /// <summary>
            /// Hit inside the frame
            /// </summary>
            InsideFrame,

            SizeNW,
            SizeNE,
            SizeSW,
            SizeSE
        }

        #region fields

        private const int CornerMouseLength = 8;
        private const int CornerMouseSensibility = 1;

        private MapBox _mapBoxControl;

        [NonSerialized]
        private Timer _resizeTimer;
        private int _resizeInterval = 500;

        private volatile int _generation;
        private volatile Map _currentMap;

        private Rectangle _frame;
        private bool _mouseDown;
        private BorderStyle _borderStyle = BorderStyle.Fixed3D;
        private DashStyle _framePenDashStyle = DashStyle.Solid;
        private PenAlignment _framePenAlignment = PenAlignment.Center;
        private int _frameHalo;
        private Color _frameBrushColor = Color.Red;
        private Color _framePenColor = Color.Red;
        private int _framePenWidth = 2;
        private float _opacity;
        private Color _frameHaloColor;
        private Point _translate;
        private Cursor _oldCursor;
        private HitResult _hitResult;
        private Guid _mapId = Guid.NewGuid();

        [ThreadStatic]
        private static bool _isRenderingThread;

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
        /// Gets a value indicating if the current thread is rendering the map.
        /// </summary>
        /// <remarks>A layer could use this value to customize the rendering into the minimap.</remarks>
        public static bool IsRenderingThread
        {
            get { return _isRenderingThread; }
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
                    return;
                if (value < 1)
                    throw new ArgumentException("The resize interval must be a positive value");
                _resizeInterval = value;

                _resizeTimer.Stop();
                _resizeTimer.Interval = _resizeInterval;
                _resizeTimer.Start();

                //OnResizeIntervalChanged(EventArgs.Empty);
            }
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
        [DefaultValue(typeof(Color), "System.Drawing.Color.White")]
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
        [DefaultValue(typeof(Color), "Red")]
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

        /// <summary>
        /// Returns a value that determines what the provided location hits
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected HitResult HitTest(int x, int y)
        {
            if ((x >= _frame.X - CornerMouseSensibility &&
                 x <= _frame.X + CornerMouseLength &&
                 y >= _frame.Y - CornerMouseSensibility && y <= _frame.Y + CornerMouseSensibility) ||
                (x >= _frame.X - CornerMouseSensibility && x <= _frame.X + CornerMouseSensibility &&
                 y >= _frame.Y - CornerMouseSensibility &&
                 y <= _frame.Y + CornerMouseLength))
                return HitResult.SizeNW;

            if ((x >= _frame.Right - CornerMouseLength && x < _frame.Right + CornerMouseSensibility &&
                 y >= _frame.Y - CornerMouseSensibility && y <= _frame.Y + CornerMouseSensibility) ||
                (x >= _frame.Right - CornerMouseSensibility && x <= _frame.Right + CornerMouseSensibility &&
                 y >= _frame.Y - CornerMouseSensibility && y <= _frame.Y + CornerMouseLength))
                return HitResult.SizeNE;

            if ((x >= _frame.X - CornerMouseSensibility && x <= _frame.X + CornerMouseLength &&
                 y >= _frame.Bottom - CornerMouseLength && y <= _frame.Bottom + CornerMouseSensibility) ||
                (x >= _frame.X - CornerMouseSensibility && x <= _frame.X + CornerMouseLength &&
                 y >= _frame.Bottom - CornerMouseLength && y <= _frame.Bottom + CornerMouseSensibility))
                return HitResult.SizeSW;

            if ((x >= _frame.Right - CornerMouseLength && x < _frame.Right + CornerMouseSensibility &&
                 y >= _frame.Bottom - CornerMouseSensibility && y <= _frame.Bottom + CornerMouseSensibility) ||
                (x >= _frame.Right - CornerMouseSensibility && x <= _frame.Right + CornerMouseSensibility &&
                 y >= _frame.Bottom - CornerMouseLength && y <= _frame.Bottom + CornerMouseSensibility))
                return HitResult.SizeSE;

            if (x >= _frame.X && x <= _frame.Right && y >= _frame.Y && y <= _frame.Bottom)
                return HitResult.InsideFrame;

            return HitResult.None;
        }

        public override void Refresh()
        {
            CreateMiniMap();
            base.Refresh();
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

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_resizeTimer != null))
            {
                _resizeTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
                return;

            if (_hitResult == HitResult.None)
            {
                _translate = Point.Empty;
                _frame = CalculateNewFrame(e.X, e.Y);
            }
            else
            {
                _translate = new Point(Convert.ToInt32(e.X - (_frame.X + _frame.Width * 0.5)),
                    Convert.ToInt32(e.Y - (_frame.Y + _frame.Height * 0.5)));
            }

            Rectangle clippingRect;

            switch (_hitResult)
            {
                case HitResult.None:
                case HitResult.InsideFrame:
                    clippingRect = ClientRectangle;
                    break;

                case HitResult.SizeNW:
                    clippingRect = new Rectangle(ClientRectangle.X, ClientRectangle.Y, _frame.Right, _frame.Bottom);
                    break;

                case HitResult.SizeNE:
                    clippingRect = new Rectangle(_frame.X, ClientRectangle.Y, ClientRectangle.Width - _frame.X, _frame.Bottom);
                    break;

                case HitResult.SizeSW:
                    clippingRect = new Rectangle(ClientRectangle.X, _frame.Y, _frame.Right - ClientRectangle.X,
                        ClientRectangle.Height - _frame.Y);
                    break;

                case HitResult.SizeSE:
                    clippingRect = new Rectangle(_frame.X, _frame.Y, ClientRectangle.Width - _frame.X,
                        ClientRectangle.Height - _frame.Y);
                    break;

                default:
                    clippingRect = ClientRectangle;
                    break;
            }

            Cursor.Clip = RectangleToScreen(clippingRect);

            _mouseDown = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouseDown)
            {
                switch (_hitResult)
                {
                    case HitResult.InsideFrame:
                        _frame = CalculateNewFrame(e.X - _translate.X, e.Y - _translate.Y);
                        break;

                    case HitResult.None:
                        _frame = CalculateNewFrame(e.X, e.Y);
                        break;

                    case HitResult.SizeNW:
                        _frame = new Rectangle(e.X, e.Y, _frame.Right - e.X, _frame.Bottom - e.Y);
                        break;

                    case HitResult.SizeNE:
                        _frame = new Rectangle(_frame.X, e.Y, e.X - _frame.X, _frame.Bottom - e.Y);
                        break;

                    case HitResult.SizeSW:
                        _frame = new Rectangle(e.X, _frame.Y, _frame.Right - e.X, e.Y - _frame.Y);
                        break;

                    case HitResult.SizeSE:
                        _frame = new Rectangle(_frame.X, _frame.Y, e.X - _frame.X, e.Y - _frame.Y);
                        break;
                }


                Invalidate();
            }
            else
            {
                _hitResult = HitTest(e.X, e.Y);
                if (_hitResult == HitResult.SizeNW || _hitResult == HitResult.SizeSE)
                {
                    if (_oldCursor == null)
                        _oldCursor = Cursor;
                    Cursor = Cursors.SizeNWSE;
                }
                else if (_hitResult == HitResult.SizeNE || _hitResult == HitResult.SizeSW)
                {
                    if (_oldCursor == null)
                        _oldCursor = Cursor;

                    Cursor = Cursors.SizeNESW;
                }
                else if (_oldCursor != null)
                {
                    Cursor = _oldCursor;
                    _oldCursor = null;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (MapControl != null && _currentMap != null && _mouseDown)
            {
                var newCenter =
                    _currentMap.ImageToWorld(new PointF(
                        Convert.ToInt32(_frame.X + _frame.Width * 0.5),
                        Convert.ToInt32(_frame.Y + _frame.Height * 0.5)));

                MapControl.Map.Center = newCenter;

                if (_hitResult != HitResult.None && _hitResult != HitResult.InsideFrame)
                {
                    var p1 = _currentMap.ImageToWorld(_frame.Location);
                    var p2 = _currentMap.ImageToWorld(new PointF(_frame.X + _frame.Width, _frame.Y + _frame.Height));

                    MapControl.Map.ZoomToBox(new Envelope(p1.X, p2.X, p1.Y, p2.Y));
                }

                MapControl.Refresh();

                Invalidate();

                Cursor.Clip = Rectangle.Empty;
            }

            _mouseDown = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            try
            {
                if (DesignMode)
                {
                    var width = Convert.ToInt32(ClientSize.Width * 0.5);
                    var height = width * (MapControl != null
                        ? (double)MapControl.ClientSize.Height / MapControl.ClientSize.Width
                        : (double)ClientSize.Height / ClientSize.Width);

                    DrawFrame(e.Graphics, new Rectangle(
                        Convert.ToInt32(ClientSize.Width * 0.2), Convert.ToInt32(ClientSize.Height * 0.3),
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
            catch (OverflowException)
            {
                // sometimes can happen this exception if the frame rectangle is too big, we can simply skip it
            }
        }

        #endregion

        #region private members

        private Rectangle CalculateNewFrame(int x, int y)
        {
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
            if (_mapBoxControl == null)
                return;

            var clonedMap = _mapBoxControl.Map.Clone();
            clonedMap.ID = _mapId;
            clonedMap.Decorations.Clear();
            clonedMap.EnforceMaximumExtents = false;
            clonedMap.MaximumExtents = null;
            clonedMap.MinimumZoom = Double.Epsilon;
            clonedMap.MaximumZoom = Double.MaxValue;

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
                using (var b = new SolidBrush(Color.FromArgb(Convert.ToInt32(Opacity * 255), FrameBrushColor)))
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

        private Tuple<Image, int, Rectangle> GenerateMap(object state)
        {
            var args = (object[])state;
            var currentGeneration = (int)args[0];

            try
            {
                _isRenderingThread = true;
                
                var map = (Map) args[1];

                Image img = null;
                Rectangle frame;

                if (map.Layers.Count > 0 && Height > 0 && Width > 0)
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
                return new Tuple<Image, int, Rectangle>(new Bitmap(1, 1), currentGeneration, Rectangle.Empty);
            }
            finally
            {
                _isRenderingThread = false;
            }
        }

        private void HandleResizeTimerTick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            OnMapBoxRendered(sender, e);
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
