using System;

namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsParameterNotSpecifiedException : WmsExceptionBase
    {
        public WmsParameterNotSpecifiedException(string parameter) :
            base(String.Format("Required parameter {0} not specified", parameter)) { }
    }
}
