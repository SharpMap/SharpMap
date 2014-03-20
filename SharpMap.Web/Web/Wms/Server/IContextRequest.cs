using System;

namespace SharpMap.Web.Wms.Server
{
    public interface IContextRequest
    {
        Uri Url { get; }
        string GetParam(string key);
        string Encode(string s);
    }
}