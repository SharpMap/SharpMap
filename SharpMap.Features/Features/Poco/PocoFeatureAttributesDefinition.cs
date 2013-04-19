using System;
using System.Collections.Generic;
using System.Reflection;
using GeoAPI.Features;

namespace SharpMap.Features.Poco
{
    public class PocoFeatureAttributesDefinition : List<PocoFeatureAttributeDefinition>
    {
        public PocoFeatureAttributesDefinition(Type type)
        {
            if (!typeof(IFeature).IsAssignableFrom(type))
                throw new ArgumentException("The given type is not a feature");

            var props = GetPropertyInfos(type);
            while (props.Count > 0)
            {
                var d = new PocoFeatureAttributeDefinition(props.Pop());
                if (d.Valid) Add(d);
            }

            var fields = GetFieldInfos(type);
            while (fields.Count > 0)
            {
                var d = new PocoFeatureAttributeDefinition(fields.Pop());
                if (d.Valid) Add(d);
            }
        }

        private static Stack<PropertyInfo> GetPropertyInfos(Type type)
        {
            var res = new Stack<PropertyInfo>();

            var pi = type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | 
                                        BindingFlags.Public | 
                                        BindingFlags.Static | BindingFlags.Instance);

            foreach (var propertyInfo in pi)
            {
                res.Push(propertyInfo);
            }

            if (type.BaseType != typeof (object))
            {
                var basePi = GetPropertyInfos(type.BaseType);
                foreach (var propertyInfo in basePi)
                {
                    res.Push(propertyInfo);
                }
            }

            return res;
        }

        private static Stack<FieldInfo> GetFieldInfos(Type type)
        {
            var res = new Stack<FieldInfo>();

            var pi = type.GetFields(BindingFlags.GetProperty | BindingFlags.SetProperty |
                                    BindingFlags.Public |
                                    BindingFlags.Static | BindingFlags.Instance);

            foreach (var propertyInfo in pi)
            {
                res.Push(propertyInfo);
            }

            if (type.BaseType != typeof(object))
            {
                var basePi = GetFieldInfos(type.BaseType);
                foreach (var propertyInfo in basePi)
                {
                    res.Push(propertyInfo);
                }
            }

            return res;
        }

        public int GetOrdinal(string name)
        {
            // maybe a dictionary is better
            return FindIndex(0, t => t.AttributeName == name);
        }

        public IList<IFeatureAttributeDefinition> AsList()
        {
            var res = new IFeatureAttributeDefinition[Count];
            Array.Copy(ToArray(), 0, res, 0, Count);
            return res;
        }
    }
}