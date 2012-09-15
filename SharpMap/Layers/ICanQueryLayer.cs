using GeoAPI.Geometries;
using SharpMap.Data;

namespace SharpMap.Layers
{
    /// <summary>
    /// Interface for Layers, that can be queried
    /// </summary>
    public interface ICanQueryLayer : ILayer
    {
        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// 
        /// Note! The table added should be named according to the LayerName!
        /// </summary>
        /// <param name="box">Bounding box to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// 
        /// Note! The table added should be named according to the LayerName!
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        void ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds);

        /// <summary>
        /// Whether the layer is queryable when used in a SharpMap.Web.Wms.WmsServer, 
        /// ExecuteIntersectionQuery() will be possible in all other situations when set to FALSE.
        /// This property currently only applies to WMS and should perhaps be moved to a WMS
        /// specific class.
        /// </summary>
        bool IsQueryEnabled { get; set; }
    }
}