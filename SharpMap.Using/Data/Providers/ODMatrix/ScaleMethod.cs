namespace SharpMap.Data.Providers.ODMatrix
{
    /// <summary>
    /// Defined scale methods
    /// </summary>
    public enum ScaleMethod
    {
        /// <summary>
        /// No scaling is applied, <c>output = input</c>
        /// </summary>
        None,

        /// <summary>
        /// Linear scaling, <c>output = ScaleFactor * input</c>
        /// </summary>
        Linear,

        /// <summary>
        /// Square scaling, <c>output = Math.Sqrt(input)</c>
        /// </summary>
        Square,

        /// <summary>
        /// Qubic scaling, <c>output = Math.Pow(input, 1d/3d)</c>
        /// </summary>
        Qubic,

        /// <summary>
        /// Logarithmic scaling, <c>output = Math.Log(input, ScaleFactor)</c>
        /// </summary>
        Log,

        /// <summary>
        /// Logarithmic scaling with a base of 10, <c>output = Math.Log10(input)</c>
        /// </summary>
        Log10
    }
}