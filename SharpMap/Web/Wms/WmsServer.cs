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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Web;
using System.Xml;
using SharpMap.Geometries;
using SharpMap.Layers;

namespace SharpMap.Web.Wms
{
    /// <summary>
    /// This is a helper class designed to make it easy to create a WMS Service
    /// </summary>
    public static class WmsServer
    {
        /// <summary>
        /// Generates a WMS 1.3.0 compliant response based on a <see cref="SharpMap.Map"/> and the current HttpRequest.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Web Map Server implementation in SharpMap requires v1.3.0 compatible clients,
        /// and support the basic operations "GetCapabilities" and "GetMap"
        /// as required by the WMS v1.3.0 specification. SharpMap does not support the optional
        /// GetFeatureInfo operation for querying.
        /// </para>
        /// <example>
        /// Creating a WMS server in ASP.NET is very simple using the classes in the SharpMap.Web.Wms namespace.
        /// <code lang="C#">
        /// void page_load(object o, EventArgs e)
        /// {
        ///		//Get the path of this page
        ///		string url = (Request.Url.Query.Length>0?Request.Url.AbsoluteUri.Replace(Request.Url.Query,""):Request.Url.AbsoluteUri);
        ///		SharpMap.Web.Wms.Capabilities.WmsServiceDescription description =
        ///			new SharpMap.Web.Wms.Capabilities.WmsServiceDescription("Acme Corp. Map Server", url);
        ///		
        ///		// The following service descriptions below are not strictly required by the WMS specification.
        ///		
        ///		// Narrative description and keywords providing additional information 
        ///		description.Abstract = "Map Server maintained by Acme Corporation. Contact: webmaster@wmt.acme.com. High-quality maps showing roadrunner nests and possible ambush locations.";
        ///		description.Keywords.Add("bird");
        ///		description.Keywords.Add("roadrunner");
        ///		description.Keywords.Add("ambush");
        ///		
        ///		//Contact information 
        ///		description.ContactInformation.PersonPrimary.Person = "John Doe";
        ///		description.ContactInformation.PersonPrimary.Organisation = "Acme Inc";
        ///		description.ContactInformation.Address.AddressType = "postal";
        ///		description.ContactInformation.Address.Country = "Neverland";
        ///		description.ContactInformation.VoiceTelephone = "1-800-WE DO MAPS";
        ///		//Impose WMS constraints
        ///		description.MaxWidth = 1000; //Set image request size width
        ///		description.MaxHeight = 500; //Set image request size height
        ///		
        ///		//Call method that sets up the map
        ///		//We just add a dummy-size, since the wms requests will set the image-size
        ///		SharpMap.Map myMap = MapHelper.InitializeMap(new System.Drawing.Size(1,1));
        ///		
        ///		//Parse the request and create a response
        ///		SharpMap.Web.Wms.WmsServer.ParseQueryString(myMap,description);
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="map">Map to serve on WMS</param>
        /// <param name="description">Description of map service</param>
        public static void ParseQueryString(Map map, Capabilities.WmsServiceDescription description)
        {
            if (map == null)
                throw (new ArgumentException("Map for WMS is null"));
            if (map.Layers.Count == 0)
                throw (new ArgumentException("Map doesn't contain any layers for WMS service"));

            if (HttpContext.Current == null)
                throw (new ApplicationException(
                    "An attempt was made to access the WMS server outside a valid HttpContext"));

            HttpContext context = HttpContext.Current;

            //IgnoreCase value should be set according to the VERSION parameter
            //v1.3.0 is case sensitive, but since it causes a lot of problems with several WMS clients, we ignore casing anyway.
            bool ignorecase = true;

            //Check for required parameters
            //Request parameter is mandatory
            if (context.Request.Params["REQUEST"] == null)
            {
                WmsException.ThrowWmsException("Required parameter REQUEST not specified");
                return;
            }
            //Check if version is supported
            if (context.Request.Params["VERSION"] != null)
            {
                if (String.Compare(context.Request.Params["VERSION"], "1.3.0", ignorecase) != 0)
                {
                    WmsException.ThrowWmsException("Only version 1.3.0 supported");
                    return;
                }
            }
            else
            //Version is mandatory if REQUEST!=GetCapabilities. Check if this is a capabilities request, since VERSION is null
            {
                if (String.Compare(context.Request.Params["REQUEST"], "GetCapabilities", ignorecase) != 0)
                {
                    WmsException.ThrowWmsException("VERSION parameter not supplied");
                    return;
                }
            }

            //If Capabilities was requested
            if (String.Compare(context.Request.Params["REQUEST"], "GetCapabilities", ignorecase) == 0)
            {
                //Service parameter is mandatory for GetCapabilities request
                if (context.Request.Params["SERVICE"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter SERVICE not specified");
                    return;
                }

                if (String.Compare(context.Request.Params["SERVICE"], "WMS") != 0)
                    WmsException.ThrowWmsException(
                        "Invalid service for GetCapabilities Request. Service parameter must be 'WMS'");

                XmlDocument capabilities = Capabilities.GetCapabilities(map, description);
                context.Response.Clear();
                context.Response.ContentType = "text/xml";
                XmlWriter writer = XmlWriter.Create(context.Response.OutputStream);
                capabilities.WriteTo(writer);
                writer.Close();
                context.Response.End();
            }
            else if (String.Compare(context.Request.Params["REQUEST"], "GetFeatureInfo", ignorecase) == 0) //FeatureInfo Requested
            {
                if (context.Request.Params["LAYERS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter LAYERS not specified");
                    return;
                }
                if (context.Request.Params["STYLES"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter STYLES not specified");
                    return;
                }
                if (context.Request.Params["CRS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter CRS not specified");
                    return;
                }
                else if (context.Request.Params["CRS"] != "EPSG:" + map.Layers[0].SRID)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidCRS, "CRS not supported");
                    return;
                }
                if (context.Request.Params["BBOX"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter BBOX not specified");
                    return;
                }
                if (context.Request.Params["WIDTH"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter WIDTH not specified");
                    return;
                }
                if (context.Request.Params["HEIGHT"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter HEIGHT not specified");
                    return;
                }
                if (context.Request.Params["FORMAT"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter FORMAT not specified");
                    return;
                }
                if (context.Request.Params["QUERY_LAYERS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter QUERY_LAYERS not specified");
                    return;
                }
                if (context.Request.Params["INFO_FORMAT"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter INFO_FORMAT not specified");
                    return;
                }
                //parameters X&Y are not part of the 1.3.0 specification, but are included for backwards compatability with 1.1.1 (OpenLayers likes it when used together with wms1.1.1 services)
                if (context.Request.Params["X"] == null && context.Request.Params["I"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter I not specified");
                    return;
                }
                if (context.Request.Params["Y"] == null && context.Request.Params["J"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter J not specified");
                    return;
                }
                //sets the map size to the size of the client in order to calculate the coordinates of the projection of the client
                try
                {
                    map.Size = new System.Drawing.Size(System.Convert.ToInt16(context.Request.Params["WIDTH"]), System.Convert.ToInt16(context.Request.Params["HEIGHT"]));
                }
                catch
                {
                    WmsException.ThrowWmsException("Invalid parameters for HEIGHT or WITDH");
                    return;
                }
                //sets the boundingbox to the boundingbox of the client in order to calculate the coordinates of the projection of the client
                BoundingBox bbox = ParseBBOX(context.Request.Params["bbox"]);
                if (bbox == null)
                {
                    WmsException.ThrowWmsException("Invalid parameter BBOX");
                    return;
                }
                map.ZoomToBox(bbox);
                //sets the point clicked by the client
                SharpMap.Geometries.Point p = new SharpMap.Geometries.Point();
                Single x = 0;
                Single y = 0;
                //tries to set the x to the Param I, if the client send an X, it will try the X, if both fail, exception is thrown
                if (context.Request.Params["X"] != null)
                    try
                    {
                        x = System.Convert.ToSingle(context.Request.Params["X"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameters for I");
                    }
                if (context.Request.Params["I"] != null)
                    try
                    {
                        x = System.Convert.ToSingle(context.Request.Params["I"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameters for I");
                    }
                //same procedure for J (Y)
                if (context.Request.Params["Y"] != null)
                    try
                    {
                        y = System.Convert.ToSingle(context.Request.Params["Y"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameters for I");
                    }
                if (context.Request.Params["J"] != null)
                    try
                    {
                        y = System.Convert.ToSingle(context.Request.Params["J"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameters for I");
                    }
                p = map.ImageToWorld(new System.Drawing.PointF(x, y));
                int fc;
                try
                {
                    fc = System.Convert.ToInt16(context.Request.Params["FEATURE_COUNT"]);
                    if (fc < 1)
                        fc = 1;
                }
                catch
                {
                    fc = 1;
                }
                String vstr = "GetFeatureInfo results: \n";
                string[] requestLayers = context.Request.Params["QUERY_LAYERS"].Split(new[] { ',' });
                foreach (string requestLayer in requestLayers)
                {
                    bool found = false;
                    foreach (ILayer mapLayer in map.Layers)
                    {
                        if (String.Equals(mapLayer.LayerName, requestLayer,
                                          StringComparison.InvariantCultureIgnoreCase))
                        {
                            found = true;
                            if (!(mapLayer is ICanQueryLayer)) continue;

                            ICanQueryLayer queryLayer = mapLayer as ICanQueryLayer;
                            if (queryLayer.IsQueryEnabled)
                            {
                                SharpMap.Data.FeatureDataSet fds = new SharpMap.Data.FeatureDataSet();
                                queryLayer.ExecuteIntersectionQuery(p.GetBoundingBox(), fds);
                                if (fds.Tables.Count == 0)
                                {
                                    vstr = vstr + "\nSearch returned no results on layer: " + requestLayer;
                                }
                                else
                                {
                                    if (fds.Tables[0].Rows.Count == 0)
                                    {
                                        vstr = vstr + "\nSearch returned no results on layer: " + requestLayer + " ";
                                    }
                                    else
                                    {
                                        //if featurecount < fds...count, select smallest bbox, because most likely to be clicked
                                        vstr = vstr + "\n Layer: '" + requestLayer + "'\n Featureinfo:\n";
                                        int[] keys = new int[fds.Tables[0].Rows.Count];
                                        double[] area = new double[fds.Tables[0].Rows.Count];
                                        for (int l = 0; l < fds.Tables[0].Rows.Count; l++)
                                        {
                                            SharpMap.Data.FeatureDataRow fdr = fds.Tables[0].Rows[l] as SharpMap.Data.FeatureDataRow;
                                            area[l] = fdr.Geometry.GetBoundingBox().GetArea();
                                            keys[l] = l;
                                        }
                                        Array.Sort(area, keys);
                                        if (fds.Tables[0].Rows.Count < fc)
                                        {
                                            fc = fds.Tables[0].Rows.Count;
                                        }
                                        for (int k = 0; k < fc; k++)
                                        {
                                            for (int j = 0; j < fds.Tables[0].Rows[keys[k]].ItemArray.Length; j++)
                                            {
                                                vstr = vstr + " '" + fds.Tables[0].Rows[keys[k]].ItemArray[j].ToString() + "'";
                                            }
                                            if ((k + 1) < fc)
                                                vstr = vstr + ",\n";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (found == false)
                    {
                        WmsException.ThrowWmsException(WmsException.WmsExceptionCode.LayerNotDefined,
                                                       "Unknown layer '" + requestLayer + "'");
                        return;
                    }
                }
                context.Response.Clear();
                context.Response.ContentType = "text/plain";
                context.Response.Charset = "windows-1252";
                context.Response.Write(vstr);
                context.Response.Flush();
                context.Response.End();
            }
            else if (String.Compare(context.Request.Params["REQUEST"], "GetMap", ignorecase) == 0) //Map requested
            {
                //Check for required parameters
                if (context.Request.Params["LAYERS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter LAYERS not specified");
                    return;
                }
                if (context.Request.Params["STYLES"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter STYLES not specified");
                    return;
                }
                if (context.Request.Params["CRS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter CRS not specified");
                    return;
                }
                else if (context.Request.Params["CRS"] != "EPSG:" + map.Layers[0].SRID)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidCRS, "CRS not supported");
                    return;
                }
                if (context.Request.Params["BBOX"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter BBOX not specified");
                    return;
                }
                if (context.Request.Params["WIDTH"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter WIDTH not specified");
                    return;
                }
                if (context.Request.Params["HEIGHT"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter HEIGHT not specified");
                    return;
                }
                if (context.Request.Params["FORMAT"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter FORMAT not specified");
                    return;
                }

                //Set background color of map
                if (String.Compare(context.Request.Params["TRANSPARENT"], "TRUE", ignorecase) == 0)
                    map.BackColor = Color.Transparent;
                else if (context.Request.Params["BGCOLOR"] != null)
                {
                    try
                    {
                        map.BackColor = ColorTranslator.FromHtml(context.Request.Params["BGCOLOR"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameter BGCOLOR");
                        return;
                    }
                    ;
                }
                else
                    map.BackColor = Color.White;

                //Get the image format requested
                ImageCodecInfo imageEncoder = GetEncoderInfo(context.Request.Params["FORMAT"]);
                if (imageEncoder == null)
                {
                    WmsException.ThrowWmsException("Invalid MimeType specified in FORMAT parameter");
                    return;
                }

                //Parse map size
                int width = 0;
                int height = 0;
                if (!int.TryParse(context.Request.Params["WIDTH"], out width))
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Invalid parameter WIDTH");
                    return;
                }
                else if (description.MaxWidth > 0 && width > description.MaxWidth)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                                                   "Parameter WIDTH too large");
                    return;
                }
                if (!int.TryParse(context.Request.Params["HEIGHT"], out height))
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Invalid parameter HEIGHT");
                    return;
                }
                else if (description.MaxHeight > 0 && height > description.MaxHeight)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                                                   "Parameter HEIGHT too large");
                    return;
                }
                map.Size = new Size(width, height);

                BoundingBox bbox = ParseBBOX(context.Request.Params["bbox"]);
                if (bbox == null)
                {
                    WmsException.ThrowWmsException("Invalid parameter BBOX");
                    return;
                }
                map.PixelAspectRatio = (width / (double)height) / (bbox.Width / bbox.Height);
                map.Center = bbox.GetCentroid();
                map.Zoom = bbox.Width;

                //Set layers on/off
                if (!String.IsNullOrEmpty(context.Request.Params["LAYERS"]))
                //If LAYERS is empty, use default layer on/off settings
                {
                    string[] layers = context.Request.Params["LAYERS"].Split(new[] { ',' });
                    if (description.LayerLimit > 0)
                    {
                        if (layers.Length == 0 && map.Layers.Count > description.LayerLimit ||
                            layers.Length > description.LayerLimit)
                        {
                            WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                                                           "Too many layers requested");
                            return;
                        }
                    }
                    foreach (ILayer layer in map.Layers)
                        layer.Enabled = false;
                    foreach (string layer in layers)
                    {
                        //SharpMap.Layers.ILayer lay = map.Layers.Find(delegate(SharpMap.Layers.ILayer findlay) { return findlay.LayerName == layer; });
                        ILayer lay = null;
                        for (int i = 0; i < map.Layers.Count; i++)
                            if (String.Equals(map.Layers[i].LayerName, layer,
                                              StringComparison.InvariantCultureIgnoreCase))
                                lay = map.Layers[i];


                        if (lay == null)
                        {
                            WmsException.ThrowWmsException(WmsException.WmsExceptionCode.LayerNotDefined,
                                                           "Unknown layer '" + layer + "'");
                            return;
                        }
                        else
                            lay.Enabled = true;
                    }
                }
                //Render map
                Image img = map.GetMap();

                //Png can't stream directy. Going through a memorystream instead
                MemoryStream MS = new MemoryStream();
                img.Save(MS, imageEncoder, null);
                img.Dispose();
                byte[] buffer = MS.ToArray();
                context.Response.Clear();
                context.Response.ContentType = imageEncoder.MimeType;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.End();
            }
            else
                WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported, "Invalid request");
        }

        /// <summary>
        /// Used for setting up output format of image file
        /// </summary>
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            foreach (ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders())
                if (encoder.MimeType == mimeType)
                    return encoder;
            return null;
        }

        /// <summary>
        /// Parses a boundingbox string to a boundingbox geometry from the format minx,miny,maxx,maxy. Returns null if the format is invalid
        /// </summary>
        /// <param name="strBBOX">string representation of a boundingbox</param>
        /// <returns>Boundingbox or null if invalid parameter</returns>
        private static BoundingBox ParseBBOX(string strBBOX)
        {
            string[] strVals = strBBOX.Split(new[] { ',' });
            if (strVals.Length != 4)
                return null;
            double minx = 0;
            double miny = 0;
            double maxx = 0;
            double maxy = 0;
            if (!double.TryParse(strVals[0], NumberStyles.Float, Map.NumberFormatEnUs, out minx))
                return null;
            if (!double.TryParse(strVals[2], NumberStyles.Float, Map.NumberFormatEnUs, out maxx))
                return null;
            if (maxx < minx)
                return null;

            if (!double.TryParse(strVals[1], NumberStyles.Float, Map.NumberFormatEnUs, out miny))
                return null;
            if (!double.TryParse(strVals[3], NumberStyles.Float, Map.NumberFormatEnUs, out maxy))
                return null;
            if (maxy < miny)
                return null;

            return new BoundingBox(minx, miny, maxx, maxy);
        }
    }
}