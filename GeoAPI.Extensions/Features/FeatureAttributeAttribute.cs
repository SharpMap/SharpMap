using System;

namespace GeoAPI.Features
{
    /// <summary>
    /// Attribute to be used for properties which need to be declared as Feature Attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    public class FeatureAttributeAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name as used for table columns
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name as used for table columns
        /// </summary>
        public string AttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets a value that this property of field is only for internal usage
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that, even though there may be setters or the field is not readonly, this value is not to be changed
        /// </summary>
        public bool Readonly { get; set; }
    }
}