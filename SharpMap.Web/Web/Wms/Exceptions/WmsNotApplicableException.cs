namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsNotApplicableException : WmsExceptionBase
    {
        public WmsNotApplicableException(string message) :
            base(message, WmsExceptionCode.NotApplicable) { }
    }
}
