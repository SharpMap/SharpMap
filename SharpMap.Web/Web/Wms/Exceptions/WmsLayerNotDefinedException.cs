
namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsLayerNotDefinedException:WmsExceptionBase
    {
        public WmsLayerNotDefinedException(string layerName)
            : base("Unknown Layer : " + layerName, WmsExceptionCode.LayerNotDefined)
        { }
    }
}
