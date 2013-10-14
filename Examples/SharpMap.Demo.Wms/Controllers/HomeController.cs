using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using SharpMap.Demo.Wms.Models;

namespace SharpMap.Demo.Wms.Controllers
{
    public class HomeController : Controller
    {
        private List<DemoItem> GetDemoItems()
        {
            List<DemoItem> items = new List<DemoItem>();
            Type type = GetType();

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
            ViewData["DemoItems"] = GetDemoItems();
            return View();
        }

        public ActionResult Default()
        {
            return View();
        }

        public ActionResult Openlayers()
        {
            return View();
        }

        public ActionResult Polymaps()
        {
            return View();
        }

        public ActionResult Leaflet()
        {
            return View();
        }

        public ActionResult Geojson()
        {
            return View();
        }

        public ActionResult Editor()
        {
            return View();
        }

        public ActionResult Offline()
        {
            return View();
        }

        public ActionResult TileCanvas()
        {
            return View();
        }

        public ActionResult Buildings()
        {
            return View();
        }

        public ActionResult UtfGrid()
        {
            return View();
        }

        public ActionResult D3()
        {
            return View();
        }

        public ActionResult BruTile()
        {
            return View();
        }
    }
}
