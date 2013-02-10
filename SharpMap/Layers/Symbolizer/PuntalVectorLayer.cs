using System;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Rendering.Symbolizer;

namespace SharpMap.Layers.Symbolizer
{
    /// <summary>
    /// A vector layer class that can symbolize puntal geometries
    /// </summary>
    [Serializable]
    public class PuntalVectorLayer : BaseVectorLayer<IPuntal>
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layer's name</param>
        public PuntalVectorLayer(string layerName) : this(layerName, null)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layer's name</param>
        /// <param name="dataSource">The layer's data source</param>
        public PuntalVectorLayer(string layerName, IProvider dataSource)
            : this(layerName, dataSource, new RasterPointSymbolizer())
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="layerName">The layer's name</param>
        /// <param name="dataSource">The layer's data source</param>
        /// <param name="symbolizer">The layer's symbolizer</param>
        public PuntalVectorLayer(string layerName, IProvider dataSource, ISymbolizer<IPuntal> symbolizer)
            : base(layerName, dataSource, symbolizer)
        {
        }
    }
}