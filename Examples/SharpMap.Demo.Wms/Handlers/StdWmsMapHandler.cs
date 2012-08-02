namespace SharpMap.Demo.Wms.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Web;

    using SharpMap.Web.Wms;

    public class StdWmsMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(HttpContext context)
        {
            try
            {
                HttpRequest request = context.Request;
                string url = this.GetFixedUrl(request);
                Capabilities.WmsServiceDescription description = this.GetDescription(url);
                Map map = this.GetMap(request);
                WmsServer.ParseQueryString(map, description, 10, null);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }
    }
}