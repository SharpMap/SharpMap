using System;
using System.Web;
using System.Xml;

namespace SharpMap.Web.Wms.Server
{
    public class ContextResponse : IContextResponse
    {
        private readonly HttpResponse _response;
        private readonly HttpServerUtility _server;

        public ContextResponse(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            _response = context.Response;
            _server = context.Server;
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

        public void End()
        {
            _response.Flush();
            _response.End();
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

        public string Decode(string s)
        {
            if (String.IsNullOrEmpty(s))
                throw new ArgumentNullException("s");
            return _server.HtmlDecode(s);
        }
    }
}