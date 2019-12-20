namespace SharpMap.Demo.Wms.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using GeoAPI;
    using GeoAPI.Geometries;

    using NetTopologySuite;
    using NetTopologySuite.Geometries;

    using SharpMap.Data;
    using SharpMap.Data.Providers;

    public class BuildingsController : Controller
    {
        private const int ScaleZ = 3;
        private const int TileSize = 256;
        private const int MaxZoom = 18;

        public BuildingsController()
        {
            GeometryServiceProvider.SetInstanceIfNotAlreadySetDirectly(new NtsGeometryServices());
        }

        private Point GeoToPixel(double lat, double lon, int zoom)
        {
            int size = TileSize << zoom;
            double latd = Math.Min(1, Math.Max(0, 0.5 - (Math.Log(Math.Tan(Math.PI / 4 + Math.PI / 2 * lat / 180)) / Math.PI) / 2));
            int lati = Convert.ToInt32(latd * size);

            double lngd = lon / 360 + 0.5;
            int lngi = Convert.ToInt32(lngd * size);
            return new Point(lngi, lati);
        }

        [HttpGet]
        public JsonResult GetData(float w, float n, float e, float s, int z)
        {
            string format = String.Format("~/App_Data/berlin/{0}", "osmbuildings.shp");
            string path = this.HttpContext.Server.MapPath(format);
            if (!System.IO.File.Exists(path))
                throw new FileNotFoundException("file not found", path);

            Point start = this.GeoToPixel(n, w, z);
            var meta = new { n, w, s, e, x = start.X, y = start.Y, z };

            Envelope bbox = new Envelope();
            bbox.ExpandToInclude(new Coordinate(n, w));
            bbox.ExpandToInclude(new Coordinate(s, e));

            FeatureDataSet ds = new FeatureDataSet();
            using (ShapeFile provider = new ShapeFile(path))
            {
                provider.DoTrueIntersectionQuery = true;
                provider.Open();
                provider.ExecuteIntersectionQuery(bbox, ds);
                provider.Close();
            }

            int zz = MaxZoom - z;
            List<object> data = new List<object>();
            FeatureDataTable table = ds.Tables[0];
            foreach (FeatureDataRow row in table)
            {
                int c = (short)(row["height"]);
                if (c == 0)
                    c = 5; // default value for "null" (zero) heights
                int h = c * ScaleZ >> zz;
                if (h <= 1) 
                    h = 1;

                IGeometry geometry = row.Geometry;
                Coordinate[] coords = geometry.Coordinates;
                int total = coords.Length;
                double[] values = new double[total * 2];
                int i = 0;
                foreach (Coordinate curr in coords)
                {
                    Point p = this.GeoToPixel(curr.X, curr.Y, z);
                    values[i++] = p.X - start.X;
                    values[i++] = p.Y - start.Y;
                }
                data.Add(new object[] { h, values });
            }

            return this.Json(new { meta, data }, JsonRequestBehavior.AllowGet);
        }
    }
}
