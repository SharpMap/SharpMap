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

namespace SharpMap.Layers
{
    /// <summary>
    /// Interface for map layers
    /// </summary>
    public interface ILayer
    {
        /// <summary>
        /// Minimum visible zoom level
        /// </summary>
        double MinVisible { get; set; }

        /// <summary>
        /// Minimum visible zoom level
        /// </summary>
        double MaxVisible { get; set; }

        /// <summary>
        /// Gets or Sets what level-reference the Min/Max values are defined in
        /// </summary>
        Styles.VisibilityUnits VisibilityUnits { get; set; }

        /// <summary>
        /// Specifies whether this layer should be rendered or not
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Name of layer
        /// </summary>
        string LayerName { get; set; }

        /// <summary>
        /// Optional title of layer. It will be used for services like WMS where both Name and Title are supported.
        /// </summary>
        string LayerTitle { get; set; }

        /// <summary>
        /// Gets the boundingbox of the entire layer
        /// </summary>
        Envelope Envelope { get; }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        int SRID { get; set; }

        /// <summary>
        /// The spatial reference ID (CRS) that can be exposed externally.
        /// </summary>
        /// <remarks>
        /// TODO: explain better why I need this property
        /// </remarks>
        int TargetSRID { get; }

        /// <summary>
        /// Proj4 String Projection
        /// </summary>
        string Proj4Projection { get; set; }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        [Obsolete("Use Render(Graphics, MapViewport)")]
        void Render(Graphics g, Map map);

        /// <summary>
        /// Renders the layer using the current viewport
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        void Render(Graphics g, MapViewport  map);

        //SharpMap.CoordinateSystems.CoordinateSystem CoordinateSystem { get; set; }
    }

    internal static class LayerExtensions
    {
        public static bool DoRender(this ILayer self, Graphics g, Map map )
        {
            // Is the layer enabled for rendering at all
            if (!self.Enabled)
                return false;
            
            // Get the compare value, zoom or scale
            var compare = self.VisibilityUnits == Styles.VisibilityUnits.ZoomLevel
                ? map.Zoom
                : map.GetMapScale((int) g.DpiX);

            // Is the layer enabled for the current scale
            if (self.MinVisible <= compare || compare <= self.MaxVisible)
                return false;

            // Does the layer intersect the viewport at all
            if (!self.Envelope.Intersects(map.Envelope))
                return false;

            return true;
        }

        public static bool DoRender(this ILayer self, Graphics g, MapViewport map)
        {
            // Is the layer enabled for rendering at all
            if (!self.Enabled)
                return false;

            // Get the compare value, zoom or scale
            var compare = self.VisibilityUnits == Styles.VisibilityUnits.ZoomLevel
                ? map.Zoom
                : map.GetMapScale((int)g.DpiX);

            // Is the layer enabled for the current scale
            if (self.MinVisible <= compare || compare <= self.MaxVisible)
                return false;

            // Does the layer intersect the viewport at all
            if (!self.Envelope.Intersects(map.Envelope))
                return false;

            return true;
        }

        public static bool DoRender(this Styles.IStyle self, Graphics g, Map map)
        {
            if (!self.Enabled)
                return false;

            var compare = self.VisibilityUnits == Styles.VisibilityUnits.ZoomLevel
                ? map.Zoom
                : map.GetMapScale((int)g.DpiX);

            if (self.MinVisible <= compare || compare <= self.MaxVisible)
                return false;

            return true;
        }
    }

}
