namespace SharpMap.Geometries
{
    /// <summary>
    /// Interface for all classes that define
    /// </summary>
    public interface IGeometryClassifier
    {
        /// <summary>
        /// 
        /// </summary>
        GeometryType2 GeometryType { get; }
    }

    /// <summary>
    /// Interface for all classes that represent geometries
    /// </summary>
    public interface IUndefined : IGeometryClassifier
    {}

    /// <summary>
    /// Interface for all classes that represent puntal geometries, e.g. Point, Point3D and MultiPoint
    /// </summary>
    public interface IPuntal : IGeometryClassifier
    {
    }
    /// <summary>
    /// Interface for all classes that represent lineal geometries, e.g. Point, Point3D and MultiPoint
    /// </summary>
    public interface ILineal : IGeometryClassifier
    {
    }
    /// <summary>
    /// Interface for all classes that represent polygonal geometries, e.g. Point, Point3D and MultiPoint
    /// </summary>
    public interface IPolygonal : IGeometryClassifier
    {
    }
}