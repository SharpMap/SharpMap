using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMap.Web.Wms.Exceptions
{
    class WmsArgumentNullException:WmsExceptionBase
    {
        public WmsArgumentNullException(string Message)
            : base(Message,WmsExceptionCode.NotApplicable)
        { }
    }
}
