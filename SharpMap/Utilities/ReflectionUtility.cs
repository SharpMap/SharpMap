using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace SharpMap.Utilities
{
    internal static class ReflectionUtility
    {
        internal const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        internal static void SetFieldValue<T>(ref object obj, string fieldName,
                                              BindingFlags bindingFlags = PrivateInstance, T newValue = default(T))
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
                                           T defaultValue = default(T))
        {
            var type = obj.GetType();
            var field = SearchField(type, fieldName, bindingFlags);
            if (field != null)
                return (T)field.GetValue(obj);
            System.Diagnostics.Trace.WriteLine(obj,
                                               string.Format("could not get '{0}' from '{1}'", fieldName, obj.GetType()));
            return defaultValue;
        }

        internal static FieldInfo SearchField(Type type, string name, BindingFlags bindingFlags)
        {
            CheckBindingFlags(bindingFlags);
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
            CheckBindingFlags(bindingFlags);
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

        // ReSharper disable UnusedParameter.Local
        private static void CheckBindingFlags(BindingFlags bindingFlags)
        // ReSharper restore UnusedParameter.Local
        {
            if ((bindingFlags & (BindingFlags.Public | BindingFlags.NonPublic)) == BindingFlags.Default)
                throw new ArgumentException("Binding flags need to specify Public or NonPublic");
            if ((bindingFlags & (BindingFlags.Static | BindingFlags.Instance)) == BindingFlags.Default)
                throw new ArgumentException("Binding flags need to specify Static or Instance");
        }

        internal static void SetPropertyValue<T>(ref object obj, string propertyName,
                                                 BindingFlags bindingFlags = PrivateInstance, T newValue = default(T))
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
                                              BindingFlags bindingFlags = PrivateInstance, T newValue = default(T))
        {
            var type = obj.GetType();
            var property = SearchProperty(type, propertyName, bindingFlags);

            if (property != null)
                return (T)property.GetValue(obj, null);
            System.Diagnostics.Trace.WriteLine(obj,
                                               string.Format("could not get '{0}' from '{1}'", propertyName,
                                                             obj.GetType()));
            return newValue;
        }

        public static void GetList<T>(IList<T> obj, SerializationInfo info, StreamingContext context,
                                      string baseFieldName)
        {
            info.AddValue(baseFieldName + "IsNull", obj == null);
            if (obj == null) return;

            info.AddValue(baseFieldName + "Type", obj.GetType());
            info.AddValue(baseFieldName + "Count", obj.Count);
            for (var i = 0; i < obj.Count; i++)
            {
                info.AddValue(baseFieldName + i, obj[i]);
            }
        }

        public static IList<T> SetList<T>(SerializationInfo info, StreamingContext context, string baseFieldName)
        {
            if (info.GetBoolean(baseFieldName + "IsNull"))
                return null;

            var type = (Type)info.GetValue(baseFieldName + "Type", typeof(Type));
            var instance = (IList<T>)Activator.CreateInstance(type);

            var count = info.GetInt32(baseFieldName + "Count");
            for (var i = 0; i < count; i++)
                instance.Add((T)info.GetValue(baseFieldName + i, typeof(T)));
            return instance;
        }

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

        public static IDictionary<TKey, TValue> SetDictionary<TKey, TValue>(SerializationInfo info, StreamingContext context, string baseFieldName)
        {
            if (info.GetBoolean(baseFieldName + "IsNull"))
                return null;

            var type = (Type)info.GetValue(baseFieldName + "Type", typeof(Type));
            var instance = (IDictionary<TKey, TValue>)Activator.CreateInstance(type);

            var count = info.GetInt32(baseFieldName + "Count");
            for (var i = 0; i < count; i++)
            {
                var key = (TKey)info.GetValue(baseFieldName + "Key" + i, typeof(TKey));
                var value = (TValue)info.GetValue(baseFieldName + "Value" + i, typeof(TValue));
                instance.Add(key, value);
            }
            return instance;
        }

    }
}