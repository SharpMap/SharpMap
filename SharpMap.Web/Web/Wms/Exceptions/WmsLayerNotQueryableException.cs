
namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsLayerNotQueryableException:WmsExceptionBase
    {
        public WmsLayerNotQueryableException(string layerName)
            : base(layerName, WmsExceptionCode.LayerNotQueryable)
        { }
    }
}
