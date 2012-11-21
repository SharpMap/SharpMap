using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SharpMap.Rendering.Decoration
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Abstract base class for all map decorations.
    /// <para>
    /// Handles framing and positioning of the decoration
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class MapDecoration : /* Component, */IMapDecoration
    {
        /// <summary>
        /// The size of this map decoration as computed/assigned previously
        /// </summary>
        protected Size _cachedSize;
        /// <summary>
        /// The bounding rectangle around the map decoration
        /// </summary>
        protected Rectangle _boundingRectangle;

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        protected MapDecoration()
        {
            BackgroundColor = Color.Transparent;
            Enabled = true;
            Opacity = 1;
            Location = new Point(5, 5);
            Padding = new Size(3, 3);
            Anchor = MapDecorationAnchor.RightBottom;

        }

        /// <summary>
        /// Gets or sets enabled status
        /// </summary>
        public bool Enabled { get; set; }


        #region Position

        /// <summary>
        /// The anchor of the map Decoration
        /// </summary>
        public MapDecorationAnchor Anchor { get; set; }

        /// <summary>
        /// The point that defines the location
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// Returns the left-top location of the Map decoration
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        private Point GetLocation(Map map)
        {
            var clipRect = map.Size;
            var objectSize = Size;

            var offsetX = Location.X;
            var offsetY = Location.Y;

            var anchors = (MapDecorationAnchorFlags)Anchor;
            switch (anchors & MapDecorationAnchorFlags.Horizontal)
            {
                case MapDecorationAnchorFlags.Right:
                    offsetX = clipRect.Width - (Location.X + objectSize.Width);
                    break;
                case MapDecorationAnchorFlags.HorizontalCenter:
                    offsetX = (clipRect.Width - (Location.X + objectSize.Width)) / 2;
                    break;
            }

            switch (anchors & MapDecorationAnchorFlags.Vertical)
            {
                case MapDecorationAnchorFlags.Bottom:
                    offsetY = clipRect.Height - (Location.Y + objectSize.Height);
                    break;
                case MapDecorationAnchorFlags.HorizontalCenter:
                    offsetY = (clipRect.Height - (Location.Y + objectSize.Height)) / 2;
                    break;
            }

            return new Point(offsetX, offsetY);
        }


        /// <summary>
        /// Gets or sets the Padding of the map decoration
        /// </summary>
        public Size Padding { get; set; }

        #endregion

        /// <summary>
        /// Function to compute an transparent color by combining <see cref="Opacity"/> with <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The base color</param>
        /// <returns>The (semi) transparent color</returns>
        protected Color OpacityColor(Color color)
        {
            if (color.A != 255)
                return color;

            return Color.FromArgb((int)(_opacity * 255), color);
        }

        /// <summary>
        /// Gets or sets the background color for the map decoration
        /// </summary>
        public Color BackgroundColor { get; set; }

        private float _opacity;
        
        /// <summary>
        /// Gets or sets the opacity of map decoration
        /// </summary>
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _opacity = value;
            }
        }

        #region Appearance Border

        /// <summary>
        /// The margin between decoration and border
        /// </summary>
        public Size BorderMargin { get; set; }

        /// <summary>
        /// Gets or sets the color of the border
        /// </summary>
        public Color BorderColor { get; set; }

        /// <summary>
        /// Gets or sets the width of the border
        /// </summary>
        public int BorderWidth { get; set; }

        /// <summary>
        /// Gets or sets whether the border should be rendered with rounded edges
        /// </summary>
        public bool RoundedEdges { get; set; }

#endregion

        /// <summary>
        /// Function to compute the required size for rendering the map decoration object
        /// <para>This is just the size of the decoration object, border settings are excluded</para>
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        /// <returns>The size of the map decoration</returns>
        protected abstract Size InternalSize(Graphics g, Map map);

        private void CalcMapDecorationMetrics(Graphics g, Map map)
        {
            _cachedSize = InternalSize(g, map);
            var rect = new Rectangle(Point.Add(GetLocation(map), BorderMargin), _cachedSize);
            _boundingRectangle = Rectangle.Inflate(rect, 2 * BorderMargin.Width, 2 * BorderMargin.Height);
        }

        private Size Size
        {
            get { return Size.Add(_cachedSize, Size.Add(BorderMargin, BorderMargin)); }
        }

        private Region GetClipRegion(Map map)
        {
            return new Region(new Rectangle(Point.Add(GetLocation(map), BorderMargin), _cachedSize));
        }

        private static GraphicsPath CreateRoundedRectangle(Rectangle rectangle, Size margin)
        {
            var gp = new GraphicsPath();

            //metrics
            int x1 = rectangle.Left + margin.Width;
            int x2 = rectangle.Right - margin.Width;
            int y1 = rectangle.Top + margin.Height;
            int y2 = rectangle.Bottom - margin.Height;

            int arcWidth = 2*margin.Width;
            int arcHeight = 2*margin.Height;

            if (arcWidth > 0 && arcHeight > 0)
            {

                gp.AddLine(x1, rectangle.Top, x2, rectangle.Top);
                gp.AddArc(x2 - margin.Width, rectangle.Top, arcWidth, arcHeight, 270, 90);
                gp.AddLine(rectangle.Right, y1, rectangle.Right, y2);
                gp.AddArc(x2 - margin.Width, y2 - margin.Height, arcWidth, arcHeight, 0, 90);
                gp.AddLine(x2, rectangle.Bottom, x1, rectangle.Bottom);
                gp.AddArc(rectangle.Left, y2 - margin.Height, arcWidth, arcHeight, 90, 90);
                gp.AddLine(rectangle.Left, y2, rectangle.Left, y1);
                gp.AddArc(rectangle.Left, rectangle.Top, arcWidth, arcWidth, 180, 90);

                gp.CloseFigure();
            }
            else
                gp.AddRectangle(rectangle);
            return gp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        public void Render(Graphics g, Map map)
        {
            //Is this map decoration enabled?
            if (!Enabled)
                return;

            //Preparing rendering
            OnRendering(g, map);

            //Draw border
            GraphicsPath gp;
            if (RoundedEdges && !BorderMargin.IsEmpty)
                gp = CreateRoundedRectangle(_boundingRectangle, BorderMargin);
            else
            {
                gp = new GraphicsPath();
                gp.AddRectangle(_boundingRectangle);
            }
            g.DrawPath(new Pen(OpacityColor(BorderColor), BorderWidth), gp);
            g.FillPath(new SolidBrush(OpacityColor(BackgroundColor)), gp);

            //Clip region
            var oldClip = g.Clip;
            g.Clip = GetClipRegion(map);

            //Actually render the Decoration
            OnRender(g, map);

            //Restore old clip region
            g.Clip = oldClip;


            //Finished rendering
            OnRendered(g, map);
        }

        /// <summary>
        /// Function to render the actual map decoration
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        protected virtual void OnRender(Graphics g, Map map)
        {
        }
        /// <summary>
        /// Function to render the actual map decoration
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        protected virtual void OnRendering(Graphics g, Map map)
        {
            CalcMapDecorationMetrics(g, map);
        }
        /// <summary>
        /// Function to render the actual map decoration
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        protected virtual void OnRendered(Graphics g, Map map)
        {
        }
    }
}