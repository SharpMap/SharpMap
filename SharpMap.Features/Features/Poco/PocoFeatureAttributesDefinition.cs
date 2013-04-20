using System;
using System.Collections.Generic;
using System.Reflection;
using GeoAPI.Features;

namespace SharpMap.Features.Poco
{
    using Tuple = KeyValuePair<PropertyInfo, FeatureAttributeAttribute>;
    
    public class PocoFeatureAttributesDefinition : List<PocoFeatureAttributeDefinition>
    {
        public PocoFeatureAttributesDefinition(Type type)
        {
            if (!typeof(IFeature).IsAssignableFrom(type))
                throw new ArgumentException("The given type is not a feature");

            var props = GetPropertyInfos(type);
            while (props.Count > 0)
            {
                var t = props.Pop();
                var d = new PocoFeatureAttributeDefinition(t.Key, t.Value);
                if (!d.Ignore) Add(d);
            }

            var fields = GetFieldInfos(type);
            while (fields.Count > 0)
            {
                var d = new PocoFeatureAttributeDefinition(fields.Pop());
                if (!d.Ignore) Add(d);
            }
        }

        private static FeatureAttributeAttribute Copy(FeatureAttributeAttribute att)
        {
            if (att == null) 
                return null;
            
            return new FeatureAttributeAttribute
                {
                    AttributeName = att.AttributeName,
                    AttributeDescription = att.AttributeDescription,
                    Ignore = att.Ignore,
                    Readonly = att.Readonly
                };
        }
        private static IEnumerable<Tuple> Verify(List<InterfaceMapping> mappings,
                                                 IEnumerable<PropertyInfo> propertyInfos)
        {

            foreach (var propertyInfo in propertyInfos)
            {
                FeatureAttributeAttribute fatt = null;
                // Check if this property has a FeatureAttributeAttribute attached to it.
                var att = propertyInfo.GetCustomAttributes(typeof (FeatureAttributeAttribute), true);
                
                if (att.Length > 0)
                {
                    fatt = (FeatureAttributeAttribute) att[0];
                    // Does it say ignore?
                    if (fatt.Ignore)
                    {
                        continue;
                    }
                }

                // See if we can find some interface that defines this property
                var foundInInterfaces = false;
                foreach (var interfaceMapping in mappings)
                {
                    var index = Array.IndexOf(interfaceMapping.TargetMethods, propertyInfo.GetGetMethod());
                    if (index >= 0)
                    {
                        // yes, here it is
                        foundInInterfaces = true;

                        var interfaceType = interfaceMapping.InterfaceType;
                        var pi = interfaceType.GetProperty(interfaceMapping.InterfaceMethods[index].Name.Substring(4));
                        
                        // Check if this property has a FeatureAttributeAttribute attached to it.
                        var attTmp = pi.GetCustomAttributes(typeof(FeatureAttributeAttribute), true);
                        if (attTmp.Length > 0)
                        {
                            // Does it say don't ignore it
                            if (!((FeatureAttributeAttribute)attTmp[0]).Ignore)
                                yield return new Tuple(propertyInfo, Copy((FeatureAttributeAttribute)attTmp[0]));
                        }

                        /* this does not work

                        // Check if this property has a FeatureAttributeAttribute attached to it.
                        att = interfaceMapping.InterfaceMethods[index].GetCustomAttributes(
                                typeof (FeatureAttributeAttribute), true);
                        if (att.Length > 0)
                        {
                            // Does it say don't ignore it
                            if (!((FeatureAttributeAttribute)att[0]).Ignore)
                                yield return propertyInfo;
                        }
                        */

                        // Won't find it in another interface
                        break;
                    }
                }

                if (!foundInInterfaces)
                {
                    yield return new Tuple(propertyInfo, Copy(fatt));
                }
            }
        }

        private static List<InterfaceMapping> GetInterfaceMappings(Type type)
        {
            var interfaces = type.GetInterfaces();
            var interfaceMaps = new List<InterfaceMapping>(interfaces.Length);
            foreach (var @interface in interfaces)
            {
                interfaceMaps.Add(type.GetInterfaceMap(@interface));
            }
            return interfaceMaps;
        }

        private static Stack<Tuple> GetPropertyInfos(Type type)
        {
            var res = new Stack<Tuple>();

            var propertyInfos = type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | 
                                        BindingFlags.Public | 
                                        BindingFlags.Static | BindingFlags.Instance);

            foreach (var propertyInfo in Verify(GetInterfaceMappings(type), propertyInfos))
            {
                res.Push(propertyInfo);
            }

            /* This leads to duplicate entries
             
            // Add all those properties in base classes, but object!
            if (type.BaseType != typeof (object))
            {
                var basePi = GetPropertyInfos(type.BaseType);
                foreach (var propertyInfo in basePi)
                {
                    res.Push(propertyInfo);
                }
            }
             */

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