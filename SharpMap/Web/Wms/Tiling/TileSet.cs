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
using System.Globalization;
using System.Text;
using System.Xml;
using GeoAPI.Geometries;

namespace SharpMap.Web.Wms.Tiling
{
    /// <summary>
    /// Class for handling a set of tiles
    /// </summary>
    public class TileSet
    {
        #region Fields

        private Envelope _boundingBox;
        private string _format;
        private int _height;
        private List<string> _layers = new List<string>();
        private string _name;
        private List<double> _resolutions = new List<double>();
        private string _srs;
        private List<string> _styles = new List<string>();
        private ITileCache _tileCache;
        private int _width;

        #endregion

        /// <summary>
        /// Checks if the TileSet is initialized and throws an exception if not
        /// </summary>
        public void Verify()
        {
            if (_layers.Count == 0)
            {
                throw new Exception(String.Format("No Layers were added for the TileSet"));
            }
            if (_srs == String.Empty)
            {
                throw new Exception(String.Format("The SRS was not set for TileSet '{0}'", Name));
            }
            if (_boundingBox == null)
            {
                throw new Exception(String.Format("The BoundingBox was not set for TileSet '{0}'", Name));
            }
            if (_resolutions.Count == 0)
            {
                throw new Exception(String.Format("No Resolutions were added for TileSet '{0}'", Name));
            }
            if (_width == 0)
            {
                throw new Exception(String.Format("The Width was not set for TileSet '{0}'", Name));
            }
            if (_height == 0)
            {
                throw new Exception(String.Format("The Height was not set for TileSet '{0}'", Name));
            }
            if (_format == String.Empty)
            {
                throw new Exception(String.Format("The Format was not set for TileSet '{0}'", Name));
            }

            //Note: An empty Style list is allowed so dont check for Style

            //TODO: BoundingBox should contain a SRS, and we should check if BoundingBox.Srs is the same
            //as tileset Srs because we do not project one to the other. 
        }

        /// <summary>
        /// Creates a default name from a list of <paramref name="layers"/> names
        /// </summary>
        /// <param name="layers">A series of layer names</param>
        /// <returns>A string</returns>
        private static string CreateDefaultName(IEnumerable<string> layers)
        {
            var stringBuilder = new StringBuilder();
            foreach (var layer in layers)
            {
                stringBuilder.Append(layer + ",");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Parses the TileSets from the VendorSpecificCapabilities node of the WMS Capabilities 
        /// and adds them to the TileSets member
        /// </summary>
        /// <param name="xnlVendorSpecificCapabilities">The VendorSpecificCapabilities node of the Capabilities</param>
        /// <returns>A sorted list of <see cref="TileSet"/>s</returns>
        public static SortedList<string, TileSet> ParseVendorSpecificCapabilitiesNode(
            XmlNode xnlVendorSpecificCapabilities)
        {
            var tileSets = new SortedList<string, TileSet>();
            var nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("sm", "");

            var xnlTileSets = xnlVendorSpecificCapabilities.SelectNodes("sm:TileSet", nsmgr);
            if (xnlTileSets != null)
            {
                foreach (XmlNode xnlTileSet in xnlTileSets)
                {
                    var tileSet = ParseTileSetNode(xnlTileSet, nsmgr);
                    tileSets.Add(tileSet.Name + "-" + tileSet.Format + "-" + tileSet.Srs, tileSet);
                }
            }
            return tileSets;
        }

        private static TileSet ParseTileSetNode(XmlNode xnlTileSet, XmlNamespaceManager nsmgr)
        {
            var tileSet = new TileSet();

            var xnLayers = xnlTileSet.SelectSingleNode("sm:Layers", nsmgr);
            if (xnLayers != null)
                tileSet.Layers.AddRange(xnLayers.InnerText.Split(new[] {','}));

            tileSet.Name = CreateDefaultName(tileSet._layers);

            var xnSrs = xnlTileSet.SelectSingleNode("sm:SRS", nsmgr);
            if (xnSrs != null)
                tileSet.Srs = xnSrs.InnerText;

            var xnWidth = xnlTileSet.SelectSingleNode("sm:Width", nsmgr);
            if (xnWidth != null)
            {
                int width;
                if (!Int32.TryParse(xnWidth.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out width))
                    throw new ArgumentException("Invalid width on tileset '" + tileSet.Name + "'");
                tileSet.Width = width;
            }

            var xnHeight = xnlTileSet.SelectSingleNode("sm:Height", nsmgr);
            if (xnHeight != null)
            {
                int height;
                if (!Int32.TryParse(xnHeight.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out height))
                    throw new ArgumentException("Invalid width on tileset '" + tileSet.Name + "'");
                tileSet.Height = height;
            }

            var xnFormat = xnlTileSet.SelectSingleNode("sm:Format", nsmgr);
            if (xnFormat != null)
                tileSet.Format = xnFormat.InnerText;

            var xnStyles = xnlTileSet.SelectSingleNode("sm:Styles", nsmgr);
            if (xnStyles != null)
                tileSet.Styles.AddRange(xnStyles.InnerText.Split(new[] {','}));

            var xnBoundingBox = xnlTileSet.SelectSingleNode("sm:BoundingBox", nsmgr);
            if (xnBoundingBox != null)
            {
                var att = xnBoundingBox.Attributes;
                double minx, miny, maxx, maxy;
                if (att == null || (
                    !double.TryParse(att["minx"].Value, NumberStyles.Any, Map.NumberFormatEnUs, out minx) &
                    !double.TryParse(att["miny"].Value, NumberStyles.Any, Map.NumberFormatEnUs, out miny) &
                    !double.TryParse(att["maxx"].Value, NumberStyles.Any, Map.NumberFormatEnUs, out maxx) &
                    !double.TryParse(att["maxy"].Value, NumberStyles.Any, Map.NumberFormatEnUs, out maxy)))
                {
                    throw new ArgumentException("Invalid LatLonBoundingBox on tileset '" + tileSet.Name + "'");
                }
                tileSet.BoundingBox = new Envelope(minx, maxx, miny, maxy);
            }

            var xnResolutions = xnlTileSet.SelectSingleNode("sm:Resolutions", nsmgr);
            if (xnResolutions != null)
            {
                var resolutions = xnResolutions.InnerText.TrimEnd(' ').Split(new[] {' '});
                foreach (string resolutionStr in resolutions)
                {
                    if (resolutionStr != "")
                    {
                        double resolution;
                        if (!Double.TryParse(resolutionStr, NumberStyles.Any, Map.NumberFormatEnUs, out resolution))
                            throw new ArgumentException("Invalid resolution on tileset '" + tileSet.Name + "'");
                        tileSet.Resolutions.Add(resolution);
                    }
                }
            }
            return tileSet;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the local tile cache. Use a local tile cache if you also want to store the tiles on 
        /// the local machine. 
        /// </summary>
        public ITileCache TileCache
        {
            get { return _tileCache; }
            set { _tileCache = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the name of the tile set
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets a string describing the spatial reference system of the tile set
        /// </summary>
        public string Srs
        {
            get { return _srs; }
            set { _srs = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the extent of the tile set
        /// </summary>
        public Envelope BoundingBox
        {
            get { return _boundingBox; }
            set { _boundingBox = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the resolutions covered by this tile set.
        /// </summary>
        public List<double> Resolutions
        {
            get { return _resolutions; }
            set { _resolutions = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the width (in pixel) of each tile.
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the height (in pixel) of each tile.
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the tile image format in this tile set.
        /// </summary>
        public string Format
        {
            get { return _format; }
            set { _format = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the layers contained in this tile set.
        /// </summary>
        public List<string> Layers
        {
            get { return _layers; }
            set { _layers = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the styles covered in this tile set.
        /// </summary>
        public List<string> Styles
        {
            get { return _styles; }
            set { _styles = value; }
        }

        #endregion
    }
}