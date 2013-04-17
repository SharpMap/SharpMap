using System;

namespace GeoAPI.Features
{
    /// <summary>
    /// Interface for all classes that provide access to a <see cref="IFeature{T}"/>s attributes
    /// </summary>
    public interface IFeatureAttributes : ICloneable
    {
        /// <summary>
        /// Gets or sets an attribute value by its index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The associated value</returns>
        object this[int index] { get; set; }

        /// <summary>
        /// Gets or sets an attribute value by its index
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The associated value</returns>
        object this[string key] { get; set; }

        /// <summary>
        /// Function to get all the values associated with this
        /// </summary>
        /// <returns></returns>
        object[] GetValues();

    }
}