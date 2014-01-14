using System;
using System.Collections.Specialized;
using System.Web;
using System.Xml;

namespace SharpMap.Web.Wms.Server
{
    public class Context : IContext
    {
        private readonly HttpContext _context;
       
        public Context(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context", "An attempt was made to access the WMS server outside a valid HttpContext");

            _context = context;           
        }

        public Uri Url
        {
            get { return _context.Request.Url; }
        }

        public NameValueCollection Params
        {
            get { return _context.Request.Params; }
        }

        public string ContentType
        {
            get { return _context.Response.ContentType; }
            set { _context.Response.ContentType = value; }
        }

        public string Charset
        {
            get { return _context.Response.Charset; }
            set { _context.Response.Charset = value; }
        }

        public bool BufferOutput
        {
            get { return _context.Response.BufferOutput; }
            set { _context.Response.BufferOutput = value; }
        }

        public void Clear()
        {
            _context.Response.Clear();
        }

        public void Write(string s)
        {
            _context.Response.Write(s);
        }

        public void Write(byte[] buffer)
        {
            _context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public XmlWriter CreateXmlWriter()
        {
            return XmlWriter.Create(_context.Response.OutputStream);
        }

        public void End()
        {
            _context.Response.Flush();
            _context.Response.End();
        }
    }
}