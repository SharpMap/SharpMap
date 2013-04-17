using System;
using System.Collections.Generic;
using GeoAPI.Features;

namespace SharpMap.Features
{
    /// <summary>
    /// Sample impementation of a feature attribute collection
    /// </summary>
    [Serializable]
    public class FeatureAttributes : IFeatureAttributes
    {
        private readonly Dictionary<string, int> _index;
        private readonly object[]  _attributes;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="index">An index for key to index translation</param>
        public FeatureAttributes(Dictionary<string, int> index)
        {
            if (index == null || index.Count == 0)
                throw new ArgumentNullException("index");

            _index = index;
            _attributes = new object[_index.Count];
        }

        /// <summary>
        /// Creates an instance of this class, using the values provided in the <paramref name="attributes"/> collection
        /// </summary>
        /// <param name="attributes">A reference collection</param>
        private FeatureAttributes(FeatureAttributes attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");

            _index = attributes._index;
            _attributes = new object[_index.Count];
            for (var i = 0; i < _index.Count; i++)
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
            set { _attributes[index] = value; }
        }

        object IFeatureAttributes.this[string key]
        {
            get { return _attributes[_index[key]]; }
            set { _attributes[_index[key]] = value; }
        }

        public object[] GetValues()
        {
            var res = new object[_attributes.Length];
            Array.Copy(_attributes, res, _attributes.Length);
            return res;
        }

        public object Clone()
        {
            return new FeatureAttributes(this);
        }

    }
}