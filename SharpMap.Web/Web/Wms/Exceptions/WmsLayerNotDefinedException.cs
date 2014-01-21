using System;

namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsLayerNotDefinedException : WmsExceptionBase
    {
        public WmsLayerNotDefinedException(string layerName) :
            base(String.Format("Unknown Layer : {0}", layerName), WmsExceptionCode.LayerNotDefined) { }
    }
}
