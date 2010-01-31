using SharpMap.Data;
using SharpMap.Geometries;

namespace SharpMap.Layers
{
    public interface ICanQueryLayer
    {
        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds);
    }
}