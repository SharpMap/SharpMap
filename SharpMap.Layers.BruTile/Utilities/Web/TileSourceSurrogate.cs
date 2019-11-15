// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile;

namespace SharpMap.Utilities.Web
{
    internal class TileSourceSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate


        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ts = (TileSource)obj;

            info.AddValue("name", ts.Name);

            // Provider
            var tp = Utility.GetFieldValue<ITileProvider>(ts, "_provider");
            info.AddValue("providerType", tp.GetType());
            info.AddValue("provider", tp);

            // Schema
            info.AddValue("schemaType", ts.Schema.GetType());
            info.AddValue("schema", ts.Schema);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var ts = (TileSource) obj;
            ts.Name = info.GetString("name");

            var type = (Type) info.GetValue("providerType", typeof (Type));
            Utility.SetFieldValue(ref obj, "_provider", BindingFlags.NonPublic | BindingFlags.Instance, (ITileProvider)info.GetValue("provider", type));
            type = (Type)info.GetValue("schemaType", typeof(Type));
            Utility.SetPropertyValue(ref obj, "Schema", BindingFlags.Public | BindingFlags.Instance, (ITileSchema)info.GetValue("schema", type));
            return obj;
        }

        #endregion
    }
}