
namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsOperationNotSupportedException : WmsExceptionBase
    {
        public WmsOperationNotSupportedException(string Message)
            : base(Message, WmsExceptionCode.OperationNotSupported)
        { }
    }
}
