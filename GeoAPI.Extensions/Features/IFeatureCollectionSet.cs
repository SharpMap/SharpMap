using System.Collections.Generic;

namespace GeoAPI.Features
{
    /// <summary>
    /// Interface for a collection of <see cref="IFeatureCollection"/>s.
    /// </summary>
    public interface IFeatureCollectionSet : ICollection<IFeatureCollection>
    {
        /// <summary>
        /// Indexer for a <see cref="IFeatureCollection"/> by its name.
        /// </summary>
        /// <param name="name">The name of the feature collection</param>
        /// <returns>A feature collection</returns>
        /// <remarks>If there is no matching collection, implementation should not throw an exception but return <c>null</c>.</remarks>
        IFeatureCollection this[string name] { get; }

        /// <summary>
        /// Indexer for a <see cref="IFeatureCollection"/> by its name.
        /// </summary>
        /// <param name="index">The index of the feature collection</param>
        /// <returns>A feature collection</returns>
        /// <remarks>If there is no matching collection, implementation should not throw an exception but return <c>null</c>.</remarks>
        IFeatureCollection this[int index] { get; }

        /// <summary>
        /// Function to evalutate if a feature collection named <paramref name="name"/> is present.
        /// </summary>
        /// <param name="name">The name of the collection</param>
        /// <returns></returns>
        bool Contains(string name);

        /// <summary>
        /// Function to remove the feature collection named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the collection</param>
        /// <returns><c>true</c> if removed, <c>false</c> otherwise.</returns>
        bool Remove(string name);
    }
}