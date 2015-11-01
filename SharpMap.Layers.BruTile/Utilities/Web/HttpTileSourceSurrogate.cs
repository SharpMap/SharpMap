// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace BruTile.Web
{
    internal class HttpTileSourceSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ts = (HttpTileSource) obj;
            info.AddValue("name", ts.Name, typeof(string));

            //Schema
            var type = ts.Schema.GetType();
            info.AddValue("schemaType", type);
            info.AddValue("schema", ts.Schema, type);
            
            //Provider
            var p = Utility.GetFieldValue<HttpTileProvider>(ts, "_provider");
            info.AddValue("provider", p, typeof(HttpTileProvider));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var ts = (HttpTileSource)obj;
            ts.Name = info.GetString("name");

            //Schema
            var type = (Type)info.GetValue("schemaType", typeof (Type));
            Utility.SetFieldValue(ref obj, "_tileSchema", newValue: info.GetValue("schema", type));

            //Provider
            Utility.SetFieldValue(ref obj, "_provider", newValue: info.GetValue("provider", typeof(HttpTileProvider)));

            return ts;
        }
        #endregion
    }
}