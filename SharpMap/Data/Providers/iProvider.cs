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
    /// Interface for data providers
    /// </summary>
    public interface IProvider : IDisposable
    {
        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        string ConnectionID { get; }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        int SRID { get; set; }

        /// <summary>
        /// Gets the features within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="GeoAPI.Geometries.Envelope"/></returns>
        Collection<IGeometry> GetGeometriesInView(Envelope bbox);

        /// <summary>
        /// Returns all objects whose <see cref="GeoAPI.Geometries.Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their <see cref="GeoAPI.Geometries.Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
        Collection<uint> GetObjectIDsInView(Envelope bbox);

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        IGeometry GetGeometryByID(uint oid);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds);

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        int GetFeatureCount();

        /// <summary>
        /// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on a RowID
        /// </summary>
        /// <param name="rowId">The id of the row.</param>
        /// <returns>datarow</returns>
        FeatureDataRow GetFeature(uint rowId);

        /// <summary>
        /// <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>The 2d extent of the layer</returns>
        Envelope GetExtents();

        /// <summary>
        /// Opens the datasource
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the datasource
        /// </summary>
        void Close();
    }
}