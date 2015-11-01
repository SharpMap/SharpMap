// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.
using System.Reflection;
using System.Runtime.Serialization;

namespace BruTile
{
    internal class ExtentSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ex = (Extent)obj;
            info.AddValue("minX", ex.MinX);
            info.AddValue("maxX", ex.MaxX);
            info.AddValue("minY", ex.MinY);
            info.AddValue("maxY", ex.MaxY);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Utility.SetPropertyValue(ref obj, "MinX", BindingFlags.Public | BindingFlags.Instance, info.GetDouble("minX"));
            Utility.SetPropertyValue(ref obj, "MaxX", BindingFlags.Public | BindingFlags.Instance, info.GetDouble("maxX"));
            Utility.SetPropertyValue(ref obj, "MinY", BindingFlags.Public | BindingFlags.Instance, info.GetDouble("minY"));
            Utility.SetPropertyValue(ref obj, "MaxY", BindingFlags.Public | BindingFlags.Instance, info.GetDouble("maxY"));
            
            return obj;
        }

        #endregion
    }
}