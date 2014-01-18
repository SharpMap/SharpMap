using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMap.Web.Wms.Exceptions
{
   public class WmsInvalidCRSException:WmsExceptionBase
    {
       public WmsInvalidCRSException(string message)
            : base(message,WmsExceptionCode.InvalidCRS)
        { }
    }
}
