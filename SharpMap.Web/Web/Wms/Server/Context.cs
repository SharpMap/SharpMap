using System;
using System.Web;

namespace SharpMap.Web.Wms.Server
{
    public class Context : IContext
    {
        private readonly IContextRequest _request;
        private readonly IContextResponse _response;

        public Context(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context", "An attempt was made to access the WMS server outside a valid HttpContext");

            _request = new ContextRequest(context);
            _response = new ContextResponse(context);
        }

        public IContextRequest Request
        {
            get { return _request; }
        }

        public IContextResponse Response
        {
            get { return _response; }
        }
    }
}