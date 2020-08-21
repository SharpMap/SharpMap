using GeoAPI;

namespace SharpMap
{
    /// <summary>
    /// A SharpMap session
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// The geometry services instance
        /// </summary>
        IGeometryServices GeometryServices { get; }

        /// <summary>
        /// Gets the coordinate system services instance
        /// </summary>
        ICoordinateSystemServices CoordinateSystemServices { get; }

        /// <summary>
        /// Gets the coordinate system repository
        /// </summary>
        ICoordinateSystemRepository CoordinateSystemRepository { get; }

#region Fluent configuration

        /// <summary>
        /// Method to set the <see cref="GeometryServices"/> for a session
        /// </summary>
        /// <param name="geometryServices">The geometry services object</param>
        /// <returns>The updated session</returns>
        ISession SetGeometryServices(IGeometryServices geometryServices);

        /// <summary>
        /// Method to set the <see cref="CoordinateSystemServices"/> for a session
        /// </summary>
        /// <param name="csServices">The <see cref="GeoAPI.CoordinateSystems.ICoordinateSystem"/>s services object</param>
        /// <returns>The updated session</returns>
        ISession SetCoordinateSystemServices(ICoordinateSystemServices csServices);

        /// <summary>
        /// Method to set the <see cref="CoordinateSystemRepository"/> for a session
        /// </summary>
        /// <param name="csRepository">The <see cref="GeoAPI.CoordinateSystems.ICoordinateSystem"/>s repository</param>
        /// <returns>The updated session</returns>
        ISession SetCoordinateSystemRepository(ICoordinateSystemRepository csRepository);

#endregion

    }
}
