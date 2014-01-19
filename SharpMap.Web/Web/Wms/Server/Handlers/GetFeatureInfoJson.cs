using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using SharpMap.Converters.GeoJSON;
using SharpMap.Data;
using SharpMap.Layers;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoJson : GetFeatureInfo
    {
        public GetFeatureInfoJson(Capabilities.WmsServiceDescription description,
            int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate, Encoding encoding) :
            base(description, pixelSensitivity, intersectDelegate, encoding) { }

        protected override string CreateFeatureInfo(Map map, IEnumerable<string> requestedLayers, float x, float y, int featureCount, string cqlFilter,
            int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate)
        {
            List<GeoJSON> items = new List<GeoJSON>();
            foreach (string requestLayer in requestedLayers)
            {
                ICanQueryLayer queryLayer = GetQueryLayer(map, requestLayer);

                FeatureDataSet fds;
                // at this point queryLayer should have a non null value
                // check if the layer can be queried and if there is any data
                if (queryLayer.IsQueryEnabled && TryGetData(map, x, y, pixelSensitivity, intersectDelegate, queryLayer, cqlFilter, out fds))
                {
                    // maybe this part should go into the TryGetData method
                    // afterall we are going to use data after applying filter
                    IEnumerable<GeoJSON> data = GeoJSONHelper.GetData(fds);
#if DotSpatialProjections
                        throw new NotImplementedException();
#else
                    // reproject geometries if needed
                    IMathTransform transform = null;
                    if (queryLayer is VectorLayer)
                    {
                        ICoordinateTransformation transformation = (queryLayer as VectorLayer).CoordinateTransformation;
                        transform = transformation == null ? null : transformation.MathTransform;
                    }

                    if (transform != null)
                    {
                        data = data.Select(d =>
                        {
                            IGeometry converted = GeometryTransform.TransformGeometry(d.Geometry, transform, map.Factory);
                            d.SetGeometry(converted);
                            return d;
                        });
                    }
#endif
                    items.AddRange(data);
                }
            }
            StringWriter writer = new StringWriter();
            GeoJSONWriter.Write(items, writer);
            return writer.ToString();
        }
    }
}