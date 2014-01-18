namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsInvalidParameterException : WmsExceptionBase
    {
        public WmsInvalidParameterException(string message) :
            this(message, WmsExceptionCode.NotApplicable) { }

        public WmsInvalidParameterException(string message, WmsExceptionCode code) :
            base(message, code) { }
    }
}
