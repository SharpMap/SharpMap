using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        public GetFeatureInfo(Capabilities.WmsServiceDescription description) :
            base(description) { }

        protected override WmsParams ValidateParams(IContext context, int targetSrid)
        {
            WmsParams @params = ValidateCommons(context, targetSrid);
            if (!@params.IsValid)
                return @params;

            // Code specific for GetFeatureInfo
            string queryLayers = context.Params["QUERY_LAYERS"];
            if (queryLayers == null)
                return WmsParams.Failure("Required parameter QUERY_LAYERS not specified");
            @params.QueryLayers = queryLayers;

            string infoFormat = context.Params["INFO_FORMAT"];
            if (infoFormat == null)
                return WmsParams.Failure("Required parameter INFO_FORMAT not specified");
            @params.InfoFormat = infoFormat;

            //parameters X&Y are not part of the 1.3.0 specification, but are included for backwards compatability with 1.1.1 (OpenLayers likes it when used together with wms1.1.1 services)
            string x = context.Params["X"];
            string i = context.Params["I"];
            if (x == null && i == null)
                return WmsParams.Failure("Required parameter I not specified");
            string y = context.Params["Y"];
            string j = context.Params["J"];
            if (y == null && j == null)
                return WmsParams.Failure("Required parameter J not specified");
            float cx = 0, cy = 0;
            if (x != null)
            {
                try { cx = Convert.ToSingle(x); }
                catch
                {
                    return WmsParams.Failure("Invalid parameters for X");
                }
            }
            if (i != null)
            {
                try { cx = Convert.ToSingle(i); }
                catch
                {
                    return WmsParams.Failure("Invalid parameters for I");
                }
            }
            if (y != null)
            {
                try { cy = Convert.ToSingle(y); }
                catch
                {
                    return WmsParams.Failure("Invalid parameters for Y");
                }
            }
            if (j != null)
            {
                try { cy = Convert.ToSingle(j); }
                catch
                {
                    return WmsParams.Failure("Invalid parameters for I");
                }
            }
            @params.X = cx;
            @params.Y = cy;

            string featureCount = context.Params["FEATURE_COUNT"];
            int fc = Int32.TryParse(featureCount, out fc) ? Math.Max(fc, 1) : 1;
            @params.FeatureCount = fc;
            return @params;
        }

        public override void Handle(Map map, IContext context)
        {
            WmsParams @params = ValidateParams(context, TargetSrid(map));
            if (!@params.IsValid)
            {
                throw new WmsInvalidParameterException(@params.Error, @params.ErrorCode);
                //WmsException.ThrowWmsException(@params.ErrorCode, @params.Error, context);
                //return;
            }

            map.Size = new Size(@params.Width, @params.Height);
            map.ZoomToBox(@params.BBOX);

            string vstr;
            string[] requestLayers = @params.QueryLayers.Split(new[] { ',' });
            if (String.Equals(@params.InfoFormat, "text/json", StringComparison.InvariantCultureIgnoreCase))
            {
                vstr = CreateFeatureInfoGeoJSON(map, requestLayers, @params.X, @params.Y, @params.CqlFilter, context);
                //string.Empty is the result if a return WmsParams.Failure(...) has been called
                if (vstr == String.Empty)
                    return;
                context.ContentType = "text/json";
            }
            else
            {
                vstr = CreateFeatureInfoPlain(map, requestLayers, @params.X, @params.Y, @params.FeatureCount, @params.CqlFilter, context);
                //string.Empty is the result if a return WmsParams.Failure(...) has been called
                if (vstr == String.Empty)
                    return;
                context.ContentType = "text/plain";
            }
            context.Clear();
            if (WmsServer.FeatureInfoResponseEncoding != null)
            {
                //"windows-1252";
                context.Charset = WmsServer.FeatureInfoResponseEncoding.WebName;
            }
            context.Write(vstr);
            context.End();
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
        /// <param name="context">The <see cref="HttpContext"/> to use. If not specified or <value>null</value>, <see cref="HttpContext.Current"/> is used.</param>
        /// <exception cref="InvalidOperationException">Thrown if this function is used without a valid <see cref="HttpContext"/> at hand</exception>
        /// <returns>Plain text string with featureinfo results</returns>
        private string CreateFeatureInfoPlain(Map map, string[] requestedLayers, float x, float y, int featureCount, string cqlFilter, IContext context)
        {
            string vstr = "GetFeatureInfo results: \n";
            foreach (string requestLayer in requestedLayers)
            {
                bool found = false;
                foreach (ILayer mapLayer in map.Layers)
                {
                    if (String.Equals(mapLayer.LayerName, requestLayer,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        ICanQueryLayer queryLayer = mapLayer as ICanQueryLayer;
                        if (queryLayer == null || !queryLayer.IsQueryEnabled) continue;

                        float queryBoxMinX = x - (WmsServer.PixelSensitivity);
                        float queryBoxMinY = y - (WmsServer.PixelSensitivity);
                        float queryBoxMaxX = x + (WmsServer.PixelSensitivity);
                        float queryBoxMaxY = y + (WmsServer.PixelSensitivity);

                        Coordinate minXY = map.ImageToWorld(new PointF(queryBoxMinX, queryBoxMinY));
                        Coordinate maxXY = map.ImageToWorld(new PointF(queryBoxMaxX, queryBoxMaxY));
                        Envelope queryBox = new Envelope(minXY, maxXY);
                        FeatureDataSet fds = new FeatureDataSet();
                        queryLayer.ExecuteIntersectionQuery(queryBox, fds);

                        if (WmsServer.IntersectDelegate != null)
                        {
                            fds.Tables[0] = WmsServer.IntersectDelegate(fds.Tables[0], queryBox);
                        }
                        if (fds.Tables.Count == 0)
                        {
                            vstr = vstr + "\nSearch returned no results on layer: " + requestLayer;
                        }
                        else
                        {
                            if (fds.Tables[0].Rows.Count == 0)
                            {
                                vstr = vstr + "\nSearch returned no results on layer: " + requestLayer + " ";
                            }
                            else
                            {
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
                                //if featurecount < fds...count, select smallest bbox, because most likely to be clicked
                                vstr = vstr + "\n Layer: '" + requestLayer + "'\n Featureinfo:\n";
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
                                        vstr = vstr + " '" + fds.Tables[0].Rows[keys[k]].ItemArray[j] + "'";
                                    }
                                    if ((k + 1) < featureCount)
                                        vstr = vstr + ",\n";
                                }
                            }
                        }

                    }
                }
                if (found == false)
                {
                    throw new WmsLayerNotDefinedException(requestLayer);
                }
            }
            return vstr;
        }

        /// <summary>
        /// Gets FeatureInfo as GeoJSON
        /// </summary>
        /// <param name="map">The map to create the feature info from</param>
        /// <param name="requestedLayers">The layers to create the feature info for</param>
        /// <param name="x">The x-Ordinate</param>
        /// <param name="y">The y-Ordinate</param>
        /// <param name="cqlFilter">The CQL Filter string</param>
        /// <param name="context">The <see cref="HttpContext"/> to use. If not specified or <value>null</value>, <see cref="HttpContext.Current"/> is used.</param>
        /// <exception cref="InvalidOperationException">Thrown if this function is used without a valid <see cref="HttpContext"/> at hand</exception>
        /// <returns>GeoJSON string with featureinfo results</returns>
        private string CreateFeatureInfoGeoJSON(Map map, string[] requestedLayers, float x, float y, string cqlFilter, IContext context)
        {
            List<GeoJSON> items = new List<Converters.GeoJSON.GeoJSON>();
            foreach (string requestLayer in requestedLayers)
            {
                bool found = false;
                foreach (ILayer mapLayer in map.Layers)
                {
                    if (String.Equals(mapLayer.LayerName, requestLayer,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        ICanQueryLayer queryLayer = mapLayer as ICanQueryLayer;
                        if (queryLayer == null || !queryLayer.IsQueryEnabled) continue;

                        float queryBoxMinX = x - (WmsServer.PixelSensitivity);
                        float queryBoxMinY = y - (WmsServer.PixelSensitivity);
                        float queryBoxMaxX = x + (WmsServer.PixelSensitivity);
                        float queryBoxMaxY = y + (WmsServer.PixelSensitivity);
                        Coordinate minXY = map.ImageToWorld(new PointF(queryBoxMinX, queryBoxMinY));
                        Coordinate maxXY = map.ImageToWorld(new PointF(queryBoxMaxX, queryBoxMaxY));
                        Envelope queryBox = new Envelope(minXY, maxXY);
                        FeatureDataSet fds = new FeatureDataSet();
                        queryLayer.ExecuteIntersectionQuery(queryBox, fds);
                        //
                        if (WmsServer.IntersectDelegate != null)
                        {
                            fds.Tables[0] = WmsServer.IntersectDelegate(fds.Tables[0], queryBox);
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
                if (found == false)
                {
                    throw new WmsLayerNotDefinedException(requestLayer);
                }
            }
            StringWriter writer = new StringWriter();
            GeoJSONWriter.Write(items, writer);
            return writer.ToString();
        }
    }
}
