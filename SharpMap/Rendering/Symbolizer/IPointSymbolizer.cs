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
using Point = GeoAPI.Geometries.IPoint;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Interface for all classes that can symbolize Points
    /// </summary>
    public interface IPointSymbolizer : ISymbolizer<IPuntal>
    {
        /// <summary>
        /// Offset of the point from the point
        /// </summary>
        PointF Offset { get; set; }

        /// <summary>
        /// Rotation of the symbol
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the Size of the symbol
        /// <para>
        /// Implementations may ignore the setter, the getter must return a <see cref="Size"/> with positive width and height values.
        /// </para>
        /// </summary>
        Size Size { get; set; }

        /// <summary>
        /// Gets or sets the scale 
        /// </summary>
        float Scale { get; set; }

        //ToDo: this would be neat.
        ///// <summary>
        ///// Function to generate an image of the symbol as defined.
        ///// </summary>
        ///// <returns>An image</returns>
        //Image ToSymbol();

    }
}