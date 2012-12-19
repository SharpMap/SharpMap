using System;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Defines possible spatial indices
    /// </summary>
    public enum SpatiaLiteIndex
    {
        /// <summary>
        /// No spatial index defined
        /// </summary>
        None = 0,
        /// <summary>
        /// RTree
        /// </summary>
        RTree = 1,
        /// <summary>
        /// Cache of minimum bounding rectangles (MBR)
        /// </summary>
        [Obsolete("Do not use on purpose!")]
        MbrCache = 2
    }
}