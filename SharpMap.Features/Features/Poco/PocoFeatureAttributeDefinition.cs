using System;
using System.Reflection;
using GeoAPI.Features;

namespace SharpMap.Features.Poco
{
    public class PocoFeatureAttributeDefinition : IFeatureAttributeDefinition
    {
        private readonly bool _static;
        private bool _readonly;

        public PocoFeatureAttributeDefinition(PropertyInfo propertyInfo)
        {
            AttributeName = propertyInfo.Name;
            AttributeType = propertyInfo.PropertyType;
            if (propertyInfo.CanRead)
            {
                _getMethod = propertyInfo.GetGetMethod();
                _static = _getMethod.IsStatic;
            }
            if (propertyInfo.CanWrite)
            {
                _setMethod = propertyInfo.GetSetMethod();
            }
            else
            {
                _readonly = true;
            }
            
            var att = propertyInfo.GetCustomAttributes(typeof (FeatureAttributeAttribute), true);
            if (att.Length > 0) CorrectByAttribute((FeatureAttributeAttribute)att[0]);
        }

        public PocoFeatureAttributeDefinition(FieldInfo fieldInfo)
        {
            AttributeName = fieldInfo.Name;
            AttributeType = fieldInfo.FieldType;
            _static = fieldInfo.IsStatic;

            _field = fieldInfo;
            _readonly = fieldInfo.IsInitOnly;

            var att = fieldInfo.GetCustomAttributes(typeof(FeatureAttributeAttribute), true);
            if (att.Length > 0) CorrectByAttribute((FeatureAttributeAttribute)att[0]);
        }

        public void CorrectByAttribute(FeatureAttributeAttribute attribute)
        {
            if (attribute.Ignore)
            {
                Ignore = true;
                return;
            }

            if (string.IsNullOrEmpty(attribute.AttributeName))
                AttributeName = attribute.AttributeName;
            if (string.IsNullOrEmpty(attribute.AttributeName))
                AttributeName = attribute.AttributeName;

            _readonly |= attribute.Readonly;
            Ignore = attribute.Ignore;
        }


        public string AttributeName { get; set; }
        public string AttributeDescription { get; set; }
        public Type AttributeType { get; set; }
        public bool IsNullable { get; set; }

        public bool Ignore { get; private set; }

        private readonly MethodInfo _setMethod;
        private readonly MethodInfo _getMethod;
        private readonly FieldInfo _field;

        public void SetValue(object instance, object value)
        {
            if (_readonly)
            {
                return;
            }

            if (!IsNullable && ReferenceEquals(value, null))
            {
                return;
            }

            if (_field != null)
            {
                if (!_field.IsInitOnly)
                    _field.SetValue(_field.IsStatic ? null : instance, value);
                return;
            }

            if (_setMethod != null)
            {
                _setMethod.Invoke(_static ? null : instance, new[] {value});
            }
        }

        public object GetValue(object instance)
        {
            if (_field != null)
            {
                return _field.GetValue(_static ? null : instance);
            }

            if (_getMethod != null)
            {
                return _getMethod.Invoke(_static ? null : instance, null);
            }
            return null;
        }
    }
}