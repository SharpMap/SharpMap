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
using System.Web;
using SharpMap.Web.Wms.Server;

namespace SharpMap.Web.Wms
{
    /// <summary>
    /// Class for throwing WMS exceptions to client
    /// </summary>
    public static class WmsException
    {
        /// <summary>
        /// Throws a <see cref="WmsExceptionCode.NotApplicable"/> WMS excption. The <paramref name="context"/> is used to write the response stream.
        /// </summary>
        /// <param name="message">An additional message text</param>
        /// <param name="context">The <see cref="HttpContext"/></param>
        /// <exception cref="InvalidOperationException">Thrown if this function is used outside a valid valid <see cref="HttpContext"/></exception>
        public static void ThrowWmsException(string message, IContext context)
        {
            ThrowWmsException(WmsExceptionCode.NotApplicable, message, context);
        }

        /// <summary>
        /// Throws a <paramref name="code"/> WMS excption. The <paramref name="context"/> is used to write the response stream.
        /// </summary>
        /// <param name="code">The WMS exception code</param>
        /// <param name="message">An additional message text</param>
        /// <param name="context">The <see cref="HttpContext"/></param>
        /// <exception cref="InvalidOperationException">Thrown if this function is used outside a valid valid <see cref="HttpContext"/></exception>
        public static void ThrowWmsException(WmsExceptionCode code, string message, IContext context)
        {
            context.Clear();
            context.ContentType = "text/xml";
            context.Write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n");
            context.Write(
                "<ServiceExceptionReport version=\"1.3.0\" xmlns=\"http://www.opengis.net/ogc\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.opengis.net/ogc http://schemas.opengis.net/wms/1.3.0/exceptions_1_3_0.xsd\">\n");
            context.Write("<ServiceException");
            if (code != WmsExceptionCode.NotApplicable)
                context.Write(" code=\"" + code + "\"");
            context.Write(">" + message + "</ServiceException>\n");
            context.Write("</ServiceExceptionReport>");
            context.End();
        }

        /// <summary>
        /// WMS Exception codes
        /// </summary>
        public enum WmsExceptionCode
        {
            /// <summary>
            /// No error code
            /// </summary>
            NotApplicable,
            /// <summary>
            /// Request contains a Format not offered by the server.
            /// </summary>
            InvalidFormat,
            /// <summary>
            /// Request contains a CRS not offered by the server for one or more of the
            /// Layers in the request.
            /// </summary>
            InvalidCRS,
            /// <summary>
            /// GetMap request is for a Layer not offered by the server, or GetFeatureInfo
            /// request is for a Layer not shown on the map.
            /// </summary>
            LayerNotDefined,
            /// <summary>
            /// Request is for a Layer in a Style not offered by the server.
            /// </summary>
            StyleNotDefined,
            /// <summary>
            /// GetFeatureInfo request is applied to a Layer which is not declared queryable.
            /// </summary>
            LayerNotQueryable,
            /// <summary>
            /// GetFeatureInfo request contains invalid X or Y value.
            /// </summary>
            InvalidPoint,
            /// <summary>
            /// Value of (optional) UpdateSequence parameter in GetCapabilities request is
            /// equal to current value of service metadata update sequence number.
            /// </summary>
            CurrentUpdateSequence,
            /// <summary>
            /// Value of (optional) UpdateSequence parameter in GetCapabilities request is
            /// greater than current value of service metadata update sequence number.
            /// </summary>
            InvalidUpdateSequence,
            /// <summary>
            /// Request does not include a sample dimension value, and the server did not
            /// declare a default value for that dimension.
            /// </summary>
            MissingDimensionValue,
            /// <summary>
            /// Request contains an invalid sample dimension value.
            /// </summary>
            InvalidDimensionValue,
            /// <summary>
            /// Request is for an optional operation that is not supported by the server.
            /// </summary>
            OperationNotSupported            
        }
    }
}