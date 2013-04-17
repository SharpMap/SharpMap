using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Features;

namespace SharpMap.Features
{

    /// <summary>
    /// Sample implementation of IFeatureAttributeDefinition
    /// </summary>
    public class FeatureAttributeDefinition : IFeatureAttributeDefinition
    {

        public string AttributeName {get;set;}

        public string AttributeDescription { get; set; }

        public Type AttributeType { get; set; }

        public bool IsNullable { get; set; }
    }
}
