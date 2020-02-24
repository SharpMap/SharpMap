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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using GeoAPI.Geometries;
using SharpMap.Utilities;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Base class for all possible Point symbolizers
    /// </summary>
    [Serializable]
    public abstract class PointSymbolizer : BaseSymbolizer, IPointSymbolizerEx
    {
        private float _scale = 1f;

        [NonSerialized]
        protected RectangleF _bounds;

        /// <summary>
        /// Offset of the point from the point
        /// </summary>
        public PointF Offset { get; set; }

        /// <summary>
        /// Rotation of the symbol
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the Size of the symbol
        /// <para>
        /// Implementations may ignore the setter, the getter must return a <see cref="Size"/> with positive width and height values.
        /// </para>
        /// </summary>
        public abstract Size Size
        {
            get; set;
        }


        /// <summary>
        /// Gets or sets the scale 
        /// </summary>
        public virtual float Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (value <= 0)
                    return;
                _scale = value;
            }
        }

        private SizeF GetOffset()
        {
            var size = Size;
            var result = new SizeF(Offset.X - Scale * (size.Width * 0.5f), Offset.Y - Scale * (size.Height * 0.5f));
            return result;
        }



        /// <summary>
        /// Function to render the symbol
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="point">The point to symbolize</param>
        /// <param name="g">The graphics object</param>
        protected void RenderPoint(MapViewport map, Coordinate point, Graphics g)
        {
            if (point == null)
                return;


            PointF pp = map.WorldToImage(point);

            if (Rotation != 0f && !Single.IsNaN(Rotation))
            {
                SizeF offset = GetOffset();

                using (var old = g.Transform.Clone())
                using (var t = g.Transform)
                {
                    PointF rotationCenter = pp;
                    t.RotateAt(Rotation, rotationCenter);
                    t.Translate(offset.Width + 1, offset.Height + 1);

                    g.Transform = t;

                    OnRenderInternal(pp, g);

                    using (var rev = new System.Drawing.Drawing2D.Matrix())
                    {
                        rev.RotateAt(Rotation, rotationCenter);
                        rev.Translate(offset.Width + 1, offset.Height + 1);
                        var pts = new[]
                        {
                            new PointF(_bounds.Left, _bounds.Top),
                            new PointF(_bounds.Right, _bounds.Top),
                            new PointF(_bounds.Right, _bounds.Bottom),
                            new PointF(_bounds.Left, _bounds.Bottom),
                        };
                        rev.TransformPoints(pts);

                        var minX = Math.Min(pts[0].X, Math.Min(pts[1].X, Math.Min(pts[2].X, pts[3].X)));
                        var maxX = Math.Max(pts[0].X, Math.Max(pts[1].X, Math.Max(pts[2].X, pts[3].X))); 
                        var minY = Math.Min(pts[0].Y, Math.Min(pts[1].Y, Math.Min(pts[2].Y, pts[3].Y))); 
                        var maxY = Math.Max(pts[0].Y, Math.Max(pts[1].Y, Math.Max(pts[2].Y, pts[3].Y)));
                        _bounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);    
                    }
                   
                    g.Transform = old;    
                }
            }
            else
            {
                pp = PointF.Add(pp, GetOffset());
                OnRenderInternal(pp, g);
            }
        }

        /// <summary>
        /// Function that does the actual rendering
        /// </summary>
        /// <param name="pt">The point</param>
        /// <param name="g">The graphics object</param>
        internal abstract void OnRenderInternal(PointF pt, Graphics g);

        /// <summary>
        /// Utility function to transform any <see cref="IPointSymbolizer"/> into an unscaled <see cref="RasterPointSymbolizer"/>. This may bring performance benefits.
        /// </summary>
        /// <returns></returns>
        public virtual IPointSymbolizer ToRasterPointSymbolizer()
        {
            var bitmap = new Bitmap(Size.Width, Size.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                OnRenderInternal(new PointF(Size.Width * 0.5f, Size.Height * 0.5f), g);
            }

            return new RasterPointSymbolizer
            {
                Offset = Offset,
                Rotation = Rotation,
                Scale = Scale,
                //ImageAttributes = new ImageAttributes(),
                Symbol = bitmap
            };
        }

        /// <summary>
        /// Function to render the geometry
        /// </summary>
        /// <param name="map">The map object, mainly needed for transformation purposes.</param>
        /// <param name="geometry">The geometry to symbolize.</param>
        /// <param name="graphics">The graphics object to use.</param>
        public void Render(MapViewport map, IPuntal geometry, Graphics graphics)
        {
            var mp = geometry as IMultiPoint;
            if (mp != null)
            {
                foreach (var point in mp.Coordinates)
                    RenderPoint(map, point, graphics);
                return;
            }
            RenderPoint(map, ((IPoint)geometry).Coordinate, graphics);
            
        }
        
        public RectangleF Bounds => _bounds;
    }
}
