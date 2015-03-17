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
using System.Drawing;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.Reflection;
using GeoAPI.Geometries;
using SharpMap.Rendering.Symbolizer;
using SharpMap.Styles;
using SharpMap.Utilities;
using Point = GeoAPI.Geometries.Coordinate;
using System.Runtime.CompilerServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using SharpMap.Layers.Styles;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Brush = SharpDX.Direct2D1.Brush;
using GdiBitmap = System.Drawing.Bitmap;

namespace SharpMap.Rendering
{
    /// <summary>
    /// This class renders individual geometry features to a graphics object using the settings of a map object.
    /// </summary>
    internal static class SharpDXVectorRenderer
    {
        internal const float ExtremeValueLimit = 1E+8f;
        internal const float NearZero = 1E-30f; // 1/Infinity

        internal static readonly GdiBitmap DefaultSymbol =
            (GdiBitmap)
                GdiBitmap.FromStream(
                    Assembly.GetAssembly(typeof (Map))
                        .GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png"));

        /// <summary>
        /// Renders a MultiLineString to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="lines">MultiLineString to be rendered</param>
        /// <param name="pen">Pen style used for rendering</param>
        /// <param name="map">Map reference</param>
        /// <param name="offset">Offset by which line will be moved to right</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DrawMultiLineString(RenderTarget g, Factory factory, IMultiLineString lines, Brush pen, float penWidth, StrokeStyle penStrokeStyle, Map map, float offset)
        {
            for (var i = 0; i < lines.NumGeometries; i++)
            {
                var line = (ILineString)lines[i];
                DrawLineString(g, factory, line, pen, penWidth, penStrokeStyle, map, offset);
            }
        }

        /// <summary>
        /// Offset drawn linestring by given pixel width
        /// </summary>
        /// <param name="points"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal static Vector2[] OffsetRight(Vector2[] points, float offset)
        {
            int length = points.Length;
            var newPoints = new Vector2[(length - 1) * 2];

            float space = (offset * offset / 4) + 1;

            //if there are two or more points
            if (length >= 2)
            {
                var counter = 0;
                float x = 0, y = 0;
                for (var i = 0; i < length - 1; i++)
                {
                    var b = -(points[i + 1].X - points[i].X);
                    if (b != 0)
                    {
                        var a = points[i + 1].Y - points[i].Y;
                        var c = a / b;
                        y = 2 * (float)Math.Sqrt(space / (c * c + 1));
                        y = b < 0 ? y : -y;
                        x = (c * y);

                        if (offset < 0)
                        {
                            y = -y;
                            x = -x;
                        }

                        newPoints[counter] = new Vector2(points[i].X + x, points[i].Y + y);
                        newPoints[counter + 1] = new Vector2(points[i + 1].X + x, points[i + 1].Y + y);
                    }
                    else
                    {
                        newPoints[counter] = new Vector2(points[i].X + x, points[i].Y + y);
                        newPoints[counter + 1] = new Vector2(points[i + 1].X + x, points[i + 1].Y + y);
                    }
                    counter += 2;
                }

                return newPoints;
            }
            return points;
        }

        /// <summary>
        /// Renders a LineString to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="line">LineString to render</param>
        /// <param name="pen">Pen style used for rendering</param>
        /// <param name="map">Map reference</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DrawLineString(RenderTarget g, Factory factory, ILineString line, Brush pen, float penWidth, StrokeStyle penStrokeStyle, Map map)
        {
            DrawLineString(g, factory, line, pen, penWidth, penStrokeStyle, map, 0);
        }
        /// <summary>
        /// Renders a LineString to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="line">LineString to render</param>
        /// <param name="pen">Pen style used for rendering</param>
        /// <param name="map">Map reference</param>
        /// <param name="offset">Offset by which line will be moved to right</param>
        public static void DrawLineString(RenderTarget g, Factory factory, ILineString line, Brush pen, float penWidth, StrokeStyle penStrokeStyle, Map map, float offset)
        {
            var points = TransformToImage(line, map);
            if (points.Length > 1)
            {
                using (var geom = new PathGeometry(factory))
                {
                    
                    using (var gs = geom.Open())
                    {
                        gs.BeginFigure(points[0], FigureBegin.Filled);
                        gs.AddLines(points);
                        gs.EndFigure(FigureEnd.Open);

                        gs.Close();
                    }

                    g.DrawGeometry(geom, pen, penWidth, penStrokeStyle);
                }
            }
        }

       /// <summary>
        /// Transforms from world coordinate system (WCS) to image coordinates
        /// NOTE: This method DOES NOT take the MapTransform property into account (use <see cref="Map.WorldToImage(GeoAPI.Geometries.Coordinate,bool)"/> instead)
        /// </summary>
        /// <param name="p">Point in WCS</param>
        /// <param name="map">Map reference</param>
        /// <returns>Point in image coordinates</returns>
        private static Vector2 TransformToImage(Coordinate p, Map map)
        {
            //if (map.MapTransform != null && !map.MapTransform.IsIdentity)
            //	map.MapTransform.TransformPoints(new System.Drawing.PointF[] { p });
            if (p.IsEmpty())
                return new Vector2(float.NaN);

            var height = (map.Zoom * map.Size.Height) / map.Size.Width;
            var left = map.Center.X - map.Zoom * 0.5;
            var top = map.Center.Y + height * 0.5 * map.PixelAspectRatio;

            var x = (float)((p.X - left) / map.PixelWidth);
            var y = (float)((top - p.Y) / map.PixelHeight);

            if (double.IsNaN(x) || double.IsNaN(y))
                return new Vector2(float.NaN);

            return new Vector2(x, y);
        }

        private static Vector2[] TransformToImage(ILineString line, Map map)
        {
            var height = (map.Zoom * map.Size.Height) / map.Size.Width;
            var left = map.Center.X - map.Zoom * 0.5;
            var top = map.Center.Y + height * 0.5 * map.PixelAspectRatio;

            var cs = line.CoordinateSequence;
            var res = new Vector2[cs.Count];
            for (var i = 0; i < cs.Count; i++)
            {
                var p = cs.GetCoordinate(i);
                var x = (float)((p.X - left) / map.PixelWidth);
                var y = (float)((top - p.Y) / map.PixelHeight);
                res [i] = new Vector2(x, y);
            }
            return res;
        }
        private static Vector2 TransformToImage(ILineString line, Map map, out Vector2[] points)
        {
            var height = (map.Zoom * map.Size.Height) / map.Size.Width;
            var left = map.Center.X - map.Zoom * 0.5;
            var top = map.Center.Y + height * 0.5 * map.PixelAspectRatio;

            var cs = line.CoordinateSequence;

            var p = cs.GetCoordinate(0);
            var x = (float)((p.X - left) / map.PixelWidth);
            var y = (float)((top - p.Y) / map.PixelHeight);
            var res = new Vector2(x, y);

            points = new Vector2[cs.Count - 1];
            for (var i = 1; i < cs.Count; i++)
            {
                p = cs.GetCoordinate(i);
                x = (float)((p.X - left) / map.PixelWidth);
                y = (float)((top - p.Y) / map.PixelHeight);
                points[i-1] = new Vector2(x, y);
            }
            return res;
        }

        /// <summary>
        /// Renders a multipolygon byt rendering each polygon in the collection by calling DrawPolygon.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="pols">MultiPolygon to render</param>
        /// <param name="brush">Brush used for filling (null or transparent for no filling)</param>
        /// <param name="pen">Outline pen style (null if no outline)</param>
        /// <param name="clip">Specifies whether polygon clipping should be applied</param>
        /// <param name="map">Map reference</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DrawMultiPolygon(RenderTarget g, Factory factory, IMultiPolygon pols, Brush brush, Brush pen, float penWidth, StrokeStyle penStrokeStyle, bool clip, Map map)
        {
            for (var i = 0; i < pols.NumGeometries; i++)
            {
                var p = (IPolygon)pols[i];
                DrawPolygon(g, factory, p, brush, pen, penWidth, penStrokeStyle, clip, map);
            }
        }

        /// <summary>
        /// Renders a polygon to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="pol">Polygon to render</param>
        /// <param name="brush">Brush used for filling (null or transparent for no filling)</param>
        /// <param name="pen">Outline pen style (null if no outline)</param>
        /// <param name="clip">Specifies whether polygon clipping should be applied</param>
        /// <param name="map">Map reference</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DrawPolygon(RenderTarget g, Factory factory, IPolygon pol, Brush brush, Brush pen, float penWidth, StrokeStyle penStrokeStyle, bool clip, Map map)
        {
            if (pol.ExteriorRing == null)
                return;

            Vector2[] points;
            var startPoint = TransformToImage(pol.ExteriorRing, map, out points);
            if (points.Length > 1)
            {
                using (var geom = new PathGeometry(factory))
                {
                    using (var gs = geom.Open())
                    {
                        gs.SetFillMode(FillMode.Alternate);

                        gs.BeginFigure(startPoint, FigureBegin.Filled);
                        gs.AddLines(points);
                        gs.EndFigure(FigureEnd.Closed);

                        for (var i = 0; i < pol.NumInteriorRings; i++)
                        {
                            startPoint = TransformToImage(pol.GetInteriorRingN(i), map, out points);
                            if (points.Length > 1)
                            {
                                gs.BeginFigure(startPoint, FigureBegin.Filled);
                                gs.AddLines(points);
                                gs.EndFigure(FigureEnd.Closed);
                            }
                        }

                        gs.Close();
                    }

                    if (brush != null)
                        g.FillGeometry(geom, brush);
                    if (pen != null)
                        g.DrawGeometry(geom, pen, penWidth, penStrokeStyle);
                }
            }
        }

        /*
        private static void DrawPolygonClipped(GraphicsPath gp, PointF[] polygon, int width, int height)
        {
            var clipState = DetermineClipState(polygon, width, height);
            if (clipState == ClipState.Within)
            {
                gp.AddPolygon(polygon);
            }
            else if (clipState == ClipState.Intersecting)
            {
                var clippedPolygon = ClipPolygon(polygon, width, height);
                if (clippedPolygon.Length > 2)
                    gp.AddPolygon(clippedPolygon);
            }
        }
         */

        /// <summary>
        /// Purpose of this method is to prevent the 'overflow error' exception in the FillPath method. 
        /// This Exception is thrown when the coordinate values become too big (values over -2E+9f always 
        /// throw an exception, values under 1E+8f seem to be okay). This method limits the coordinates to 
        /// the values given by the second parameter (plus an minus). Theoretically the lines to and from 
        /// these limited points are not correct but GDI+ paints incorrect even before that limit is reached. 
        /// </summary>
        /// <param name="vertices">The vertices that need to be limited</param>
        /// <param name="limit">The limit at which coordinate values will be cutoff</param>
        /// <returns>The limited vertices</returns>
        private static Vector2[] LimitValues(Vector2[] vertices, float limit)
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].X = Math.Max(-limit, Math.Min(limit, vertices[i].X));
                vertices[i].Y = Math.Max(-limit, Math.Min(limit, vertices[i].Y));
            }
            return vertices;
        }


        private static ClipState DetermineClipState(Vector2[] vertices, int width, int height)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                minX = Math.Min(minX, vertices[i].X);
                minY = Math.Min(minY, vertices[i].Y);
                maxX = Math.Max(maxX, vertices[i].X);
                maxY = Math.Max(maxY, vertices[i].Y);
            }

            if (maxX < 0) return ClipState.Outside;
            if (maxY < 0) return ClipState.Outside;
            if (minX > width) return ClipState.Outside;
            if (minY > height) return ClipState.Outside;
            if (minX > 0 && maxX < width && minY > 0 && maxY < height) return ClipState.Within;
            return ClipState.Intersecting;
        }

        /// <summary>
        /// Clips a polygon to the view.
        /// Based on UMN Mapserver renderer 
        /// </summary>
        /// <param name="vertices">vertices in image coordinates</param>
        /// <param name="width">Width of map in image coordinates</param>
        /// <param name="height">Height of map in image coordinates</param>
        /// <returns>Clipped polygon</returns>
        internal static Vector2[] ClipPolygon(Vector2[] vertices, int width, int height)
        {
            var line = new List<Vector2>();
            if (vertices.Length <= 1) /* nothing to clip */
                return vertices;

            for (int i = 0; i < vertices.Length - 1; i++)
            {
                var x1 = vertices[i].X;
                var y1 = vertices[i].Y;
                var x2 = vertices[i + 1].X;
                var y2 = vertices[i + 1].Y;

                var deltax = x2 - x1;
                if (deltax == 0f)
                {
                    // bump off of the vertical
                    deltax = (x1 > 0) ? -NearZero : NearZero;
                }
                var deltay = y2 - y1;
                if (deltay == 0f)
                {
                    // bump off of the horizontal
                    deltay = (y1 > 0) ? -NearZero : NearZero;
                }

                float xin;
                float xout;
                if (deltax > 0)
                {
                    //  points to right
                    xin = 0;
                    xout = width;
                }
                else
                {
                    xin = width;
                    xout = 0;
                }

                float yin;
                float yout;
                if (deltay > 0)
                {
                    //  points up
                    yin = 0;
                    yout = height;
                }
                else
                {
                    yin = height;
                    yout = 0;
                }

                var tinx = (xin - x1) / deltax;
                var tiny = (yin - y1) / deltay;

                float tin1;
                float tin2;
                if (tinx < tiny)
                {
                    // hits x first
                    tin1 = tinx;
                    tin2 = tiny;
                }
                else
                {
                    // hits y first
                    tin1 = tiny;
                    tin2 = tinx;
                }

                if (1 >= tin1)
                {
                    if (0 < tin1)
                        line.Add(new Vector2(xin, yin));

                    if (1 >= tin2)
                    {
                        var toutx = (xout - x1) / deltax;
                        var touty = (yout - y1) / deltay;

                        var tout = (toutx < touty) ? toutx : touty;

                        if (0 < tin2 || 0 < tout)
                        {
                            if (tin2 <= tout)
                            {
                                if (0 < tin2)
                                {
                                    line.Add(tinx > tiny
                                                 ? new Vector2(xin, y1 + tinx * deltay)
                                                 : new Vector2(x1 + tiny * deltax, yin));
                                }

                                if (1 > tout)
                                {
                                    line.Add(toutx < touty
                                                 ? new Vector2(xout, y1 + toutx * deltay)
                                                 : new Vector2(x1 + touty * deltax, yout));
                                }
                                else
                                    line.Add(new Vector2(x2, y2));
                            }
                            else
                            {
                                line.Add(tinx > tiny ? new Vector2(xin, yout) : new Vector2(xout, yin));
                            }
                        }
                    }
                }
            }
            if (line.Count > 0)
                line.Add(new Vector2(line[0].X, line[0].Y));

            return line.ToArray();
        }

        /// <summary>
        /// Renders a point to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="point">Point to render</param>
        /// <param name="b">Brush reference</param>
        /// <param name="size">Size of drawn Point</param>
        /// <param name="offset">Symbol offset af scale=1</param>
        /// <param name="map">Map reference</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DrawPoint(RenderTarget g, Factory factory, IPoint point, Brush b, float size, Vector2 offset, Map map)
        {
            if (point == null)
                return;

            var pp = TransformToImage(point.Coordinate, map);
            if (double.IsNaN(point.X)) return;

            pp += offset;

            var e = new Ellipse(pp, size, size);
            g.FillEllipse(e, b);
            g.DrawEllipse(e, b);
        }

        /// <summary>
        /// Renders a point to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="point">Point to render</param>
        /// <param name="symbol">Symbol to place over point</param>
        /// <param name="symbolscale">The amount that the symbol should be scaled. A scale of '1' equals to no scaling</param>
        /// <param name="offset">Symbol offset af scale=1</param>
        /// <param name="rotation">Symbol rotation in degrees</param>
        /// <param name="map">Map reference</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DrawPoint(RenderTarget g, Factory factory, IPoint point, Bitmap symbol, Vector2 offset,
                                     float rotation, Map map)
        {
            if (point == null)
                return;

            var pp = TransformToImage(point.Coordinate, map);
            if (double.IsNaN(pp.X)) return;
            pp += offset;

            bool symbolCreated = false;
            if (symbol == null) //We have no point style - Use a default symbol
            {
                symbol = CreateDefaultsymbol(g);
                symbolCreated = true;
            }


            lock (symbol)
            {
                if (rotation != 0 && !Single.IsNaN(rotation))
                {
                    var startingTransform = new Matrix3x2(g.Transform.ToArray());

                    var transform = g.Transform;
                    var rotationCenter = pp;
                    Matrix3x2.Rotation(rotation, rotationCenter);
                    transform *= Matrix3x2.Rotation(rotation, rotationCenter);
                    g.Transform = transform;

                    //if (symbolscale == 1f)
                    //{
                    //    g.DrawImage(symbol,  (pp.X - symbol.Width/2f + offset.X),
                    //                                (pp.Y - symbol.Height/2f + offset.Y));
                    //}
                    //else
                    //{
                    //    var width = symbol.Width*symbolscale;
                    //    var height = symbol.Height*symbolscale;
                    //    g.DrawImage(symbol, (int) pp.X - width/2 + offset.X*symbolscale,
                    //                        (int) pp.Y - height/2 + offset.Y*symbolscale, width, height);
                    //}
                    var dx = 0.5d*symbol.PixelSize.Width;
                    var dy = 0.5d*symbol.PixelSize.Height;
                    g.DrawBitmap(symbol, new SharpDX.RectangleF(Convert.ToSingle(pp.X-dx), Convert.ToSingle(pp.Y+dy),
                                                                symbol.PixelSize.Width, symbol.PixelSize.Height),
                                                                1f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
                    g.Transform = startingTransform;
                }
                else
                {
                    //if (symbolscale == 1f)
                    //{
                    //    g.DrawImageUnscaled(symbol, (int) (pp.X - symbol.Width/2f + offset.X),
                    //                                (int) (pp.Y - symbol.Height/2f + offset.Y));
                    //}
                    //else
                    //{
                    //    var width = symbol.Width*symbolscale;
                    //    var height = symbol.Height*symbolscale;
                    //    g.DrawImage(symbol, (int) pp.X - width/2 + offset.X*symbolscale,
                    //                        (int) pp.Y - height/2 + offset.Y*symbolscale, width, height);
                    //}
                    var dx = 0.5d * symbol.PixelSize.Width;
                    var dy = 0.5d * symbol.PixelSize.Height;
                    g.DrawBitmap(symbol, new SharpDX.RectangleF(Convert.ToSingle(pp.X - dx), Convert.ToSingle(pp.Y + dy),
                                                                symbol.PixelSize.Width, symbol.PixelSize.Height),
                                                                1f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
                }
            }
            if (symbolCreated)
                symbol.Dispose();
        }

        private static Bitmap CreateDefaultsymbol(RenderTarget renderTarget)
        {
            return Converter.ToSharpDXBitmap(renderTarget, DefaultSymbol, 1f);
        }

        /// <summary>
        /// Renders a <see cref="GeoAPI.Geometries.IMultiPoint"/> to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="points">MultiPoint to render</param>
        /// <param name="symbol">Symbol to place over point</param>
        /// <param name="offset">Symbol offset af scale=1</param>
        /// <param name="rotation">Symbol rotation in degrees</param>
        /// <param name="map">Map reference</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DrawMultiPoint(RenderTarget  g, Factory factory, IMultiPoint points, Bitmap symbol, Vector2 offset,
                                          float rotation, Map map)
        {
            for (var i = 0; i < points.NumGeometries; i++)
            {
                var point = (IPoint)points[i];
                DrawPoint(g, factory, point, symbol, offset, rotation, map);
            }
        }

        /// <summary>
        /// Renders a <see cref="GeoAPI.Geometries.IMultiPoint"/> to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="points">MultiPoint to render</param>
        /// <param name="brush">Brush reference</param>
        /// <param name="size">Size of drawn Point</param>
        /// <param name="offset">Symbol offset af scale=1</param>
        /// <param name="map">Map reference</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DrawMultiPoint(RenderTarget g,Factory f, 
            IMultiPoint points, Brush brush, float size, Vector2 offset, Map map)
        {
            for (var i = 0; i < points.NumGeometries; i++)
            {
                var point = (IPoint)points[i];
                DrawPoint(g, f, point, brush, size, offset, map);
            }
        }

        #region Nested type: ClipState

        private enum ClipState
        {
            Within,
            Outside,
            Intersecting
        } ;

        #endregion
    }
}