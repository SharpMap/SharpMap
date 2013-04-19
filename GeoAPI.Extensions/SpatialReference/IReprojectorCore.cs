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

        /// <summary>
        /// Gets a spatial reference factory to creates spatial references by their definition
        /// </summary>
        ISpatialReferenceFactory Factory { get; }
    }
}