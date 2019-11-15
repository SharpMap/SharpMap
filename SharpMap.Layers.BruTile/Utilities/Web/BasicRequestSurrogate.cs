// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile.Web;

namespace SharpMap.Utilities.Web
{
    internal class BasicRequestSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var br = (BasicRequest)obj;
            info.AddValue("urlFormatter", Utility.GetFieldValue(br, "_urlFormatter", BindingFlags.NonPublic | BindingFlags.Instance, string.Empty));
            info.AddValue("serverNodes", Utility.GetFieldValue<List<string>>(br, "_serverNodes"), typeof(List<string>));

        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var br = (BasicRequest)obj;
            Utility.SetFieldValue(ref obj, "_urlFormatter", BindingFlags.NonPublic | BindingFlags.Instance, info.GetString("urlFormatter"));
            Utility.SetFieldValue(ref obj, "_serverNodes", newValue:(List<string>)info.GetValue("serverNodes", typeof(List<string>)));
            Utility.SetFieldValue(ref obj, "_nodeCounterLock", newValue: new object());
            return br;
        }
    }
}