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
using System.Text;
using SharpMap.Geometries;
using System.Xml;

namespace SharpMap.Web.Wms.Tiling
{
  public class TileSet
  {
    #region Fields

    private string name;
    private string srs;
    private BoundingBox boundingBox = null;
    private List<double> resolutions = new List<double>();
    private int width = 0;
    private int height = 0;
    private string format;
    private List<string> layers = new List<string>();
    private List<string> styles = new List<string>();
    private ITileCache tileCache;
    
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
        throw new Exception(String.Format("The SRS was not set for TileSet '{0}'", this.Name));
      }
      if (boundingBox == null)
      {
        throw new Exception(String.Format("The BoundingBox was not set for TileSet '{0}'", this.Name));
      }
      if (resolutions.Count == 0)
      {
        throw new Exception(String.Format("No Resolutions were added for TileSet '{0}'", this.Name));
      }
      if (width == 0)
      {
        throw new Exception(String.Format("The Width was not set for TileSet '{0}'", this.Name));
      }
      if (height == 0)
      {
        throw new Exception(String.Format("The Height was not set for TileSet '{0}'", this.Name));
      }
      if (format == String.Empty)
      {
        throw new Exception(String.Format("The Format was not set for TileSet '{0}'", this.Name));
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

    public override string ToString()
    {
      return this.Name;
    }

    /// <summary>
    /// Parses the TileSets from the VendorSpecificCapabilities node of the WMS Capabilties 
    /// and adds them to the TileSets member
    /// </summary>
    /// <param name="xnlVendorSpecificCapabilities">The VendorSpecificCapabilities node of the Capabilties</param>
    /// <param name="nsmgr"></param>
    public static SortedList<string, TileSet> ParseVendorSpecificCapabilitiesNode(XmlNode xnlVendorSpecificCapabilities)
    {
      SortedList<string, TileSet> tileSets = new SortedList<string, TileSet>();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
      nsmgr.AddNamespace("sm", "");

      XmlNodeList xnlTileSets = xnlVendorSpecificCapabilities.SelectNodes("sm:TileSet", nsmgr);

      foreach (XmlNode xnlTileSet in xnlTileSets)
      {
        TileSet tileSet = ParseTileSetNode(xnlTileSet, nsmgr);
        tileSets.Add(tileSet.Name, tileSet);
      }
      return tileSets;
    }

    private static TileSet ParseTileSetNode(XmlNode xnlTileSet, XmlNamespaceManager nsmgr)
    {
      TileSet tileSet = new TileSet();

      XmlNode xnLayers = xnlTileSet.SelectSingleNode("sm:Layers", nsmgr);
      if (xnLayers != null)
        tileSet.Layers.AddRange(xnLayers.InnerText.Split(new char[] { ',' }));

      tileSet.Name = CreateDefaultName(tileSet.layers);

      XmlNode xnSRS = xnlTileSet.SelectSingleNode("sm:SRS", nsmgr);
      if (xnSRS != null)
        tileSet.Srs = xnSRS.InnerText;

      XmlNode xnWidth = xnlTileSet.SelectSingleNode("sm:Width", nsmgr);
      if (xnWidth != null)
      {
        int width;
        if (!Int32.TryParse(xnWidth.InnerText, System.Globalization.NumberStyles.Any, SharpMap.Map.numberFormat_EnUS, out width))
          throw new ArgumentException("Invalid width on tileset '" + tileSet.Name + "'");
        tileSet.Width = width;
      }

      XmlNode xnHeight = xnlTileSet.SelectSingleNode("sm:Height", nsmgr);
      if (xnHeight != null)
      {
        int height;
        if (!Int32.TryParse(xnWidth.InnerText, System.Globalization.NumberStyles.Any, SharpMap.Map.numberFormat_EnUS, out height))
          throw new ArgumentException("Invalid width on tileset '" + tileSet.Name + "'");
        tileSet.Height = height;
      }

      XmlNode xnFormat = xnlTileSet.SelectSingleNode("sm:Format", nsmgr);
      if (xnFormat != null)
        tileSet.Format = xnFormat.InnerText;

      XmlNode xnStyles = xnlTileSet.SelectSingleNode("sm:Styles", nsmgr);
      if (xnStyles != null)
        tileSet.Styles.AddRange(xnStyles.InnerText.Split(new char[] { ',' }));

      XmlNode xnBoundingBox = xnlTileSet.SelectSingleNode("sm:BoundingBox", nsmgr);
      if (xnBoundingBox != null)
      {
        double minx = 0; double miny = 0; double maxx = 0; double maxy = 0;
        if
        (
          !double.TryParse(xnBoundingBox.Attributes["minx"].Value, System.Globalization.NumberStyles.Any, SharpMap.Map.numberFormat_EnUS, out minx) &
          !double.TryParse(xnBoundingBox.Attributes["miny"].Value, System.Globalization.NumberStyles.Any, SharpMap.Map.numberFormat_EnUS, out miny) &
          !double.TryParse(xnBoundingBox.Attributes["maxx"].Value, System.Globalization.NumberStyles.Any, SharpMap.Map.numberFormat_EnUS, out maxx) &
          !double.TryParse(xnBoundingBox.Attributes["maxy"].Value, System.Globalization.NumberStyles.Any, SharpMap.Map.numberFormat_EnUS, out maxy)
        )
        {
          throw new ArgumentException("Invalid LatLonBoundingBox on tileset '" + tileSet.Name + "'");
        }
        tileSet.BoundingBox = new SharpMap.Geometries.BoundingBox(minx, miny, maxx, maxy);
      }

      XmlNode xnResolutions = xnlTileSet.SelectSingleNode("sm:Resolutions", nsmgr);
      if (xnResolutions != null)
      {
        double resolution;
        string[] resolutions = xnResolutions.InnerText.Split(new char[] { ' ' });
        foreach (string resolutionStr in resolutions)
        {
          if (!Double.TryParse(resolutionStr, System.Globalization.NumberStyles.Any, SharpMap.Map.numberFormat_EnUS, out resolution))
            throw new ArgumentException("Invalid resolution on tileset '" + tileSet.Name + "'");
          tileSet.Resolutions.Add(resolution);
        }
      }
      return tileSet;
    }

  }
}
