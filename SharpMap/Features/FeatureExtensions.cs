using System;
using GeoAPI.Features;

namespace SharpMap.Features
{
    /// <summary>
    /// Extension methods for <see cref="IFeature"/>
    /// </summary>
    public static class FeatureExtensions
    {
        /// <summary>
        /// A utility method to evaluate if a feature attribute is <c>null</c>
        /// </summary>
        /// <param name="self">The feature</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <returns><c>true</c> if the attribute value is null</returns>
        public static bool IsNull(this IFeature self, string attributeName)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (string.IsNullOrEmpty(attributeName))
                throw new ArgumentNullException("attributeName");

            return self.Attributes[attributeName] == null;
        }
    }
}