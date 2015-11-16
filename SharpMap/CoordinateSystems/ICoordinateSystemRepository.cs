﻿using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems;

namespace GeoAPI
{
    /// <summary>
   /// An interface for classes that offer access to coordinate system creating facillities.
    /// </summary>
    //public interface ICoordinateSystemServicesRepository : ICoordinateSystemServices, IEnumerable<KeyValuePair<int, ICoordinateSystem>>
    public interface ICoordinateSystemRepository : IEnumerable<KeyValuePair<int, ICoordinateSystem>>
    {
        /// <summary>
        /// Gets a value indicating the number of unique coordinate systems in the repository
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating that this coordinate system repository is readonly
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Method to add <paramref name="coordinateSystem"/> to the service and register it with the <paramref name="srid"/> value.
        /// </summary>
        /// <param name="srid">The identifier for the <paramref name="coordinateSystem"/> in the store.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        void AddCoordinateSystem(int srid, ICoordinateSystem coordinateSystem);

        /// <summary>
        /// Method to remove all coordinate systems from the service
        /// </summary>
        void Clear();

        /// <summary>
        /// Method to remove a coordinate system form the service by its <paramref name="srid"/> identifier
        /// </summary>
        /// <param name="srid">The identifier of the coordinate system to remove</param>
        /// <returns><value>true</value> if the coordinate system was removed successfully, otherwise <value>false</value></returns>
        bool RemoveCoordinateSystem(int srid);
    }
}