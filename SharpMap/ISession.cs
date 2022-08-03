using GeoAPI;
using NetTopologySuite;
using ProjNet;

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
        NtsGeometryServices GeometryServices { get; }

        /// <summary>
        /// Gets the coordinate system services instance
        /// </summary>
        CoordinateSystemServices CoordinateSystemServices { get; }

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
        ISession SetGeometryServices(NtsGeometryServices geometryServices);

        /// <summary>
        /// Method to set the <see cref="CoordinateSystemServices"/> for a session
        /// </summary>
        /// <param name="csServices">The <see cref="ProjNet.CoordinateSystems.CoordinateSystem"/>s services object</param>
        /// <returns>The updated session</returns>
        ISession SetCoordinateSystemServices(CoordinateSystemServices csServices);

        /// <summary>
        /// Method to set the <see cref="CoordinateSystemRepository"/> for a session
        /// </summary>
        /// <param name="csRepository">The <see cref="ProjNet.CoordinateSystems.CoordinateSystem"/>s repository</param>
        /// <returns>The updated session</returns>
        ISession SetCoordinateSystemRepository(ICoordinateSystemRepository csRepository);

        #endregion

    }
}
