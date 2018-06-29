// Copyright 2006 - Morten Nielsen (www.iter.dk)
//
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
using System.Runtime.Serialization;
using SharpMap.Data;
using SharpMap.Styles;

namespace SharpMap.Rendering.Thematics
{
    /// <summary>
    /// The CustomTheme class is used for defining your own thematic rendering by using a custom get-style-delegate.
    /// </summary>
    [Serializable]
    public class CustomTheme : IThemeEx, ISerializable, ICloneable
    {
        #region Delegates

        /// <summary>
        /// Custom Style Delegate method
        /// </summary>
        /// <remarks>
        /// The GetStyle delegate is used for determining the style of a feature using the <see cref="StyleDelegate"/> property.
        /// The method must take a <see cref="SharpMap.Data.FeatureDataRow"/> and return an <see cref="SharpMap.Styles.IStyle"/>.
        /// If the method returns null, the default style will be used for rendering.
        /// <para>
        /// <example>
        /// The following example can used for highlighting all features where the attribute "NAME" starts with "S".
        /// <code lang="C#">
        /// SharpMap.Rendering.Thematics.CustomTheme iTheme = new SharpMap.Rendering.Thematics.CustomTheme(GetCustomStyle);
        /// SharpMap.Styles.VectorStyle defaultstyle = new SharpMap.Styles.VectorStyle(); //Create default renderstyle
        /// defaultstyle.Fill = Brushes.Gray;
        /// iTheme.DefaultStyle = defaultstyle;
        /// 
        /// [...]
        /// 
        /// //Set up delegate for determining fill-color.
        /// //Delegate will fill all objects with a yellow color where the attribute "NAME" starts with "S".
        /// private static SharpMap.Styles.VectorStyle GetCustomStyle(SharpMap.Data.FeatureDataRow row)
        /// {
        /// 
        /// 	if (row["NAME"].ToString().StartsWith("S"))
        /// 	{
        /// 		SharpMap.Styles.VectorStyle style = new SharpMap.Styles.VectorStyle();
        /// 		style.Fill = Brushes.Yellow;
        /// 		return style;
        /// 	}
        /// 	else
        /// 		return null; //Return null which will render the default style
        /// }
        /// </code>
        /// </example>
        /// </para>
        /// </remarks>
        /// <param name="dr">Feature</param>
        /// <returns>Style to be applied to feature</returns>
        public delegate IStyle GetStyleMethod(FeatureDataRow dr);

        #endregion

        private IStyle _defaultStyle;

        private GetStyleMethod _getStyleDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTheme"/> class
        /// </summary>
        /// <param name="getStyleMethod"></param>
        public CustomTheme(GetStyleMethod getStyleMethod)
        {
            _getStyleDelegate = getStyleMethod;
        }
             
        /// <summary>
        /// Gets or sets the default style when an attribute isn't found in any bucket
        /// </summary>
        public IStyle DefaultStyle
        {
            get { return _defaultStyle; }
            set { _defaultStyle = value; }
        }

        /// <summary>
        /// Gets or sets the style delegate used for determining the style of a feature
        /// If map zoom/scale are also required by delegate then refer to <see cref="IThemeEx"/> and <see cref="GetStyleDelegateEx"/> property
        /// </summary>
        /// <remarks>
        /// The delegate must take a <see cref="SharpMap.Data.FeatureDataRow"/> and return an <see cref="SharpMap.Styles.IStyle"/>.
        /// If the method returns null, the default style will be used for rendering (note - default style may be set to null if required).
        /// <example>
        /// The example below creates a delegate that can be used for assigning the rendering of a road theme. If the road-class
        /// is larger than '3', it will be rendered using a thick red line.
        /// <code lang="C#">
        /// private static SharpMap.Styles.VectorStyle GetRoadStyle(SharpMap.Data.FeatureDataRow row)
        /// {
        ///		SharpMap.Styles.VectorStyle style = new SharpMap.Styles.VectorStyle();
        ///		if(((int)row["RoadClass"])>3)
        ///			style.Line = new Pen(Color.Red,5f);
        ///		else
        ///			style.Line = new Pen(Color.Black,1f);
        ///		return style;
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="GetStyleMethod"/>
        public GetStyleMethod StyleDelegate
        {
            get { return _getStyleDelegate; }
            set { _getStyleDelegate = value; }
        }

        #region IThemeEx Members

        /// <summary>
        /// Returns the <see cref="SharpMap.Styles.Style">style</see> based on an attribute value
        /// </summary>
        /// <param name="feature">DataRow</param>
        /// <returns>Style generated by GetStyle-Delegate</returns>
        public IStyle GetStyle(FeatureDataRow feature)
        {
            IStyle style = _getStyleDelegate(feature);
            if (style != null)
                return style;
            else
                return _defaultStyle;
        }

        /// <summary>
        /// Returns the <see cref="SharpMap.Styles.Style">style</see> based on an mapViewPort scale or zoom and/or attribute value(s)
        /// </summary>
        /// <param name="mapViewPort">MapViewport</param>
        /// <param name="feature">DataRow</param>
        /// <returns>Style generated by GetStyle-Delegate</returns>
        public IStyle GetStyle(MapViewport mapViewPort, FeatureDataRow feature)
        {
            //return GetStyleDelegateEx?Invoke(mapViewPort, feature) ?? GetStyle(feature);
            if (GetStyleDelegateEx != null)
                return GetStyleDelegateEx.Invoke(mapViewPort, feature);
            else
                return GetStyle(feature);
        }

        #endregion

        /// <summary>
        /// Custom Style Delegate method providing access to current zoom/scale via MapViewport. GetStyleDelegateEx takes priority over GetStyle
        /// </summary>
        /// <remarks>
        /// GetStyleDelegateEx is used for determining the style of a feature.
        /// Provide a lambda-style reference to public function in your implementation of CustomTheme 
        /// <para>
        /// <example>
        /// <code lang="C#">
        /// Func &lt; MapViewport, FeatureDataRow, IStyle> f = (mvp, fdr) => MyPublicFunction(mvp, fdr);
        /// GetStyleDelegateEx = f;
        /// </code>
        /// in which signature of MyPublicFunction is:
        /// <code lang="C#">
        /// public SharpMap.Styles.VectorStyle MyPublicFunction(MapViewport mapViewport, FeatureDataRow feature)
        /// </code>
        /// </example>
        /// </para>
        /// </remarks>
        /// <returns>Style to be applied to feature</returns>
        public Func<MapViewport, FeatureDataRow, IStyle> GetStyleDelegateEx { get; set; }

        #region ISerializable Members
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("defaultStyle", _defaultStyle);
        }

        #endregion

        public object Clone()
        {
            var res = (CustomTheme)MemberwiseClone();
            res._defaultStyle = _defaultStyle is ICloneable
                ? (IStyle)((ICloneable)_defaultStyle).Clone()
                : _defaultStyle;

            return res;
        }
    }
}
