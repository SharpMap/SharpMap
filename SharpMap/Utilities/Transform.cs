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
using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Class for transforming between world and image coordinate
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// Am abbreviated transform from world coordinate system (WCS) to image coordinates
        /// for use ONLY when MapTransformRotation == 0
        /// </summary>
        /// <param name="coordinates">Coordinate array in WCS</param>
        /// <param name="worldLeft">Minimum X value of non-rotated viewport in world coordinates</param>
        /// <param name="worldTop">Maximum Y value of non-rotated viewport in world coordinates</param>
        /// <param name="pixelWidth">Apparent width of pixel in world units</param>
        /// <param name="pixelHeight">Apparent height of pixel in world units</param>
        /// <returns>Point array in image coordinates</returns>
        internal static PointF[] WorldToMap(Coordinate[] coordinates, double worldLeft, double worldTop,
            double pixelWidth, double pixelHeight)
        {
            // ONLY when MapTransFormRotation == 0
            var points = new PointF[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                var coord = coordinates[i];
                if (coord.IsEmpty() || double.IsNaN(coord.X) || double.IsNaN(coord.Y))
                {
                    points[i] = PointF.Empty;
                }
                else
                {
                    double x = (coord.X - worldLeft) / pixelWidth;
                    double y = (worldTop - coord.Y) / pixelHeight;
                    points[i] = new PointF((float) x, (float) y);
                }
            }
            return points;
        }

        /// <summary>
        /// Full affine transformation from world coordinate system (WCS) to image coordinates taking into 
        /// account Zoom, Pixel Width/Height, and MapTransformRotation. 
        /// </summary>
        /// <param name="coordinates">Coordinate array in WCS</param>
        /// <param name="matrix">Appropriate affine transformation as defined by <see cref="WorldToMapMatrix"/></param>
        /// <returns>Point array in image coordinates</returns>
        public static PointF[] WorldToMap(Coordinate[] coordinates, AffineTransformation matrix)
        {
            var points = new PointF[coordinates.Length];
            var transformed = new Coordinate();
            for (var i = 0; i < coordinates.Length; i++)
            {
                matrix.Transform(coordinates[i], transformed);
                points[i] = new PointF((float)transformed.X, (float)transformed.Y);
            }
            return points;
        }

        /// <summary>
        /// Affine transformation defining complete transformation from world coordinate system (WCS) to image coordinates taking into 
        /// account Zoom, Pixel Width/Height, and MapTransformRotation. Additionally, if <paramref name="careAboutTransform"/> = false,
        /// the viewport rotation will be reverted at the end of the transformation, to be re-applied by Graphics object when rendering. 
        /// </summary>
        /// <param name="worldCenter">Map center in WCS</param>
        /// <param name="pixelWidth">Width of pixel in world units</param>
        /// <param name="pixelHeight">Height of pixel in world units</param>
        /// <param name="mapTransformRotation">map rotation in degrees</param>
        /// <param name="imageSize">Map Size when rendered</param>
        /// <param name="careAboutTransform">True for coordinate calculations, False if Graphics object will apply MapTransform</param>
        /// <returns>Affine Transformation</returns>
        public static AffineTransformation WorldToMapMatrix(Coordinate worldCenter, 
            double pixelWidth, double pixelHeight, float mapTransformRotation, Size imageSize,
            bool careAboutTransform)
        {
            var rad = NetTopologySuite.Utilities.Degrees.ToRadians(mapTransformRotation);
            var trans = new AffineTransformation();
            trans.Compose(AffineTransformation.TranslationInstance(-worldCenter.X, -worldCenter.Y));
            trans.Compose(AffineTransformation.RotationInstance(-rad));
            trans.Compose(AffineTransformation.ScaleInstance(1 / pixelWidth, -1 / pixelHeight));
            trans.Compose(AffineTransformation.TranslationInstance(imageSize.Width * 0.5, imageSize.Height * 0.5));

            if (!careAboutTransform)
            {
                // if we DON'T care about transform (implies that rotation in image space WILL be performed by graphics
                // object once drawing of ALL objects has been completed) then need to revert rotation (ie MapTransform).
                trans.Compose(AffineTransformation.RotationInstance(-rad, imageSize.Width * 0.5, imageSize.Height * 0.5));
            }
            return trans;
        }

        /// <summary>
        /// Am abbreviated transform from image coordinates to world coordinate system (WCS) 
        /// for use ONLY when MapTransformRotation == 0
        /// </summary>
        /// <param name="points">Point array in image coordinates</param>
        /// <param name="map">Map defining current view properties</param>
        public static Coordinate[] MapToWorld(PointF[] points, Map map)
        {
            return MapToWorld(points, map.Center, map.Zoom, map.MapHeight, map.PixelWidth, map.PixelHeight);
        }


        /// <summary>
        /// Am abbreviated transform from image coordinates to world coordinate system (WCS) 
        /// for use ONLY when MapTransformRotation == 0
        /// </summary>
        /// <param name="points">Point array in image coordinates</param>
        /// <param name="worldCenter">Map center in WCS</param>
        /// <param name="mapZoom">current map zoom (width) in world units</param>
        /// <param name="mapHeight">current map height in world units</param>
        /// <param name="pixelWidth">Apparent width of pixel in world units</param>
        /// <param name="pixelHeight">Apparent height of pixel in world units</param>
        internal static Coordinate[] MapToWorld(PointF[] points, Coordinate worldCenter, double mapZoom,
            double mapHeight, double pixelWidth, double pixelHeight)
        {
            var coords = new Coordinate[points.Length];
            if (worldCenter.IsEmpty() || double.IsNaN(mapHeight))
                for (var i = 0; i < points.Length; i++)
                    coords[i] = new Coordinate(0, 0);
            else
            {
                var ul = new Coordinate(worldCenter.X - mapZoom * .5,worldCenter.Y + mapHeight * .5);
                for (var i = 0; i < points.Length; i++)
                    coords[i] = new Coordinate(ul.X + points[i].X * pixelWidth,ul.Y - points[i].Y * pixelHeight);
            }

            return coords;
        }

        /// <summary>
        /// Transforms from world coordinate system (WCS) to image coordinates
        /// NOTE: This method is only applicable when MapTransformRotation = 0.
        /// </summary>
        /// <param name="p">Point in WCS</param>
        /// <param name="map">Map reference</param>
        /// <returns>Point in image coordinates</returns>
        [Obsolete("Use WorldToMap(Coordinate[], Map)")]
        public static PointF WorldToMap(Coordinate p, Map map)
        {
            var left = map.Center.X - map.Zoom * 0.5;
            var top = map.Center.Y + map.MapHeight * 0.5;
            var points = WorldToMap(new [] {p}, left, top, map.PixelWidth, map.PixelHeight);
            return points[0];
        }

        /// <summary>
        /// Transforms from image coordinates to world coordinate system (WCS).
        /// NOTE: This method is only applicable when MapTransformRotation = 0.
        /// </summary>
        /// <param name="p">Point in image coordinate system</param>
        /// <param name="map">Map reference</param>
        /// <returns>Point in WCS</returns>
        [Obsolete("Use MapToWorld(PointF[], Map)")]
        public static Coordinate MapToWorld(PointF p, Map map)
        {
            var coords = MapToWorld(new [] {p}, map);
            return coords[0];
        }
        
        /// <summary>
        /// Transforms from image coordinates to world coordinate system (WCS).
        /// NOTE: This method is only applicable when MapTransformRotation = 0.
        /// </summary>
        /// <param name="p">Point in image coordinate system</param>
        /// <param name="map">Map reference</param>
        /// <returns>Point in WCS</returns>
        [Obsolete("Use MapToWorld(PointF[], MapViewport)")]
        public static Coordinate MapToWorld(PointF p, MapViewport map)
        {
            var coords = MapToWorld(new [] {p}, map.Center, map.Zoom, map.MapHeight, map.PixelWidth, map.PixelHeight);
            return coords[0];
        }
    }
}
