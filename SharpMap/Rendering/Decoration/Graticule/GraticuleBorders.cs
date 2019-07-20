using System;

namespace SharpMap.Rendering.Decoration.Graticule
{
    /// <summary>
    /// Enumeration defining which borders should be labelled
    /// </summary>
    [Flags]
    public enum GraticuleBorders
    {
        /// <summary>
        /// No borders
        /// </summary>
        None = 0,

        /// <summary>
        /// Label Left and Top borders
        /// </summary>
        LeftTop = MapDecorationAnchorFlags.Left | MapDecorationAnchorFlags.Top,

        /// <summary>
        /// Label Left and Bottom borders
        /// </summary>
        LeftBottom = MapDecorationAnchorFlags.Left | MapDecorationAnchorFlags.Bottom,

        /// <summary>
        /// Label Right and Top borders
        /// </summary>
        RightTop = MapDecorationAnchorFlags.Right | MapDecorationAnchorFlags.Top,

        /// <summary>
        /// Label Right and Bottom borders
        /// </summary>
        RightBottom = MapDecorationAnchorFlags.Right | MapDecorationAnchorFlags.Bottom,

        /// <summary>
        /// Label all borders
        /// </summary>
        All = MapDecorationAnchorFlags.Left | MapDecorationAnchorFlags.Top | MapDecorationAnchorFlags.Right |
              MapDecorationAnchorFlags.Bottom,
    }
}
