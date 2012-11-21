// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using GeoAPI.Geometries;

namespace SharpMap.Utilities.Wfs
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Text resources interface
    /// </summary>
    public interface IWFS_TextResources
    {
        /// <summary>
        /// Gets a value indicating the feature type prefix
        /// </summary>
        string NSFEATURETYPEPREFIX { get; }
        /// <summary>
        /// Gets a value indicating the GML namespace URI
        /// </summary>
        string NSGML { get; }
        /// <summary>
        /// Gets a value indicating the prefix of the GML namespace
        /// </summary>
        string NSGMLPREFIX { get; }
        /// <summary>
        /// Gets a value indicating the the OGC namespace URI
        /// </summary>
        string NSOGC { get; }
        /// <summary>
        /// Gets a value indicating the prefix of the OGC namespace
        /// </summary>
        string NSOGCPREFIX { get; }
        /// <summary>
        /// Gets a value indicating the OWS namespace URI
        /// </summary>
        string NSOWS { get; }
        /// <summary>
        /// Gets a value indicating the prefix of the OWS namespace
        /// </summary>
        string NSOWSPREFIX { get; }
        /// <summary>
        /// Gets a value indicating the XML namespace URI
        /// </summary>
        string NSSCHEMA { get; }
        /// <summary>
        /// Gets a value indicating the prefix of the XML namespace
        /// </summary>
        string NSSCHEMAPREFIX { get; }
        /// <summary>
        /// Gets a value indicating the WFS namespace URI
        /// </summary>
        string NSWFS { get; }
        /// <summary>
        /// Gets a value indicating the prefix of the WFS namespace
        /// </summary>
        string NSWFSPREFIX { get; }
        /// <summary>
        /// Gets a value indicating the XLink namespace URI
        /// </summary>
        string NSXLINK { get; }
        /// <summary>
        /// Gets a value indicating the prefix of the XLink namespace
        /// </summary>
        string NSXLINKPREFIX { get; }

        /// <summary>
        /// Gets an XPath string addressing the bounding box of a featuretype in 'GetCapabilities'.
        /// </summary>
        string XPATH_BBOX { get; }
        /// <summary>
        /// Gets an XPath string addressing the upper corner of a featuretype's bounding box in 'GetCapabilities'
        /// for extracting 'maxx'.
        /// </summary>
        string XPATH_BOUNDINGBOXMAXX { get; }
        /// <summary>
        /// Gets an XPath string addressing the upper corner of a featuretype's bounding box in 'GetCapabilities'
        /// for extracting 'maxy'.
        /// </summary>
        string XPATH_BOUNDINGBOXMAXY { get; }
        /// <summary>
        /// Gets an XPath string addressing the lower corner of a featuretype's bounding box in 'GetCapabilities'
        /// for extracting 'minx'.
        /// </summary>
        string XPATH_BOUNDINGBOXMINX { get; }
        /// <summary>
        /// Gets an XPath string addressing the lower corner of a featuretype's bounding box in 'GetCapabilities'
        /// for extracting 'miny'.
        /// </summary>
        string XPATH_BOUNDINGBOXMINY { get; }

        /// <summary>
        /// Gets an XPath string addressing the URI of 'DescribeFeatureType'in 'GetCapabilities'.
        /// </summary>
        string XPATH_DESCRIBEFEATURETYPERESOURCE { get; }
        /// <summary>
        /// Gets an XPath string addressing the name of an element described by an anonymous complex type 
        /// hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
        /// Step2Alt: Finding the name of the featuretype's element with an anonymous complex type hosting the GML geometry.
        /// </summary>
        string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE { get; }

        /// <summary>
        /// Gets an XPath string addressing the name of an element having a type-attribute referencing 
        /// a complex type hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
        /// Step2: Finding the name of the featuretype's element with a named complex type hosting the GML geometry.
        /// </summary>
        string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY { get; }

        /// <summary>
        /// Gets an XPath string addressing the 'gml'-prefixed  ref-attribute of an element.
        /// This is for querying the name of the GML geometry element.
        /// </summary>
        string XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY { get; }
        /// <summary>
        /// Gets an XPath string addressing an element with a 'gml'-prefixed type-attribute in 'DescribeFeatureType'.
        /// This for querying the geometry element of a featuretype in the most simple manner.
        /// </summary>
        string XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY { get; }

        /// <summary>
        /// Gets an XPath string addressing a complex type hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
        /// This for querying the geometry element of a featuretype. 
        /// Step1: Finding the complex type with a geometry element from GML specification. 
        /// </summary>
        string XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY { get; }

        /// <summary>
        /// Gets an XPath string addressing the URI of 'GetFeature'in 'GetCapabilities'.
        /// </summary>
        string XPATH_GETFEATURERESOURCE { get; }

        /// <summary>
        /// Gets an XPath string addressing a name-attribute.
        /// </summary>
        string XPATH_NAMEATTRIBUTEQUERY { get; }
        /// <summary>
        /// Gets an XPath string addressing the SRID of a featuretype in 'GetCapabilities'.
        /// </summary>

        string XPATH_SRS { get; }
        /// <summary>
        /// Gets an XPath string addressing the target namespace in 'DescribeFeatureType'.
        /// </summary>

        string XPATH_TARGETNS { get; }
        /// <summary>
        ///  Gets an XPath string addressing a type-attribute.
        /// </summary>
        string XPATH_TYPEATTRIBUTEQUERY { get; }

        /// <summary>
        /// Method to return the query string for a 'DescribeFeatureType' request.
        /// </summary>
        /// <param name="featureTypeName">The name of the featuretype to query</param>
        /// <returns>An URI string for a 'DescribeFeatureType' request</returns>
        string DescribeFeatureTypeRequest(string featureTypeName);

        /// <summary>
        /// Method to return the query string for a 'GetCapabilities' request.
        /// </summary>
        /// <returns>An URI string for a 'GetCapabilities' request</returns>
        string GetCapabilitiesRequest();

        /// <summary>
        /// Method to return the query string for a 'GetFeature' - <b>GET</b> request.
        /// </summary>
        /// <param name="featureTypeInfo">A <see cref="Wfs.WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="boundingBox">The bounding box of the query</param>
        /// <param name="filter">An instance implementing <see cref="IFilter"/></param>
        /// <returns>An URI for a 'GetFeature' - <b>GET</b> request.</returns>
        string GetFeatureGETRequest(WfsFeatureTypeInfo featureTypeInfo, Envelope boundingBox, IFilter filter);

        /// <summary>
        /// Method to return the query string for a 'GetFeature - <b>POST</b> request'.
        /// </summary>
        /// <param name="featureTypeInfo">A <see cref="Wfs.WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelProperty">A property necessary for label rendering</param>
        /// <param name="boundingBox">The bounding box of the query</param>
        /// <param name="filter">An instance implementing <see cref="IFilter"/></param>
        /// <returns>An URI for a 'GetFeature' - <b>POST</b> request.</returns>
        byte[] GetFeaturePOSTRequest(WfsFeatureTypeInfo featureTypeInfo, string labelProperty, Envelope boundingBox,
                                     IFilter filter);
    }
}