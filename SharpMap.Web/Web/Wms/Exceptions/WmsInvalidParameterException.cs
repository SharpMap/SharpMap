using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsInvalidParameterException : WmsExceptionBase
    {
        public WmsInvalidParameterException(string Message)
            : this(Message,WmsExceptionCode.NotApplicable)
        { }
        public WmsInvalidParameterException(string Message,WmsExceptionCode ExceptionCode)
            : base(Message,ExceptionCode)
        { }
    }
}
