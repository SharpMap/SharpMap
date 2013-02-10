using System;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Rendering.Symbolizer;

namespace SharpMap.Layers.Symbolizer
{
    /// <summary>
    /// Vector layer class than can symbolize any type of geometry
    /// </summary>
    [Serializable]
    public class AnyGeometryVectorLayer : BaseVectorLayer<IGeometry>
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layer's name</param>
        public AnyGeometryVectorLayer(string layerName)
            : this(layerName, null)
        {}

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layer's name</param>
        /// <param name="datasource">The layers's datasource</param>
        public AnyGeometryVectorLayer(string layerName, IProvider datasource)
            : this(layerName, datasource, new GeometrySymbolizer
                             {
                                 PointSymbolizer = new RasterPointSymbolizer(),
                                 LineSymbolizer = new BasicLineSymbolizer(),
                                 PolygonSymbolizer = new BasicPolygonSymbolizer()
                             })
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layer's name</param>
        /// <param name="datasource">The layers's datasource</param>
        /// <param name="symbolizer">The layers's symbolizer</param>
        private AnyGeometryVectorLayer(string layerName, IProvider datasource, GeometrySymbolizer symbolizer)
            : base(layerName, datasource, symbolizer)
        {
        }
    }
}