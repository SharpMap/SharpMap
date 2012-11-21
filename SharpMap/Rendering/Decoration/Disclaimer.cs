using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap.Rendering.Symbolizer;

namespace SharpMap.Rendering.Decoration
{
    /// <summary>
    /// Disclaimer map decoration
    /// </summary>
    [Serializable]
    public class Disclaimer : MapDecoration
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public Disclaimer()
        {
            Font = SystemFonts.DefaultFont;
            ForeColor = Color.Black;
            Format = StringFormat.GenericTypographic;
            Text = "Powered by SharpMap";
            Anchor = MapDecorationAnchor.CenterBottom;
            Font = new Font("Arial", 8f, FontStyle.Italic);
            BorderMargin = new Size(3, 3);
            BorderColor = Color.Black;
            BorderWidth = 1;

            RoundedEdges = true;
        }

        /// <summary>
        /// Gets or sets the disclaimer text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the disclaimer font
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// Gets or sets the font color
        /// </summary>
        public Color ForeColor
        {
            get ; set;
        }

        private Pen _halo;
        [NonSerialized]
        private Color _haloColor;

        /// <summary>
        /// Gets or sets the halo width. A width of 0 disables rendering halo
        /// </summary>
        public int Halo
        {
            get { return _halo == null ? 0 : (int) _halo.Width; }
            set
            {
                if (value < 0) value = 0;
                if (value == 0)
                    _halo = null;
                else
                    _halo = new Pen(_haloColor, value);
            }
        }

        /// <summary>
        /// Gets or sets the halo color
        /// </summary>
        public Color HaloColor
        {
            get { return _haloColor; }
            set { 
                _haloColor = value;
                if (Halo > 0)
                    _halo = new Pen(value, Halo);
            }
        }

        /// <summary>
        /// Gets or sets the Format
        /// </summary>
        public StringFormat Format { get; set; }

        #region MapDecoration overrides

        /// <summary>
        /// Function to compute the required size for rendering the map decoration object
        /// <para>This is just the size of the decoration object, border settings are excluded</para>
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <returns>The</returns>
        protected override Size InternalSize(Graphics g, Map map)
        {
            var s = g.MeasureString(Text, Font);
            return new Size((int)Math.Ceiling(s.Width), (int)Math.Ceiling(s.Height));
        }

        /// <summary>
        /// Function to render the actual map decoration
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        protected override void OnRender(Graphics g, Map map)
        {
            var layoutRectangle = g.ClipBounds;
            var b = new SolidBrush(OpacityColor(ForeColor));
            if (Halo > 0)
            {
                var gp = new GraphicsPath();
                gp.AddString(Text, Font.FontFamily, (int)Font.Style, Utility.ScaleSizeToDeviceUnits(Font.SizeInPoints, GraphicsUnit.Point, g), layoutRectangle, Format);
                g.DrawPath(_halo, gp);
                g.FillPath(b, gp);
            }
            else
                g.DrawString(Text, Font, b, layoutRectangle);

        }

        #endregion
    }
}