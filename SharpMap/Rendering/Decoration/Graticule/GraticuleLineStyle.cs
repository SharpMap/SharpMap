namespace SharpMap.Rendering.Decoration.Graticule
{
    public enum GraticuleLineStyle
    {
        /// <summary>
        /// Do not draw line (or ticks)
        /// </summary>
        None = 0,

        /// <summary>
        /// Parallels and meridians plot as continuous lines 
        /// </summary>
        Continuous = 1,

        /// <summary>
        /// Plot intersections of parallels and meridians only, using a Solid tick mark 
        /// </summary>
        SolidTick = 2,

        /// <summary>
        /// Plot intersections of parallels and meridians only, using a Hollow tick mark 
        /// </summary>
        HollowTick = 3
    }
}
