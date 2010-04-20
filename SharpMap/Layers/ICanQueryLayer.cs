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

        /// <summary>
        /// Whether the layer is queryable when used in a SharpMap.Web.Wms.WmsServer, 
        /// ExecuteIntersectionQuery() will be possible in all other situations when set to FALSE.
        /// This property currently only applies to WMS and should perhaps be moved to a WMS
        /// specific class.
        /// </summary>
        bool IsQueryEnabled { get; set; }
    }
}