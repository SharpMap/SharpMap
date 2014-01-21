using System;

namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsInvalidBboxException : WmsExceptionBase
    {
        public WmsInvalidBboxException(string bbox) :
            base(String.Format("Invalid parameter BBOX:{0}", bbox), WmsExceptionCode.InvalidDimensionValue) { }
    }
}