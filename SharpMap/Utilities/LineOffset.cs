using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Methods for calculating line offsets
    /// </summary>
    public class LineOffset
    {
        
        /// <summary>
        /// Offset a Linestring the given amount perpendicular to the line
        /// For example if a line should be drawn 10px to the right of its original position
        ///
        /// Positive offset offsets right
        /// Negative offset offsets left
        /// </summary>
        /// <param name="lineCoordinates">LineString</param>
        /// <param name="offset">offset amount</param>
        /// <returns>Array of coordinates for the offseted line</returns>
        public static PointF[] OffsetPolyline(PointF[] lineCoordinates, float offset)
        {
            List<PointF> retPoints = new List<PointF>();
            PointF old_pt, old_diffdir = PointF.Empty, old_offdir = PointF.Empty;
            int idx = 0;
            bool first = true;

            /* saved metrics of the last processed point */
            if (lineCoordinates.Length > 0)
            {
                old_pt = lineCoordinates[0];
                for (int j = 1; j < lineCoordinates.Length; j++)
                {
                    PointF pt = lineCoordinates[j]; /* place of the point */
                    PointF diffdir = point_norm(point_diff(pt, old_pt)); /* direction of the line */
                    PointF offdir = point_rotz90(diffdir);/* direction where the distance between the line and the offset is measured */
                    PointF offPt;
                    if (first)
                    {
                        first = false;
                        offPt = point_sum(old_pt, point_mul(offdir, offset));
                    }
                    else /* middle points */
                    {
                        /* curve is the angle of the last and the current line's direction (supplementary angle of the shape's inner angle) */
                        double sin_curve = point_cross(diffdir, old_diffdir);
                        double cos_curve = point_cross(old_offdir, diffdir);
                        if ((-1.0) * CURVE_SIN_LIMIT < sin_curve && sin_curve < CURVE_SIN_LIMIT)
                        {
                            offPt = point_sum(old_pt, point_mul(point_sum(offdir, old_offdir), 0.5 * offset));
                        }
                        else
                        {
                            double base_shift = -1.0 * (1.0 + cos_curve) / sin_curve;
                            offPt = point_sum(old_pt, point_mul(point_sum(point_mul(diffdir, base_shift), offdir), offset));
                        }
                    }

                    retPoints.Add(offPt);

                    old_pt = pt;
                    old_diffdir = diffdir;
                    old_offdir = offdir;
                }

                /* last point */
                if (!first)
                {
                    PointF offpt = point_sum(old_pt, point_mul(old_offdir, offset));
                    retPoints.Add(offpt);
                    idx++;
                }
            }
            return retPoints.ToArray();
        }

        #region helper_methods
        static PointF point_norm(PointF a)
        {
            double lenmul;
            PointF retv = new PointF();
            if (a.X == 0 && a.Y == 0)
                return a;

            lenmul = 1.0 / Math.Sqrt(point_abs2(a));  /* this seems to be the costly operation */

            retv.X = (float)(a.X * lenmul);
            retv.Y = (float)(a.Y * lenmul);

            return retv;
        }
        static double point_abs2(PointF a)
        {
            return a.X * a.X + a.Y * a.Y;
        }
        /* rotate a vector 90 degrees */
        static PointF point_rotz90(PointF a)
        {
            double nx = -1.0 * a.Y, ny = a.X;
            PointF retv = new PointF();
            retv.X = (float)nx; retv.Y = (float)ny;
            return retv;
        }

        /* vector multiply */
        static PointF point_mul(PointF a, double b)
        {
            PointF retv = new PointF((float)(a.X * b), (float)(a.Y * b));
            return retv;
        }
        static PointF point_sum(PointF a, PointF b)
        {
            PointF retv = new PointF(a.X + b.X, a.Y + b.Y);
            return retv;
        }
        static PointF point_diff(PointF a, PointF b)
        {
            PointF retv = new PointF(a.X - b.X, a.Y - b.Y);
            return retv;
        }
        static double point_cross(PointF a, PointF b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
        #endregion

        static readonly double CURVE_SIN_LIMIT = 0.3;
    }
}
