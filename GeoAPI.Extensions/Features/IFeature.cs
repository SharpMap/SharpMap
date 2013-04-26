using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;

namespace GeoAPI.Features
{
    public interface IFeature : IEntity, ICloneable, IDisposable
    {
        /// <summary>
        /// Gets the factory that created this feature
        /// </summary>
        [FeatureAttribute(Ignore = true)]
        IFeatureFactory Factory { get; }

        /// <summary>
        /// Gets or sets the geometry defining the feature
        /// </summary>
        [FeatureAttribute(Ignore = false)]
        IGeometry Geometry { get; set; }

        /// <summary>
        /// Gets the attributes associated with this feature
        /// </summary>
        [FeatureAttribute(Ignore = true)]
        IFeatureAttributes Attributes { get; }
    }
    

    /// <summary>
    /// Interface for all classes that can be used as a feature
    /// </summary>
    public interface IFeature<T> : IFeature, IEntity<T>
        where T : IComparable<T>, IEquatable<T>
    {
    }
}