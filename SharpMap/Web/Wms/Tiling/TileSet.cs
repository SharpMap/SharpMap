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
using SharpMap.Geometries;

namespace SharpMap.Web.Wms.Tiling
{
    public class TileSet
    {
        #region Fields

        private BoundingBox boundingBox = null;
        private string format;
        private int height = 0;
        private List<string> layers = new List<string>();
        private string name;
        private List<double> resolutions = new List<double>();
        private string srs;
        private List<string> styles = new List<string>();
        private ITileCache tileCache;
        private int width = 0;

        #endregion

        /// <summary>
        /// Checks if the TileSet is initialized and throws an exception if not
        /// </summary>
        public void Verify()
        {
            if (layers.Count == 0)
            {
                throw new Exception(String.Format("No Layers were added for the TileSet"));
            }
            if (srs == String.Empty)
            {
                throw new Exception(String.Format("The SRS was not set for TileSet '{0}'", Name));
            }
            if (boundingBox == null)
            {
                throw new Exception(String.Format("The BoundingBox was not set for TileSet '{0}'", Name));
            }
            if (resolutions.Count == 0)
            {
                throw new Exception(String.Format("No Resolutions were added for TileSet '{0}'", Name));
            }
            if (width == 0)
            {
                throw new Exception(String.Format("The Width was not set for TileSet '{0}'", Name));
            }
            if (height == 0)
            {
                throw new Exception(String.Format("The Height was not set for TileSet '{0}'", Name));
            }
            if (format == String.Empty)
            {
                throw new Exception(String.Format("The Format was not set for TileSet '{0}'", Name));
            }

            //Note: An empty Style list is allowed so dont check for Style

            //TODO: BoundingBox should contain a SRS, and we should check if BoundingBox.Srs is the same
            //as tileset Srs because we do not project one to the other. 
        }

        private static string CreateDefaultName(List<string> layers)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string layer in layers)
            {
                stringBuilder.Append(layer + ",");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Parses the TileSets from the VendorSpecificCapabilities node of the WMS Capabilties 
        /// and adds them to the TileSets member
        /// </summary>
        /// <param name="xnlVendorSpecificCapabilities">The VendorSpecificCapabilities node of the Capabilties</param>
        /// <param name="nsmgr"></param>
        public static SortedList<string, TileSet> ParseVendorSpecificCapabilitiesNode(
            XmlNode xnlVendorSpecificCapabilities)
        {
            SortedList<string, TileSet> tileSets = new SortedList<string, TileSet>();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("sm", "");

            XmlNodeList xnlTileSets = xnlVendorSpecificCapabilities.SelectNodes("sm:TileSet", nsmgr);

            foreach (XmlNode xnlTileSet in xnlTileSets)
            {
                TileSet tileSet = ParseTileSetNode(xnlTileSet, nsmgr);
                tileSets.Add(tileSet.Name+"-" + tileSet.Format +"-"+tileSet.Srs , tileSet);
            }
            return tileSets;
        }

        private static TileSet ParseTileSetNode(XmlNode xnlTileSet, XmlNamespaceManager nsmgr)
        {
            TileSet tileSet = new TileSet();

            XmlNode xnLayers = xnlTileSet.SelectSingleNode("sm:Layers", nsmgr);
            if (xnLayers != null)
                tileSet.Layers.AddRange(xnLayers.InnerText.Split(new char[] {','}));

            tileSet.Name = CreateDefaultName(tileSet.layers);

            XmlNode xnSRS = xnlTileSet.SelectSingleNode("sm:SRS", nsmgr);
            if (xnSRS != null)
                tileSet.Srs = xnSRS.InnerText;

            XmlNode xnWidth = xnlTileSet.SelectSingleNode("sm:Width", nsmgr);
            if (xnWidth != null)
            {
                int width;
                if (!Int32.TryParse(xnWidth.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out width))
                    throw new ArgumentException("Invalid width on tileset '" + tileSet.Name + "'");
                tileSet.Width = width;
            }

            XmlNode xnHeight = xnlTileSet.SelectSingleNode("sm:Height", nsmgr);
            if (xnHeight != null)
            {
                int height;
                if (!Int32.TryParse(xnWidth.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out height))
                    throw new ArgumentException("Invalid width on tileset '" + tileSet.Name + "'");
                tileSet.Height = height;
            }

            XmlNode xnFormat = xnlTileSet.SelectSingleNode("sm:Format", nsmgr);
            if (xnFormat != null)
                tileSet.Format = xnFormat.InnerText;

            XmlNode xnStyles = xnlTileSet.SelectSingleNode("sm:Styles", nsmgr);
            if (xnStyles != null)
                tileSet.Styles.AddRange(xnStyles.InnerText.Split(new char[] {','}));

            XmlNode xnBoundingBox = xnlTileSet.SelectSingleNode("sm:BoundingBox", nsmgr);
            if (xnBoundingBox != null)
            {
                double minx = 0;
                double miny = 0;
                double maxx = 0;
                double maxy = 0;
                if
                    (
                    !double.TryParse(xnBoundingBox.Attributes["minx"].Value, NumberStyles.Any, Map.NumberFormatEnUs,
                                     out minx) &
                    !double.TryParse(xnBoundingBox.Attributes["miny"].Value, NumberStyles.Any, Map.NumberFormatEnUs,
                                     out miny) &
                    !double.TryParse(xnBoundingBox.Attributes["maxx"].Value, NumberStyles.Any, Map.NumberFormatEnUs,
                                     out maxx) &
                    !double.TryParse(xnBoundingBox.Attributes["maxy"].Value, NumberStyles.Any, Map.NumberFormatEnUs,
                                     out maxy)
                    )
                {
                    throw new ArgumentException("Invalid LatLonBoundingBox on tileset '" + tileSet.Name + "'");
                }
                tileSet.BoundingBox = new BoundingBox(minx, miny, maxx, maxy);
            }

            XmlNode xnResolutions = xnlTileSet.SelectSingleNode("sm:Resolutions", nsmgr);
            if (xnResolutions != null)
            {
                double resolution;
                string[] resolutions = xnResolutions.InnerText.TrimEnd(' ').Split(new char[] {' '});
                foreach (string resolutionStr in resolutions)
                {
                    if (resolutionStr != "")
                    {
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
            get { return tileCache; }
            set { tileCache = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Srs
        {
            get { return srs; }
            set { srs = value; }
        }

        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }

        public List<double> Resolutions
        {
            get { return resolutions; }
            set { resolutions = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public string Format
        {
            get { return format; }
            set { format = value; }
        }

        public List<string> Layers
        {
            get { return layers; }
            set { layers = value; }
        }

        public List<string> Styles
        {
            get { return styles; }
            set { styles = value; }
        }

        #endregion
    }
}