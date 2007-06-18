using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SharpMap.Forms
{
    /// <summary>
    /// MapBox Class - MapBox control for Windows forms
    /// </summary>
    /// <remarks>
    /// The ExtendedMapImage control adds more than basic functionality to a Windows Form, such as dynamic pan, widow zoom and data query.
    /// </remarks>
    [DesignTimeVisible(true)]
    public class MapBox : System.Windows.Forms.Control
    {
        #region PreviewModes enumerator
        public enum PreviewModes
        { 
            Best,
            Fast
        }
        #endregion

        #region Position enumerators
        private enum XPosition
        {
            Center = 0,
            Right = 1,
            Left = -1
        }

        private enum YPosition
        {
            Center = 0,
            Top = -1,
            Bottom = 1
        }
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
            /// No active tool
            /// </summary>
            None
        }
        #endregion

        #region Events
        /// <summary>
        /// MouseEventtype fired from the MapImage control
        /// </summary>
        /// <param name="WorldPos"></param>
        /// <param name="ImagePos"></param>
        public delegate void MouseEventHandler(SharpMap.Geometries.Point WorldPos, System.Windows.Forms.MouseEventArgs ImagePos);
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
        public event System.EventHandler MapRefreshed;

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
        public delegate void MapQueryHandler(SharpMap.Data.FeatureDataTable data);
        /// <summary>
        /// Fired when the map is queried
        /// </summary>
        public event MapQueryHandler MapQueried;


        /// <summary>
        /// Eventtype fired when the center has changed
        /// </summary>
        /// <param name="center"></param>
        public delegate void MapCenterChangedHandler(SharpMap.Geometries.Point center);
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
        #endregion

        private static int m_DefaultColorIndex = 0;
        private static Color[] m_DefaultColors = new Color[] { Color.DarkRed, Color.DarkGreen, Color.DarkBlue, Color.Orange, Color.Cyan, Color.Black, Color.Purple, Color.Yellow, Color.LightBlue, Color.Fuchsia };
        private const float MIN_DRAG_SCALING_BEFORE_REGEN = 0.3333f;
        private const float MAX_DRAG_SCALING_BEFORE_REGEN = 3f;

        public static void RandomizeLayerColors(Layers.VectorLayer layer)
        {
            layer.Style.EnableOutline = true;
            layer.Style.Fill =  new SolidBrush(Color.FromArgb(80, m_DefaultColors[m_DefaultColorIndex % m_DefaultColors.Length]));
            layer.Style.Outline = new Pen(Color.FromArgb(100, m_DefaultColors[(m_DefaultColorIndex + ((int)(m_DefaultColors.Length * 0.5))) % m_DefaultColors.Length]), 1f);
            m_DefaultColorIndex++;
        }

        private bool m_IsCtrlPressed = false;
        private double m_WheelZoomMagnitude = 2;
        private Tools m_ActiveTool;
        private double m_FineZoomFactor = 10;
        private SharpMap.Map m_Map;
        private int m_QueryLayerIndex;
        private System.Drawing.Point m_DragStartPoint;
        private System.Drawing.Point m_DragEndPoint;
        private System.Drawing.Bitmap m_DragImage;
        private Rectangle m_Rectangle = Rectangle.Empty;
        private bool m_Dragging = false;
        private SolidBrush m_RectangleBrush = new SolidBrush(Color.FromArgb(210, 244, 244, 244));
        private Pen m_RectanglePen = new Pen(Color.FromArgb(244, 244, 244), 1);
        private float m_Scaling = 0;
        private Image m_Image;
        private PreviewModes m_PreviewMode;

        [Description("The color of selecting rectangle.")]
        [Category("Appearance")]
        public Color SelectionBackColor
        {
            get { return m_RectangleBrush.Color; }
            set
            {
                if (value != m_RectangleBrush.Color)
                    m_RectangleBrush.Color = value;
            }
        }

        [Description("The map image currently visualized.")]
        [Category("Appearance")]
        public Image Image
        {
            get { return m_Image; }
        }

        [Description("The color of selectiong rectangle frame.")]
        [Category("Appearance")]
        public Color SelectionForeColor
        {
            get { return m_RectanglePen.Color; }
            set
            {
                if (value != m_RectanglePen.Color)
                    m_RectanglePen.Color = value;
            }
        }

        [Description("The amount which a single movement of the mouse wheel zooms by.")]
        [DefaultValue(2)]
        [Category("Behavior")]
        public double WheelZoomMagnitude
        {
            get { return m_WheelZoomMagnitude; }
            set { m_WheelZoomMagnitude = value; }
        }

        [Description("Mode used to create preview image while panning or zooming.")]
        [DefaultValue(MapBox.PreviewModes.Best)]

        [Category("Behavior")]
        public PreviewModes PreviewMode
        {
            get { return m_PreviewMode; }
            set
            {
                if (!m_Dragging)
                    m_PreviewMode = value;
            }
        }

        [Description("The amount which the WheelZoomMagnitude is divided by " +
            "when the Control key is pressed. A number greater than 1 decreases " +
            "the zoom, and less than 1 increases it. A negative number reverses it.")]
        [DefaultValue(10)]
        [Category("Behavior")]
        public double FineZoomFactor
        {
            get { return m_FineZoomFactor; }
            set { m_FineZoomFactor = value; }
        }

        /// <summary>
        /// Map reference
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SharpMap.Map Map
        {
            get { return m_Map; }
            set
            {
                m_Map = value;
                
                if (m_Map != null)
                    Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the index of the active query layer 
        /// </summary>
        public int QueryLayerIndex
        {
            get { return m_QueryLayerIndex; }
            set { m_QueryLayerIndex = value; }
        }

        /// <summary>
        /// Sets the active map tool
        /// </summary>
        public Tools ActiveTool
        {
            get { return m_ActiveTool; }
            set
            {
                bool check = (value != m_ActiveTool);
                m_ActiveTool = value;

                SetCursor();

                if (check && ActiveToolChanged != null)
                    ActiveToolChanged(value);
            }
        }

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public MapBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            m_Map = new SharpMap.Map(ClientSize);
            m_ActiveTool = Tools.None;
            LostFocus += new EventHandler(MapBox_LostFocus);
        }

        void MapBox_LostFocus(object sender, EventArgs e)
        {
            if (m_Dragging)
            {
                m_Dragging = false;
                Invalidate(ClientRectangle);
            }
        }

        private void SetCursor()
        {
            if (m_ActiveTool == Tools.None)
                Cursor = Cursors.Default;
            if (m_ActiveTool == Tools.Pan)
                Cursor = Cursors.Hand;
            else if (m_ActiveTool == Tools.Query)
                Cursor = Cursors.Help;
            else if (m_ActiveTool == Tools.ZoomIn || m_ActiveTool == Tools.ZoomOut || m_ActiveTool == Tools.ZoomWindow)
                Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Refreshes the map
        /// </summary>
        public override void Refresh()
        {
            if (m_Map != null)
            {
                m_Map.Size = ClientSize;
                if (m_Map.Layers == null || m_Map.Layers.Count == 0)
                    m_Image = null;
                else
                {
                    Cursor c = Cursor;
                    Cursor = Cursors.WaitCursor;
                    m_Image = m_Map.GetMap();
                    Cursor = c;
                }
                
                base.Refresh();
                
                if (MapRefreshed != null)
                    MapRefreshed(this, null);
            }
        }        

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

        protected override void OnMouseHover(EventArgs e)
        {
            if (!Focused)
            {
                bool isFocused = Focus();
                System.Diagnostics.Debug.WriteLine("Focused: " + isFocused);
            }

            base.OnMouseHover(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (m_Map != null)
            {
                double scale = ((double)e.Delta / 120.0);
                double scaleBase = 1 + (m_WheelZoomMagnitude / (10 * ((double)(m_IsCtrlPressed ? m_FineZoomFactor : 1))));

                m_Map.Zoom *= Math.Pow(scaleBase, scale);

                if (MapZoomChanged != null)
                    MapZoomChanged(m_Map.Zoom);

                Refresh();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (m_Map != null)
            {
                if (e.Button == MouseButtons.Left) //dragging
                {
                    m_DragStartPoint = e.Location;
                    m_DragEndPoint = e.Location;
                }
                
                if (MouseDown != null)
                    MouseDown(m_Map.ImageToWorld(new System.Drawing.Point(e.X, e.Y)), e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (m_Map != null)
            {
                SharpMap.Geometries.Point p = m_Map.ImageToWorld(new System.Drawing.Point(e.X, e.Y));

                if (MouseMove != null)
                    MouseMove(p, e);

                if (m_Image != null && e.Location != m_DragStartPoint && !m_Dragging && e.Button == MouseButtons.Left)
                {
                    m_Dragging = true;

                    if (m_ActiveTool == Tools.Pan || m_ActiveTool == Tools.ZoomIn || m_ActiveTool == Tools.ZoomOut)
                        m_DragImage = GenerateDragImage(m_PreviewMode);
                    else
                        m_DragImage = GenerateDragImage(PreviewModes.Fast);
                }

                if (m_Dragging)
                {
                    if (MouseDrag != null)
                        MouseDrag(p, e);

                    if (m_ActiveTool == Tools.Pan)
                    {
                        m_DragEndPoint = ClipPoint(e.Location);
                        Invalidate(ClientRectangle);
                    }
                    else if (m_ActiveTool == Tools.ZoomIn || m_ActiveTool == Tools.ZoomOut)
                    {
                        m_DragEndPoint = ClipPoint(e.Location);

                        if (m_DragEndPoint.Y - m_DragStartPoint.Y < 0) //Zoom out
                            m_Scaling = (float)Math.Pow(1 / (float)(m_DragStartPoint.Y - m_DragEndPoint.Y), 0.5);
                        else //Zoom in
                            m_Scaling = 1 + (m_DragEndPoint.Y - m_DragStartPoint.Y) * 0.1f;

                        if (MapZooming != null)
                            MapZooming(m_Map.Zoom / m_Scaling);

                        if (m_PreviewMode == PreviewModes.Best && (m_Scaling < MIN_DRAG_SCALING_BEFORE_REGEN || m_Scaling > MAX_DRAG_SCALING_BEFORE_REGEN))
                            RegenerateZoomingImage();

                        Invalidate(ClientRectangle);
                    }
                    else if (m_ActiveTool == Tools.ZoomWindow || m_ActiveTool == Tools.Query)
                    {
                        m_DragEndPoint = ClipPoint(e.Location);
                        Rectangle oldRectangle = m_Rectangle;
                        m_Rectangle = GenerateRectangle(m_DragStartPoint, m_DragEndPoint);
                        Invalidate(new Region(ClientRectangle));
                    }
                }
            }
        }

        private void RegenerateZoomingImage()
        {
            Cursor c = Cursor;
            Cursor = Cursors.WaitCursor;
            m_Map.Zoom /= m_Scaling;
            m_Image = m_Map.GetMap();
            m_Scaling = 1;
            m_DragImage = GenerateDragImage(PreviewModes.Best);
            m_DragStartPoint = m_DragEndPoint;
            Cursor = c;
        }

        private Bitmap GenerateDragImage(PreviewModes mode)
        {
            if (mode == PreviewModes.Best)
            {
                Cursor c = Cursor;
                Cursor = Cursors.WaitCursor;

                SharpMap.Geometries.Point realCenter = m_Map.Center;
                Bitmap bmp = new Bitmap(m_Map.Size.Width * 3, m_Map.Size.Height * 3);
                Graphics g = Graphics.FromImage(bmp);

                int i, j;
                for (i = -1; i <= 1; i++)
                {
                    for (j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                            g.DrawImageUnscaled(m_Image.Clone() as Image, m_Map.Size.Width, m_Map.Size.Height);
                        else
                            g.DrawImageUnscaled(GeneratePartialBitmap(realCenter, (XPosition)i, (YPosition)j), (i + 1) * m_Map.Size.Width, (j + 1) * m_Map.Size.Height);
                    }
                }
                g.Dispose();
                m_Map.Center = realCenter;

                Cursor = c;

                return bmp;
            }
            else
                return m_Image.Clone() as Bitmap;
        }

        private Bitmap GeneratePartialBitmap(SharpMap.Geometries.Point center, XPosition xPos, YPosition yPos)
        {
            double x = center.X, y = center.Y;

            switch (xPos)
            { 
                case XPosition.Right:
                    x += m_Map.Envelope.Width;
                    break;
                case XPosition.Left:
                    x -= m_Map.Envelope.Width;
                    break;                
            }

            switch (yPos)
            {
                case YPosition.Top:
                    y += m_Map.Envelope.Height;
                    break;
                case YPosition.Bottom:
                    y -= m_Map.Envelope.Height;
                    break;
            }

            m_Map.Center = new SharpMap.Geometries.Point(x, y);
            return m_Map.GetMap() as Bitmap;
        }

        private Point ClipPoint(Point p)
        {
            int x = p.X < 0 ? 0 : (p.X > ClientSize.Width ? ClientSize.Width : p.X);
            int y = p.Y < 0 ? 0 : (p.Y > ClientSize.Height ? ClientSize.Height : p.Y);
            return new Point(x, y);
        }

        private Rectangle GenerateRectangle(Point p1, Point p2)
        {
            int x = Math.Min(p1.X, p2.X);
            int y = Math.Min(p1.Y, p2.Y);
            int width = Math.Abs(p2.X - p1.X);
            int height = Math.Abs(p2.Y - p1.Y);

            return new Rectangle(x, y, width, height);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (m_Dragging)
            {
                if (m_ActiveTool == Tools.ZoomWindow || m_ActiveTool == Tools.Query)
                {
                    //Reset image to normal view
                    Bitmap patch = m_DragImage.Clone(pe.ClipRectangle, System.Drawing.Imaging.PixelFormat.DontCare);
                    pe.Graphics.DrawImageUnscaled(patch, pe.ClipRectangle);
                    patch.Dispose();

                    //Draw selection rectangle
                    if (m_Rectangle.Width > 0 && m_Rectangle.Height > 0)
                    {
                        pe.Graphics.FillRectangle(m_RectangleBrush, m_Rectangle);
                        Rectangle border = new Rectangle(m_Rectangle.X + (int)m_RectanglePen.Width / 2, m_Rectangle.Y + (int)m_RectanglePen.Width / 2, m_Rectangle.Width - (int)m_RectanglePen.Width, m_Rectangle.Height - (int)m_RectanglePen.Width);
                        pe.Graphics.DrawRectangle(m_RectanglePen, border);
                    }
                    return;
                }
                else if (m_ActiveTool == Tools.Pan)
                {
                    if (m_PreviewMode == PreviewModes.Best)
                        pe.Graphics.DrawImageUnscaled(m_DragImage, new Point(-m_Map.Size.Width + m_DragEndPoint.X - m_DragStartPoint.X, -m_Map.Size.Height + m_DragEndPoint.Y - m_DragStartPoint.Y));
                    else
                        pe.Graphics.DrawImageUnscaled(m_DragImage, new Point(m_DragEndPoint.X - m_DragStartPoint.X, m_DragEndPoint.Y - m_DragStartPoint.Y));
                    return;
                }
                else if (m_ActiveTool == Tools.ZoomIn || m_ActiveTool == Tools.ZoomOut)
                {
                    RectangleF rect = new RectangleF(0, 0, m_Map.Size.Width, m_Map.Size.Height);

                    if (m_Map.Zoom / m_Scaling < m_Map.MinimumZoom)
                        m_Scaling = (float)Math.Round(m_Map.Zoom / m_Map.MinimumZoom, 4);

                    //System.Diagnostics.Debug.WriteLine("Scaling: " + m_Scaling);

                    if (m_PreviewMode == PreviewModes.Best)
                        m_Scaling *= 3;

                    rect.Width *= m_Scaling;
                    rect.Height *= m_Scaling;

                    rect.Offset(m_Map.Size.Width / 2 - rect.Width / 2, m_Map.Size.Height / 2 - rect.Height / 2);

                    pe.Graphics.DrawImage(m_DragImage, rect);
                    return;
                }
            }

            if (m_Image != null)
                pe.Graphics.DrawImageUnscaled(m_Image, 0, 0);
            else
                base.OnPaint(pe);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (m_Map != null)
            {
                if (MouseUp != null)
                    MouseUp(m_Map.ImageToWorld(new Point(e.X, e.Y)), e);

                if (e.Button == MouseButtons.Left)
                {
                    if (m_ActiveTool == Tools.ZoomOut)
                    {
                        double scale = 0.5;
                        if (m_Dragging)
                        {
                            if (e.Y - m_DragStartPoint.Y < 0) //Zoom out
                                scale = (float)Math.Pow(1 / (float)(m_DragStartPoint.Y - e.Y), 0.5);
                            else //Zoom in
                                scale = 1 + (e.Y - m_DragStartPoint.Y) * 0.1;
                        }
                        else
                        {
                            m_Map.Center = m_Map.ImageToWorld(new System.Drawing.Point(e.X, e.Y));
                            
                            if (MapCenterChanged != null)
                                MapCenterChanged(m_Map.Center);
                        }

                        m_Map.Zoom /= scale;

                        if (MapZoomChanged != null)
                            MapZoomChanged(m_Map.Zoom);
                    }
                    else if (m_ActiveTool == Tools.ZoomIn)
                    {
                        double scale = 2;
                        if (m_Dragging)
                        {
                            if (e.Y - m_DragStartPoint.Y < 0) //Zoom out
                                scale = (float)Math.Pow(1 / (float)(m_DragStartPoint.Y - e.Y), 0.5);
                            else //Zoom in
                                scale = 1 + (e.Y - m_DragStartPoint.Y) * 0.1;
                        }
                        else
                        {
                            m_Map.Center = m_Map.ImageToWorld(new System.Drawing.Point(e.X, e.Y));
                            
                            if (MapCenterChanged != null)
                                MapCenterChanged(m_Map.Center);
                        }

                        m_Map.Zoom *= 1 / scale;
                        
                        if (MapZoomChanged != null)
                            MapZoomChanged(m_Map.Zoom);
                    }
                    else if (m_ActiveTool == Tools.Pan)
                    {
                        if (m_Dragging)
                        {
                            System.Drawing.Point point = new System.Drawing.Point(ClientSize.Width / 2 + (m_DragStartPoint.X - e.Location.X), ClientSize.Height / 2 + (m_DragStartPoint.Y - e.Location.Y));
                            m_Map.Center = m_Map.ImageToWorld(point);
                            
                            if (MapCenterChanged != null)
                                MapCenterChanged(m_Map.Center);
                        }
                        else
                        {
                            m_Map.Center = m_Map.ImageToWorld(new System.Drawing.Point(e.X, e.Y));
                            
                            if (MapCenterChanged != null)
                                MapCenterChanged(m_Map.Center);
                        }
                    }
                    else if (m_ActiveTool == Tools.Query)
                    {
                        if (m_Map.Layers.Count > m_QueryLayerIndex && m_QueryLayerIndex > -1)
                        {
                            if (m_Map.Layers[m_QueryLayerIndex].GetType() == typeof(SharpMap.Layers.VectorLayer))
                            {
                                
                                SharpMap.Layers.VectorLayer layer = m_Map.Layers[m_QueryLayerIndex] as SharpMap.Layers.VectorLayer;
                                SharpMap.Geometries.BoundingBox bounding;
                                
                                if (m_Dragging)
                                {
                                    SharpMap.Geometries.Point lowerLeft;
                                    SharpMap.Geometries.Point upperRight;
                                    GetBounds(m_Map.ImageToWorld(m_DragStartPoint), m_Map.ImageToWorld(m_DragEndPoint), out lowerLeft, out upperRight);

                                    bounding = new SharpMap.Geometries.BoundingBox(lowerLeft, upperRight);
                                }
                                else
                                    bounding = m_Map.ImageToWorld(new System.Drawing.Point(e.X, e.Y)).GetBoundingBox().Grow(m_Map.PixelSize * 5);
                                
                                SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();
                                layer.DataSource.Open();
                                layer.DataSource.ExecuteIntersectionQuery(bounding, ds);
                                layer.DataSource.Close();

                                if (MapQueried != null)
                                    MapQueried((ds.Tables.Count > 0 ? ds.Tables[0] : new SharpMap.Data.FeatureDataTable()));
                            }
                        }
                        else
                            MessageBox.Show("No active layer to query");
                    }
                    else if (m_ActiveTool == Tools.ZoomWindow)
                    {
                        if (m_Rectangle.Width > 0 && m_Rectangle.Height > 0)
                        {
                            SharpMap.Geometries.Point lowerLeft;
                            SharpMap.Geometries.Point upperRight;
                            GetBounds(m_Map.ImageToWorld(m_DragStartPoint), m_Map.ImageToWorld(m_DragEndPoint), out lowerLeft, out upperRight);

                            m_Map.ZoomToBox(new SharpMap.Geometries.BoundingBox(lowerLeft, upperRight));
                        }
                    }
                }

                if (m_DragImage != null)
                {
                    m_DragImage.Dispose();
                    m_DragImage = null;
                }

                if (m_Dragging)
                {
                    m_Dragging = false;

                    if (m_ActiveTool == Tools.Query)
                        Invalidate(m_Rectangle);

                    if (m_ActiveTool == Tools.ZoomWindow || m_ActiveTool == Tools.Query)
                        m_Rectangle = Rectangle.Empty;

                    Refresh();
                }
                else if (m_ActiveTool == Tools.ZoomIn || m_ActiveTool == Tools.ZoomOut || m_ActiveTool== Tools.Pan)
                    Refresh();
            }
        }

        private void GetBounds(SharpMap.Geometries.Point p1, SharpMap.Geometries.Point p2, out SharpMap.Geometries.Point lowerLeft, out SharpMap.Geometries.Point upperRight)
        {
            lowerLeft = new SharpMap.Geometries.Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            upperRight = new SharpMap.Geometries.Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));

            System.Diagnostics.Debug.WriteLine("p1: " + p1);
            System.Diagnostics.Debug.WriteLine("p2: " + p2);
            System.Diagnostics.Debug.WriteLine("lowerLeft: " + lowerLeft);
            System.Diagnostics.Debug.WriteLine("upperRight: " + upperRight);
        }
    }
}