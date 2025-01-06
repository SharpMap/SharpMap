namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// A spatial reference object
    /// </summary>
    public interface ICoordinateSystem
    {
        /// <summary>
        /// The Well-known text representation of this spatial referene object
        /// </summary>
        string WKT { get; }
    }
}
