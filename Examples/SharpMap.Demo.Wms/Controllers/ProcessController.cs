namespace SharpMap.Demo.Wms.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using GeoAPI.Geometries;

    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Utilities;
    using NetTopologySuite.IO;
    using NetTopologySuite.Operation.Linemerge;
    using NetTopologySuite.Operation.Polygonize;
    using NetTopologySuite.Operation.Union;
    using NetTopologySuite.Simplify;

    using SharpMap.Converters.GeoJSON;

    public class ProcessController : Controller
    {
        private readonly IGeometryFactory factory = GeometryFactory.Default;

        private IGeometryCollection ValidateGeo(string geo)
        {
            if (String.IsNullOrEmpty(geo))
                throw new ArgumentException("invalid argument 'geo': empty string");

            WKTReader reader = new WKTReader(this.factory);
            IGeometry data = reader.Read(geo);

            // we should expect a GeometryCollection object
            if (data is IGeometryCollection)
                return data as IGeometryCollection;
            throw new ArgumentException("invalid argument 'geo': IGeometryCollection expected");
        }

        private ILineString ValidateCut(string cut)
        {
            if (String.IsNullOrEmpty(cut))
                throw new ArgumentException("invalid argument 'cut': empty string");

            WKTReader reader = new WKTReader(this.factory);
            IGeometry data = reader.Read(cut);

            // we should expect a GeometryCollection object
            if (data is ILineString)
                return data as ILineString;
            throw new ArgumentException("invalid argument 'cut': ILineString expected");
        }

        private ActionResult MakeResponse(IEnumerable<IGeometry> cleaned)
        {
            // we should return a GeometryCollection object
            IGeometryCollection result = this.factory.CreateGeometryCollection(cleaned.ToArray());
            StringWriter writer = new StringWriter();
            GeoJSONWriter.Write(result, writer);
            return this.Json(new { geo = writer.ToString() });
        }

        private static IEnumerable<IGeometry> GetItems(IGeometry coll)
        {
            if (coll == null) 
                throw new ArgumentNullException("coll");

            for (int i = 0; i < coll.NumGeometries; i++)
                yield return coll.GetGeometryN(i);
        }

        [HttpPost]
        public ActionResult Clean(string geo)
        {
            IGeometryCollection coll = this.ValidateGeo(geo);
            IEnumerable<IGeometry> data = DoClean(coll);
            return this.MakeResponse(data);
        }

        private static IEnumerable<IGeometry> DoClean(IGeometryCollection coll)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");

            IEnumerable<IGeometry> items = GetItems(coll);
            foreach (IGeometry geom in items)
            {
                DouglasPeuckerSimplifier simplifier = new DouglasPeuckerSimplifier(geom);
                IGeometry clean = simplifier.GetResultGeometry();
                yield return clean;
            }
        }       

        [HttpPost]
        public ActionResult Merge(string geo)
        {
            IGeometryCollection coll = this.ValidateGeo(geo);
            IEnumerable<IGeometry> data = DoMerge(coll);
            return this.MakeResponse(data);
        }

        private static IEnumerable<IGeometry> DoMerge(IGeometryCollection coll)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");

            IEnumerable<IGeometry> items = GetItems(coll);
            yield return UnaryUnionOp.Union(items.ToArray());
        }

        [HttpPost]
        public ActionResult Split(string geo, string cut)
        {
            IGeometryCollection coll = this.ValidateGeo(geo);
            ILineString split = this.ValidateCut(cut);
            IEnumerable<IGeometry> data = DoSplit(coll, split);
            return this.MakeResponse(data);
        }

        public static IGeometry Polygonize(IGeometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            IList<ILineString> lines = LineStringExtracter.GetLines(geometry);
            IList<IGeometry> geoms = lines.Cast<IGeometry>().ToList();

            Polygonizer polygonizer = new Polygonizer();            
            polygonizer.Add(geoms);
            IList<IGeometry> polys = polygonizer.GetPolygons();

            IGeometry[] array = GeometryFactory.ToPolygonArray(polys);
            return geometry.Factory.CreateGeometryCollection(array);
        }

        private static IEnumerable<IGeometry> DoSplit(IGeometryCollection coll, ILineString cut)
        {
            if (coll == null)
                throw new ArgumentNullException("coll");
            if (cut == null)
                throw new ArgumentNullException("cut");

            // adapted from 'SplitPolygon WPS' GeoServer extension:
            // https://github.com/mdavisog/wps-splitpoly/tree/master/src

            IList<IGeometry> output = new List<IGeometry>();
            IEnumerable<IGeometry> items = GetItems(coll);
            IEnumerable<IGeometry> valid = items.Where(i => i is IPolygon);
            foreach (IGeometry item in valid)
            {
                IGeometry nodedLinework = item.Boundary.Union(cut);
                IGeometry polygons = Polygonize(nodedLinework);

                // only keep polygons which are inside the input                
                for (int i = 0; i < polygons.NumGeometries; i++)
                {
                    IPolygon candpoly = (IPolygon)polygons.GetGeometryN(i);
                    if (item.Contains(candpoly.InteriorPoint))
                        output.Add(candpoly);
                }                
            }
            return output;

            /*
            IEnumerable<IGeometry> items = GetItems(coll);
            LineMerger merger = new LineMerger();            
            merger.Add(items);
            merger.Add(cut);
            IList<IGeometry> list = merger.GetMergedLineStrings();

            IGeometry union = UnaryUnionOp.Union(list);

            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(union);
            ICollection<IGeometry> split = polygonizer.GetPolygons();
            return split;
             * */
        }
    }
}
