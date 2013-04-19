namespace GeoAPI.SpatialReference
{
    /// <summary>
    /// Interface for classes that can parse or convert to and from
    /// </summary>
    public interface ISpatialReferenceConverter
    {
        ISpatialReference FromWellKnownText(string spatialReference);
        ISpatialReference FromProj4(string proj4);
        ISpatialReference FromAuthorityCode(string authorityCode);

        string ToWellKnownText(ISpatialReference spatialReference);
        string ToProj4(ISpatialReference spatialReference);
        string ToAuthorityCode(ISpatialReference spatialReference);
    }
}