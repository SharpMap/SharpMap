using System.Collections.Generic;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Interface for relations origin -> destination
    /// </summary>
    public interface IRelationProvider : ILocationProvider
    {
        /// <summary>
        /// Function to return all relations.
        /// </summary>
        IEnumerable<KeyValuePair<KeyValuePair<ushort, IPoint>, KeyValuePair<ushort, IPoint>>> Relations(
            ushort? restrict);
    }
}