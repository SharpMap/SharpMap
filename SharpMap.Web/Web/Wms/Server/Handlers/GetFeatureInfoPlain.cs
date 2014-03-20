using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using SharpMap.Data;
using SharpMap.Layers;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoPlain : AbstractGetFeatureInfoText
    {
        private const char NewLine = '\n';

        public GetFeatureInfoPlain(Capabilities.WmsServiceDescription description) :
            base(description) { }

        public GetFeatureInfoPlain(Capabilities.WmsServiceDescription description,
            GetFeatureInfoParams @params) : base(description, @params) { }

        protected override AbstractGetFeatureInfoResponse CreateFeatureInfo(Map map,
            IEnumerable<string> requestedLayers,
            float x, float y,
            int featureCount,
            string cqlFilter,
            int pixelSensitivity,
            WmsServer.InterSectDelegate intersectDelegate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("GetFeatureInfo results:{0}", NewLine);
            foreach (string requestLayer in requestedLayers)
            {
                ICanQueryLayer layer = GetQueryLayer(map, requestLayer);
                FeatureDataSet fds;
                if (!TryGetData(map, x, y, pixelSensitivity, intersectDelegate, layer, cqlFilter, out fds))
                {
                    sb.AppendFormat("Search returned no results on layer: {0}{1}", requestLayer, NewLine);
                    continue;
                }

                sb.AppendFormat("Layer: '{0}'{1}", requestLayer, NewLine);
                sb.AppendFormat("Featureinfo:{0}", NewLine);
                FeatureDataTable table = fds.Tables[0];
                string rowsText = GetRowsText(table.Rows, featureCount);
                sb.Append(rowsText).Append(NewLine);
            }
            return new GetFeatureInfoResponsePlain(sb.ToString());
        }

        protected override string FormatRows(DataRowCollection rows, int[] keys, int maxFeatures)
        {
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < maxFeatures; k++)
            {
                int key = keys[k];
                object[] arr = rows[key].ItemArray;
                int length = arr.Length;
                for (int i = 0; i < length; i++)
                {
                    string separator = (i == length - 1) ? String.Empty : " ";
                    sb.AppendFormat("'{0}'{1}", arr[i], separator);
                }
                if ((k + 1) < maxFeatures)
                    sb.AppendFormat(",{0}", NewLine);
            }
            return sb.ToString();
        }
    }
}