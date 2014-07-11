using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeoAPI.Geometries;

namespace SharpMap.Forms
{
    /// <summary>
    /// This control displays a minimap of the whole extension of a map, and let the user drag the viewport.
    /// </summary>
    public partial class MiniMapControl : UserControl
    {
        #region fields
        private MapBox _mapBoxControl;
        private volatile int _generation;
        private volatile Map _currentMap;
        private Rectangle _frame;
        private bool _mouseDown;
        #endregion

        #region ctor
        public MiniMapControl()
        {
            InitializeComponent();
        } 
        #endregion

        #region properties
        
        /// <summary>
        /// The <see cref="MapBox"/> control linked to the mini map.
        /// </summary>
        [DefaultValue(null)]
        public MapBox MapBoxControl
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
        #endregion

        #region protected members

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _mouseDown = true;
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

            if (MapBoxControl != null && _currentMap != null && _mouseDown)
            {
                var x = e.X;
                var y = e.Y;

                var newCenter = _currentMap.ImageToWorld(new PointF(x, y));

                MapBoxControl.Map.Center = newCenter;
                MapBoxControl.Refresh();

                _frame = CalculateNewFrame(e);
                Invalidate();

                Cursor.Clip = Rectangle.Empty;
            }

            _mouseDown = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            DrawFrame(e.Graphics, _frame);
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
            Debug.WriteLine("SizeChanged");
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

            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var newPen = new Pen(Color.Black, 1f))
            {
                newPen.DashStyle = DashStyle.Dash;
                g.DrawRectangle(newPen, rect);
            }
            using (var newPen = new Pen(Color.White, 1f))
            {
                newPen.DashStyle = DashStyle.Dash;
                rect.Inflate(1, 1);
                g.DrawRectangle(newPen, rect);
            }
        }

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
            catch (Exception e)
            {
                return null;
            }

        }

        private void _resizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            OnMapBoxRendered(sender, e);
        }

        #endregion
    }
}
