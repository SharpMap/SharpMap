#define EnableMetafileClipboardSupport
/*
 * Note:
 * 
 * If you want to use MapBox control along with MapImage controls
 * you have to define the compile time constant 'UseMapBox' in the
 * properties dialog of this project. As a result you will have the
 * MapImage control and the MapBox control included in your SharpMap.UI
 * assembly.
 * 
 * If you want to use MapBox control as a replacement of MapImage 
 * control you have to define the compile time constant 'UseMapBoxAsMapImage'.
 * in the  * properties dialog of this project. As a result you will have a
 * MapImage control in your SharpMap.UI assembly which is actually this
 * MapBox control.
 * 
 * If you don't define any of the two compile time constants this control
 * is omitted.
 * 
 * FObermaier
 */
#if UseMapBox || UseMapBoxAsMapImage
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpMap.Layers;
using System.Drawing.Imaging;
using System.Diagnostics;

using GeoPoint = SharpMap.Geometries.Point;
using Geometry = SharpMap.Geometries.Geometry;
using BoundingBox = SharpMap.Geometries.BoundingBox;

namespace SharpMap.Forms
{
    /// <summary>
    /// MapBox Class - MapBox control for Windows forms
    /// </summary>
    /// <remarks>
    /// The ExtendedMapImage control adds more than basic functionality to a Windows Form, such as dynamic pan, widow zoom and data query.
    /// </remarks>
    [DesignTimeVisible(true)]
#if UseMapBoxAsMapImage
    public class MapImage : Control
#else
    public class MapBox : Control
#endif
    {
        #region PreviewModes enumerator
        // ReSharper disable UnusedMember.Local
        /// <summary>
        /// Preview modes
        /// </summary>
        public enum PreviewModes
        {
            /// <summary>
            /// Best preview mode
            /// </summary>
            Best,
            /// <summary>
            /// Fast preview mode
            /// </summary>
            Fast
        }
        #endregion

        #region Position enumerators
        /// <summary>
        /// Horizontal alignment enumeration
        /// </summary>
        private enum XPosition
        {
            Center = 0,
            Right = 1,
            Left = -1
        }

        /// <summary>
        /// Vertical alignment enumeration
        /// </summary>
        private enum YPosition
        {
            Center = 0,
            Top = -1,
            Bottom = 1
        }
        // ReSharper restore UnusedMember.Local
        #endregion

        #region Tools enumerator
        /// <summary>
        /// Map tools enumeration
        /// </summary>
        public enum Tools
        {
            /// <summary>
            /// Pan
            /// </summary>
            Pan,
            /// <summary>
            /// Zoom in
            /// </summary>
            ZoomIn,
            /// <summary>
            /// Zoom out
            /// </summary>
            ZoomOut,
            /// <summary>
            /// Query tool
            /// </summary>
            Query,
            /// <summary>
            /// Zoom window tool
            /// </summary>
            ZoomWindow,
            /// <summary>
            /// Define Point on Map
            /// </summary>
            DrawPoint,
            /// <summary>
            /// Define Line on Map
            /// </summary>
            DrawLine,
            /// <summary>
            /// Define Polygon on Map
            /// </summary>
            DrawPolygon,
            /// <summary>
            /// No active tool
            /// </summary>
            None
        }
        #endregion

        #region Events
        /// <summary>
        /// MouseEventtype fired from the MapImage control
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="imagePos"></param>
        public delegate void MouseEventHandler(GeoPoint worldPos, MouseEventArgs imagePos);
        /// <summary>
        /// Fires when mouse moves over the map
        /// </summary>
        public new event MouseEventHandler MouseMove;
        /// <summary>
        /// Fires when map received a mouseclick
        /// </summary>
        public new event MouseEventHandler MouseDown;
        /// <summary>
        /// Fires when mouse is released
        /// </summary>		
        public new event MouseEventHandler MouseUp;
        /// <summary>
        /// Fired when mouse is dragging
        /// </summary>
        public event MouseEventHandler MouseDrag;

        /// <summary>
        /// Fired when the map has been refreshed
        /// </summary>
        public event EventHandler MapRefreshed;

        /// <summary>
        /// Eventtype fired when the zoom was or are being changed
        /// </summary>
        /// <param name="zoom"></param>
        public delegate void MapZoomHandler(double zoom);
        /// <summary>
        /// Fired when the zoom value has changed
        /// </summary>
        public event MapZoomHandler MapZoomChanged;
        /// <summary>
        /// Fired when the map is being zoomed
        /// </summary>
        public event MapZoomHandler MapZooming;

        /// <summary>
        /// Eventtype fired when the map is queried
        /// </summary>
        /// <param name="data"></param>
        public delegate void MapQueryHandler(Data.FeatureDataTable data);
        /// <summary>
        /// Fired when the map is queried
        /// </summary>
        public event MapQueryHandler MapQueried;


        /// <summary>
        /// Eventtype fired when the center has changed
        /// </summary>
        /// <param name="center"></param>
        public delegate void MapCenterChangedHandler(GeoPoint center);
        /// <summary>
        /// Fired when the center of the map has changed
        /// </summary>
        public event MapCenterChangedHandler MapCenterChanged;

        /// <summary>
        /// Eventtype fired when the map tool is changed
        /// </summary>
        /// <param name="tool"></param>
        public delegate void ActiveToolChangedHandler(Tools tool);
        /// <summary>
        /// Fired when the active map tool has changed
        /// </summary>
        public event ActiveToolChangedHandler ActiveToolChanged;

        /// <summary>
        /// Eventtype fired when a new geometry has been defined
        /// </summary>
        /// <param name="geometry">New Geometry</param>
        public delegate void GeometryDefinedHandler(Geometry geometry);

        /// <summary>
        /// Fired when a new polygon has been defined
        /// </summary>
        public event GeometryDefinedHandler GeometryDefined;


        #endregion

        private static int _defaultColorIndex;

        private static readonly Color[] DefaultColors =
            new []
                {
                    Color.DarkRed,
                    Color.DarkGreen, 
                    Color.DarkBlue, 
                    Color.Orange, 
                    Color.Cyan, 
                    Color.Black, 
                    Color.Purple, 
                    Color.Yellow,
                    Color.LightBlue, 
                    Color.Fuchsia
                };
        private const float MinDragScalingBeforeRegen = 0.3333f;
        private const float MaxDragScalingBeforeRegen = 3f;
        
        private readonly ProgressBar _progressBar;

#if DEBUG
        private readonly Stopwatch _watch = new Stopwatch();
#endif

        //private bool m_IsCtrlPressed;
        private double _wheelZoomMagnitude = 2;
        private Tools _activeTool;
        private double _fineZoomFactor = 10;
        private Map _map;
        private int _queryLayerIndex;
        private Point _dragStartPoint;
        private Point _dragEndPoint;
        private Bitmap _dragImage;
        private Rectangle _rectangle = Rectangle.Empty;
        private bool _dragging;
        
        private readonly SolidBrush _rectangleBrush = new SolidBrush(Color.FromArgb(210, 244, 244, 244));
        private readonly Pen _rectanglePen = new Pen(Color.FromArgb(244, 244, 244), 1);
        
        private float _scaling;
        private Image _image = new Bitmap(1, 1);
        private Image _imageBackground = new Bitmap(1, 1);
        private Image _imageStatic = new Bitmap(1, 1);
        private Image _imageVariable = new Bitmap(1, 1);
        private PreviewModes _previewMode;
        private bool _isRefreshing;
        private Point[] _pointArray;
        private bool _showProgress;

        public static void RandomizeLayerColors(VectorLayer layer)
        {
            layer.Style.EnableOutline = true;
            layer.Style.Fill = new SolidBrush(Color.FromArgb(80, DefaultColors[_defaultColorIndex % DefaultColors.Length]));
            layer.Style.Outline = new Pen(Color.FromArgb(100, DefaultColors[(_defaultColorIndex + ((int)(DefaultColors.Length * 0.5))) % DefaultColors.Length]), 1f);
            _defaultColorIndex++;
        }

        [Description("Define if the progress Bar is shown")]
        [Category("Appearance")]
        public bool ShowProgressUpdate
        {
            get
            {
                return _showProgress;
            }

            set
            {
                _showProgress = value;
                _progressBar.Visible = _showProgress;
            }
        }

        [Description("The color of selecting rectangle.")]
        [Category("Appearance")]
        public Color SelectionBackColor
        {
            get { return _rectangleBrush.Color; }
            set
            {
                //if (value != m_RectangleBrush.Color)
                    _rectangleBrush.Color = value;
            }
        }

        [Description("The map image currently visualized.")]
        [Category("Appearance")]
        public Image Image
        {
            get
            {
                //Updates Map
                lock (_map)
                {
                   //GetImagesAsync();
                   GetImagesAsyncEnd(null);
                }
                return _image;
            }
        }

        [Description("The color of selectiong rectangle frame.")]
        [Category("Appearance")]
        public Color SelectionForeColor
        {
            get { return _rectanglePen.Color; }
            set
            {
                //if (value != m_RectanglePen.Color)
                    _rectanglePen.Color = value;
            }
        }

        [Description("The amount which a single movement of the mouse wheel zooms by.")]
        [DefaultValue(2)]
        [Category("Behavior")]
        public double WheelZoomMagnitude
        {
            get { return _wheelZoomMagnitude; }
            set { _wheelZoomMagnitude = value; }
        }

        [Description("Mode used to create preview image while panning or zooming.")]
        [DefaultValue(PreviewModes.Best)]
        [Category("Behavior")]
        public PreviewModes PreviewMode
        {
            get { return _previewMode; }
            set
            {
                if (!_dragging)
                    _previewMode = value;
            }
        }

        [Description("The amount which the WheelZoomMagnitude is divided by " +
            "when the Control key is pressed. A number greater than 1 decreases " +
            "the zoom, and less than 1 increases it. A negative number reverses it.")]
        [DefaultValue(10)]
        [Category("Behavior")]
        public double FineZoomFactor
        {
            get { return _fineZoomFactor; }
            set { _fineZoomFactor = value; }
        }

        /// <summary>
        /// Map reference
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Map Map
        {
            get { return _map; }
            set
            {
                _map = value;

                if (_map != null)
                {
                    VariableLayerCollection.VariableLayerCollectionRequery += HandleVariableLayersRequery;
                    _map.MapNewTileAvaliable += HandleMapNewTileAvaliable;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the index of the active query layer 
        /// </summary>
        public int QueryLayerIndex
        {
            get { return _queryLayerIndex; }
            set { _queryLayerIndex = value; }
        }

        /// <summary>
        /// Sets the active map tool
        /// </summary>
        public Tools ActiveTool
        {
            get { return _activeTool; }
            set
            {
                var check = (value != _activeTool);
                _activeTool = value;

                SetCursor();

                _pointArray = null;

                if (check && ActiveToolChanged != null)
                    ActiveToolChanged(value);
            }
        }

#pragma warning disable 1587
#if DEBUG
        /// <summary>
        /// TimeSpan for refreshing maps
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TimeSpan LastRefreshTime { get; set; }
#endif

 #if UseMapBoxAsMapImage
        /// <summary>
        /// Initializes a new map
        /// </summary>
       public MapImage()
#else
        /// <summary>
        /// Initializes a new map
        /// </summary>
        public MapBox()
#endif
#pragma warning restore 1587
        {
            
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            base.DoubleBuffered = true;
            _map = new Map(ClientSize);
            VariableLayerCollection.VariableLayerCollectionRequery += HandleVariableLayersRequery;
            _map.MapNewTileAvaliable += HandleMapNewTileAvaliable;

            _activeTool = Tools.None;
            LostFocus += HandleMapBoxLostFocus;


            _progressBar = new ProgressBar
                               {
                                   Style = ProgressBarStyle.Marquee,
                                   Location = new Point(2, 2),
                                   Size = new Size(50, 10)
                               };
            Controls.Add(_progressBar);

        }

        protected override void Dispose(bool disposing)
        {
            VariableLayerCollection.VariableLayerCollectionRequery -= HandleVariableLayersRequery;
            _map.MapNewTileAvaliable -= HandleMapNewTileAvaliable;
            LostFocus -= HandleMapBoxLostFocus;

            base.Dispose(disposing);
        }

        #region event handling
        /// <summary>
        /// Handles LostFocus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMapBoxLostFocus(object sender, EventArgs e)
        {
            if (!_dragging) return;

            _dragging = false;
            Invalidate(ClientRectangle);
        }

        /// <summary>
        /// Handles need to requery of variable layers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleVariableLayersRequery(object sender, EventArgs e)
        {
            Image oldRef;
            lock (_map)
            {
                if (_dragging) return;
                oldRef = _imageVariable;
                _imageVariable = GetMap(_map.VariableLayers, LayerCollectionType.Variable);
            }

            UpdateImage(false);
            if (oldRef != null)
                oldRef.Dispose();

            Invalidate();
            Application.DoEvents();
        }

        void HandleMapNewTileAvaliable(TileLayer sender, BoundingBox box, Bitmap bm, int sourceWidth, int sourceHeight, ImageAttributes imageAttributes)
        {
            lock (_imageBackground)
            {
                try
                {
                    PointF min = _map.WorldToImage(new GeoPoint(box.Min.X, box.Min.Y));
                    PointF max = _map.WorldToImage(new GeoPoint(box.Max.X, box.Max.Y));

                    min = new PointF((float)Math.Round(min.X), (float)Math.Round(min.Y));
                    max = new PointF((float)Math.Round(max.X), (float)Math.Round(max.Y));

                    if (IsDisposed == false)
                    {
                        Graphics g = Graphics.FromImage(_imageBackground);

                        g.DrawImage(bm,
                            new Rectangle((int)min.X, (int)max.Y, (int)(max.X - min.X), (int)(min.Y - max.Y)),
                            0, 0,
                            sourceWidth, sourceHeight,
                            GraphicsUnit.Pixel,
                            imageAttributes);

                        g.Dispose();
                        UpdateImage(false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    //this can be a GDI+ Hell Exception...
                }
            }


        }

        #endregion

        private Image GetMap(LayerCollection layers, LayerCollectionType layerCollectionType)
        {

            if ((layers == null || layers.Count == 0 || Width == 0 || Height == 0))
            {
                if (layerCollectionType == LayerCollectionType.Background)
                    return new Bitmap(1, 1);
                else
                return null;
            }

            var retval = new Bitmap(Width, Height);

            Graphics g = Graphics.FromImage(retval);
            _map.RenderMap(g, layerCollectionType);
            g.Dispose();

            if (layerCollectionType == LayerCollectionType.Variable)
                retval.MakeTransparent(_map.BackColor);

            return retval;
        }


        private void GetImagesAsync()
        {
            lock (_map)
            {
                _imageVariable = GetMap(_map.VariableLayers, LayerCollectionType.Variable);
                _imageStatic = GetMap(_map.Layers, LayerCollectionType.Static);
                _imageBackground = GetMap(_map.BackgroundLayer, LayerCollectionType.Background);
            }
        }

        private void GetImagesAsyncEnd(IAsyncResult res)
        {
            if (InvokeRequired)
            {
                Invoke(new AsyncCallback(GetImagesAsyncEnd), res);
            }
            else
            {
                try
                {
                    //All this code is thread-safe because in inside of an UI-Invoke
                    var oldRef = _image;
                    var bmp = new Bitmap(Width, Height);

                    using (var g = Graphics.FromImage(bmp))
                    {
                        //Draws the background Image
                        if (_imageBackground != null)
                        {
                            try
                            {
                                g.DrawImageUnscaled(_imageBackground, 0, 0);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }

                        //Draws the static images
                        if (_imageStatic != null)
                        {
                            try
                            {
                                g.DrawImageUnscaled(_imageStatic, 0, 0);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }

                        }

                        //Draws the variable Images
                        if (_imageVariable != null)
                        {
                            try
                            {
                                g.DrawImageUnscaled(_imageVariable, 0, 0);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }

                        g.Dispose();
                    }

                    _image = bmp;
                    if (res != null)
                    {
                        ActiveTool = (Tools)res.AsyncState;
                    }

                    if (oldRef != null)
                        oldRef.Dispose();
                    Invalidate();
                    _dragEndPoint = new Point(0, 0);
                    _isRefreshing = false;
                    Enabled = true;
                    if (ShowProgressUpdate)
                    {
                        _progressBar.Enabled = false;
                        _progressBar.Visible = false;
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

#if DEBUG
                _watch.Stop();
                LastRefreshTime = _watch.Elapsed;
#endif

                if (MapRefreshed != null)
                    MapRefreshed(this, null);



            }

        }

        private void UpdateImage(bool forceRefresh)
        {
            if (((_imageStatic == null && _imageVariable == null && _imageBackground == null) && !forceRefresh) ||
                (Width == 0 || Height == 0)) return;
            
            if (forceRefresh && _isRefreshing == false)
            {
                _isRefreshing = true;
                Enabled = false;
                Tools oldTool = ActiveTool;
                ActiveTool = Tools.None;
                if (ShowProgressUpdate)
                {
                    _progressBar.Visible = true;
                    _progressBar.Enabled = true;
                }

                new MethodInvoker(GetImagesAsync).BeginInvoke(GetImagesAsyncEnd, oldTool);
            }
            else
            {
                GetImagesAsyncEnd(null);
            }
        }


        private void SetCursor()
        {
            if (_activeTool == Tools.None)
                Cursor = Cursors.Default;
            if (_activeTool == Tools.Pan)
                Cursor = Cursors.Hand;
            else if (_activeTool == Tools.Query)
                Cursor = Cursors.Help;
            else if (_activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut || _activeTool == Tools.ZoomWindow)
                Cursor = Cursors.Cross;
            else if (_activeTool == Tools.DrawPoint || _activeTool == Tools.DrawPolygon || _activeTool == Tools.DrawLine)
                Cursor = Cursors.Cross;
        }


        /// <summary>
        /// Refreshes the map
        /// </summary>
        public override void Refresh()
        {
            try
            {
#if DEBUG
                _watch.Reset();
                _watch.Start();
#endif
                if (_map != null)
                {
                    _map.Size = ClientSize;
                    if ((_map.Layers == null || _map.Layers.Count == 0) &&
                        (_map.BackgroundLayer == null || _map.BackgroundLayer.Count == 0) &&
                        (_map.VariableLayers == null || _map.VariableLayers.Count == 0))
                        _image = null;
                    else
                    {
                        Cursor c = Cursor;
                        Cursor = Cursors.WaitCursor;
                        UpdateImage(true);
                        Cursor = c;
                    }

                    base.Refresh();
                    Invalidate();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /*
        protected override void OnKeyDown(KeyEventArgs e)
        {
            m_IsCtrlPressed = e.Control;
            System.Diagnostics.Debug.WriteLine(String.Format("Ctrl: {0}", m_IsCtrlPressed));

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            m_IsCtrlPressed = e.Control;
            System.Diagnostics.Debug.WriteLine(String.Format("Ctrl: {0}", m_IsCtrlPressed));

            base.OnKeyUp(e);
        }
         */

        private static Boolean IsControlPressed { get { return (ModifierKeys & Keys.ControlKey) == Keys.ControlKey; } }


        protected override void OnMouseHover(EventArgs e)
        {
            if (!Focused)
            {
                bool isFocused = Focus();
                Debug.WriteLine("Focused: " + isFocused);
            }

            base.OnMouseHover(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (_map != null)
            {
                double scale = (e.Delta / 120.0);
                double scaleBase = 1 + (_wheelZoomMagnitude / (10 * (IsControlPressed ? _fineZoomFactor : 1)));

                _map.Zoom *= Math.Pow(scaleBase, scale);

                if (MapZoomChanged != null)
                    MapZoomChanged(_map.Zoom);

                Refresh();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_map != null)
            {
                if (e.Button == MouseButtons.Left) //dragging
                {
                    _dragStartPoint = e.Location;
                    _dragEndPoint = e.Location;
                }

                if (MouseDown != null)
                    MouseDown(_map.ImageToWorld(new Point(e.X, e.Y)), e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_map != null)
            {
                GeoPoint p = _map.ImageToWorld(new Point(e.X, e.Y));

                if (MouseMove != null)
                    MouseMove(p, e);

                if (_image != null && e.Location != _dragStartPoint && !_dragging && e.Button == MouseButtons.Left &&
                    !(_activeTool == Tools.DrawLine || _activeTool == Tools.DrawPoint || _activeTool == Tools.DrawPolygon))
                {
                    _dragging = true;

                    if (_activeTool == Tools.Pan || _activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut)
                        _dragImage = GenerateDragImage(_previewMode);
                    else
                        _dragImage = GenerateDragImage(PreviewModes.Fast);
                }

                if (_dragging)
                {
                    if (MouseDrag != null)
                        MouseDrag(p, e);

                    if (_activeTool == Tools.Pan)
                    {
                        _dragEndPoint = ClipPoint(e.Location);
                        Invalidate(ClientRectangle);
                    }
                    else if (_activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut)
                    {
                        _dragEndPoint = ClipPoint(e.Location);

                        if (_dragEndPoint.Y - _dragStartPoint.Y < 0) //Zoom out
                            _scaling = (float)Math.Pow(1 / (float)(_dragStartPoint.Y - _dragEndPoint.Y), 0.5);
                        else //Zoom in
                            _scaling = 1 + (_dragEndPoint.Y - _dragStartPoint.Y) * 0.1f;

                        if (MapZooming != null)
                            MapZooming(_map.Zoom / _scaling);

                        if (_previewMode == PreviewModes.Best && (_scaling < MinDragScalingBeforeRegen || _scaling > MaxDragScalingBeforeRegen))
                            RegenerateZoomingImage();

                        Invalidate(ClientRectangle);
                    }
                    else if (_activeTool == Tools.ZoomWindow || _activeTool == Tools.Query)
                    {
                        _dragEndPoint = ClipPoint(e.Location);
                        _rectangle = GenerateRectangle(_dragStartPoint, _dragEndPoint);
                        Invalidate(new Region(ClientRectangle));
                    }
                }
                else
                {
                    if (_activeTool == Tools.DrawPolygon || _activeTool == Tools.DrawLine)
                    {
                        _dragEndPoint = new Point(0, 0);
                        if (_pointArray != null)
                        {
                            _pointArray[_pointArray.GetUpperBound(0)] = ClipPoint(e.Location);
                            //Rectangle oldRectangle = _rectangle;
                            _rectangle = GenerateRectangle(_dragStartPoint, ClipPoint(e.Location));
                            Invalidate(new Region(ClientRectangle));
                        }
                    }
                }
            }
        }


         
        // _map.GetMapAsMetaFile does not work as expected,
        // therefore it is commented out.
        // 

#if EnableMetafileClipboardSupport

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                Clipboard.Clear();
                ClipboardMetafileHelper.PutEnhMetafileOnClipboard(Handle, _map.GetMapAsMetafile());
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
        
#endif

        private void RegenerateZoomingImage()
        {
            Cursor c = Cursor;
            Cursor = Cursors.WaitCursor;
            _map.Zoom /= _scaling;
            lock (_map)
            {
                _image = _map.GetMap();
            }
            _scaling = 1;
            _dragImage = GenerateDragImage(PreviewModes.Best);
            _dragStartPoint = _dragEndPoint;
            Cursor = c;
        }

        private Bitmap GenerateDragImage(PreviewModes mode)
        {
            if (mode == PreviewModes.Best)
            {
                Cursor c = Cursor;
                Cursor = Cursors.WaitCursor;

                GeoPoint realCenter = _map.Center;
                Bitmap bmp = new Bitmap(_map.Size.Width * 3, _map.Size.Height * 3);
                Graphics g = Graphics.FromImage(bmp);

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            var clone = _image.Clone() as Image;
                            if (clone != null)
                                g.DrawImageUnscaled(clone, _map.Size.Width, _map.Size.Height);
                        }
                        else
                            g.DrawImageUnscaled(GeneratePartialBitmap(realCenter, (XPosition)i, (YPosition)j), (i + 1) * _map.Size.Width, (j + 1) * _map.Size.Height);
                    }
                }
                g.Dispose();
                _map.Center = realCenter;

                Cursor = c;

                return bmp;
            }
            if (_image.PixelFormat != PixelFormat.Undefined)
                return _image.Clone() as Bitmap;
            return null;
        }

        private Bitmap GeneratePartialBitmap(GeoPoint center, XPosition xPos, YPosition yPos)
        {
            double x = center.X, y = center.Y;

            switch (xPos)
            {
                case XPosition.Right:
                    x += _map.Envelope.Width;
                    break;
                case XPosition.Left:
                    x -= _map.Envelope.Width;
                    break;
            }

            switch (yPos)
            {
                case YPosition.Top:
                    y += _map.Envelope.Height;
                    break;
                case YPosition.Bottom:
                    y -= _map.Envelope.Height;
                    break;
            }

            _map.Center = new GeoPoint(x, y);
            return _map.GetMap() as Bitmap;
        }

        private Point ClipPoint(Point p)
        {
            int x = p.X < 0 ? 0 : (p.X > ClientSize.Width ? ClientSize.Width : p.X);
            int y = p.Y < 0 ? 0 : (p.Y > ClientSize.Height ? ClientSize.Height : p.Y);
            return new Point(x, y);
        }

        private static Rectangle GenerateRectangle(Point p1, Point p2)
        {
            int x = Math.Min(p1.X, p2.X);
            int y = Math.Min(p1.Y, p2.Y);
            int width = Math.Abs(p2.X - p1.X);
            int height = Math.Abs(p2.Y - p1.Y);

            return new Rectangle(x, y, width, height);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (_dragging)
            {
                if (_activeTool == Tools.ZoomWindow || _activeTool == Tools.Query)
                {
                    //Reset image to normal view
                    Bitmap patch = _dragImage.Clone(pe.ClipRectangle, PixelFormat.DontCare);
                    pe.Graphics.DrawImageUnscaled(patch, pe.ClipRectangle);
                    patch.Dispose();

                    //Draw selection rectangle
                    if (_rectangle.Width > 0 && _rectangle.Height > 0)
                    {
                        pe.Graphics.FillRectangle(_rectangleBrush, _rectangle);
                        Rectangle border = new Rectangle(_rectangle.X + (int)_rectanglePen.Width / 2, _rectangle.Y + (int)_rectanglePen.Width / 2, _rectangle.Width - (int)_rectanglePen.Width, _rectangle.Height - (int)_rectanglePen.Width);
                        pe.Graphics.DrawRectangle(_rectanglePen, border);
                    }
                    return;
                }
                if (_activeTool == Tools.Pan)
                {
                    pe.Graphics.DrawImageUnscaled(_dragImage,
                                                  _previewMode == PreviewModes.Best
                                                      ? new Point(
                                                            -_map.Size.Width + _dragEndPoint.X - _dragStartPoint.X,
                                                            -_map.Size.Height + _dragEndPoint.Y - _dragStartPoint.Y)
                                                      : new Point(_dragEndPoint.X - _dragStartPoint.X,
                                                                  _dragEndPoint.Y - _dragStartPoint.Y));
                    return;
                }
                if (_activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut)
                {
                    RectangleF rect = new RectangleF(0, 0, _map.Size.Width, _map.Size.Height);

                    if (_map.Zoom / _scaling < _map.MinimumZoom)
                        _scaling = (float)Math.Round(_map.Zoom / _map.MinimumZoom, 4);

                    //System.Diagnostics.Debug.WriteLine("Scaling: " + m_Scaling);

                    if (_previewMode == PreviewModes.Best)
                        _scaling *= 3;

                    rect.Width *= _scaling;
                    rect.Height *= _scaling;

                    rect.Offset(_map.Size.Width / 2f - rect.Width / 2, _map.Size.Height / 2f - rect.Height / 2);

                    pe.Graphics.DrawImage(_dragImage, rect);
                    return;
                }

            }

            if (_image != null && _image.PixelFormat != PixelFormat.Undefined)
            {
                if (/*_dragEndPoint != null &&*/ _dragEndPoint.X != 0 && _dragEndPoint.Y != 0 && _dragEndPoint != _dragStartPoint)
                {
                    pe.Graphics.DrawImageUnscaled(_dragImage,
                                                  _previewMode == PreviewModes.Best
                                                      ? new Point(
                                                            -_map.Size.Width + _dragEndPoint.X - _dragStartPoint.X,
                                                            -_map.Size.Height + _dragEndPoint.Y - _dragStartPoint.Y)
                                                      : new Point(_dragEndPoint.X - _dragStartPoint.X,
                                                                  _dragEndPoint.Y - _dragStartPoint.Y));
                }
                else
                {
                    pe.Graphics.DrawImageUnscaled(_image, 0, 0);

                    //Draws current line or polygon (Draw Line or Draw Polygon tool)
                    if (_pointArray != null)
                    {
                        if (_pointArray.GetUpperBound(0) == 1)
                        {
                            pe.Graphics.DrawLine(new Pen(Color.Gray, 2F), _pointArray[0], _pointArray[1]);
                        }
                        else
                        {
                            if (_activeTool == Tools.DrawPolygon)
                            {
                                Color c = Color.FromArgb(127, Color.Gray);
                                pe.Graphics.FillPolygon(new SolidBrush(c), _pointArray);
                                pe.Graphics.DrawPolygon(new Pen(Color.Gray, 2F), _pointArray);
                            }
                            else
                                pe.Graphics.DrawLines(new Pen(Color.Gray, 2F), _pointArray);
                        }
                    }
                }
            }
            else
                base.OnPaint(pe);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_map != null)
            {
                if (MouseUp != null)
                    MouseUp(_map.ImageToWorld(new Point(e.X, e.Y)), e);

                if (e.Button == MouseButtons.Left)
                {
                    if (_activeTool == Tools.ZoomOut)
                    {
                        double scale = 0.5;
                        if (_dragging)
                        {
                            if (e.Y - _dragStartPoint.Y < 0) //Zoom out
                                scale = (float)Math.Pow(1 / (float)(_dragStartPoint.Y - e.Y), 0.5);
                            else //Zoom in
                                scale = 1 + (e.Y - _dragStartPoint.Y) * 0.1;
                        }
                        else
                        {
                            _map.Center = _map.ImageToWorld(new Point(e.X, e.Y));

                            if (MapCenterChanged != null)
                                MapCenterChanged(_map.Center);
                        }

                        _map.Zoom /= scale;

                        if (MapZoomChanged != null)
                            MapZoomChanged(_map.Zoom);
                    }
                    else if (_activeTool == Tools.ZoomIn)
                    {
                        double scale = 2;
                        if (_dragging)
                        {
                            if (e.Y - _dragStartPoint.Y < 0) //Zoom out
                                scale = (float)Math.Pow(1 / (float)(_dragStartPoint.Y - e.Y), 0.5);
                            else //Zoom in
                                scale = 1 + (e.Y - _dragStartPoint.Y) * 0.1;
                        }
                        else
                        {
                            _map.Center = _map.ImageToWorld(new Point(e.X, e.Y));

                            if (MapCenterChanged != null)
                                MapCenterChanged(_map.Center);
                        }

                        _map.Zoom *= 1 / scale;

                        if (MapZoomChanged != null)
                            MapZoomChanged(_map.Zoom);

                    }
                    else if (_activeTool == Tools.Pan)
                    {
                        if (_dragging)
                        {
                            Point point = new Point(ClientSize.Width / 2 + (_dragStartPoint.X - e.Location.X), ClientSize.Height / 2 + (_dragStartPoint.Y - e.Location.Y));
                            _map.Center = _map.ImageToWorld(point);

                            if (MapCenterChanged != null)
                                MapCenterChanged(_map.Center);
                        }
                        else
                        {
                            _map.Center = _map.ImageToWorld(new Point(e.X, e.Y));

                            if (MapCenterChanged != null)
                                MapCenterChanged(_map.Center);
                        }
                    }
                    else if (_activeTool == Tools.Query)
                    {
                        if (_map.Layers.Count > _queryLayerIndex && _queryLayerIndex > -1)
                        {
                            /*
                            if (m_Map.Layers[m_QueryLayerIndex].GetType() == typeof(Layers.VectorLayer))
                            {
                                
                                Layers.VectorLayer layer = m_Map.Layers[m_QueryLayerIndex] as Layers.VectorLayer;
                                Geometries.BoundingBox bounding;
                                
                                if (m_Dragging)
                                {
                                    GeoPoint lowerLeft;
                                    GeoPoint upperRight;
                                    GetBounds(m_Map.ImageToWorld(m_DragStartPoint), m_Map.ImageToWorld(m_DragEndPoint), out lowerLeft, out upperRight);

                                    bounding = new Geometries.BoundingBox(lowerLeft, upperRight);
                                }
                                else
                                    bounding = m_Map.ImageToWorld(new Point(e.X, e.Y)).GetBoundingBox().Grow(m_Map.PixelSize * 5);
                                
                                Data.FeatureDataSet ds = new Data.FeatureDataSet();
                                layer.DataSource.Open();
                                layer.DataSource.ExecuteIntersectionQuery(bounding, ds);
                                layer.DataSource.Close();

                                if (MapQueried != null)
                                    MapQueried((ds.Tables.Count > 0 ? ds.Tables[0] : new Data.FeatureDataTable()));
                            }
                             */
                            var layer = _map.Layers[_queryLayerIndex] as ICanQueryLayer;
                            if (layer != null)
                            {
                                BoundingBox bounding;

                                if (_dragging)
                                {
                                    GeoPoint lowerLeft;
                                    GeoPoint upperRight;
                                    GetBounds(_map.ImageToWorld(_dragStartPoint), _map.ImageToWorld(_dragEndPoint), out lowerLeft, out upperRight);

                                    bounding = new BoundingBox(lowerLeft, upperRight);
                                }
                                else
                                    bounding = _map.ImageToWorld(new Point(e.X, e.Y)).GetBoundingBox().Grow(_map.PixelSize * 5);

                                Data.FeatureDataSet ds = new Data.FeatureDataSet();
                                layer.ExecuteIntersectionQuery(bounding, ds);
                                if (ds.Tables.Count > 0)
                                    if (MapQueried != null) MapQueried(ds.Tables[0]);
                                    else if (MapQueried != null) MapQueried(new Data.FeatureDataTable());
                            }

                        }
                        else
                            MessageBox.Show("No active layer to query");
                    }
                    else if (_activeTool == Tools.ZoomWindow)
                    {
                        if (_rectangle.Width > 0 && _rectangle.Height > 0)
                        {
                            GeoPoint lowerLeft;
                            GeoPoint upperRight;
                            GetBounds(_map.ImageToWorld(_dragStartPoint), _map.ImageToWorld(_dragEndPoint), out lowerLeft, out upperRight);
                            _dragEndPoint.X = 0;
                            _dragEndPoint.Y = 0;

                            _map.ZoomToBox(new BoundingBox(lowerLeft, upperRight));
                            
                            if (MapZoomChanged != null)
                                MapZoomChanged(_map.Zoom);

                        }
                    }
                    else if (_activeTool == Tools.DrawPoint)
                    {
                        if (GeometryDefined != null)
                        {
                            GeometryDefined(Map.ImageToWorld(new PointF(e.X, e.Y)));
                        }
                    }
                    else if (_activeTool == Tools.DrawPolygon || _activeTool == Tools.DrawLine)
                    {
                        //pointArray = null;
                        if (_pointArray == null)
                        {
                            _pointArray = new Point[2];
                            _pointArray[0] = e.Location;
                            _pointArray[1] = e.Location;
                        }
                        else
                        {
                            Point[] temp = new Point[_pointArray.GetUpperBound(0) + 2];
                            for (int i = 0; i <= _pointArray.GetUpperBound(0); i++)
                                temp[i] = _pointArray[i];

                            temp[temp.GetUpperBound(0)] = e.Location;
                            _pointArray = temp;
                        }
                    }
                }


                if (_dragging)
                {

                    _dragging = false;

                    if (_activeTool == Tools.Query)
                        Invalidate(_rectangle);

                    if (_activeTool == Tools.ZoomWindow || _activeTool == Tools.Query)
                        _rectangle = Rectangle.Empty;

                    Refresh();

                    if (_activeTool != Tools.ZoomOut)
                    {
                        if (_image != null)
                        {
                            _image.Dispose();
                            _image = null;
                        }
                        _image = _dragImage;
                        Invalidate();
                    }


                }
                else if (_activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut || _activeTool == Tools.Pan)
                {
                    Refresh();
                }


            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (_activeTool == Tools.DrawPolygon)
            {
                if (GeometryDefined != null)
                {
                    Geometries.LinearRing extRing = new Geometries.LinearRing();
                    for (int i = 0; i < _pointArray.GetUpperBound(0); i++)
                        extRing.Vertices.Add(Map.ImageToWorld(new PointF(_pointArray[i].X, _pointArray[i].Y)));

                    extRing.Vertices.Add(Map.ImageToWorld(new PointF(_pointArray[0].X, _pointArray[0].Y)));

                    GeometryDefined(new Geometries.Polygon(extRing));
                }
                ActiveTool = Tools.None;
            }
            else if (_activeTool == Tools.DrawLine)
            {
                if (GeometryDefined != null)
                {
                    Geometries.LineString line = new Geometries.LineString();
                    for (int i = 0; i <= _pointArray.GetUpperBound(0); i++)
                        line.Vertices.Add(Map.ImageToWorld(new PointF(_pointArray[i].X, _pointArray[i].Y)));

                    GeometryDefined(line);
                }
                ActiveTool = Tools.None;
            }
        }


        private static void GetBounds(GeoPoint p1, GeoPoint p2,
            out GeoPoint lowerLeft, out GeoPoint upperRight)
        {
            lowerLeft = new GeoPoint(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            upperRight = new GeoPoint(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));

            Debug.WriteLine("p1: " + p1);
            Debug.WriteLine("p2: " + p2);
            Debug.WriteLine("lowerLeft: " + lowerLeft);
            Debug.WriteLine("upperRight: " + upperRight);
        }
    }

#if EnableMetafileClipboardSupport
    public class ClipboardMetafileHelper
    {
        [DllImport("user32.dll")]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll")]
        static extern bool CloseClipboard();

        [DllImport("gdi32.dll")]
        static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, IntPtr hNULL);

        [DllImport("gdi32.dll")]
        static extern bool DeleteEnhMetaFile(IntPtr hemf);

        /*
        [DllImport("gdi32")]
        static extern int GetEnhMetaFileBits(int hemf, int cbBuffer, byte[] lpbBuffer);
        */
        // 
        /// <summary>
        /// Puts the metafile to the clipboard
        /// </summary>
        /// <param name="hWnd">The handle</param>
        /// <param name="mf">The metafie</param>
        /// <remarks>Metafile mf is set to a state that is not valid inside this function.</remarks>
        /// <returns><see langword="true"/> if operation was successfull</returns>
        static public bool PutEnhMetafileOnClipboard(IntPtr hWnd, Metafile mf)
        {
            bool bResult = false;
            IntPtr hEMF2;
            IntPtr hEMF = mf.GetHenhmetafile();

            if (!hEMF.Equals(new IntPtr(0)))
            {
                hEMF2 = CopyEnhMetaFile(hEMF, new IntPtr(0));
                if (!hEMF2.Equals(new IntPtr(0)))
                {
                    if (OpenClipboard(hWnd))
                    {
                        if (EmptyClipboard())
                        {
                            IntPtr hRes = SetClipboardData(14 /*CF_ENHMETAFILE*/, hEMF2);
                            bResult = hRes.Equals(hEMF2);
                            CloseClipboard();
                        }
                    }
                }
                DeleteEnhMetaFile(hEMF);
            }
            return bResult;
        }
    }
#endif
}
#endif