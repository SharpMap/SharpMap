// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using BruTile.Web;

namespace SharpMap.Utilities.Web
{
    [Obsolete]
    internal class BingRequestSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            info.AddValue("baseUrl", Utility.GetFieldValue(obj, "_baseUrl", Utility.PrivateInstance, BingRequest.UrlBingStaging));
            info.AddValue("token", Utility.GetFieldValue(obj, "_token", Utility.PrivateInstance, BingRequest.UrlBingStaging));
            info.AddValue("mapType", Utility.GetFieldValue(obj, "_mapType", Utility.PrivateInstance, 'h'));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Utility.SetFieldValue(ref obj, "_baseUrl", Utility.PrivateInstance, info.GetString("baseUrl"));
            Utility.SetFieldValue(ref obj, "_token", Utility.PrivateInstance, info.GetString("token"));
            Utility.SetFieldValue(ref obj, "_mapType", Utility.PrivateInstance, info.GetChar("mapType"));
            return obj;
        }

        #endregion
    }
}