using System;
using System.Collections.Generic;
using System.Drawing;
using SharpMap.Utilities;

namespace SharpMap.Rendering.Decoration.ScaleBar
{
    /*
  Description  : Implement the Control interfaces.
  Developer    : Simen
  Contact      : SimenWu@hotmail.com
  Last Modified: 03/03/2003
*/

    /// <summary>
    /// Scale Bar map decoration
    /// </summary>
    [Serializable]
    public class ScaleBar : MapDecoration
    {
        #region Constants

        private const int PowerRangeMin = -5;
        private const int PowerRangeMax = 10;
        private const int NiceNumber = 4;
        private const int DefaultBarWidth = 9;
        private static readonly double[] NiceNumberArray = {1, 2, 2.5, 5};
        private const int ScalePrecisionDigits = 5;
        private const int GapScaleTextToBar = 3;
        private const int GapBarToSegmentText = 1;

        private const bool DefaultBarOutline = true;

        private static readonly Color DefaultForeColor = Color.Black;
        private static readonly Color DefaultBarOutlineCr = Color.Black;
        private static readonly Color DefaultBarColor1 = Color.Red;
        private static readonly Color DefaultBarColor2 = Color.White;

        private const int DefaultNumTics = 4;
        private const int DefaultMarginX = 8;
        private const int DefaultWidth = 180;

        private const double VerySmall = 0.0000001;
        #endregion

        private static readonly Dictionary<int, UnitInfo> Units = new Dictionary<int, UnitInfo>();
        
        static ScaleBar()
        {
            Units.Add((int)Unit.Custom, new UnitInfo((int)Unit.Custom, 1.0, "Unknown", "Unknown"));
            Units.Add((int)Unit.Meter, new UnitInfo((int)Unit.Meter, 1, "Meter", "m"));
            Units.Add((int)Unit.Foot_US, new UnitInfo((int)Unit.Custom, 0.30480061, "Foot_US", "ft"));
            Units.Add((int)Unit.Yard_Sears, new UnitInfo((int)Unit.Custom, 0.914398415, "Yards", "yard"));
            Units.Add((int)Unit.Yard_Indian, new UnitInfo((int)Unit.Custom, 0.914398531, "Yards", "Unknown"));
            Units.Add((int)Unit.Mile_US, new UnitInfo((int)Unit.Custom, 1609.347219, "Miles", "mi"));
            Units.Add((int)Unit.Kilometer, new UnitInfo((int)Unit.Custom, 1000.0, "Kilometers", "km"));
            Units.Add((int)Unit.Degree, new UnitInfo((int)Unit.Custom, 0.0175, "Degree", "d"));
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public ScaleBar()
        {
            MapUnit = (int)Unit.Meter;
            BarUnit = (int)Unit.Meter;
            Scale = 1;
            Width = DefaultWidth;
        }

        private Color _barColor1 = DefaultBarColor1;
        private Color _barColor2 = DefaultBarColor2;

        //unit
        private int _mapUnit = (int)Unit.Meter; //defaultMapUnit;
        private int _barUnit = (int)Unit.Meter; //defaultScaleBarUnit;
        
        //scale
        private double _scale; //Initial scale.  
        private double _mapWidth;
        private int _pageWidth;
        private double _lon1 ;
        private double _lon2;
        private double _lat;

        //bar
        private int _numTics = DefaultNumTics;
        private bool _barOutline = DefaultBarOutline;
        private Color _barOutlineColor = DefaultBarOutlineCr;
        private int _barWidth = DefaultBarWidth;
        private ScaleBarLabelText _barLabelText = ScaleBarLabelText.RepresentativeFraction;
        private ScaleBarStyle _barStyle = ScaleBarStyle.Standard;

        ////control
        //private bool m_bTransparentBG;
        //private bool m_bFormatNumber;

        // Create a default font to use with this control.
        private Color _foreColor = DefaultForeColor;
        private Font _font = (Font)SystemFonts.DefaultFont.Clone();

        private int _marginLeft = DefaultMarginX; //left margin for the scale bar
        private int _marginRight = DefaultMarginX; //right margin for the scale bar

        #region MapDecoration overrides

        /// <summary>
        /// Function to compute the required size for rendering the map decoration object
        /// <para>This is just the size of the decoration object, border settings are excluded</para>
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        /// <returns>The size of the map decoration</returns>
        protected override Size InternalSize(Graphics g, Map map)
        {
            CalcScale((int)g.DpiX);
            double width = MarginLeft + MarginRight + DefaultWidth;
            double height = 2 * BarWidth + 2 * _font.Height;
            return new Size((int)width, (int)height);
        }

        /// <summary>
        /// Function to render the actual map decoration
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        protected override void OnRender(Graphics g, Map map)
        {
            var rectF = g.ClipBounds;
            
            if (MapUnit == (int)Unit.Degree)
            {
                var p1 = map.ImageToWorld(new PointF(0, map.Size.Height*0.5f));
                var p2 = map.ImageToWorld(new PointF(map.Size.Width, map.Size.Height*0.5f));
                SetScaleD((int)g.DpiX, p1.X, p2.X, p1.Y, map.Size.Width);
            }
            else
                SetScale((int)g.DpiX, map.Envelope.Width, map.Size.Width);

            var rect = new Rectangle(Point.Truncate(rectF.Location), Size.Truncate(rectF.Size));
            RenderScaleBar(g, rect);
        }

        #endregion

        #region Private render methods
        
        private void RenderScaleBar(Graphics g, Rectangle rc)
        {
            int width = rc.Right - rc.Left;
            int height = rc.Bottom - rc.Top;
            int pixelsPerTic;

            double scaleBarUnitsPerTick;

            //if (!m_bTransparentBG) RenderBackground(g, rc);
            //if (_borderVisible && m_nBorderWidth > 0) RenderBorder(g, rc);

            //Get the scale first.
            if (_scale < VerySmall)
                return; //return if the scale is just too small

            ////Initialize the locale. So the we can use the latest locale setting to show the numbers.
            //m_locale.Init();

            //Draw the bar.
            CalcBarScale((int) g.DpiX, width, _numTics, _scale, _barUnitFactor, out pixelsPerTic,
                         out scaleBarUnitsPerTick);
            int nOffsetX = (width - _numTics*pixelsPerTic - _marginLeft - _marginRight)/2 + _marginLeft;
                //left margin 
            int nOffsetY = (height - _barWidth)/2;
            RenderBar(g, pixelsPerTic, rc.Left+nOffsetX, rc.Top + nOffsetY);
            RenderVerbalScale(g, rc.Left + width / 2, rc.Top + nOffsetY - GapScaleTextToBar);
            RenderSegmentText(g, rc.Left + nOffsetX, rc.Top + nOffsetY + _barWidth + GapBarToSegmentText, _numTics, pixelsPerTic,
                              scaleBarUnitsPerTick,
                              _barUnitShortName);
        }

        /// <summary>
        /// Render the bar
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="nTicLength">The length of each tic</param>
        /// <param name="nOffsetX">The x-offset</param>
        /// <param name="nOffsetY">The y-offset</param>
        private void RenderBar(Graphics g, int nTicLength, int nOffsetX, int nOffsetY)
        {
            Color cr1 = OpacityColor(_barColor1);
            Color cr2 = OpacityColor(_barColor2);
            Color crOutline = OpacityColor(_barOutlineColor);

            RenderBarWithStyle(g, nOffsetX, nOffsetY, _numTics, nTicLength, _barWidth, cr1, cr2, _barOutline,
                               crOutline, _barStyle);
        }

        /// <summary>
        /// Render the bar with <paramref name="style"/> Style.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="nOffsetX">The x-offset</param>
        /// <param name="nOffsetY">The y-offset</param>
        /// <param name="nNumTics">The number of tics</param>
        /// <param name="nTicLength">The length of each tic</param>
        /// <param name="nBarWidth">The width of the bar</param>
        /// <param name="crBar1">The first bar color</param>
        /// <param name="crBar2">The second bar color</param>
        /// <param name="bOutline">Value indicating whether the bar should be outlined</param>
        /// <param name="crOutline">The outline color</param>
        /// <param name="style">The <see cref="ScaleBarStyle"/></param>
        private static void RenderBarWithStyle(Graphics g, int nOffsetX, int nOffsetY, int nNumTics, int nTicLength,
                                               int nBarWidth,
                                               Color crBar1, Color crBar2, bool bOutline, Color crOutline,
                                               ScaleBarStyle style)
        {
            if (nBarWidth > 1)
            {
                switch (style)
                {
                    case ScaleBarStyle.Standard:
                        RenderTicBarStandard(g, nNumTics, nBarWidth, crBar1, crBar2, nTicLength, nOffsetX, nOffsetY,
                                             bOutline, crOutline);
                        break;
                    case ScaleBarStyle.Meridian:
                        RenderTicBarMeridian(g, nNumTics, nBarWidth, crBar1, crBar2, nTicLength, nOffsetX, nOffsetY,
                                             bOutline, crOutline);
                        break;
                    case ScaleBarStyle.Meridian1:
                        RenderTicBarMeridian1(g, nNumTics, nBarWidth, crBar1, crBar2, nTicLength, nOffsetX, nOffsetY,
                                              bOutline, crOutline);
                        break;
                    default:
                        RenderTicBarStandard(g, nNumTics, nBarWidth, crBar1, crBar2, nTicLength, nOffsetX, nOffsetY,
                                             bOutline, crOutline);
                        break;
                }
            }
            else
                RenderTicLine(g, nNumTics, crBar1, crBar2, nTicLength, nOffsetX, nOffsetY);
        }

        /// <summary>
        /// Render just the tic line
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="nNumTics">The number of tics</param>
        /// <param name="crBar1">The first bar color</param>
        /// <param name="crBar2">The second bar color</param>
        /// <param name="nTicLength">The length of each tic</param>
        /// <param name="xOrigin">The x-origin</param>
        /// <param name="yOrigin">The y-origin</param>
        private static void RenderTicLine(Graphics g, int nNumTics, Color crBar1, Color crBar2, int nTicLength,
                                          int xOrigin, int yOrigin)
        {
            var pen1 = new Pen(crBar1, 1);
            var pen2 = new Pen(crBar2, 1);

            int x = xOrigin;
            int y = yOrigin;
            for (int i = 0; i < nNumTics; i++)
            {
                g.DrawLine(i % 2 != 0 ? pen2 : pen1, x, y, x + nTicLength, y);
                x += nTicLength;
            }
        }

        /// <summary>
        /// Render the bar with <see name="ScaleBarStyle.Standard"/> Style.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="xOrigin">The x-offset</param>
        /// <param name="yOrigin">The y-offset</param>
        /// <param name="nNumTics">The number of tics</param>
        /// <param name="nTicLength">The length of each tic</param>
        /// <param name="nBarWidth">The width of the bar</param>
        /// <param name="crBar1">The first bar color</param>
        /// <param name="crBar2">The second bar color</param>
        /// <param name="bOutline">Value indicating whether the bar should be outlined</param>
        /// <param name="crOutline">The outline color</param>
        private static void RenderTicBarStandard(Graphics g, int nNumTics, int nBarWidth, Color crBar1, Color crBar2,
                                                 int nTicLength, int xOrigin, int yOrigin, bool bOutline,
                                                 Color crOutline)
        {
            Color cr1, cr2;

            //Create pens.
            if (bOutline)
            {
                cr1 = crOutline;
                cr2 = crOutline;
            }
            else
            {
                cr1 = crBar1;
                cr2 = crBar2;
            }
            var pen1 = new Pen(cr1, 1);
            var pen2 = new Pen(cr2, 1);

            //Create brushes.
            Brush brush1 = new SolidBrush(crBar1);
            Brush brush2 = new SolidBrush(crBar2);

            int x = xOrigin;
            int y1 = yOrigin;
            /*
                        int y2 = y1 + nWidth;
            */
            for (int i = 0; i < nNumTics; i++)
            {
                if (i % 2 == 1)
                    RenderRectangle(g, x, y1, nTicLength + 1, nBarWidth, pen1, brush1);
                else
                    RenderRectangle(g, x, y1, nTicLength + 1, nBarWidth, pen2, brush2);

                x += nTicLength;
            }
        }

        private static void RenderRectangle(Graphics g, int x, int y, int width, int height, Pen pen, Brush brush)
        {
            if (brush != null)
                g.FillRectangle(brush, x, y, width, height);
            g.DrawRectangle(pen, x, y, width, height);
        }

        /// <summary>
        /// Render the bar with <see cref="ScaleBarStyle.Meridian"/> Style.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="xOrigin">The x-offset</param>
        /// <param name="yOrigin">The y-offset</param>
        /// <param name="nNumTics">The number of tics</param>
        /// <param name="nTicLength">The length of each tic</param>
        /// <param name="nBarWidth">The width of the bar</param>
        /// <param name="crBar1">The first bar color</param>
        /// <param name="crBar2">The second bar color</param>
        /// <param name="bOutline">Value indicating whether the bar should be outlined</param>
        /// <param name="crOutline">The outline color</param>
        private static void RenderTicBarMeridian(Graphics g, int nNumTics, int nBarWidth, Color crBar1, Color crBar2,
                                                 int nTicLength, int xOrigin, int yOrigin, bool bOutline,
                                                 Color crOutline)
        {
            Color cr1, cr2;
            //Create pens.
            if (bOutline)
            {
                cr1 = crOutline;
                cr2 = crOutline;
            }
            else
            {
                cr1 = crBar1;
                cr2 = crBar2;
            }
            var pen1 = new Pen(cr1, 1);
            var pen2 = new Pen(cr2, 1);

            //Create brushes.
            Brush brush1 = new SolidBrush(crBar1);
            Brush brush2 = new SolidBrush(crBar2);

            int x = xOrigin;
            int y1 = yOrigin;
            //int y2 = y1 + nWidth;
            int y12 = y1 + nBarWidth / 2;
            int widthHalf = nBarWidth / 2 + 1;
            for (int i = 0; i < nNumTics; i++)
            {
                if (i % 2 == 1)
                {
                    RenderRectangle(g, x, y1, nTicLength + 1, widthHalf, pen2, brush2);
                    RenderRectangle(g, x, y12, nTicLength + 1, widthHalf, pen1, brush1);
                }
                else
                {
                    RenderRectangle(g, x, y1, nTicLength + 1, widthHalf, pen1, brush1);
                    RenderRectangle(g, x, y12, nTicLength + 1, widthHalf, pen2, brush2);
                }
                x += nTicLength;
            }
        }

        /// <summary>
        /// Render the bar with <see cref="ScaleBarStyle.Meridian1"/> Style.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="xOrigin">The x-offset</param>
        /// <param name="yOrigin">The y-offset</param>
        /// <param name="nNumTics">The number of tics</param>
        /// <param name="nTicLength">The length of each tic</param>
        /// <param name="nBarWidth">The width of the bar</param>
        /// <param name="crBar1">The first bar color</param>
        /// <param name="crBar2">The second bar color</param>
        /// <param name="bOutline">Value indicating whether the bar should be outlined</param>
        /// <param name="crOutline">The outline color</param>
        private static void RenderTicBarMeridian1(Graphics g, int nNumTics, int nBarWidth, Color crBar1, Color crBar2,
                                                  int nTicLength, int xOrigin, int yOrigin, bool bOutline,
                                                  Color crOutline)
        {
            Color cr1, cr2;
            //Create pens.
            if (bOutline)
            {
                cr1 = crOutline;
                cr2 = crOutline;
            }
            else
            {
                cr1 = crBar1;
                cr2 = crBar2;
            }
            Pen pen1 = new Pen(cr1, 1);
            Pen pen2 = new Pen(cr2, 1);

            //Create brushes.
            Brush brush1 = new SolidBrush(crBar1);
            Brush brush2 = new SolidBrush(crBar2);

            int x = xOrigin;
            int y1 = yOrigin;
            //int y2 = y1 + nWidth;
            int y12 = y1 + nBarWidth / 2;
            int widthHalf = nBarWidth / 2;
            for (int i = 0; i < nNumTics; i++)
            {
                if (i % 2 == 1)
                {
                    RenderRectangle(g, x, y1, nTicLength + 1, nBarWidth, pen2, brush2);
                }
                else
                {
                    RenderRectangle(g, x, y1, nTicLength + 1, widthHalf + 1, pen1, brush1);
                    RenderRectangle(g, x, y12, nTicLength + 1, widthHalf + 1, pen1, brush1);
                }
                x += nTicLength;
            }
        }


        #endregion

        /// <summary>
        /// Renders the verbal text above the scale bar.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="x">x-ordinate of the reference point for the text, the center of the bar</param>
        /// <param name="y">y-ordinate of the reference point for the text, the top or the bar</param>
        private void RenderVerbalScale(Graphics g, int x, int y)
        {
            int lastX = int.MinValue;
            //Get the scale text.
            var sbText = ScaleBarText(_scale, _barLabelText);
            //Draw the text.
            RenderTextWithFormat(g, sbText, x, y,
                                 new StringFormat
                                     {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far}, ref lastX);
        }

        private static readonly StringFormat TopCenter = 
            new StringFormat { 
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Center 
            };

        /// <summary>
        /// Renders the segment text below the scale bar
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="x">The x-ordinate of the reference point for the segment text, the beginning of the bar</param>
        /// <param name="y">The y-ordinate of the reference point for the segment text, the bottom of the bar</param>
        /// <param name="tics">The number of tics</param>
        /// <param name="ticWidth">The width of each tic in pixels</param>
        /// <param name="scaleBarUnitsPerTic">The scale bar units per tic</param>
        /// <param name="strUnit">The abbreviated unit text</param>
        private void RenderSegmentText(Graphics g, int x, int y, int tics, int ticWidth, double scaleBarUnitsPerTic,
                                       string strUnit)
        {
            int lastX = int.MinValue;
            RenderTextWithFormat(g, _barLabelText == ScaleBarLabelText.JustUnits ? "0" : strUnit, x, y, TopCenter, ref lastX);

            lastX = int.MinValue;
            //Set the output format.
            int precision = PrecitionOfSegmentText(scaleBarUnitsPerTic);
            string format = String.Format("F{0}", precision);

            for (int i = 1; i <= tics; i++)
            {
                double value = scaleBarUnitsPerTic*i;
                string text = value.ToString(format, System.Globalization.CultureInfo.CurrentUICulture);
                int offsetX = 0;
                if (i == tics)
                {
                    //Make sure this text is not overdrawn... (should be aligned with ticmark)
                    var size = MeasureDisplayStringWidthExact(g, text, _font);
                    int endX = x + ticWidth * i + size;
                    if (endX > _boundingRectangle.Right)
                    {
                        offsetX = _boundingRectangle.Right - endX;
                    }

                }

                RenderTextWithFormat(g, text, x + ticWidth * i + offsetX, y, TopCenter, ref lastX);
            }
        }

        /// <summary>
        /// Renders the text
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="text">The text to render</param>
        /// <param name="x">The x-ordinate of the reference point</param>
        /// <param name="y">The y-ordinate of the reference point</param>
        /// <param name="format">The string format, mainly vertical an horizontal alignment are of interest.</param>
        /// <param name="lastX">The last horizontal position, to ensure that texts are not overlapping</param>
        private void RenderTextWithFormat(Graphics g, string text, int x, int y, StringFormat format, ref int lastX)
        {
            //Get the size of the string
            var size = g.MeasureString(text, _font);
            switch (format.Alignment)
            {
                case StringAlignment.Far:
                    if (x - size.Width < lastX) return;
                    break;
                case StringAlignment.Center:
                    if (x - size.Width / 2 < lastX) return;
                    break;
                case StringAlignment.Near:
                    if (x  < lastX) return;
                    break;
            }

            //Output the text.
            g.DrawString(text, _font, new SolidBrush(OpacityColor(_foreColor)), new Point(x, y), format);

            //Keep track of latest x position.
            switch (format.Alignment)
            {
                case StringAlignment.Far:
                    lastX = x + (int) size.Width;
                    break;
                case StringAlignment.Center:
                    lastX = x + (int) (size.Width/2);
                    break;
                case StringAlignment.Near:
                    lastX = x;
                    break;
            }
        }

        /// <summary>
        /// Calculate the scale and store it in <see cref="_scale"/>.
        /// <para>
        /// It should be called to calculate the real map scale everytime the user change mapunit or set the scale
        /// </para>
        /// </summary>
        private void CalcScale(int dpi)
        {
            double fScale;
            if (_mapUnit == (int)Unit.Degree) //LatLong
                fScale = CalcRepresentativeFractionScaleD(_lon1, _lon2, _lat, _pageWidth, dpi);
            else
                fScale = CalcRepresentativeFractionScale(_mapWidth, _pageWidth, _mapUnitFactor, dpi);
            _scale = fScale;
        }

        /// <summary>
        /// Gets or sets the foreground color, used to render the labeling
        /// </summary>
        public Color ForeColor
        {
            get { return _foreColor; }
            set
            {
                _foreColor = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the font to label the bar
        /// </summary>
        public Font Font { get { return _font; }
            set
            {
                if (value == null)
                    return;
                _font = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the first bar color
        /// </summary>
        public Color BarColor1
        {
            get { return _barColor1; }
            set
            {
                _barColor1 = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the second bar color
        /// </summary>
        public Color BarColor2
        {
            get { return _barColor2; }
            set
            {
                _barColor2 = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the bar width
        /// </summary>
        public int BarWidth
        {
            get { return _barWidth; }
            set
            {
                if (value < 1) value = 1;
                _barWidth = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        private double _mapUnitFactor;
        private string _mapUnitName, _mapUnitShortName;

        private double _barUnitFactor;
        private string _barUnitName, _barUnitShortName;

        /// <summary>
        /// Map unit
        /// </summary>
        public int MapUnit
        {
            get { return _mapUnit; }
            set
            {
                _mapUnit = value;
                GetUnitInformation(_mapUnit, out _mapUnitFactor, out _mapUnitName, out _mapUnitShortName);
                CalcScale(96);
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Bar unit
        /// </summary>
        public int BarUnit
        {
            get { return _barUnit; }
            set
            {
                _barUnit = value;
                GetUnitInformation(_barUnit, out _barUnitFactor, out _barUnitName, out _barUnitShortName);
                CalcScale(96);
                OnViewChanged();
                Dirty = true;
            }
        }

        private static void GetUnitInformation(int mapUnit, out double mapUnitFactor, out string mapUnitName,
                                               out string mapUnitShortName)
        {
            UnitInfo unitInfo;
            if (Units.TryGetValue(mapUnit, out unitInfo))
            {
                mapUnitFactor = unitInfo.ToMeter;
                mapUnitName = unitInfo.Name;
                mapUnitShortName = unitInfo.Abbreviation;
            }
            else
            {
                mapUnitFactor = 1;
                mapUnitName = "Cusom";
                mapUnitShortName = string.Empty;
            }
        }


        /// <summary>
        /// Bar outline
        /// </summary>
        public bool BarOutline
        {
            get { return _barOutline; }
            set
            {
                _barOutline = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Bar outline color
        /// </summary>
        public Color BarOutlineColor
        {
            get { return _barOutlineColor; }
            set
            {
                _barOutlineColor = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Scale
        /// </summary>
        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="name"></param>
        /// <param name="shortName"></param>
        public void SetCustomUnit(double factor, string name, string shortName)
        {
            //If the user wants to use customer unit, then the map unit and the bar unit will be the same
            //Map Unit
            if (factor <= 0.0) //factor should be >0
                factor = 1.0;
            _mapUnitFactor = factor;
            _mapUnitName = name;
            _mapUnitShortName = shortName;

            //Bar Unit   
            _barUnitFactor = factor;
            _barUnitName = name;
            _barUnitShortName = shortName;

            CalcScale(96);
            OnViewChanged();
        }

        /*
        private void GetMapUnitInfo(out double factor, out string name, out string shortName)
        {
            factor = _mapUnitFactor;
            name = _mapUnitName;
            shortName = _mapUnitShortName;
        }

        private void GetBarUnitInfo(out double factor, out string name, out string shortName)
        {
            factor = _barUnitFactor;
            name = _barUnitName;
            shortName = _barUnitShortName;
        }
        */

        private void SetScaleD(int dpi, double lon1, double lon2, double lat, int widthInPixel)
        {
            _lon1 = lon1;
            _lon2 = lon2;
            _lat = lat;
            _pageWidth = widthInPixel;
            CalcScale(dpi);
            OnViewChanged();
        }

        private void SetScale(int dpi, double mapWidth, int widthInPixel)
        {
            _mapWidth = mapWidth;
            _pageWidth = widthInPixel;
            CalcScale(dpi);
            OnViewChanged();
        }

        /// <summary>
        /// Gets or sets the number of ticks
        /// </summary>
        public int NumTicks
        {
            get { return _numTics; }
            set
            {
                if (value < 1) value = 1;
                _numTics = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the labeling for the bar
        /// </summary>
        public ScaleBarLabelText ScaleText
        {
            get { return _barLabelText; }
            set
            {
                _barLabelText = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the width of the scale bar
        /// </summary>
        public int Width { get; set; }

        private string ScaleBarText(double scale, ScaleBarLabelText scaleText)
        {
            switch (scaleText)
            {
                case ScaleBarLabelText.NoText:
                    return string.Empty;
                case ScaleBarLabelText.JustUnits:
                    return _barUnitName;
                default:
                    var precision = 0;

                    //set the precision. Keep the first 5 (ScalePrecisionDigits) digits. 
                    if (scale > 0)
                    {
                        var magnitude = (int) (Math.Log10(scale));
                        precision = ScalePrecisionDigits - magnitude;
                        if (precision < 0)
                            precision = 0;
                        if (magnitude >= 2)
                            //don't show the precision if the scale is less than than 1:100 (e.g. 1:1000)
                            precision = 0;

                    }
                    var format = string.Format("F{0}", precision);

                    scale = FormatRealScale(scale);
                    return "1:" + scale.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets or sets the bar style
        /// </summary>
        public ScaleBarStyle BarStyle
        {
            get { return _barStyle; }
            set
            {
                _barStyle = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the minimum margin on the left of the bar
        /// </summary>
        public int MarginLeft
        {
            get { return _marginLeft; }
            set
            {
                _marginLeft = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the minimum margin on the right of the bar
        /// </summary>
        public int MarginRight
        {
            get { return _marginRight; }
            set
            {
                _marginRight = value;
                OnViewChanged();
                Dirty = true;
            }
        }

        #region EventInvokers

        private void OnViewChanged()
        {
        }

        /// <summary>
        /// Gets or (private) sets whether the display settings for the scale bar have been changed
        /// </summary>
        public bool Dirty { get; private set; }

        #endregion


        #region Private static helpers

        private class UnitInfo
        {
            public readonly int Unit;
            
            public readonly double ToMeter;
            public readonly string Name;
            public readonly string Abbreviation;

            public UnitInfo(int unit, double toMeter, string name, string abbreviation)
            {
                Unit = unit;
                ToMeter = toMeter;
                Name = name;
                Abbreviation = abbreviation;
            }
        }

        //for multipliers ranging from .00001 to 10000000000
        //  Candidates are 1, 2, 2.5, and 5 * multiplier
        //    First candidate >starting interval is new interval 
        //  next candidate
        //next multiplier
        private static double GetRoundIncrement(double startValue)
        {
            int nPower; //power of 10. Range of -5 to 10 gives huge scale range.
            double candidate = 0d; //Candidate value for new interval.
            for (nPower = PowerRangeMin; nPower <= PowerRangeMax; nPower++)
            {
                double multiplier = Math.Pow(10, nPower); //Mulitiplier, =10^exp, to apply to nice numbers.
                for (int i = 0; i < NiceNumber; i++)
                {
                    candidate = NiceNumberArray[i]*multiplier;
                    if (candidate > startValue)
                        return candidate;
                }
            }
            return candidate; //return the maximum
        }

        //Calculate the Representative Fraction Scale for non Lat/Long map.
        //MapUnitFactor is the factor the unit used on the map
        private double CalcRepresentativeFractionScale(double widthMap, double widthPage, double mapUnitFactor, int dpi)
        {
            int nPxlPerInch = dpi;
            double ratio;

            if (widthPage <= 0) return 0.0;
            //convert map width to meters
            double mapWidth = widthMap*mapUnitFactor;
            //convert page width to meters.
            try
            {
                double pageWidth = widthPage/nPxlPerInch*GeoSpatialMath.MetersPerInch;
                ratio = Math.Abs(mapWidth/pageWidth);
            }
            catch
            {
                ratio = 0.0;
            }
            return ratio;
        }

        /// <summary>
        /// Calculate the representative fraction scale for lat/long map.
        /// </summary>
        /// <param name="lon1">The first longitue value</param>
        /// <param name="lon2">The second longitue value</param>
        /// <param name="lat">The common latitued value for <paramref name="lon1"/> and <paramref name="lon2"/></param>
        /// <param name="widthPage">The width of the display area</param>
        /// <param name="dpi">The "dots per inch" of the display</param>
        /// <returns>The representative fraction</returns>
        private double CalcRepresentativeFractionScaleD(double lon1, double lon2, double lat, double widthPage, int dpi)
        {
            double distance = GeoSpatialMath.GreatCircleDistance(lon1, lon2, lat);
            double r = CalcRepresentativeFractionScale(distance, widthPage, 1, dpi);
            return r;
        }

        private void CalcBarScale(int dpi, int widthOnDevice, int numTics, double mapScale, double fBarUnitFactor,
                                  out int pixelsPerTic, out double scaleBarUnitsPerTic)
        {
            int minPixelsPerTic = widthOnDevice / (numTics * 2);
            double barScale = mapScale/fBarUnitFactor;
            int pixelsPerInch = dpi;
            double barUnitsPerPixel = barScale*GeoSpatialMath.MetersPerInch/pixelsPerInch;

            //calculate the result
            scaleBarUnitsPerTic = minPixelsPerTic*barUnitsPerPixel;
            scaleBarUnitsPerTic = GetRoundIncrement(scaleBarUnitsPerTic);
            pixelsPerTic = (int) (scaleBarUnitsPerTic/barUnitsPerPixel);
        }

        ///<summary>
        /// Keep only 5 (ScalePrecisionDigits) digits of precision for the scale and return the scale after formatted.
        ///</summary>
        private static double FormatRealScale(double scale)
        {
            double roundedScale;

            var rfMagnitude = (int) (Math.Log10(scale));
            var factor = Math.Pow(10, rfMagnitude - ScalePrecisionDigits);
            if (Math.Abs(factor - 0) > 1e6)
                roundedScale = (int) ((scale/factor) + 0.5)*factor;
            else
                roundedScale = (int) (scale + 0.5);
            return roundedScale;
        }

        ///<summary>
        /// Return the precision for the segment text of the scale bar.
        ///</summary>
        private static int PrecitionOfSegmentText(double value)
        {
            int precision = 0;
            if (value < 1 && value > 0)
            {
                precision = (int) Math.Ceiling(Math.Abs(Math.Log10(value)));
                double f = value*Math.Pow(10, precision);
                if (f - (int) f > 0.01)
                    precision++;
            }
            return precision;
        }

        static private int MeasureDisplayStringWidthExact(Graphics graphics, string text,
                                            Font font)
        {
            var ranges = new[] { new CharacterRange(0, text.Length) };
            var format = new StringFormat();
            format.SetMeasurableCharacterRanges(ranges);

            var rect = new RectangleF(0, 0, 1000, 1000);

            var regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int)(rect.Right + 1.0f);
        }

        #endregion
    }
}