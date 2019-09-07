using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap.Styles;

namespace SharpMap.Rendering.Decoration.Graticule
{
    /// <summary>
    /// Graticule rendering properties, with Primary (major) and Secondary (minor) intervals.
    /// Use <see cref="PrimaryLineStyle"/>, <see cref="SecondaryLineStyle"/>, and <see cref="NumSubdivisions"/>
    /// to define how a graticule will render, and configure other properties to tune layout and formatting.  
    /// </summary>
    public partial class GraticuleStyle
    {
        private GraphicsPath _primaryTick;
        private GraphicsPath _secondaryTick;
        private bool _isDirty = true;

        private int _primaryTickSize = 10;
        private int _secondaryTickSize = 5;

        /// <summary>
        /// Apply a styling theme to a Graticule Style 
        /// </summary>
        public enum GraticuleTheme
        {
            Bold = 0,
            Subtle = 1,
            None = 2
        }

        /// <summary>
        /// Primary line style  
        /// Dashed and dotted line styles can be defined using Pen.DashPattern and Pen.DashStyle
        /// </summary>
        public Pen PrimaryPen { get; set; } = new Pen(Brushes.DarkSlateGray, 2);

        /// <summary>
        /// Line / tick style for primary graticule, or GraticuleLineStyle.None to disable this GraticuleStyle 
        /// </summary>
        public GraticuleLineStyle PrimaryLineStyle { get; set; } = GraticuleLineStyle.SolidTick;

        /// <summary>
        /// Size of tick (pixels) at intersection of primary graticule lines
        /// </summary>
        public int PrimaryTickSize
        {
            get => _primaryTickSize;
            set
            {
                _primaryTickSize = value;
                _isDirty = true;
            } 
        }

        /// <summary>
        /// Custom cross-hair style tick (design your own), null to use <see cref="PrimaryLineStyle"/>
        /// </summary>
        public GraphicsPath PrimaryCustomTick { get; set; }

        /// <summary>
        /// Length (pixels) of edge cuts (ie where graticule intersects border) or zero for none
        /// </summary>
        public int PrimaryMargin { get; set; } = 10;

        /// <summary>
        /// Secondary line style.
        /// Dashed and dotted line styles can be defined using Pen.DashPattern and Pen.DashStyle
        /// </summary>
        public Pen SecondaryPen { get; set; } = new Pen(Brushes.LightGray, 1);

        /// <summary>
        /// Line / tick style for secondary graticule, or GraticuleLineStyle.None to disable Secondary units
        /// </summary>
        public GraticuleLineStyle SecondaryLineStyle { get; set; } = GraticuleLineStyle.None;

        /// <summary>
        /// Size of tick (pixels) at intersection of secondary graticule lines
        /// </summary>
        public int SecondaryTickSize
        {
            get => _secondaryTickSize;
            set
            {
                _secondaryTickSize = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Custom cross-hair style tick (design your own), or null to use <see cref="SecondaryLineStyle"/>
        /// </summary>
        public GraphicsPath SecondaryCustomTick { get; set; }

        /// <summary>
        /// Length (pixels) of edge cuts (ie where graticule intersects border) or zero for none
        /// </summary>
        public int SecondaryMargin { get; set; } = 5;

        /// <summary>
        /// Edges to be labelled, taking into account any map rotation 
        /// </summary>
        public GraticuleBorders LabelBorders { get; set; } = GraticuleBorders.LeftBottom;

        /// <summary>
        /// Font for labelling primary graticule lines 
        /// </summary>
        public Font PrimaryLabelFont { get; set; } = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);

        /// <summary>
        /// Primary label color 
        /// </summary>
        public Brush PrimaryLabelColor { get; set; } = Brushes.DarkSlateGray;

        /// <summary>
        /// Offset to lower-left corner of label position relative to graticule intersection with border.
        /// Positive X offset is applied along the graticule line.
        /// Positive Y offset is applied perpendicular to graticule line.
        /// An offset of (2,2) will draw the label just inside the border, sitting just above the graticule.
        /// To position label on extension of edge cut, set X = <see cref="PrimaryMargin"/> and Y = negative half <see cref="PrimaryLabelFont"/> height
        /// </summary>
        public Point PrimaryLabelOffset { get; set; } = new Point(2, 2);

        /// <summary>
        /// Font for labelling secondary graticule lines 
        /// </summary>
        public Font SecondaryLabelFont { get; set; } = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);

        /// <summary>
        /// Secondary label color 
        /// </summary>
        public Brush SecondaryLabelColor { get; set; } = Brushes.DarkSlateGray;

        /// <summary>
        /// Offset to lower-left corner of label position relative to graticule intersection with border.
        /// Positive X offset is applied along the graticule line.
        /// Positive Y offset is applied perpendicular to graticule line.
        /// An offset of (2,2) will draw the label just inside the border, sitting just above the graticule.
        /// To position label on extension of edge cut, set X = <see cref="SecondaryMargin"/> and Y = negative half <see cref="SecondaryLabelFont"/> height
        /// </summary>
        public Point SecondaryLabelOffset { get; set; } = new Point(2, 2);

        /// <summary>
        /// True if secondary edge cuts (ie subdivisions) should be labelled in addition to primary labels (ie division)/>
        /// </summary>
        public bool LabelSubdivisions { get; set; } 

        /// <summary>
        /// Background color to assist reading labels, or null for no halo
        /// </summary>
        public Brush LabelHalo { get; set; }
        
        /// <summary>
        /// Interval between Primary graticule lines (calculated to provide at least 2 cuts along shortest border)
        /// </summary>
        public double Division { get; internal set; }

        /// <summary>
        /// Interval between Secondary graticule lines, derived from Division and NumSubdivisions
        /// </summary>
        public double Subdivision { get; internal set; }

        /// <summary>
        /// Number of intervals to appear between Primary graticule lines (or zero to disable Secondary graticule)
        /// </summary>
        public int NumSubdivisions { get; set; } = 2;

        /// <summary>
        /// Minimum visible zoom level for rendering, or zero for not applicable
        /// </summary>
        public double MinVisible { get; set; } = 0;

        /// <summary>
        /// Maximum visible zoom level for rendering, or double.MaxValue for not applicable
        /// </summary>
        public double MaxVisible { get; set; } = double.MaxValue;

        /// <summary>
        /// Gets or Sets what level-reference the Min/Max values are defined in
        /// </summary>
        public VisibilityUnits VisibilityUnits { get; set; } = VisibilityUnits.Scale;

        public GraticuleStyle()
        {
        }

        /// <summary>
        /// Quickly style a graticule without configuring each individual property 
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="lineStyle"></param>
        /// <param name="withSecondaryIntervals"></param>
        /// <param name="labelBorders"></param>
        public GraticuleStyle(GraticuleTheme theme, 
            GraticuleLineStyle lineStyle, 
            bool withSecondaryIntervals,
            GraticuleBorders labelBorders) : this()
        {
            switch (theme)
            {
                case GraticuleTheme.Bold:

                    PrimaryPen = new Pen(Brushes.DarkSlateGray, 2);
                    PrimaryLineStyle = lineStyle;
                    PrimaryTickSize = 16;
                    PrimaryMargin = 10;

                    SecondaryPen = new Pen(Brushes.LightSlateGray, 1);
                    SecondaryLineStyle = lineStyle;
                    SecondaryTickSize = 10;
                    SecondaryMargin = 6;

                    NumSubdivisions = 4;

                    PrimaryLabelFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular);
                    PrimaryLabelColor = Brushes.DarkSlateGray;
                    PrimaryLabelOffset = new Point(2, 1);

                    SecondaryLabelFont = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Regular);
                    SecondaryLabelColor = Brushes.LightSlateGray;
                    SecondaryLabelOffset = new Point(2, 1);

                    LabelBorders = labelBorders;
                    LabelHalo = Brushes.AliceBlue; 
                    LabelSubdivisions = true;

                    break;


                default:
                    
                    PrimaryPen = new Pen(Brushes.Gray, 2);
                    PrimaryLineStyle = lineStyle;
                    PrimaryTickSize = 10;
                    PrimaryMargin = 8;

                    SecondaryPen = new Pen(Brushes.LightGray, 1);
                    SecondaryLineStyle = lineStyle;
                    SecondaryTickSize = 6;
                    SecondaryMargin = 4;

                    NumSubdivisions = 4;

                    PrimaryLabelFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular);
                    PrimaryLabelColor = Brushes.Gray;
                    PrimaryLabelOffset = new Point(PrimaryMargin + 1, -1 - PrimaryLabelFont.Height / 2);

                    SecondaryLabelFont = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Regular);
                    SecondaryLabelColor = Brushes.LightGray;
                    SecondaryLabelOffset = new Point(SecondaryMargin + 1, -1 - SecondaryLabelFont.Height/2 );

                    LabelBorders = labelBorders;
                    LabelHalo = Brushes.Gainsboro; // Brushes.PowderBlue; 
                    LabelSubdivisions = true;

                    break;
            }

            if (theme == GraticuleTheme.None)
                PrimaryLineStyle = GraticuleLineStyle.None;

            if (!withSecondaryIntervals)
                SecondaryLineStyle = GraticuleLineStyle.None;
        }
        
        /// <summary>
        /// Returns true if a primary or secondary tick is required for given combination of primary and secondary meridians and parallels 
        /// </summary>
        public bool IsTickRequired(bool isPrimaryMeridian, bool isPrimaryParallel)
        {
            if (isPrimaryMeridian && isPrimaryParallel)
                return PrimaryLineStyle == GraticuleLineStyle.SolidTick ||
                       PrimaryLineStyle == GraticuleLineStyle.HollowTick;
            else
                return SecondaryLineStyle == GraticuleLineStyle.SolidTick ||
                       SecondaryLineStyle == GraticuleLineStyle.HollowTick;
        }
        
        /// <summary>
        /// Returns the appropriate tick
        /// </summary>
        public GraphicsPath GetTick(bool isPrimaryTick)
        {
            if (PrimaryCustomTick != null && isPrimaryTick) return PrimaryCustomTick;
            if (SecondaryCustomTick != null && !isPrimaryTick) return SecondaryCustomTick;

            if (_isDirty)
            {
                _primaryTick = CreateTick(PrimaryTickSize, PrimaryLineStyle);
                _secondaryTick = CreateTick(SecondaryMargin, SecondaryLineStyle);
                _isDirty = false;
            }
                
            return isPrimaryTick ? _primaryTick : _secondaryTick;
        }

        /// <summary>
        /// Create a simple tick (cross) symbol 
        /// </summary>
        private GraphicsPath CreateTick(int primaryTickSize, GraticuleLineStyle lineStyle)
        {
            if (lineStyle == GraticuleLineStyle.None
                || lineStyle == GraticuleLineStyle.Continuous)
                return null;

            PointF[] points;

            var tickSize50 = primaryTickSize * 0.5f;
            var tickSize20 = primaryTickSize * 0.2f;
            
            switch (lineStyle)
            {
                case GraticuleLineStyle.SolidTick:
                    points = new PointF[4];
                    points[0] = new PointF(-tickSize50, 0);
                    points[1] = new PointF(tickSize50, 0);
                    points[2] = new PointF(0, -tickSize50);
                    points[3] = new PointF(0, tickSize50);
                    
                    break;
                case GraticuleLineStyle.HollowTick:
                    points = new PointF[8];
                    points[0] = new PointF(-tickSize50, 0);
                    points[1] = new PointF(-tickSize20, 0);
                    points[2] = new PointF(tickSize20, 0);
                    points[3] = new PointF(tickSize50, 0);
                    
                    points[4] = new PointF(0,-tickSize50);
                    points[5] = new PointF(0,-tickSize20);
                    points[6] = new PointF(0,tickSize20);
                    points[7] = new PointF(0,tickSize50);
                    break;
                
                default:
                    return null;
            }
            
            var gp = new GraphicsPath();
            for (var i = 0; i < points.Length; i += 2)
            {
                gp.StartFigure();
                gp.AddLine(points[i], points[i+1]);
            }

            return gp;
        }

    }
}
