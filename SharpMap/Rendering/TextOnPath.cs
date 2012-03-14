using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
namespace SharpMap.Rendering
{
    public enum TextPathAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }
    public enum TextPathPosition
    {
        OverPath = 0,
        CenterPath = 1,
        UnderPath = 2
    }
    public static class GraphicsExtension
    {
        private static readonly TextOnPath TEXT_ON_PATH = new TextOnPath();

        public static RectangleF[] MeasureString(Graphics graphics, string s, Font font, Brush brush, GraphicsPath graphicsPath)
        {
            return MeasureString(graphics, s, font, brush, TextPathAlign.Left, TextPathPosition.CenterPath, 100, graphicsPath);
        }

        public static RectangleF[] MeasureString( Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, GraphicsPath graphicsPath)
        {
            return MeasureString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, graphicsPath);
        }

        public static void DrawString( Graphics graphics, string s, Font font, Brush brush, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, TextPathAlign.Left, TextPathPosition.CenterPath, 100, graphicsPath);
        }

        public static void DrawString( Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, graphicsPath);
        }

        public static RectangleF[] MeasureString( Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, GraphicsPath graphicsPath)
        {
            List<float> angles = new List<float>();
            List<PointF> pointsText = new List<PointF>();
            List<Point> pointsTextUp = new List<Point>();
            return MeasureString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, 0, graphicsPath,ref angles, ref pointsText, ref pointsTextUp);
        }

        public static void DrawString( Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, 0, graphicsPath,false);
        }
        public static RectangleF[] MeasureString( Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, float rotateDegree, GraphicsPath graphicsPath,ref List<float> angles, ref List<PointF> pointsText, ref List<Point> pointsUp)
        {
            TEXT_ON_PATH.Text = s;
            TEXT_ON_PATH.Font = font;
            TEXT_ON_PATH.FillColorTop = brush;
            TEXT_ON_PATH.TextPathPathPosition = textPathPosition;
            TEXT_ON_PATH.TextPathAlignTop = textPathAlign;
            TEXT_ON_PATH.PathDataTop = graphicsPath.PathData;
            TEXT_ON_PATH.LetterSpacePercentage = letterSpace;
            TEXT_ON_PATH._graphics = graphics;
            TEXT_ON_PATH._graphicsPath = graphicsPath;
            TEXT_ON_PATH._measureString = true;
            TEXT_ON_PATH._rotateDegree = rotateDegree;
            TEXT_ON_PATH.DrawTextOnPath();
            angles = TEXT_ON_PATH._angles;
            pointsText = TEXT_ON_PATH._pointText;
            pointsUp = TEXT_ON_PATH._pointTextUp;
            return TEXT_ON_PATH._regionList.ToArray();
        }

        public static void DrawString( Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, float rotateDegree, GraphicsPath graphicsPath, bool showPath)
        {
            TEXT_ON_PATH.Text = s;
            TEXT_ON_PATH.Font = font;
            TEXT_ON_PATH.FillColorTop = brush;
            TEXT_ON_PATH.TextPathPathPosition = textPathPosition;
            TEXT_ON_PATH.TextPathAlignTop = textPathAlign;
            TEXT_ON_PATH.PathDataTop = graphicsPath.PathData;
            TEXT_ON_PATH.LetterSpacePercentage = letterSpace;
            TEXT_ON_PATH._graphics = graphics;
            TEXT_ON_PATH._graphicsPath = graphicsPath;
            TEXT_ON_PATH._measureString = false;
            TEXT_ON_PATH._rotateDegree = rotateDegree;
            TEXT_ON_PATH.ShowPath = showPath;
            TEXT_ON_PATH.DrawTextOnPath();

        }
    }
    public class TextOnPath
    {
        private PathData _pathdata;
        private string _text;
        private Font _font;
        private Pen _colorHalo = new Pen(Color.Black,1);
        private Brush _fillBrush = new SolidBrush(Color.Black);
        private TextPathAlign _pathalign = TextPathAlign.Center;
        private Color _pathColorTop = Color.LightBlue;
        private int _letterspacepercentage = 100;
        private TextPathPosition _textPathPathPosition = TextPathPosition.CenterPath;
        public Exception LastError;
        internal Graphics _graphics;
        internal GraphicsPath _graphicsPath;
        public Graphics Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
        }
        internal bool _measureString=false;
        public bool MeasureString
        {
            get { return _measureString; }
            set { _measureString = value; }
        }
        internal List<RectangleF> _regionList = new List<RectangleF>();
        public List<RectangleF> RegionList
        {
            get { return _regionList; }
            set { _regionList = value; }
        }
        internal List<PointF> _pointText = new List<PointF>();
        public List<PointF> PointsText
        {
            get { return _pointText; }
            set { _pointText = value; }
        }
        internal List<Point> _pointTextUp = new List<Point>();
        public List<Point> PointsTextUp
        {
            get { return _pointTextUp; }
            set { _pointTextUp = value; }
        }
        internal List<float> _angles = new List<float>();
        public List<float> Angles
        {
            get { return _angles; }
        }
        internal float _rotateDegree;
        private bool _showPath = false;
        public TextPathPosition TextPathPathPosition
        {
            get { return _textPathPathPosition; }
            set { _textPathPathPosition = value; }
        }
        public PathData PathDataTop
        {
            get { return _pathdata; }
            set { _pathdata = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public Font Font
        {
            get { return _font; }
            set { _font = value; }
        }

        public Pen ColorHalo
        {
            get { return _colorHalo; }
            set { _colorHalo = value; }
        }

        public Brush FillColorTop
        {
            get { return _fillBrush; }
            set { _fillBrush = value; }
        }

        public TextPathAlign TextPathAlignTop
        {
            get { return _pathalign; }
            set { _pathalign = value; }
        }
        public Color PathColorTop
        {
            get { return _pathColorTop; }
            set { _pathColorTop = value; }
        }


        public int LetterSpacePercentage
        {
            get { return _letterspacepercentage; }
            set { _letterspacepercentage = value; }
        }
        public bool ShowPath
        {
            get { return _showPath; }
            set { _showPath = value; }
        }
        public void DrawTextOnPath(PathData pathdata, string text, Font font, Pen color, Brush fillcolor, int letterspacepercentage)
        {

            _pathdata = pathdata;
            _text = text;
            _font = font;
            _colorHalo= color;
            _fillBrush = fillcolor;
            _letterspacepercentage = letterspacepercentage;

            DrawTextOnPath();
        }


        public void DrawTextOnPath()
        {
            PointF[] tmpPoints;
            PointF[] points = new PointF[25001];
            int count = 0;
            GraphicsPath gp = new GraphicsPath(_pathdata.Points, _pathdata.Types) { FillMode = FillMode.Winding };
            _regionList.Clear();
            _angles.Clear();
            _pointText.Clear();
            _pointTextUp.Clear();
            gp.Flatten(null, 1);
            try
            {
                PointF tmpPoint = gp.PathPoints[0];
                int i;
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
                if (isVisible(points, count) == true)
                {
                    // if can show all letter
                    DrawText(points, count);
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
        /// Clear same point
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static PointF[] CleanPoints(PointF[] points)
        {

            int i;
            PointF[] tmppoints = new PointF[points.Length + 1];
            PointF lastpoint = default(PointF);
            int count = 0;

            for (i = 0; i <= points.Length - 1; i++)
            {
                if (i == 0 | points[i].X != lastpoint.X | points[i].Y != lastpoint.Y)
                {
                    tmppoints[count] = points[i];
                    count += 1;
                }
                lastpoint = points[i];
            }


            points = new PointF[count];
            Array.Copy(tmppoints, points, count);

            return points;
        }
        public bool isVisible(PointF[] points, int maxPoints)
        {
            bool result = true;
            Graphics g = _graphics;           
            int count = 0;     
            double maxWidthText = default(double);
            int i;
            for (i = 0; i <= _text.Length - 1; i++)
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
                    count = (int)(maxPoints - maxWidthText - (double)StringRegion(g, _text.Length - 1) * LetterSpacePercentage / 100);                  
                    break;
            }
            int lStrWidth = (int)(StringRegion(g, 0) * LetterSpacePercentage / 100);
            if ((count + lStrWidth / 2) < 0)
            {
                count = -(lStrWidth / 2);
            }
            double currentWidthText = 0;
            for (int j =(int)(count+lStrWidth/2); j <= _text.Length - 1; j++)
            {
                currentWidthText += StringRegion(g, j) * LetterSpacePercentage / 100;
            }          
            if ((int)currentWidthText >= maxPoints)
            {
                result = false;
            }
            return result;

        }
        private void DrawText(PointF[] points, int maxPoints)
        {

            //GraphicsPath gp = new GraphicsPath(_pathdata.Points, _pathdata.Types) { FillMode = FillMode.Winding };
            //gp.Flatten();
            //gp.Dispose();
            Graphics g = _graphics;
            //GraphicsContainer graphicsContainer= g.BeginContainer();
            //g.TranslateTransform(_graphicsPath.GetBounds().X, _graphicsPath.GetBounds().Y);
            if (_showPath == true)
            {
                Pen pen = new Pen(_pathColorTop);
                foreach (PointF p in points)
                {
                    g.DrawEllipse(pen, p.X, p.Y, 1, 1);
                }
                pen.Dispose();
            }
            int count = 0;
            PointF point1 = default(PointF);
            int charStep = 0;
            double maxWidthText = default(double);
            int i;

            for (i = 0; i <= _text.Length - 1; i++)
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
                    if (count > 0)
                    {
                        point1 = points[count];
                    }
                    else
                    {
                        point1 = points[0];
                        //count = 0;
                    }

                    break;
                case TextPathAlign.Right:
                    count = (int)(maxPoints - maxWidthText - (double)StringRegion(g, _text.Length - 1) * LetterSpacePercentage / 100);
                    if (count > 0)
                    {
                        point1 = points[count];
                    }
                    else
                    {
                        point1 = points[0];
                        //count = 0;
                    }

                    break;
            }
            int lStrWidth = (int)(StringRegion(g, charStep) * LetterSpacePercentage / 100);
            if ((count + lStrWidth / 2) < 0)
            {
                count = -(lStrWidth / 2);
            }
            while (!(charStep > _text.Length - 1))
            {
                double angle = 0;
                lStrWidth = (int)(StringRegion(g, charStep) * LetterSpacePercentage / 100);
                if ((count + lStrWidth / 2) >= 0 && (count + lStrWidth) <= maxPoints)
                {
                    count += lStrWidth;
                    PointF point2 = points[count];
                    //PointF point = points[count - lStrWidth / 2];
                    PointF point = new PointF((point2.X+point1.X)/2,(point2.Y+point1.Y)/2);
                    angle = GetAngle(point1, point2);
                    DrawRotatedText(g, _text[charStep].ToString(), (float)angle, point);
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

            string measureString = _text.Substring(textpos, 1);
            int numChars = measureString.Length;
            CharacterRange[] characterRanges = new CharacterRange[numChars + 1];
            StringFormat stringFormat = new StringFormat
            {
                Trimming = StringTrimming.None,
                FormatFlags =
                StringFormatFlags.NoClip | StringFormatFlags.NoWrap |
                StringFormatFlags.LineLimit
            };
            SizeF size = g.MeasureString(_text, _font, LetterSpacePercentage);
            RectangleF layoutRect = new RectangleF(0f, 0f, size.Width, size.Height);
            characterRanges[0] = new CharacterRange(0, 1);
            stringFormat.FormatFlags = StringFormatFlags.NoClip;
            stringFormat.SetMeasurableCharacterRanges(characterRanges);
            stringFormat.Alignment = StringAlignment.Center;
            Region[] stringRegions = g.MeasureCharacterRanges(_text.Substring(textpos), _font, layoutRect, stringFormat);
            return stringRegions[0].GetBounds(g);
        }

        private float StringRegion(Graphics g, int textpos)
        {
            return StringRegionValue(g, textpos).Width;
        }

        private static double GetAngle(PointF point1, PointF point2)
        {

            double c = Math.Sqrt(Math.Pow((point2.X - point1.X), 2) + Math.Pow((point2.Y - point1.Y), 2));
            if (c == 0)
            {
                return 0;
            }
            if (point1.X > point2.X)
            {
                //We must change the side where the triangle is
                return Math.Asin((point1.Y - point2.Y) / c) * 180 / Math.PI - 180;
            }
            return Math.Asin((point2.Y - point1.Y) / c) * 180 / Math.PI;
            
            //return Math.Atan2(point1.Y - point2.Y, point1.X - point1.X) ;
            //if (point1.X > point2.X)
            //{
            //    return Math.Atan((point1.Y - point2.Y) / (point1.X - point2.X)) * (180 / Math.PI);
            //}
            //else
            //{
            //    return Math.Atan((point2.Y - point1.Y) / (point1.X - point2.X)) * (180 / Math.PI);
            //}    
        }
        private void DrawRotatedText(Graphics gr, string text, float angle, PointF pointCenter)
        {
            angle -= _rotateDegree;
            StringFormat stringFormat =  new StringFormat { Alignment = StringAlignment.Center };

            //gr.SmoothingMode = SmoothingMode.HighQuality;
            //gr.CompositingQuality = CompositingQuality.HighQuality;
            //gr.TextRenderingHint = TextRenderingHint.AntiAlias;
            GraphicsPath graphicsPath = new GraphicsPath();
            int x = (int)pointCenter.X;
            int y = (int)pointCenter.Y;
            Point pOrigin=new Point();
            switch (TextPathPathPosition)
            {
                case TextPathPosition.OverPath:
                    pOrigin = new Point(x, (int)(y - _font.Size));
                    graphicsPath.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, new Point(x, (int)(y - _font.Size)), stringFormat);
                    break;
                case TextPathPosition.CenterPath:
                    pOrigin = new Point(x, (int)(y - _font.Size / 2));
                    graphicsPath.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, new Point(x, (int)(y - _font.Size / 2)), stringFormat);
                    break;
                case TextPathPosition.UnderPath:
                    pOrigin = new Point(x, y);
                    graphicsPath.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, new Point(x, y), stringFormat);
                    break;
            }


            Matrix rotationMatrix = gr.Transform.Clone();// new Matrix();
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

            graphicsPath.Dispose();
        }



        public PointF[] GetLinePoints(PointF p1, PointF p2, int stepWitdth)
        {

            int lCount = 0;
            PointF[] tmpPoints = new PointF[10001];
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

            PointF[] tmpPoints2 = new PointF[lCount];

            Array.Copy(tmpPoints, tmpPoints2, lCount);

            return tmpPoints2;
        }
    }
}

