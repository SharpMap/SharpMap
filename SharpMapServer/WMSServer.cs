using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharpMapServer
{
    public class WMSServer : IHttpHandler
    {
        internal static SharpMap.Map m_Map;
        internal static SharpMap.Web.Wms.Capabilities.WmsServiceDescription m_Capabilities;
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            SharpMap.Web.Wms.WmsServer.ParseQueryString(m_Map, m_Capabilities);
        }
    }
}