using GeoAPI.Geometries;

namespace GeoAPI.Features
{
    /// <summary>
    /// Interface for all classes that can create features
    /// </summary>
    public interface IFeatureFactory
    {
        /// <summary>
        /// Gets the geometry factory to create features
        /// </summary>
        IGeometryFactory GeometryFactory { get; }

        /// <summary>
        /// Creates a new feature
        /// </summary>
        /// <returns></returns>
        IFeature Create(IGeometry geometry);
    }
}