
namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsNotApplicableException : WmsExceptionBase
    {
        public WmsNotApplicableException(string Message)
            : base(Message, WmsExceptionCode.NotApplicable)
        { }
    }
}
