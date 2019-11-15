// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile;
using BruTile.Web;

namespace SharpMap.Utilities.Web
{
    internal class HttpTileSourceSurrogate : ISerializationSurrogate
    {
        [Serializable]
        internal class HttpTileSourceRef : IObjectReference, ISerializable
        {
            private readonly HttpTileSource _httpTileSource;

            public HttpTileSourceRef(SerializationInfo info, StreamingContext context)
            {
                var name = info.GetString("name");

                // Schema
                var type = (Type)info.GetValue("schemaType", typeof(Type));
                var schema = (ITileSchema)info.GetValue("schema", type);

                // Attribution
                var attribution = new Attribution(info.GetString("attText"), info.GetString("attUrl"));

                // Provider
                var tp = (HttpTileProvider)info.GetValue("provider", typeof(HttpTileProvider));

                _httpTileSource = new HttpTileSource(schema, "http://localhost", attribution: attribution) {Name = name};
                object obj = _httpTileSource;
                Utility.SetFieldValue(ref obj, "_provider", BindingFlags.NonPublic | BindingFlags.Instance, tp);
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotSupportedException();
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                return _httpTileSource;
            }
        }
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ts = (HttpTileSource) obj;
            info.SetType(typeof(HttpTileSourceRef));
            info.AddValue("name", ts.Name, typeof(string));

            //Schema
            var type = ts.Schema.GetType();
            info.AddValue("schemaType", type);
            info.AddValue("schema", ts.Schema, type);
            
            // Attribution
            info.AddValue("attText", ts.Attribution?.Text ?? string.Empty);
            info.AddValue("attUrl", ts.Attribution?.Url?? string.Empty);

            //Provider
            var p = Utility.GetFieldValue<HttpTileProvider>(ts, "_provider");
            info.AddValue("provider", p, typeof(HttpTileProvider));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return obj;
        }
        #endregion
    }
}
