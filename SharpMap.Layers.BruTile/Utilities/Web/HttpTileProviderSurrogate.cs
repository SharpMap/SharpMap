// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile.Cache;
using BruTile.Web;

namespace SharpMap.Utilities.Web
{
    [Serializable]
    internal class HttpTileProviderSurrogate : ISerializationSurrogate
    {
        [Serializable]
        internal class HttpTileProviderRef : IObjectReference, ISerializable
        {
            private readonly HttpTileProvider _httpTileProvider;
            public HttpTileProviderRef(SerializationInfo info, StreamingContext context)
            {
                var type = (Type)info.GetValue("_requestType", typeof(Type));
                var request = (IRequest)info.GetValue("_request", type);

                type = (Type)info.GetValue("_fetchTileType", typeof(Type));
                var fetchTile = (Func<Uri, byte[]>)info.GetValue("_fetchTile", type);

                type = (Type)info.GetValue("_persistentCacheType", typeof(Type));
                var persistentCache = (IPersistentCache<byte[]>) info.GetValue("_persistentCache", type);

                _httpTileProvider = new HttpTileProvider(request, persistentCache, fetchTile);
            }
            
            object IObjectReference.GetRealObject(StreamingContext context)
            {
                return _httpTileProvider;
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotSupportedException();
            }
        }
        
        
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(HttpTileProviderRef));

            var wp = (HttpTileProvider)obj;
            var request = Utility.GetFieldValue<IRequest>(wp, "_request");
            info.AddValue("_requestType", request.GetType());
            info.AddValue("_request", request);

            var fetchTile = Utility.GetFieldValue(wp, "_fetchTile", BindingFlags.NonPublic | BindingFlags.Instance, (Func<Uri, byte[]>)null);
            info.AddValue("_fetchTileType", fetchTile.GetType());
            info.AddValue("_fetchTile", null);

            IPersistentCache<byte[]> defaultCache = new NullCache();
            var cache = Utility.GetPropertyValue(wp, "PersistentCache", BindingFlags.Public | BindingFlags.Instance, defaultCache);
            if (cache == null) cache = new NullCache();
            info.AddValue("_persistentCacheType", cache.GetType());
            info.AddValue("_persistentCache", cache);

            var httpClient = Utility.GetFieldValue<HttpClient>(obj, "_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var userAgent = httpClient.DefaultRequestHeaders.UserAgent;

            /*
            info.AddValue("userAgent", Utility.GetPropertyValue(obj, "UserAgent", BindingFlags.NonPublic | BindingFlags.Instance, string.Empty));
            info.AddValue("referer", Utility.GetPropertyValue(obj, "Referer", BindingFlags.NonPublic | BindingFlags.Instance, string.Empty));
             */
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return obj;
        }

        #endregion
    }
}
