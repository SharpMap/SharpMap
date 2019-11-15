// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using BruTile.Wmts;
using BruTile.Wmts.Generated;

namespace SharpMap.Utilities.Wmts
{
    namespace Generated
    {

        internal class ResourceUrlSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var ru = (ResourceUrl) obj;
                info.AddValue("format", ru.Format);
                info.AddValue("template", ru.Template);
                info.AddValue("resourceType", (int) ru.ResourceType);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
                ISurrogateSelector selector)
            {
                var ru = (ResourceUrl) obj;
                ru.Format = info.GetString("format");
                ru.Template = info.GetString("template");
                ru.ResourceType = (URLTemplateTypeResourceType) info.GetInt32("resourceType");
                return ru;
            }
        }
    }
}