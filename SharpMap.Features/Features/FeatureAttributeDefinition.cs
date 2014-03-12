using System;
using GeoAPI.Features;

namespace SharpMap.Features
{
    /// <summary>
    /// Sample implementation of <see cref="IFeatureAttributeDefinition"/>
    /// </summary>
    public class FeatureAttributeDefinition : IFeatureAttributeDefinition
    {
        /// <summary>
        /// The name of the attribute
        /// </summary>
        public string AttributeName {get;set;}

        /// <summary>
        /// Description of the attribute
        /// </summary>
        public string AttributeDescription { get; set; }

        /// <summary>
        /// Type of value
        /// </summary>
        public Type AttributeType { get; set; }

        /// <summary>
        /// True if this attribute can be null
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the default value
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Method to verify input data
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="exception">The exception to throw</param>
        /// <returns><value>true</value> if this value is valid</returns>
        internal protected virtual bool VerifyValue(object value, out Exception exception)
        {
            if (value == null && !IsNullable)
            {
                exception = new ArgumentException("Attribute " + AttributeName + " is not nullable");
                return false;
            }

            if (value != null && !AttributeType.IsInstanceOfType(value))
            {
                exception = new ArgumentException("Wrong type for attribute " + AttributeName);
                return false;
            }

            exception = null;
            return true;
        }
    }
}
