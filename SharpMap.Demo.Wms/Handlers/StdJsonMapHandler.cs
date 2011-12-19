namespace SharpMap.Demo.Wms.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web;

    using ProjNet.CoordinateSystems.Transformations;

    using SharpMap.Converters.GeoJSON;
    using SharpMap.Data;
    using SharpMap.Geometries;
    using SharpMap.Layers;
    using SharpMap.Web.Wms;

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
                IEnumerable<GeoJSON> items = GeoData(map, bbox);

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

        private IEnumerable<GeoJSON> GeoData(Map map, BoundingBox bbox)
        {
            List<GeoJSON> items = new List<GeoJSON>();

            // Only queryable data!
            IQueryable<ICanQueryLayer> collection =
                map.Layers.AsQueryable().OfType<ICanQueryLayer>().Where(l => l.Enabled && l.IsQueryEnabled);
            foreach (ICanQueryLayer layer in collection)
            {
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
                    data = data.Select(d =>
                    {
                        Geometry converted = GeometryTransform.TransformGeometry(d.Geometry, transform);
                        d.SetGeometry(converted);
                        return d;
                    });
                }

                items.AddRange(data);
            }
            return items;
        }
    }
}