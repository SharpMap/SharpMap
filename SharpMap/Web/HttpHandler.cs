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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace SharpMap.Web
{
    /// <summary>
    /// HttpHandler class for web applications
    /// </summary>
    public class HttpHandler : IHttpHandler
    {
        #region IHttpHandler Members

        /// <summary>
        /// Enable Http pooling
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the requested image in an http imagestream
        /// </summary>
        /// <param name="context">HttpContext</param>
        public void ProcessRequest(HttpContext context)
        {
            string imgID = context.Request.QueryString["ID"];
            if (context.Cache[imgID] == null)
            {
                context.Response.Clear();
                context.Response.Write("Invalid Image requested");
                return;
            }
            if (context.Cache[imgID].GetType() == typeof (Bitmap))
            {
                context.Response.ContentType = "image/png";
                Bitmap b = (Bitmap) context.Cache[imgID];
                // send the image to the viewer
                MemoryStream MS = new MemoryStream();
                b.Save(MS, ImageFormat.Png);
                // tidy up  
                b.Dispose();

                byte[] buffer = MS.ToArray();
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else if (context.Cache[imgID].GetType() == typeof (byte[]))
            {
                context.Response.ContentType = "image/png";
                byte[] buffer = (byte[]) context.Cache[imgID];
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                return;
            }
            context.Response.End();
        }

        #endregion
    }
}