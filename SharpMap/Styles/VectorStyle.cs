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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using GeoAPI.Geometries;
using SharpMap.Rendering.Symbolizer;
using Common.Logging;

namespace SharpMap.Styles
{
    /// <summary>
    /// Defines a style used for rendering vector data
    /// </summary>
    [Serializable]
    public class VectorStyle : Style, ICloneable
    {
        static ILog logger = LogManager.GetLogger(typeof(VectorStyle));
        /// <summary>
        /// Default Symbol
        /// </summary>
        public static readonly Image DefaultSymbol;

        /// <summary>
        /// Static constructor
        /// </summary>
        static VectorStyle()
        {
            var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png");
            if (rs != null)
                DefaultSymbol = Image.FromStream(rs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public VectorStyle Clone()
        {
            VectorStyle vs;
            lock (this)
            {
                try
                {
                    vs = (VectorStyle)MemberwiseClone();// new VectorStyle();

                    if (_fillStyle != null)
                        vs._fillStyle = _fillStyle.Clone() as Brush;

                    if (_lineStyle != null)
                        vs._lineStyle = _lineStyle.Clone() as Pen;

                    if (_outlineStyle != null)
                        vs._outlineStyle = _outlineStyle.Clone() as Pen;

                    if (_pointBrush != null)
                        vs._pointBrush = _pointBrush.Clone() as Brush;

                    vs._symbol = (_symbol != null ? _symbol.Clone() as Image : null);
                    vs._symbolRotation = _symbolRotation;
                    vs._symbolScale = _symbolScale;
                    vs.PointSymbolizer = PointSymbolizer;
                    vs.LineSymbolizer = LineSymbolizer;
                    vs.PolygonSymbolizer = PolygonSymbolizer;
                }
                catch (Exception ee)
                {
                    logger.Error("Exception while creating cloned style", ee);
                    /* if we got an exception, set the style to null and return since we don't know what we got...*/
                    vs = null;
                }
            }
            return vs;
        }
        
        object ICloneable.Clone()
        {
            return Clone();
        }

        #region Privates

        private Brush _fillStyle;
        private Pen _lineStyle;
        private bool _outline;
        private Pen _outlineStyle;
        private Image _symbol;
        private float _lineOffset;

        #endregion

        /// <summary>
        /// Initializes a new VectorStyle and sets the default values
        /// </summary>
        /// <remarks>
        /// Default style values when initialized:<br/>
        /// *LineStyle: 1px solid black<br/>
        /// *FillStyle: Solid black<br/>
        /// *Outline: No Outline
        /// *Symbol: null-reference
        /// </remarks>
        public VectorStyle()
        {
            Outline = new Pen(Color.Black, 1);
            Line = new Pen(Color.Black, 1);
            Fill = new SolidBrush (Color.FromArgb(192, Color.Black));
            EnableOutline = false;
            SymbolScale = 1f;
            PointColor = new SolidBrush(Color.Red);
            PointSize = 10f;
            LineOffset = 0;
        }

        #region Properties

        private PointF _symbolOffset;
        private float _symbolRotation;
        private float _symbolScale;

        private float _pointSize;
        private Brush _pointBrush;

        /// <summary>
        /// Linestyle for line geometries
        /// </summary>
        public Pen Line
        {
            get { return _lineStyle; }
            set { _lineStyle = value; }
        }

        /// <summary>
        /// Outline style for line and polygon geometries
        /// </summary>
        public Pen Outline
        {
            get { return _outlineStyle; }
            set { _outlineStyle = value; }
        }

        /// <summary>
        /// Specified whether the objects are rendered with or without outlining
        /// </summary>
        public bool EnableOutline
        {
            get { return _outline; }
            set { _outline = value; }
        }

        /// <summary>
        /// Fillstyle for Polygon geometries
        /// </summary>
        public Brush Fill
        {
            get { return _fillStyle; }
            set { _fillStyle = value; }
        }

        /// <summary>
        /// Fillstyle for Point geometries (will be used if no Symbol is set)
        /// </summary>
        public Brush PointColor
        {
            get { return _pointBrush; }
            set { _pointBrush = value; }
        }

        /// <summary>
        /// Size for Point geometries (if drawn with PointColor), will not have affect for Points drawn with Symbol
        /// </summary>
        public float PointSize
        {
            get { return _pointSize; }
            set { _pointSize= value; }
        }

        /// <summary>
        /// Symbol used for rendering points
        /// </summary>
        public Image Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        /// <summary>
        /// Scale of the symbol (defaults to 1)
        /// </summary>
        /// <remarks>
        /// Setting the symbolscale to '2.0' doubles the size of the symbol, where a scale of 0.5 makes the scale half the size of the original image
        /// </remarks>
        public float SymbolScale
        {
            get { return _symbolScale; }
            set { _symbolScale = value; }
        }

        /// <summary>
        /// Gets or sets the offset in pixels of the symbol.
        /// </summary>
        /// <remarks>
        /// The symbol offset is scaled with the <see cref="SymbolScale"/> property and refers to the offset af <see cref="SymbolScale"/>=1.0.
        /// </remarks>
        public PointF SymbolOffset
        {
            get { return _symbolOffset; }
            set { _symbolOffset = value; }
        }

        /// <summary>
        /// Gets or sets the rotation of the symbol in degrees (clockwise is positive)
        /// </summary>
        public float SymbolRotation
        {
            get { return _symbolRotation; }
            set { _symbolRotation = value; }
        }

        /// <summary>
        /// Gets or sets the offset (in pixel units) by which line will be offset from its original posision (perpendicular).
        /// </summary>
        /// <remarks>
        /// A positive value offsets the line to the right
        /// A negative value offsets to the left
        /// </remarks>
        public float LineOffset
        {
            get { return _lineOffset; }
            set { _lineOffset = value; }
        }

        /// <summary>
        /// Gets or sets the symbolizer for puntal geometries
        /// </summary>
        /// <remarks>Setting this property will lead to ignorance towards all <see cref="IPuntal"/> related style settings</remarks>
        public IPointSymbolizer PointSymbolizer { get; set; }

        /// <summary>
        /// Gets or sets the symbolizer for lineal geometries
        /// </summary>
        /// <remarks>Setting this property will lead to ignorance towards all <see cref="ILineal"/> related style settings</remarks>
        public ILineSymbolizer LineSymbolizer { get; set; }

        /// <summary>
        /// Gets or sets the symbolizer for polygonal geometries
        /// </summary>
        /// <remarks>Setting this property will lead to ignorance towards all <see cref="IPolygonal"/> related style settings</remarks>
        public IPolygonSymbolizer PolygonSymbolizer { get; set; }

        #endregion

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (IsDisposed)
                return;

            if (_fillStyle != null)
            {
                _fillStyle.Dispose();
                _fillStyle = null;
            }

            if (_lineStyle != null)
            {
                _lineStyle.Dispose();
                _lineStyle = null;
            }


            if (_outlineStyle != null)
            {
                _outlineStyle.Dispose();
                _outlineStyle = null;
            }

            if (_pointBrush != null)
            {
                _pointBrush.Dispose();
                _pointBrush = null;
            }


            if (_symbol != null)
            {
                _symbol.Dispose();
                _symbol = null;
            }
            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Utility function to create a random style
        /// </summary>
        /// <returns>A vector style</returns>
        public static VectorStyle CreateRandomStyle()
        {
            var res = new VectorStyle();
            RandomizePuntalStyle(res);
            RandomizeLinealStyle(res);
            RandomizePolygonalStyle(res);
            return res;
        }

        /// <summary>
        /// Factory method to create a random puntal style
        /// </summary>
        /// <returns>A puntal vector style</returns>
        public static VectorStyle CreateRandomPuntalStyle()
        {
            var res = new VectorStyle();
            ClearLinealStyle(res);
            ClearPolygonalStyle(res);
            RandomizePuntalStyle(res);
            return res;
        }

        /// <summary>
        /// Factory method to create a random puntal style
        /// </summary>
        /// <returns>A puntal vector style</returns>
        public static VectorStyle CreateRandomLinealStyle()
        {
            var res = new VectorStyle();
            ClearPuntalStyle(res);
            ClearPolygonalStyle(res);
            RandomizeLinealStyle(res);
            return res;
        }

        /// <summary>
        /// Factory method to create a random puntal style
        /// </summary>
        /// <returns>A puntal vector style</returns>
        public static VectorStyle CreateRandomPolygonalStyle()
        {
            var res = new VectorStyle();
            ClearPuntalStyle(res);
            ClearLinealStyle(res);
            RandomizePolygonalStyle(res);
            return res;
        }

        /// <summary>
        /// Utility function to modify <paramref name="style"/> in order to prevent drawing of any puntal components
        /// </summary>
        /// <param name="style">The style to modify</param>
        private static void ClearPuntalStyle(VectorStyle style)
        {
            style.PointColor = Brushes.Transparent;
            style.PointSize = 0f;
            style.Symbol = null;
            style.PointSymbolizer = null;
        }

        /// <summary>
        /// Utility function to modify <paramref name="style"/> in order to prevent drawing of any puntal components
        /// </summary>
        /// <param name="style">The style to modify</param>
        private static void ClearLinealStyle(VectorStyle style)
        {
            style.EnableOutline = false;
            style.Line = Pens.Transparent;
            style.Outline = Pens.Transparent;
        }

        /// <summary>
        /// Utility function to modify <paramref name="style"/> in order to prevent drawing of any puntal components
        /// </summary>
        /// <param name="style">The style to modify</param>
        private static void ClearPolygonalStyle(VectorStyle style)
        {
            style.EnableOutline = false;
            style.Line = Pens.Transparent;
            style.Outline = Pens.Transparent;
            style.Fill = Brushes.Transparent;
        }

        /// <summary>
        /// Utility function to randomize puntal settings
        /// </summary>
        /// <param name="res">The style to randomize</param>
        private static void RandomizePuntalStyle(VectorStyle res)
        {
            var rnd = new Random();
            switch (rnd.Next(2))
            {
                case 0:
                    res.Symbol = DefaultSymbol;
                    res.SymbolScale = 0.01f * new Random().Next(80, 200);
                    break;
                case 1:
                    res.Symbol = null;
                    res.PointColor = new SolidBrush(CreateRandomKnownColor(new Random().Next(67, 256)));
                    res.PointSize = 0.1f*rnd.Next(5, 20);
                    break;
            }
        }

        /// <summary>
        /// Utility function to randomize lineal settings
        /// </summary>
        /// <param name="res">The style to randomize</param>
        private static void RandomizeLinealStyle(VectorStyle res)
        {
            var rnd = new Random();
            
            res.Line = new Pen(CreateRandomKnownColor(rnd.Next(67,256)), rnd.Next(1, 3));
            res.EnableOutline = rnd.Next(0, 2) == 1;
            if (res.EnableOutline)
                res.Outline = new Pen(CreateRandomKnownColor(rnd.Next(67, 256)), rnd.Next((int) res.Line.Width, 5));
        }

        /// <summary>
        /// Utility function to randomize polygonal settings
        /// </summary>
        /// <param name="res"></param>
        private static void RandomizePolygonalStyle(VectorStyle res)
        {
            var rnd = new Random();
            switch (rnd.Next(3))
            {
                case 0:
                    res.Fill = new SolidBrush(CreateRandomKnownColor(rnd.Next(67, 256)));
                    break;
                case 1:
                    res.Fill = new HatchBrush((HatchStyle)rnd.Next(0, 53),
                        CreateRandomKnownColor(), CreateRandomKnownColor(rnd.Next(67,256)));
                    break;
                case 2:
                    var alpha = rnd.Next(67, 256);
                    res.Fill = new LinearGradientBrush(new Point(0, 0), new Point(rnd.Next(5, 10), rnd.Next(5, 10)),
                        CreateRandomKnownColor(alpha), CreateRandomKnownColor(alpha));
                    break;
            }
        }

        /// <summary>
        /// Factory method to create a random color from the <see cref="KnownColor"/>s enumeration
        /// </summary>
        /// <param name="alpha">An optional alpha value.</param>
        /// <returns></returns>
        public static Color CreateRandomKnownColor(int alpha = 255)
        {
            var kc = (KnownColor) new Random().Next(28, 168);
            return alpha == 255 
                ? Color.FromKnownColor(kc) 
                : Color.FromArgb(alpha, Color.FromKnownColor(kc));
        }
    }
}