
namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsArgumentException : WmsExceptionBase
    {
        public WmsArgumentException(string Message)
            : base(Message,WmsExceptionCode.NotApplicable)
        { }
    }
}
