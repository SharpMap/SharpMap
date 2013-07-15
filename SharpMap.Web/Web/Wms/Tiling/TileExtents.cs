// Copyright 2007 - Paul den Dulk (Geodan)
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
using GeoAPI.Geometries;

namespace SharpMap.Web.Wms.Tiling
{
    internal static class TileExtents
    {
        /// <summary>
        /// Returns a List of the tile BoundingBoxes which cover the complete area of the map BoundingBox 
        /// </summary>
        /// <param name="tileSet">The TileSet that provides the tiles</param>
        /// <param name="extent">The BoundingBox of the map</param>
        /// <param name="mapResolution">The resolution of the map</param>
        /// <returns></returns>
        public static List<Envelope> GetTileExtents(TileSet tileSet, Envelope extent, double mapResolution)
        {
            tileSet.Resolutions.Sort();

            double tileResolution = GetTileResolution(tileSet.Resolutions, mapResolution);

            var tileExtents = new List<Envelope>();

            double xOrigin = tileSet.BoundingBox.MinX;
            double yOrigin = tileSet.BoundingBox.MinY;

            double tileWorldUnits = tileResolution*tileSet.Width;

            var tileBbox = new Envelope(
                Math.Floor((extent.MinX - xOrigin)/tileWorldUnits)*tileWorldUnits + xOrigin,
                Math.Floor((extent.MinY - yOrigin)/tileWorldUnits)*tileWorldUnits + yOrigin,
                Math.Ceiling((extent.MaxX - xOrigin)/tileWorldUnits)*tileWorldUnits + xOrigin,
                Math.Ceiling((extent.MaxY - yOrigin)/tileWorldUnits)*tileWorldUnits + yOrigin);

            int tileCountX = (int) Math.Round((tileBbox.MaxX - tileBbox.MinX)/tileWorldUnits);
            int tileCountY = (int) Math.Round((tileBbox.MaxY - tileBbox.MinY)/tileWorldUnits);

            for (int x = 0; x < tileCountX; x++)
            {
                for (int y = 0; y < tileCountY; y++)
                {
                    double x1 = tileBbox.MinX    + x*tileWorldUnits;
                    double y1 = tileBbox.MinY + y*tileWorldUnits;
                    double x2 = x1 + tileWorldUnits;
                    double y2 = y1 + tileWorldUnits;

                    var tileExtent = new Envelope(x1, x2, y1, y2);

                    if (CheckForBounds(tileSet.BoundingBox, tileExtent))
                    {
                        tileExtents.Add(tileExtent);
                    }
                }
            }
            return tileExtents;
        }

        /// <summary>
        /// Checks if the tile with given extent is within the bounds given by
        /// the TileSet BoundingBox. It is not a normal Within operation however
        /// because the extent can be partially outside to the top right but not 
        /// outside of the bottom left. That is how it is defined in WMS-C.
        /// </summary>
        /// <returns>Returns true if the tile with this extent is part of the tile collection on the server</returns>
        private static bool CheckForBounds(Envelope boundingBox, Envelope tileExtent)
        {
            if (tileExtent.MinX < boundingBox.MinX)
            {
                return false;
            }
            if (tileExtent.MinY < boundingBox.MinY)
            {
                return false;
            }
            if (tileExtent.MinX > boundingBox.MaxX)
            {
                return false;
            }
            if (tileExtent.MinY > boundingBox.MaxY)
            {
                return false;
            }
            return true;
        }

        private static double GetTileResolution(List<double> resolutions, double resolution)
        {
            if (resolutions.Count == 0)
            {
                throw new Exception("No tile resolutions defined");
            }

            if (resolutions[resolutions.Count - 1] < resolution)
            {
                return resolutions[resolutions.Count - 1];
            }

            int result = 0;
            double resultDistance = double.MaxValue;
            for (int i = 0; i < resolutions.Count; i++)
            {
                double distance = Math.Abs(resolutions[i] - resolution);
                if (distance < resultDistance)
                {
                    result = i;
                    resultDistance = distance;
                }
            }
            return resolutions[result];
        }
    }
}