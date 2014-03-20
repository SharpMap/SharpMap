namespace SharpMap.Web.Wms.Exceptions
{
    class WmsArgumentNullException : WmsExceptionBase
    {
        public WmsArgumentNullException(string message) :
            base(message, WmsExceptionCode.NotApplicable) { }
    }
}
