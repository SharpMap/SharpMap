using System;
using System.Xml;

namespace SharpMap.Web.Wms.Server
{
    public interface IContext
    {
        IContextRequest Request { get; }
        IContextResponse Response { get; }
    }

    public interface IContextRequest
    {
        Uri Url { get; }
        string GetParam(string key);
    }

    public interface IContextResponse
    {
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
