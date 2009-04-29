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
using SharpMap.Geometries;

namespace SharpMap.Web.Wms.Tiling
{
    /// <summary>
    /// Very simple implemenation of ITileCache to demonstrate its functionality. It dumps all tiles als png's in a single directory.
    /// </summary>
    public class TileCacheFileStorage : ITileCache
    {
        private CultureInfo cultureInfo = new CultureInfo("en-US", false);
        private string directory;

        /// <summary>
        /// This constructor creates the storage directory if it does not exist.
        /// </summary>
        /// <param name="directory">Directory where the tiles will be stored</param>
        public TileCacheFileStorage(string directory)
        {
            this.directory = directory;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        #region ITileCache Members

        public void AddTile(BoundingBox box, Bitmap bitmap)
        {
            bitmap.Save(GetFileName(box), ImageFormat.Png);
        }

        public Bitmap GetTile(BoundingBox box)
        {
            return new Bitmap(GetFileName(box));
        }

        public bool ContainsTile(BoundingBox box)
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
            string dataDir = Environment.GetEnvironmentVariable("AppData");
            string appName = Assembly.GetEntryAssembly().GetName().Name.ToString();
            return dataDir + "/" + appName + "/TileCache/Layer_" + layerName + "/TileSet_" + tileSetName;
        }

        private string GetFileName(BoundingBox boundingBox)
        {
            return String.Format("{0}/{1}_{2}_{3}_{4}.{5}", directory,
                                 boundingBox.Left.ToString("r", cultureInfo), boundingBox.Top.ToString("r", cultureInfo),
                                 boundingBox.Right.ToString("r", cultureInfo),
                                 boundingBox.Bottom.ToString("r", cultureInfo),
                                 "png");
        }
    }
}