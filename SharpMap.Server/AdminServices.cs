using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.Script.Serialization;
using System.Reflection;

namespace SharpMapServer
{
    public class AdminServices : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string operation = context.Request.QueryString["operation"];
            switch (operation)
            {
                case "status":
                    context.Response.ContentType = "application/json";
                    context.Response.Write(new JavaScriptSerializer().Serialize(new
                    {
                        Status = "OK",
                        Version = Assembly.GetExecutingAssembly().GetName().Name + " Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    }));
                    break;
                case "getwmslayers":
                    context.Response.ContentType = "application/json";
                    var res = new { layers = WMSServer.m_Map.Layers.Select(x => new { Name = x.LayerName }).ToArray() };
                    context.Response.Write(new JavaScriptSerializer().Serialize(new
                    {
                        Status = "OK",
                        layers = res.layers
                    }));
                    break;
                case "generalsettings":
                    context.Response.ContentType = "application/json";
                    context.Response.Write(new JavaScriptSerializer().Serialize(new
                    {
                        Status = "OK",
                        Title = WMSServer.m_Capabilities.Title,
                        Abstract = WMSServer.m_Capabilities.Abstract,
                        AccessConstraints = WMSServer.m_Capabilities.AccessConstraints,
                        ContactInformation = WMSServer.m_Capabilities.ContactInformation,
                        Fees = WMSServer.m_Capabilities.Fees,
                        KeyWords = string.Join(",",WMSServer.m_Capabilities.Keywords),
                        OnlineResource = WMSServer.m_Capabilities.OnlineResource,
                    }));
                    break;
                default:
                    context.Response.ContentType = "application/json";
                    context.Response.Write(new JavaScriptSerializer().Serialize(new { Status = "Err", Text = "Unknown operation" }));
                    break;
            }
        }
    }
}