using System;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Rendering.Symbolizer;

namespace SharpMap.Layers.Symbolizer
{
    /// <summary>
    /// A vector layer class that can symbolize polygonal geometries.
    /// </summary>
    [Serializable]
    public class PolygonalVectorLayer : BaseVectorLayer<IPolygonal> 
    {

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layers's name</param>
        public PolygonalVectorLayer(string layerName) 
            : this(layerName, null)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layers's name</param>
        /// <param name="dataSource">The layers's datasource</param>
        public PolygonalVectorLayer(string layerName, IProvider dataSource)
            : this(layerName, dataSource, new BasicPolygonSymbolizer())
        {
        }
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layer's name</param>
        /// <param name="dataSource">The layer's datasource</param>
        /// <param name="symbolizer">The layer's symbolizer</param>
        public PolygonalVectorLayer(string layerName, IProvider dataSource, ISymbolizer<IPolygonal> symbolizer)
            : base(layerName, dataSource, symbolizer)
        {
        }

    }
}