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
using System.Collections.ObjectModel;
using System.Drawing;
using SharpMap.Geometries;
using Point = SharpMap.Geometries.Point;

namespace SharpMap.Rendering.Symbolizer    
{
    /// <summary>
    /// ListPointSymbolizer class
    /// </summary>
    [Serializable]
    public class ListPointSymbolizer : Collection<PointSymbolizer>, IPointSymbolizer
    {
        private Size _size;

        #region Collection<T> overrides
        protected override void ClearItems()
        {
            base.ClearItems();
            _size = new Size();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            _size = new Size();
        }

        protected override void InsertItem(int index, PointSymbolizer item)
        {
            base.InsertItem(index, item);
            _size = new Size();
        }

        protected override void SetItem(int index, PointSymbolizer item)
        {
            base.SetItem(index, item);
            _size = new Size();
        }
        #endregion

        /// <summary>
        /// Method to render the Point to the <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="map">The map object</param>
        /// <param name="points">Location where to render the Symbol</param>
        /// <param name="g">The graphics object to use.</param>
        public void Render(Map map, IPuntal points, Graphics g)
        {
            foreach (var pointSymbolizer in Items)
                pointSymbolizer.Render(map, points, g);
        }

        /// <summary>
        /// Offset of the point from the point
        /// </summary>
        public PointF Offset
        {
            get { return PointF.Empty; }
            set { }
        }

        /// <summary>
        /// Rotation of the symbol
        /// </summary>
        public float Rotation
        {
            get { return 0f; }
            set { }
        }

        public Size Size
        {
            get
            {
                if (!_size.IsEmpty)
                    foreach (PointSymbolizer pointSymbolizer in Items)
                    {
                        var scale = pointSymbolizer.Scale;
                        var size = pointSymbolizer.Size;
                        var width = (int)Math.Max(_size.Width, scale * size.Width);
                        var height = (int)Math.Max(_size.Height, scale * size.Height);
                        _size = new Size(width, height);
                    }
                return _size;
            }
            set
            {
            }
        }

        public float  Scale
        {
            get
            {
                return 1;
            }
            set { }
        }


        public void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
        }

        public void Symbolize(Graphics g, Map map)
        {
        }

        public void End(Graphics g, Map map)
        {
        }
    }
}