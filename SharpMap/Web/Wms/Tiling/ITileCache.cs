// Copyright 2007 - Paul den Dulk (Geodan)
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
using GeoAPI.Geometries;

namespace SharpMap.Web.Wms.Tiling
{
    /// <summary>
    /// Basic interface for a <see cref="ITileCache"/>
    /// </summary>
    public interface ITileCache
    {
        /// <summary>
        /// Method to add a tile to the cache.
        /// </summary>
        /// <param name="box">The bounding <paramref name="box"/> of the area covered by the <paramref name="tile"/>.</param>
        /// <param name="tile">The tile image</param>
        void AddTile(Envelope box, Bitmap tile);
        
        /// <summary>
        /// Function to retrieve a tile from the cache that covers the provided <paramref name="box"/>.
        /// </summary>
        /// <param name="box">The area that is to be covered by the tile</param>
        /// <returns></returns>
        Bitmap GetTile(Envelope box);

        /// <summary>
        /// Function to evaluate if the cache contains a tile that covers the provided <paramref name="box"/>.
        /// </summary>
        /// <param name="box">The area that is to be covered by the tile</param>
        /// <returns><value>true</value> if such a tile image is in the cache.</returns>
        bool ContainsTile(Envelope box);
    }
}