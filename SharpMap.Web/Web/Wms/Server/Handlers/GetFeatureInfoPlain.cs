using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Layers;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoPlain : GetFeatureInfo
    {
        public GetFeatureInfoPlain(Capabilities.WmsServiceDescription description,
            int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate, Encoding encoding) :
            base(description, pixelSensitivity, intersectDelegate, encoding) { }

        protected override string CreateFeatureInfo(Map map, 
            IEnumerable<string> requestedLayers, 
            float x, float y, 
            int featureCount, 
            string cqlFilter,
            int pixelSensitivity, 
            WmsServer.InterSectDelegate intersectDelegate)
        {
            StringBuilder sb = new StringBuilder("GetFeatureInfo results: \n");
            foreach (string requestLayer in requestedLayers)
            {
                ICanQueryLayer layer = GetQueryLayer(map, requestLayer);
                FeatureDataSet fds;
                if (!TryGetData(map, x, y, pixelSensitivity, intersectDelegate, layer, cqlFilter, out fds))
                {
                    sb.AppendFormat("Search returned no results on layer: {0}", requestLayer);
                    continue;
                }

                sb.AppendFormat("\n Layer: '{0}'\n Featureinfo:\n", requestLayer);
                FeatureDataTable table = fds.Tables[0];
                sb.Append(GetText(table, featureCount));
            }
            return sb.ToString();
        }
        
        private string GetText(FeatureDataTable table, int maxFeatures)
        {
            // if featurecount < fds...count, select smallest bbox, because most likely to be clicked
            DataRowCollection rows = table.Rows;
            int[] keys = new int[rows.Count];
            double[] area = new double[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                FeatureDataRow row = (FeatureDataRow)rows[i];
                IGeometry geometry = row.Geometry;
                Envelope envelope = geometry.EnvelopeInternal;
                area[i] = envelope.Area;
                keys[i] = i;
            }
            Array.Sort(area, keys);

            if (rows.Count < maxFeatures)
                maxFeatures = rows.Count;

            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < maxFeatures; k++)
            {
                int i = keys[k];
                object[] arr = rows[i].ItemArray;
                foreach (object t in arr)
                    sb.AppendFormat(" '{0}'", t);
                if ((k + 1) < maxFeatures)
                    sb.Append(",\n");
            }
            return sb.ToString();
        }
    }
}