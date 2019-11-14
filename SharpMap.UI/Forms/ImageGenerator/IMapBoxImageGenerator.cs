/*
 * Copyright © 2018 - Felix Obermaier, Ingenieurgruppe IVV GmbH & Co. KG
 * 
 * This file is part of SharpMap.UI.
 *
 * SharpMap.UI is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * SharpMap.UI is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with SharpMap; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
 *
 */
using System;
using System.Drawing;
using GeoAPI.Geometries;

namespace SharpMap.Forms.ImageGenerator
{
    /// <summary>
    /// Interface for map image generators that can be used with <see cref="MapBox"/>
    /// </summary>
    public interface IMapBoxImageGenerator : IDisposable
    {
        /// <summary>
        /// Gets a value representing the complete rendered map image
        /// </summary>
        Image Image { get; }

        /// <summary>
        /// Gets the current image value as it is now
        /// </summary>
        Image ImageValue { get; }

        /// <summary>
        /// Gets the location and extent of the map image.
        /// </summary>
        /// <remarks>Implementation should always return a copy of the actual value</remarks>
        Envelope ImageEnvelope { get; }

        /// <summary>
        /// Gets a value indicating that this object is disposed
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Method to generate a new set of images
        /// </summary>
        void Generate();
    }
}
