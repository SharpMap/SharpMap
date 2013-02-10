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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using GeoAPI.Geometries;
using SharpMap.Rendering.Exceptions;
using SharpMap.Utilities;
using SharpMap.Web.Wms;
using SharpMap.Web.Wms.Tiling;
using Common.Logging;

namespace SharpMap.Layers
{
    /// <summary>
    /// Client layer for WMS-C service
    /// </summary>
    /// <remarks>
    /// Initialize the TiledWmsLayer with the url to the capabilities document
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
    [Obsolete("use TileLayer instead") ]

    public class TiledWmsLayer : Layer, ILayer
    {
        ILog logger = LogManager.GetLogger(typeof(TiledWmsLayer));

        #region Fields

        private Boolean _ContinueOnError;
        private ICredentials _Credentials;
        private Dictionary<string, string> _CustomParameters = new Dictionary<string, string>();
        private ImageAttributes _ImageAttributes = new ImageAttributes();
        private WebProxy _Proxy;
        private SortedList<string, TileSet> _TileSets = new SortedList<string, TileSet>();
        private Collection<string> _TileSetsActive = new Collection<string>();
        private int _TimeOut;
        private Client _WmsClient;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new layer, and downloads and parses the service description
        /// </summary>
        /// <remarks>In and ASP.NET application the service description is automatically cached for 24 hours when not specified</remarks>
        /// <param name="layername">Layername</param>
        /// <param name="url">Url of WMS server's Capabilities</param>
        public TiledWmsLayer(string layername, string url)
            : this(layername, url, new TimeSpan(24, 0, 0))
        {
            _ImageAttributes.SetWrapMode(WrapMode.TileFlipXY);
        }

        /// <summary>
        /// Initializes a new layer, and downloads and parses the service description
        /// </summary>
        /// <param name="layername">Layername</param>
        /// <param name="url">Url of WMS server's Capabilities</param>
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
        /// <param name="url">Url of WMS server's Capabilities</param>
        /// <param name="proxy">Proxy</param>
        public TiledWmsLayer(string layername, string url, WebProxy proxy)
            : this(layername, url, new TimeSpan(24, 0, 0), proxy)
        {
        }

        /// <summary>
        /// Initializes a new layer, and downloads and parses the service description
        /// </summary>
        /// <param name="layername">Layername</param>
        /// <param name="url">Url of WMS server's Capabilities</param>
        /// <param name="cachetime">Time for caching Service Description (ASP.NET only)</param>
        /// <param name="proxy">Proxy</param>
        public TiledWmsLayer(string layername, string url, TimeSpan cachetime, WebProxy proxy)
        {
            _Proxy = proxy;
            _TimeOut = 10000;
            LayerName = layername;
            _ContinueOnError = true;

            if (!Web.HttpCacheUtility.TryGetValue("SharpMap_WmsClient_" + url, out _WmsClient))
            {
                if (logger.IsDebugEnabled)
                    logger.Debug("Creating new client for url " + url);
                _WmsClient = new Client(url, _Proxy, _Credentials);

                if (!Web.HttpCacheUtility.TryAddValue("SharpMap_WmsClient_" + url, _WmsClient))
                {
                    if (logger.IsDebugEnabled)
                        logger.Debug("Adding client to Cache for url " + url + " failed");
                }
            }
            _TileSets = TileSet.ParseVendorSpecificCapabilitiesNode(_WmsClient.VendorSpecificCapabilities);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Provides the base authentication interface for retrieving credentials for Web client authentication.
        /// </summary>
        public ICredentials Credentials
        {
            get { return _Credentials; }
            set { _Credentials = value; }
        }

        /// <summary>
        /// Gets or sets the proxy used for requesting a webresource
        /// </summary>
        public WebProxy Proxy
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

        /// <summary>
        /// Gets a list of tile sets that are currently active
        /// </summary>
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

        // <summary>

        #region ILayer Members

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            Bitmap bitmap = null;

            try
            {
                foreach (string key in _TileSetsActive)
                {
                    TileSet tileSet = _TileSets[key];

                    tileSet.Verify();

                    List<Envelope> tileExtents = TileExtents.GetTileExtents(tileSet, map.Envelope, map.PixelSize);

                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("TileCount: {0}", tileExtents.Count);

                    //TODO: Retrieve several tiles at the same time asynchronously to improve performance. PDD.
                    foreach (Envelope tileExtent in tileExtents)
                    {
                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                        }

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
                            PointF destMin = Transform.WorldtoMap(tileExtent.Min(), map);
                            PointF destMax = Transform.WorldtoMap(tileExtent.Max(), map);

                            double minX = (int) Math.Round(destMin.X);
                            double minY = (int) Math.Round(destMax.Y);
                            double maxX = (int) Math.Round(destMax.X);
                            double maxY = (int) Math.Round(destMin.Y);

                            g.DrawImage(bitmap,
                                        new Rectangle((int) minX, (int) minY, (int) (maxX - minX), (int) (maxY - minY)),
                                        0, 0, tileSet.Width, tileSet.Height,
                                        GraphicsUnit.Pixel, _ImageAttributes);
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
        public override Envelope Envelope
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
        /// Removes a custom parameter name-value pair from the WMS request
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

        private string GetRequestUrl(Envelope box, TileSet tileSet)
        {
            Client.WmsOnlineResource resource = GetPreferredMethod();
            StringBuilder strReq = new StringBuilder(resource.OnlineResource);
            if (!resource.OnlineResource.Contains("?"))
                strReq.Append("?");
            if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
                strReq.Append("&");

            strReq.AppendFormat(Map.NumberFormatEnUs, "&REQUEST=GetMap&BBOX={0},{1},{2},{3}",
                                box.MinX, box.MinY, box.MaxX, box.MaxY);
            strReq.AppendFormat("&WIDTH={0}&Height={1}", tileSet.Width, tileSet.Height);
            strReq.Append("&LAYERS=");
                // LAYERS is set in caps because the current version of tilecache.py does not accept mixed case (a little bug)
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

        private Bitmap WmsGetMap(Envelope extent, TileSet tileSet)
        {
            Stream responseStream = null;
            Bitmap bitmap = null;

            Client.WmsOnlineResource resource = GetPreferredMethod();
            string requestUrl = GetRequestUrl(extent, tileSet);
            Uri myUri = new Uri(requestUrl);
            WebRequest webRequest = WebRequest.Create(myUri);
            webRequest.Method = resource.Type;
            webRequest.Timeout = TimeOut;

            if (Credentials != null)
                webRequest.Credentials = Credentials;
            else
                webRequest.Credentials = CredentialCache.DefaultCredentials;

            if (Proxy != null)
                webRequest.Proxy = Proxy;

            HttpWebResponse webResponse = null;

            try
            {
                webResponse = (HttpWebResponse) webRequest.GetResponse();

                if (webResponse.ContentType.StartsWith("image"))
                {
                    responseStream = webResponse.GetResponseStream();
                    bitmap = (Bitmap) Bitmap.FromStream(responseStream);
                    return (Bitmap) bitmap;
                }
                else
                {
                    //if the result was not an image retrieve content anyway for debugging.
                    responseStream = webResponse.GetResponseStream();
                    StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);
                    StringWriter stringWriter = new StringWriter();
                    stringWriter.Write(readStream.ReadToEnd());
                    string message = "Failed to retrieve image from the WMS in layer '" + LayerName +
                                     "'. Was expecting image but received this: " + stringWriter.ToString();
                    HandleGetMapException(message, null);
                    ;
                    return null;
                }
            }
            catch (WebException webEx)
            {
                string message = "There was a problem connecting to the WMS server when rendering layer '" + LayerName +
                                 "'";
                HandleGetMapException(message, webEx);
            }
            catch (Exception ex)
            {
                string message = "There was a problem while retrieving the image from the WMS in layer '" + LayerName +
                                 "'";
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
            if (ContinueOnError)
            {
                Trace.Write(message);
            }
            else
            {
                throw (new RenderException(message, ex));
            }
        }

        private Client.WmsOnlineResource GetPreferredMethod()
        {
            //We prefer get. Seek for supported 'get' method
            for (int i = 0; i < _WmsClient.GetMapRequests.Length; i++)
                if (_WmsClient.GetMapRequests[i].Type.ToLower() == "get")
                    return _WmsClient.GetMapRequests[i];
            //Next we prefer the 'post' method
            for (int i = 0; i < _WmsClient.GetMapRequests.Length; i++)
                if (_WmsClient.GetMapRequests[i].Type.ToLower() == "post")
                    return _WmsClient.GetMapRequests[i];
            return _WmsClient.GetMapRequests[0];
        }

        #endregion

        private static Rectangle RoundRectangle(RectangleF dest)
        {
            double minX = Math.Round(dest.X);
            double minY = Math.Round(dest.Y);
            double maxX = Math.Round(dest.Right);
            double maxY = Math.Round(dest.Bottom);
            return new Rectangle((int) minX, (int) minY, (int) (maxX - minX), (int) (maxY - minY));
        }
    }
}