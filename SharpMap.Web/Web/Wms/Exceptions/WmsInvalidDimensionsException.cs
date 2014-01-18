namespace SharpMap.Web.Wms.Exceptions
{
    class WmsInvalidDimensionsException : WmsExceptionBase
    {
        public WmsInvalidDimensionsException(string message) :
            base(message, WmsExceptionCode.InvalidDimensionValue) { }
    }
}
