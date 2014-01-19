using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Web;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Web.Wms.Exceptions;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public abstract class GetFeatureInfo : AbstractHandler
    {
        private readonly int _pixelSensitivity;
        private readonly WmsServer.InterSectDelegate _intersectDelegate;
        private readonly Encoding _encoding;

        public GetFeatureInfo(Capabilities.WmsServiceDescription description,
            int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate, Encoding encoding) :
            base(description)
        {
            _pixelSensitivity = pixelSensitivity;
            _intersectDelegate = intersectDelegate;
            _encoding = encoding;
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
            //(OpenLayers likes it when used together with wms1.1.1 services)
            // we will assume that Openlayers is used by default 
            // so check X&Y first
            string x = request.GetParam("X") ?? request.GetParam("I");
            string y = request.GetParam("Y") ?? request.GetParam("J");

            if (x == null) 
                throw new WmsParameterNotSpecifiedException("I");

            if (y == null) 
                throw new WmsParameterNotSpecifiedException("J");

            float cx = 0, cy = 0;

            if (!float.TryParse(x, out cx)) 
                throw new WmsInvalidParameterException("Invalid parameters for X / I");

            if (!float.TryParse(y, out cy)) 
                throw new WmsInvalidParameterException("Invalid parameters for Y / J");

            @params.X = cx;
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
            string info = CreateFeatureInfo(map, requestLayers, @params.X, @params.Y, @params.FeatureCount, @params.CqlFilter, _pixelSensitivity, _intersectDelegate);
            GetFeatureInfoResponse response = new GetFeatureInfoResponsePlain(info);
            response.Charset = _encoding.WebName;
            return response;
        }

        /// <summary>
        /// Gets FeatureInfo
        /// </summary>
        /// <param name="map">The map to create the feature info from</param>
        /// <param name="requestedLayers">The layers to create the feature info for</param>
        /// <param name="x">The x-ordinate</param>
        /// <param name="y">The y-ordinate</param>
        /// <param name="featureCount">The max number of features retrieved.</param>
        /// <param name="cqlFilter">The CQL Filter string</param>
        /// <param name="pixelSensitivity">The sensitivity to use when querying data.</param>
        /// <param name="intersectDelegate">A <see cref="WmsServer.InterSectDelegate"/> to filter data.</param>
        /// <returns>Plain text string with featureinfo results</returns>
        protected abstract string CreateFeatureInfo(Map map, IEnumerable<string> requestedLayers, float x, float y,
            int featureCount, string cqlFilter, int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate);

        protected ICanQueryLayer GetQueryLayer(Map map, string requestLayer)
        {
            foreach (ILayer mapLayer in map.Layers)
                if (String.Equals(mapLayer.LayerName, requestLayer))
                    return mapLayer as ICanQueryLayer;
            throw new WmsLayerNotDefinedException(requestLayer);
        }

        protected bool TryGetData(Map map, float x, float y, int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate, ICanQueryLayer queryLayer, string cqlFilter, out FeatureDataSet fds)
        {
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

            return tables.Count > 0 && table.Rows.Count > 0;
        }
    }
}
