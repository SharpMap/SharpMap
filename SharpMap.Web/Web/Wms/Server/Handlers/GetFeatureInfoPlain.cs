using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Data;
using SharpMap.Layers;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoPlain : GetFeatureInfo
    {
        public GetFeatureInfoPlain(Capabilities.WmsServiceDescription description,
            int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate, Encoding encoding) :
                base(description, pixelSensitivity, intersectDelegate, encoding) { }

        protected override string CreateFeatureInfo(Map map, IEnumerable<string> requestedLayers, float x, float y, int featureCount, string cqlFilter,
            int pixelSensitivity, WmsServer.InterSectDelegate intersectDelegate)
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
    }
}