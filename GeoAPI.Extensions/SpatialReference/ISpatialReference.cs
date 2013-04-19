using GeoAPI.Features;

namespace GeoAPI.SpatialReference
{
    /// <summary>
    /// Interface for classes that hold spatial reference information
    /// </summary>
    public interface ISpatialReference : IEntity<string>
    {
        /// <summary>
        /// Gets the definition
        /// </summary>
        string Definition { get; }

        /// <summary>
        /// Gets the 
        /// </summary>
        SpatialReferenceDefinitionType DefinitionType { get; }
    }

    /// <summary>
    /// Interfacer for factory classes that can create <see cref="ISpatialReference"/>s.
    /// </summary>
    public interface ISpatialReferenceFactory
    {
        /// <summary>
        /// Function to create a <see cref="ISpatialReference"/> by its <paramref name="definition"/>
        /// <para>The <paramref name="definition"/> is also used as its <see cref="ISpatialReference.Oid"/></para>
        /// </summary>
        /// <param name="definition">The string that defines spatial reference</param>
        /// <returns>A spatial reference</returns>
        ISpatialReference Create(string definition);
    }
}