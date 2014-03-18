using System;
using System.IO;
using System.Runtime.CompilerServices;
using Common.Logging;
using System.Collections.Generic;
#if !DotSpatialProjections
using GeoAPI.CoordinateSystems;
using ProjNet.CoordinateSystems;
#else
using DotSpatial.Projections.Transforms;
using ICoordinateSystem = DotSpatial.Projections.ProjectionInfo;
#endif
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
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        private static Dictionary<int, string> _sridDefinition;

#if !DotSpatialProjections
        private static readonly Dictionary<int, ICoordinateSystem> _sridCoordinateSystem = new Dictionary<int, ICoordinateSystem>();
#endif

        /// <summary>
        /// Gets a coordinate system for the map based on the <see cref="Map.SRID"/> property
        /// </summary>
        /// <param name="self">The map</param>
        /// <returns>A coordinate system</returns>
        public static ICoordinateSystem GetCoordinateSystem(this Map self)
        {
            Logger.Debug( fmh => fmh("Getting coordinate system for map"));
            return GetCoordinateSystemForSrid(self.SRID);
        }

        /// <summary>
        /// Gets a coordinate system for the map based on the <see cref="ILayer.SRID"/> property
        /// </summary>
        /// <param name="self">The layer</param>
        /// <returns>A coordinate system</returns>
        public static ICoordinateSystem GetCoordinateSystem(this ILayer self)
        {
            Logger.Debug(fmh => fmh("Getting coordinate system for {0} '{1}'", self.GetType().Name, self.LayerName));
            return GetCoordinateSystemForSrid(self.SRID);
        }

        /// <summary>
        /// Gets a coordinate system for the map based on the <see cref="IProvider.SRID"/> property
        /// </summary>
        /// <param name="self">The provider</param>
        /// <returns>A coordinate system</returns>
        public static ICoordinateSystem GetCoordinateSystem(this IProvider self)
        {
            Logger.Debug(fmh => fmh("Getting coordinate system for {0} '{1}'", self.GetType().Name, self.ConnectionID));
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

            ICoordinateSystem res = null;
#if !DotSpatialProjections
            if (_sridCoordinateSystem.TryGetValue(srid, out res))
                return res;

            var wkt = Converters.WellKnownText.SpatialReference.SridToWkt(srid);
            if (string.IsNullOrEmpty(wkt))            
            {
                Logger.Error( fmh => fmh("No definition for SRID {0}!", srid));
                return null;
            }

            var csFactory = new CoordinateSystemFactory();
            try
            {
                res = csFactory.CreateFromWkt(wkt);
            }
            catch (Exception)
            {
                Logger.Error( fmh => fmh("Could not parse definition for SRID {0}:\n{1}", srid, wkt));
                return null;
            }
            _sridCoordinateSystem.Add(srid, res);
#else
            try
            {
                if (_sridDefinition != null)
                {
                    string proj4;
                    if (_sridDefinition.TryGetValue(srid, out proj4))
                        res = ICoordinateSystem.FromProj4String(proj4);
                }

                if (res == null)
                    res = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(srid);
            }
            catch (Exception)
            {
                Logger.Error( fmh => fmh("Could not get coordinate system for SRID {0}!", srid) );
                return null;
            }
#endif
            return res;

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