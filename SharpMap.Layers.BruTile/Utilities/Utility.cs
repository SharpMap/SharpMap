// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Tms;
using BruTile.Web;
using BruTile.Wmsc;
using BruTile.Wmts;
using SharpMap.Utilities.Cache;
using SharpMap.Utilities.Predefined;
using SharpMap.Utilities.Web;
using SharpMap.Utilities.Wmts;
using SharpMap.Utilities.Wmts.Generated;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Utility class for reflection on BruTile classes
    /// </summary>
    public static class Utility
    {
        internal const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        internal static void SetFieldValue<T>(ref object obj, string fieldName,
                                              BindingFlags bindingFlags = PrivateInstance, T newValue = default)
        {
            var type = obj.GetType();
            var field = SearchField(type, fieldName, bindingFlags);
            var setObj = (bindingFlags & BindingFlags.Static) == BindingFlags.Static ? null : obj;
            if (field != null)
                field.SetValue(setObj, newValue);
            else
                System.Diagnostics.Trace.WriteLine(obj,
                                                   string.Format("could not get '{0}' from '{1}'", fieldName,
                                                                 obj.GetType()));
        }

        internal static T GetFieldValue<T>(object obj, string fieldName, BindingFlags bindingFlags = PrivateInstance,
                                           T defaultValue = default)
        {
            var type = obj.GetType();
            var field = SearchField(type, fieldName, bindingFlags);
            if (field != null)
                return (T) field.GetValue(obj);
            System.Diagnostics.Trace.WriteLine(obj,
                                               string.Format("could not get '{0}' from '{1}'", fieldName, obj.GetType()));
            return defaultValue;
        }

        internal static FieldInfo SearchField(Type type, string name, BindingFlags bindingFlags)
        {
            var ex = CheckBindingFlags(bindingFlags);
            if (ex != null) throw ex;

            while (type != null)
            {
                var fi = type.GetField(name, bindingFlags);
                if (fi != null)
                    return fi;
                type = type.BaseType;
            }
            return null;
        }

        internal static PropertyInfo SearchProperty(Type type, string name, BindingFlags bindingFlags, bool set = false)
        {
            var ex = CheckBindingFlags(bindingFlags);
            if (ex != null) throw ex;

            while (type != null)
            {
                var fi = type.GetProperty(name, bindingFlags);
                if (fi != null)
                {
                    if (set && !fi.CanWrite) fi = null;
                    if (fi != null) return fi;
                }
                type = type.BaseType;
            }
            return null;
        }

        private static Exception CheckBindingFlags(BindingFlags bindingFlags)
        {
            if ((bindingFlags & (BindingFlags.Public | BindingFlags.NonPublic)) == BindingFlags.Default)
                return new ArgumentException("Binding flags need to specify Public or NonPublic");
            if ((bindingFlags & (BindingFlags.Static | BindingFlags.Instance)) == BindingFlags.Default)
                return new ArgumentException("Binding flags need to specify Static or Instance");
            return null;
        }

        internal static void SetPropertyValue<T>(ref object obj, string propertyName,
                                                 BindingFlags bindingFlags = PrivateInstance, T newValue = default)
        {
            var type = obj.GetType();
            var property = SearchProperty(type, propertyName, bindingFlags, true);
            
            var setObj = (bindingFlags & BindingFlags.Static) == BindingFlags.Static ? null : obj;
            if (property != null)
                property.SetValue(setObj, newValue, null);
            else
                System.Diagnostics.Trace.WriteLine(obj,
                                                   string.Format("could not get '{0}' from '{1}'", propertyName,
                                                                 obj.GetType()));
        }

        internal static T GetPropertyValue<T>(object obj, string propertyName,
                                              BindingFlags bindingFlags = PrivateInstance, T newValue = default)
        {
            var type = obj.GetType();
            var property = SearchProperty(type, propertyName, bindingFlags);
            
            if (property != null)
                return (T) property.GetValue(obj, null);
            System.Diagnostics.Trace.WriteLine(obj,
                                               string.Format("could not get '{0}' from '{1}'", propertyName,
                                                             obj.GetType()));
            return newValue;
        }

        /// <summary>
        /// Adds required surrogates to <paramref name="formatter"/> to (de-) serialize BruTile objects.
        /// </summary>
        /// <param name="formatter">A formatter</param>
        public static void AddBruTileSurrogates(this IFormatter formatter)
        {
            var ss = new SurrogateSelector();

            // Base types
            ss.AddSurrogate(typeof (Extent), new StreamingContext(StreamingContextStates.All), new ExtentSurrogate());
            ss.AddSurrogate(typeof (Resolution), new StreamingContext(StreamingContextStates.All),
                            new ResolutionSurrogate());
            ss.AddSurrogate(typeof (TileSchema), new StreamingContext(StreamingContextStates.All),
                            new TileSchemaSurrogate());

            // Caches
            ss.AddSurrogate(typeof (NullCache), new StreamingContext(StreamingContextStates.All),
                            new NullCacheSurrogate());
            ss.AddSurrogate(typeof (MemoryCache<byte[]>), new StreamingContext(StreamingContextStates.All),
                            new MemoryCacheSurrogate<byte[]>());
            ss.AddSurrogate(typeof (MemoryCache<System.Drawing.Bitmap>),
                            new StreamingContext(StreamingContextStates.All),
                            new MemoryCacheSurrogate<System.Drawing.Bitmap>());
            ss.AddSurrogate(typeof(FileCache),
                new StreamingContext(StreamingContextStates.All),
                new FileCacheSurrogate());

            // Predefined
            var tss1 = new TileSchemaSurrogate();
            ss.AddSurrogate(typeof (GlobalMercator), new StreamingContext(StreamingContextStates.All), tss1);
            ss.AddSurrogate(typeof(GlobalSphericalMercator), new StreamingContext(StreamingContextStates.All), tss1);
            //ss.AddSurrogate(typeof(SphericalMercatorWorldSchema), new StreamingContext(StreamingContextStates.All), tss1);
            //ss.AddSurrogate(typeof (SphericalMercatorInvertedWorldSchema), 
            //                new StreamingContext(StreamingContextStates.All),tss1);
            //ss.AddSurrogate(typeof (BingSchema), new StreamingContext(StreamingContextStates.All), tss1);
            ss.AddSurrogate(typeof (WkstNederlandSchema), new StreamingContext(StreamingContextStates.All), tss1);
            ss.AddSurrogate(typeof (WmtsTileSchema), new StreamingContext(StreamingContextStates.All),
                new WmtsTileSchemaSurrogate());

            //Web
            var tss2 = new TileSourceSurrogate();
            ss.AddSurrogate(typeof(TileSource), new StreamingContext(StreamingContextStates.All), tss2);
            ss.AddSurrogate(typeof (ArcGisTileRequest), new StreamingContext(StreamingContextStates.All),
                            new ArcGisTileRequestSurrogate());
            ss.AddSurrogate(typeof (ArcGisTileSource), new StreamingContext(StreamingContextStates.All), 
                            new ArcGisTileSourceSurrogate());
            ss.AddSurrogate(typeof (BasicRequest), new StreamingContext(StreamingContextStates.All),
                            new BasicRequestSurrogate());
            
            ss.AddSurrogate(typeof(TmsRequest), new StreamingContext(StreamingContextStates.All),
                            new TmsRequestSurrogate());
            ss.AddSurrogate(typeof (TmsTileSource), new StreamingContext(StreamingContextStates.All), tss2);

            ss.AddSurrogate(typeof (HttpTileProvider), new StreamingContext(StreamingContextStates.All),
                            new HttpTileProviderSurrogate());
            ss.AddSurrogate(typeof(WmscTileSource), new StreamingContext(StreamingContextStates.All), tss2);
            ss.AddSurrogate(typeof(WmscRequest), new StreamingContext(StreamingContextStates.All),
                            new WmscRequestSurrogate());

            ss.AddSurrogate(typeof(HttpTileSource), new StreamingContext(StreamingContextStates.All), 
                            new HttpTileSourceSurrogate());

            //Wmts
            ss.AddSurrogate(typeof(ResourceUrl), new StreamingContext(StreamingContextStates.All), new ResourceUrlSurrogate());
            ss.AddSurrogate(typeof(WmtsRequest), new StreamingContext(StreamingContextStates.All), new WmtsRequestSurrogate());
            
            
            if (formatter.SurrogateSelector != null)
                formatter.SurrogateSelector.ChainSelector(ss);
            else
                formatter.SurrogateSelector = ss;
        }

        /// <summary>
        /// Serializes a <see cref="IList{T}"/> to <paramref name="info"/>
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="obj">The list</param>
        /// <param name="info">A streaming info</param>
        /// <param name="context">A serialization context</param>
        /// <param name="baseFieldName">A base field name</param>
        public static void GetList<T>(IList<T> obj, SerializationInfo info, StreamingContext context,
                                      string baseFieldName)
        {
            info.AddValue(baseFieldName + "IsNull", obj==null);
            if (obj == null) return;

            info.AddValue(baseFieldName + "Type", obj.GetType());
            info.AddValue(baseFieldName + "Count", obj.Count);
            for (int i = 0; i < obj.Count; i++)
            {
                info.AddValue(baseFieldName + i, obj[i]);
            }
        }

        /// <summary>
        /// Deserializes a <see cref="IList{T}"/> from <paramref name="info"/>
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="info">A streaming info</param>
        /// <param name="context">A serialization context</param>
        /// <param name="baseFieldName">A base field name</param>
        public static IList<T> SetList<T>(SerializationInfo info, StreamingContext context, string baseFieldName)
        {
            if (info.GetBoolean(baseFieldName + "IsNull"))
                return null;

            var type = (Type)info.GetValue(baseFieldName + "Type", typeof(Type));
            var instance = (IList<T>)Activator.CreateInstance(type);

            var count = info.GetInt32(baseFieldName + "Count");
            for (int i = 0; i < count; i++)
                instance.Add((T)info.GetValue(baseFieldName + i, typeof(T)));
            return instance;
        }

        /// <summary>
        /// Serializes a <see cref="IDictionary{TKey,TValue}"/> to <paramref name="info"/>
        /// </summary>
        /// <typeparam name="TKey">The type of the key items in the dictionary</typeparam>
        /// <typeparam name="TValue">The type of the value items in the dictionary</typeparam>
        /// <param name="obj">The list</param>
        /// <param name="info">A streaming info</param>
        /// <param name="context">A serialization context</param>
        /// <param name="baseFieldName">A base field name</param>
        public static void GetDictionary<TKey, TValue>(IDictionary<TKey, TValue> obj, SerializationInfo info, StreamingContext context,
                                            string baseFieldName)
        {
            info.AddValue(baseFieldName + "IsNull", obj == null);
            if (obj == null) return;

            info.AddValue(baseFieldName + "Type", obj.GetType());
            info.AddValue(baseFieldName + "Count", obj.Count);
            var i = 0;
            foreach (var kvp in obj)
            {
                info.AddValue(baseFieldName + "Key" + i, kvp.Key);
                info.AddValue(baseFieldName + "Value" + i++, kvp.Value);
            }
        }

        /// <summary>
        /// Deserializes a <see cref="IDictionary{TKey,TValue}"/> from <paramref name="info"/>
        /// </summary>
        /// <typeparam name="TKey">The type of the key items in the dictionary</typeparam>
        /// <typeparam name="TValue">The type of the value items in the dictionary</typeparam>
        /// <param name="info">A streaming info</param>
        /// <param name="context">A serialization context</param>
        /// <param name="baseFieldName">A base field name</param>
        public static IDictionary<TKey, TValue> SetDictionary<TKey, TValue>(SerializationInfo info, StreamingContext context, string baseFieldName)
        {
            if (info.GetBoolean(baseFieldName + "IsNull"))
                return null;

            var type = (Type)info.GetValue(baseFieldName + "Type", typeof(Type));
            var instance = (IDictionary<TKey, TValue>)Activator.CreateInstance(type);

            var count = info.GetInt32(baseFieldName + "Count");
            for (var i = 0; i < count; i++)
            {
                var key = (TKey) info.GetValue(baseFieldName + "Key" + i, typeof (TKey));
                var value = (TValue)info.GetValue(baseFieldName + "Value" + i, typeof(TValue));
                instance.Add(key, value);
            }
            return instance;
        }

    }
}
