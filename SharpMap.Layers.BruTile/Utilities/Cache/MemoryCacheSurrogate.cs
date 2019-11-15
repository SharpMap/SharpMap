// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile;
using BruTile.Cache;

namespace SharpMap.Utilities.Cache
{
    internal class MemoryCacheSurrogate<T> : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ms = (MemoryCache<T>) obj;
            info.AddValue("min", ms.MinTiles);
            info.AddValue("max", ms.MaxTiles);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var ms = (MemoryCache<T>) obj;
            ms.MinTiles = info.GetInt32("min");
            ms.MaxTiles = info.GetInt32("max");
  
            Utility.SetFieldValue(ref obj, "_syncRoot", BindingFlags.NonPublic | BindingFlags.Instance, new object());
            Utility.SetFieldValue(ref obj, "_bitmaps", BindingFlags.NonPublic | BindingFlags.Instance, new Dictionary<TileIndex, T>());
            Utility.SetFieldValue(ref obj, "_touched", BindingFlags.NonPublic | BindingFlags.Instance, new Dictionary<TileIndex, long>());
            return obj;
        }

        #endregion
    }
}
