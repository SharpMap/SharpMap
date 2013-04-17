using System;
using System.Collections.Generic;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace GeoAPI.SpatialReference
{
    public interface IReprojectorCore
    {
        /// <summary>
        /// Function to reproject a single coordinate
        /// </summary>
        /// <param name="coordinate">The coordinate to reproject</param>
        /// <param name="from">The spatial reference the <paramref name="coordinate"/> is in.</param>
        /// <param name="to">The spatial reference the return value should be in.</param>
        /// <returns>
        /// A <see cref="Coordinate"/> that represents <paramref name="coordinate"/> in <paramref name="to"/> <see cref="ISpatialReference"/>.
        /// </returns>
        Coordinate Reproject(Coordinate coordinate, ISpatialReference from, ISpatialReference to);

        /// <summary>
        /// Function to reproject an <see cref="Envelope"/>
        /// </summary>
        /// <param name="envelope">The coordinate to reproject</param>
        /// <param name="from">The spatial reference the <paramref name="envelope"/> is in.</param>
        /// <param name="to">The spatial reference the return value should be in.</param>
        /// <returns>
        /// A <see cref="Envelope"/> that represents <paramref name="envelope"/> in <paramref name="to"/> <see cref="ISpatialReference"/>.
        /// </returns>
        Envelope Reproject(Envelope envelope, ISpatialReference from, ISpatialReference to);

        /// <summary>
        /// Function to reproject an <see cref="ICoordinateSequence"/>
        /// </summary>
        /// <param name="sequence">The coordinate to reproject</param>
        /// <param name="from">The spatial reference the <paramref name="sequence"/> is in.</param>
        /// <param name="to">The spatial reference the return value should be in.</param>
        /// <returns>
        /// A <see cref="ICoordinateSequence"/> that represents <paramref name="sequence"/> in <paramref name="to"/> <see cref="ISpatialReference"/>.
        /// </returns>
        ICoordinateSequence Reproject(ICoordinateSequence sequence, ISpatialReference from, ISpatialReference to);
    }

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
        IFeature<T> Reproject<T>(IFeature<T> feature, ISpatialReference from, ISpatialReference to) 
            where T : IComparable<T>, IEquatable<T>;

        /// <summary>
        /// Function to reproject an <see cref="IGeometry"/>
        /// </summary>
        /// <param name="features">The coordinate to reproject</param>
        /// <param name="from">The spatial reference the <paramref name="features"/> is in.</param>
        /// <param name="to">The spatial reference the return value should be in.</param>
        /// <returns>
        /// A <see cref="IEnumerable{IFeature}"/> that represents <paramref name="features"/> in <paramref name="to"/> <see cref="ISpatialReference"/>.
        /// </returns>
        IEnumerable<IFeature<T>> Reproject<T>(IEnumerable<IFeature<T>> features, ISpatialReference from, ISpatialReference to)
            where T : IComparable<T>, IEquatable<T>;
    }
}