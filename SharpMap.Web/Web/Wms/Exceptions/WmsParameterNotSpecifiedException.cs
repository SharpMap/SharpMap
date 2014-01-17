using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMap.Web.Wms.Exceptions
{
    public class WmsParameterNotSpecifiedException : WmsExceptionBase
    {
        public WmsParameterNotSpecifiedException(string Parameter)
            : base("Required parameter " + Parameter + " not specified")
        { }
    }
}
