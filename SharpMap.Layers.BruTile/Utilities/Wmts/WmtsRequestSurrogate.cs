// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;
using BruTile.Wmts;

namespace SharpMap.Utilities.Wmts
{
    internal class WmtsRequestSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var urls = Utility.GetFieldValue<List<ResourceUrl>>(obj, "_resourceUrls");
            info.AddValue("numResourceUrls", urls.Count);
            for (var i = 0; i < urls.Count; i++)
                info.AddValue(string.Format("resourceUrl{0}", i), urls[i]);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var urls = new List<ResourceUrl>(info.GetInt32("numResourceUrls"));
            for(var i = 0; i < urls.Capacity;i++)
                urls.Add((ResourceUrl)info.GetValue(string.Format("resourceUrl{0}",i), typeof(ResourceUrl)));

            Utility.SetFieldValue(ref obj, "_resourceUrls", newValue: urls);
            Utility.SetFieldValue(ref obj, "_syncLock", newValue: new object());
            return obj;
        }
    }
}