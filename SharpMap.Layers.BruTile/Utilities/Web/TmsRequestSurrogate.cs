// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SharpMap.Utilities.Web
{
    internal class TmsRequestSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            info.AddValue("baseUrl", Utility.GetFieldValue<string>(obj, "_baseUrl"));
            Utility.GetDictionary(Utility.GetFieldValue(obj, "_baseUrls", Utility.PrivateInstance, (IDictionary<string, System.Uri>)null), info, context, "baseUrls");
            info.AddValue("imageFormat", Utility.GetFieldValue(obj, "_imageFormat", Utility.PrivateInstance, "png"));
            Utility.GetDictionary(Utility.GetFieldValue(obj, "_customParameters", Utility.PrivateInstance, (IDictionary<string, string>)null), info, context, "baseUrls");
            Utility.GetList(Utility.GetFieldValue(obj, "_serverNodes", Utility.PrivateInstance, (IList<string>)null), info, context, "baseUrls");
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Utility.SetFieldValue(ref obj, "_baseUrl", Utility.PrivateInstance, info.GetString("baseUrl"));
            Utility.SetFieldValue(ref obj, "_baseUrls", Utility.PrivateInstance, 
                                  Utility.SetDictionary<string, System.Uri>(info, context, "baseUrls"));
            Utility.SetFieldValue(ref obj, "_imageFormat", Utility.PrivateInstance, info.GetString("imageFormat"));
            Utility.SetFieldValue(ref obj, "_customParameters", Utility.PrivateInstance,
                                  Utility.SetDictionary<string, string>(info, context, "customParameters"));
            Utility.SetFieldValue(ref obj, "_serverNodes", Utility.PrivateInstance, 
                                  Utility.SetList<string>(info, context,"serverNodes"));
            //Not serialized, but nonethelesss needed.
            Utility.SetFieldValue(ref obj, "_random", Utility.PrivateInstance, new System.Random());
            return null;
        }

        #endregion
    }
}