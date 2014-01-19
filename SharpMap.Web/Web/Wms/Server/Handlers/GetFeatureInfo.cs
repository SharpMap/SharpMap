using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using SharpMap.Converters.GeoJSON;
using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Web.Wms.Exceptions;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfo : AbstractHandler
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

            // Code specific for GetFeatureInfo
            string queryLayers = request.GetParam("QUERY_LAYERS");
            if (queryLayers == null)
                throw new WmsParameterNotSpecifiedException("QUERY_LAYERS");
            @params.QueryLayers = queryLayers;

            string infoFormat = request.GetParam("INFO_FORMAT");
            if (infoFormat == null)
                throw new WmsParameterNotSpecifiedException("INFO_FORMAT");
            @params.InfoFormat = infoFormat;

            //parameters X&Y are not part of the 1.3.0 specification, but are included for backwards compatability with 1.1.1 (OpenLayers likes it when used together with wms1.1.1 services)
            string x = request.GetParam("X");
            string i = request.GetParam("I");
            if (x == null && i == null)
                throw new WmsParameterNotSpecifiedException("I");
            string y = request.GetParam("Y");
            string j = request.GetParam("J");
            if (y == null && j == null)
                throw new WmsParameterNotSpecifiedException("J");
            float cx = 0, cy = 0;
            if (x != null)
            {
                try { cx = Convert.ToSingle(x); }
                catch
                {
                    throw new WmsInvalidParameterException("Invalid parameters for X");
                }
            }
            if (i != null)
            {
                try { cx = Convert.ToSingle(i); }
                catch
                {
                    throw new WmsInvalidParameterException("Invalid parameters for I");
                }
            }
            if (y != null)
            {
                try { cy = Convert.ToSingle(y); }
                catch
                {
                    throw new WmsInvalidParameterException("Invalid parameters for Y");
                }
            }
            if (j != null)
            {
                try { cy = Convert.ToSingle(j); }
                catch
                {
                    throw new WmsInvalidParameterException("Invalid parameters for I");
                }
            }
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

            string vstr;
            GetFeatureInfoResponse response;
            string[] requestLayers = @params.QueryLayers.Split(new[] { ',' });
            bool json = String.Equals(@params.InfoFormat, "text/json", StringComparison.InvariantCultureIgnoreCase);
            if (json)
            {
                vstr = CreateFeatureInfoGeoJSON(map, requestLayers, @params.X, @params.Y, @params.CqlFilter, _pixelSensitivity, _intersectDelegate);
                response = new GetFeatureInfoResponseJson(vstr);
            }
            else
            {
                vstr = CreateFeatureInfoPlain(map, requestLayers, @params.X, @params.Y, @params.FeatureCount, @params.CqlFilter, _pixelSensitivity, _intersectDelegate);
                response = new GetFeatureInfoResponsePlain(vstr);
            }

            response.Charset = _encoding.WebName;
            return response;
        }

        /// <summary>
        /// Gets FeatureInfo as text/plain
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="requestedLayers">The requested layers</param>
        /// <param name="x">The x-ordinate</param>
        /// <param name="y">The y-ordinate</param>
        /// <param name="featureCount"></param>
        /// <param name="cqlFilter">The code query language</param>
        /// <param name="pixelSensitivity"></param>
        /// <param name="intersectDelegate"></param>
        /// <exception cref="InvalidOperationException">Thrown if this function is used without a valid <see cref="HttpContext"/> at hand</exception>
        /// <returns>Plain text string with featureinfo results</returns>
        private string CreateFeatureInfoPlain(Map map, string[] requestedLayers, float x, float y, int featureCount, string cqlFilter, int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate)
        {
            StringBuilder vstr = new StringBuilder("GetFeatureInfo results: \n");
            foreach (string requestLayer in requestedLayers)
            {
                ICanQueryLayer queryLayer = GetQueryLayer(map, requestLayer);

                FeatureDataSet fds;
                // at this point queryLayer should have a non null value
                // check if the layer can be queried and if there is any data
                if (queryLayer.IsQueryEnabled && TryGetData(map, x, y, pixelSensitivity, intersectDelegate, queryLayer, cqlFilter, out fds))
                {
                    //if featurecount < fds...count, select smallest bbox, because most likely to be clicked
                    vstr.AppendFormat("\n Layer: '{0}'\n Featureinfo:\n", requestLayer);
                    int[] keys = new int[fds.Tables[0].Rows.Count];
                    double[] area = new double[fds.Tables[0].Rows.Count];
                    for (int l = 0; l < fds.Tables[0].Rows.Count; l++)
                    {
                        FeatureDataRow fdr = (FeatureDataRow)fds.Tables[0].Rows[l];
                        area[l] = fdr.Geometry.EnvelopeInternal.Area;
                        keys[l] = l;
                    }
                    Array.Sort(area, keys);
                    if (fds.Tables[0].Rows.Count < featureCount)
                    {
                        featureCount = fds.Tables[0].Rows.Count;
                    }
                    for (int k = 0; k < featureCount; k++)
                    {
                        for (int j = 0; j < fds.Tables[0].Rows[keys[k]].ItemArray.Length; j++)
                        {
                            vstr.AppendFormat(" '{0}'", fds.Tables[0].Rows[keys[k]].ItemArray[j]);
                        }
                        if ((k + 1) < featureCount)
                            vstr.Append(",\n");
                    }
                }
                else
                {
                    vstr.AppendFormat("\nSearch returned no results on layer: {0}", requestLayer);
                }

            }
            return vstr.ToString();
        }

        private ICanQueryLayer GetQueryLayer(Map map, string requestLayer)
        {
            foreach (ILayer mapLayer in map.Layers)
            {
                if (!((String.Equals(mapLayer.LayerName, requestLayer, StringComparison.InvariantCultureIgnoreCase))))
                    continue;

                return mapLayer as ICanQueryLayer;
            }
            throw new WmsLayerNotDefinedException(requestLayer);
        }

        private bool TryGetData(Map map, float x, float y, int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate, ICanQueryLayer queryLayer, string cqlFilter, out FeatureDataSet fds)
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

            if (intersectDelegate != null)
            {
                fds.Tables[0] = intersectDelegate(fds.Tables[0], queryBox);
            }

            //filter the rows with the CQLFilter if one is provided
            if (cqlFilter != null)
            {
                for (int i = fds.Tables[0].Rows.Count - 1; i >= 0; i--)
                {
                    if (!CqlFilter((FeatureDataRow)fds.Tables[0].Rows[i], cqlFilter))
                    {
                        fds.Tables[0].Rows.RemoveAt(i);
                    }
                }
            }

            return fds.Tables.Count > 0 && fds.Tables[0].Rows.Count > 0;
        }

        /// <summary>
        /// Gets FeatureInfo as GeoJSON
        /// </summary>
        /// <param name="map">The map to create the feature info from</param>
        /// <param name="requestedLayers">The layers to create the feature info for</param>
        /// <param name="x">The x-Ordinate</param>
        /// <param name="y">The y-Ordinate</param>
        /// <param name="cqlFilter">The CQL Filter string</param>
        /// <param name="pixelSensitivity"></param>
        /// <param name="intersectDelegate"></param>
        /// <exception cref="InvalidOperationException">Thrown if this function is used without a valid <see cref="HttpContext"/> at hand</exception>
        /// <returns>GeoJSON string with featureinfo results</returns>
        private string CreateFeatureInfoGeoJSON(Map map, string[] requestedLayers, float x, float y, string cqlFilter, int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate)
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
                    // Reproject geometries if needed
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

    public abstract class GetFeatureInfoResponse : IHandlerResponse
    {
        private readonly string _response;
        private string _charset;
        private bool _bufferOutput;

        protected GetFeatureInfoResponse(string response)
        {
            if (String.IsNullOrEmpty(response))
                throw new ArgumentNullException("response");
            _response = response;
        }

        public abstract string ContentType { get; }

        public string Charset
        {
            get { return _charset; }
            set { _charset = value; }
        }

        protected bool BufferOutput
        {
            get { return _bufferOutput; }
            set { _bufferOutput = value; }
        }

        public void WriteToContextAndFlush(IContextResponse response)
        {
            response.Clear();
            if (Charset != null)
            {
                //"windows-1252";
                response.Charset = Charset;
            }
            response.BufferOutput = BufferOutput;
            response.Write(_response);
            response.End();
        }
    }

    public class GetFeatureInfoResponseJson : GetFeatureInfoResponse
    {
        public GetFeatureInfoResponseJson(string response)
            : base(response)
        {
            BufferOutput = true;
        }

        public override string ContentType
        {
            get { return "text/json"; }
        }
    }

    public class GetFeatureInfoResponsePlain : GetFeatureInfoResponse
    {
        public GetFeatureInfoResponsePlain(string response) : base(response) { }

        public override string ContentType
        {
            get { return "text/plain"; }
        }
    }
}
