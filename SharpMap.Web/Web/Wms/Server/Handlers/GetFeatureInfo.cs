using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Web.Wms.Exceptions;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public abstract class GetFeatureInfo : AbstractHandler
    {
        private readonly GetFeatureInfoParams _params;
        
        protected GetFeatureInfo(Capabilities.WmsServiceDescription description) :
            this(description, GetFeatureInfoParams.Empty) { }

        protected GetFeatureInfo(Capabilities.WmsServiceDescription description, 
            GetFeatureInfoParams @params) : base(description)
        {
            if (@params == null) 
                throw new ArgumentNullException("params");
            _params = @params;
        }
        
        protected override WmsParams ValidateParams(IContextRequest request, int targetSrid)
        {
            WmsParams @params = ValidateCommons(request, targetSrid);

            // code specific for GetFeatureInfo
            string queryLayers = request.GetParam("QUERY_LAYERS");
            if (queryLayers == null)
                throw new WmsParameterNotSpecifiedException("QUERY_LAYERS");
            @params.QueryLayers = queryLayers;

            string infoFormat = request.GetParam("INFO_FORMAT");
            if (infoFormat == null)
                throw new WmsParameterNotSpecifiedException("INFO_FORMAT");

            @params.InfoFormat = infoFormat;

            // parameters X&Y are not part of the 1.3.0 specification, 
            // but are included for backwards compatability with 1.1.1 
            // (OpenLayers likes it when used together with wms1.1.1 services)
            // we will assume that Openlayers is used by default 
            // so check X&Y first
            string x = request.GetParam("X") ?? request.GetParam("I");
            if (x == null)
                throw new WmsParameterNotSpecifiedException("I");
            string y = request.GetParam("Y") ?? request.GetParam("J");
            if (y == null)
                throw new WmsParameterNotSpecifiedException("J");

            float cx;
            if (!Single.TryParse(x, out cx))
                throw new WmsInvalidParameterException("Invalid parameters for X / I");
            @params.X = cx;
            float cy;
            if (!Single.TryParse(y, out cy))
                throw new WmsInvalidParameterException("Invalid parameters for Y / J");
            @params.Y = cy;

            string featureCount = request.GetParam("FEATURE_COUNT");
            int fc = Int32.TryParse(featureCount, out fc) ? Math.Max(fc, 1) : 1;
            @params.FeatureCount = fc;
            return @params;
        }

        public override IHandlerResponse Handle(Map map, IContextRequest request)
        {
            WmsParams @params = ValidateParams(request, TargetSrid(map));
            map.Size = new Size(@params.Width, @params.Height);
            map.ZoomToBox(@params.BBOX);
            string[] requestLayers = @params.QueryLayers.Split(new[] { ',' });
            AbstractGetFeatureInfoResponse info = CreateFeatureInfo(map,
                requestLayers,
                @params.X, @params.Y,
                @params.FeatureCount,
                @params.CqlFilter,
                _params.PixelSensitivity,
                _params.IntersectDelegate);
            if (_params.Encoding != null)
                info.Charset = _params.Encoding.WebName;
            return info;
        }

        /// <summary>
        /// Gets FeatureInfo.
        /// </summary>
        /// <param name="map">The map to create the feature info from.</param>
        /// <param name="requestedLayers">The layers to create the feature info for.</param>
        /// <param name="x">The x-ordinate.</param>
        /// <param name="y">The y-ordinate.</param>
        /// <param name="featureCount">The max number of features retrieved.</param>
        /// <param name="cqlFilter">The CQL Filter string.</param>
        /// <param name="pixelSensitivity">The sensitivity to use when querying data.</param>
        /// <param name="intersectDelegate">A <see cref="WmsServer.InterSectDelegate"/> to filter data.</param>
        /// <returns>Text string with featureinfo results.</returns>
        protected abstract AbstractGetFeatureInfoResponse CreateFeatureInfo(Map map,
            IEnumerable<string> requestedLayers,
            float x, float y,
            int featureCount,
            string cqlFilter,
            int pixelSensitivity,
            WmsServer.InterSectDelegate intersectDelegate);

        protected ICanQueryLayer GetQueryLayer(Map map, string requestLayer)
        {
            foreach (ILayer layer in map.Layers)
            {
                if (String.Equals(layer.LayerName, requestLayer))
                {
                    if (layer is ICanQueryLayer)
                        return layer as ICanQueryLayer;
                    throw new WmsLayerNotQueryableException(requestLayer);
                }
            }
            throw new WmsLayerNotDefinedException(requestLayer);
        }

        /// <summary>
        /// Check if the layer can be queried and retrieve data, if there is any.
        /// </summary>
        protected bool TryGetData(Map map,
            float x, float y,
            int pixelSensitivity,
            WmsServer.InterSectDelegate intersectDelegate,
            ICanQueryLayer queryLayer,
            string cqlFilter,
            out FeatureDataSet fds)
        {
            if (!queryLayer.IsQueryEnabled)
            {
                fds = null;
                return false;
            }

            float queryBoxMinX = x - pixelSensitivity;
            float queryBoxMinY = y - pixelSensitivity;
            float queryBoxMaxX = x + pixelSensitivity;
            float queryBoxMaxY = y + pixelSensitivity;

            Coordinate minXY = map.ImageToWorld(new PointF(queryBoxMinX, queryBoxMinY));
            Coordinate maxXY = map.ImageToWorld(new PointF(queryBoxMaxX, queryBoxMaxY));
            Envelope queryBox = new Envelope(minXY, maxXY);

            fds = new FeatureDataSet();
            queryLayer.ExecuteIntersectionQuery(queryBox, fds);

            FeatureTableCollection tables = fds.Tables;
            FeatureDataTable table = tables[0];
            if (intersectDelegate != null)
                tables[0] = intersectDelegate(table, queryBox);

            // filter the rows with the CQLFilter if one is provided
            if (cqlFilter != null)
            {
                DataRowCollection rows = table.Rows;
                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    FeatureDataRow row = (FeatureDataRow)rows[i];
                    bool b = CqlFilter(row, cqlFilter);
                    if (!b)
                        rows.RemoveAt(i);
                }
            }

            bool res = tables.Count > 0 && table.Rows.Count > 0;
            return res;
        }
    }
}
