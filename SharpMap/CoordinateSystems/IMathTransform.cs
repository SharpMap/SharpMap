namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// Interface for classes that can perform coordinate transformation between <see cref="ICoordinateSystem"/> spatial reference systems
    /// </summary>
    public interface IMathTransform
    {
        /// <summary>
        /// Perorms in-place transformation of the coordinates in the given <paramref name="sequence"/>.
        /// </summary>
        /// <param name="sequence">The sequence to transform</param>
        void Transform(CoordinateSequence sequence);

        /// <summary>
        /// Transforms a <paramref name="sequence"/> of coordinates and returns the values in a new sequence.
        /// </summary>
        /// <param name="sequence">The sequence to transform</param>
        /// <returns>A sequence containing transformed values</returns>
        CoordinateSequence GetTransformed(CoordinateSequence sequence);
    }

    internal class NoopMathTransform : IMathTransform
    {
        public void Transform(CoordinateSequence sequence) { }

        public CoordinateSequence GetTransformed(CoordinateSequene sequence)
            => sequence.Copy();
    }
}
