using System;
using System.Runtime.CompilerServices;
using Common.Logging;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// Extension methods to get hold of coordinate systems
    /// </summary>
    public static class CoordinateSystemExtensions
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CoordinateSystemExtensions));

        private static Dictionary<int, string> _sridDefinition;

        private static readonly Dictionary<int, ICoordinateSystem> _sridCoordinateSystem = new Dictionary<int, ICoordinateSystem>();

        /// <summary>
        /// Gets a coordinate system for the map based on the <see cref="Map.SRID"/> property
        /// </summary>
        /// <param name="self">The map</param>
        /// <returns>A coordinate system</returns>
        public static ICoordinateSystem GetCoordinateSystem(this Map self)
        {
            _logger.Debug( fmh => fmh("Getting coordinate system for map"));
            return GetCoordinateSystemForSrid(self.SRID);
        }

        /// <summary>
        /// Gets a coordinate system for the map based on the <see cref="ILayer.SRID"/> property
        /// </summary>
        /// <param name="self">The layer</param>
        /// <returns>A coordinate system</returns>
        public static ICoordinateSystem GetCoordinateSystem(this ILayer self)
        {
            _logger.Debug(fmh => fmh("Getting coordinate system for {0} '{1}'", self.GetType().Name, self.LayerName));
            return GetCoordinateSystemForSrid(self.SRID);
        }

        /// <summary>
        /// Gets a coordinate system for the map based on the <see cref="IBaseProvider.SRID"/> property
        /// </summary>
        /// <param name="self">The provider</param>
        /// <returns>A coordinate system</returns>
        public static ICoordinateSystem GetCoordinateSystem(this IProvider self)
        {
            _logger.Debug(fmh => fmh("Getting coordinate system for {0} '{1}'", self.GetType().Name, self.ConnectionID));
            return GetCoordinateSystemForSrid(self.SRID);
        }

        /// <summary>
        /// Gets a coordinate system for the map based on the <see cref="IGeometry.SRID"/> property
        /// </summary>
        /// <param name="self">The layer</param>
        /// <returns>A coordinate system</returns>
        public static ICoordinateSystem GetCoordinateSystem(this IGeometry self)
        {
            return GetCoordinateSystemForSrid(self.SRID);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static ICoordinateSystem GetCoordinateSystemForSrid(int srid)
        {
            if (srid <= 0)
                return null;

            return Session.Instance.CoordinateSystemServices.GetCoordinateSystem(srid);
        }

        /// <summary>
        /// Method to get a <see cref="IGeometryFactory"/> for the specified <paramref name="self">map</paramref>
        /// </summary>
        /// <param name="self">The map</param>
        /// <returns>A geometry factory</returns>
        public static IGeometryFactory GetFactory(this Map self)
        {
            return GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(self.SRID);
        }

        /// <summary>
        /// Method to get a <see cref="IGeometryFactory"/> for the specified <paramref name="self">layer</paramref>
        /// </summary>
        /// <param name="self">The layer</param>
        /// <returns>A geometry factory</returns>
        public static IGeometryFactory GetFactory(this ILayer self)
        {
            return GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(self.SRID);
        }

        /// <summary>
        /// Method to get a <see cref="IGeometryFactory"/> for the specified <paramref name="self">provider</paramref>
        /// </summary>
        /// <param name="self">The provider</param>
        /// <returns>A geometry factory</returns>
        public static IGeometryFactory GetFactory(this IProvider self)
        {
            return GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(self.SRID);
        }
        
    }
}