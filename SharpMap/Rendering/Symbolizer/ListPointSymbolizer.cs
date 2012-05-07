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
using GeoAPI.Geometries;
using SharpMap.Base;
using Point = GeoAPI.Geometries.IPoint;

namespace SharpMap.Rendering.Symbolizer    
{
    /// <summary>
    /// ListPointSymbolizer class
    /// </summary>
    [Serializable]
    public class ListPointSymbolizer : Collection<PointSymbolizer>, IPointSymbolizer, IDisposableEx
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

        #region Implementation of ICloneable

        /// <summary>
        /// Erstellt ein neues Objekt, das eine Kopie der aktuellen Instanz darstellt.
        /// </summary>
        /// <returns>
        /// Ein neues Objekt, das eine Kopie dieser Instanz darstellt.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            var res = new ListPointSymbolizer();
            foreach (var pointSymbolizer in Items)
                res.Add((PointSymbolizer)pointSymbolizer.Clone());
            return res;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                foreach (IDisposable pointSymbolizer in Items)
                {
                    pointSymbolizer.Dispose();
                }
                ClearItems();
            }
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Implementation of IDisposableEx

        /// <summary>
        /// Gets whether this object was already disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion
    }
}