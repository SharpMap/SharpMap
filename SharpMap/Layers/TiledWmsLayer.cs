// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
// Copyright 2007 - Paul den Dulk (Geodan) - Created TiledWmsLayer from WmsLayer
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
using System.Drawing;
using SharpMap.Geometries;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.ObjectModel;
using SharpMap.Web.Wms.Tiling;
using SharpMap.Web.Wms;

namespace SharpMap.Layers
{
  /// <summary>
  /// Client layer for WMS-C service
  /// </summary>
  /// <remarks>
  /// Initialize the TiledWmsLayer with the url to the capabilities documtent
  /// and it will set the remaining BoundingBox property and proper requests that changes between the requests.
  /// See the example below.
  /// </remarks>
  /// <example>
  /// The following example creates a map with a TiledWmsLayer the metacarta tile server
  /// <code lang="C#">
  /// map = new SharpMap.Map(mapImage1.Size);
  /// string url = "http://labs.metacarta.com/wms-c/tilecache.py?version=1.1.1&amp;request=GetCapabilities&amp;service=wms-c";
  /// TiledWmsLayer tiledWmsLayer = new TiledWmsLayer("Metacarta", url);
  /// tiledWmsLayer.TileSetsActive.Add(tiledWmsLayer.TileSets["satellite"].Name);
  /// map.Layers.Add(tiledWmsLayer);
  /// map.ZoomToBox(new SharpMap.Geometries.BoundingBox(-180.0, -90.0, 180.0, 90.0));
  /// </code>
  /// </example>
  
  
  public class TiledWmsLayer : SharpMap.Layers.Layer, ILayer
  {
    #region Fields

    private SharpMap.Web.Wms.Client _WmsClient;
    private Dictionary<string, string> _CustomParameters = new Dictionary<string, string>();
    private Boolean _ContinueOnError;
    private ImageAttributes _ImageAttributes = new ImageAttributes();
    private System.Net.ICredentials _Credentials;
    private System.Net.WebProxy _Proxy;
    private int _TimeOut;
    private SortedList<string, TileSet> _TileSets = new SortedList<string, TileSet>();
    private Collection<string> _TileSetsActive = new Collection<string>();

    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new layer, and downloads and parses the service description
    /// </summary>
    /// <remarks>In and ASP.NET application the service description is automatically cached for 24 hours when not specified</remarks>
    /// <param name="layername">Layername</param>
    /// <param name="url">Url of WMS server's Capabilties</param>
    public TiledWmsLayer(string layername, string url)
      : this(layername, url, new TimeSpan(24, 0, 0))
    {
    }

    /// <summary>
    /// Initializes a new layer, and downloads and parses the service description
    /// </summary>
    /// <param name="layername">Layername</param>
    /// <param name="url">Url of WMS server's Capabilties</param>
    /// <param name="cachetime">Time for caching Service Description (ASP.NET only)</param>
    public TiledWmsLayer(string layername, string url, TimeSpan cachetime)
      : this(layername, url, cachetime, null)
    {
    }

    /// <summary>
    /// Initializes a new layer, and downloads and parses the service description
    /// </summary>
    /// <remarks>In and ASP.NET application the service description is automatically cached for 24 hours when not specified</remarks>
    /// <param name="layername">Layername</param>
    /// <param name="url">Url of WMS server's Capabilties</param>
    /// <param name="proxy">Proxy</param>
    public TiledWmsLayer(string layername, string url, System.Net.WebProxy proxy)
      : this(layername, url, new TimeSpan(24, 0, 0), proxy)
    {
    }

    /// <summary>
    /// Initializes a new layer, and downloads and parses the service description
    /// </summary>
    /// <param name="layername">Layername</param>
    /// <param name="url">Url of WMS server's Capabilties</param>
    /// <param name="cachetime">Time for caching Service Description (ASP.NET only)</param>
    /// <param name="proxy">Proxy</param>
    public TiledWmsLayer(string layername, string url, TimeSpan cachetime, System.Net.WebProxy proxy)
    {
      _Proxy = proxy;
      _TimeOut = 10000;
      this.LayerName = layername;
      _ContinueOnError = true;

      if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Cache["SharpMap_WmsClient_" + url] != null)
      {
        _WmsClient = (Client)System.Web.HttpContext.Current.Cache["SharpMap_WmsClient_" + url];
      }
      else
      {
        _WmsClient = new Client(url, _Proxy);
        if (System.Web.HttpContext.Current != null)
          System.Web.HttpContext.Current.Cache.Insert("SharpMap_WmsClient_" + url, _WmsClient, null,
            System.Web.Caching.Cache.NoAbsoluteExpiration, cachetime);

      }
      _TileSets = TileSet.ParseVendorSpecificCapabilitiesNode(_WmsClient.VendorSpecificCapabilities);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Provides the base authentication interface for retrieving credentials for Web client authentication.
    /// </summary>
    public System.Net.ICredentials Credentials
    {
      get { return _Credentials; }
      set { _Credentials = value; }
    }

    /// <summary>
    /// Gets or sets the proxy used for requesting a webresource
    /// </summary>
    public System.Net.WebProxy Proxy
    {
      get { return _Proxy; }
      set { _Proxy = value; }
    }

    /// <summary>
    /// Timeout of webrequest in milliseconds. Defaults to 10 seconds
    /// </summary>
    public int TimeOut
    {
      get { return _TimeOut; }
      set { _TimeOut = value; }
    }

    public Collection<string> TileSetsActive
    {
      get { return _TileSetsActive; }
    }

    /// <summary>
    /// Gets the collection of TileSets that will be rendered
    /// </summary>
    public SortedList<string, TileSet> TileSets
    {
      get { return _TileSets; }
    }

    /// <summary>
    /// Specifies whether to throw an exception if the Wms request failed, or to just skip rendering the layer. 
    /// </summary>
    public Boolean ContinueOnError
    {
      get { return _ContinueOnError; }
      set { _ContinueOnError = value; }
    }

    /// <summary>
    /// Gets the list of available formats
    /// </summary>
    public Collection<string> OutputFormats
    {
      get { return _WmsClient.GetMapOutputFormats; }
    }

    #endregion

    #region ILayer Members

    // <summary>
    /// Renders the layer
    /// </summary>
    /// <param name="g">Graphics object reference</param>
    /// <param name="map">Map which is rendered</param>
    public override void Render(System.Drawing.Graphics g, Map map)
    {
      System.Drawing.Bitmap bitmap = null;

      try
      {
        foreach (string key in _TileSetsActive)
        {
          TileSet tileSet = _TileSets[key];

          tileSet.Verify();

          List<BoundingBox> tileExtents = TileExtents.GetTileExtents(tileSet, map.Envelope, map.PixelSize);

          //TODO: Retrieve several tiles at the same time asynchronously to improve performance. PDD.
          foreach (BoundingBox tileExtent in tileExtents)
          {
            if (bitmap != null) { bitmap.Dispose(); }

            if ((tileSet.TileCache != null) && (tileSet.TileCache.ContainsTile(tileExtent)))
            {
              bitmap = tileSet.TileCache.GetTile(tileExtent);
            }
            else
            {
              bitmap = WmsGetMap(tileExtent, tileSet);
              if ((tileSet.TileCache != null) && (bitmap != null))
              {
                tileSet.TileCache.AddTile(tileExtent, bitmap);
              }
            }

            if (bitmap != null)
            {
              PointF destMin = SharpMap.Utilities.Transform.WorldtoMap(tileExtent.Min, map);
              PointF destMax = SharpMap.Utilities.Transform.WorldtoMap(tileExtent.Max, map);

              #region Comment on BorderBug correction
              // Even when tiles border to one another without any space between them there are 
              // seams visible between the tiles in the map image.
              // This problem could be resolved with the solution suggested here:
              // http://www.codeproject.com/csharp/BorderBug.asp
              // The suggested correction value of 0.5f still results in seams in some cases, not so with 0.4999f.
              // Also it was necessary to apply Math.Round and Math.Ceiling on the destination rectangle.
              // PDD.
              #endregion

              float correction = 0.4999f;
              RectangleF srcRect = new RectangleF(0 - correction, 0 - correction, tileSet.Width, tileSet.Height);

              InterpolationMode tempInterpolationMode = g.InterpolationMode;
              g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

              //TODO: Allow custom image attributes for each TileSet.

              int x = (int)Math.Round(destMin.X);
              int y = (int)Math.Round(destMax.Y);
              int width = (int)Math.Round(destMax.X - x);
              int height = (int)Math.Round(destMin.Y - y);

              g.DrawImage(bitmap, new Rectangle(x, y, width, height),
                srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height,
                GraphicsUnit.Pixel, _ImageAttributes);

              g.InterpolationMode = tempInterpolationMode; //Put InterpolationMode back so drawing of other layers is not affected.
            }
          }
        }
      }
      finally
      {
        if (bitmap != null)
        {
          bitmap.Dispose();
        }
      }
    }

    /// <summary>
    /// Returns the extent of the layer
    /// </summary>
    /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
    public override SharpMap.Geometries.BoundingBox Envelope
    {
      get
      {
        return _WmsClient.Layer.LatLonBoundingBox; //TODO: no box is allowed in capabilities so check for it
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Appends a custom parameter name-value pair to the WMS request
    /// </summary>
    /// <param name="name">Name of custom parameter</param>
    /// <param name="value">Value of custom parameter</param>
    public void AddCustomParameter(string name, string value)
    {
      _CustomParameters.Add(name, value);
    }

    /// <summary>
    /// Removes a custom paramter name-value pair from the WMS request
    /// </summary>
    /// <param name="name">Name of the custom parameter to remove</param>
    public void RemoveCustomParameter(string name)
    {
      _CustomParameters.Remove(name);
    }

    /// <summary>
    /// Removes all custom parameter from the WMS request
    /// </summary>
    public void RemoveAllCustomParameters()
    {
      _CustomParameters.Clear();
    }

    private string GetRequestUrl(SharpMap.Geometries.BoundingBox box, TileSet tileSet)
    {
      SharpMap.Web.Wms.Client.WmsOnlineResource resource = GetPreferredMethod();
      System.Text.StringBuilder strReq = new StringBuilder(resource.OnlineResource);
      if (!resource.OnlineResource.Contains("?"))
        strReq.Append("?");
      if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
        strReq.Append("&");

      strReq.AppendFormat(SharpMap.Map.numberFormat_EnUS, "&REQUEST=GetMap&BBOX={0},{1},{2},{3}",
        box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
      strReq.AppendFormat("&WIDTH={0}&Height={1}", tileSet.Width, tileSet.Height);
      strReq.Append("&LAYERS="); // LAYERS is set in caps because the current version of tilecache.py does not accept mixed case (a little bug)
      if (tileSet.Layers != null && tileSet.Layers.Count > 0)
      {
        foreach (string layer in tileSet.Layers)
          strReq.AppendFormat("{0},", layer);
        strReq.Remove(strReq.Length - 1, 1);
      }
      strReq.AppendFormat("&FORMAT={0}", tileSet.Format);

      if (_WmsClient.WmsVersion == "1.3.0")
        strReq.AppendFormat("&CRS={0}", tileSet.Srs);
      else
        strReq.AppendFormat("&SRS={0}", tileSet.Srs);
      strReq.AppendFormat("&VERSION={0}", _WmsClient.WmsVersion);

      if (tileSet.Styles != null && tileSet.Styles.Count > 0)
      {
        strReq.Append("&STYLES=");
        foreach (string style in tileSet.Styles)
          strReq.AppendFormat("{0},", style);
        strReq.Remove(strReq.Length - 1, 1);
      }

      if (_CustomParameters != null && _CustomParameters.Count > 0)
      {
        foreach (string name in _CustomParameters.Keys)
        {
          string value = _CustomParameters[name];
          strReq.AppendFormat("&{0}={1}", name, value);
        }
      }

      return strReq.ToString();
    }

    private Bitmap WmsGetMap(BoundingBox extent, TileSet tileSet)
    {
      Stream responseStream = null;
      System.Drawing.Bitmap bitmap = null;

      SharpMap.Web.Wms.Client.WmsOnlineResource resource = GetPreferredMethod();
      string requestUrl = GetRequestUrl(extent, tileSet);
      Uri myUri = new Uri(requestUrl);
      System.Net.WebRequest webRequest = System.Net.WebRequest.Create(myUri);
      webRequest.Method = resource.Type;
      webRequest.Timeout = this.TimeOut;

      if (this.Credentials != null)
        webRequest.Credentials = this.Credentials;
      else
        webRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;

      if (this.Proxy != null)
        webRequest.Proxy = this.Proxy;

      System.Net.HttpWebResponse webResponse = null;

      try
      {
        webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse();

        if (webResponse.ContentType.StartsWith("image"))
        {
          responseStream = webResponse.GetResponseStream();
          bitmap = (Bitmap)System.Drawing.Bitmap.FromStream(responseStream);
          return (Bitmap)bitmap;
        }
        else
        {
          //if the result was not an image retrieve content anyway for debugging.
          responseStream = webResponse.GetResponseStream();
          StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);
          StringWriter stringWriter = new StringWriter();
          stringWriter.Write(readStream.ReadToEnd());
          string message = "Failed to retrieve image from the WMS in layer '" + this.LayerName + "'. Was expecting image but received this: " + stringWriter.ToString();
          HandleGetMapException(message, null); ;
          return null;
        }
      }
      catch (System.Net.WebException webEx)
      {
        string message = "There was a problem connecting to the WMS server when rendering layer '" + this.LayerName + "'";
        HandleGetMapException(message, webEx);
      }
      catch (System.Exception ex)
      {
        string message = "There was a problem while retrieving the image from the WMS in layer '" + this.LayerName + "'";
        HandleGetMapException(message, ex);
      }
      finally
      {
        if (webResponse != null)
        {
          webResponse.Close();
        }
        if (responseStream != null)
        {
          responseStream.Close();
          responseStream.Dispose();
        }
      }
      return bitmap;
    }

    private void HandleGetMapException(string message, Exception ex)
    {
      if (this.ContinueOnError)
      {
        System.Diagnostics.Trace.Write(message);
      }
      else
      {
        throw (new SharpMap.Rendering.Exceptions.RenderException(message, ex));
      }
    }

		private SharpMap.Web.Wms.Client.WmsOnlineResource GetPreferredMethod()
		{
			//We prefer get. Seek for supported 'get' method
			for (int i = 0; i < wmsClient.GetMapRequests.Length; i++)
				if (wmsClient.GetMapRequests[i].Type.ToLower() == "get")
					return wmsClient.GetMapRequests[i];
			//Next we prefer the 'post' method
			for (int i = 0; i < wmsClient.GetMapRequests.Length; i++)
				if (wmsClient.GetMapRequests[i].Type.ToLower() == "post")
					return wmsClient.GetMapRequests[i];
			return wmsClient.GetMapRequests[0];
		}
    
    #endregion

    #region ICloneable Members

    /// <summary>
    /// Clones the object
    /// </summary>
    /// <returns></returns>
    public override object Clone()
    {
      throw new NotImplementedException();
    }

    #endregion


  }
}
