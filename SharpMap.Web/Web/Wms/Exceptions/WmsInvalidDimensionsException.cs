using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMap.Web.Wms.Exceptions
{
    class WmsInvalidDimensionsException : WmsExceptionBase
    {
        public WmsInvalidDimensionsException(string message)
            : base(message, WmsExceptionCode.InvalidDimensionValue)
        { }
    }
}
