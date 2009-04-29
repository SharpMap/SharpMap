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
using System.Drawing.Drawing2D;
using System.Reflection;
using SharpMap.Geometries;
using SharpMap.Utilities;
using Point=SharpMap.Geometries.Point;

namespace SharpMap.Rendering
{
    /// <summary>
    /// This class renders individual geometry features to a graphics object using the settings of a map object.
    /// </summary>
    public class VectorRenderer
    {
        private const float extremeValueLimit = 1E+8f;
        private const float nearZero = 1E-30f; // 1/Infinity

        private static readonly Bitmap defaultsymbol =
            (Bitmap)
            Image.FromStream(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png"));

        /// <summary>
        /// Renders a MultiLineString to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="lines">MultiLineString to be rendered</param>
        /// <param name="pen">Pen style used for rendering</param>
        /// <param name="map">Map reference</param>
        public static void DrawMultiLineString(Graphics g, MultiLineString lines, Pen pen, Map map)
        {
            for (int i = 0; i < lines.LineStrings.Count; i++)
                DrawLineString(g, lines.LineStrings[i], pen, map);
        }

        /// <summary>
        /// Renders a LineString to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="line">LineString to render</param>
        /// <param name="pen">Pen style used for rendering</param>
        /// <param name="map">Map reference</param>
        public static void DrawLineString(Graphics g, LineString line, Pen pen, Map map)
        {
            if (line.Vertices.Count > 1)
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddLines(LimitValues(line.TransformToImage(map), extremeValueLimit));
                g.DrawPath(pen, gp);
            }
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
        public static void DrawMultiPolygon(Graphics g, MultiPolygon pols, Brush brush, Pen pen, bool clip, Map map)
        {
            for (int i = 0; i < pols.Polygons.Count; i++)
                DrawPolygon(g, pols.Polygons[i], brush, pen, clip, map);
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
        public static void DrawPolygon(Graphics g, Polygon pol, Brush brush, Pen pen, bool clip, Map map)
        {
            if (pol.ExteriorRing == null)
                return;
            if (pol.ExteriorRing.Vertices.Count > 2)
            {
                //Use a graphics path instead of DrawPolygon. DrawPolygon has a problem with several interior holes
                GraphicsPath gp = new GraphicsPath();

                //Add the exterior polygon
                if (!clip)
                    gp.AddPolygon(LimitValues(pol.ExteriorRing.TransformToImage(map), extremeValueLimit));
                else
                    DrawPolygonClipped(gp, pol.ExteriorRing.TransformToImage(map), map.Size.Width, map.Size.Height);

                //Add the interior polygons (holes)
                for (int i = 0; i < pol.InteriorRings.Count; i++)
                    if (!clip)
                        gp.AddPolygon(LimitValues(pol.InteriorRings[i].TransformToImage(map), extremeValueLimit));
                    else
                        DrawPolygonClipped(gp, pol.InteriorRings[i].TransformToImage(map), map.Size.Width,
                                           map.Size.Height);

                // Only render inside of polygon if the brush isn't null or isn't transparent
                if (brush != null && brush != Brushes.Transparent)
                    g.FillPath(brush, gp);
                // Create an outline if a pen style is available
                if (pen != null)
                    g.DrawPath(pen, gp);
            }
        }

        private static void DrawPolygonClipped(GraphicsPath gp, PointF[] polygon, int width, int height)
        {
            ClipState clipState = DetermineClipState(polygon, width, height);
            if (clipState == ClipState.Within)
            {
                gp.AddPolygon(polygon);
            }
            else if (clipState == ClipState.Intersecting)
            {
                PointF[] clippedPolygon = clipPolygon(polygon, width, height);
                if (clippedPolygon.Length > 2)
                    gp.AddPolygon(clippedPolygon);
            }
        }

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
        private static PointF[] LimitValues(PointF[] vertices, float limit)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].X = Math.Max(-limit, Math.Min(limit, vertices[i].X));
                vertices[i].Y = Math.Max(-limit, Math.Min(limit, vertices[i].Y));
            }
            return vertices;
        }

        /// <summary>
        /// Renders a label to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="LabelPoint">Label placement</param>
        /// <param name="Offset">Offset of label in screen coordinates</param>
        /// <param name="font">Font used for rendering</param>
        /// <param name="forecolor">Font forecolor</param>
        /// <param name="backcolor">Background color</param>
        /// <param name="halo">Color of halo</param>
        /// <param name="rotation">Text rotation in degrees</param>
        /// <param name="text">Text to render</param>
        /// <param name="map">Map reference</param>
        public static void DrawLabel(Graphics g, PointF LabelPoint, PointF Offset, Font font, Color forecolor,
                                     Brush backcolor, Pen halo, float rotation, string text, Map map)
        {
            SizeF fontSize = g.MeasureString(text, font); //Calculate the size of the text
            LabelPoint.X += Offset.X;
            LabelPoint.Y += Offset.Y; //add label offset
            if (rotation != 0 && rotation != float.NaN)
            {
                g.TranslateTransform(LabelPoint.X, LabelPoint.Y);
                g.RotateTransform(rotation);
                g.TranslateTransform(-fontSize.Width/2, -fontSize.Height/2);
                if (backcolor != null && backcolor != Brushes.Transparent)
                    g.FillRectangle(backcolor, 0, 0, fontSize.Width*0.74f + 1f, fontSize.Height*0.74f);
                GraphicsPath path = new GraphicsPath();
                path.AddString(text, font.FontFamily, (int) font.Style, font.Size, new System.Drawing.Point(0, 0), null);
                if (halo != null)
                    g.DrawPath(halo, path);
                g.FillPath(new SolidBrush(forecolor), path);
                //g.DrawString(text, font, new System.Drawing.SolidBrush(forecolor), 0, 0);        
                g.Transform = map.MapTransform;
            }
            else
            {
                if (backcolor != null && backcolor != Brushes.Transparent)
                    g.FillRectangle(backcolor, LabelPoint.X, LabelPoint.Y, fontSize.Width*0.74f + 1,
                                    fontSize.Height*0.74f);

                GraphicsPath path = new GraphicsPath();

                path.AddString(text, font.FontFamily, (int) font.Style, font.Size, LabelPoint, null);
                if (halo != null)
                    g.DrawPath(halo, path);
                g.FillPath(new SolidBrush(forecolor), path);
                //g.DrawString(text, font, new System.Drawing.SolidBrush(forecolor), LabelPoint.X, LabelPoint.Y);
            }
        }

        private static ClipState DetermineClipState(PointF[] vertices, int width, int height)
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
        internal static PointF[] clipPolygon(PointF[] vertices, int width, int height)
        {
            float deltax, deltay, xin, xout, yin, yout;
            float tinx, tiny, toutx, touty, tin1, tin2, tout;
            float x1, y1, x2, y2;

            List<PointF> line = new List<PointF>();
            if (vertices.Length <= 1) /* nothing to clip */
                return vertices;

            for (int i = 0; i < vertices.Length - 1; i++)
            {
                x1 = vertices[i].X;
                y1 = vertices[i].Y;
                x2 = vertices[i + 1].X;
                y2 = vertices[i + 1].Y;

                deltax = x2 - x1;
                if (deltax == 0)
                {
                    // bump off of the vertical
                    deltax = (x1 > 0) ? -nearZero : nearZero;
                }
                deltay = y2 - y1;
                if (deltay == 0)
                {
                    // bump off of the horizontal
                    deltay = (y1 > 0) ? -nearZero : nearZero;
                }

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

                tinx = (xin - x1)/deltax;
                tiny = (yin - y1)/deltay;

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
                        line.Add(new PointF(xin, yin));

                    if (1 >= tin2)
                    {
                        toutx = (xout - x1)/deltax;
                        touty = (yout - y1)/deltay;

                        tout = (toutx < touty) ? toutx : touty;

                        if (0 < tin2 || 0 < tout)
                        {
                            if (tin2 <= tout)
                            {
                                if (0 < tin2)
                                {
                                    if (tinx > tiny)
                                        line.Add(new PointF(xin, y1 + tinx*deltay));
                                    else
                                        line.Add(new PointF(x1 + tiny*deltax, yin));
                                }

                                if (1 > tout)
                                {
                                    if (toutx < touty)
                                        line.Add(new PointF(xout, y1 + toutx*deltay));
                                    else
                                        line.Add(new PointF(x1 + touty*deltax, yout));
                                }
                                else
                                    line.Add(new PointF(x2, y2));
                            }
                            else
                            {
                                if (tinx > tiny)
                                    line.Add(new PointF(xin, yout));
                                else
                                    line.Add(new PointF(xout, yin));
                            }
                        }
                    }
                }
            }
            if (line.Count > 0)
                line.Add(new PointF(line[0].X, line[0].Y));

            return line.ToArray();
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
        public static void DrawPoint(Graphics g, Point point, Bitmap symbol, float symbolscale, PointF offset,
                                     float rotation, Map map)
        {
            if (point == null)
                return;
            if (symbol == null) //We have no point style - Use a default symbol
                symbol = defaultsymbol;

            PointF pp = Transform.WorldtoMap(point, map);

            Matrix startingTransform = g.Transform;

            if (rotation != 0 && !Single.IsNaN(rotation))
            {
                PointF rotationCenter = PointF.Add(pp, new SizeF(symbol.Width/2, symbol.Height/2));
                Matrix transform = new Matrix();
                transform.RotateAt(rotation, rotationCenter);

                g.Transform = transform;

                if (symbolscale == 1f)
                    g.DrawImageUnscaled(symbol, (int) (pp.X - symbol.Width/2 + offset.X),
                                        (int) (pp.Y - symbol.Height/2 + offset.Y));
                else
                {
                    float width = symbol.Width*symbolscale;
                    float height = symbol.Height*symbolscale;
                    g.DrawImage(symbol, (int) pp.X - width/2 + offset.X*symbolscale,
                                (int) pp.Y - height/2 + offset.Y*symbolscale, width, height);
                }

                g.Transform = startingTransform;
            }
            else
            {
                if (symbolscale == 1f)
                    g.DrawImageUnscaled(symbol, (int) (pp.X - symbol.Width/2 + offset.X),
                                        (int) (pp.Y - symbol.Height/2 + offset.Y));
                else
                {
                    float width = symbol.Width*symbolscale;
                    float height = symbol.Height*symbolscale;
                    g.DrawImage(symbol, (int) pp.X - width/2 + offset.X*symbolscale,
                                (int) pp.Y - height/2 + offset.Y*symbolscale, width, height);
                }
            }
        }

        /// <summary>
        /// Renders a <see cref="SharpMap.Geometries.MultiPoint"/> to the map.
        /// </summary>
        /// <param name="g">Graphics reference</param>
        /// <param name="points">MultiPoint to render</param>
        /// <param name="symbol">Symbol to place over point</param>
        /// <param name="symbolscale">The amount that the symbol should be scaled. A scale of '1' equals to no scaling</param>
        /// <param name="offset">Symbol offset af scale=1</param>
        /// <param name="rotation">Symbol rotation in degrees</param>
        /// <param name="map">Map reference</param>
        public static void DrawMultiPoint(Graphics g, MultiPoint points, Bitmap symbol, float symbolscale, PointF offset,
                                          float rotation, Map map)
        {
            for (int i = 0; i < points.Points.Count; i++)
                DrawPoint(g, points.Points[i], symbol, symbolscale, offset, rotation, map);
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