using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI.CoordinateSystems.Transformations;
using NetTopologySuite.Geometries;
using SharpMap.Converters.GeoJSON;
using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Web.Wms.Exceptions;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;

namespace SharpMap.Demo.Wms.Handlers
{
    using Geometry = GeoAPI.Geometries.IGeometry;
    using BoundingBox = GeoAPI.Geometries.Envelope;

    public class StdJsonMapHandler : AbstractStdMapHandler
    {
        public override void ProcessRequest(IContext context)
        {
            IContextRequest request = context.Request;
            IContextResponse response = context.Response;
            try
            {
                string s = request.Params["BBOX"];
                if (String.IsNullOrEmpty(s))
                    throw new WmsInvalidParameterException("BBOX");


                Map map = GetMap(request);
                LayerCollection layers = map.Layers;
                ILayer first = layers.First();
                bool flip = first.TargetSRID == 4326;
                BoundingBox bbox = AbstractHandler.ParseBBOX(s, flip);
                if (bbox == null)
                    throw new WmsInvalidParameterException("Invalid parameter BBOX");

                string ls = request.Params["LAYERS"];
                if (!String.IsNullOrEmpty(ls))
                {
                    string[] strings = ls.Split(',');
                    foreach (ILayer layer in layers)
                        if (!strings.Contains(layer.LayerName))
                            layer.Enabled = false;
                }

                IEnumerable<GeoJSON> items = GetData(map, bbox);
                StringWriter writer = new StringWriter();
                GeoJSONWriter.Write(items, writer);
                string buffer = writer.ToString();

                IHandlerResponse result = new GetFeatureInfoResponseJson(buffer);
                result.WriteToContextAndFlush(response);
            }
            catch (WmsExceptionBase ex)
            {
                ex.WriteToContextAndFlush(response);
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