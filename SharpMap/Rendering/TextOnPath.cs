using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace SharpMap.Rendering
{
    /// <summary>
    /// Horizontal alignment options for texts on path
    /// </summary>
    public enum TextPathAlign
    {
        /// <summary>
        /// Aligned on the left
        /// </summary>
        Left = 0,
        /// <summary>
        /// Aligned in the middle
        /// </summary>
        Center = 1,
        /// <summary>
        /// Aligned on the right
        /// </summary>
        Right = 2
    }

    /// <summary>
    /// Vertical alignment option for texts on path
    /// </summary>
    public enum TextPathPosition
    {
        /// <summary>
        /// Above the path
        /// </summary>
        OverPath = 0,
        /// <summary>
        /// Center
        /// </summary>
        CenterPath = 1,
        /// <summary>
        /// Below the path
        /// </summary>
        UnderPath = 2
    }

    /// <summary>
    /// Extensions methods for text on path label rendering 
    /// </summary>
    public static class GraphicsExtension
    {
        /// <summary>
        /// Method to measure the length of a string
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to use</param>
        /// <param name="s">The string to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="brush">The <see cref="Brush"/> to use</param>
        /// <param name="graphicsPath">The <see cref="GraphicsPath"/> describing the </param>
        /// <returns>An array of <see cref="RectangleF"/>s</returns>
        public static RectangleF[] MeasureString(Graphics graphics, string s, Font font, Brush brush, GraphicsPath graphicsPath)
        {
            return MeasureString(graphics, s, font, brush, TextPathAlign.Left, TextPathPosition.CenterPath, 100, graphicsPath);
        }

        /// <summary>
        /// Method to measure the length of a string
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to use</param>
        /// <param name="s">The string to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="brush">The <see cref="Brush"/> to use</param>
        /// <param name="textPathAlign">The horizontal position on the <paramref name="graphicsPath"/></param>
        /// <param name="textPathPosition">The vertical position on the <paramref name="graphicsPath"/></param>
        /// <param name="graphicsPath">The <see cref="GraphicsPath"/> describing the </param>
        /// <returns>An array of <see cref="RectangleF"/>s</returns>
        public static RectangleF[] MeasureString(Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, GraphicsPath graphicsPath)
        {
            return MeasureString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, graphicsPath);
        }

        /// <summary>
        /// Method to draw <paramref name="s"/> on the provided <paramref name="graphicsPath"/>
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to use</param>
        /// <param name="s">The string to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="brush">The <see cref="Brush"/> to use</param>
        /// <param name="graphicsPath">The <see cref="GraphicsPath"/> describing the </param>
        public static void DrawString(Graphics graphics, string s, Font font, Brush brush, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, TextPathAlign.Left, TextPathPosition.CenterPath, 100, graphicsPath);
        }

        /// <summary>
        /// Method to draw <paramref name="s"/> on the provided <paramref name="graphicsPath"/>
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to use</param>
        /// <param name="s">The string to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="brush">The <see cref="Brush"/> to use</param>
        /// <param name="textPathAlign">The horizontal position on the <paramref name="graphicsPath"/></param>
        /// <param name="textPathPosition">The vertical position on the <paramref name="graphicsPath"/></param>
        /// <param name="graphicsPath">The <see cref="GraphicsPath"/> describing the </param>
        public static void DrawString(Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, graphicsPath);
        }

        /// <summary>
        /// Method to measure the length of a string along a <see cref="GraphicsPath"/>
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to use</param>
        /// <param name="s">The string to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="brush">The <see cref="Brush"/> to use</param>
        /// <param name="textPathAlign">The horizontal position on the <paramref name="graphicsPath"/></param>
        /// <param name="textPathPosition">The vertical position on the <paramref name="graphicsPath"/></param>
        /// <param name="letterSpace">A value controlling the spacing between letters</param>
        /// <param name="graphicsPath">The <see cref="GraphicsPath"/> describing the </param>
        /// <returns>An array of <see cref="RectangleF"/>s</returns>
        public static RectangleF[] MeasureString(Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, GraphicsPath graphicsPath)
        {
            var angles = new List<float>();
            var pointsText = new List<PointF>();
            var pointsTextUp = new List<Point>();
            return MeasureString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, 0, graphicsPath, ref angles, ref pointsText, ref pointsTextUp);
        }

        /// <summary>
        /// Method to measure the length of a string along a <see cref="GraphicsPath"/>
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to use</param>
        /// <param name="s">The string to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="brush">The <see cref="Brush"/> to use</param>
        /// <param name="textPathAlign">The horizontal position on the <paramref name="graphicsPath"/></param>
        /// <param name="textPathPosition">The vertical position on the <paramref name="graphicsPath"/></param>
        /// <param name="letterSpace">A value controlling the spacing between letters</param>
        /// <param name="graphicsPath">The <see cref="GraphicsPath"/> describing the </param>
        /// <returns>An array of <see cref="RectangleF"/>s</returns>
        public static void DrawString(Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, 0, graphicsPath,false);
        }

        /// <summary>
        /// Method to draw a string on a <see cref="GraphicsPath"/> of a string
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to use</param>
        /// <param name="s">The string to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="brush">The <see cref="Brush"/> to use</param>
        /// <param name="textPathAlign">The horizontal position on the <paramref name="graphicsPath"/></param>
        /// <param name="textPathPosition">The vertical position on the <paramref name="graphicsPath"/></param>
        /// <param name="letterSpace">A value controlling the spacing between letters</param>
        /// <param name="rotateDegree">A value controlling the rotation of <paramref name="s"/></param>
        /// <param name="graphicsPath">The <see cref="GraphicsPath"/> along which to render.</param>
        /// <param name="angles">A list of angle values (in degrees), one for each letter</param>
        /// <param name="pointsText">A list of positions, one for each letter</param>
        /// <param name="pointsUp">A list of points (don't know what for)</param>
        public static RectangleF[] MeasureString(Graphics graphics, string s, Font font, Brush brush,
                                                 TextPathAlign textPathAlign, TextPathPosition textPathPosition,
                                                 int letterSpace, float rotateDegree, GraphicsPath graphicsPath,
                                                 ref List<float> angles, ref List<PointF> pointsText,
                                                 ref List<Point> pointsUp)
        {
            var top = TextOnPath.TextOnPathInstance;
            top.Text = s;
            top.Font = font;
            top.FillColorTop = brush;
            top.TextPathPathPosition = textPathPosition;
            top.TextPathAlignTop = textPathAlign;
            top.PathDataTop = graphicsPath.PathData;
            top.LetterSpacePercentage = letterSpace;
            top.Graphics = graphics;
            top.GraphicsPath = graphicsPath;
            top.MeasureString = true;
            top.RotateDegree = rotateDegree;
            top.DrawTextOnPath();
            angles = top.Angles;
            pointsText = top.PointsText;
            pointsUp = top.PointsTextUp;
            return top.RegionList.ToArray();
        }

        /// <summary>
        /// Method to draw a string on a <see cref="GraphicsPath"/> of a string
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to use</param>
        /// <param name="s">The string to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="brush">The <see cref="Brush"/> to use</param>
        /// <param name="textPathAlign">The horizontal position on the <paramref name="graphicsPath"/></param>
        /// <param name="textPathPosition">The vertical position on the <paramref name="graphicsPath"/></param>
        /// <param name="letterSpace">A value controlling the spacing between letters</param>
        /// <param name="rotateDegree">A value controlling the rotation of <paramref name="s"/></param>
        /// <param name="graphicsPath">The <see cref="GraphicsPath"/> describing the </param>
        /// <param name="showPath">A value indicating if the <paramref name="graphicsPath"/> should be drawn, too.</param>
        public static void DrawString(Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, float rotateDegree, GraphicsPath graphicsPath, bool showPath)
        {
            var top = TextOnPath.TextOnPathInstance;
            
            top.Text = s;
            top.Font = font;
            top.FillColorTop = brush;
            top.TextPathPathPosition = textPathPosition;
            top.TextPathAlignTop = textPathAlign;
            top.PathDataTop = graphicsPath.PathData;
            top.LetterSpacePercentage = letterSpace;
            top.Graphics = graphics;
            top.GraphicsPath = graphicsPath;
            top.MeasureString = false;
            top.RotateDegree = rotateDegree;
            top.ShowPath = showPath;
            top.DrawTextOnPath();

        }
    }

    /// <summary>
    /// Text on path generator class
    /// </summary>
    public class TextOnPath
    {
        internal readonly static TextOnPath TextOnPathInstance = new TextOnPath();

        private PathData _pathdata;

        private bool _measureString;
        private Font _font;
        private Pen _colorHalo = new Pen(Color.Black,1);
        private Brush _fillBrush = new SolidBrush(Color.Black);
        private TextPathAlign _pathalign = TextPathAlign.Center;
        private double _letterspacepercentage = 1;
        private TextPathPosition _textPathPathPosition = TextPathPosition.CenterPath;

        //ToDo, this is a result, intermediate?
        private List<RectangleF> _regionList = new List<RectangleF>();
        private List<PointF> _pointText = new List<PointF>();
        private List<Point> _pointTextUp = new List<Point>();
        private readonly List<float> _angles = new List<float>();
        
        /// <summary>
        /// The last catched exception is stored here
        /// </summary>
        public Exception LastError;
        

        /// <summary>
        /// Gets or sets the <see cref="Graphics"/> object
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the <see cref="GraphicsPath"/> used to render text along
        /// </summary>
        public GraphicsPath GraphicsPath { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the string should be measured
        /// </summary>
        public bool MeasureString
        {
            get { return _measureString; }
            set { _measureString = value; }
        }
        

        /// <summary>
        /// Gets or sets a list of regions
        /// </summary>
        public List<RectangleF> RegionList
        {
            get { return _regionList; }
            set { _regionList = value; }
        }

        /// <summary>
        /// Gets or sets a list of <see cref="PointF"/>s
        /// </summary>
        public List<PointF> PointsText
        {
            get { return _pointText; }
            set { _pointText = value; }
        }
        
        /// <summary>
        /// Gets or sets a list of <see cref="PointF"/>s
        /// </summary>
        public List<Point> PointsTextUp
        {
            get { return _pointTextUp; }
            set { _pointTextUp = value; }
        }

        /// <summary>
        /// Gets or sets a list of angles (in radians?)
        /// </summary>
        public List<float> Angles
        {
            get { return _angles; }
        }

        /// <summary>
        /// Gets or sets a value indicating the rotation
        /// </summary>
        public float RotateDegree { get; set; } 

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        internal TextOnPath()
        {
            PathColorTop = Color.LightBlue;
        }

        /// <summary>
        /// Gets or sets a value indicating the vertical alignment
        /// </summary>
        public TextPathPosition TextPathPathPosition
        {
            get { return _textPathPathPosition; }
            set { _textPathPathPosition = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the path's data
        /// </summary>
        public PathData PathDataTop
        {
            get { return _pathdata; }
            set { _pathdata = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the text to render
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Font"/> to use for drawing the text
        /// </summary>
        public Font Font
        {
            get { return _font; }
            set { _font = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Pen"/> to use for halo'ing the text
        /// </summary>
        public Pen ColorHalo
        {
            get { return _colorHalo; }
            set { _colorHalo = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the <see cref="Brush"/> used to fill the text path
        /// </summary>
        public Brush FillColorTop
        {
            get { return _fillBrush; }
            set { _fillBrush = value; }
        }

        /// <summary>
        /// Get or sets a value indicating the horizontal alignment of the path
        /// </summary>
        public TextPathAlign TextPathAlignTop
        {
            get { return _pathalign; }
            set { _pathalign = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the color of the <see cref="GraphicsPath"/>
        /// </summary>
        public Color PathColorTop { get; set; }


        /// <summary>
        /// Gets or sets a value controlling the space between letters
        /// </summary>
        /// <remarks>The default value is <value>100</value></remarks>
        public int LetterSpacePercentage
        {
            get { return (int)(100 * _letterspacepercentage); }
            set { _letterspacepercentage = value/100d; }
        }

        /// <summary>
        /// Gets or sets a value indicating that the used path should be rendered as well
        /// </summary>
        public bool ShowPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathdata">The path data</param>
        /// <param name="text">The text</param>
        /// <param name="font">The font</param>
        /// <param name="haloPen">The halo pen</param>
        /// <param name="fillcolor">The brush to fill letters</param>
        /// <param name="letterspacepercentage">The </param>
        public void DrawTextOnPath(PathData pathdata, string text, Font font, Pen haloPen, Brush fillcolor, int letterspacepercentage)
        {

            _pathdata = pathdata;
            Text = text;
            _font = font;
            _colorHalo= haloPen;
            _fillBrush = fillcolor;
            _letterspacepercentage = letterspacepercentage / 100d;

            DrawTextOnPath();
        }

        /// <summary>
        /// Method to draw the text on the <see cref="GraphicsPath"/>
        /// </summary>
        public void DrawTextOnPath()
        {
            var points = new PointF[25001];
            var count = 0;
            var gp = new GraphicsPath(_pathdata.Points, _pathdata.Types) { FillMode = FillMode.Winding };
            _regionList.Clear();
            _angles.Clear();
            _pointText.Clear();
            _pointTextUp.Clear();
            gp.Flatten(null, 1);
            try
            {
                var tmpPoint = gp.PathPoints[0];
                int i;
                PointF[] tmpPoints;
                for (i = 0; i <= gp.PathPoints.Length - 2; i++)
                {
                    if (gp.PathTypes[i + 1] == (byte)PathPointType.Start | (gp.PathTypes[i] & (byte)PathPointType.CloseSubpath) == (byte)PathPointType.CloseSubpath)
                    {
                        tmpPoints = GetLinePoints(gp.PathPoints[i], tmpPoint, 1);
                        Array.ConstrainedCopy(tmpPoints, 0, points, count, tmpPoints.Length);
                        count += 1;
                        tmpPoint = gp.PathPoints[i + 1];
                    }
                    else
                    {
                        tmpPoints = GetLinePoints(gp.PathPoints[i], gp.PathPoints[i + 1], 1);
                        Array.ConstrainedCopy(tmpPoints, 0, points, count, tmpPoints.Length);
                        count += tmpPoints.Length - 1;

                    }
                }
                tmpPoints = new PointF[count];
                Array.Copy(points, tmpPoints, count);
                points = CleanPoints(tmpPoints);

                count = points.Length - 1;
                if (IsVisible(points, count))
                {
                    // if can show all letter
                    DrawText(Graphics, points, count);
                }
                gp.Dispose();
                //DrawText(points, count);
            }
            catch (Exception ex)
            {
                LastError = ex;


            }
        }
        /// <summary>
        /// Method to remove consecutive same/equal <see cref="PointF"/>s from the array
        /// </summary>
        /// <param name="points">An array of <see cref="PointF"/>s</param>
        /// <returns>An array of <see cref="PointF"/>s without repeated values.</returns>
        private static PointF[] CleanPoints(PointF[] points)
        {
            if (points == null)
                return null;
            if (points.Length == 0) 
                return new PointF[0];
            var tmpPoints = new List<PointF>(points.Length);
            tmpPoints.Add(points[0]);
            var lastIndex = 0;
            for (var i = 1; i <= points.Length - 1; i++)
            {
                if (!tmpPoints[lastIndex].Equals(points[i]))
                {
                    tmpPoints.Add(points[i]);
                    lastIndex++;
                }
            }

            return tmpPoints.ToArray();
        }

        /// <summary>
        /// Method to evaluate visibility
        /// </summary>
        /// <param name="points">An array of <see cref="PointF"/>s</param>
        /// <param name="maxPoints">The maximum number of points</param>
        /// <returns></returns>
        public bool IsVisible(PointF[] points, int maxPoints)
        {
            var result = true;
            var g = Graphics;           
            var count = 0;     
            var maxWidthText = default(double);
            int i;
            for (i = 0; i <= Text.Length - 1; i++)
            {
                maxWidthText += StringRegion(g, i) * LetterSpacePercentage / 100;
            }
            switch (_pathalign)
            {
                case TextPathAlign.Left:                  
                    count = 0;
                    break;
                case TextPathAlign.Center:
                    count = (int)((maxPoints - maxWidthText) / 2);                    
                    break;
                case TextPathAlign.Right:
                    count = (int)(maxPoints - maxWidthText - (double)StringRegion(g, Text.Length - 1) * LetterSpacePercentage / 100);                  
                    break;
            }
            var lStrWidth = (int)(StringRegion(g, 0) * LetterSpacePercentage / 100);
            if ((count + lStrWidth / 2) < 0)
            {
                count = -(lStrWidth / 2);
            }
            double currentWidthText = 0;
            for (int j =count+lStrWidth/2; j <= Text.Length - 1; j++)
            {
                currentWidthText += StringRegion(g, j) * LetterSpacePercentage / 100;
            }          
            if ((int)currentWidthText >= maxPoints)
            {
                result = false;
            }
            return result;

        }

        private void DrawText(Graphics g, PointF[] points, int maxPoints)
        {

            //GraphicsPath gp = new GraphicsPath(_pathdata.Points, _pathdata.Types) { FillMode = FillMode.Winding };
            //gp.Flatten();
            //gp.Dispose();
            //var g = _graphics;
            //GraphicsContainer graphicsContainer= g.BeginContainer();
            //g.TranslateTransform(_graphicsPath.GetBounds().X, _graphicsPath.GetBounds().Y);
            if (ShowPath)
            {
                var pen = new Pen(PathColorTop);
                foreach (var p in points)
                {
                    g.DrawEllipse(pen, p.X, p.Y, 1, 1);
                }
                pen.Dispose();
            }
            var count = 0;
            var point1 = default(PointF);
            var charStep = 0;
            var maxWidthText = default(double);
            int i;

            for (i = 0; i <= Text.Length - 1; i++)
            {
                maxWidthText += StringRegion(g, i) * LetterSpacePercentage / 100;
            }

            switch (_pathalign)
            {
                case TextPathAlign.Left:
                    point1 = points[0];
                    count = 0;
                    break;
                case TextPathAlign.Center:
                    count = (int)((maxPoints - maxWidthText) / 2);
                    point1 = count > 0 ? points[count] : points[0];

                    break;
                case TextPathAlign.Right:
                    count = (int)(maxPoints - maxWidthText - (double)StringRegion(g, Text.Length - 1) * LetterSpacePercentage / 100);
                    point1 = count > 0 ? points[count] : points[0];

                    break;
            }
            var lStrWidth = (int)(StringRegion(g, charStep) * LetterSpacePercentage / 100);
            if ((count + lStrWidth / 2) < 0)
            {
                count = -(lStrWidth / 2);
            }
            while (!(charStep > Text.Length - 1))
            {
                lStrWidth = (int)(StringRegion(g, charStep) * LetterSpacePercentage / 100);
                if ((count + lStrWidth / 2) >= 0 && (count + lStrWidth) <= maxPoints)
                {
                    count += lStrWidth;
                    var point2 = points[count];
                    //PointF point = points[count - lStrWidth / 2];
                    var point = new PointF((point2.X+point1.X)/2,(point2.Y+point1.Y)/2);
                    var angle = GetAngle(point1, point2);
                    DrawRotatedText(g, Text[charStep].ToString(CultureInfo.InvariantCulture), (float)angle, point);
                    point1 = points[count];
                }
                else
                {
                    count += lStrWidth;                  
                }
                charStep += 1;
            }

        }

        private RectangleF StringRegionValue(Graphics g, int textpos)
        {

            var measureString = Text.Substring(textpos, 1);
            var numChars = measureString.Length;
            var characterRanges = new CharacterRange[numChars + 1];
            var stringFormat = new StringFormat
            {
                Trimming = StringTrimming.None,
                FormatFlags =
                StringFormatFlags.NoClip | StringFormatFlags.NoWrap |
                StringFormatFlags.LineLimit
            };
            var size = g.MeasureString(Text, _font, LetterSpacePercentage);
            var layoutRect = new RectangleF(0f, 0f, size.Width, size.Height);
            characterRanges[0] = new CharacterRange(0, 1);
            stringFormat.FormatFlags = StringFormatFlags.NoClip;
            stringFormat.SetMeasurableCharacterRanges(characterRanges);
            stringFormat.Alignment = StringAlignment.Center;
            var stringRegions = g.MeasureCharacterRanges(Text.Substring(textpos), _font, layoutRect, stringFormat);
            return stringRegions[0].GetBounds(g);
        }

        private float StringRegion(Graphics g, int textpos)
        {
            return StringRegionValue(g, textpos).Width;
        }

        /// <summary>
        /// Method to compute the angle of a segment |<paramref name="point1"/>, <paramref name="point2"/>| compared to the horizontal line.
        /// </summary>
        /// <param name="point1">The 1st point of the segment</param>
        /// <param name="point2">The 2nd point of the segment</param>
        /// <returns>An angle in degrees</returns>
        private static double GetAngle(PointF point1, PointF point2)
        {
            const double rad2Deg = 180d/Math.PI;

            var c = Math.Sqrt(Math.Pow((point2.X - point1.X), 2) + Math.Pow((point2.Y - point1.Y), 2));
            if (c == 0d)
            {
                return 0;
            }

            var res = Math.Asin((point2.Y - point1.Y)/c)*rad2Deg;
            return point1.X > point2.X
                       ? res - 180
                       : res;
        }

        /// <summary>
        /// Method to draw <paramref name="text"/>, rotated by <paramref name="angle"/> around <paramref name="pointCenter"/>.
        /// </summary>
        /// <param name="gr">The <see cref="Graphics"/> object to use.</param>
        /// <param name="text">The text string</param>
        /// <param name="angle">The rotation angle</param>
        /// <param name="pointCenter">The center point around which to rotate</param>
        private void DrawRotatedText(Graphics gr, string text, float angle, PointF pointCenter)
        {
            angle -= RotateDegree;
            var stringFormat =  new StringFormat { Alignment = StringAlignment.Center };

            //gr.SmoothingMode = SmoothingMode.HighQuality;
            //gr.CompositingQuality = CompositingQuality.HighQuality;
            //gr.TextRenderingHint = TextRenderingHint.AntiAlias;
            using (var graphicsPath = new GraphicsPath())
            {
                var x = (int) pointCenter.X;
                var y = (int) pointCenter.Y;
                var pOrigin = new Point();
                switch (TextPathPathPosition)
                {
                    case TextPathPosition.OverPath:
                        pOrigin = new Point(x, (int) (y - _font.Size));
                        graphicsPath.AddString(text, _font.FontFamily, (int) _font.Style, _font.Size,
                                               new Point(x, (int) (y - _font.Size)), stringFormat);
                        break;
                    case TextPathPosition.CenterPath:
                        pOrigin = new Point(x, (int) (y - _font.Size/2));
                        graphicsPath.AddString(text, _font.FontFamily, (int) _font.Style, _font.Size,
                                               new Point(x, (int) (y - _font.Size/2)), stringFormat);
                        break;
                    case TextPathPosition.UnderPath:
                        pOrigin = new Point(x, y);
                        graphicsPath.AddString(text, _font.FontFamily, (int) _font.Style, _font.Size, new Point(x, y),
                                               stringFormat);
                        break;
                }


                var rotationMatrix = gr.Transform.Clone(); // new Matrix();
                rotationMatrix.RotateAt(angle, new PointF(x, y));
                graphicsPath.Transform(rotationMatrix);
                if (!_measureString)
                {
                    if (_colorHalo != null)
                    {
                        gr.DrawPath(_colorHalo, graphicsPath);
                    }
                    gr.FillPath(_fillBrush, graphicsPath);

                }
                else
                {
                    _regionList.Add(graphicsPath.GetBounds());
                    _angles.Add(angle);
                    _pointText.Add(pointCenter);
                    _pointTextUp.Add(pOrigin);
                }
            }
        }

        /// <summary>
        /// Metod to get
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="stepWitdth"></param>
        /// <returns></returns>
        public PointF[] GetLinePoints(PointF p1, PointF p2, int stepWitdth)
        {

            int lCount = 0;
            var tmpPoints = new PointF[10001];
            long ix;
            long iy;
            int dd;
            int id;
            int lStep = stepWitdth;

            p1.X = (int)p1.X;
            p1.Y = (int)p1.Y;
            p2.X = (int)p2.X;
            p2.Y = (int)p2.Y;
            long width = (long)(p2.X - p1.X);
            long height = (long)(p2.Y - p1.Y);
            long d = 0;

            if (width < 0)
            {
                width = -width;
                ix = -1;
            }
            else
            {
                ix = 1;
            }

            if (height < 0)
            {
                height = -height;
                iy = -1;
            }
            else
            {
                iy = 1;
            }

            if (width > height)
            {
                dd = (int)(width + width);
                id = (int)(height + height);

                do
                {
                    if (lStep == stepWitdth)
                    {
                        tmpPoints[lCount].X = p1.X;
                        tmpPoints[lCount].Y = p1.Y;
                        lCount += 1;
                    }
                    else
                    {
                        lStep = lStep + stepWitdth;
                    }
                    if ((int)p1.X == (int)p2.X) break;

                    p1.X = p1.X + ix;
                    d = d + id;

                    if (d > width)
                    {
                        p1.Y = p1.Y + iy;
                        d = d - dd;
                    }
                }

                while (true);
            }
            else
            {
                dd = (int)(height + height);
                id = (int)(width + width);

                do
                {
                    if (lStep == stepWitdth)
                    {
                        tmpPoints[lCount].X = p1.X;
                        tmpPoints[lCount].Y = p1.Y;
                        lCount += 1;
                    }
                    else
                    {
                        lStep = lStep + stepWitdth;
                    }
                    if ((int)p1.Y == (int)p2.Y) break;

                    p1.Y = p1.Y + iy;
                    d = d + id;

                    if (d > height)
                    {
                        p1.X = p1.X + ix;
                        d = d - dd;
                    }
                }
                while (true);
            }

            var tmpPoints2 = new PointF[lCount];

            Array.Copy(tmpPoints, tmpPoints2, lCount);

            return tmpPoints2;
        }
    }
}

