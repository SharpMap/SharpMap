using System.Globalization;
using System.Threading;
using GeoAPI.Geometries;
using GeoAPI.Utilities;

namespace GeoAPI
{
    public static class GeometryServicesExtensions
    {
        private static readonly ThreadSafeStore<string, IGeometryFactory> _store;

        static GeometryServicesExtensions()
        {
            _store = new ThreadSafeStore<string, IGeometryFactory>( t => GeometryServiceProvider.Instance.CreateGeometryFactory(GetSrid(t)));
        }

        private static int _userSrid = 100000000;

        private static int GetSrid(string oid)
        {
            if (string.IsNullOrEmpty(oid))
                return GeometryServiceProvider.Instance.DefaultSRID;

            int srid;
            if (int.TryParse(oid, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out srid))
                return srid;

            Interlocked.Increment(ref _userSrid);
            return _userSrid;
        }

        public static IGeometryFactory CreateGeometryFactory(this IGeometryServices self, string oid)
        {
            return _store.Get(oid);
        }


    }
}