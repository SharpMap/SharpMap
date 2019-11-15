// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile.Web;

namespace SharpMap.Utilities.Web
{
    internal class ArcGisTileRequestSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var agtc = (ArcGisTileRequest) obj;
            info.AddValue("baseUrl", Utility.GetFieldValue(agtc, "_baseUrl", BindingFlags.NonPublic | BindingFlags.Instance, string.Empty));
            var dict = Utility.GetFieldValue(agtc, "_customParameters", BindingFlags.NonPublic | BindingFlags.Instance,
                                             new Dictionary<string, string>());
            info.AddValue("customParametersCount", dict.Count);
            var i = 0;
            foreach (KeyValuePair<string, string> cp in dict)
                info.AddValue(string.Format("customParameter{0}", i++), string.Format("{0}\t{1}}",cp.Key, cp.Value));
            info.AddValue("format", Utility.GetFieldValue(agtc, "_format", BindingFlags.NonPublic | BindingFlags.Instance, string.Empty));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var agtc = (ArcGisTileRequest)obj;
            Utility.SetFieldValue(ref obj, "_baseUrl", BindingFlags.NonPublic | BindingFlags.Instance, info.GetString("baseUrl"));
            var dict = Utility.GetFieldValue(agtc, "_customParameters", BindingFlags.NonPublic | BindingFlags.Instance,
                                             new Dictionary<string, string>());
            var count = info.GetInt32("customParametersCount");
            for (var i = 0; i < count; i++)
            {
                var kvp = info.GetString(string.Format("customParameter{0}", i)).Split(new[] {'\t'});
                dict.Add(kvp[0], kvp[1]);
            }
            Utility.SetFieldValue(ref obj, "_customParameters", BindingFlags.NonPublic | BindingFlags.Instance, dict);
            Utility.SetFieldValue(ref obj, "_format", BindingFlags.NonPublic | BindingFlags.Instance, info.GetString("format"));
            return agtc;
        }

        #endregion

    }
}