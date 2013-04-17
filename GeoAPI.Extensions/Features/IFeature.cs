using System;
using GeoAPI.Geometries;

namespace GeoAPI.Features
{
    /// <summary>
    /// Interface for all classes that can be used as a feature
    /// </summary>
    public interface IFeature<T> : IEntity<T>, ICloneable, IDisposable where T : IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Gets the factory that created this feature
        /// </summary>
        IFeatureFactory<T> Factory { get; }

        /// <summary>
        /// Gets or sets the geometry defining the feature
        /// </summary>
        IGeometry Geometry { get; set; }

        /// <summary>
        /// Gets the attributes associated with this feature
        /// </summary>
        IFeatureAttributes Attributes { get; }
    }
}