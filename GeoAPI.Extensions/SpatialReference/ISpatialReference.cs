using GeoAPI.Features;

namespace GeoAPI.SpatialReference
{
    /// <summary>
    /// Interface for classes that hold spatial reference information
    /// </summary>
    public interface ISpatialReference : IUnique<string>
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

        /// <summary>
        /// Function to create a <see cref="ISpatialReference"/> by its <paramref name="definition"/>
        /// </summary>
        /// <param name="oid">The identifier</param>
        /// <param name="definition">The string that defines spatial reference</param>
        /// <returns>A spatial reference</returns>
        ISpatialReference Create(string oid, string definition);
    }

    /// <summary>
    /// Interface for classes that can parse or convert to and from
    /// </summary>
    public interface ISpatialReferenceConverter
    {
        ISpatialReference FromWellKnownText(string spatialReference);
        ISpatialReference FromProj4(string proj4);
        ISpatialReference FromAuthorityCode(string authorityCode);

        string ToWellKnownText(ISpatialReference spatialReference);
        string ToProj4(ISpatialReference spatialReference);
        string ToAuthorityCode(ISpatialReference spatialReference);
    }
}