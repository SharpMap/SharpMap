namespace SharpMap.Demo.Wms.Controllers
{
    using System;
    using System.IO;
    using System.Web.Mvc;

    using GeoAPI.Geometries;

    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NetTopologySuite.Simplify;

    using SharpMap.Converters.GeoJSON;

    public class ProcessController : Controller
    {
        [HttpPost]
        public ActionResult Clean(string geo)
        {
            if (String.IsNullOrEmpty(geo))
                throw new ArgumentException("invalid argument 'geo': empty string");

            IGeometryFactory factory = GeometryFactory.Default;
            WKTReader reader = new WKTReader(factory);
            IGeometry data = reader.Read(geo);

            // we should expect a GeometryCollection object
            if (!(data is IGeometryCollection))
                throw new ArgumentException("invalid argument 'geo': GeometryCollection expected");

            // Simplify each item
            int num = data.NumGeometries;
            IGeometry[] cleaned = new IGeometry[num];
            for (int i = 0; i < num; i++)
            {
                IGeometry geom = data.GetGeometryN(i);
                DouglasPeuckerSimplifier simplifier = new DouglasPeuckerSimplifier(geom);
                IGeometry clean = simplifier.GetResultGeometry();
                cleaned[i] = clean;
            }

            // we must return a GeometryCollection object
            IGeometryCollection result = factory.CreateGeometryCollection(cleaned);
            StringWriter writer = new StringWriter();
            GeoJSONWriter.Write(result, writer);
            return this.Json(new { geo = writer.ToString() });
        }
    }
}
