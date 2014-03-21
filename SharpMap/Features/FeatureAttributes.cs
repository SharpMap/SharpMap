using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using GeoAPI.Features;

namespace SharpMap.Features
{
    /// <summary>
    /// Sample impementation of a feature attribute collection
    /// </summary>
    [Serializable]
    public class FeatureAttributes<T> : IFeatureAttributes where T : IComparable<T>, IEquatable<T>
    {
        private readonly Feature<T> _feature;
        private readonly object[]  _attributes;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="feature">The feature this attribute collection belongs to</param>
        internal FeatureAttributes(Feature<T> feature)
        {
            if (feature== null)
                throw new ArgumentNullException("feature");

            _feature = feature;
            _attributes = new object[feature.Factory.AttributesDefinition.Count];
            InitializeAttributes();
        }

        private void InitializeAttributes()
        {
            _attributes[0] = _feature.Oid;
            for (var i = 1; i < _attributes.Length; i++)
            {
                var attDef = _feature.Factory.AttributesDefinition[i];
                _attributes[i] = attDef.Default != null ? attDef.Default :  CreateType(attDef.AttributeType);
            }

        }

        private static object CreateType(System.Type type) 
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return false;
                case TypeCode.Byte:
                    return (byte) 0;
                case TypeCode.Char:
                    return new char();
                case TypeCode.DBNull:
                    return null;
                case TypeCode.DateTime:
                    return new DateTime();
                case TypeCode.Decimal:
                    return new decimal();
                case TypeCode.Double:
                    return 0d;
                case TypeCode.Int16:
                    return new short();
                case TypeCode.Int32:
                    return new int();
                case TypeCode.Int64:
                    return new long();
                case TypeCode.Object:
                    return null;
                case TypeCode.SByte:
                    return new sbyte();
                case TypeCode.Single:
                    return 0f;
                case TypeCode.String:
                    return null;
                case TypeCode.UInt16:
                    return new ushort();
                case TypeCode.UInt32:
                    return new uint();
                case TypeCode.UInt64:
                    return new ulong();
            }
            return null;
        }

        /// <summary>
        /// Creates an instance of this class, using the values provided in the <paramref name="attributes"/> collection
        /// </summary>
        /// <param name="attributes">A reference collection</param>
        private FeatureAttributes(FeatureAttributes<T> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");

            _feature = attributes._feature;
            _attributes = new object[_feature.Factory.AttributesDefinition.Count];
            for (var i = 0; i < _attributes.Length; i++)
            {
                var value = attributes._attributes[i];
                if (value is ICloneable)
                {
                    value = ((ICloneable) value).Clone();
                }
                _attributes[i] =  value;
            }
        }

        object IFeatureAttributes.this[int index]
        {
            get { return _attributes[index]; }
            set
            {
                Exception exception;
                var fdad = (FeatureAttributeDefinition)_feature.Factory.AttributesDefinition[index];
                if (!fdad.VerifyValue(value, out exception))
                {
                    throw exception;
                }
                _attributes[index] = value;
                _feature.OnPropertyChanged(_feature.GetFieldName(index));
            }
        }

        //TODO evaluate if it is not sufficient to have a KeyNotFound exception inside GetOrdinal
        object IFeatureAttributes.this[string key]
        {
            get
            {
                int ordinal = _feature.GetOrdinal(key);
                if (ordinal >= 0)
                    return _attributes[ordinal];
                throw new ArgumentOutOfRangeException("Unknown attributename: " + key);
            }
            set
            {
                int ordinal = _feature.GetOrdinal(key);
                if (ordinal >= 0)
                    _attributes[ordinal] = value;
                else
                    throw new ArgumentOutOfRangeException("Unknown attributename: " + key);
            }
        }

        public object[] GetValues()
        {
            var res = new object[_attributes.Length];
            Array.Copy(_attributes, res, _attributes.Length);
            return res;
        }

        public object Clone()
        {
            return new FeatureAttributes<T>(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder("FeatureAttributes { ");
            for (var i = 0 ; i < _attributes.Length; i++)
            {
                if (i > 0) sb.Append(";");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}:={1}", _feature.GetFieldName(i), _attributes[i]);
            }
            sb.Append(" }");
            return sb.ToString();
        }
    }
}