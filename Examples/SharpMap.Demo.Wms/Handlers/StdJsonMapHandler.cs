namespace SharpMap.Demo.Wms.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web;

    using NetTopologySuite.Geometries;

    using ProjNet.CoordinateSystems.Transformations;

    using SharpMap.Converters.GeoJSON;
    using SharpMap.Data;
    using SharpMap.Layers;
    using SharpMap.Web.Wms;

    using Geometry = GeoAPI.Geometries.IGeometry;
    using BoundingBox = GeoAPI.Geometries.Envelope;

    public class StdJsonMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(HttpContext context)
        {
            try
            {
                string s = context.Request.Params["BBOX"];
                if (String.IsNullOrEmpty(s))
                {
                    WmsException.ThrowWmsException(WmsException.WmsExceptionCode.InvalidDimensionValue, "Required parameter BBOX not specified");
                    return;
                }

                BoundingBox bbox = WmsServer.ParseBBOX(s);
                if (bbox == null)
                {
                    WmsException.ThrowWmsException("Invalid parameter BBOX");
                    return;
                }

                Map map = this.GetMap(context.Request);
                IEnumerable<GeoJSON> items = GetData(map, bbox);
                
                StringWriter writer = new StringWriter();
                GeoJSONWriter.Write(items, writer);
                string buffer = writer.ToString();

                context.Response.Clear();
                context.Response.ContentType = "text/json";
                context.Response.BufferOutput = true;
                context.Response.Write(buffer);
                context.Response.End();
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
            IQueryable<ICanQueryLayer> collection = map.Layers
                .AsQueryable()
                .OfType<ICanQueryLayer>()
                .Where(l => l.Enabled && l.IsQueryEnabled);

            List<GeoJSON> items = new List<GeoJSON>();
            foreach (ICanQueryLayer layer in collection)
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
            IMathTransform transform = null;
            if (layer is VectorLayer)
            {
                ICoordinateTransformation transformation = (layer as VectorLayer).CoordinateTransformation;
                transform = transformation == null ? null : transformation.MathTransform;
            }
            if (transform != null)
            {
                GeometryFactory gf = new GeometryFactory();
                data = data.Select(d =>
                    {
                        Geometry converted = GeometryTransform.TransformGeometry(d.Geometry, transform, gf);
                        d.SetGeometry(converted);
                        return d;
                    });
            }
            return data;
        }
    }
}