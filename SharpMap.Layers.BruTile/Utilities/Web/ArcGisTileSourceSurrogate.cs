// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile;
using BruTile.Web;

namespace SharpMap.Utilities.Web
{
    internal class ArcGisTileSourceSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var agts = (ArcGisTileSource) obj;
            
            // This is broken because of ITileSource.GetTile change
            
            // Provider
            //var type = agts.GetType();
            //info.AddValue("providerType", type);
            //info.AddValue("provider", agts.Provider, type);

            //Schema
            var type = agts.Schema.GetType();
            info.AddValue("schemaType", type);
            info.AddValue("schema", agts.Schema, type);

            //BaseUrl
            info.AddValue("baseUrl", agts.BaseUrl);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var agts = (ArcGisTileSource)obj;

            // Provider
            var type = (Type)info.GetValue("providerType", typeof(Type));
            Utility.SetPropertyValue(ref obj, "Provider", BindingFlags.NonPublic | BindingFlags.Instance, (ITileProvider)info.GetValue("provider", type));

            //Schema
            type = (Type) info.GetValue("schemaType", typeof (Type));
            Utility.SetPropertyValue(ref obj, "Schema", BindingFlags.NonPublic | BindingFlags.Instance, (ITileSchema)info.GetValue("schema", type));

            //BaseUrl
            Utility.SetPropertyValue(ref obj, "BaseUrl", BindingFlags.NonPublic | BindingFlags.Instance, info.GetString("baseUrl"));
            info.AddValue("baseUrl", agts.BaseUrl);

            return agts;
        }

        #endregion
    }
}