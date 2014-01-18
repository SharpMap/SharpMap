using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMap.Web.Wms.Exceptions
{
    /// <summary>
    /// WMS Exception codes
    /// </summary>
    public enum WmsExceptionCode
    {
        /// <summary>
        /// No error code
        /// </summary>
        NotApplicable,
        /// <summary>
        /// Request contains a Format not offered by the server.
        /// </summary>
        InvalidFormat,
        /// <summary>
        /// Request contains a CRS not offered by the server for one or more of the
        /// Layers in the request.
        /// </summary>
        InvalidCRS,
        /// <summary>
        /// GetMap request is for a Layer not offered by the server, or GetFeatureInfo
        /// request is for a Layer not shown on the map.
        /// </summary>
        LayerNotDefined,
        /// <summary>
        /// Request is for a Layer in a Style not offered by the server.
        /// </summary>
        StyleNotDefined,
        /// <summary>
        /// GetFeatureInfo request is applied to a Layer which is not declared queryable.
        /// </summary>
        LayerNotQueryable,
        /// <summary>
        /// GetFeatureInfo request contains invalid X or Y value.
        /// </summary>
        InvalidPoint,
        /// <summary>
        /// Value of (optional) UpdateSequence parameter in GetCapabilities request is
        /// equal to current value of service metadata update sequence number.
        /// </summary>
        CurrentUpdateSequence,
        /// <summary>
        /// Value of (optional) UpdateSequence parameter in GetCapabilities request is
        /// greater than current value of service metadata update sequence number.
        /// </summary>
        InvalidUpdateSequence,
        /// <summary>
        /// Request does not include a sample dimension value, and the server did not
        /// declare a default value for that dimension.
        /// </summary>
        MissingDimensionValue,
        /// <summary>
        /// Request contains an invalid sample dimension value.
        /// </summary>
        InvalidDimensionValue,
        /// <summary>
        /// Request is for an optional operation that is not supported by the server.
        /// </summary>
        OperationNotSupported
    }
}
