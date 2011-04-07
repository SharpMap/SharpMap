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

using System.Drawing;
using System.Reflection;

namespace SharpMap.Styles
{
    /// <summary>
    /// Defines a style used for rendering vector data
    /// </summary>
    public class VectorStyle : Style
    {
        /// <summary>
        /// Default Symbol
        /// </summary>
        public static readonly Image DefaultSymbol;

        /// <summary>
        /// Static constructor
        /// </summary>
        static VectorStyle()
        {
            System.IO.Stream rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png");
            if (rs != null)
                DefaultSymbol = Image.FromStream(rs);
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
            Fill = Brushes.Black;
            EnableOutline = false;
            SymbolScale = 1f;
            PointColor = Brushes.Red;
            PointSize = 10f;
            LineOffset = 0;
        }

        #region Properties

        private PointF _symbolOffset;
        private float _symbolRotation;
        private float _symbolScale;
        private float _PointSize;

        private Brush _PointBrush = null;

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
            get { return _PointBrush; }
            set { _PointBrush = value; }
        }

        /// <summary>
        /// Size for Point geometries (if drawn with PointColor), will not have affect for Points drawn with Symbol
        /// </summary>
        public float PointSize
        {
            get { return _PointSize; }
            set { _PointSize= value; }
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
        /// A positive value offsets the line to the right
        /// A negative value offsets to the left
        /// </summary>
        public float LineOffset
        {
            get { return _lineOffset; }
            set { _lineOffset = value; }
        }

        #endregion
    }
}