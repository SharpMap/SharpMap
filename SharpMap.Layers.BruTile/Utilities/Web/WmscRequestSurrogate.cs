// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SharpMap.Utilities.Web
{
    internal class WmscRequestSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            info.AddValue("baseUrl", Utility.GetFieldValue(obj, "_baseUrl", Utility.PrivateInstance, string.Empty));
            Utility.GetDictionary(Utility.GetFieldValue(obj, "_customParameters", Utility.PrivateInstance, (IDictionary<string, string>)null), info, context, "baseUrls");
            Utility.GetList(Utility.GetFieldValue(obj, "_layers", Utility.PrivateInstance, (IList<string>)null), info, context, "layers");
            Utility.GetList(Utility.GetFieldValue(obj, "_styles", Utility.PrivateInstance, (IList<string>)null), info, context, "styles");
            info.AddValue("format", Utility.GetFieldValue(obj, "_format", Utility.PrivateInstance, "png"));
            info.AddValue("width", Utility.GetFieldValue(obj, "_width", Utility.PrivateInstance, 100));
            info.AddValue("height", Utility.GetFieldValue(obj, "_height", Utility.PrivateInstance, 100));
            info.AddValue("srs", Utility.GetFieldValue(obj, "_srs", Utility.PrivateInstance, "4326"));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Utility.SetFieldValue(ref obj, "_baseUrl", Utility.PrivateInstance, info.GetString("baseUrl"));
            Utility.SetFieldValue(ref obj, "_customParameters", Utility.PrivateInstance,
                                  Utility.SetDictionary<string, string>(info, context, "customParameters"));
            Utility.SetFieldValue(ref obj, "_layers", Utility.PrivateInstance,
                                  Utility.SetList<string>(info, context, "layers"));
            Utility.SetFieldValue(ref obj, "_styles", Utility.PrivateInstance,
                                  Utility.SetList<string>(info, context, "styles"));

            Utility.SetFieldValue(ref obj, "_format", Utility.PrivateInstance, info.GetString("format"));
            Utility.SetFieldValue(ref obj, "_width", Utility.PrivateInstance, info.GetInt32("width"));
            Utility.SetFieldValue(ref obj, "_width", Utility.PrivateInstance, info.GetInt32("height"));
            Utility.SetFieldValue(ref obj, "_srs", Utility.PrivateInstance, info.GetString("srs"));
            return obj;
        }

        #endregion
    }
}