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
using System.IO;
using System.Web;
using System.Web.Caching;

namespace SharpMap.Web
{
    /// <summary>
    /// Class for storing rendered images in the httpcache
    /// </summary>
    public class Caching
    {
        /// <summary>
        /// Inserts an image into the HttpCache and returns the cache identifier.
        /// </summary>
        /// <remarks>
        /// Image can after insertion into the cache be requested by calling getmap.aspx?ID=[identifier]<br/>
        /// This requires you to add the following to web.config:
        /// <code escaped="true">
        /// <httpHandlers>
        ///	   <add verb="*" path="GetMap.aspx" type="SharpMap.Web.HttpHandler,SharpMap.Web"/>
        /// </httpHandlers>
        /// </code>
        /// <example>
        /// Inserting the map into the cache and setting the ImageUrl:
        /// <code>
        /// string imgID = SharpMap.Web.Caching.CacheMap(5, myMap.GetMap(), Session.SessionID, Context);
        /// imgMap.ImageUrl = "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="minutes">Number of minutes to cache the map</param>
        /// <param name="map">Map reference</param>
        /// <returns>Image identifier</returns>
        public static string InsertIntoCache(int minutes, Image map)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            using (var stream = new MemoryStream())
            {
                map.Save(stream, ImageFormat.Png);
                HttpContext.Current.Cache.Insert(guid, stream.ToArray(), null, 
                                                 Cache.NoAbsoluteExpiration,
                                                 TimeSpan.FromMinutes(minutes));
            }
            map.Dispose();
            return guid;
        }
    }
}
