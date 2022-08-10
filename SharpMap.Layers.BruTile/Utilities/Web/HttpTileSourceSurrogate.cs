// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using BruTile;
using BruTile.Web;
using System;
using System.Reflection;
using System.Runtime.Serialization;

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

                // Request
                var tr = (BasicRequest)info.GetValue("request", typeof(BasicRequest));
                var urlFormatter = Utility.GetFieldValue(tr, "_urlFormatter", BindingFlags.NonPublic | BindingFlags.Instance, "http://localhost");

                _httpTileSource = new HttpTileSource(schema, urlFormatter, attribution: attribution) { Name = name };
                object obj = _httpTileSource;
                Utility.SetFieldValue(ref obj, "_request", BindingFlags.NonPublic | BindingFlags.Instance, tr);
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
            var ts = (HttpTileSource)obj;
            info.SetType(typeof(HttpTileSourceRef));
            info.AddValue("name", ts.Name, typeof(string));

            //Schema
            var type = ts.Schema.GetType();
            info.AddValue("schemaType", type);
            info.AddValue("schema", ts.Schema, type);

            // Attribution
            info.AddValue("attText", ts.Attribution?.Text ?? string.Empty);
            info.AddValue("attUrl", ts.Attribution?.Url ?? string.Empty);

            //Request
            var br = Utility.GetFieldValue<BasicRequest>(ts, "_request");
            info.AddValue("request", br, typeof(BasicRequest));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return obj;
        }
        #endregion
    }
}
