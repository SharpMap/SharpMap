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
    }
}
