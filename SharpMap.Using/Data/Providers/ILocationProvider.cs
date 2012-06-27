using System.Collections.Generic;
using GeoPoint = GeoAPI.Geometries.IPoint;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Interface for location providers
    /// </summary>
    public interface ILocationProvider : IEnumerable<KeyValuePair<ushort, GeoPoint>>
    {
        /// <summary>
        /// Indexer to a specific location
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        GeoPoint this[ushort id] { get; set; }

        /// <summary>
        /// A readonly collection of the locations
        /// </summary>
        ICollection<GeoPoint> Locations { get; }

        /// <summary>
        /// Gets the number of locations
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a location to the provider
        /// </summary>
        /// <param name="id">The new id of the zone</param>
        /// <param name="point">The location of the zone</param>
        void Add(ushort id, GeoPoint point);

        /// <summary>
        /// Remove a location from the provider
        /// </summary>
        /// <param name="id">The new id of the zone</param>
        void Remove(ushort id);
    }
}