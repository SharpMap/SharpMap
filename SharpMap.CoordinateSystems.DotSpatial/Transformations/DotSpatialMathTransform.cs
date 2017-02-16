using System;
using System.Collections.Generic;
using DotSpatial.Projections;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;

namespace SharpMap.CoordinateSystems.Transformations
{
    /// <summary>
    /// A math transform based on two <see cref="ProjectionInfo"/>s.
    /// </summary>
    internal class DotSpatialMathTransform : IMathTransform
    {
        private readonly ProjectionInfo _source;
        private readonly ProjectionInfo _target;
        //private DotSpatialCoordinateTransformation _parent;
        private DotSpatialMathTransform _inverse;
        private bool _isInverted;

        private ProjectionInfo[] _order;

        /// <summary>
        /// Creates an instance of this class using the provided projection infos
        /// </summary>
        /// <param name="source">The source projection info</param>
        /// <param name="target">The target projection info</param>
        public DotSpatialMathTransform(ProjectionInfo source, ProjectionInfo target)
        {
            _source = source;
            _target = target;
            _order = new[] {_source, _target};
        }

        /// <summary>
        /// Tests whether this transform does not move any points.
        /// </summary>
        /// <returns></returns>
        public bool Identity()
        {
            return _source.Matches(_target);
        }

        /// <summary>
        /// Gets the derivative of this transform at a point. If the transform does
        /// not have a well-defined derivative at the point, then this function should
        /// fail in the usual way for the DCP. The derivative is the matrix of the
        /// non-translating portion of the approximate affine map at the point. The
        /// matrix will have dimensions corresponding to the source and target
        /// coordinate systems. If the input dimension is M, and the output dimension
        /// is N, then the matrix will have size [M][N]. The elements of the matrix
        /// {elt[n][m] : n=0..(N-1)} form a vector in the output space which is
        /// parallel to the displacement caused by a small change in the m'th ordinate
        /// in the input space.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double[,] Derivative(double[] point)
        {
            throw new NotSupportedException();
        }

        /// <summary>Gets transformed convex hull.</summary>
        /// <remarks>
        /// <para>The supplied ordinates are interpreted as a sequence of points, which generates a convex
        /// hull in the source space. The returned sequence of ordinates represents a convex hull in the
        /// output space. The number of output points will often be different from the number of input
        /// points. Each of the input points should be inside the valid domain (this can be checked by
        /// testing the points' domain flags individually). However, the convex hull of the input points
        /// may go outside the valid domain. The returned convex hull should contain the transformed image
        /// of the intersection of the source convex hull and the source domain.</para>
        /// <para>A convex hull is a shape in a coordinate system, where if two positions A and B are
        /// inside the shape, then all positions in the straight line between A and B are also inside
        /// the shape. So in 3D a cube and a sphere are both convex hulls. Other less obvious examples
        /// of convex hulls are straight lines, and single points. (A single point is a convex hull,
        /// because the positions A and B must both be the same - i.e. the point itself. So the straight
        /// line between A and B has zero length.)</para>
        /// <para>Some examples of shapes that are NOT convex hulls are donuts, and horseshoes.</para>
        /// </remarks>
        /// <param name="points"></param>
        /// <returns></returns>
        public List<double> GetCodomainConvexHull(List<double> points)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets flags classifying domain points within a convex hull.
        /// </summary>
        /// <remarks>
        ///  The supplied ordinates are interpreted as a sequence of points, which
        /// generates a convex hull in the source space. Conceptually, each of the
        /// (usually infinite) points inside the convex hull is then tested against
        /// the source domain. The flags of all these tests are then combined. In
        /// practice, implementations of different transforms will use different
        /// short-cuts to avoid doing an infinite number of tests.
        /// </remarks>
        /// <param name="points"></param>
        /// <returns></returns>
        public DomainFlags GetDomainFlags(List<double> points)
        {
            throw new NotSupportedException();
        }

        /// <summary>Creates the inverse transform of this object.</summary>
        /// <remarks>This method may fail if the transform is not one to one. However, all cartographic projections should succeed.</remarks>
        /// <returns></returns>
        public IMathTransform Inverse()
        {
            return _inverse ?? (_inverse = new DotSpatialMathTransform(_target, _source));
        }

        /// <summary>
        /// Transforms a coordinate point. The passed parameter point should not be modified.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double[] Transform(double[] point)
        {
            var xy = new double[2];
            var numOrdinates = point.Length;
            var z = numOrdinates > 2 ? new double[1] : null;
            var m = numOrdinates > 3 ? new double[1] : null;

            xy[0] = point[0];
            xy[1] = point[1];
            if (numOrdinates > 2) z[0] = point[2];
            if (numOrdinates > 3) z[0] = point[3];

            // Do the reprojection
            Reproject.ReprojectPoints(xy, z, _order[0], _order[1], 0, 1);

            // Set up result
            if (numOrdinates < 3)
                return new[] { xy[0], xy[1] };
            if (numOrdinates < 4)
                return new[] { xy[2], xy[1], z[0] };
            return new[] { xy[0], xy[1], z[0], m[0] };
        }

        /// <summary>
        /// Transforms a a coordinate. The input coordinate remains unchanged.
        /// </summary>
        /// <param name="coordinate">The coordinate to transform</param>
        /// <returns>The transformed coordinate</returns>
        [Obsolete]
        public ICoordinate Transform(ICoordinate coordinate)
        {
            // Set up fields
            var xy = new[] {coordinate.X, coordinate.Y};
            var z = double.IsNaN(coordinate.Z) ? null : new[] {coordinate.Z};
            var m = double.IsNaN(coordinate.M) ? null : new[] {coordinate.M};
            
            //Do the reprojection
            Reproject.ReprojectPoints(xy, z, _order[0], _order[1], 0, 1);

            // Put result in new object
            var res = (ICoordinate) Activator.CreateInstance(coordinate.GetType());
            res.X = xy[0];
            res.Y = xy[1];
            if (z != null) res.Z = z[0];
            if (m != null) res.M = m[0];
            return res;
        }

        /// <summary>
        /// Transforms a a coordinate. The input coordinate remains unchanged.
        /// </summary>
        /// <param name="coordinate">The coordinate to transform</param>
        /// <returns>The transformed coordinate</returns>
        public Coordinate Transform(Coordinate coordinate)
        {
            // Set up fields
            var xy = new[] { coordinate.X, coordinate.Y };
            var z = double.IsNaN(coordinate.Z) ? null : new[] { coordinate.Z };
            //Do the reprojection
            Reproject.ReprojectPoints(xy, z, _order[0], _order[1], 0, 1);

            // Put result in new object
            var res = new Coordinate(xy[0], xy[1]);
            if (z != null) res.Z = z[0];

            // return it
            return res;
        }

        /// <summary>Transforms a list of coordinate point ordinal values.</summary>
        /// <remarks>
        /// This method is provided for efficiently transforming many points. The supplied array
        /// of ordinal values will contain packed ordinal values. For example, if the source
        /// dimension is 3, then the ordinals will be packed in this order (x0,y0,z0,x1,y1,z1 ...).
        /// The size of the passed array must be an integer multiple of DimSource. The returned
        /// ordinal values are packed in a similar way. In some DCPs. the ordinals may be
        /// transformed in-place, and the returned array may be the same as the passed array.
        /// So any client code should not attempt to reuse the passed ordinal values (although
        /// they can certainly reuse the passed array). If there is any problem then the server
        /// implementation will throw an exception. If this happens then the client should not
        /// make any assumptions about the state of the ordinal values.
        /// </remarks>
        /// <param name="points"></param>
        /// <returns></returns>
        public IList<double[]> TransformList(IList<double[]> points)
        {
            var xy = new double[points.Count*2];
            var numOrdinates = points[0].Length;
            var z = numOrdinates > 2 ? new double[points.Count] : null;
            var m = numOrdinates > 3 ? new double[points.Count] : null;

            int i = 0, j = 0;
            foreach (var point in points)
            {
                xy[i++] = point[0];
                xy[i++] = point[1];
                if (numOrdinates > 2) z[j++] = point[2];
            }

            // Do the reprojection
            Reproject.ReprojectPoints(xy, z, _order[0], _order[1], 0, points.Count);

            // Set up result
            var res = new List<double[]>(points.Count);
            j = 0;
            for (i = 0; i < points.Count; i++)
            {
                if (numOrdinates > 3)
                    res.Add(new[] { xy[j++], xy[j++], z[j], m[j] });
                else if (numOrdinates > 2)
                    res.Add(new[] { xy[j++], xy[j++], z[j] });
                else
                    res.Add(new []{ xy[j++], xy[j++] });
            }
            return res;
        }

        /// <summary>Transforms a list of coordinates.</summary>
        /// <remarks>
        /// This method is provided for efficiently transforming many points. The supplied array
        /// of ordinal values will contain packed ordinal values. For example, if the source
        /// dimension is 3, then the ordinals will be packed in this order (x0,y0,z0,x1,y1,z1 ...).
        /// The size of the passed array must be an integer multiple of DimSource. The returned
        /// ordinal values are packed in a similar way. In some DCPs. the ordinals may be
        /// transformed in-place, and the returned array may be the same as the passed array.
        /// So any client code should not attempt to reuse the passed ordinal values (although
        /// they can certainly reuse the passed array). If there is any problem then the server
        /// implementation will throw an exception. If this happens then the client should not
        /// make any assumptions about the state of the ordinal values.
        /// </remarks>
        /// <param name="points"></param>
        /// <returns></returns>
        public IList<Coordinate> TransformList(IList<Coordinate> points)
        {
            var res = new List<Coordinate>(points.Count);
            foreach (var point in points)
                res.Add(Transform(point));
            return res;
        }

        /// <summary>Reverses the transformation</summary>
        public void Invert()
        {
            if (_isInverted)
                _order = new[] {_source, _target};
            else
                _order = new[] { _target, _source};
            _isInverted = !_isInverted;
        }

        /// <summary>
        /// Transforms a coordinate sequence. The input coordinate sequence remains unchanged.
        /// </summary>
        /// <param name="coordinateSequence">The coordinate sequence to transform.</param>
        /// <returns>The transformed coordinate sequence.</returns>
        public ICoordinateSequence Transform(ICoordinateSequence coordinateSequence)
        {
            //if (coordinateSequence)
            
            var xy = new double[coordinateSequence.Count * 2];
            var numOrdinates = coordinateSequence.Dimension;
            var z = numOrdinates > 2 ? new double[coordinateSequence.Count] : null;
            var m = numOrdinates > 3 ? new double[coordinateSequence.Count] : null;

            var j = 0;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                xy[j++] = coordinateSequence.GetX(i);
                xy[j++] = coordinateSequence.GetY(i);
                if (numOrdinates > 2) 
                    z[i] = coordinateSequence.GetOrdinate(i, Ordinate.Z);
                if (numOrdinates > 3) 
                    m[i] = coordinateSequence.GetOrdinate(i, Ordinate.M);
            }

            // Do the reprojection
            Reproject.ReprojectPoints(xy, z, _order[0], _order[1], 0, coordinateSequence.Count);

            // Set up result
            j = 0;
            var res = (ICoordinateSequence)coordinateSequence.Clone();
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                res.SetOrdinate(i, Ordinate.X, xy[j++]);
                res.SetOrdinate(i, Ordinate.Y, xy[j++]);
                if (numOrdinates > 2)
                    res.SetOrdinate(i, Ordinate.Z, z[i]);
                else if (numOrdinates > 3)
                    res.SetOrdinate(i, Ordinate.M, m[i]);
            }

            // return it
            return res;
        }

        /// <summary>Gets the dimension of input points.</summary>
        public int DimSource
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>Gets the dimension of output points.</summary>
        public int DimTarget
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>Gets a Well-Known text representation of this object.</summary>
        public string WKT
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>Gets an XML representation of this object.</summary>
        public string XML
        {
            get { throw new NotSupportedException(); }
        }
    }
}