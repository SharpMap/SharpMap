// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using BruTile;
using BruTile.Web;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace SharpMap.Utilities.Web
{
    internal class TileSourceSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate


        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ts = (TileSource)obj;

            info.AddValue("name", ts.Name);

            // Request
            var tr = Utility.GetFieldValue<IRequest>(ts, "_request");
            info.AddValue("requestType", tr.GetType());
            info.AddValue("request", tr);

            // Schema
            info.AddValue("schemaType", ts.Schema.GetType());
            info.AddValue("schema", ts.Schema);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var ts = (TileSource)obj;
            ts.Name = info.GetString("name");

            var type = (Type)info.GetValue("schemaType", typeof(Type));
            Utility.SetPropertyValue(ref obj, "Schema", BindingFlags.Public | BindingFlags.Instance, (ITileSchema)info.GetValue("schema", type));
            return obj;
        }

        #endregion
    }
}
