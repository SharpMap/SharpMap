// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SharpMap.Geometries
{
	/// <summary>
	/// A LinearRing is a LineString that is both closed and simple.
	/// </summary>
	public class LinearRing : LineString
	{
		/// <summary>
		/// Initializes an instance of a LinearRing from a set of vertices
		/// </summary>
		/// <param name="vertices"></param>
		public LinearRing(Collection<Point> vertices)
			: base(vertices)
		{
		}

		/// <summary>
		/// Initializes an instance of a LinearRing
		/// </summary>
		public LinearRing() : base()
		{
		}

        /// <summary>
        /// Initializes an instance of a LinearRing
        /// </summary>
        /// <param name="points"></param>
        public LinearRing(List<double[]> points)
            : base(points)
        {
        }	

		#region ICloneable Members

		/// <summary>
		/// Return a copy of this geometry
		/// </summary>
		/// <returns>Copy of Geometry</returns>
		public new LinearRing Clone()
		{
			LinearRing l = new LinearRing();
			for (int i = 0; i < this.Vertices.Count; i++)
				l.Vertices.Add(this.Vertices[i].Clone());
			return l;
		}

		#endregion

		/// <summary>
		/// Tests whether a ring is oriented counter-clockwise.
		/// </summary>
		/// <returns>Returns true if ring is oriented counter-clockwise.</returns>
		public bool IsCCW()
		{
			Point hip, p, prev, next;
			int hii, i;
			int nPts = this.Vertices.Count;

			// check that this is a valid ring - if not, simply return a dummy value
			if (nPts < 4) return false;

			// algorithm to check if a Ring is stored in CCW order
			// find highest point
			hip = this.Vertices[0];
			hii = 0;
			for (i = 1; i < nPts; i++)
			{
				p = this.Vertices[i];
				if (p.Y > hip.Y)
				{
					hip = p;
					hii = i;
				}
			}
			// find points on either side of highest
			int iPrev = hii - 1;
			if (iPrev < 0) iPrev = nPts - 2;
			int iNext = hii + 1;
			if (iNext >= nPts) iNext = 1;
			prev = this.Vertices[iPrev];
			next = this.Vertices[iNext];
			// translate so that hip is at the origin.
			// This will not affect the area calculation, and will avoid
			// finite-accuracy errors (i.e very small vectors with very large coordinates)
			// This also simplifies the discriminant calculation.
			double prev2x = prev.X - hip.X;
			double prev2y = prev.Y - hip.Y;
			double next2x = next.X - hip.X;
			double next2y = next.Y - hip.Y;
			// compute cross-product of vectors hip->next and hip->prev
			// (e.g. area of parallelogram they enclose)
			double disc = next2x * prev2y - next2y * prev2x;
			// If disc is exactly 0, lines are collinear.  There are two possible cases:
			//	(1) the lines lie along the x axis in opposite directions
			//	(2) the line lie on top of one another
			//  (2) should never happen, so we're going to ignore it!
			//	(Might want to assert this)
			//  (1) is handled by checking if next is left of prev ==> CCW

			if (disc == 0.0)
			{
				// poly is CCW if prev x is right of next x
				return (prev.X > next.X);
			}
			else
			{
				// if area is positive, points are ordered CCW
				return (disc > 0.0);
			}

		}

		/// <summary>
		/// Returns the area of the LinearRing
		/// </summary>
		public double Area
		{
			get {
				if (this.Vertices.Count < 3)
					return 0;
				double sum = 0;
				double ax = this.Vertices[0].X;
				double ay = this.Vertices[0].Y;
				for(int i = 1; i < this.Vertices.Count - 1; i++)
				{
					double bx = this.Vertices[i].X;
					double by = this.Vertices[i].Y;
					double cx = this.Vertices[i + 1].X;
					double cy = this.Vertices[i + 1].Y;
					sum += ax * by - ay * bx +
						ay * cx - ax * cy +
						bx * cy - cx * by;
				}
				return Math.Abs(-sum / 2);
			}
		}
		/// <summary>
    /// Returns true of the Point 'p' is within the instance of this ring
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IsPointWithin(Point p)
    {
      bool c = false;
      for (int i = 0, j = this.Vertices.Count - 1; i < this.Vertices.Count; j = i++)
      {
        if ((((this.Vertices[i].Y <= p.Y) && (p.Y < this.Vertices[j].Y)) ||
             ((this.Vertices[j].Y <= p.Y) && (p.Y < this.Vertices[i].Y))) &&
            (p.X < (this.Vertices[j].X - this.Vertices[i].X) * (p.Y - this.Vertices[i].Y) / (this.Vertices[j].Y - this.Vertices[i].Y) + this.Vertices[i].X))
          c = !c;
      }
      return c;
    }
	}
}
