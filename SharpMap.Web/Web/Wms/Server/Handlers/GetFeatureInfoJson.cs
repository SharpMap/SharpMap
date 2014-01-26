using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using SharpMap.Converters.GeoJSON;
using SharpMap.Data;
using SharpMap.Layers;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoJson : GetFeatureInfo
    {
        public GetFeatureInfoJson(Capabilities.WmsServiceDescription description) :
            base(description) { }

        public GetFeatureInfoJson(Capabilities.WmsServiceDescription description,
            GetFeatureInfoParams @params) : base(description, @params) { }

        protected override AbstractGetFeatureInfoResponse CreateFeatureInfo(Map map, 
            IEnumerable<string> requestedLayers, 
            float x, float y, 
            int featureCount, 
            string cqlFilter,
            int pixelSensitivity, 
            WmsServer.InterSectDelegate intersectDelegate)
        {
            List<GeoJSON> items = new List<GeoJSON>();
            foreach (string requestLayer in requestedLayers)
            {
                ICanQueryLayer queryLayer = GetQueryLayer(map, requestLayer);
                FeatureDataSet fds;
                if (!TryGetData(map, x, y, pixelSensitivity, intersectDelegate, queryLayer, cqlFilter, out fds))
                    continue;
                
                IEnumerable<GeoJSON> data = GeoJSONHelper.GetData(fds);
                // reproject geometries if needed
                IMathTransform transform = null;
                if (queryLayer is VectorLayer)
                {
                    ICoordinateTransformation transformation = (queryLayer as VectorLayer).CoordinateTransformation;
                    if (transformation != null)
                        transform = transformation.MathTransform;                    
                }

                if (transform != null)
                {
#if DotSpatialProjections
                    throw new NotImplementedException();
#else
                    data = data.Select(d =>
                    {
                        IGeometry converted = GeometryTransform.TransformGeometry(d.Geometry, transform, map.Factory);
                        d.SetGeometry(converted);
                        return d;
                    });
#endif
                }
                items.AddRange(data);
            }

            StringWriter sb = new StringWriter();
            GeoJSONWriter.Write(items, sb);
            return new GetFeatureInfoResponseJson(sb.ToString());
        }
    }
}