using System;
using Common.Logging;
using GeoAPI;

namespace SharpMap
{
    /// <summary>
    /// A SharpMap Session class
    /// </summary>
    public class Session : ISession
    {
        private static ICoordinateSystemRepository _repository;
        private ICoordinateSystemServices _coordinateSystemServices;

        /// <summary>
        /// Static constructor
        /// </summary>
        static Session()
        {
            Instance = new Session();
        }

        /// <summary>
        /// Gets a value indicating the current instance
        /// </summary>
        public static ISession Instance { get; private set; }

        /// <summary>
        /// The geometry services instance
        /// </summary>
        public IGeometryServices GeometryServices { get; set; }

        /// <summary>
        /// Gets the coordinate system services instance
        /// </summary>
        public ICoordinateSystemServices CoordinateSystemServices
        {
            get
            {
                if (_coordinateSystemServices == null)
                    throw new InvalidOperationException("Must declare a coordinate system services object!");
                return _coordinateSystemServices;
            }
            set { _coordinateSystemServices = value; }
        }

        /// <summary>
        /// Gets the coordinate system repository
        /// </summary>
        public ICoordinateSystemRepository CoordinateSystemRepository
        {
            get { return _repository ?? CoordinateSystemServices as ICoordinateSystemRepository; }
            set { _repository = value; }
        }

        #region Fluent

        /// <summary>
        /// Method to set the geometry services class
        /// </summary>
        /// <param name="geometryServices">The geometry services class</param>
        /// <returns>A reference to this session</returns>
        public ISession SetGeometryServices(IGeometryServices geometryServices)
        {
            GeometryServices = geometryServices;
            return this;
        }

        /// <summary>
        /// Method to set the coordinate system services class
        /// </summary>
        /// <param name="coordinateSystemServices">The coordinate system services class</param>
        /// <returns>A reference to this session</returns>
        public ISession SetCoordinateSystemServices(ICoordinateSystemServices coordinateSystemServices)
        {
            CoordinateSystemServices = coordinateSystemServices;
            return this;
        }

        /// <summary>
        /// Method to set the coordinate system repository class
        /// </summary>
        /// <param name="coordinateSystemRepository">The coordinate system repository class</param>
        /// <returns>A reference to this session</returns>
        public ISession SetCoordinateSystemRepository(ICoordinateSystemRepository coordinateSystemRepository)
        {
            CoordinateSystemRepository = coordinateSystemRepository;
            return this;
        }

        /// <summary>
        /// Method to read the configuration
        /// </summary>
        /// <returns>A reference to this session</returns>
        public ISession ReadConfiguration()
        {
            var log = LogManager.GetLogger<ISession>();
            log.Warn(f => f("Configuring SharpMap session via ReadConfiguration currently not supported"));
            return this;
        }
        #endregion
    }
}