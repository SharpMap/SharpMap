using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GeoAPI.Features
{
    /// <summary>
    /// Interface for all classes that can create features
    /// </summary>
    public interface IFeatureFactory<T> where T : IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Gets the geometry factory to create features
        /// </summary>
        IGeometryFactory GeometryFactory { get; }

        /// <summary>
        /// Gets a list of attribute names
        /// </summary>
        IList<IFeatureAttributeDefinition> Attributes { get; }

        /// <summary>
        /// Creates a new feature
        /// </summary>
        /// <returns>A new feature with no geometry and attributes</returns>
        IFeature<T> Create();

        /// <summary>
        /// Creates a new feature with <paramref name="geometry"/>, but no attributes
        /// </summary>
        /// <returns>A new feature with <paramref name="geometry"/>, but no attributes</returns>
        IFeature<T> Create(IGeometry geometry);
    }
}