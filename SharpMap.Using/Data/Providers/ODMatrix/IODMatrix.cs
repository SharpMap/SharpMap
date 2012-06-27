namespace SharpMap.Data.Providers.ODMatrix
{
    /// <summary>
    /// Interface for origin-destination matrices (ODMatrix)
    ///  </summary>
    /// <remarks>
    /// An ODMatrix is a nxn Matrix where the rows represent the sources and the columns represent the destinations.
    /// </remarks>
    public interface IODMatrix : IRelationProvider
    {
        /// <summary>
        /// Gets or sets the matrix' name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the size (n) of the nxn Matrix
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Indexer to the value of the relation <paramref name="originId"/>  -> <paramref name="destinationId"/>
        /// </summary>
        /// <param name="originId">The origin id</param>
        /// <param name="destinationId">The destination id</param>
        /// <returns>The value for the relation</returns>
        double this[ushort originId, ushort destinationId] { get; set; }

        /// <summary>
        /// Indexer to the origin-/ destination potential
        /// </summary>
        /// <param name="zoneId">The id of the zone</param>
        /// <param name="column">The value to get</param>
        /// <returns>The potential</returns>
        double this[ushort zoneId, ODMatrixVector column] { get; }
    }
}