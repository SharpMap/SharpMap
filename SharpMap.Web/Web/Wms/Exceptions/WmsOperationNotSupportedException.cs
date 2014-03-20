namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsOperationNotSupportedException : WmsExceptionBase
    {
        public WmsOperationNotSupportedException(string message) :
            base(message, WmsExceptionCode.OperationNotSupported) { }
    }
}
