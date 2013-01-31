namespace SharpMap.Rendering.Decoration
{
    /// <summary>
    /// Anchor relative position
    /// </summary>
    public enum MapDecorationAnchor
    {
        /// <summary>
        /// Default anchor position, <see cref="RightBottom"/>.
        /// </summary>
        Default = RightBottom,

        /// <summary>
        /// Left top
        /// </summary>
        LeftTop = MapDecorationAnchorFlags.Left | MapDecorationAnchorFlags.Top,

        /// <summary>
        /// Left center
        /// </summary>
        LeftCenter = MapDecorationAnchorFlags.Left | MapDecorationAnchorFlags.VerticalCenter,

        /// <summary>
        /// Left bottom
        /// </summary>
        LeftBottom = MapDecorationAnchorFlags.Left | MapDecorationAnchorFlags.Bottom,

        /// <summary>
        /// Right top
        /// </summary>
        RightTop = MapDecorationAnchorFlags.Right | MapDecorationAnchorFlags.Top,

        /// <summary>
        /// Right bottom
        /// </summary>
        RightBottom = MapDecorationAnchorFlags.Right | MapDecorationAnchorFlags.Bottom,

        /// <summary>
        /// Right center
        /// </summary>
        RightCenter = MapDecorationAnchorFlags.Right | MapDecorationAnchorFlags.VerticalCenter,

        /// <summary>
        /// Center top
        /// </summary>
        CenterTop = MapDecorationAnchorFlags.HorizontalCenter | MapDecorationAnchorFlags.Top,

        /// <summary>
        /// Right bottom
        /// </summary>
        CenterBottom = MapDecorationAnchorFlags.HorizontalCenter | MapDecorationAnchorFlags.Bottom,

        /// <summary>
        /// Right center
        /// </summary>
        Center = MapDecorationAnchorFlags.HorizontalCenter | MapDecorationAnchorFlags.VerticalCenter,
    }
}