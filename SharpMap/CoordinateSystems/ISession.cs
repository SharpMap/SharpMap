using GeoAPI;

namespace SharpMap.CoordinateSystems
{
    public interface ISession
    {
        IGeometryServices GeometryServices { get; }
        ICoordinateSystemServices CoordinateSystemServices { get; }
        ICoordinateSystemRepository CoordinateSystemRepository { get; }

        ISession SetGeometryServices(IGeometryServices geometryServices);
        ISession SetCoordinateSystemServices(ICoordinateSystemServices geometryServices);
        ISession SetCoordinateSystemRepository(ICoordinateSystemRepository geometryServices);
        ISession ReadConfiguration();
    }
}