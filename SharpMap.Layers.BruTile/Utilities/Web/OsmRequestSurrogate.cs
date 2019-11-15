// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

namespace SharpMap.Utilities.Web
{
    //[Obsolete]
    //internal class OsmRequestSurrogate : ISerializationSurrogate
    //{
    //    #region Implementation of ISerializationSurrogate

    //    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    //    {
    //        var or = (OsmRequest)obj;
    //        info.AddValue("config", or.OsmConfig);
    //    }

    //    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    //    {
    //        Utility.SetFieldValue(ref obj, "OsmConfig", BindingFlags.Public | BindingFlags.Instance, info.GetValue("config", typeof(OsmTileServerConfig)));
    //        return obj;
    //    }

    //    #endregion
    //}

    //[Obsolete]
    //public class OsmTileServerConfigSurrogate : ISerializationSurrogate
    //{
    //    #region Implementation of ISerializationSurrogate

    //    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    //    {
    //        var oc = (OsmTileServerConfig)obj;
    //        info.AddValue("urlFormat", oc.UrlFormat);
    //        info.AddValue("serverCount", oc.NumberOfServers);
    //        info.AddValue("serverIdentifier", oc.ServerIdentifier);
    //        info.AddValue("min", oc.MinResolution);
    //        info.AddValue("max", oc.MaxResolution);
    //    }

    //    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    //    {
    //        Utility.SetFieldValue(ref obj, "UrlFormat", BindingFlags.Public | BindingFlags.Instance, info.GetString("urlFormat"));
    //        Utility.SetFieldValue(ref obj, "NumberOfServers", BindingFlags.Public | BindingFlags.Instance, info.GetInt32("serverCount"));
    //        Utility.SetFieldValue(ref obj, "ServerIdentifier", BindingFlags.Public | BindingFlags.Instance, info.GetValue("serverIdentifier", typeof(string[])));
    //        Utility.SetFieldValue(ref obj, "MinResolution", BindingFlags.Public | BindingFlags.Instance, info.GetInt32("min"));
    //        Utility.SetFieldValue(ref obj, "MaxResolution", BindingFlags.Public | BindingFlags.Instance, info.GetInt32("max"));
    //        return obj;
    //    }

    //    #endregion
    //}
}
