using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// SharpMap's Cohen-Sutherland line clipping algorithm
    /// </summary>
    /// <remarks>Inspired by <see href="http://en.wikipedia.org/wiki/Line_clipping"/></remarks>
    public class CohenSutherlandLineClipping
    {
        [Flags]
        private enum OutsideClipCodes
        {
            Inside = 0,
            Left = 1,
            Right = 2,
            Bottom = 4,
            Top = 8
        }

        private readonly double _xmin, _xmax, _ymin, _ymax ;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="xmin">The minimum x-ordinate value</param>
        /// <param name="ymin">The minimum y-ordinate value</param>
        /// <param name="xmax">The maximum x-ordinate value</param>
        /// <param name="ymax">The maximum y-ordinate value</param>
        public CohenSutherlandLineClipping(double xmin, double ymin, double xmax, double ymax)
        {
            _xmin = xmin;
            _ymin = ymin;
            _xmax = xmax;
            _ymax = ymax;
        }

        private OutsideClipCodes ComputeClipCode(Coordinate point, out double x, out double y)
        {
            x = point.X;
            y = point.Y;
            return ComputeClipCode(x, y);
        }

        private OutsideClipCodes ComputeClipCode(double x, double y)
        {
            var result = OutsideClipCodes.Inside;
            if (x < _xmin) result |= OutsideClipCodes.Left;
            if (x > _xmax) result |= OutsideClipCodes.Right;
            if (y < _ymin) result |= OutsideClipCodes.Bottom;
            if (y > _ymax) result |= OutsideClipCodes.Top;
            return result;
        }

        /// <summary>
        /// Clips a <see cref="ILineString"/> to the bounding box defined by <see cref="CohenSutherlandLineClipping(double,double,double,double)"/>.
        /// </summary>
        /// <param name="lineString">The line string to clip</param>
        /// <returns>A (possibly multi) line string</returns>
        public IMultiLineString ClipLineString(ILineString lineString)
        {
            //Factory
            var factory = lineString.Factory;

            //List of line strings that make up the multi line string result
            var lineStrings = new List<ILineString>();

            //list of clipped vertices for current pass
            var clippedVertices = new List<Coordinate>();

            //vertices of current line string
            var vertices = new List<Coordinate>(lineString.Coordinates);
            var count = vertices.Count;

            //Compute starting clipcode
            double x0, y0;
            OutsideClipCodes oc0Initial;
            var oc0 = oc0Initial = ComputeClipCode(vertices[0], out x0, out y0);
            
            //Point is inside => add it to the list
            if (oc0 == OutsideClipCodes.Inside)
                clippedVertices.Add( vertices[0] );

            double x1Old = double.NaN, y1Old = double.NaN;

            for (var i = 1; i < count; i++)
            {
                double x1, y1;
                OutsideClipCodes oc1Initial;
                var oc1 = oc1Initial = ComputeClipCode(vertices[i], out x1, out y1);
                var x1Orig = x1;
                var y1Orig = y1;

                var accept = false;

                while (true)
                {
                    if ((oc0 | oc1) == OutsideClipCodes.Inside)
                    {
                        // Bitwise OR is 0. Trivially accept and get out of loop
                        accept = true;
                        break;
                    }

                    if ((oc0 & oc1) != OutsideClipCodes.Inside)
                    {
                        // Bitwise AND is not 0. Trivially reject and get out of loop
                        break;
                    }

                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge
                    double x = double.NaN, y = double.NaN;

                    // At least one endpoint is outside the clip rectangle; pick it.
                    var ocOut = oc0 != OutsideClipCodes.Inside ? oc0 : oc1;

                    // Now find the intersection point;
                    // use formulas y = y0 + slope * (x - x0), x = x0 + (1 / slope) * (y - y0)
                    if ((ocOut & OutsideClipCodes.Top) == OutsideClipCodes.Top)
                    {
                        // point is above the clip rectangle
                        x = x0 + (x1 - x0)*(_ymax - y0)/(y1 - y0);
                        y = _ymax;
                    }
                    else if ((ocOut & OutsideClipCodes.Bottom) == OutsideClipCodes.Bottom)
                    {
                        // point is below the clip rectangle
                        x = x0 + (x1 - x0)*(_ymin - y0)/(y1 - y0);
                        y = _ymin;
                    }
                    else if ((ocOut & OutsideClipCodes.Right) == OutsideClipCodes.Right)
                    {
                        // point is to the right of clip rectangle
                        y = y0 + (y1 - y0)*(_xmax - x0)/(x1 - x0);
                        x = _xmax;
                    }
                    else if ((ocOut & OutsideClipCodes.Left) == OutsideClipCodes.Left)
                    {
                        // point is to the left of clip rectangle
                        y = y0 + (y1 - y0)*(_xmin - x0)/(x1 - x0);
                        x = _xmin;
                    }
                    // Now we move outside point to intersection point to clip
                    // and get ready for next pass.
                    if (oc0 == ocOut)
                    {
                        x0 = x;
                        y0 = y;
                        oc0 = ComputeClipCode(x, y);
                    }
                    else
                    {
                        x1 = x;
                        y1 = y;
                        oc1 = ComputeClipCode(x, y);
                    }
                }
                
                if (accept)
                {
                    if (oc0Initial != oc0)
                        clippedVertices.Add(new Coordinate(x0, y0));
                    
                    if (x1Old != x1 || y1Old != y1)
                        clippedVertices.Add(new Coordinate(x1, y1));

                    if (oc1Initial != OutsideClipCodes.Inside)
                    {
                        if (clippedVertices.Count > 0)
                        {
                            lineStrings.Add(factory.CreateLineString(clippedVertices.ToArray()));
                            clippedVertices = new List<Coordinate>();
                        }
                    }
                }
                x0 = x1Old = x1Orig;
                y0 = y1Old = y1Orig;
                oc0 = oc0Initial = oc1Initial;
            }

            if (clippedVertices.Count > 0)
                lineStrings.Add(factory.CreateLineString(clippedVertices.ToArray()));

            return factory.CreateMultiLineString(lineStrings.ToArray());
        }


        /// <summary>
        /// Clips a <see cref="IMultiLineString"/> to the bounding box defined by <see cref="CohenSutherlandLineClipping(double,double,double,double)"/>.
        /// </summary>
        /// <param name="lineStrings">The multi-line string to clip</param>
        /// <returns>A (possibly multi) line string</returns>
        public IMultiLineString ClipLineString(IMultiLineString lineStrings)
        {
            var clippedLineStringList = new List<ILineString>();


            for (var i = 0; i < lineStrings.NumGeometries; i++)
            {
                var s = (ILineString) lineStrings.GetGeometryN(i);
                var clippedLineStrings = ClipLineString(s);
                for (var j = 0; j < clippedLineStrings.NumGeometries; j++)
                {
                    var clippedLineString = (ILineString)clippedLineStrings.GetGeometryN(j);
                    clippedLineStringList.Add(clippedLineString);
                }
            }

            return lineStrings.Factory.CreateMultiLineString(clippedLineStringList.ToArray());
        }

    }
}