// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
using System;
using System.Collections.Generic;
using SharpMap.Styles;

namespace SharpMap.Rendering.Thematics
{
    /// <summary>
    /// UniqueValuesTheme is a theme each rendered feature is matched against at category that have a different style
    /// </summary>
    /// <typeparam name="T">Type of the featureattribute to match</typeparam>
    [Serializable]
    public class UniqueValuesTheme<T>  : ITheme
    {
        readonly IStyle _default;

        //Internally we use strings to compare everything since we don't know what we might get from the datasource...
        readonly Dictionary<string, IStyle> _styleMap;
        readonly string _attributeName;

        /// <summary>
        /// CategoriesTheme is a theme each rendered feature is matched against at category that have a different style
        /// </summary>
        /// <param name="attributeName">the featureattribute to categorize by</param>
        /// <param name="styleMap">the map of attributevalue to style</param>
        /// <param name="defaultStyle">the default style to map features that does not exist in the stylemap with</param>
        public UniqueValuesTheme(string attributeName, Dictionary<T, IStyle> styleMap, IStyle defaultStyle)
        {
            _attributeName = attributeName;
            _styleMap = new Dictionary<string, IStyle>();
            foreach (var kvp in styleMap)
                _styleMap.Add(kvp.Key.ToString(), kvp.Value);

            _default = defaultStyle;
        }

        /// <summary>
        /// Returns the style based on a feature
        /// </summary>
        /// <param name="attribute">Set of attribute values to calculate the <see cref="IStyle"/> from</param>
        /// <returns>The style</returns>
        public IStyle GetStyle(Data.FeatureDataRow attribute)
        {
            if (attribute.IsNull(_attributeName))
                return _default;

            var val = attribute[_attributeName].ToString();
            if (_styleMap.ContainsKey(val))
                return _styleMap[val];          

            return _default;
        }

        /// <summary>
        /// Gets the name of the attribute column
        /// </summary>
        public string AttributeName { get { return _attributeName; } }

        /// <summary>
        /// Gets the unique values
        /// </summary>
        public String[] UniqueValues
        {
            get
            {
                var res = new string[_styleMap.Count];
                _styleMap.Keys.CopyTo(res, 0);
                return res;
            }
        }

        /// <summary>
        /// Gets the default style, that is applied if <see cref="Attribute"/>
        /// </summary>
        public IStyle DefaultStyle { get { return _default; } }

        /// <summary>
        /// Function to retrieve the style for a given value
        /// </summary>
        /// <param name="value">The attribute value as string</param>
        /// <returns>The style</returns>
        public IStyle GetStyle(string value)
        {
            IStyle result;
            if (_styleMap.TryGetValue(value, out result))
                return result;
            return _default;
        }
    }
}
