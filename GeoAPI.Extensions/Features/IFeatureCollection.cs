using System.Collections.Generic;

namespace GeoAPI.Features
{
    
    /// <summary>
    /// Interface for a collection of features
    /// </summary>
    public interface IFeatureCollection : ICollection<IFeature>
    {
        /// <summary>
        /// Gets the name of a feature collection, e.g. the type of features in it
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets a list of attribute names
        /// </summary>
        [FeatureAttribute(Ignore = true)]
        IList<IFeatureAttributeDefinition> AttributesDefinition { get; }

        /// <summary>
        /// Gets a feature by its object identifier
        /// </summary>
        /// <param name="oid">The object identifier</param>
        /// <returns>The feature associated with the identifier, if present, otherwise <c>null</c></returns>
        IFeature this[object oid] { get; }

        /// <summary>
        /// Gets a feature by an index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The feature at the index, if present, otherwise <c>null</c></returns>
        IFeature this[int index] { get; }

        /*
        /// <summary>
        /// Gets the factory to create new features
        /// </summary>
        IFeatureFactory Factory { get; }


        /// <summary>
        /// Gets the feature by its identifier
        /// </summary>
        /// <param name="oid">The features identifier</param>
        /// <returns>A feature if present</returns>
        IFeature GetFeatureByOid(object oid);
        */

        /// <summary>
        /// Creates a clone of the collection, without the items in it.
        /// </summary>
        /// <returns>A clone of the feature collection</returns>
        IFeatureCollection Clone();

        /// <summary>
        /// Adds a range of features to the collection
        /// </summary>
        /// <param name="features">The features to add</param>
        void AddRange(IEnumerable<IFeature> features);
    }

    /// <summary>
    /// Interface for classes that have a feature factory
    /// </summary>
    public interface IHasFeatureFactory
    {
        /// <summary>
        /// Get the factory to create features
        /// </summary>
        IFeatureFactory Factory { get; }
    }
}