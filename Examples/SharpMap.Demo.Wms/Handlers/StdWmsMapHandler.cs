using System;
using System.Diagnostics;
using System.Web;
using SharpMap.Web.Wms;

namespace SharpMap.Demo.Wms.Handlers
{
    public class StdWmsMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(HttpContext context)
        {
            try
            {
                HttpRequest request = context.Request;
                string url = GetFixedUrl(request);
                Capabilities.WmsServiceDescription description = GetDescription(url);
                Map map = GetMap(request);
                WmsServer.ParseQueryString(map, description, 10, null, context);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }
    }
}