using GeoAPI.Geometries;

namespace GeoAPI.Features
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFeature : IUnique<long>
    {
        /// <summary>
        /// The Factory that created this feature
        /// </summary>
        IFeatureFactory Factory { get; }

        /// <summary>
        /// The geometry defining the feature
        /// </summary>
        IGeometry Geometry { get; set; }
    }
}