using System;
using System.Collections.Specialized;
using System.Web;

namespace SharpMap.Web.Wms.Server
{
    public class ContextRequest : IContextRequest
    {
        private readonly HttpRequest _request;
        private readonly HttpServerUtility _server;

        public ContextRequest(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            _request = context.Request;
            _server = context.Server;
        }

        public Uri Url
        {
            get { return _request.Url; }
        }

        public NameValueCollection Params
        {
            get { return _request.Params; }
        }

        public string GetParam(string key)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            return Params[key];
        }

        public string Encode(string s)
        {
            if (String.IsNullOrEmpty(s)) 
                throw new ArgumentNullException("s");
            return _server.HtmlEncode(s);
        }
    }
}