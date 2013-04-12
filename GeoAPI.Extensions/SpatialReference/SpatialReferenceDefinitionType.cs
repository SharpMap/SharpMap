namespace GeoAPI.SpatialReference
{
    /// <summary>
    /// Enumeration of spatial known (and used) spatial reference definition types
    /// </summary>
    public enum SpatialReferenceDefinitionType
    {
        /// <summary>
        /// Well known text format
        /// </summary>
        WellKnownText,

        /// <summary>
        /// Proj4 definition string
        /// </summary>
        Proj4,

        /// <summary>
        /// Authority and AuthorityCode separated by a colon. <para>Example: EPSG:4326</para>
        /// </summary>
        AuthorityCode,
    }
}