#if !UseMapBoxAsMapImage
// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Layers;
using Point = GeoAPI.Geometries.Coordinate;
using Common.Logging;

namespace SharpMap.Forms
{
    /// <summary>
    /// MapImage Class - MapImage control for Windows forms
    /// </summary>
    /// <remarks>
    /// The MapImage control adds basic functionality to a Windows Form, such as dynamic pan, zoom and data query.
    /// </remarks>
    [DesignTimeVisible(true)]
    [Obsolete("Use MapBox instead, MapImage will be removed after 1.0 release")]
    public class MapImage : PictureBox
    {
        static ILog logger = LogManager.GetLogger(typeof(MapImage));
        #region Tools enum

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
            /// Pan on drag, Query on click
            /// </summary>
            PanOrQuery,
            /// <summary>
            /// No active tool
            /// </summary>
            None
        }

        #endregion

        private Tools _activetool;
        private double _fineZoomFactor = 10;
        private bool _isCtrlPressed;
        private Map _map;
        private int _queryLayerIndex;

        private double _wheelZoomMagnitude = 2;
        private System.Drawing.Point _mousedrag;
        private bool _mousedragging;
        private Image _mousedragImg;
        private bool _panOnClick;
        private bool _zoomOnDblClick;
        private Image _dragImg1, _dragImg2, _dragImgSupp;

        private Bitmap _staticMap;
        private Bitmap _variableMap;

        private bool _panOrQueryIsPan;
        private bool _zoomToPointer = false;

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public MapImage()
        {
            _map = new Map(Size);
            _activetool = Tools.None;
            base.MouseMove += new System.Windows.Forms.MouseEventHandler(MapImage_MouseMove);
            base.MouseUp += new System.Windows.Forms.MouseEventHandler(MapImage_MouseUp);
            base.MouseDown += new System.Windows.Forms.MouseEventHandler(MapImage_MouseDown);
            base.MouseWheel += new System.Windows.Forms.MouseEventHandler(MapImage_Wheel);
            base.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MapImage_DblClick);
            VariableLayerCollection.VariableLayerCollectionRequery += this.VariableLayersRequery;
            Cursor = Cursors.Cross;
            DoubleBuffered = true;
        }

        protected override void Dispose(bool disposing)        {            VariableLayerCollection.VariableLayerCollectionRequery -= this.VariableLayersRequery;            base.Dispose(disposing);        }        [Description("The amount which a single movement of the mouse wheel zooms by.")]
        [DefaultValue(-2)]
        [Category("Behavior")]
        public double WheelZoomMagnitude
        {
            get { return _wheelZoomMagnitude; }
            set { _wheelZoomMagnitude = value; }
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
        /// Sets whether the mouse wheel should zoom to the pointer location
        /// </summary>
        [Description("Sets whether the mouse wheel should zoom to the pointer location")]
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool ZoomToPointer
        {
            get { return _zoomToPointer; }
            set { _zoomToPointer = value; }
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
                    VariableLayerCollection.VariableLayerCollectionRequery += new VariableLayerCollectionRequeryHandler(VariableLayersRequery);
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Handles need to requery of variable layers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VariableLayersRequery(object sender, EventArgs e)
        {
            lock (_map)
            {
                if (_mousedragging) return;
                _variableMap = GetMap(_map.VariableLayers, LayerCollectionType.Variable);
            }
            UpdateImage();
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
            get { return _activetool; }
            set
            {
                bool fireevent = (value != _activetool);
                _activetool = value;
                if (value == Tools.Pan)
                    Cursor = Cursors.Hand;
                else
                    Cursor = Cursors.Cross;
                if (fireevent)
                    if (ActiveToolChanged != null)
                        ActiveToolChanged(value);

                // Check if settings collide
                if (value != Tools.None && ZoomOnDblClick)
                    ZoomOnDblClick = false;
                if (value != Tools.Pan && PanOnClick)
                    PanOnClick = false;
            }
        }

        ///// <summary>
        ///// Sets the direction of wheel for zoom-in operation
        ///// </summary>
        //public bool WheelZoomForward
        //{
        //    get { return _wheelZoomDirection==-1; }
        //    set { _wheelZoomDirection = (value == true) ? -1 : 1; }
        //}

        /// <summary>
        /// Sets whether the "go-to-cursor-on-click" feature is enabled or not (even if enabled it works only if the active tool is Pan)
        /// </summary>
        [Description("Sets whether the \"go-to-cursor-on-click\" feature is enabled or not (even if enabled it works only if the active tool is Pan)")]
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
        /// Sets whether the "go-to-cursor-and-zoom-in-on-double-click" feature is enable or not
        /// </summary>
        [Description("Sets whether the \"go-to-cursor-and-zoom-in-on-double-click\" feature is enable or not. This only works if no tool is currently active.")]
        [DefaultValue(true)]
        [Category("Behavior")]
        public bool ZoomOnDblClick
        {
            get { return _zoomOnDblClick; }
            set { 
                if (value)
                    ActiveTool = Tools.None;
                _zoomOnDblClick = value;
            }
        }

        /// <summary>
        /// Refreshes the map
        /// </summary>
        public override void Refresh()
        {
            if (_map != null)
            {
                _map.Size = Size;
                _staticMap = GetMap(_map.Layers, LayerCollectionType.Static);
                _variableMap = GetMap(_map.VariableLayers, LayerCollectionType.Variable);

                UpdateImage();
                base.Refresh();
                if (MapRefreshed != null)
                    MapRefreshed(this, null);
            }
        }

        private Bitmap GetMap(LayerCollection layers, LayerCollectionType layerCollectionType)
        {
            if ((layers == null || layers.Count == 0))
                return null;

            Bitmap retval = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(retval);
            _map.RenderMap(g, layerCollectionType);
            g.Dispose();

            if (layerCollectionType == LayerCollectionType.Variable)
                retval.MakeTransparent(_map.BackColor);

            return retval;

        }

        private void UpdateImage()
        {
            if (!(_staticMap == null && _variableMap == null))
            {
                Bitmap bmp = new Bitmap(Width, Height);
                Graphics g = Graphics.FromImage(bmp);
                if (_staticMap != null)
                    g.DrawImageUnscaled(_staticMap, 0, 0);
                if (_variableMap != null)
                    g.DrawImageUnscaled(_variableMap, 0, 0);
                g.Dispose();

                Image = bmp;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            _isCtrlPressed = e.Control;
            if (logger.IsDebugEnabled)
                logger.DebugFormat("Ctrl: {0}", _isCtrlPressed);

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _isCtrlPressed = e.Control;
            if (logger.IsDebugEnabled)
                logger.DebugFormat("Ctrl: {0}", _isCtrlPressed);

            base.OnKeyUp(e);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            if (!Focused)
            {
                bool isFocused = Focus();
                if (logger.IsDebugEnabled)
                    logger.Debug(isFocused);
            }

            base.OnMouseHover(e);
        }

        private void MapImage_Wheel(object sender, MouseEventArgs e)
        {
            if (_map != null)
            {
                if (_zoomToPointer)
                    _map.Center = _map.ImageToWorld(new System.Drawing.Point(e.X, e.Y), true);
         
                double scale = (e.Delta / 120.0);
                double scaleBase = 1 + (_wheelZoomMagnitude / (10 * (_isCtrlPressed ? _fineZoomFactor : 1)));

                _map.Zoom *= Math.Pow(scaleBase, scale);

                if (MapZoomChanged != null)
                    MapZoomChanged(_map.Zoom);

                if (_zoomToPointer)
                {
                    int NewCenterX = (this.Width / 2) + ((this.Width / 2) - e.X);
                    int NewCenterY = (this.Height / 2) + ((this.Height / 2) - e.Y);

                    _map.Center = _map.ImageToWorld(new System.Drawing.Point(NewCenterX, NewCenterY), true);

                    if (MapCenterChanged != null)
                        MapCenterChanged(_map.Center);
                }

                Refresh();
            }
        }

        private void MapImage_MouseDown(object sender, MouseEventArgs e)
        {
            _panOrQueryIsPan = false;
            if (_map != null)
            {
                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle) //dragging
                    _mousedrag = e.Location;
                if (MouseDown != null)
                    MouseDown(_map.ImageToWorld(new System.Drawing.Point(e.X, e.Y), true), e);
            }
        }

        private void MapImage_DblClick(object sender, MouseEventArgs e)
        {
            if (_map != null && ActiveTool == Tools.None)
            {
                double scaleBase = 1d + (Math.Abs(_wheelZoomMagnitude) / 10d);
                if (_zoomOnDblClick && e.Button == MouseButtons.Left)
                {
                    _map.Center = _map.ImageToWorld(new System.Drawing.Point(e.X, e.Y), true);
                    if (MapCenterChanged != null) { MapCenterChanged(_map.Center); }
                    _map.Zoom /= scaleBase;
                    if (MapZoomChanged != null) { MapZoomChanged(_map.Zoom); }
                    Refresh();
                }
                else if (_zoomOnDblClick && e.Button == MouseButtons.Right)
                {
                    _map.Zoom *= scaleBase;
                    if (MapZoomChanged != null) { MapZoomChanged(_map.Zoom); }
                    Refresh();
                }
            }
        }

        private void MapImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_map != null)
            {
                Point p = _map.ImageToWorld(new System.Drawing.Point(e.X, e.Y), true);

                if (MouseMove != null)
                    MouseMove(p, e);

                if (Image != null && e.Location != _mousedrag && !_mousedragging && (e.Button == MouseButtons.Left|| e.Button == MouseButtons.Middle))
                {
                    _mousedragImg = Image.Clone() as Image;
                    _mousedragging = true;
                    _dragImg1 = new Bitmap(Size.Width, Size.Height);
                    _dragImg2 = new Bitmap(Size.Width, Size.Height);
                }

                if (_mousedragging)
                {
                    if (MouseDrag != null)
                        MouseDrag(p, e);

                    if (ActiveTool == Tools.Pan || ActiveTool == Tools.PanOrQuery)
                    {
                        Graphics g = Graphics.FromImage(_dragImg1);
                        g.Clear(Color.Transparent);
                        g.DrawImageUnscaled(_mousedragImg,
                                            new System.Drawing.Point(e.Location.X - _mousedrag.X,
                                                                     e.Location.Y - _mousedrag.Y));
                        g.Dispose();
                        _dragImgSupp = _dragImg2;
                        _dragImg2 = _dragImg1;
                        _dragImg1 = _dragImgSupp;
                        Image = _dragImg2;
                        _panOrQueryIsPan = true;
                        base.Refresh();

                    }
                    else if (ActiveTool == Tools.ZoomIn || ActiveTool == Tools.ZoomOut)
                    {
                        Image img = new Bitmap(Size.Width, Size.Height);
                        Graphics g = Graphics.FromImage(img);
                        g.Clear(Color.Transparent);
                        float scale;
                        if (e.Y - _mousedrag.Y < 0) //Zoom out
                            scale = (float)Math.Pow(1 / (float)(_mousedrag.Y - e.Y), 0.5);
                        else //Zoom in
                            scale = 1 + (e.Y - _mousedrag.Y) * 0.1f;
                        RectangleF rect = new RectangleF(0, 0, Width, Height);
                        if (_map.Zoom / scale < _map.MinimumZoom)
                            scale = (float)Math.Round(_map.Zoom / _map.MinimumZoom, 4);
                        rect.Width *= scale;
                        rect.Height *= scale;
                        rect.Offset(Width / 2f - rect.Width / 2f, Height / 2f - rect.Height / 2);
                        g.DrawImage(_mousedragImg, rect);
                        g.Dispose();
                        Image = img;
                        if (MapZooming != null)
                            MapZooming(scale);
                    }
                }
            }
        }

        private void MapImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (_map != null)
            {
                if (MouseUp != null)
                    MouseUp(_map.ImageToWorld(new System.Drawing.Point(e.X, e.Y), true), e);

                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
                {
                    if (ActiveTool == Tools.ZoomOut)
                    {
                        double scale = 0.5;
                        if (!_mousedragging)
                        {
                            _map.Center = _map.ImageToWorld(new System.Drawing.Point(e.X, e.Y), true);
                            if (MapCenterChanged != null)
                                MapCenterChanged(_map.Center);
                        }
                        else
                        {
                            if (e.Y - _mousedrag.Y < 0) //Zoom out
                                scale = (float)Math.Pow(1 / (float)(_mousedrag.Y - e.Y), 0.5);
                            else //Zoom in
                                scale = 1 + (e.Y - _mousedrag.Y) * 0.1;
                        }
                        _map.Zoom *= 1 / scale;
                        if (MapZoomChanged != null)
                            MapZoomChanged(_map.Zoom);
                        Refresh();
                    }
                    else if (ActiveTool == Tools.ZoomIn)
                    {
                        double scale = 2;
                        if (!_mousedragging)
                        {
                            _map.Center = _map.ImageToWorld(new System.Drawing.Point(e.X, e.Y), true);
                            if (MapCenterChanged != null)
                                MapCenterChanged(_map.Center);
                        }
                        else
                        {
                            if (e.Y - _mousedrag.Y < 0) //Zoom out
                                scale = (float)Math.Pow(1 / (float)(_mousedrag.Y - e.Y), 0.5);
                            else //Zoom in
                                scale = 1 + (e.Y - _mousedrag.Y) * 0.1;
                        }
                        _map.Zoom *= 1 / scale;
                        if (MapZoomChanged != null)
                            MapZoomChanged(_map.Zoom);
                        Refresh();
                    }
                    else if (ActiveTool == Tools.Pan || (ActiveTool == Tools.PanOrQuery && _panOrQueryIsPan))
                    {
                        if (_mousedragging)
                        {
                            System.Drawing.Point pnt = new System.Drawing.Point(Width / 2 + (_mousedrag.X - e.Location.X),
                                                                                Height / 2 + (_mousedrag.Y - e.Location.Y));
                            _map.Center = _map.ImageToWorld(pnt, true);
                            if (MapCenterChanged != null)
                                MapCenterChanged(_map.Center);
                        }
                        else if(_panOnClick && !_zoomOnDblClick)
                        {
                            _map.Center = _map.ImageToWorld(new System.Drawing.Point(e.X, e.Y), true);
                            if (MapCenterChanged != null)
                                MapCenterChanged(_map.Center);
                        }
                        Refresh();
                    }
                    else if (ActiveTool == Tools.Query || (ActiveTool == Tools.PanOrQuery && !_panOrQueryIsPan))
                    {
                        if (_queryLayerIndex < 0)
                            MessageBox.Show("No active layer to query");
                        else if (_queryLayerIndex < _map.Layers.Count)
                            QueryLayer(_map.Layers[_queryLayerIndex], new PointF(e.X, e.Y));
                        else if(_queryLayerIndex - Map.Layers.Count < _map.VariableLayers.Count)
                            QueryLayer(_map.VariableLayers[_queryLayerIndex - Map.Layers.Count], new PointF(e.X, e.Y));
                        else
                            MessageBox.Show("No active layer to query");
                    }
                }
                if (_mousedragImg != null)
                {
                    _mousedragImg.Dispose();
                    _mousedragImg = null;
                }
                _mousedragging = false;
            }
        }

        /// <summary>
        /// Performs query on layer if it is of <see cref="ICanQueryLayer"/>
        /// </summary>
        /// <param name="layer">The layer to query</param>
        /// <param name="pt">The point to perform the query on</param>
        private void QueryLayer(ILayer layer, PointF pt)
        {
            if (layer is ICanQueryLayer)
            {
                ICanQueryLayer queryLayer = layer as ICanQueryLayer;

                Envelope bbox = new Envelope(
                        _map.ImageToWorld(pt, true)).Grow(_map.PixelSize*5);
                FeatureDataSet ds = new FeatureDataSet();
                queryLayer.ExecuteIntersectionQuery(bbox, ds);
                if (MapQueried != null)
                {
                    if (ds.Tables.Count > 0)
                        MapQueried(ds.Tables[0]);
                    else
                        MapQueried(new FeatureDataTable());
                }
                if (MapQueriedDataSet != null)
                    MapQueriedDataSet(ds);
            }
        }

        #region Events

        #region Delegates

        /// <summary>
        /// Eventtype fired when the map tool is changed
        /// </summary>
        /// <param name="tool"></param>
        public delegate void ActiveToolChangedHandler(Tools tool);

        /// <summary>
        /// Eventtype fired when the center has changed
        /// </summary>
        /// <param name="center"></param>
        public delegate void MapCenterChangedHandler(Point center);

        /// <summary>
        /// Eventtype fired when the map is queried
        /// </summary>
        /// <param name="data"></param>
        [Obsolete]
        public delegate void MapQueryHandler(FeatureDataTable data);

        /// <summary>
        /// Eventtype fired when the map is queried
        /// </summary>
        /// <param name="data"></param>
        public delegate void MapQueryDataSetHandler(FeatureDataSet data);

        /// <summary>
        /// Eventtype fired when the zoom was or are being changed
        /// </summary>
        /// <param name="zoom"></param>
        public delegate void MapZoomHandler(double zoom);

        /// <summary>
        /// MouseEventtype fired from the MapImage control
        /// </summary>
        /// <param name="WorldPos"></param>
        /// <param name="ImagePos"></param>
        public delegate void MouseEventHandler(Point WorldPos, MouseEventArgs ImagePos);

        #endregion

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
        /// Fired when the zoom value has changed
        /// </summary>
        public event MapZoomHandler MapZoomChanged;

        /// <summary>
        /// Fired when the map is being zoomed
        /// </summary>
        public event MapZoomHandler MapZooming;

        /// <summary>
        /// Fired when the map is queried
        /// </summary>
        public event MapQueryHandler MapQueried;

        public event MapQueryDataSetHandler MapQueriedDataSet;

        /// <summary>
        /// Fired when the center of the map has changed
        /// </summary>
        public event MapCenterChangedHandler MapCenterChanged;

        /// <summary>
        /// Fired when the active map tool has changed
        /// </summary>
        public event ActiveToolChangedHandler ActiveToolChanged;

        #endregion
    }
}
#endif