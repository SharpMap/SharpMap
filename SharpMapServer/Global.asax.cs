using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Data.Entity;
using SharpMapServer.Model;
using SharpMap.Layers;
using System.IO;
using System.Reflection;
namespace SharpMapServer
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Database.SetInitializer<SharpMapContext>(new DropCreateDatabaseIfModelChanges<SharpMapContext>());
            using (var _ctx = new SharpMapContext())
            {
                var caps = _ctx.Capabilities.FirstOrDefault();
                if (caps == null)
                {
                    /*create default*/
                    caps = new WmsCapabilities
                    {
                        Title = "SharpMap Demo Server",
                        Abstract = "This is an example SharpMap server",
                    };
                    caps.Keywords = "SharpMap,WMS";
                    _ctx.Capabilities.Add(caps);

                }

                WMSServer.m_Capabilities = new SharpMap.Web.Wms.Capabilities.WmsServiceDescription
                {
                    Abstract = caps.Abstract,
                    AccessConstraints = caps.AccessConstraints,
                    Fees = caps.Fees,
                    Keywords = caps.Keywords.Split(','),
                    LayerLimit = caps.LayerLimit,
                    MaxHeight = caps.MaxHeight,
                    MaxWidth = caps.MaxWidth,
                    OnlineResource = caps.OnlineResource,
                    Title = caps.Title
                };

                if (_ctx.Users.Count() == 0)
                {
                    _ctx.Users.Add(new User { UserName = "admin", Password = "sharpmap" });
                }

                if (_ctx.Layers.Count() == 0)
                {
                    /*add default layer*/
                    _ctx.Layers.Add(new SharpMapServer.Model.WmsLayer() { Name = "States", Description = "Demo data over US States", Provider = "Shapefile", DataSource = "states.shp" });
                }

                WMSServer.m_Map = new SharpMap.Map();
                foreach (var l in _ctx.Layers)
                {
                    switch (l.Provider)
                    {
                        case "Shapefile":
                            VectorLayer lay = new VectorLayer(l.Name);
                            string ds = l.DataSource;
                            if (!Path.IsPathRooted(ds))
                                ds = Server.MapPath(ds);

                            lay.DataSource = new SharpMap.Data.Providers.ShapeFile(ds);
                            WMSServer.m_Map.Layers.Add(lay);
                            break;
                    }
                }

                _ctx.SaveChanges();
            }

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}