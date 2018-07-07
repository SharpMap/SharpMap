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

using SharpMap.Data;
using SharpMap.Styles;

namespace SharpMap.Rendering.Thematics
{
    /// <summary>
    /// Interface for rendering a thematic layer
    /// </summary>
    /// <remarks>
    /// Implementations of this interface should consider implementing <see cref="T:System.ICloneable"/> 
    /// when they make use of <see cref="T:System.Drawing.Pen"/>, <see cref="T:System.Drawing.Brush"/> or likewise 
    /// objects of the <see cref="N:System.Drawing"/> namespace. Otherwise they are prone to GDI+ rendering exceptions.
    /// </remarks>
    public interface ITheme
    {
        /// <summary>
        /// Returns the style based on a feature
        /// </summary>
        /// <param name="feature">Set of attribute values to calculate the <see cref="IStyle"/> from</param>
        /// <returns>The style</returns>
        IStyle GetStyle(FeatureDataRow feature);
    }

    /// <summary>
    /// Extended interface for rendering a thematic layer based upon current scale or zoom and/or feature attributes
    /// </summary>
    /// <remarks>
    /// Implementations of this interface should consider implementing <see cref="T:System.ICloneable"/> 
    /// when they make use of <see cref="T:System.Drawing.Pen"/>, <see cref="T:System.Drawing.Brush"/> or likewise 
    /// objects of the <see cref="N:System.Drawing"/> namespace. Otherwise they are prone to GDI+ rendering exceptions.
    /// </remarks>
    public interface IThemeEx: ITheme
    {
        /// <summary>
        /// Calculates a style for a given <paramref name="feature"/> based on a given <paramref name="mapViewPort"/>. 
        /// </summary>
        /// <param name="mapViewPort">The viewport</param>
        /// <param name="feature">The feature</param>
        /// <returns>A style</returns>
        IStyle GetStyle(MapViewport mapViewPort, FeatureDataRow feature);
    }
}
