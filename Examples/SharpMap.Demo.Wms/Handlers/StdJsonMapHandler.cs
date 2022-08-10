namespace SharpMap.Demo.Wms.Handlers
{
    using NetTopologySuite.CoordinateSystems.Transformations;
    using ProjNet.CoordinateSystems.Transformations;
    using SharpMap.Converters.GeoJSON;
    using SharpMap.Data;
    using SharpMap.Layers;
    using SharpMap.Web.Wms;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web;
    using BoundingBox = NetTopologySuite.Geometries.Envelope;
    using Geometry = NetTopologySuite.Geometries.Geometry;

    public class StdJsonMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(HttpContext context)
        {
            try
            {
                string s = context.Request.Params["BBOX"];
                if (String.IsNullOrEmpty(s))
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue, "Required parameter BBOX not specified", context);
                    return;
                }

                Map map = this.GetMap(context.Request);
                bool flip = map.Layers[0].TargetSRID == 4326;
                BoundingBox bbox = WmsServer.ParseBBOX(s, flip);
                if (bbox == null)
                {
                    WmsException.ThrowWmsException("Invalid parameter BBOX", context);
                    return;
                }

                string ls = context.Request.Params["LAYERS"];
                if (!String.IsNullOrEmpty(ls))
                {
                    string[] layers = ls.Split(',');
                    foreach (ILayer layer in map.Layers)
                        if (!layers.Contains(layer.LayerName))
                            layer.Enabled = false;
                }

                IEnumerable<GeoJSON> items = GetData(map, bbox);
                StringWriter writer = new StringWriter();
                GeoJSONWriter.Write(items, writer);
                string buffer = writer.ToString();

                context.Response.Clear();
                context.Response.ContentType = "text/json";
                context.Response.BufferOutput = true;
                context.Response.Write(buffer);
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
                //context.Response.End();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }

        private static IEnumerable<GeoJSON> GetData(Map map, BoundingBox bbox)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            // Only queryable data!
            IQueryable<ICanQueryLayer> coll = map.Layers
                .AsQueryable()
                .Where(l => l.Enabled)
                .OfType<ICanQueryLayer>()
                .Where(l => l.IsQueryEnabled);

            List<GeoJSON> items = new List<GeoJSON>();
            foreach (ICanQueryLayer layer in coll)
            {
                IEnumerable<GeoJSON> data = QueryData(bbox, layer);
                items.AddRange(data);
            }
            return items;
        }

        private static IEnumerable<GeoJSON> QueryData(BoundingBox bbox, ICanQueryLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException("layer");

            // Query for data
            FeatureDataSet ds = new FeatureDataSet();
            layer.ExecuteIntersectionQuery(bbox, ds);
            IEnumerable<GeoJSON> data = GeoJSONHelper.GetData(ds);

            // Reproject geometries if needed
            MathTransform transform = null;
            if (layer is VectorLayer)
            {
                ICoordinateTransformation transformation = (layer as VectorLayer).CoordinateTransformation;
                transform = transformation == null ? null : transformation.MathTransform;
            }
            if (transform != null)
            {
                data = data.Select(d =>
                    {
                        Geometry converted = GeometryTransform.TransformGeometry(d.Geometry, transform, d.Geometry.Factory);
                        d.SetGeometry(converted);
                        return d;
                    });
            }
            return data;
        }
    }
}
