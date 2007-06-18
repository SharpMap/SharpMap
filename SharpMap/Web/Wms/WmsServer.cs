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
using System.Collections.Generic;
using System.Text;

namespace SharpMap.Web.Wms
{
	/// <summary>
	/// This is a helper class designed to make it easy to create a WMS Service
	/// </summary>
	public class WmsServer
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
		public static void ParseQueryString(SharpMap.Map map, Capabilities.WmsServiceDescription description)
		{
			if (map == null)
				throw (new ArgumentException("Map for WMS is null"));
			if (map.Layers.Count == 0)
				throw (new ArgumentException("Map doesn't contain any layers for WMS service"));

			if (System.Web.HttpContext.Current == null)
				throw (new ApplicationException("An attempt was made to access the WMS server outside a valid HttpContext"));

			System.Web.HttpContext context = System.Web.HttpContext.Current;

			//IgnoreCase value should be set according to the VERSION parameter
			//v1.3.0 is case sensitive, but since it causes a lot of problems with several WMS clients, we ignore casing anyway.
			bool ignorecase = true; 

			//Check for required parameters
			//Request parameter is mandatory
			if (context.Request.Params["REQUEST"] == null)
				{ WmsException.ThrowWmsException("Required parameter REQUEST not specified"); return; }
			//Check if version is supported
			if (context.Request.Params["VERSION"] != null)
			{
				if (String.Compare(context.Request.Params["VERSION"], "1.3.0", ignorecase) != 0)
					{ WmsException.ThrowWmsException("Only version 1.3.0 supported"); return; }
			}
			else //Version is mandatory if REQUEST!=GetCapabilities. Check if this is a capabilities request, since VERSION is null
			{
				if (String.Compare(context.Request.Params["REQUEST"], "GetCapabilities", ignorecase) != 0)
					{ WmsException.ThrowWmsException("VERSION parameter not supplied"); return; }
			}

			//If Capabilities was requested
			if (String.Compare(context.Request.Params["REQUEST"], "GetCapabilities", ignorecase) == 0)
			{
				//Service parameter is mandatory for GetCapabilities request
				if (context.Request.Params["SERVICE"] == null)
				{ WmsException.ThrowWmsException("Required parameter SERVICE not specified"); return; }

				if (String.Compare(context.Request.Params["SERVICE"], "WMS") != 0)
					WmsException.ThrowWmsException("Invalid service for GetCapabilities Request. Service parameter must be 'WMS'");

				System.Xml.XmlDocument capabilities = Wms.Capabilities.GetCapabilities(map, description);
				context.Response.Clear();
				context.Response.ContentType = "text/xml";
				System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(context.Response.OutputStream);
				capabilities.WriteTo(writer);
				writer.Close();
				context.Response.End();
			}
			else if (String.Compare(context.Request.Params["REQUEST"], "GetMap", ignorecase) == 0) //Map requested
			{
				//Check for required parameters
				if (context.Request.Params["LAYERS"] == null)
					{ WmsException.ThrowWmsException("Required parameter LAYERS not specified"); return; }
				if (context.Request.Params["STYLES"] == null)
					{ WmsException.ThrowWmsException("Required parameter STYLES not specified"); return; }
				if (context.Request.Params["CRS"] == null)
					{ WmsException.ThrowWmsException("Required parameter CRS not specified"); return; }
				else if (context.Request.Params["CRS"] != "EPSG:" + map.Layers[0].SRID.ToString())
					{ WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidCRS, "CRS not supported"); return; }
				if (context.Request.Params["BBOX"] == null)
					{ WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue, "Required parameter BBOX not specified"); return; }
				if (context.Request.Params["WIDTH"] == null)
					{ WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue, "Required parameter WIDTH not specified"); return; }
				if (context.Request.Params["HEIGHT"] == null)
					{ WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue, "Required parameter HEIGHT not specified"); return; }
				if (context.Request.Params["FORMAT"] == null)
					{ WmsException.ThrowWmsException("Required parameter FORMAT not specified"); return; }

				//Set background color of map
				if (String.Compare(context.Request.Params["TRANSPARENT"], "TRUE", ignorecase) == 0)
					map.BackColor = System.Drawing.Color.Transparent;
				else if (context.Request.Params["BGCOLOR"] != null)
				{
					try { map.BackColor = System.Drawing.ColorTranslator.FromHtml(context.Request.Params["BGCOLOR"]); }
					catch { WmsException.ThrowWmsException("Invalid parameter BGCOLOR"); return; };
				}
				else
					map.BackColor = System.Drawing.Color.White;

				//Get the image format requested
				System.Drawing.Imaging.ImageCodecInfo imageEncoder = GetEncoderInfo(context.Request.Params["FORMAT"]);
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
					WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue, "Invalid parameter WIDTH");
					return;
				}
				else if (description.MaxWidth > 0 && width > description.MaxWidth)
				{
					WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported, "Parameter WIDTH too large");
					return;
				} 
				if (!int.TryParse(context.Request.Params["HEIGHT"], out height))
				{
					WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue, "Invalid parameter HEIGHT");
					return;
				}
				else if (description.MaxHeight > 0 && height > description.MaxHeight)
				{
					WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported, "Parameter HEIGHT too large");
					return;
				}
				map.Size = new System.Drawing.Size(width, height);

				SharpMap.Geometries.BoundingBox bbox = ParseBBOX(context.Request.Params["bbox"]);
				if (bbox == null)
				{
					WmsException.ThrowWmsException("Invalid parameter BBOX");
					return;
				}
				map.PixelAspectRatio = ((double)width / (double)height) / (bbox.Width / bbox.Height);
				map.Center = bbox.GetCentroid();
				map.Zoom = bbox.Width;

				//Set layers on/off
				if (!String.IsNullOrEmpty(context.Request.Params["LAYERS"])) //If LAYERS is empty, use default layer on/off settings
				{
					string[] layers = context.Request.Params["LAYERS"].Split(new char[] { ',' });
					if(description.LayerLimit>0)
					{
						if (layers.Length == 0 && map.Layers.Count > description.LayerLimit ||
							layers.Length > description.LayerLimit)
						{
							WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported, "Too many layers requested");
							return;
						}
					}
                    foreach (SharpMap.Layers.ILayer layer in map.Layers)
						layer.Enabled = false;
                    foreach (string layer in layers)
                    {

                        //SharpMap.Layers.ILayer lay = map.Layers.Find(delegate(SharpMap.Layers.ILayer findlay) { return findlay.LayerName == layer; });
                        SharpMap.Layers.ILayer lay = null;
                        for (int i = 0; i < map.Layers.Count; i++)
                            if (String.Equals(map.Layers[i].LayerName, layer, StringComparison.InvariantCultureIgnoreCase))
                                 lay = map.Layers[i];

                        


                        

                        
                        
                        if (lay == null)
						{
							WmsException.ThrowWmsException(WmsException.WmsExceptionCode.LayerNotDefined, "Unknown layer '" + layer + "'");
							return;
						}
						else
							lay.Enabled = true;
                    }
				}
				//Render map
				System.Drawing.Image img = map.GetMap();

				//Png can't stream directy. Going through a memorystream instead
				System.IO.MemoryStream MS = new System.IO.MemoryStream();
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
		private static System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			foreach(System.Drawing.Imaging.ImageCodecInfo encoder in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
				if (encoder.MimeType == mimeType)
					return encoder;
			return null;
		}

		/// <summary>
		/// Parses a boundingbox string to a boundingbox geometry from the format minx,miny,maxx,maxy. Returns null if the format is invalid
		/// </summary>
		/// <param name="strBBOX">string representation of a boundingbox</param>
		/// <returns>Boundingbox or null if invalid parameter</returns>
		private static SharpMap.Geometries.BoundingBox ParseBBOX(string strBBOX)
		{
			string[] strVals = strBBOX.Split(new char[] {','});
			if(strVals.Length!=4)
				return null;
			double minx = 0; double miny = 0;
			double maxx = 0; double maxy = 0;
			if (!double.TryParse(strVals[0], System.Globalization.NumberStyles.Float, SharpMap.Map.numberFormat_EnUS, out minx))
				return null;
			if (!double.TryParse(strVals[2], System.Globalization.NumberStyles.Float, SharpMap.Map.numberFormat_EnUS, out maxx))
				return null;
			if (maxx < minx)
				return null;

			if (!double.TryParse(strVals[1], System.Globalization.NumberStyles.Float, SharpMap.Map.numberFormat_EnUS, out miny))
				return null;
			if (!double.TryParse(strVals[3], System.Globalization.NumberStyles.Float, SharpMap.Map.numberFormat_EnUS, out maxy))
				return null;
			if (maxy < miny)
				return null;

			return new SharpMap.Geometries.BoundingBox(minx, miny, maxx, maxy);
		}
	}
}
