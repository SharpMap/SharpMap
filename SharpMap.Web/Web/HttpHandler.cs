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
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using Image = System.Drawing.Image;

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
            var imgID = context.Request.QueryString["ID"];
            var cached = context.Cache[imgID];
            if (cached == null)
            {
                context.Response.Clear();
                context.Response.ContentType = "text/plain";
                context.Response.Write("Invalid Image requested");
                context.Response.Flush();
                context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (cached is byte[])
            {
                /*
                context.Response.ContentType = "image/png";
                var buffer = (byte[])cached;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                 */
                WriteResponseInChunks((byte[])cached, context.Response);
                context.ApplicationInstance.CompleteRequest();
                return;
            }

            //FObermaier:
            // Do we really need to check this, InsertIntoCache does the transformation to
            // an array of bytes.
            if (cached is Image)
            {
                //context.Response.ContentType = "image/png";
                var b = (Image) cached;
                
                // send the image to the viewer
                using (var ms = new MemoryStream())
                {
                    b.Save(ms, ImageFormat.Png);

                    //Don't tidy up we might need it again. If we want to tidy up we need to update the cached object to the 
                    //the buffer created below. Don't know if that works
                    //// tidy up  
                    //b.Dispose();

                    WriteResponseInChunks(ms.ToArray(), context.Response);
                    context.ApplicationInstance.CompleteRequest();
                    return;
                    //var buffer = ms.ToArray();
                    //context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }

        }

        #endregion

        /// <summary>
        /// The size of the chunks written to response.
        /// </summary>
        private const int ChunkSize = 2 * 8192;

        /// <summary>
        /// Method to write an array of bytes in chunks to a http response
        /// </summary>
        /// <remarks>
        /// The code was adopted from http://support.microsoft.com/kb/812406/en-us
        /// </remarks>
        /// <param name="buffer">The array of bytes</param>
        /// <param name="response">The response</param>
        private static void WriteResponseInChunks(byte[] buffer, HttpResponse response)
        {
            try
            {
                response.ClearContent();
                response.ContentType = "image/png";
                using (var ms = new MemoryStream(buffer))
                {
                    var dataToRead = buffer.Length;
                    while (dataToRead > 0)
                    {
                        if (response.IsClientConnected)
                        {
                            {
                                var tmpBuffer = new byte[ChunkSize];
                                
                                var length = ms.Read(tmpBuffer, 0, tmpBuffer.Length);
                                response.OutputStream.Write(tmpBuffer, 0, length);
                                response.Flush();

                                dataToRead -= length;
                            }
                        }
                        else
                        {
                            dataToRead = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.ClearContent();
                response.ContentType = "text/plain";
                response.Write(string.Format("Error     : {0}", ex.Message));
                response.Write(string.Format("Source    : {0}", ex.Message));
                response.Write(string.Format("StackTrace: {0}", ex.StackTrace));
            }
            finally
            {
                response.Flush();
                response.SuppressContent = true;
            }
        }
    }
}
