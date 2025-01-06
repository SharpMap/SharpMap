using NetTopologySuite.Geometries;
using SharpMap.Utilities;

namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// An interface for factories that can create <see cref="ICoordinateSystem"/> spatial reference objects
    /// </summary>
    public interface ICoordinateSystemFactory    
    {
        /// <summary>
        /// Creates a spatial reference object given its Well-known text representation.
        /// </summary>
        /// <param name="wkt">The Well-known text representation for the spatial reference</param>
        /// <returns>The resulting spatial reference object</returns>
        ICoordinateSystem CreateFromWkt(string wkt);
    }
}
