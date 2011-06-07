namespace SharpMap.Demo.Wms
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class MvcApplication : HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            var defaults = new { controller = "Home", action = "Index", id = UrlParameter.Optional };
            routes.MapRoute("Default", "{controller}/{action}/{id}", defaults);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
        }
    }
}