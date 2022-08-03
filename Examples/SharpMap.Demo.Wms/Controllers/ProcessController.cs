namespace SharpMap.Demo.Wms.Controllers
{
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Utilities;
    using NetTopologySuite.IO;
    using NetTopologySuite.Operation.Polygonize;
    using NetTopologySuite.Operation.Union;
    using NetTopologySuite.Simplify;
    using SharpMap.Converters.GeoJSON;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    public class ProcessController : Controller
    {
        private readonly GeometryFactory factory = GeometryFactory.Default;

        private GeometryCollection ValidateGeo(string geo)
        {
            if (String.IsNullOrEmpty(geo))
                throw new ArgumentException("invalid argument 'geo': empty string");

            WKTReader reader = new WKTReader(NetTopologySuite.NtsGeometryServices.Instance);
            Geometry data = reader.Read(geo);

            // we should expect a GeometryCollection object
            if (data is GeometryCollection)
                return data as GeometryCollection;
            throw new ArgumentException("invalid argument 'geo': GeometryCollection expected");
        }

        private LineString ValidateCut(string cut)
        {
            if (String.IsNullOrEmpty(cut))
                throw new ArgumentException("invalid argument 'cut': empty string");

            WKTReader reader = new WKTReader(this.factory);
            Geometry data = reader.Read(cut);

            // we should expect a GeometryCollection object
            if (data is LineString)
                return data as LineString;
            throw new ArgumentException("invalid argument 'cut': LineString expected");
        }

        private ActionResult MakeResponse(IEnumerable<Geometry> geometries)
        {
            if (geometries == null)
                throw new ArgumentNullException("geometries");

            // we should return a GeometryCollection object
            Geometry[] array = geometries.ToArray();
            GeometryCollection result = this.factory.CreateGeometryCollection(array);
            StringWriter writer = new StringWriter();
            GeoJSONWriter.Write(result, writer);
            return this.Json(new { geo = writer.ToString() });
        }

        private static IEnumerable<Geometry> GetItems(Geometry coll)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");

            for (int i = 0; i < coll.NumGeometries; i++)
                yield return coll.GetGeometryN(i);
        }

        [HttpPost]
        public ActionResult Clean(string geo)
        {
            GeometryCollection coll = this.ValidateGeo(geo);
            IEnumerable<Geometry> data = DoClean(coll);
            return this.MakeResponse(data);
        }

        private static IEnumerable<Geometry> DoClean(GeometryCollection coll)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");

            IEnumerable<Geometry> items = GetItems(coll);
            foreach (Geometry geom in items)
            {
                DouglasPeuckerSimplifier simplifier = new DouglasPeuckerSimplifier(geom);
                Geometry clean = simplifier.GetResultGeometry();
                yield return clean;
            }
        }

        [HttpPost]
        public ActionResult Merge(string geo)
        {
            GeometryCollection coll = this.ValidateGeo(geo);
            IEnumerable<Geometry> data = DoMerge(coll);
            return this.MakeResponse(data);
        }

        private static IEnumerable<Geometry> DoMerge(GeometryCollection coll)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");

            IEnumerable<Geometry> items = GetItems(coll);
            yield return UnaryUnionOp.Union(items.ToArray());
        }

        [HttpPost]
        public ActionResult Split(string geo, string cut)
        {
            GeometryCollection coll = this.ValidateGeo(geo);
            LineString split = this.ValidateCut(cut);
            IEnumerable<Geometry> data = DoSplit(coll, split);
            return this.MakeResponse(data);
        }

        public static Geometry Polygonize(Geometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            var lines = LineStringExtracter.GetLines(geometry);
            var geoms = lines.ToList();

            var polygonizer = new Polygonizer();
            polygonizer.Add(geoms);
            var polys = polygonizer.GetPolygons();
            return geometry.Factory.BuildGeometry(polys);
        }

        private static IEnumerable<Geometry> DoSplit(GeometryCollection coll, LineString cut)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");
            if (cut == null)
                throw new ArgumentNullException("cut");

            // adapted from 'SplitPolygon WPS' GeoServer extension:
            // https://github.com/mdavisog/wps-splitpoly/tree/master/src

            IList<Geometry> output = new List<Geometry>();
            IEnumerable<Geometry> items = GetItems(coll);
            IEnumerable<Geometry> valid = items.Where(i => i is Polygon);
            foreach (Geometry item in valid)
            {
                Geometry nodedLinework = item.Boundary.Union(cut);
                Geometry polygons = Polygonize(nodedLinework);

                // only keep polygons which are inside the input                
                for (int i = 0; i < polygons.NumGeometries; i++)
                {
                    Polygon candpoly = (Polygon)polygons.GetGeometryN(i);
                    if (item.Contains(candpoly.InteriorPoint))
                        output.Add(candpoly);
                }
            }
            return output;

            /*
            IEnumerable<Geometry> items = GetItems(coll);
            LineMerger merger = new LineMerger();            
            merger.Add(items);
            merger.Add(cut);
            IList<Geometry> list = merger.GetMergedLineStrings();

            Geometry union = UnaryUnionOp.Union(list);

            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(union);
            ICollection<Geometry> split = polygonizer.GetPolygons();
            return split;
             * */
        }
    }
}
