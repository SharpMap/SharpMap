
namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsStyleNotDefinedException : WmsExceptionBase
    {
        public WmsStyleNotDefinedException(string message)
            : base( message, WmsExceptionCode.StyleNotDefined)
        { }
    }
}
