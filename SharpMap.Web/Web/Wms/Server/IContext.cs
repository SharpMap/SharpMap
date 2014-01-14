using System;
using System.Collections.Specialized;
using System.Xml;

namespace SharpMap.Web.Wms.Server
{
    public interface IContext
    {
        // request
        Uri Url { get; }
        NameValueCollection Params { get; }

        // response
        string ContentType { get; set; }
        string Charset { get; set; }
        bool BufferOutput { get; set; }

        void Clear();
        void End();

        void Write(string s);
        void Write(byte[] buffer);
        
        XmlWriter CreateXmlWriter();
    }
}
