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

using System.Drawing;
using SharpMap.Geometries;
using Point=SharpMap.Geometries.Point;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Class for transforming between world and image coordinate
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// Transforms from world coordinate system (WCS) to image coordinates
        /// NOTE: This method DOES NOT take the MapTransform property into account (use SharpMap.Map.MapToWorld instead)
        /// </summary>
        /// <param name="p">Point in WCS</param>
        /// <param name="map">Map reference</param>
        /// <returns>Point in image coordinates</returns>
        public static PointF WorldtoMap(Point p, Map map)
        {
            //if (map.MapTransform != null && !map.MapTransform.IsIdentity)
            //	map.MapTransform.TransformPoints(new System.Drawing.PointF[] { p });
            PointF result = new System.Drawing.Point();
            double height = (map.Zoom*map.Size.Height)/map.Size.Width;
            double left = map.Center.X - map.Zoom*0.5;
            double top = map.Center.Y + height*0.5*map.PixelAspectRatio;
            result.X = (float) ((p.X - left)/map.PixelWidth);
            result.Y = (float) ((top - p.Y)/map.PixelHeight);
            return result;
        }

        /// <summary>
        /// Transforms from image coordinates to world coordinate system (WCS).
        /// NOTE: This method DOES NOT take the MapTransform property into account (use SharpMap.Map.MapToWorld instead)
        /// </summary>
        /// <param name="p">Point in image coordinate system</param>
        /// <param name="map">Map reference</param>
        /// <returns>Point in WCS</returns>
        public static Point MapToWorld(PointF p, Map map)
        {
            //if (this.MapTransform != null && !this.MapTransform.IsIdentity)
            //{
            //    System.Drawing.PointF[] p2 = new System.Drawing.PointF[] { p };
            //    this.MapTransform.TransformPoints(new System.Drawing.PointF[] { p });
            //    this.MapTransformInverted.TransformPoints(p2);
            //    return Utilities.Transform.MapToWorld(p2[0], this);
            //}
            //else 
            Point ul = new Point(map.Center.X - map.Zoom * .5, map.Center.Y + map.MapHeight * .5);
            return new Point(ul.X + p.X * map.PixelWidth,
                             ul.Y - p.Y * map.PixelHeight);
            //BoundingBox env = map.Envelope;
            //return new Point(env.Min.X + p.X*map.PixelWidth,
            //                 env.Max.Y - p.Y*map.PixelHeight);
        }
    }
}