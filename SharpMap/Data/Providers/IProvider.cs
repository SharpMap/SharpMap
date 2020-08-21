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

using System;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;
using IGeometry = GeoAPI.Geometries.IGeometry;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Interface for data providers that have an uint key
    /// </summary>
    public interface IProvider : IProvider<uint>
    { }

    /// <summary>
    /// Interface for data provider implementations with a <see cref="Guid"/> as unique identifier
    /// </summary>
    public interface IGuidProvider : IProvider<Guid>
    { }

    /// <summary>
    /// Interface for data providers
    /// </summary>
    public interface IProvider<TOid> : IBaseProvider where TOid: IComparable<TOid>
    {
        /// <summary>
        /// Returns all objects whose <see cref="GeoAPI.Geometries.Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their <see cref="GeoAPI.Geometries.Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
        Collection<TOid> GetObjectIDsInView(Envelope bbox);

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        IGeometry GetGeometryByID(TOid oid);

        /// <summary>
        /// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on a RowID
        /// </summary>
        /// <param name="rowId">The id of the row.</param>
        /// <returns>datarow</returns>
        FeatureDataRow GetFeature(TOid rowId);
    }
}
