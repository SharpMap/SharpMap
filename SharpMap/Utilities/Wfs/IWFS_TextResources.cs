// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// Text resources interface
    /// </summary>
    public interface IWFS_TextResources
    {
        string DescribeFeatureTypeRequest(string featureTypeName);
        string GetCapabilitiesRequest();
        string GetFeatureGETRequest(SharpMap.Utilities.Wfs.WfsFeatureTypeInfo featureTypeInfo, SharpMap.Geometries.BoundingBox boundingBox, IFilter filter);
        byte[] GetFeaturePOSTRequest(SharpMap.Utilities.Wfs.WfsFeatureTypeInfo featureTypeInfo, string labelProperty, SharpMap.Geometries.BoundingBox boundingBox, IFilter filter);
        string NSFEATURETYPEPREFIX { get; }
        string NSGML { get; }
        string NSGMLPREFIX { get; }
        string NSOGC { get; }
        string NSOGCPREFIX { get; }
        string NSOWS { get; }
        string NSOWSPREFIX { get; }
        string NSSCHEMA { get; }
        string NSSCHEMAPREFIX { get; }
        string NSWFS { get; }
        string NSWFSPREFIX { get; }
        string NSXLINK { get; }
        string NSXLINKPREFIX { get; }
        string XPATH_BBOX { get; }
        string XPATH_BOUNDINGBOXMAXX { get; }
        string XPATH_BOUNDINGBOXMAXY { get; }
        string XPATH_BOUNDINGBOXMINX { get; }
        string XPATH_BOUNDINGBOXMINY { get; }
        string XPATH_DESCRIBEFEATURETYPERESOURCE { get; }
        string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE { get; }
        string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY { get; }
        string XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY { get; }
        string XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY { get; }
        string XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY { get; }
        string XPATH_GETFEATURERESOURCE { get; }
        string XPATH_NAMEATTRIBUTEQUERY { get; }
        string XPATH_SRS { get; }
        string XPATH_TARGETNS { get; }
        string XPATH_TYPEATTRIBUTEQUERY { get; }
    }
}
