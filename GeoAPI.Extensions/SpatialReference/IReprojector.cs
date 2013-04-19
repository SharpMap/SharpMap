using System;
using System.Collections.Generic;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace GeoAPI.SpatialReference
{
    /// <summary>
    /// Interface for objects that can reproject from one <see cref="ISpatialReference"/> 
    /// to another <see cref="ISpatialReference"/>
    /// </summary>
    public interface IReprojector : IReprojectorCore
    {
        /// <summary>
        /// Function to reproject an <see cref="IGeometry"/>
        /// </summary>
        /// <param name="geometry">The coordinate to reproject</param>
        /// <param name="from">The spatial reference the <paramref name="geometry"/> is in.</param>
        /// <param name="to">The spatial reference the return value should be in.</param>
        /// <returns>
        /// A <see cref="IGeometry"/> that represents <paramref name="geometry"/> in <paramref name="to"/> <see cref="ISpatialReference"/>.
        /// </returns>
        IGeometry Reproject(IGeometry geometry, ISpatialReference from, ISpatialReference to);

        /// <summary>
        /// Function to reproject an <see cref="IFeature{T}"/>
        /// </summary>
        /// <param name="feature">The coordinate to reproject</param>
        /// <param name="from">The spatial reference the <paramref name="feature"/> is in.</param>
        /// <param name="to">The spatial reference the return value should be in.</param>
        /// <returns>
        /// A <see cref="IFeature{T}"/> that represents <paramref name="feature"/> in <paramref name="to"/> <see cref="ISpatialReference"/>.
        /// </returns>
        IFeature Reproject(IFeature feature, ISpatialReference from, ISpatialReference to);

        /// <summary>
        /// Function to reproject an <see cref="IGeometry"/>
        /// </summary>
        /// <param name="features">The coordinate to reproject</param>
        /// <param name="from">The spatial reference the <paramref name="features"/> is in.</param>
        /// <param name="to">The spatial reference the return value should be in.</param>
        /// <returns>
        /// A <see cref="IEnumerable{IFeature}"/> that represents <paramref name="features"/> in <paramref name="to"/> <see cref="ISpatialReference"/>.
        /// </returns>
        IEnumerable<IFeature> Reproject(IEnumerable<IFeature> features, ISpatialReference from, ISpatialReference to);
    }
}