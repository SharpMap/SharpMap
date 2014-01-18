using System;
using System.Collections.Specialized;
using System.Web;
using System.Xml;

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

            _request = new ContextRequest(context.Request);
            _response = new ContextResponse(context.Response);
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

    public class ContextRequest : IContextRequest
    {
        private readonly HttpRequest _request;

        public ContextRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            _request = request;
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
    }

    public class ContextResponse : IContextResponse
    {
        private readonly HttpResponse _response;

        public ContextResponse(HttpResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");
            _response = response;
        }

        public string ContentType
        {
            get { return _response.ContentType; }
            set { _response.ContentType = value; }
        }

        public string Charset
        {
            get { return _response.Charset; }
            set { _response.Charset = value; }
        }

        public bool BufferOutput
        {
            get { return _response.BufferOutput; }
            set { _response.BufferOutput = value; }
        }

        public void Clear()
        {
            _response.Clear();
        }

        public void Write(string s)
        {
            _response.Write(s);
        }

        public void Write(byte[] buffer)
        {
            _response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public XmlWriter CreateXmlWriter()
        {
            return XmlWriter.Create(_response.OutputStream);
        }

        public void End()
        {
            _response.Flush();
            _response.End();
        }
    }
}