using System;

namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsLayerNotQueryableException : WmsExceptionBase
    {
        public WmsLayerNotQueryableException(string layerName) :
            base(String.Format("Layer not queryable: {0}", layerName), WmsExceptionCode.LayerNotQueryable) { }
    }
}
