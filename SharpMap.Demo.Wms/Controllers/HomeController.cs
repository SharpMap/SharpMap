namespace SharpMap.Demo.Wms.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.Mvc;

    using GeoAPI.Geometries;

    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NetTopologySuite.Simplify;

    using SharpMap.Converters.GeoJSON;
    using SharpMap.Demo.Wms.Models;

    public class HomeController : Controller
    {
        private List<DemoItem> GetDemoItems()
        {
            List<DemoItem> items = new List<DemoItem>();
            Type type = this.GetType();

            // how to retrieve all public instance methods? 
            // BindingFlags.Public | BindingFlags.DeclaredOnly returns an empty array...
            MethodInfo[] methods = type.GetMethods();
            foreach (MethodInfo method in methods)
            {                
                bool valid = method.ReturnType == typeof(ActionResult);
                if (!valid) 
                    continue;

                object[] attributes = method.GetCustomAttributes(typeof(ObsoleteAttribute), false);
                if (attributes.Length != 0)
                    continue;

                int i = type.Name.IndexOf("Controller", StringComparison.Ordinal);
                string url = String.Format("{0}/{1}", type.Name.Substring(0, i), method.Name);
                items.Add(new DemoItem(method.Name, url));
            }            
            return items;
        }
        
        [Obsolete]
        public ActionResult Index()
        {
            this.ViewData["DemoItems"] = this.GetDemoItems();
            return this.View();
        }        

        public ActionResult Openlayers()
        {
            return this.View();
        }

        public ActionResult Polymaps()
        {
            return this.View();
        }

        public ActionResult Leaflet()
        {
            return this.View();
        }

        public ActionResult Geojson()
        {
            return this.View();
        }

        public ActionResult Editor()
        {
            return this.View();
        }

        public ActionResult Offline()
        {
            return this.View();
        }

        public ActionResult TileCanvas()
        {
            return this.View();
        }        
    }
}
