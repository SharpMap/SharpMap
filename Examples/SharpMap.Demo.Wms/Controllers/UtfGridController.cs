// code adapted from: https://github.com/awcoats/mapstache

using System.Linq.Expressions;
using SharpMap.Features;

namespace SharpMap.Demo.Wms.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Web.Mvc;

    using GeoAPI.Geometries;

    using Mapstache;

    using GeoAPI.CoordinateSystems.Transformations;

    using SharpMap.Converters.GeoJSON;
    using SharpMap.Demo.Wms.Helpers;
    using SharpMap.Layers;

    public class UtfGridController : Controller
    {
        private readonly ICoordinateTransformation projection;

        public UtfGridController()
        {
            this.projection = ProjHelper.LatLonToGoogle();
        }

        const int UtfGridResolution = 2;

        private Envelope GetBoundingBoxInLatLngWithMargin(int tileX, int tileY, int zoom)
        {
            Point px1 = new Point((tileX * 256), (tileY * 256));
            Point px2 = new Point(((tileX + 1) * 256), ((tileY + 1) * 256));

            PointF ll1 = TileSystemHelper.PixelXYToLatLong(px1, zoom);
            PointF ll2 = TileSystemHelper.PixelXYToLatLong(px2, zoom);

            double[] prj1 = this.projection.MathTransform.Transform(new double[] { ll1.X, ll1.Y });
            double[] prj2 = this.projection.MathTransform.Transform(new double[] { ll2.X, ll2.Y });

            Envelope bbox = new Envelope();
            bbox.ExpandToInclude(prj1[0], prj1[1]);
            bbox.ExpandToInclude(prj2[0], prj2[1]);
            return bbox;
        }

        public JsonResult GetData(string layer, int z, int x, int y)
        {
            if (String.IsNullOrEmpty(layer))
                throw new ArgumentNullException("layer");

            Map map = ShapefileHelper.Spherical();
            IQueryable<VectorLayer> coll = map.Layers
                .AsQueryable()
                .OfType<VectorLayer>()
                .Where(l => l.Enabled && l.IsQueryEnabled)
                .Where(l => String.Equals(l.LayerName, layer));
            VectorLayer query = coll.SingleOrDefault();
            if (query == null)
                throw new ArgumentException("Layer not found: " + layer);

            if (query.SRID != 4326)
                throw new ArgumentException("Only EPSG:4326 supported");

            using (Utf8Grid grid = new Utf8Grid(UtfGridResolution, x, y, z))
            {
                Envelope bbox = this.GetBoundingBoxInLatLngWithMargin(x, y, z);
                var ds = new FeatureCollectionSet();
                query.ExecuteIntersectionQuery(bbox, ds);
                IEnumerable<GeoJSON> data = GeoJSONHelper.GetData(ds);

                int i = 1;
                foreach (GeoJSON val in data)
                {
                    IGeometry geom = val.Geometry;
                    IDictionary<string, object> dict = val.Values;
                    grid.FillPolygon(geom, i, dict);
                    i = i + 1;
                }

                Utf8GridResults results = grid.CreateUtfGridJson();
                return this.Json(new { keys = results.Keys, data = results.Data, grid = results.Grid, }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}