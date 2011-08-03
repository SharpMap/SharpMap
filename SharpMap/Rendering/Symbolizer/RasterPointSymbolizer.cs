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

        private ImageAttributes _imageAttributes;

        /// <summary>
        /// Gets or sets the symbol
        /// </summary>
        public Image Symbol { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ImageAttributes"/> for rendering the <see cref="Symbol"/>
        /// </summary>
        public ImageAttributes ImageAttributes
        {
            get
            {
                return _imageAttributes;
            }
            set
            {
                _imageAttributes = value;
                //if (_imageAttributes)
            }
        }

        public override Size Size
        {
            get
            {
                
                var size = Symbol == null ? DefaultSymbol.Size : Symbol.Size;
                return new Size((int)(Scale * size.Width), (int)(Scale * size.Height));
            }
            set
            {
            }
        }

        internal override void OnRenderInternal(PointF pt, Graphics g)
        {
            Image symbol = Symbol ?? DefaultSymbol;

            if (ImageAttributes == null)
            {
                if (Scale == 1f)
                {
                    lock (symbol)
                    {
                        g.DrawImageUnscaled(symbol, (int)(pt.X), (int)(pt.Y));
                    }
                }
                else
                {
                    float width = symbol.Width * Scale;
                    float height = symbol.Height * Scale;
                    lock (symbol)
                    {
                        g.DrawImage(
                            symbol,
                            (int)pt.X,
                            (int)pt.Y,
                            width,
                            height);
                    }
                }
            }
            else
            {
                float width = symbol.Width * Scale;
                float height = symbol.Height * Scale;
                int x = (int)(pt.X);
                int y = (int)(pt.Y);
                g.DrawImage(
                    symbol,
                    new Rectangle(x, y, (int)width, (int)height),
                    0,
                    0,
                    symbol.Width,
                    symbol.Height,
                    GraphicsUnit.Pixel,
                    ImageAttributes);
            }
        }
    }
}