namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsArgumentException : WmsExceptionBase
    {
        public WmsArgumentException(string message) :
            base(message, WmsExceptionCode.NotApplicable) { }
    }
}
