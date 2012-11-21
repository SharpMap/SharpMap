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
using GeoAPI.Geometries;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Class for transforming between world and image coordinate
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// Transforms from world coordinate system (WCS) to image coordinates
        /// NOTE: This method DOES NOT take the MapTransform property into account (use <see cref="Map.WorldToImage(GeoAPI.Geometries.Coordinate,bool)"/> instead)
        /// </summary>
        /// <param name="p">Point in WCS</param>
        /// <param name="map">Map reference</param>
        /// <returns>Point in image coordinates</returns>
        public static PointF WorldtoMap(Coordinate p, Map map)
        {
            //if (map.MapTransform != null && !map.MapTransform.IsIdentity)
            //	map.MapTransform.TransformPoints(new System.Drawing.PointF[] { p });
            if (p.IsEmpty())
                return PointF.Empty;

            var result = new PointF();

            var height = (map.Zoom * map.Size.Height) / map.Size.Width;
            var left = map.Center.X - map.Zoom * 0.5;
            var top = map.Center.Y + height * 0.5 * map.PixelAspectRatio;
            result.X = (float)((p.X - left) / map.PixelWidth);
            result.Y = (float)((top - p.Y) / map.PixelHeight);
            if (double.IsNaN(result.X) || double.IsNaN(result.Y))
                result = PointF.Empty;
            return result;
        }

        /// <summary>
        /// Transforms from image coordinates to world coordinate system (WCS).
        /// NOTE: This method DOES NOT take the MapTransform property into account (use <see cref="Map.ImageToWorld(System.Drawing.PointF,bool)"/> instead)
        /// </summary>
        /// <param name="p">Point in image coordinate system</param>
        /// <param name="map">Map reference</param>
        /// <returns>Point in WCS</returns>
        public static Coordinate MapToWorld(PointF p, Map map)
        {
            if (map.Center.IsEmpty() || double.IsNaN(map.MapHeight))
            {
                return new Coordinate(0, 0);
            }
            var ul = new Coordinate(map.Center.X - map.Zoom * .5, map.Center.Y + map.MapHeight * .5);
            return new Coordinate(ul.X + p.X * map.PixelWidth,
                                  ul.Y - p.Y * map.PixelHeight);
        }
    }
}