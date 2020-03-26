// Copyright 2011 - Felix Obermaier (www.ivv-aachen.de)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace SharpMap.Rendering.Symbolizer
{
    ///<summary>
    /// 
    ///</summary>
    [Serializable]
    public class RasterPointSymbolizer : PointSymbolizer
    {
        private static readonly Bitmap DefaultSymbol =
            (Bitmap)
            Image.FromStream(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png"));

        [NonSerialized]
        private ImageAttributes _imageAttributes;

        private float _transparency = 0f;

        private Color _symbolColor = Color.Empty;
        private Color _remapColor = Color.Empty;

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (_imageAttributes != null)
            {
                _imageAttributes.Dispose();
                _imageAttributes = null;
            }

            if (Symbol != null)
            {
                Symbol.Dispose();
                Symbol = null;
            }

            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Optional transparency in range 0 (opaque) to 1 (fully transparent).
        /// </summary>
        public float Transparency
        {
            get { return _transparency; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("Require value from 0 (opaque) to 1 (fully transparent)");
                _transparency = value;
                ConstructImageAttributes();
            }
        }

        /// <summary>
        /// Optional colour to replace the RemapColor pixels in Symbol. 
        /// If Transparency is also specified, transparency will replace SymbolColor.Alpha.
        /// </summary>
        public Color SymbolColor
        {
            get { return _symbolColor; }
            set
            {
                _symbolColor = value;
                ConstructImageAttributes();
            }
        }

        /// <summary>
        /// Optional colour to be replaced by SymbolColor during re-map.
        /// Pixels must have an exact match (including RemapColor.Alpha) to be re-mapped.
        /// </summary>
        public Color RemapColor
        {
            get { return _remapColor; }
            set
            {
                _remapColor = value;
                ConstructImageAttributes();
            }
        }

        /// <inheritdoc/>
        public override object Clone()
        {
            var res = (RasterPointSymbolizer)MemberwiseClone();
            res.Transparency = Transparency;
            res.Symbol = (Image)Symbol.Clone();
            res.SymbolColor = SymbolColor;
            res.RemapColor = RemapColor;

            return res;
        }

        /// <summary>
        /// Gets or sets the symbol
        /// </summary>
        public Image Symbol { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ImageAttributes"/> for rendering the <see cref="Symbol"/>
        /// </summary>
        internal ImageAttributes ImageAttributes
        {
            get
            {
                return _imageAttributes;
            }
        }

        /// <summary>
        /// Construct imageattribute based upon Transparency and/or Color Re-map
        /// </summary>
        private void ConstructImageAttributes()
        {
            if (_imageAttributes != null)
                _imageAttributes.Dispose();

            if (Transparency == 0 && (SymbolColor.ToArgb() == RemapColor.ToArgb()))
                return;

            _imageAttributes = new ImageAttributes();

            if (SymbolColor.ToArgb() != RemapColor.ToArgb())
            {
                var cm = new ColorMap[1];

                var a = SymbolColor.A;

                if (Transparency > 0)
                    a = (byte)(Math.Ceiling(255 * (1F - Transparency)));

                var nc = Color.FromArgb(a, SymbolColor);
                cm[0] = new ColorMap();
                cm[0].OldColor = RemapColor;
                cm[0].NewColor = nc;
                ImageAttributes.SetRemapTable(cm);
            }
            else
            {
                var cm = new ColorMatrix();
                cm.Matrix33 = 1F - _transparency;
                ImageAttributes.SetColorMatrix(cm);
            }
        }

        /// <summary>
        /// Gets or sets the Size of the symbol
        /// <para>
        /// Implementations may ignore the setter, the getter must return a <see cref="PointSymbolizer.Size"/> with positive width and height values.
        /// </para>
        /// </summary>
        public override Size Size
        {
            get
            {
                //return native size. Any required scaling is applied during render - see
                //PointSymbolizer::GetOffset and RasterPointSymbolizer::OnRenderInternal
                var size = Symbol == null ? DefaultSymbol.Size : Symbol.Size;
                return new Size((int)(size.Width), (int)(size.Height));
            }
            set
            {
            }
        }

        /// <summary>
        /// Function that does the actual rendering
        /// </summary>
        /// <param name="pt">The point</param>
        /// <param name="g">The graphics object</param>
        internal override void OnRenderInternal(PointF pt, Graphics g)
        {
            Image symbol = Symbol ?? DefaultSymbol;
            float width = symbol.Width * Scale;
            float height = symbol.Height * Scale;

            if (ImageAttributes == null)
            {
                if (Scale == 1f)
                {
                    lock (symbol)
                    {
                        g.DrawImageUnscaled(symbol, (int) (pt.X), (int) (pt.Y));
                    }
                }
                else
                {
                    lock (symbol)
                    {
                        g.DrawImage(symbol, (int) pt.X, (int) pt.Y, width, height);
                    }
                }
            }
            else
            {
                int x = (int) (pt.X);
                int y = (int) (pt.Y);
                g.DrawImage(
                    symbol,
                    new Rectangle(x, y, (int) width, (int) height),
                    0,
                    0,
                    symbol.Width,
                    symbol.Height,
                    GraphicsUnit.Pixel,
                    ImageAttributes);
            }

            CanvasArea = new RectangleF(pt.X, pt.Y, width, height);
        }
    }
}
