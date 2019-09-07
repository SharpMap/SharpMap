namespace SharpMap.Rendering.Decoration.Graticule
{
    /// <summary>
    /// Define how a PcsGraticuleStyle will render 
    /// </summary>
    public enum PcsGraticuleMode
    {
        /// <summary>
        /// Classic rectilinear graticule
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Overrides Projected graticule behaviour ONLY for Web Mercator (SRID 3857), causing 
        /// the graticule to be rendered as the meridian scale distortion lines from equator
        /// to the poles. Can be used in conjunction or as an alternative to a ScaleBar. 
        /// </summary>
        WebMercatorScaleLines = 1
    }
}
