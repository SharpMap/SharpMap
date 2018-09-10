using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using SharpMapServer.Model;
using SharpMap.Layers;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using NetTopologySuite;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace SharpMapServer
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var gss = new NtsGeometryServices();
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new CoordinateSystemFactory(),
                new CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

            GeoAPI.GeometryServiceProvider.Instance = gss;
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);

            string settingsfile = Server.MapPath("~/App_Data/settings.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(SharpMapContext));
            if (!System.IO.File.Exists(settingsfile))
            {
                /*create default settings*/
                SharpMapContext ctx = new SharpMapContext();
                ctx.Capabilities = new WmsCapabilities()
                {
                    Title = "SharpMap Demo Server",
                    Abstract = "This is an example SharpMap server",
                    Keywords = "SharpMap,WMS"
                };

                ctx.Users = new List<User>();
                ctx.Users.Add(new User { UserName = "admin", Password = "sharpmap" });

                /*add default layer*/
                ctx.Layers = new List<SharpMapServer.Model.WmsLayer>();
                ctx.Layers.Add(new SharpMapServer.Model.WmsLayer() { Name = "States", Description = "Demo data over US States", Provider = "Shapefile", DataSource = "states.shp" });                
                FileStream fs = File.Create(settingsfile);
                serializer.Serialize(fs, ctx);
                fs.Close();
            }

            FileStream settingsStream = File.OpenRead(settingsfile);
            var settings = (SharpMapContext)serializer.Deserialize(settingsStream);
            settingsStream.Close();

            WMSServer.m_Capabilities = new SharpMap.Web.Wms.Capabilities.WmsServiceDescription
            {
                Abstract = settings.Capabilities.Abstract,
                AccessConstraints = settings.Capabilities.AccessConstraints,
                Fees = settings.Capabilities.Fees,
                Keywords = settings.Capabilities.Keywords.Split(','),
                LayerLimit = settings.Capabilities.LayerLimit,
                MaxHeight = settings.Capabilities.MaxHeight,
                MaxWidth = settings.Capabilities.MaxWidth,
                OnlineResource = settings.Capabilities.OnlineResource,
                Title = settings.Capabilities.Title
            };

            WMSServer.m_Map = new SharpMap.Map();
            foreach (var l in settings.Layers)
            {
                switch (l.Provider)
                {
                    case "Shapefile":
                        VectorLayer lay = new VectorLayer(l.Name);
                        string ds = l.DataSource;
                        if (!Path.IsPathRooted(ds))
                            ds = Server.MapPath(ds);

                        lay.DataSource = new SharpMap.Data.Providers.ShapeFile(ds);
                        lay.SRID = 4326;
                        WMSServer.m_Map.Layers.Add(lay);
                        break;
                }
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
