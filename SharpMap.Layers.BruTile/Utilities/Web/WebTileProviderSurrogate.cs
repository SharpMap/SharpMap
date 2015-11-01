// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile.Cache;

namespace BruTile.Web
{
    internal class HttpTileProviderSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var wp = (HttpTileProvider)obj;
            var request = Utility.GetFieldValue<IRequest>(wp, "_request");
            info.AddValue("_requestType", request.GetType());
            info.AddValue("_request", request);

            var fetchTile = Utility.GetFieldValue(wp, "_fetchTile", BindingFlags.NonPublic | BindingFlags.Instance, (Func<Uri, byte[]>)null);
            info.AddValue("_fetchTileType", fetchTile.GetType());
            info.AddValue("_fetchTile", fetchTile);

            IPersistentCache<byte[]> defaultCache = new NullCache();
            var cache = Utility.GetFieldValue(wp, "_persistentCache", BindingFlags.NonPublic | BindingFlags.Instance, defaultCache);
            if (cache == null) cache = new NullCache();
            info.AddValue("_persistentCacheType", cache.GetType());
            info.AddValue("_persistentCache", cache);

            /*
            info.AddValue("userAgent", Utility.GetPropertyValue(obj, "UserAgent", BindingFlags.NonPublic | BindingFlags.Instance, string.Empty));
            info.AddValue("referer", Utility.GetPropertyValue(obj, "Referer", BindingFlags.NonPublic | BindingFlags.Instance, string.Empty));
             */
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var wp = (HttpTileProvider)obj;
            var type = (Type)info.GetValue("_requestType", typeof(Type));
            Utility.SetFieldValue(ref obj, "_request", BindingFlags.NonPublic | BindingFlags.Instance, (IRequest)info.GetValue("_request", type));

            type = (Type)info.GetValue("_fetchTileType", typeof(Type));
            Utility.SetFieldValue(ref obj, "_fetchTile", BindingFlags.NonPublic | BindingFlags.Instance, info.GetValue("_fetchTile", type));

            type = (Type)info.GetValue("_persistentCacheType", typeof(Type));
            Utility.SetFieldValue(ref obj, "_persistentCache", BindingFlags.NonPublic | BindingFlags.Instance, info.GetValue("_persistentCache", type));

            /*
            Utility.SetFieldValue(ref obj, "_userAgent", newValue: info.GetString("userAgent"));
            Utility.SetFieldValue(ref obj, "_referer", newValue: info.GetString("referer"));
             */
            return wp;
        }

        #endregion
    }
}