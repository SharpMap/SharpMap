using System;
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
        public FeatureAttributes(Feature<T> feature)
        {
            if (feature== null)
                throw new ArgumentNullException("feature");

            _feature = feature;
            _attributes = new object[feature.Factory.AttributesDefinition.Count];
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

    }
}