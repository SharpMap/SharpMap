namespace SharpMap.Demo.Wms.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Web;

    using Web.Wms;

    public class StdWmsMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(HttpContext context)
        {
            try
            {
                HttpRequest request = context.Request;
                string url = GetFixedUrl(request);
                Capabilities.WmsServiceDescription description = GetDescription(url);
                Map map = this.GetMap(request);
                WmsServer.ParseQueryString(map, description);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }
    }
}