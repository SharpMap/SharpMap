// Copyright 2008-, SharpMapTeam
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

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
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GeoAPI.Geometries;
using SharpMap.Forms.Tools;
using SharpMap.Layers;
using System.Drawing.Imaging;
using IGeometry = GeoAPI.Geometries.IGeometry;
using System.Threading;
using Common.Logging;
using System.Collections.Generic;
using SharpMap.Forms.ImageGenerator;

namespace SharpMap.Forms
{
    /// <summary>
    /// MapBox Class - MapBox control for Windows forms
    /// </summary>
    /// <remarks>
    /// The ExtendedMapImage control adds more than basic functionality to a Windows Form, such as dynamic pan, widow zoom and data query.
    /// </remarks>
    [DesignTimeVisible(true)]
// ReSharper disable once PartialTypeWithSinglePart
    public partial class MapBox : Control
    {
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
            _logger.Debug(t => t("Rectangle {0} invalidate", e.InvalidRect));
        }


        private static readonly ILog _logger = LogManager.GetLogger(typeof (MapBox));
        private const double PrecisionTolerance = 0.00000001;

        static MapBox() { Map.Configure(); }

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
            /// Query bounding boxes for intersection
            /// </summary>
            QueryBox,

            /// <summary>
            /// Query tool
            /// </summary>
            [Obsolete ("Use QueryBox")]
            Query = QueryBox,

            /// <summary>
            /// Attempt true intersection query on geometry
            /// </summary>
            QueryPoint,

            /// <summary>
            /// Attempt true intersection query on geometry
            /// </summary>
            [Obsolete ("Use QueryPoint")]
            QueryGeometry = QueryPoint,

            ///// <summary>
            ///// Attempt true intersection query on polygonal geometry
            ///// </summary>
            //QueryPolygon,

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
            None,

            /// <summary>
            /// Custom tool, implementing <see cref="IMapTool"/>
            /// </summary>
            Custom
        }

        /// <summary>
        /// Enumeration of map query types
        /// </summary>
        public enum MapQueryType
        {
            /// <summary>
            /// Layer set in QueryLayerIndex is the only layers Queried  (Default)
            /// </summary>
            LayerByIndex,

            /// <summary>
            /// All layers are queried
            /// </summary>
            AllLayers,

            /// <summary>
            /// All visible layers are queried
            /// </summary>
            VisibleLayers,

            /// <summary>
            /// Visible layers are queried from Top and down until a layer with an intersecting feature is found 
            /// </summary>
            TopMostLayer
        };


        #endregion

        #region Events

        /// <summary>
        /// MouseEventtype fired from the MapImage control
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="imagePos"></param>
        public delegate void MouseEventHandler(Coordinate worldPos, MouseEventArgs imagePos);

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
        /// Fired when the map is about to change
        /// </summary>
        public event CancelEventHandler MapChanging;

        /// <summary>
        /// Fired when the map has been changed
        /// </summary>
        public event EventHandler MapChanged;

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
        /// 
        /// Will be fired one time for each layer selected for query depending on QueryLayerIndex and QuerySettings
        /// </summary>
        public event MapQueryHandler MapQueried;

        /// <summary>
        /// Fired when Map is Queried before the first MapQueried event is fired for that query
        /// </summary>
        public event EventHandler MapQueryStarted;

        /// <summary>
        /// Fired when Map is Queried after the last MapQueried event is fired for that query
        /// </summary>
        public event EventHandler MapQueryDone;


        /// <summary>
        /// Eventtype fired when the center has changed
        /// </summary>
        /// <param name="center"></param>
        public delegate void MapCenterChangedHandler(Coordinate center);

        /// <summary>
        /// Fired when the center of the map has changed
        /// </summary>
        public event MapCenterChangedHandler MapCenterChanged;

        /// <summary>
        /// Eventtype fired befor the active map tool change
        /// </summary>
        /// <param name="toolPre">pre-tool</param>
        /// <param name="toolNew">new tool</param>
        /// <param name="cea">a cancel indicator</param>
        public delegate void ActiveToolChangingHandler(Tools toolPre, Tools toolNew, CancelEventArgs cea);

        /// <summary>
        /// Fired befor the active map tool change
        /// </summary>
        public event ActiveToolChangingHandler ActiveToolChanging;

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
        public delegate void GeometryDefinedHandler(IGeometry geometry);

        /// <summary>
        /// Fired when a new polygon has been defined
        /// </summary>
        public event GeometryDefinedHandler GeometryDefined;

        #endregion

        private readonly IMapBoxImageGenerator _miRenderer;

        private static int m_defaultColorIndex;

        private static readonly Color[] _defaultColors ={
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

        /*
        private const float MinDragScalingBeforeRegen = 0.3333f;
        private const float MaxDragScalingBeforeRegen = 3f;
         */
        private readonly ProgressBar _progressBar;

#if DEBUG
        private readonly Stopwatch _watch = new Stopwatch();
#endif

        //private bool m_IsCtrlPressed;
        private IMapTool _currentTool;
        private double _wheelZoomMagnitude = -2;
        private Tools _activeTool;
        private double _fineZoomFactor = 10;
        private Map _map;
        private int _queryLayerIndex;
        private Point _dragStartPoint;
        private Point _dragEndPoint;
        //private Bitmap _dragImage;
        private Rectangle _rectangle = Rectangle.Empty;
        private bool _dragging;
        private readonly SolidBrush _rectangleBrush = new SolidBrush(Color.FromArgb(210, 244, 244, 244));
        private readonly Pen _rectanglePen = new Pen(Color.FromArgb(244, 244, 244), 1);

        private float _scaling;
        //private Image _image = new Bitmap(1, 1);
        //private Image _imageBackground = new Bitmap(1, 1);
        //private Image _imageStatic = new Bitmap(1, 1);
        //private Image _imageVariable = new Bitmap(1, 1);
        //private Envelope _imageEnvelope = new Envelope(0, 1, 0, 1);
        //private int _imageGeneration;

        private readonly object _mapLocker = new object();

        private int _needToRefreshAfterWheel;
        private PreviewModes _previewMode;
        //private bool _isRefreshing;
        private List<Coordinate> _pointArray = new List<Coordinate>();
        private bool _showProgress;
        private bool _zoomToPointer = true;
        private bool _setActiveToolNoneDuringRedraw;
        private bool _shiftButtonDragRectangleZoom = true;
        private bool _focusOnHover;
        private bool _panOnClick = true;
        private float _queryGrowFactor = 5f;
        private MapQueryType _mapQueryMode = MapQueryType.LayerByIndex;

        private readonly IMessageFilter _mousePreviewFilter;

        /// <summary>
        /// Assigns a random color to a vector layers style
        /// </summary>
        /// <param name="layer"></param>
        public static void RandomizeLayerColors(VectorLayer layer)
        {
            layer.Style.EnableOutline = true;
            layer.Style.Fill = new SolidBrush(Color.FromArgb(80, _defaultColors[m_defaultColorIndex%_defaultColors.Length]));
            layer.Style.Outline =
                new Pen(
                    Color.FromArgb(100,
                                   _defaultColors[
                                       (m_defaultColorIndex + ((int) (_defaultColors.Length*0.5)))%_defaultColors.Length]),
                    1f);
            m_defaultColorIndex++;
        }

        /// <summary>
        /// Gets or sets a value on whether to report progress of map generation
        /// </summary>
        [Description("Define if the progress Bar is shown")]
        [Category("Appearance")]
        public bool ShowProgressUpdate
        {
            get { return _showProgress; }

            set
            {
                _showProgress = value;
                _progressBar.Visible = _showProgress;
            }
        }

        /// <summary>
        /// Gets or sets whether the "go-to-cursor-on-click" feature is enabled or not (even if enabled it works only if the active tool is Pan)
        /// </summary>
        [Description(
            "Sets whether the \"go-to-cursor-on-click\" feature is enabled or not (even if enabled it works only if the active tool is Pan)"
            )]
        [DefaultValue(true)]
        [Category("Behavior")]
        public bool PanOnClick
        {
            get { return _panOnClick; }
            set
            {
                ActiveTool = Tools.Pan;
                _panOnClick = value;

            }
        }

        /// <summary>
        /// Sets whether the mapcontrol should automatically grab focus when mouse is hovering the control
        /// </summary>
        [Description("Sets whether the mapcontrol should automatically grab focus when mouse is hovering the control")]
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool TakeFocusOnHover
        {
            get { return _focusOnHover; }
            set { _focusOnHover = value; }
        }

        /// <summary>
        /// Sets whether the mouse wheel should zoom to the pointer location
        /// </summary>
        [Description("Sets whether the mouse wheel should zoom to the pointer location")]
        [DefaultValue(true)]
        [Category("Behavior")]
        public bool ZoomToPointer
        {
            get { return _zoomToPointer; }
            set { _zoomToPointer = value; }
        }

        /// <summary>
        /// Sets ActiveTool to None (and changing cursor) while redrawing the map
        /// </summary>
        [Description("Sets ActiveTool to None (and changing cursor) while redrawing the map")]
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool SetToolsNoneWhileRedrawing
        {
            get { return _setActiveToolNoneDuringRedraw; }
            set { _setActiveToolNoneDuringRedraw = value; }
        }

        /// <summary>
        /// Gets or sets the number of pixels by which a bounding box around the query point should be "grown" prior to perform the query
        /// </summary>
        /// <remarks>Does not apply when querying against boxes.</remarks>
        [Description(
            "Gets or sets the number of pixels by which a bounding box around the query point should be \"grown\" prior to perform the query"
            )]
        [DefaultValue(5)]
        [Category("Behavior")]
        public float QueryGrowFactor
        {
            get { return _queryGrowFactor; }
            set
            {
                if (value < 0) value = 0;
                //if (value > 10)
                _queryGrowFactor = value;
            }
        }


        /// <summary>
        /// Gets or sets the value of the back color for the selection rectangle
        /// </summary>
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

        /// <summary>
        /// Gets or sets the value of the border color for the selection rectangle
        /// </summary>
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

        /// <summary>
        /// Gets the current map image
        /// </summary>
        [Description("The map image currently visualized.")]
        [Category("Appearance")]
        public Image Image
        {
            get
            {
                return _miRenderer.Image;
                //GetImagesAsyncEnd(null);
                //return _image;
            }
        }

        /// <summary>
        /// Gets or sets the amount which a single movement of the mouse wheel zooms by.
        /// </summary>
        [Description(
            "The amount which a single movement of the mouse wheel zooms by. (Negative values are similar as OpenLayers/Google, positive are like ArcMap"
            )]
        [DefaultValue(-2)]
        [Category("Behavior")]
        public double WheelZoomMagnitude
        {
            get { return _wheelZoomMagnitude; }
            set { _wheelZoomMagnitude = value; }
        }

        /// <summary>
        /// Gets or sets the mode used to create preview image while panning or zooming.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating the amount which the WheelZoomMagnitude is divided by 
        /// when the Control key is pressed. A number greater than 1 decreases 
        /// the zoom, and less than 1 increases it. A negative number reverses it.
        /// </summary>
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
        /// Gets or sets a value that enables shortcut to rectangle-zoom by holding down shift-button and drag rectangle
        /// </summary>
        [Description("Enables shortcut to rectangle-zoom by holding down shift-button and drag rectangle")]
        [DefaultValue(true)]
        [Category("Behavior")]
        public bool EnableShiftButtonDragRectangleZoom
        {
            get { return _shiftButtonDragRectangleZoom; }
            set { _shiftButtonDragRectangleZoom = value; }
        }

        /// <summary>
        /// Gets or sets the map reference
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Map Map
        {
            get { return _map; }
            set
            {
                if (value != _map)
                {
                    var cea = new CancelEventArgs(false);
                    OnMapChanging(cea);
                    if (cea.Cancel) return;

                    _map = value;

                    OnMapChanged(EventArgs.Empty);
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
        /// Gets or sets the mapquerying mode
        /// </summary>
        public MapQueryType MapQueryMode
        {
            get { return _mapQueryMode; }
            set { _mapQueryMode = value; }
        }

        /// <summary>
        /// Sets the active map tool
        /// </summary>
        public Tools ActiveTool
        {
            get { return _activeTool; }
            set
            {
                var cea = new CancelEventArgs(false);
                OnActiveToolChanging(ActiveTool, value, cea);
                if (cea.Cancel)
                {
                    if (CustomTool != null)
                        CustomTool.Enabled = true;
                    return;
                }

                _activeTool = value;

                SetCursor();

                _pointArray = null;

                OnActiveToolChanged(value);
            }
        }

        /// <summary>
        /// Event invoker for the <see cref="ActiveToolChanging"/>
        /// </summary>
        /// <param name="toolPre">pre-tool</param>
        /// <param name="toolNew">new tool</param>
        /// <param name="cea">a cancel indicator</param>
        protected virtual void OnActiveToolChanging(Tools toolPre, Tools toolNew, CancelEventArgs cea)
        {
            if (CustomTool != null)
                CustomTool.Enabled = false;
            var handler = ActiveToolChanging;
            if (handler != null)
                handler(toolPre, toolNew, cea);
        }

        /// <summary>
        /// Event invoker for the <see cref="ActiveToolChanged"/> event
        /// </summary>
        /// <param name="activeTool">The tool</param>
        protected virtual void OnActiveToolChanged(Tools activeTool)
        {
            if (CustomTool != null)
                CustomTool.Enabled = true;
            var handler = ActiveToolChanged;
            if (handler != null)
                handler(activeTool);
        }

        /// <summary>
        /// Gets or sets a value indicating the currently active custom tool
        /// </summary>
        public IMapTool CustomTool
        {
            get { return _currentTool; }
            set
            {
                if (value == _currentTool)
                    return;

                var raiseActiveToolChanged = ActiveTool == Tools.Custom && value != null;

                _currentTool = value;
                ActiveTool = _currentTool != null
                    ? Tools.Custom
                    : Tools.None;

                if (_currentTool != null)
                    _currentTool.Map = _map;

                if (raiseActiveToolChanged)
                    OnActiveToolChanged(ActiveTool);
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

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public MapBox()
#pragma warning restore 1587
        {

            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            base.DoubleBuffered = true;

            _map = new Map(ClientSize);
            //_map.VariableLayers.VariableLayerCollectionRequery += HandleVariableLayersRequery;
            //_map.RefreshNeeded += HandleRefreshNeeded;
            //_map.MapNewTileAvaliable += HandleMapNewTileAvaliable;

            _progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Location = new Point(2, 2),
                Size = new Size(50, 10)
            };
            Controls.Add(_progressBar);

            _miRenderer = new LegacyMapBoxImageGenerator(this, _progressBar);

            _activeTool = Tools.None;
            LostFocus += HandleMapBoxLostFocus;
            _progressBar.Visible = ShowProgressUpdate;

            _mousePreviewFilter = new MouseWheelGrabber(this);
            Application.AddMessageFilter(_mousePreviewFilter);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (Map != null)
            {
                if (Size != Map.Size)
                {
                    Map.Size = Size;
                    Refresh();
                }
            }
            base.OnSizeChanged(e);
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        /// <param name="disposing">A parameter indicating that this method is called from either a call to <see cref="Control.Dispose()"/> (<c>true</c>)
        /// or the finalizer (<c>false</c>)</param>
        protected override void Dispose(bool disposing)
        {
            if (_miRenderer.IsDisposed || IsDisposed)
                return;

            LostFocus -= HandleMapBoxLostFocus;
            if (_mousePreviewFilter != null)
                Application.RemoveMessageFilter(_mousePreviewFilter);

            if (_map != null)
            {
                // special handling to prevent spurious VariableLayers events
                _map.VariableLayers.Interval = 0;
                //_map.VariableLayers.VariableLayerCollectionRequery -= HandleVariableLayersRequery;
                //_map.MapNewTileAvaliable -= HandleMapNewTileAvaliable;
                //_map.RefreshNeeded -= HandleRefreshNeeded;
            }


            lock (_mapLocker)
            {
                _miRenderer.Dispose();
                _map = null;

                //if (_imageStatic != null)
                //{
                //    _imageStatic.Dispose();
                //    _imageStatic = null;
                //}
                //if (_imageBackground != null)
                //{
                //    _imageBackground.Dispose();
                //    _imageBackground = null;
                //}
                //if (_imageVariable != null)
                //{
                //    _imageVariable.Dispose();
                //    _imageVariable = null;
                //}
                //if (_image != null)
                //{
                //    _image.Dispose();
                //    _image = null;
                //}

                //if (_dragImage != null)
                //{
                //    _dragImage.Dispose();
                //    _dragImage = null;
                //}

                if (_rectanglePen != null)
                {
                    _rectanglePen.Dispose();
                }
                if (_rectangleBrush != null)
                {
                    _rectangleBrush.Dispose();
                }

                base.Dispose(disposing);
            }
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

        ///// <summary>
        ///// Handles need to requery of variable layers
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void HandleVariableLayersRequery(object sender, EventArgs e)
        //{
        //    if (IsDisposed || _isDisposed)
        //        return;

        //    Image oldRef;
        //    lock (_mapLocker)
        //    {
        //        if (_dragging) return;
        //        oldRef = _imageVariable;
        //        _imageVariable = GetMap(_map, _map.VariableLayers, LayerCollectionType.Variable, _map.Envelope);
        //    }

        //    UpdateImage(false);
        //    if (oldRef != null)
        //        oldRef.Dispose();

        //    Invalidate();
        //    Application.DoEvents();
        //}

//        private void HandleMapNewTileAvaliable(ITileAsyncLayer sender, Envelope box, Bitmap bm, int sourceWidth,
//                                               int sourceHeight, ImageAttributes imageAttributes)
//        {
//            lock (_backgroundImagesLocker)
//            {
//                try
//                {
//                    var min = Point.Round(_map.WorldToImage(box.Min()));
//                    var max = Point.Round(_map.WorldToImage(box.Max()));

//                    if (IsDisposed == false && _isDisposed == false)
//                    {

//                        using (var g = Graphics.FromImage(_imageBackground))
//                        {

//                            g.DrawImage(bm,
//                                        new Rectangle(min.X, max.Y, (max.X - min.X), (min.Y - max.Y)),
//                                        0, 0,
//                                        sourceWidth, sourceHeight,
//                                        GraphicsUnit.Pixel,
//                                        imageAttributes);

//                        }

//                        UpdateImage(false);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.Warn(ex.Message, ex);
//                    //this can be a GDI+ Hell Exception...
//                }

//            }

//        }

          #endregion

//        private Image GetMap(Map map, LayerCollection layers, LayerCollectionType layerCollectionType,
//                             Envelope extent)
//        {
//            try
//            {
//                var width = Width;
//                var height = Height;

//                if ((layers == null || layers.Count == 0 || width <= 0 || height <= 0))
//                {
//                    if (layerCollectionType == LayerCollectionType.Background)
//                        return new Bitmap(1, 1);
//                    return null;
//                }

//                var retval = new Bitmap(width, height);

//                using (var g = Graphics.FromImage(retval))
//                {
//                    g.Clear(Color.Transparent);
//                    map.RenderMap(g, layerCollectionType, false, true);
//                }

//                /*if (layerCollectionType == LayerCollectionType.Variable)
//                    retval.MakeTransparent(_map.BackColor);
//                else if (layerCollectionType == LayerCollectionType.Static && map.BackgroundLayer.Count > 0)
//                    retval.MakeTransparent(_map.BackColor);*/
//                return retval;
//            }
//            catch (Exception ee)
//            {
//                _logger.Error("Error while rendering map", ee);

//                if (layerCollectionType == LayerCollectionType.Background)
//                    return new Bitmap(1, 1);
//                return null;
//            }
//        }


//        private readonly object _staticImagesLocker = new object();
//        private readonly object _backgroundImagesLocker = new object();
//        private readonly object _paintImageLocker = new object();
//        private readonly object _mapLocker = new object();

//        private void GetImagesAsync(Envelope extent, int imageGeneration)
//        {
//            lock (_mapLocker)
//            {
//                if (_isDisposed)
//                    return;

//                if (imageGeneration < _imageGeneration)
//                {
//                    /*we're to old*/
//                    return;
//                }
//                var safeMap = _map.Clone();
//                _imageVariable = GetMap(safeMap, _map.VariableLayers, LayerCollectionType.Variable, extent);
//                lock (_staticImagesLocker)
//                {
//                    _imageStatic = GetMap(safeMap, _map.Layers, LayerCollectionType.Static, extent);
//                }
//                lock (_backgroundImagesLocker)
//                {
//                    _imageBackground = GetMap(safeMap, _map.BackgroundLayer, LayerCollectionType.Background, extent);
//                }
//            }
//        }

//        private class GetImageEndResult
//        {
//            public Tools? Tool { get; set; }
//            public Envelope bbox { get; set; }
//            public int generation { get; set; }
//        }

//        private void GetImagesAsyncEnd(GetImageEndResult res)
//        {
//            // draw only if generation is larger than the current, else we have aldready drawn something newer
//            // we must to check also IsHandleCreated because during disposal, the handle of the parent is detroyed sooner than progress bar's handle,
//            // this leads to cross thread operation and exception because InvokeRequired returns false, but for the progress bar it is true.
//            if (res == null || res.generation < _imageGeneration || _isDisposed || !IsHandleCreated)
//                return;


//            if (_logger.IsDebugEnabled)
//                _logger.DebugFormat("{2}: {0} - {1}", res.generation, res.bbox, DateTime.Now);


//            if ((_setActiveToolNoneDuringRedraw || ShowProgressUpdate) && InvokeRequired)
//            {
//                try
//                {
//                    BeginInvoke(new MethodInvoker(() => GetImagesAsyncEnd(res)));

//                }
//                catch (Exception ex)
//                {
//                    _logger.Warn(ex.Message, ex);
//                }
//            }
//            else
//            {
//                try
//                {
//                    var oldRef = _image;
//                    if (Width > 0 && Height > 0)
//                    {

//                        var bmp = new Bitmap(Width, Height);


//                        using (var g = Graphics.FromImage(bmp))
//                        {
//                            g.Clear(_map.BackColor);
//                            lock (_backgroundImagesLocker)
//                            {
//                                //Draws the background Image
//                                if (_imageBackground != null)
//                                {
//                                    try
//                                    {
//                                        g.DrawImageUnscaled(_imageBackground, 0, 0);
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        _logger.Warn(ex.Message, ex);
//                                    }
//                                }
//                            }

//                            //Draws the static images
//                            if (_staticImagesLocker != null)
//                            {
//                                try
//                                {
//                                    if (_imageStatic != null)
//                                    {
//                                        g.DrawImageUnscaled(_imageStatic, 0, 0);
//                                    }
//                                }
//                                catch (Exception ex)
//                                {
//                                    _logger.Warn(ex.Message, ex);
//                                }

//                            }

//                            //Draws the variable Images
//                            if (_imageVariable != null)
//                            {
//                                try
//                                {
//                                    g.DrawImageUnscaled(_imageVariable, 0, 0);
//                                }
//                                catch (Exception ex)
//                                {
//                                    _logger.Warn(ex.Message, ex);
//                                }
//                            }

//                            g.Dispose();
//                        }


//                        lock (_paintImageLocker)
//                        {
//                            _image = bmp;
//                            _imageEnvelope = res.bbox;
//                        }
//                    }

//                    if (res.Tool.HasValue)
//                    {
//                        if (_setActiveToolNoneDuringRedraw)
//                            ActiveTool = res.Tool.Value;

//                        _dragEndPoint = new Point(0, 0);
//                        _isRefreshing = false;

//                        if (_setActiveToolNoneDuringRedraw)
//                            Enabled = true;

//                        if (ShowProgressUpdate)
//                        {
//                            _progressBar.Enabled = false;
//                            _progressBar.Visible = false;
//                        }
//                    }

//                    lock (_paintImageLocker)
//                    {
//                        if (oldRef != null)
//                            oldRef.Dispose();
//                    }

//                    Invalidate();
//                }
//                catch (Exception ex)
//                {
//                    _logger.Warn(ex.Message, ex);
//                }

//#if DEBUG
//                _watch.Stop();
//                LastRefreshTime = _watch.Elapsed;
//#endif

//                try
//                {
//                    if (MapRefreshed != null)
//                    {
//                        MapRefreshed(this, null);
//                    }
//                }
//                catch (Exception ee)
//                {
//                    //Trap errors that occured when calling the eventhandlers
//                    _logger.Warn("Exception while calling eventhandler", ee);
//                }
//            }
//        }

        //private void UpdateImage(bool forceRefresh)
        //{
        //    if (_isDisposed || IsDisposed)
        //        return;

        //    if (((_imageStatic == null && _imageVariable == null && _imageBackground == null) && !forceRefresh) ||
        //        (Width == 0 || Height == 0)) return;

        //    Envelope bbox = _map.Envelope;
        //    if (forceRefresh) // && _isRefreshing == false)
        //    {
        //        _isRefreshing = true;
        //        Tools oldTool = ActiveTool;
        //        if (_setActiveToolNoneDuringRedraw)
        //        {
        //            ActiveTool = Tools.None;
        //            Enabled = false;
        //        }

        //        if (ShowProgressUpdate)
        //        {
        //            if (InvokeRequired)
        //            {
        //                _progressBar.BeginInvoke(new Action<ProgressBar>(p =>
        //                {
        //                    p.Visible = true;
        //                    p.Enabled = true;
        //                }), _progressBar);
        //            }
        //            else
        //            {
        //                _progressBar.Visible = true;
        //                _progressBar.Enabled = true;
        //            }
        //        }

        //        int generation = ++_imageGeneration;
        //        ThreadPool.QueueUserWorkItem(
        //            delegate
        //                {
        //                    GetImagesAsync(bbox, generation);
        //                    GetImagesAsyncEnd(new GetImageEndResult
        //                        {Tool = oldTool, bbox = bbox, generation = generation});
        //                });
        //    }
        //    else
        //    {
        //        GetImagesAsyncEnd(new GetImageEndResult {Tool = null, bbox = bbox, generation = _imageGeneration});
        //    }
        //}


        private void SetCursor()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(SetCursor));
                return;
            }

            switch (_activeTool)
            {
                case Tools.None:
                    Cursor = Cursors.Default;
                    break;
                case Tools.Pan:
                    Cursor = Cursors.Hand;
                    break;
                case Tools.QueryBox:
                case Tools.QueryPoint:
                    Cursor = Cursors.Help;
                    break;
                case Tools.ZoomIn:
                case Tools.ZoomOut:
                    Cursor = Cursors.Cross;
                    break;
                case Tools.DrawPoint:
                case Tools.DrawLine:
                case Tools.DrawPolygon:
                    Cursor = Cursors.Cross;
                    break;
                case Tools.Custom:
                    Cursor = _currentTool.Cursor;
                    break;
            }
        }


        /// <summary>
        /// Refreshes the map
        /// </summary>
        public override void Refresh()
        {
            try
            {
                //Protect against cross-thread operations...
                //We need this since we're modifying the cursor
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(Refresh));
                    return;
                }
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
                        ; //_image = null;
                    else
                    {
                        Cursor c = Cursor;
                        if (_setActiveToolNoneDuringRedraw)
                        {
                            Cursor = Cursors.WaitCursor;
                        }

                        _miRenderer.Generate();
                        if (_setActiveToolNoneDuringRedraw)
                        {
                            Cursor = c;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex.Message, ex);
            }
        }


        private static Boolean IsControlPressed
        {
            get { return (ModifierKeys & Keys.ControlKey) == Keys.ControlKey; }
        }


        private Point _lastHoverPostiton;

        /// <summary>
        /// Invokes the <see cref="E:System.Windows.Forms.Control.MouseHover"/>-event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event arguments.</param>
        protected override void OnMouseHover(EventArgs e)
        {
            _logger.DebugFormat("Hover at {0}", MousePosition);

            // update the last hover position
            _lastHoverPostiton = PointToClient(MousePosition);

            // If required test and grab focus
            if (_focusOnHover)
                TestAndGrabFocus();

            // Invoke the base implementation
            base.OnMouseHover(e);

            // Do we have a custom tool, execute it
            if (UseCurrentTool)
            {
                var p = _map.ImageToWorld((_lastHoverPostiton));
                if (_currentTool.DoMouseHover(p))
                    return;
            }
        }

        private void TestAndGrabFocus()
        {
            if (!Focused)
            {
                var isFocused = Focus();
                _logger.Debug("Focused: " + isFocused);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (UseCurrentTool)
            {
                if (_currentTool.DoMouseEnter())
                    return;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
        protected override void OnMouseLeave(EventArgs e)
        {
            _logger.DebugFormat( "MouseLeave {0}", Name);

            base.OnMouseLeave(e);
            if (UseCurrentTool)
            {
                if (_currentTool.DoMouseLeave())
                    return;
            }
        }


        private System.Timers.Timer _mouseWheelRefreshTimer;

        /// <summary>
        /// Invokes the <see cref="E:System.Windows.Forms.Control.MouseWheel"/>-event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event arguments.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            _logger.DebugFormat("MouseWheel {0} ({1})", e.Location, e.Delta);
            base.OnMouseWheel(e);

            if (_map != null)
            {
                // Do we have a custom tool
                if (UseCurrentTool)
                {
                    if (_currentTool.DoMouseWheel(_map.ImageToWorld(e.Location), e))
                        return;
                }

                var oldCenter = _map.Center;
                var oldZoom = _map.Zoom;
                var oldHeight = _map.MapHeight;

                var scale = (e.Delta / 120.0);
                var scaleBase = 1 + (_wheelZoomMagnitude/(10*(IsControlPressed ? _fineZoomFactor : 1)));

                if (!ZoomToPointer)
                    // zoom in/out maintaining existing centre
                    _map.Zoom = oldZoom * Math.Pow(scaleBase, scale);
                else
                {
                    // preserve MAP cursor posn
                    var p = _map.ImageToWorld(new Point(e.X, e.Y), true);

                    System.Drawing.Drawing2D.Matrix transform = new System.Drawing.Drawing2D.Matrix();
                    transform.Translate((float)-p.X, (float)-p.Y, System.Drawing.Drawing2D.MatrixOrder.Append);
                    transform.Scale((float)Math.Pow(scaleBase, scale), (float)Math.Pow(scaleBase, scale), System.Drawing.Drawing2D.MatrixOrder.Append);
                    transform.Translate((float)p.X, (float)p.Y, System.Drawing.Drawing2D.MatrixOrder.Append);

                    // NB zoom is independent of MapTransform. 
                    var pts = new[] { new PointF((float)oldCenter.X, (float)oldCenter.Y),
                                      new PointF((float)(oldCenter.X - 0.5 * oldZoom), (float)(oldCenter.Y - 0.5 * oldHeight)),
                                      new PointF((float)(oldCenter.X + 0.5 * oldZoom), (float)(oldCenter.Y + 0.5 * oldHeight))
                                    };
                    transform.TransformPoints(pts);
                    transform.Dispose();

                    // Subject to creep when map is rotated, but significant improvement over previous implementation
                    var newCenter = new Coordinate(pts[0].X, pts[0].Y);
                    var newZoom = pts[2].X - pts[1].X;
                    var box = new Envelope(pts[1].X, pts[2].X, pts[1].Y, pts[2].Y);

                    //pre-checks to prevent mapViewPortGuard from adjusting centre upon reaching max extents
                    if (newZoom < _map.MinimumZoom || newZoom > _map.MaximumZoom)
                        return;

                    if (_map.EnforceMaximumExtents && !_map.MaximumExtents.IsNull && !_map.MaximumExtents.Contains(box))
                        return;

                    //_map.Center = newCenter;
                    //_map.Zoom = newZoom;

                    // more efficient that Center + Zoom 
                    _map.ZoomToBox(box);

                }

                if (!_map.Center.Equals2D(oldCenter, PrecisionTolerance))
                {
                    Interlocked.Exchange(ref _needToRefreshAfterWheel, 1);
                    OnMapCenterChanged(_map.Center);
                }

                if (Math.Abs(_map.Zoom - oldZoom) > PrecisionTolerance)
                {
                    Interlocked.Exchange(ref _needToRefreshAfterWheel, 1);
                    OnMapZoomChanged(_map.Zoom);
                }

                Invalidate();

                if (_mouseWheelRefreshTimer == null)
                {
                    _mouseWheelRefreshTimer = new System.Timers.Timer(50);
                    _mouseWheelRefreshTimer.Elapsed += TimerUpdate;
                    _mouseWheelRefreshTimer.Enabled = true;
                    _mouseWheelRefreshTimer.AutoReset = false;
                    _mouseWheelRefreshTimer.Start();
                }
                else
                {
                    _mouseWheelRefreshTimer.Stop();
                    _mouseWheelRefreshTimer.Start();
                }
            }
        }

        private void TimerUpdate(object state, System.Timers.ElapsedEventArgs args)
        {
            _logger.Debug("TimerRefresh");
            if (Interlocked.CompareExchange(ref _needToRefreshAfterWheel, 0, 1)==1)
            {
                OnMapZoomChanged(_map.Zoom);

                Refresh();
            }
        }

        private Coordinate _dragStartCenter;
        private System.Drawing.Drawing2D.Matrix _dragTransform;
        private double _orgScale;

        /// <summary>
        /// Invokes the <see cref="E:System.Windows.Forms.Control.MouseDown"/>-event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event arguments.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _logger.DebugFormat("MouseDown {0} ({1}, {2})", Name, e.Location, e.Button);

            // call base function
            base.OnMouseDown(e);

            // Do we have a map? If not bail out
            if (_map == null)
                return;

            // Position in world coordinates
            var p = _map.ImageToWorld(new Point(e.X, e.Y), true);

            // Raise event
            if (MouseDown != null)
                MouseDown(p, e);

            // Do we have a custom tool
            if (UseCurrentTool)
            {
                if (_currentTool.DoMouseDown(p, e))
                    return;
            }


            // Do we have a predefined tool
            if (_activeTool == Tools.None)
                return;

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle) //dragging
            {
                _dragStartPoint = e.Location;
                _dragEndPoint = e.Location;
                _dragStartCenter = _map.Center;

                _dragTransform = new System.Drawing.Drawing2D.Matrix();
                _dragTransform.Translate(-e.X, -e.Y);
                _dragTransform.Scale(1f, -1f, System.Drawing.Drawing2D.MatrixOrder.Append);  // reflect in X axis
                if (_map.MapTransformRotation == 0)
                    _dragTransform.Scale((float)_map.PixelWidth, (float)_map.PixelHeight, System.Drawing.Drawing2D.MatrixOrder.Append);
                else
                {
                    _dragTransform.Rotate(_map.MapTransformRotation, System.Drawing.Drawing2D.MatrixOrder.Append);
                    // must derive scale for rotated maps, as "Zoom" is based upon non-rotated width
                    var env = _map.Envelope;
                    _dragTransform.Scale((float)(env.Width / Width), (float)(env.Width / Width / _map.PixelAspectRatio), System.Drawing.Drawing2D.MatrixOrder.Append);
                }
                _dragTransform.Translate((float)p.X, (float)p.Y, System.Drawing.Drawing2D.MatrixOrder.Append);

                _orgScale = _map.Zoom;
            }
        }

        //private bool ActionCheck(object sender, EventArgs e, Action<object, EventArgs> baseFunction, out Coordinate point,
        //    Action<Coordinate, EventArgs> mapDelegate)
        //{
        //    // call base function
        //    if (baseFunction != null)
        //        baseFunction(sender, e);

        //    // Do we have a map?
        //    point = null;
        //    if (_map == null) 
        //        return false;

        //    // Do we have a map delegate
        //    if (e is MouseEventArgs && mapDelegate != null)
        //    {
        //        point = _map.ImageToWorld(((MouseEventArgs) e).Location);
        //        mapDelegate(point, e);
        //    }

        //    // Do we have an active predefined tool
        //    return _activeTool != Tools.None;
        //}

        /// <summary>
        /// Private method to check if we need to reenable the <see cref="Control.MouseHover"/> event.
        /// </summary>
        /// <param name="position">The current position of the cursor</param>
        private void CheckEnableHover(Point position)
        {
            var delta = new Size(position.X - _lastHoverPostiton.X,
                                 position.Y - _lastHoverPostiton.Y);

            if (Math.Abs(delta.Width) > SystemInformation.MouseHoverSize.Width ||
                Math.Abs(delta.Height) > SystemInformation.MouseHoverSize.Height)
            {
                ResetMouseEventArgs();
            }

        }

        /// <summary>
        /// Invokes the <see cref="E:System.Windows.Forms.Control.MouseMove"/>-event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event arguments.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // call base function
            base.OnMouseMove(e);

            // Do we have a map? If not bail out
            if (_map == null)
            {
                CheckEnableHover(e.Location);
                return;
            }

            // Position in world coordinates
            var p = _map.ImageToWorld(new Point(e.X, e.Y), true);

            // Raise event
            if (MouseMove != null)
                MouseMove(p, e);

            // Do we have a custom tool
            if (UseCurrentTool)
            {
                if (_currentTool.DoMouseMove(p, e))
                {
                    CheckEnableHover(e.Location);
                    return;
                }
            }


            // If no tool is selected, bail out
            if (ActiveTool == Tools.None)
            {
                CheckEnableHover(e.Location);
                return;
            }

            bool isStartDrag = //_image != null && e.Location != _dragStartPoint && !_dragging &&
                               _miRenderer.ImageValue != null && e.Location != _dragStartPoint && !_dragging && 
                               (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle) &&
                               //Left of middle button can start drag
                               !(_setActiveToolNoneDuringRedraw &&
                                 (_activeTool == Tools.DrawLine || _activeTool == Tools.DrawPoint ||
                                  _activeTool == Tools.DrawPolygon)); //It should not be any of these tools

            if (isStartDrag)
            {
                _dragging = true;
            }

            if (_dragging)
            {
                if (MouseDrag != null)
                    MouseDrag(p, e);

                //Pan can be if we have ActiveTool Pan and not doing a ShiftButtonZoom-Operation
                bool isPanOperation = _activeTool == Tools.Pan &&
                                      !(_shiftButtonDragRectangleZoom &&
                                        (ModifierKeys & Keys.Shift) != Keys.None);

                //Pan can also be if we are in a drawline/drawpoint/drawpoly operation and pressed left mousebutton while dragging..
                //If we not are setting tool non while redrawing..
                if ((_activeTool == Tools.DrawLine || _activeTool == Tools.DrawPolygon)
                    && e.Button == MouseButtons.Left && !_setActiveToolNoneDuringRedraw)
                {
                    isPanOperation = true;
                }


                //Zoom in or Zoom Out
                bool isZoomOperation = _activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut;

                //Tool ZoomWindow or ShiftButtonDragRectangle
                bool isZoomWindowOperation = _activeTool == Tools.ZoomWindow || _activeTool == Tools.QueryBox ||
                                             //_activeTool == Tools.QueryPoint ||
                                             /*_activeTool == Tools.QueryPolygon || */
                                             (_shiftButtonDragRectangleZoom &&
                                              (Control.ModifierKeys & Keys.Shift) != Keys.None);

                if (isPanOperation)
                {
                    _dragEndPoint = ClipPoint(e.Location);
                    if (_dragStartCenter != null)
                    {
                        var oldCenter = _map.Center;
                        
                        var pts = new[] { new System.Drawing.PointF(_dragStartPoint.X, _dragStartPoint.Y),
                                          new System.Drawing.PointF(_dragEndPoint.X, _dragEndPoint.Y) //,
                                         //new System.Drawing.PointF(Width/2f, Height/2f)
                                        };

                        _dragTransform.TransformPoints(pts);

                        var dX = (pts[1].X - pts[0].X);
                        var dY = (pts[1].Y - pts[0].Y);

                        _map.Center = new Coordinate(_dragStartCenter.X - dX, _dragStartCenter.Y - dY);

                        if (!_map.Center.Equals2D(oldCenter, PrecisionTolerance))
                        {
                            OnMapCenterChanged(_map.Center);

                            Invalidate(ClientRectangle);
                        }
                    }
                }
                else if (isZoomOperation)
                {
                    _dragEndPoint = ClipPoint(e.Location);
                    if (_dragEndPoint.Y - _dragStartPoint.Y < 0) //Zoom out
                        _scaling = (float) Math.Pow(1/(float) (_dragStartPoint.Y - _dragEndPoint.Y), 0.5);
                    else //Zoom in
                        _scaling = 1 + (_dragEndPoint.Y - _dragStartPoint.Y)*0.1f;

                    _map.Zoom = _orgScale/_scaling;
                    if (MapZooming != null)
                        MapZooming(_map.Zoom);

                    Invalidate(ClientRectangle);
                }
                else if (isZoomWindowOperation)
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
                        _pointArray[_pointArray.Count - 1] = Map.ImageToWorld(ClipPoint(e.Location));
                        _rectangle = GenerateRectangle(_dragStartPoint, ClipPoint(e.Location));
                        Invalidate(new Region(ClientRectangle));
                    }
                }
            }

            CheckEnableHover(e.Location);
        }

        /// <summary>
        /// Invokes the <see cref="E:System.Windows.Forms.Control.KeyDown"/>-event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event arguments.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
#if EnableMetafileClipboardSupport
            if (e.Control && e.KeyCode == Keys.C)
            {
                Clipboard.Clear();
                ClipboardMetafileHelper.PutEnhMetafileOnClipboard(Handle, _map.GetMapAsMetafile());
                e.Handled = true;
            }
#endif

            if (UseCurrentTool)
            {
                _currentTool.DoKeyDown(_map.ImageToWorld(MousePosition), e);
            }
            base.OnKeyDown(e);
        }

        private bool UseCurrentTool { get { return _currentTool != null && _currentTool.Enabled; }}

        internal bool Dragging
        {
            get => _dragging;
        }

        public object MapLocker => _mapLocker;

        /// <summary>
        /// Invokes the <see cref="E:SharpMap.Forms.MapBox.MapChanging"/>-event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event arguments.</param>
        protected virtual void OnMapChanging(CancelEventArgs e)
        {
            if (MapChanging != null) MapChanging(this, e);
            if (e.Cancel)
                return;

            //if (_map != null)
            //{
            //    _map.VariableLayers.VariableLayerCollectionRequery -= HandleVariableLayersRequery;
            //    _map.MapNewTileAvaliable -= HandleMapNewTileAvaliable;
            //    _map.RefreshNeeded -= HandleRefreshNeeded;
            //}
        }

        /// <summary>
        /// Invokes the <see cref="E:SharpMap.Forms.MapBox.MapChanged"/>-event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event arguments.</param>
        protected virtual void OnMapChanged(EventArgs e)
        {
            if (_map != null)
            {
                //_map.VariableLayers.VariableLayerCollectionRequery += HandleVariableLayersRequery;
                //_map.MapNewTileAvaliable += HandleMapNewTileAvaliable;
                //_map.RefreshNeeded += HandleRefreshNeeded;
                Refresh();
            }

            // Assign the map to the custom tool, too.
            if (_currentTool != null) _currentTool.Map = _map;

            var handler = MapChanged;
            if (handler != null) handler(this, e);
        }

        internal protected virtual void OnMapRefreshed(EventArgs e)
        {
            MapRefreshed?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="E:SharpMap.Forms.MapBox.MapZoomChanged"/> event.
        /// </summary>
        /// <param name="zoom"></param>
        protected virtual void OnMapZoomChanged(double zoom)
        {
            var handler = MapZoomChanged;
            if (handler != null)
                handler(zoom);
        }

        /// <summary>
        /// Invokes the <see cref="E:SharpMap.Forms.MapBox.MapCenterChanged"/> event.
        /// </summary>
        /// <param name="center"></param>
        protected virtual void OnMapCenterChanged(Coordinate center)
        {
            var handler = MapCenterChanged;
            if (handler != null)
                handler(center);
        }

        //void HandleRefreshNeeded(object sender, EventArgs e)
        //{
        //    UpdateImage(true);
        //}

        /*
        private void RegenerateZoomingImage()
        {
            var c = Cursor;
            Cursor = Cursors.WaitCursor;
            _map.Zoom /= _scaling;
            lock (_mapLocker)
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

                Coordinate realCenter = _map.Center;
                Bitmap bmp = new Bitmap(_map.Size.Width*3, _map.Size.Height*3);
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
                            g.DrawImageUnscaled(GeneratePartialBitmap(realCenter, (XPosition) i, (YPosition) j),
                                                (i + 1)*_map.Size.Width, (j + 1)*_map.Size.Height);
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

        private Bitmap GeneratePartialBitmap(Coordinate center, XPosition xPos, YPosition yPos)
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

            _map.Center = new Coordinate(x, y);
            return _map.GetMap() as Bitmap;
        }
                 */

        private Point ClipPoint(Point p)
        {
            var x = p.X < 0 ? 0 : (p.X > ClientSize.Width ? ClientSize.Width : p.X);
            var y = p.Y < 0 ? 0 : (p.Y > ClientSize.Height ? ClientSize.Height : p.Y);
            return new Point(x, y);
        }

        private static Rectangle GenerateRectangle(Point p1, Point p2)
        {
            var x = Math.Min(p1.X, p2.X);
            var y = Math.Min(p1.Y, p2.Y);
            var width = Math.Abs(p2.X - p1.X);
            var height = Math.Abs(p2.Y - p1.Y);

            return new Rectangle(x, y, width, height);
        }


        /// <summary>
        /// Invokes the <see cref="E:System.Windows.Forms.Control.Paint"/>-event.
        /// </summary>
        /// <param name="pe">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event arguments.</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug($"OnPaint ({pe.ClipRectangle}), ActiveTool: {Enum.GetName(typeof(MapBox.Tools), _activeTool)}");

                //Console.WriteLine($"OnPaint ({pe.ClipRectangle.ToString()})");

                if (_dragging)
                {
                    if (_activeTool == Tools.ZoomWindow || _activeTool == Tools.QueryBox ||
                        (_shiftButtonDragRectangleZoom && (ModifierKeys & Keys.Shift) != Keys.None))
                    {
                        var image = (Bitmap) _miRenderer.ImageValue;

                        //Reset image to normal view
                        using (var patch = image.Clone(pe.ClipRectangle, PixelFormat.DontCare))
                            pe.Graphics.DrawImageUnscaled(patch, pe.ClipRectangle);

                        //Draw selection rectangle
                        if (_rectangle.Width > 0 && _rectangle.Height > 0)
                        {
                            pe.Graphics.FillRectangle(_rectangleBrush, _rectangle);
                            var border = new Rectangle(_rectangle.X + (int) _rectanglePen.Width/2,
                                                             _rectangle.Y + (int) _rectanglePen.Width/2,
                                                             _rectangle.Width - (int) _rectanglePen.Width,
                                                             _rectangle.Height - (int) _rectanglePen.Width);
                            pe.Graphics.DrawRectangle(_rectanglePen, border);
                        }
                    }
                    else if (_activeTool == Tools.Pan || _activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut ||
                             _activeTool == Tools.DrawLine || _activeTool == Tools.DrawPolygon)
                    {
                        var image = _miRenderer.ImageValue;
                        var imageEnvelope = _miRenderer.ImageEnvelope;
                        if (_map.Envelope.Equals(imageEnvelope))
                            pe.Graphics.DrawImageUnscaled(image, 0, 0);
                        else if (_activeTool == Tools.Pan)
                        {
                            var dX = (_dragEndPoint.X - _dragStartPoint.X);
                            var dY = (_dragEndPoint.Y - _dragStartPoint.Y);
                            pe.Graphics.DrawImageUnscaled(image, dX, dY);
                        }
                        else
                        {
                            var ul = _map.WorldToImage(imageEnvelope.TopLeft());
                            var lr = _map.WorldToImage(imageEnvelope.BottomRight());
                            pe.Graphics.DrawImage(image, RectangleF.FromLTRB(ul.X, ul.Y, lr.X, lr.Y));
                        }
                    }
                    //
                    // This is never going to happen with the above condition in place
                    //
                    /*
                    else if (_activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut)
                    {
                        var rect = new RectangleF(0, 0, _map.Size.Width, _map.Size.Height);

                        if (_map.Zoom/_scaling < _map.MinimumZoom)
                            _scaling = (float) Math.Round(_map.Zoom/_map.MinimumZoom, 4);

                        if (_previewMode == PreviewModes.Best)
                            _scaling *= 3;

                        rect.Width *= _scaling;
                        rect.Height *= _scaling;

                        rect.Offset(_map.Size.Width/2f - rect.Width/2, _map.Size.Height/2f - rect.Height/2);


                        pe.Graphics.DrawImage(_dragImage, rect);
                    }*/
                }
                else
                {
                    var image = _miRenderer.ImageValue;
                    if (image != null && image.PixelFormat != PixelFormat.Undefined)
                    {
                        
                        var imageEnvelope = _miRenderer.ImageEnvelope;
                        if (_map.Envelope.Equals(imageEnvelope))
                            pe.Graphics.DrawImageUnscaled(image, 0, 0);
                        else {
                            var ul = _map.WorldToImage(imageEnvelope.TopLeft());
                            var lr = _map.WorldToImage(imageEnvelope.BottomRight());
                            pe.Graphics.DrawImage(image, RectangleF.FromLTRB(ul.X, ul.Y, lr.X, lr.Y));
                        }
                    }
                }

                //Draws current line or polygon (Draw Line or Draw Polygon tool)
                if (_pointArray != null)
                {
                    if (_pointArray.Count == 1)
                    {
                        var p1 = Map.WorldToImage(_pointArray[0]);
                        var p2 = Map.WorldToImage(_pointArray[1]);
                        pe.Graphics.DrawLine(new Pen(Color.Gray, 2F), p1, p2);
                    }
                    else
                    {
                        var pts = new PointF[_pointArray.Count];
                        for (int i = 0; i < pts.Length; i++)
                            pts[i] = Map.WorldToImage(_pointArray[i]);

                        if (_activeTool == Tools.DrawPolygon)
                        {
                            Color c = Color.FromArgb(127, Color.Gray);
                            pe.Graphics.FillPolygon(new SolidBrush(c), pts);
                            pe.Graphics.DrawPolygon(new Pen(Color.Gray, 2F), pts);
                        }
                        else
                        {
                            if (pts.Length > 0)
                            pe.Graphics.DrawLines(new Pen(Color.Gray, 2F), pts);
                        }
                    }
                }

                // Invoke the base implementation to get the event fired
                base.OnPaint(pe);

                // Do we have a custom tool
                if (UseCurrentTool) { _currentTool.DoPaint(pe); }

                /*Draw Floating Map-Decorations*/
                if (_map != null && _map.Decorations != null)
                {
                    foreach (Rendering.Decoration.IMapDecoration md in _map.Decorations)
                    {
                        md.Render(pe.Graphics, _map);
                    }
                }
            }
            catch (Exception ee)
            {
                _logger.Error(ee);
            }
        }

        /// <summary>
        /// Invokes the <see cref="E:System.Windows.Forms.Control.MouseUp"/>-event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event arguments.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _logger.DebugFormat("MouseUp {0} ({1}, {2})", Name, e.Location, e.Button);

            // call base function
            base.OnMouseUp(e);

            // Do we have a map? If not bail out
            if (_map == null)
                return;

            // Position in world coordinates
            var p = _map.ImageToWorld(new Point(e.X, e.Y), true);

            // Raise event
            if (MouseUp != null)
                MouseUp(p, e);

            // Do we have a custom tool
            if (UseCurrentTool)
            {
                if (_currentTool.DoMouseUp(p, e))
                    return;
            }

            // If no tool is selected, bail out
            if (_activeTool == Tools.None)
                return;

            bool needToRefresh = false;

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
            {
                if (_activeTool == Tools.ZoomOut)
                {
                    double scale = 0.5;
                    if (_dragging)
                    {
                        if (e.Y - _dragStartPoint.Y < 0) //Zoom out
                            scale = (float) Math.Pow(1/(float) (_dragStartPoint.Y - e.Y), 0.5);
                        else //Zoom in
                            scale = 1 + (e.Y - _dragStartPoint.Y)*0.1;
                    }
                    else
                    {
                        var oldCenter = _map.Center;

                        _map.Center = _map.ImageToWorld(new Point(e.X, e.Y));

                        if (!_map.Center.Equals2D(oldCenter, PrecisionTolerance))
                        {
                            needToRefresh = true;
                            OnMapCenterChanged(_map.Center);
                        }
                    }

                    var oldZoom = _map.Zoom;

                    _map.Zoom /= scale;

                    if (Math.Abs(oldZoom - _map.Zoom) > PrecisionTolerance)
                    {
                        needToRefresh = true;
                        OnMapZoomChanged(_map.Zoom);
                    }
                        
                }
                else if (_activeTool == Tools.ZoomIn)
                {
                    double scale = 2;
                    if (_dragging)
                    {
                        if (e.Y - _dragStartPoint.Y < 0) //Zoom out
                            scale = (float) Math.Pow(1/(float) (_dragStartPoint.Y - e.Y), 0.5);
                        else //Zoom in
                            scale = 1 + (e.Y - _dragStartPoint.Y)*0.1;
                    }
                    else
                    {
                        var oldCenter = _map.Center;
                        _map.Center = _map.ImageToWorld(new Point(e.X, e.Y));

                        if (!_map.Center.Equals2D(oldCenter, PrecisionTolerance))
                        {
                            needToRefresh = true;
                            OnMapCenterChanged(_map.Center);
                        }
                            
                    }

                    var oldZoom = _map.Zoom;
                    _map.Zoom =oldZoom * 1/scale;

                    if (Math.Abs(_map.Zoom - oldZoom) > PrecisionTolerance)
                    {
                        needToRefresh = true;
                        OnMapZoomChanged(_map.Zoom);
                    }
                        
                }
                else if ((_activeTool == Tools.Pan &&
                          !(_shiftButtonDragRectangleZoom && (ModifierKeys & Keys.Shift) != Keys.None)) ||
                         (e.Button == MouseButtons.Left && _dragging &&
                          (_activeTool == Tools.DrawLine || _activeTool == Tools.DrawPolygon)))
                {
                    if (_dragging)
                    {
                        if (_dragStartCenter == null || !_dragStartCenter.Equals2D(_map.Center, PrecisionTolerance))
                        {
                            needToRefresh = true;
                            OnMapCenterChanged(_map.Center);
                        }
                            
                    }
                    else
                    {
                        if (_panOnClick)
                        {
                            var oldValue = _map.Center;
                            _map.Center = p;

                            if (!_map.Center.Equals2D(oldValue, PrecisionTolerance))
                            {
                                needToRefresh = true;
                                OnMapCenterChanged(_map.Center);
                            }
                        }
                    }
                }
                else if (_activeTool == Tools.QueryBox || _activeTool == Tools.QueryPoint
                    /*|| _activeTool == Tools.QueryPolygon*/)
                {
                    //OnMouseUpQuery(e);
                    var mqs = MapQueryStarted;
                    if (mqs != null)
                        mqs(this, new EventArgs());

                    var layersToQuery = GetLayersToQuery();

                    if (layersToQuery.Count > 0)
                    {
                        var foundData = false;
                        foreach (var layer in layersToQuery)
                        {
                            Envelope bounding;
                            var isPoint = false;
                            if (_dragging)
                            {
                                Coordinate lowerLeft, upperRight;
                                GetBounds(_map.ImageToWorld(_dragStartPoint), _map.ImageToWorld(_dragEndPoint),
                                    out lowerLeft, out upperRight);

                                bounding = new Envelope(lowerLeft, upperRight);
                            }
                            else
                            {
                                bounding = new Envelope(_map.ImageToWorld(new Point(e.X, e.Y)));
                                bounding = bounding.Grow(_map.PixelSize*_queryGrowFactor);
                                isPoint = true;
                            }

                            var ds = new Data.FeatureDataSet();
                            if (_activeTool == Tools.QueryBox)
                            {
                                layer.ExecuteIntersectionQuery(bounding, ds);
                            }
                            else
                            {
                                IGeometry geom;
                                if (isPoint && QueryGrowFactor == 0)
                                    geom = _map.Factory.CreatePoint(_map.ImageToWorld(new Point(e.X, e.Y)));
                                else
                                    geom = _map.Factory.ToGeometry(bounding);
                                layer.ExecuteIntersectionQuery(geom, ds);
                            }

                            if (MapQueried != null)
                            {
                                if (ds.Tables.Count > 0)
                                {
                                    //Fire the event for all the resulting tables
                                    foreach (var dt in ds.Tables)
                                    {
                                        if (dt.Rows.Count > 0)
                                        {
                                            MapQueried(dt);
                                            foundData = true;
                                            if (_mapQueryMode == MapQueryType.TopMostLayer)
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (_mapQueryMode == MapQueryType.LayerByIndex)
                                        MapQueried(
                                            new Data.FeatureDataTable(new System.Data.DataTable(layer.LayerName)));
                                }
                            }

                            //If we found data and querymode is TopMostLayer we should abort now..
                            if (foundData && _mapQueryMode == MapQueryType.TopMostLayer)
                            {
                                break;
                            }
                        }
                    }
                    var mqd = MapQueryDone;
                    if (mqd != null)
                        mqd(this, new EventArgs());
                }

                else if (_activeTool == Tools.ZoomWindow ||
                         (_shiftButtonDragRectangleZoom && (ModifierKeys & Keys.Shift) != Keys.None))
                {
                    if (_rectangle.Width > 0 && _rectangle.Height > 0)
                    {
                        var zoomWindowStartPoint = _dragStartPoint;
                        var zoomWindowEndPoint = new PointF(e.X, e.Y); 
                    
                        Coordinate lowerLeft;
                        Coordinate upperRight;
                        GetBounds(
                            _map.ImageToWorld(zoomWindowStartPoint), 
                            _map.ImageToWorld(zoomWindowEndPoint),
                            out lowerLeft, 
                            out upperRight);
                            
                        _dragEndPoint.X = 0;
                        _dragEndPoint.Y = 0;

                        var oldCenter = _map.Center;
                        var oldZoom = _map.Zoom;

                        _map.ZoomToBox(new Envelope(lowerLeft, upperRight));

                        if (!_map.Center.Equals2D(oldCenter, PrecisionTolerance) ||
                            Math.Abs(oldZoom - _map.Zoom) > PrecisionTolerance)
                        {
                            needToRefresh = true;
                            OnMapZoomChanged(_map.Zoom);
                        }
                        else
                        {
                            // we must to cancel the selected area anyway
                            Invalidate();
                        }
                    }
                }
                else if (_activeTool == Tools.DrawPoint)
                {
                    if (GeometryDefined != null)
                    {
                        GeometryDefined(_map.Factory.CreatePoint(Map.ImageToWorld(new PointF(e.X, e.Y))));
                    }
                }
                else if (_activeTool == Tools.DrawPolygon || _activeTool == Tools.DrawLine)
                {
                    //pointArray = null;
                    if (_pointArray == null)
                    {
                        _pointArray = new List<Coordinate>(2);
                        _pointArray.Add(Map.ImageToWorld(e.Location));
                        _pointArray.Add(Map.ImageToWorld(e.Location));
                    }
                    else
                    {
                        //var temp = new Coordinate[_pointArray.Count + 2];
                        _pointArray.Add(Map.ImageToWorld(e.Location));
                    }
                }
            }


            if (_dragging)
            {
                _dragging = false;
                if (_activeTool == Tools.QueryBox)
                    Invalidate(_rectangle);
                if (_activeTool == Tools.ZoomWindow || _activeTool == Tools.QueryBox
                    /*|| _activeTool == Tools.QueryPolygon*/)
                    _rectangle = Rectangle.Empty;

                if (_dragStartCenter == null || !_dragStartCenter.Equals2D(_map.Center, PrecisionTolerance))
                    Refresh();

                if (_dragTransform != null)
                    _dragTransform.Dispose();
            }
            else if (needToRefresh && (_activeTool == Tools.ZoomIn || _activeTool == Tools.ZoomOut || _activeTool == Tools.Pan))
            {
                Refresh();
            }
        }

        private List<ICanQueryLayer> GetLayersToQuery()
        {
            var layersToQuery = new List<ICanQueryLayer>();
            switch (_mapQueryMode)
            {
                case MapQueryType.LayerByIndex:
                    if (_map.Layers.Count > _queryLayerIndex && _queryLayerIndex > -1)
                    {
                        var layer = _map.Layers[_queryLayerIndex] as ICanQueryLayer;
                        if (layer != null)
                        {
                            layersToQuery.Add(layer);
                        }
                    }
                    break;
                case MapQueryType.TopMostLayer:
                case MapQueryType.VisibleLayers:
                    foreach (var layer in _map.Layers)
                    {
                        var cqLayer = layer as ICanQueryLayer;
                        double visibleLevel = layer.VisibilityUnits == Styles.VisibilityUnits.ZoomLevel ? _map.Zoom  : _map.MapScale;
                        if (cqLayer != null && layer.Enabled && layer.MinVisible < visibleLevel &&
                            layer.MaxVisible >= visibleLevel && cqLayer.IsQueryEnabled)
                            layersToQuery.Add(cqLayer);
                    }
                    if (_mapQueryMode == MapQueryType.TopMostLayer)
                        layersToQuery.Reverse();
                    break;
                default:
                    foreach (var layer in _map.Layers)
                    {
                        var cqLayer = layer as ICanQueryLayer;
                        if (cqLayer != null && cqLayer.IsQueryEnabled)
                            layersToQuery.Add(cqLayer);
                    }
                    break;
            }

            return layersToQuery;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            // call base function
            base.OnMouseDoubleClick(e);

            if (_map == null)
                return;

            // Do we have an active tool?
            if (_activeTool == Tools.None)
                return;

            // Do we have a custom tool
            if (UseCurrentTool)
            {
                if (_currentTool.DoMouseDoubleClick(_map.ImageToWorld(e.Location), e))
                    return;
            }


            if (_activeTool == Tools.DrawPolygon)
            {
                if (GeometryDefined != null)
                {
                    var cl = new NetTopologySuite.Geometries.CoordinateList(_pointArray, false);
                    cl.CloseRing();
                    GeometryDefined(Map.Factory.CreatePolygon(Map.Factory.CreateLinearRing(NetTopologySuite.Geometries.CoordinateArrays.AtLeastNCoordinatesOrNothing(4, cl.ToCoordinateArray())), null));
                }
                ActiveTool = Tools.None;
            }

            else if (_activeTool == Tools.DrawLine)
            {
                if (GeometryDefined != null)
                {
                    var cl = new NetTopologySuite.Geometries.CoordinateList(_pointArray, false);
                    GeometryDefined(Map.Factory.CreateLineString(NetTopologySuite.Geometries.CoordinateArrays.AtLeastNCoordinatesOrNothing(2, cl.ToCoordinateArray())));
                }
                ActiveTool = Tools.None;
            }
        }


        private static void GetBounds(Coordinate p1, Coordinate p2,
                                      out Coordinate lowerLeft, out Coordinate upperRight)
        {
            lowerLeft = new Coordinate(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            upperRight = new Coordinate(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("p1: " + p1);
                _logger.Debug("p2: " + p2);
                _logger.Debug("lowerLeft: " + lowerLeft);
                _logger.Debug("upperRight: " + upperRight);
            }
        }

        /// <summary>
        /// MouseWheelGrabber is a MessageFilter that enables mousewheelcapture on mapcontrol even if the control does 
        /// not have focus as long as the mouse is positioned over the control
        /// </summary>
        private class MouseWheelGrabber : IMessageFilter
        {
            //[DllImport("user32.dll")]
            //private static extern IntPtr WindowFromPoint(Point lpPoint);

            //[DllImport("user32.dll")]
            //private static extern bool GetCursorPos(out Point lpPoint);

            //private static IntPtr GetWindowUnderCursor()
            //{
            //    Point ptCursor;

            //    if (!(GetCursorPos(out ptCursor)))
            //        return IntPtr.Zero;

            //    return WindowFromPoint(ptCursor);
            //}

            private bool _mouseIn = false;
            private readonly MapBox _redirectHandle;

            public MouseWheelGrabber(MapBox redirectHandle)
            {
                _redirectHandle = redirectHandle;
                _redirectHandle.MouseEnter += HandleMouseEnter;
                _redirectHandle.MouseLeave += HandleMouseLeave;
            }

            private void HandleMouseLeave(object sender, EventArgs e)
            {
                _mouseIn = false;
            }

            private void HandleMouseEnter(object sender, EventArgs e)
            {
                _mouseIn = true;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == 0x020A)
                {
                    var delta = ((int) (m.WParam.ToInt64() & 0xFFFF0000) >> 16);
                    var pt = _redirectHandle.PointToClient(new Point(m.LParam.ToInt32()));
                    if (_redirectHandle.ClientRectangle.Contains(pt))
                    {
                        if (_mouseIn)
                        {
                            _redirectHandle.OnMouseWheel(
                                new MouseEventArgs(MouseButtons.Middle, 0, pt.X, pt.Y, delta));
                            return true;
                        }
                        //var hWnd = GetWindowUnderCursor();
                        //if (hWnd == _redirectHandle.Handle)
                        //{
                        //    _redirectHandle.OnMouseWheel(
                        //        new MouseEventArgs(MouseButtons.Middle, 0, pt.X, pt.Y, delta));
                        //    return true;
                        //}
                    }

                }

                return false;
            }

        }

        internal void ClearDrag()
        {
            _dragEndPoint = new Point(0, 0);
            _dragging = false;
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
        static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, IntPtr hNull);

        [DllImport("gdi32.dll")]
        static extern bool DeleteEnhMetaFile(IntPtr hemf);

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
            IntPtr hEmf = mf.GetHenhmetafile();

            if (!hEmf.Equals(new IntPtr(0)))
            {
                IntPtr hEmf2 = CopyEnhMetaFile(hEmf, new IntPtr(0));
                if (!hEmf2.Equals(new IntPtr(0)))
                {
                    if (OpenClipboard(hWnd))
                    {
                        if (EmptyClipboard())
                        {
                            IntPtr hRes = SetClipboardData(14 /*CF_ENHMETAFILE*/, hEmf2);
                            bResult = hRes.Equals(hEmf2);
                            CloseClipboard();
                        }
                    }
                }
                DeleteEnhMetaFile(hEmf);
            }
            return bResult;
        }
    }
#endif
}
