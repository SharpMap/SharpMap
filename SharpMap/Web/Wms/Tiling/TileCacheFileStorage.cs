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
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using GeoAPI.Geometries;

namespace SharpMap.Web.Wms.Tiling
{
    /// <summary>
    /// Very simple implementation of ITileCache to demonstrate its functionality. It dumps all tiles as png's in a single directory.
    /// </summary>
    public class TileCacheFileStorage : ITileCache
    {
        private readonly CultureInfo _cultureInfo = new CultureInfo("en-US", false);
        private readonly string _directory;

        /// <summary>
        /// This constructor creates the storage directory if it does not exist.
        /// </summary>
        /// <param name="directory">Directory where the tiles will be stored</param>
        public TileCacheFileStorage(string directory)
        {
            _directory = directory;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        #region ITileCache Members

        /// <summary>
        /// Method to add a tile to the cache.
        /// </summary>
        /// <param name="box">The bounding <paramref name="box"/> of the area covered by the <paramref name="tile"/>.</param>
        /// <param name="tile">The tile image</param>
        public void AddTile(Envelope box, Bitmap tile)
        {
            tile.Save(GetFileName(box), ImageFormat.Png);
        }

        /// <summary>
        /// Function to retrieve a tile from the cache that covers the provided <paramref name="box"/>.
        /// </summary>
        /// <param name="box">The area that is to be covered by the tile</param>
        /// <returns></returns>
        public Bitmap GetTile(Envelope box)
        {
            return new Bitmap(GetFileName(box));
        }

        /// <summary>
        /// Function to evaluate if the cache contains a tile that covers the provided <paramref name="box"/>.
        /// </summary>
        /// <param name="box">The area that is to be covered by the tile</param>
        /// <returns><value>true</value> if such a tile image is in the cache.</returns>
        public bool ContainsTile(Envelope box)
        {
            return File.Exists(GetFileName(box));
        }

        #endregion

        /// <summary>
        /// Helper for convenience. Generates a directory path the Application Data directory 
        /// which could be used to store tiles.
        /// </summary>
        /// <param name="layerName">Name of the SharpMap layer</param>
        /// <param name="tileSetName">Name of the TileSet</param>
        /// <returns></returns>
        public static string GenerateDirectoryPath(string layerName, string tileSetName)
        {
            var dataDir = Environment.GetEnvironmentVariable("AppData");
            var appName = Assembly.GetEntryAssembly().GetName().Name;
            return dataDir + "/" + appName + "/TileCache/Layer_" + layerName + "/TileSet_" + tileSetName;
        }

        private string GetFileName(Envelope boundingBox)
        {
            return String.Format("{0}/{1}_{2}_{3}_{4}.{5}", _directory,
                                 boundingBox.Left().ToString("r", _cultureInfo), boundingBox.Top().ToString("r", _cultureInfo),
                                 boundingBox.Right().ToString("r", _cultureInfo),
                                 boundingBox.Bottom().ToString("r", _cultureInfo),
                                 "png");
        }
    }
}