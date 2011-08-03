using System;

namespace SharpMap.Rendering.Decoration
{
    /// <summary>
    /// Anchor flag values
    /// </summary>
    [Flags]
    public enum MapDecorationAnchorFlags
    {
        /// <summary>
        /// No anchor specified
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Left anchor
        /// </summary>
        Left = 1,

        /// <summary>
        /// Top anchor
        /// </summary>
        Top = 2,

        /// <summary>
        /// Right anchor
        /// </summary>
        Right = 4,

        /// <summary>
        /// Bottom anchor
        /// </summary>
        Bottom = 8,

        /// <summary>
        /// Vertical center anchor
        /// </summary>
        VerticalCenter = 16,

        /// <summary>
        /// Horizontal center anchor
        /// </summary>
        HorizontalCenter = 32,

        /// <summary>
        /// 
        /// </summary>
        Vertical = Top | VerticalCenter | Bottom,
        
        /// <summary>
        /// 
        /// </summary>
        Horizontal = Left | HorizontalCenter | Right
    }

    
}