// Copyright 2011 - Felix Obermaier (www.ivv-aachen.de)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Class that warps one path to another path, e.g. a pattern to a linestring
    /// </summary>
    internal static class WarpPathToPath
    {
        internal class GraphSegment
        {
            internal readonly PointF First;
            internal readonly PointF Second;
            private readonly float _startOffset;
            private readonly double _ndx, _ndy;
            private readonly double _length;

            internal GraphSegment(float startOffset, PointF first, PointF second)
            {
                _startOffset = startOffset;
                First = first;
                Second = second;
                double dx = second.X - first.X;
                double dy = second.Y - first.Y;
                _length = (float)Math.Sqrt(dx * dx + dy * dy);
                _ndx = dx / _length;
                _ndy = dy / _length;
            }

            internal float DX { get { return (float)_ndx; } }
            internal float DY { get { return (float)_ndy; } }
            internal float Length { get { return (float)_length; } }

            internal PointF GetLinePoint(float ordinateX)
            {
                ordinateX -= _startOffset;
                return new PointF(First.X + (float)(ordinateX * _ndx),
                                  First.Y + (float)(ordinateX * _ndy));
            }
        }

        /// <summary>
        /// Warps one path to the flattened (<see cref="GraphicsPath.Flatten()"/>) version of another path.
        /// This comes in handy for
        /// <list type="Bullet">
        /// <item>Linestyles that cannot be created with available <see cref="Pen"/>-properties</item>
        /// <item>Warping Text to curves</item>
        /// <item>...</item>
        /// </list>
        /// </summary>
        /// <param name="pathToWarpTo">The path to warp to. This path is flattened before being used, so there is no need to call <see cref="GraphicsPath.Flatten()"/> prior to this function call.</param>
        /// <param name="pathToWarp">The path to warp</param>
        /// <param name="isPattern">Defines whether <paramref name="pathToWarp"/> is a pattern or not. If <paramref name="pathToWarp"/> is a pattern, it is repeated until it has the total length of <see paramref="pathToWarpTo"/></param>
        /// <param name="interval">The interval in which the pattern should be repeated</param>
        /// <returns>Warped <see cref="GraphicsPath"/></returns>
        /// <exception cref="ArgumentNullException">If either pathToWarpTo or pathToWarp is null</exception>
        public static GraphicsPath Warp(GraphicsPath pathToWarpTo, GraphicsPath pathToWarp, bool isPattern, float interval)
        {
            //Test for valid arguments
            if (pathToWarpTo == null)
                throw new ArgumentNullException("pathToWarpTo");
            if (pathToWarp == null)
                throw new ArgumentNullException("pathToWarp");

            //Remove all curves from path to warp to, get total length.
            SortedList<float, GraphSegment> edges;
            pathToWarpTo.Flatten();
            Double pathLength = GetPathLength(pathToWarpTo, out edges);

            //Prepare path to warp
            pathToWarp = PreparePathToWarp(pathToWarp, isPattern, pathLength, interval);
            if (pathToWarp.PointCount == 0) return null;
            GraphicsPath warpedPath = new GraphicsPath(pathToWarp.FillMode);
            using (GraphicsPathIterator iter = new GraphicsPathIterator(pathToWarp))
            {
                GraphicsPath subPath = new GraphicsPath();
                int currentIndex = 0;
                if (iter.SubpathCount > 1)
                {
                    bool isClosed;
                    while (iter.NextSubpath(subPath, out isClosed) > 0)
                    {
                        GraphicsPath warpedSubPath = WarpSubpath(subPath, edges, ref currentIndex);
                        if (isClosed) warpedSubPath.CloseFigure();
                        warpedPath.AddPath(warpedSubPath, true);
                        warpedPath.SetMarkers();
                    }
                }
                else
                {
                    warpedPath = WarpSubpath(pathToWarp, edges, ref currentIndex);
                }
            }
            return warpedPath;
        }

        private static GraphicsPath WarpSubpath(GraphicsPath pathToWarp, SortedList<float, GraphSegment> edges, ref int currentIndex)
        {
            //midpoint of figure
            float minX = float.MaxValue, maxX = float.MinValue;
            PointF[] pathPoints = pathToWarp.PathPoints;
            for (int i = 0; i < pathToWarp.PointCount; i++)
            {
                minX = Math.Min(minX, pathPoints[i].X);
                maxX = Math.Max(maxX, pathPoints[i].X);
            }
            float ordinateX = (minX + maxX) * 0.5f;

            //seek pathToWarpTo segment
            while (true)
            {
                if (edges.Keys[currentIndex] <= ordinateX &&
                    ((currentIndex == edges.Keys.Count - 1) || ordinateX < edges.Keys[currentIndex + 1]))
                {
                    break;
                }
                if (ordinateX < edges.Keys[currentIndex])
                {
                    if (currentIndex == 0) break;
                    currentIndex--;
                }
                else
                {
                    if (currentIndex == edges.Count + 1) break;
                    currentIndex++;
                }
            }

            GraphSegment s = edges.Values[currentIndex];
            float dy = s.DX;
            float dx = s.DY;
            PointF[] warpedPathPoints = new PointF[pathToWarp.PointCount];
            for (int i = 0; i < pathToWarp.PointCount; i++)
            {
                float ptX = pathPoints[i].X;
                float ptY = pathPoints[i].Y;

                PointF linePt = s.GetLinePoint(ptX);
                ptX = -ptY * dx;
                ptY = ptY * dy;

                warpedPathPoints[i] = new PointF(linePt.X + ptX, linePt.Y + ptY);
            }
            return new GraphicsPath(warpedPathPoints, pathToWarp.PathTypes, pathToWarp.FillMode);
        }

        /// <summary>
        /// Calculates the length of all segments in the Path, visible or not
        /// </summary>
        /// <param name="path">A flattened <see cref="GraphicsPath"/></param>
        /// <param name="edges">A <see cref="SortedList{TKey, TValue}"/> containing edges of the path. </param>
        /// <returns>the length</returns>
        internal static double GetPathLength(GraphicsPath path, out SortedList<float, GraphSegment> edges)
        {
            float length = 0;
            edges = new SortedList<float, GraphSegment>(path.PointCount - 2);
            PointF ptLast = new PointF();
            PointF ptClose = new PointF();
            for (int i = 0; i < path.PointCount; i++)
            {
                var ptCurr = path.PathPoints[i];
                if (path.PathTypes[i] == (byte)PathPointType.Start)
                    ptClose = ptCurr;
                if (ptCurr.Equals(ptLast)) continue;

                if (!ptLast.IsEmpty)
                {

                    var gs = new GraphSegment(length, ptLast, ptCurr);
                    edges.Add(length, gs);
                    length += gs.Length;
                    if ((path.PathTypes[i] & (byte)PathPointType.CloseSubpath) == (byte)PathPointType.CloseSubpath)
                    {
                        gs = new GraphSegment(length, ptCurr, ptClose);
                        edges.Add(length, gs);
                        length += gs.Length;
                        ptLast = ptClose;
                    }
                    else
                    {
                        ptLast = ptCurr;
                    }
                }
                else
                {
                    ptLast = ptCurr;
                }
            }
            return length;
        }

        /// <summary>
        /// Prepares a text path to be warped to a path
        /// <para>This operation performs the following tasks</para>
        /// <list type="Bullet">
        /// <item>Check if the <paramref name="path"/> length is less than <paramref name="totalPathLength"/> or <paramref name="ignoreLength"/> is set to <c>true</c></item></list>
        /// <item>Translate <paramref name="path"/> according to horizontal alignment</item>
        /// </summary>
        /// <param name="path">The text path</param>
        /// <param name="totalPathLength">The total length of the path to warp to</param>
        /// <param name="ignoreLength">value indicating if the text path should only be handled if the path fits the total length</param>
        /// <param name="format">The string format</param>
        /// <returns>The prepared text path</returns>
        internal static GraphicsPath PrepareTextPathToWarp(GraphicsPath path, Double totalPathLength, bool ignoreLength, StringFormat format)
        {
            var rect = path.GetBounds();

            double maxX = rect.Right;
            double minX = rect.Left;

            //Check if text path fits or is really wanted
            double len = maxX - minX;
            if (len > totalPathLength && !ignoreLength)
                return null;

            //Offset for center
            double xStart;
            switch (format.Alignment)
            {
                default:
                    xStart = (totalPathLength) * 0.5d;
                    break;
                case StringAlignment.Near:
                    xStart = 0;
                    break;
                case StringAlignment.Far:
                    xStart = totalPathLength;
                    break;
            }
            path.Transform(new Matrix(1f, 0f, 0f, 1f, (float)xStart, 0f));

            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isPattern"></param>
        /// <param name="totalPathLength"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        internal static GraphicsPath PreparePathToWarp(GraphicsPath path, bool isPattern, Double totalPathLength, Double interval)
        {
            var rect = path.GetBounds();

            double maxX = rect.Right;
            double minX = rect.Left;

            /*
            PointF[] pathPoints = path.PathPoints;
            for (int i = 0; i < path.PointCount; i++)
            {
                minX = Math.Min(minX, pathPoints[i].X);
                maxX = Math.Max(maxX, pathPoints[i].X);
            }
            */

            //if path has x-ordinates less than 0, we need to shift the path to values greater than 0.
            if (minX < 0d)
            {
                Matrix m = new Matrix(1f, 0f, 0f, 1f, (float)minX, 0f);
                path.Transform(m);
                maxX -= minX;
            }

            //Complete pattern
            if (isPattern) // && maxX < totalPathLength)
            {
                GraphicsPath patternPath = new GraphicsPath();
                double pathLength = maxX;
                interval = interval - pathLength;
                if (interval < pathLength) interval = pathLength;
                Matrix m = new Matrix(1f, 0f, 0f, 1f, (float)interval, 0f);
                while (maxX < totalPathLength)
                {
                    patternPath.StartFigure();
                    patternPath.AddPath(path, true);
                    maxX += interval;
                    path.Transform(m);
                }
                GraphicsPath clippedPattern = ClipPath(path, (float)totalPathLength);
                if (clippedPattern.PointCount > 0)
                    patternPath.AddPath(clippedPattern, false);
                return patternPath;
            }
            return path;
        }

        /// <summary>
        /// Clip the path to a provided length. This is tricky
        /// </summary>
        /// <param name="patternPath">the path to be clipped</param>
        /// <param name="totalPathLength">the maximum length of the path</param>
        /// <returns></returns>
        internal static GraphicsPath ClipPath(GraphicsPath patternPath, float totalPathLength)
        {
            patternPath.Flatten();
            var returnPath = new GraphicsPath(patternPath.FillMode);

            using (var iter = new GraphicsPathIterator(patternPath))
            {
                var pathPoints = new List<PointF>(patternPath.PointCount);
                var pathTypes = new List<byte>(patternPath.PointCount);
                var lastPoint = new PointF();
                var lastValid = true;
                for (int i = 0; i < patternPath.PointCount; i++)
                {
                    //current point is valid
                    if (CheckMaxX(patternPath.PathPoints[i].X, totalPathLength))
                    {
                        //last point was not valid, we need to add intersection point first
                        if (!lastValid)
                        {
                            PointF borderIntersectionPoint = BorderIntersectionPoint(lastPoint,
                                                                                     patternPath.PathPoints[i],
                                                                                     totalPathLength);
                            if (!borderIntersectionPoint.Equals(patternPath.PathPoints[i]))
                            {
                                pathPoints.Add(borderIntersectionPoint);
                                pathTypes.Add((byte)PathPointType.Line);
                            }
                            lastValid = true;
                        }
                        //Add the valid point
                        pathPoints.Add(patternPath.PathPoints[i]);
                        pathTypes.Add(patternPath.PathTypes[i]);
                    }
                    else
                    {
                        //last point was valid
                        if (lastValid && i > 0)
                        {
                            pathPoints.Add(BorderIntersectionPoint(lastPoint, patternPath.PathPoints[i], totalPathLength));
                            pathTypes.Add(patternPath.PathTypes[i]);
                        }
                        lastValid = false;
                    }
                    lastPoint = patternPath.PathPoints[i];
                }

                //All or part of the subpath is valid
                if (pathPoints.Count > 0)
                {
                    //remove last point, if dangeling
                    if (pathTypes[pathTypes.Count - 1] == 0)
                    {
                        pathPoints.RemoveAt(pathTypes.Count - 1);
                        pathTypes.RemoveAt(pathTypes.Count - 1);
                    }
                }

                if (pathPoints.Count > 0)
                {
                    GraphicsPath addPath = new GraphicsPath(pathPoints.ToArray(), pathTypes.ToArray(),
                                                            patternPath.FillMode);
                    returnPath.AddPath(addPath, true);
                }
            }
            return returnPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastPoint"></param>
        /// <param name="pathPoint"></param>
        /// <param name="totalPathLength"></param>
        /// <returns></returns>
        private static PointF BorderIntersectionPoint(PointF lastPoint, PointF pathPoint, float totalPathLength)
        {
            Single dxBorder = totalPathLength - lastPoint.X;
            Single dx = pathPoint.X - lastPoint.X;
            Single dy = pathPoint.Y - lastPoint.Y;

            Single fraction = dxBorder / dx;
            return new PointF(lastPoint.X + fraction * dx, lastPoint.Y + fraction * dy);
        }

        private static bool CheckMaxX(Single x, Single maxX)
        {
            return x <= maxX;
        }

        /// <summary>
        /// Renders text along the path
        /// </summary>
        /// <param name="self">The graphics object</param>
        /// <param name="halo">The pen to render the halo outline</param>
        /// <param name="fill">The brush to fill the text</param>
        /// <param name="text">The text to render</param>
        /// <param name="fontFamily">The font family to use</param>
        /// <param name="style">The style</param>
        /// <param name="emSize">The size</param>
        /// <param name="format">The format</param>
        /// <param name="ignoreLength"></param>
        /// <param name="path"></param>
        public static void DrawString(this Graphics self, Pen halo, Brush fill, string text, FontFamily fontFamily, int style, float emSize, StringFormat format, bool ignoreLength, GraphicsPath path)
        {
            if (path == null || path.PointCount == 0)
                return;
            
            var gp = new GraphicsPath();
            gp.AddString(text, fontFamily, style, emSize, new Point(0, 0), format);

            SortedList<float, GraphSegment> edges;
            var totalLength = GetPathLength(path, out edges);

            var warpedPath = PrepareTextPathToWarp(gp, totalLength, ignoreLength, format);
            
            if (warpedPath == null)
                return;

            var wp = Warp(path, warpedPath, false, 0f);
            if (wp != null)
            {
                if (halo != null)
                    self.DrawPath(halo, wp);
                if (fill != null)
                    self.FillPath(fill, wp);
            }
        }

    }
}
