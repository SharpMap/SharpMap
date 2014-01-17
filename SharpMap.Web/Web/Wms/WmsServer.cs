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
using System.Text;
using System.Web;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;
using SharpMap.Web.Wms.Exceptions;

namespace SharpMap.Web.Wms
{
    /// <summary>
    /// This is a helper class designed to make it easy to create a WMS Service
    /// </summary>
    public static class WmsServer
    {
        public delegate FeatureDataTable InterSectDelegate(FeatureDataTable featureDataTable, Envelope box);

        internal static InterSectDelegate IntersectDelegate;
        internal static int PixelSensitivity = -1;

        /// <summary>
        /// Set the characterset used in FeatureInfo responses
        /// </summary>
        /// <remarks>
        /// To use Windows-1252 set the FeatureInfoResponseEncoding = System.Text.Encoding.GetEncoding(1252);
        /// Set to Null to not set any specific encoding in response
        /// </remarks>
        public static Encoding FeatureInfoResponseEncoding = Encoding.UTF8;

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
        ///<param name="intersectDelegate">Delegate for Getfeatureinfo intersecting, when null, the WMS will default to ICanQueryLayer implementation</param>
        public static void ParseQueryString(Map map, Capabilities.WmsServiceDescription description, int pixelSensitivity, InterSectDelegate intersectDelegate)
        {
            IntersectDelegate = intersectDelegate;
            if (pixelSensitivity > 0)
                PixelSensitivity = pixelSensitivity;
            ParseQueryString(map, description, new Context(HttpContext.Current));
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
        /// <param name="intersectDelegate">Delegate for Getfeatureinfo intersecting, when null, the WMS will default to ICanQueryLayer implementation</param>
        /// <param name="context">The context the <see cref="WmsServer"/> is running in.</param>
        public static void ParseQueryString(Map map, Capabilities.WmsServiceDescription description, int pixelSensitivity, InterSectDelegate intersectDelegate, IContext context)
        {
            IntersectDelegate = intersectDelegate;
            if (pixelSensitivity > 0)
                PixelSensitivity = pixelSensitivity;
            ParseQueryString(map, description, context);
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
        public static void ParseQueryString(Map map, Capabilities.WmsServiceDescription description, IContext context)
        {
            IContextRequest request = context.Request;
            IContextResponse response = context.Response;
            try
            {
                if (PixelSensitivity == -1)
                    PixelSensitivity = 1;
                if (map == null)
                    throw new WmsArgumentException("Map for WMS is null");
                if (map.Layers.Count == 0)
                    throw new WmsArgumentException("Map doesn't contain any layers for WMS service");

                string req = request.Params["REQUEST"];
                if (req == null)
                    throw new WmsParameterNotSpecifiedException("Required parameter REQUEST not specified");
                const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
                IHandler handler;
                if (String.Equals(req, "GetCapabilities", comparison))
                    handler = new GetCapabilities(description);
                else if (String.Equals(req, "GetFeatureInfo", comparison))
                    handler = new GetFeatureInfo(description, PixelSensitivity, IntersectDelegate, FeatureInfoResponseEncoding);
                else if (String.Equals(req, "GetMap", comparison))
                    handler = new GetMap(description);
                else throw new WmsOperationNotSupportedException("Invalid request");

                IHandlerResponse result = handler.Handle(map, request);
                result.WriteToContextAndFlush(response);
            }
            catch (WmsExceptionBase wmsEx)
            {
                wmsEx.WriteToContextAndFlush(response);
            }
        }
    }
}