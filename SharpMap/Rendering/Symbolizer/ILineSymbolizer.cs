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

using System.Drawing;
using GeoAPI.Geometries;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Interface for all classes that render lines
    /// </summary>
    public interface ILineSymbolizer : ISymbolizer<ILineal>
    {
        /*
        /// <summary>
        /// Method to render an ILineal to the <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="map">The map object</param>
        /// <param name="lineString">Linestring to symbolize</param>
        /// <param name="g">The graphics object to use.</param>
        void Render(Map map, ILineal lineString, Graphics g);

        /// <summary>
        /// Method to render the Point to the <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="map">The map object</param>
        /// <param name="multiLineString">MutliLinestring to symbolize</param>
        /// <param name="g">The graphics object to use.</param>
        void Render(Map map, MultiLineString multiLineString, Graphics g);
         */
    }
}