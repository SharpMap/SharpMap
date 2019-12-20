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
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using System.Collections.Generic;
using System.Text;
using GeoAPI.CoordinateSystems.Transformations;

namespace SharpMap.Web.Wms
{    
    /// <summary>
    /// This is a helper class designed to make it easy to create a WMS Service
    /// </summary>
    public static class WmsServer
    {
        
		#region Delegates

        public delegate FeatureDataTable InterSectDelegate(FeatureDataTable featureDataTable, Envelope box);

        #endregion

        private static InterSectDelegate _intersectDelegate;
        private static int _pixelSensitivity = -1;

        private static Encoding _featureInfoResponseEncoding = Encoding.UTF8;

        /// <summary>
        /// Set the characterset used in FeatureInfo responses
        /// </summary>
        /// <remarks>
        /// To use Windows-1252 set the FeatureInfoResponseEncoding = System.Text.Encoding.GetEncoding(1252);
        /// Set to Null to not set any specific encoding in response
        /// </remarks>
        public static Encoding FeatureInfoResponseEncoding
        {
            get { return _featureInfoResponseEncoding; }
            set { _featureInfoResponseEncoding = value; }
        }

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
        ///<param name="pixelSensitivity"> </param>
        ///<param name="intersectDelegate">Delegate for GetFeatureInfo intersecting, when null, the WMS will default to <see cref="ICanQueryLayer"/> implementation</param>
        public static void ParseQueryString(Map map, Capabilities.WmsServiceDescription description, int pixelSensitivity, InterSectDelegate intersectDelegate)
        {
            _intersectDelegate = intersectDelegate;
            if (pixelSensitivity > 0)
                _pixelSensitivity = pixelSensitivity;
            ParseQueryString(map, description);
        }

        ///  <summary>
        ///  Generates a WMS 1.3.0 compliant response based on a <see cref="SharpMap.Map"/> and the current HttpRequest.
        ///  </summary>
        ///  <remarks>
        ///  <para>
        ///  The Web Map Server implementation in SharpMap requires v1.3.0 compatible clients,
        ///  and support the basic operations "GetCapabilities" and "GetMap"
        ///  as required by the WMS v1.3.0 specification. SharpMap does not support the optional
        ///  GetFeatureInfo operation for querying.
        ///  </para>
        ///  <example>
        ///  Creating a WMS server in ASP.NET is very simple using the classes in the SharpMap.Web.Wms namespace.
        ///  <code lang="C#">
        ///  void page_load(object o, EventArgs e)
        ///  {
        /// 		//Get the path of this page
        /// 		string url = (Request.Url.Query.Length>0?Request.Url.AbsoluteUri.Replace(Request.Url.Query,""):Request.Url.AbsoluteUri);
        /// 		SharpMap.Web.Wms.Capabilities.WmsServiceDescription description =
        /// 			new SharpMap.Web.Wms.Capabilities.WmsServiceDescription("Acme Corp. Map Server", url);
        /// 		
        /// 		// The following service descriptions below are not strictly required by the WMS specification.
        /// 		
        /// 		// Narrative description and keywords providing additional information 
        /// 		description.Abstract = "Map Server maintained by Acme Corporation. Contact: webmaster@wmt.acme.com. High-quality maps showing roadrunner nests and possible ambush locations.";
        /// 		description.Keywords.Add("bird");
        /// 		description.Keywords.Add("roadrunner");
        /// 		description.Keywords.Add("ambush");
        /// 		
        /// 		//Contact information 
        /// 		description.ContactInformation.PersonPrimary.Person = "John Doe";
        /// 		description.ContactInformation.PersonPrimary.Organisation = "Acme Inc";
        /// 		description.ContactInformation.Address.AddressType = "postal";
        /// 		description.ContactInformation.Address.Country = "Neverland";
        /// 		description.ContactInformation.VoiceTelephone = "1-800-WE DO MAPS";
        /// 		//Impose WMS constraints
        /// 		description.MaxWidth = 1000; //Set image request size width
        /// 		description.MaxHeight = 500; //Set image request size height
        /// 		
        /// 		//Call method that sets up the map
        /// 		//We just add a dummy-size, since the wms requests will set the image-size
        /// 		SharpMap.Map myMap = MapHelper.InitializeMap(new System.Drawing.Size(1,1));
        /// 		
        /// 		//Parse the request and create a response
        /// 		SharpMap.Web.Wms.WmsServer.ParseQueryString(myMap,description);
        ///  }
        ///  </code>
        ///  </example>
        ///  </remarks>
        /// <param name="map">Map to serve on WMS</param>
        ///  <param name="description">Description of map service</param>
        /// <param name="pixelSensitivity"> </param>
        ///<param name="intersectDelegate">Delegate for GetFeatureInfo intersecting, when null, the WMS will default to <see cref="ICanQueryLayer"/> implementation</param>
        /// <param name="context">The context the <see cref="WmsServer"/> is running in.</param>
        public static void ParseQueryString(Map map, Capabilities.WmsServiceDescription description, int pixelSensitivity, InterSectDelegate intersectDelegate, HttpContext context)
        {
            _intersectDelegate = intersectDelegate;
            if (pixelSensitivity > 0)
                _pixelSensitivity = pixelSensitivity;
            ParseQueryString(map, description);
        }

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
            if (HttpContext.Current == null)
                throw (new ApplicationException(
                    "An attempt was made to access the WMS server outside a valid HttpContext"));

            ParseQueryString(map, description, HttpContext.Current);
        }

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
        /// <param name="context">The context the <see cref="WmsServer"/> is running in.</param>
        public static void ParseQueryString(Map map, Capabilities.WmsServiceDescription description, HttpContext context)
        {
            if (_pixelSensitivity ==-1)
                _pixelSensitivity = 1;
            if (map == null)
                throw (new ArgumentException("Map for WMS is null"));
            if (map.Layers.Count == 0)
                throw (new ArgumentException("Map doesn't contain any layers for WMS service"));

            //IgnoreCase value should be set according to the VERSION parameter
            //v1.3.0 is case sensitive, but since it causes a lot of problems with several WMS clients, we ignore casing anyway.
            const bool ignoreCase = true;

            //Check for required parameters
            //Request parameter is mandatory
            if (context.Request.Params["REQUEST"] == null)
            {
                WmsException.ThrowWmsException("Required parameter REQUEST not specified", context);
                return;
            }
            //Check if version is supported
            if (context.Request.Params["VERSION"] != null)
            {
                if (String.Compare(context.Request.Params["VERSION"], "1.3.0", ignoreCase) != 0)
                {
                    WmsException.ThrowWmsException("Only version 1.3.0 supported", context);
                    return;
                }
            }
            else
            //Version is mandatory if REQUEST!=GetCapabilities. Check if this is a capabilities request, since VERSION is null
            {
                if (String.Compare(context.Request.Params["REQUEST"], "GetCapabilities", ignoreCase) != 0)
                {
                    WmsException.ThrowWmsException("VERSION parameter not supplied", context);
                    return;
                }
            }

            //If Capabilities was requested
            if (String.Compare(context.Request.Params["REQUEST"], "GetCapabilities", ignoreCase) == 0)
            {
                //Service parameter is mandatory for GetCapabilities request
                if (context.Request.Params["SERVICE"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter SERVICE not specified", context);
                    return;
                }

                if (String.Compare(context.Request.Params["SERVICE"], "WMS", StringComparison.InvariantCulture/*IgnoreCase*/) != 0)
                    WmsException.ThrowWmsException(
                        "Invalid service for GetCapabilities Request. Service parameter must be 'WMS'", context);

                XmlDocument capabilities = ServerCapabilities.GetCapabilities(map, description);
                context.Response.Clear();
                context.Response.ContentType = "text/xml";
                XmlWriter writer = XmlWriter.Create(context.Response.OutputStream);
                capabilities.WriteTo(writer);
                writer.Close();
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
                //context.Response.End();
            }
            else if (String.Compare(context.Request.Params["REQUEST"], "GetFeatureInfo", ignoreCase) == 0) //FeatureInfo Requested
            {
                if (context.Request.Params["LAYERS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter LAYERS not specified", context);
                    return;
                }
                if (context.Request.Params["STYLES"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter STYLES not specified", context);
                    return;
                }
                if (context.Request.Params["CRS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter CRS not specified", context);
                    return;
                }
                if (context.Request.Params["CRS"] != "EPSG:" + map.Layers[0].TargetSRID)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidCRS, "CRS not supported", context);
                    return;
                }
                if (context.Request.Params["BBOX"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter BBOX not specified", context);
                    return;
                }
                if (context.Request.Params["WIDTH"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter WIDTH not specified", context);
                    return;
                }
                if (context.Request.Params["HEIGHT"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter HEIGHT not specified", context);
                    return;
                }
                if (context.Request.Params["FORMAT"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter FORMAT not specified", context);
                    return;
                }
                if (context.Request.Params["QUERY_LAYERS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter QUERY_LAYERS not specified", context);
                    return;
                }
                if (context.Request.Params["INFO_FORMAT"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter INFO_FORMAT not specified", context);
                    return;
                }
                //parameters X&Y are not part of the 1.3.0 specification, but are included for backwards compatability with 1.1.1 (OpenLayers likes it when used together with wms1.1.1 services)
                if (context.Request.Params["X"] == null && context.Request.Params["I"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter I not specified", context);
                    return;
                }
                if (context.Request.Params["Y"] == null && context.Request.Params["J"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter J not specified", context);
                    return;
                }
                //sets the map size to the size of the client in order to calculate the coordinates of the projection of the client
                try
                {
                    map.Size = new Size(Convert.ToInt16(context.Request.Params["WIDTH"]), 
                                        Convert.ToInt16(context.Request.Params["HEIGHT"]));
                }
                catch
                {
                    WmsException.ThrowWmsException("Invalid parameters for HEIGHT or WITDH", context);
                    return;
                }
                //sets the boundingbox to the boundingbox of the client in order to calculate the coordinates of the projection of the client
                var bbox = ParseBBOX(context.Request.Params["bbox"], map.Layers[0].TargetSRID == 4326);                
                if (bbox == null)
                {
                    WmsException.ThrowWmsException("Invalid parameter BBOX", context);
                    return;
                }
                map.ZoomToBox(bbox);

                //sets the point clicked by the client
                Single x = 0f, y = 0f;
                
                //tries to set the x to the Param I, if the client send an X, it will try the X, if both fail, exception is thrown
                if (context.Request.Params["X"] != null)
                    try
                    {
                        x = Convert.ToSingle(context.Request.Params["X"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameters for X", context);
                        return;
                    }
                if (context.Request.Params["I"] != null)
                    try
                    {
                        x = Convert.ToSingle(context.Request.Params["I"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameters for I", context);
                        return;
                    }
                //same procedure for J (Y)
                if (context.Request.Params["Y"] != null)
                    try
                    {
                        y = Convert.ToSingle(context.Request.Params["Y"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameters for Y", context);
                        return;
                    }
                if (context.Request.Params["J"] != null)
                    try
                    {
                        y = Convert.ToSingle(context.Request.Params["J"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameters for I", context);
                        return;
                    }
                //var p = map.ImageToWorld(new PointF(x, y));
                int fc;
                try
                {
                    fc = Convert.ToInt16(context.Request.Params["FEATURE_COUNT"]);
                    if (fc < 1)
                        fc = 1;
                }
                catch
                {
                    fc = 1;
                }

                //default to text if an invalid format is requested
                var infoFormat = context.Request.Params["INFO_FORMAT"];
                string cqlFilter = null;
                if (context.Request.Params["CQL_FILTER"] != null)
                {
                    cqlFilter = context.Request.Params["CQL_FILTER"];
                }

                string vstr;
                var requestLayers = context.Request.Params["QUERY_LAYERS"].Split(new[] { ',' });
                if (String.Compare(infoFormat, "text/json", ignoreCase) == 0)
                {
                    vstr = CreateFeatureInfoGeoJSON(map, requestLayers, x, y, fc, cqlFilter, context);
                    //string.Empty is the result if a WmsException.ThrowWmsException(...) has been called
                    if (vstr == string.Empty) return;
                    context.Response.ContentType = "text/json";
                }
                else
                {
                    vstr = CreateFeatureInfoPlain(map, requestLayers, x, y, fc, cqlFilter, context);
                    //string.Empty is the result if a WmsException.ThrowWmsException(...) has been called
                    if (vstr == string.Empty) return;
                    context.Response.ContentType = "text/plain";
                }
                context.Response.Clear();
                if (_featureInfoResponseEncoding != null)
                {
                    context.Response.Charset = _featureInfoResponseEncoding.WebName; //"windows-1252";
                }
                context.Response.Write(vstr);
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
                //context.Response.End();
            }
            else if (String.Compare(context.Request.Params["REQUEST"], "GetMap", ignoreCase) == 0) //Map requested
            {
                //Check for required parameters
                if (context.Request.Params["LAYERS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter LAYERS not specified", context);
                    return;
                }
                if (context.Request.Params["STYLES"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter STYLES not specified", context);
                    return;
                }
                if (context.Request.Params["CRS"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter CRS not specified", context);
                    return;
                }
                if (!ConsideredEqual(context.Request.Params["CRS"],  $"EPSG:{map.Layers[0].TargetSRID}"))
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidCRS, "CRS not supported",
                                                   context);
                    return;
                }
                if (context.Request.Params["BBOX"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter BBOX not specified", context);
                    return;
                }
                if (context.Request.Params["WIDTH"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter WIDTH not specified", context);
                    return;
                }
                if (context.Request.Params["HEIGHT"] == null)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Required parameter HEIGHT not specified", context);
                    return;
                }
                if (context.Request.Params["FORMAT"] == null)
                {
                    WmsException.ThrowWmsException("Required parameter FORMAT not specified", context);
                    return;
                }

                //Set background color of map
                if (String.Compare(context.Request.Params["TRANSPARENT"], "TRUE", ignoreCase) == 0)
                    map.BackColor = Color.Transparent;
                else if (context.Request.Params["BGCOLOR"] != null)
                {
                    try
                    {
                        map.BackColor = ColorTranslator.FromHtml(context.Request.Params["BGCOLOR"]);
                    }
                    catch
                    {
                        WmsException.ThrowWmsException("Invalid parameter BGCOLOR", context);
                        return;
                    }
                }
                else
                    map.BackColor = Color.White;

                //Get the image format requested
                ImageCodecInfo imageEncoder = GetEncoderInfo(context.Request.Params["FORMAT"]);
                if (imageEncoder == null)
                {
                    WmsException.ThrowWmsException("Invalid MimeType specified in FORMAT parameter", context);
                    return;
                }

                //Parse map size
                int width, height;
                if (!int.TryParse(context.Request.Params["WIDTH"], out width))
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Invalid parameter WIDTH", context);
                    return;
                }
                if (description.MaxWidth > 0 && width > description.MaxWidth)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                                                   "Parameter WIDTH too large", context);
                    return;
                }
                if (!int.TryParse(context.Request.Params["HEIGHT"], out height))
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue,
                                                   "Invalid parameter HEIGHT", context);
                    return;
                }
                if (description.MaxHeight > 0 && height > description.MaxHeight)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                                                   "Parameter HEIGHT too large", context);
                    return;
                }
                map.Size = new Size(width, height);

                var bbox = ParseBBOX(context.Request.Params["bbox"], map.Layers[0].TargetSRID == 4326);
                if (bbox == null)
                {
                    WmsException.ThrowWmsException("Invalid parameter BBOX", context);
                    return;
                }

                map.PixelAspectRatio = (width/(double) height)/(bbox.Width/bbox.Height);
                map.Center = bbox.Centre;
                map.Zoom = bbox.Width;

                //set Styles for layers
                //first, if the request ==  STYLES=, set all the vectorlayers with Themes not null the Theme to the first theme from Themes
                if (String.IsNullOrEmpty(context.Request.Params["STYLES"]))
                {
                    foreach (var layer in map.Layers)
                    {
                        var vectorLayer = layer as VectorLayer;
                        if (vectorLayer != null)
                        {
                            if (vectorLayer.Themes != null)
                            {
                                foreach (var kvp in vectorLayer.Themes)
                                {
                                    vectorLayer.Theme = kvp.Value;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(context.Request.Params["LAYERS"]))
                    {
                        var layerz = context.Request.Params["LAYERS"].Split(new[] {','});
                        var styles = context.Request.Params["STYLES"].Split(new[] {','});
                        //test whether the lengt of the layers and the styles is the same. WMS spec is unclear on what to do if there is no one-to-one correspondence
                        if (layerz.Length == styles.Length)
                        {
                            foreach (var layer in map.Layers)
                            {
                                //is this a vector layer at all
                                var vectorLayer = layer as VectorLayer;
                                if (vectorLayer == null) continue;

                                //does it have several themes applied
                                //ToDo -> Refactor VectorLayer.Themes to Rendering.Thematics.ThemeList : ITheme
                                if (vectorLayer.Themes != null && vectorLayer.Themes.Count > 0)
                                {
                                    for (int i = 0; i < layerz.Length; i++)
                                    {
                                        if (String.Equals(layer.LayerName, layerz[i],
                                                          StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            //take default style if style is empty
                                            if (styles[i] == "")
                                            {
                                                foreach (var kvp in vectorLayer.Themes)
                                                {
                                                    vectorLayer.Theme = kvp.Value;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (vectorLayer.Themes.ContainsKey(styles[i]))
                                                {
                                                    vectorLayer.Theme = vectorLayer.Themes[styles[i]];
                                                }
                                                else
                                                {
                                                    WmsException.ThrowWmsException(
                                                        WmsException.WmsExceptionCode.StyleNotDefined,
                                                        "Style not advertised for this layer", context);
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var cqlFilter = context.Request.Params["CQL_FILTER"];
                if (!string.IsNullOrEmpty(cqlFilter))
                {
                    foreach (var layer in map.Layers)
                    {
                        var vectorLayer = layer as VectorLayer;
                        if (vectorLayer != null)
                        {
                            PrepareDataSourceForCql(vectorLayer.DataSource, cqlFilter);
                            continue;
                        }

                        var labelLayer = layer as LabelLayer;
                        if (labelLayer != null)
                        {
                            PrepareDataSourceForCql(labelLayer.DataSource, cqlFilter);
                            continue;
                        }
                    }
                }

                //Set layers on/off
                var layersString = context.Request.Params["LAYERS"];
                if (!string.IsNullOrEmpty(layersString))
                    //If LAYERS is empty, use default layer on/off settings
                {
                    var layers = layersString.Split(new[] {','});
                    if (description.LayerLimit > 0)
                    {
                        if (layers.Length == 0 && map.Layers.Count > description.LayerLimit ||
                            layers.Length > description.LayerLimit)
                        {
                            WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                                                           "Too many layers requested", context);
                            return;
                        }
                    }

                    foreach (var layer in map.Layers)
                        layer.Enabled = false;

                    foreach (var layer in layers)
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
                                                           "Unknown layer '" + layer + "'", context);
                            return;
                        }
                        lay.Enabled = true;
                    }
                }

                //Render map
                var img = map.GetMap();

                //Png can't stream directly. Going through a MemoryStream instead
                byte[] buffer;
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, imageEncoder, null);
                    img.Dispose();
                    buffer = ms.ToArray();
                }
                context.Response.Clear();
                context.Response.ContentType = imageEncoder.MimeType;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
                //context.Response.End();
            }
            else
            {
                WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported, "Invalid request", context);
                return;
            }
        }

        private static bool ConsideredEqual(string requestedCrs, string mapCrs)
        {
            if (string.Equals(requestedCrs, mapCrs, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (requestedCrs == "EPSG:900913" && mapCrs == "EPSG:3857")
                return true;

            if (requestedCrs == "EPSG:3857" && mapCrs == "EPSG:900913")
                return true;

            return false;
        }

        private static void PrepareDataSourceForCql(IBaseProvider provider, string cqlFilterString)
        {
            //for layers with a filterprovider
            var filterProvider = provider as FilterProvider;
            if (filterProvider != null)
            {
                filterProvider.FilterDelegate = row => CqlFilter(row, cqlFilterString);
                return;
            }
            //for layers with a SQL datasource with a DefinitionQuery property
            var piDefinitionQuery = provider.GetType().GetProperty("DefinitionQuery", BindingFlags.Public | BindingFlags.Instance);
            if (piDefinitionQuery != null)
            {
                string dq = piDefinitionQuery.GetValue(provider, null) as string;
                if (string.IsNullOrEmpty(dq))
                    piDefinitionQuery.SetValue(provider, cqlFilterString, null);
                else
                    piDefinitionQuery.SetValue(provider, "(" + dq + ") AND (" + cqlFilterString + ")", null);
            }
        }

        /// <summary>
        /// Used for setting up output format of image file
        /// </summary>
        public static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            foreach (var encoder in ImageCodecInfo.GetImageEncoders())
                if (encoder.MimeType == mimeType)
                    return encoder;
            return null;
        }

        /// <summary>
        /// Parses a boundingbox string to a boundingbox geometry from the format minx,miny,maxx,maxy. Returns null if the format is invalid
        /// </summary>
        /// <param name="boundingBox">string representation of a boundingbox</param>
        /// <param name="flipXY">Value indicating that x- and y-ordinates should be changed.</param>
        /// <returns>Boundingbox or null if invalid parameter</returns>
// ReSharper disable InconsistentNaming
        public static Envelope ParseBBOX(string boundingBox, bool flipXY)
// ReSharper restore InconsistentNaming
        {
            var strVals = boundingBox.Split(new[] { ',' });
            if (strVals.Length != 4)
                return null;
            double minx, miny, maxx, maxy;
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

            return flipXY 
                ? new Envelope(miny, maxy, minx, maxx) 
                : new Envelope(minx, maxx, miny, maxy);
        }

        /// <summary>
        /// Gets FeatureInfo as text/plain
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="requestedLayers">The requested layers</param>
        /// <param name="x">The x-ordinate</param>
        /// <param name="y">The y-ordinate</param>
        /// <param name="featureCount"></param>
        /// <param name="cqlFilter">The code query language</param>
        /// <param name="context">The <see cref="HttpContext"/> to use. If not specified or <value>null</value>, <see cref="HttpContext.Current"/> is used.</param>
        /// <exception cref="InvalidOperationException">Thrown if this function is used without a valid <see cref="HttpContext"/> at hand</exception>
        /// <returns>Plain text string with featureinfo results</returns>
        public static string CreateFeatureInfoPlain(Map map, string[] requestedLayers, Single x, Single y, int featureCount, string cqlFilter, HttpContext context = null)
        {
            if (context == null)
                context = HttpContext.Current;

            if (context == null)
                throw new InvalidOperationException("Cannot use CreateFeatureInfoPlain without a valid HttpContext");

            var vstr = "GetFeatureInfo results: \n";
            foreach (string requestLayer in requestedLayers)
            {
                bool found = false;
                foreach (var mapLayer in map.Layers)
                {
                    if (String.Equals(mapLayer.LayerName, requestLayer,
                                      StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        var queryLayer = mapLayer as ICanQueryLayer;
                        if (queryLayer == null || !queryLayer.IsQueryEnabled) continue;

                        var queryBoxMinX = x - (_pixelSensitivity);
                        var queryBoxMinY = y - (_pixelSensitivity);
                        var queryBoxMaxX = x + (_pixelSensitivity);
                        var queryBoxMaxY = y + (_pixelSensitivity);
                            
                        var minXY = map.ImageToWorld(new PointF(queryBoxMinX, queryBoxMinY));
                            var maxXY = map.ImageToWorld(new PointF(queryBoxMaxX, queryBoxMaxY));
                            var queryBox = new Envelope(minXY, maxXY);
                            var fds = new FeatureDataSet();
                            queryLayer.ExecuteIntersectionQuery(queryBox, fds);
                            
                            if (_intersectDelegate != null)
                            {
                                var tmp = _intersectDelegate(fds.Tables[0], queryBox);
                                fds.Tables.RemoveAt(0);
                                fds.Tables.Add(tmp);
                            }
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
                                    //filter the rows with the CQLFilter if one is provided
                                    if (cqlFilter != null)
                                    {
                                        for (int i = fds.Tables[0].Rows.Count - 1; i >= 0; i--)
                                        {
                                            if (!CqlFilter((FeatureDataRow)fds.Tables[0].Rows[i], cqlFilter))
                                            {
                                                fds.Tables[0].Rows.RemoveAt(i);
                                            }
                                        }
                                    }
                                    //if featurecount < fds...count, select smallest bbox, because most likely to be clicked
                                    vstr = vstr + "\n Layer: '" + requestLayer + "'\n Featureinfo:\n";
                                    int[] keys = new int[fds.Tables[0].Rows.Count];
                                    double[] area = new double[fds.Tables[0].Rows.Count];
                                    for (int l = 0; l < fds.Tables[0].Rows.Count; l++)
                                    {
                                        var fdr = (FeatureDataRow)fds.Tables[0].Rows[l];
                                        area[l] = fdr.Geometry.EnvelopeInternal.Area;
                                        keys[l] = l;
                                    }
                                    Array.Sort(area, keys);
                                    if (fds.Tables[0].Rows.Count < featureCount)
                                    {
                                        featureCount = fds.Tables[0].Rows.Count;
                                    }
                                    for (int k = 0; k < featureCount; k++)
                                    {
                                        for (int j = 0; j < fds.Tables[0].Rows[keys[k]].ItemArray.Length; j++)
                                        {
                                            vstr = vstr + " '" + fds.Tables[0].Rows[keys[k]].ItemArray[j] + "'";
                                        }
                                        if ((k + 1) < featureCount)
                                            vstr = vstr + ",\n";
                                    }
                                }
                            }
                        
                    }
                }
                if (found == false)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.LayerNotDefined,
                                                   "Unknown layer '" + requestLayer + "'", context);
                    return string.Empty;
                }
            }
            return vstr;
        }
        /// <summary>
        /// Gets FeatureInfo as GeoJSON
        /// </summary>
        /// <param name="map">The map to create the feature info from</param>
        /// <param name="requestedLayers">The layers to create the feature info for</param>
        /// <param name="x">The x-Ordinate</param>
        /// <param name="y">The y-Ordinate</param>
        /// <param name="featureCount">The number of features</param>
        /// <param name="cqlFilterString">The CQL Filter string</param>
        /// <param name="context">The <see cref="HttpContext"/> to use. If not specified or <value>null</value>, <see cref="HttpContext.Current"/> is used.</param>
        /// <exception cref="InvalidOperationException">Thrown if this function is used without a valid <see cref="HttpContext"/> at hand</exception>
        /// <returns>GeoJSON string with featureinfo results</returns>
        public static string CreateFeatureInfoGeoJSON(Map map, string[] requestedLayers, Single x, Single y, int featureCount, string cqlFilterString, HttpContext context = null)
        {
            if (context == null)
                context = HttpContext.Current;

            if (context == null)
                throw new InvalidOperationException("Cannot use CreateFeatureInfoGeoJSON without a valid HttpContext");

            var items = new List<Converters.GeoJSON.GeoJSON>();
            foreach (var requestLayer in requestedLayers)
            {
                var found = false;
                foreach (var mapLayer in map.Layers)
                {
                    if (String.Equals(mapLayer.LayerName, requestLayer,
                                      StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        var queryLayer = mapLayer as ICanQueryLayer;
                        if (queryLayer == null || !queryLayer.IsQueryEnabled) continue;

                        var queryBoxMinX = x - (_pixelSensitivity);
                        var queryBoxMinY = y - (_pixelSensitivity);
                        var queryBoxMaxX = x + (_pixelSensitivity);
                        var queryBoxMaxY = y + (_pixelSensitivity);
                        var minXY = map.ImageToWorld(new PointF(queryBoxMinX, queryBoxMinY));
                        var maxXY = map.ImageToWorld(new PointF(queryBoxMaxX, queryBoxMaxY));
                        var queryBox = new Envelope(minXY, maxXY);
                        var fds = new FeatureDataSet();
                        queryLayer.ExecuteIntersectionQuery(queryBox, fds);
                        //
                        if (_intersectDelegate != null)
                        {
                            var tmp =  _intersectDelegate(fds.Tables[0], queryBox);
                            fds.Tables.RemoveAt(0);
                            fds.Tables.Add(tmp);
                        }
                        //filter the rows with the CQLFilter if one is provided
                        if (cqlFilterString != null)
                        {
                            for (var i = fds.Tables[0].Rows.Count-1; i >=0 ; i--)
                            {
                                if (!CqlFilter((FeatureDataRow)fds.Tables[0].Rows[i], cqlFilterString))
                                {
                                    fds.Tables[0].Rows.RemoveAt(i);
                                }
                            }
                        }
                        var data = Converters.GeoJSON.GeoJSONHelper.GetData(fds);

                        // Reproject geometries if needed
                        IMathTransform transform = null;
                        if (queryLayer is VectorLayer)
                        {
                            ICoordinateTransformation transformation = (queryLayer as VectorLayer).CoordinateTransformation;
                            transform = transformation == null ? null : transformation.MathTransform;
                        }

                        if (transform != null)
                        {
                            data = data.Select(d =>
                            {
                                var converted = GeometryTransform.TransformGeometry(d.Geometry, transform, map.Factory);
                                d.SetGeometry(converted);
                                return d;
                            });
                        }
                        items.AddRange(data);
                        
                    }
                }
                if (found == false)
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.LayerNotDefined,
                                                   "Unknown layer '" + requestLayer + "'", context);
                    return string.Empty;
                }
            }
            var writer = new StringWriter();
            Converters.GeoJSON.GeoJSONWriter.Write(items, writer);
            return writer.ToString();            
        }
        /// <summary>
        /// Filters the features to be processed by a CQL filter
        /// </summary>
        /// <param name="row">A <see cref="T:SharpMap.Data.FeatureDataRow"/> to test.</param>
        /// <param name="cqlString">A CQL string defining the filter </param>
        /// <returns>GeoJSON string with featureinfo results</returns>
        private static bool CqlFilter(FeatureDataRow row, string cqlString)
        {
            var toreturn = true;
            //check on filter type (AND, OR, NOT)
            var splitstring =new[]{" "};
            var cqlStringItems = cqlString.Split(splitstring, StringSplitOptions.RemoveEmptyEntries);
            var comparers = new[] { "==", "!=", "<", ">", "<=", ">=", "BETWEEN", "LIKE", "IN" };
            for (int i = 0; i < cqlStringItems.Length; i++)
            {
                var tmpResult = true;
                //check first on AND OR NOT, only the case if multiple checks have to be done
// ReSharper disable InconsistentNaming
                var AND = true;
                var OR = false;
                var NOT = false;
// ReSharper restore InconsistentNaming
                if (cqlStringItems[i] == "AND") { i++; }
                if (cqlStringItems[i] == "OR") { AND = false; OR = true; i++; }
                if (cqlStringItems[i] == "NOT"){AND = false; NOT = true;i++;}
                if ((NOT && !toreturn) || (AND && !toreturn))
                    break;                
                //valid cql starts always with the column name
                string column = cqlStringItems[i];
                int columnIndex = row.Table.Columns.IndexOf(column);
                Type t = row.Table.Columns[columnIndex].DataType;
                if (columnIndex <0)
                    break;
                i++;
                string comparer = cqlStringItems[i];
                i++;
                //if the comparer isn't in the comparerslist stop
                if (!comparers.Contains(comparer))
                    break;

                if (comparer == comparers[8])//IN 
                {
                    //read all the items until the list is closed by ')' and merge them
                    //all items are assumed to be separated by space merge them first
                    //items are merged because a item might contain a space itself, and in this case it's splitted at the wrong place
                    var IN = "";
                    while (!cqlStringItems[i].Contains(")"))
                    {
                        IN = IN + " " + cqlStringItems[i];
                        i++;
                    }
                    IN = IN + " " + cqlStringItems[i];
                    string[] splitters = { "('", "', '", "','", "')" };
                    var items = IN.Split(splitters, StringSplitOptions.RemoveEmptyEntries).ToList();

                    tmpResult = items.Contains(Convert.ToString(row[columnIndex]));
                }
                else if (comparer == comparers[7])//LIKE
                {
                    //to implement
                    //tmpResult = true;
                }
                else if (comparer == comparers[6])//BETWEEN
                {
                    //get type number of string
                    if (t == typeof(string))
                    {
                        var string1 = cqlStringItems[i];
                        i += 2; //skip the AND in BETWEEN
                        var string2 = cqlStringItems[i];
                        tmpResult = 0 < String.Compare(Convert.ToString(row[columnIndex], NumberFormatInfo.InvariantInfo), string1, StringComparison.Ordinal) &&
                                    0 > String.Compare(Convert.ToString(row[columnIndex], NumberFormatInfo.InvariantInfo), string2, StringComparison.Ordinal);

                    }
                    else if (t == typeof(double))
                    {
                        double value1 = Convert.ToDouble(cqlStringItems[i]);
                        i += 2; //skip the AND in BETWEEN
                        double value2 = Convert.ToDouble(cqlStringItems[i]);
                        tmpResult = value1 < Convert.ToDouble(row[columnIndex]) && value2 > Convert.ToDouble(row[columnIndex]);
                    }
                    else if (t == typeof(int))
                    {
                        int value1 = Convert.ToInt32(cqlStringItems[i]);
                        i += 2;
                        int value2 = Convert.ToInt32(cqlStringItems[i]);
                        tmpResult = value1 < Convert.ToInt32(row[columnIndex]) && value2 > Convert.ToInt32(row[columnIndex]);
                    }
                }
                else
                {
                    if (t == typeof(string))
                    {
                        string cqlValue = Convert.ToString(cqlStringItems[i], NumberFormatInfo.InvariantInfo);
                        string rowValue = Convert.ToString(row[columnIndex], NumberFormatInfo.InvariantInfo);
                        if (comparer == comparers[5])//>=
                        {
                            tmpResult = 0 <= String.Compare(rowValue, cqlValue, StringComparison.Ordinal);
                        }
                        else if (comparer == comparers[4])//<=
                        {
                            tmpResult = 0 >= String.Compare(rowValue, cqlValue, StringComparison.Ordinal);
                        }
                        else if (comparer == comparers[3])//>
                        {
                            tmpResult = 0 < String.Compare(rowValue, cqlValue, StringComparison.Ordinal);
                        }
                        else if (comparer == comparers[2])//<
                        {
                            tmpResult = 0 > String.Compare(rowValue, cqlValue, StringComparison.Ordinal);
                        }
                        else if (comparer == comparers[1])//!=
                        {
                            tmpResult = rowValue != cqlValue;
                        }
                        else if (comparer == comparers[0])//==
                        {
                            tmpResult = rowValue == cqlValue;
                        }
                    }
                    else
                    {
                        double value = Convert.ToDouble(cqlStringItems[i]);
                        if (comparer == comparers[5])//>=
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex]) >= value;
                        }
                        else if (comparer == comparers[4])//<=
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex]) <= value;
                        }
                        else if (comparer == comparers[3])//>
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex]) > value;
                        }
                        else if (comparer == comparers[2])//<
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex]) < value;
                        }
                        else if (comparer == comparers[1])//!=
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex]) != value;
                        }
                        else if (comparer == comparers[0])//==
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex]) == value;
                        }
                    }
                }
                if (AND)
                    toreturn = tmpResult;
                if (OR && tmpResult)
                    toreturn = true;
                if (toreturn && NOT && tmpResult)
                    toreturn = false;

            }
                //OpenLayers.Filter.Comparison.EQUAL_TO = “==”;
                //OpenLayers.Filter.Comparison.NOT_EQUAL_TO = “!=”;
                //OpenLayers.Filter.Comparison.LESS_THAN = “<”;
                //OpenLayers.Filter.Comparison.GREATER_THAN = “>”;
                //OpenLayers.Filter.Comparison.LESS_THAN_OR_EQUAL_TO = “<=”;
                //OpenLayers.Filter.Comparison.GREATER_THAN_OR_EQUAL_TO = “>=”;
                //OpenLayers.Filter.Comparison.BETWEEN = “..”;
                //OpenLayers.Filter.Comparison.LIKE = “~”;
                //IN (,,)
            
            return toreturn;
        }
        
    }
}
