using System;
using System.Diagnostics;
using SharpMap.Web.Wms;
using SharpMap.Web.Wms.Server;

namespace SharpMap.Demo.Wms.Handlers
{
    public class StdWmsMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(IContext context)
        {
            try
            {
                string url = GetFixedUrl(context);
                Capabilities.WmsServiceDescription description = GetDescription(url);
                Map map = GetMap(context);
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