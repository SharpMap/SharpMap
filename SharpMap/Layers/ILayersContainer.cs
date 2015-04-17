using System;
using System.Collections.Generic;

namespace SharpMap.Layers
{
    /// <summary>
    /// Interface to mark entities that expose layers.
    /// </summary>
    public interface ILayersContainer 
    {
        /// <summary>
        /// Gets the layers exposed.
        /// </summary>
        IList<ILayer> Layers { get; }
    }
}
