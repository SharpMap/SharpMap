using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeoAPI.Features
{
    /// <summary>
    /// Interface for providing information about an attribute
    /// </summary>
    public interface IFeatureAttributeDefinition
    {
        /// <summary>
        /// The name of the attribute
        /// </summary>
        string AttributeName { get; set; }

        /// <summary>
        /// Description of the attribute
        /// </summary>
        string AttributeDescription { get; set; }
        
        /// <summary>
        /// Type of value
        /// </summary>
        Type AttributeType { get; set; }
        
        /// <summary>
        /// True if this attribute can be null
        /// </summary>
        bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the default value
        /// </summary>
        object Default { get; set; }
    }
}
