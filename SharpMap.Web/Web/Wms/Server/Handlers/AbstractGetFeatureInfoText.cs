using System;
using System.Data;
using GeoAPI.Geometries;
using SharpMap.Data;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public abstract class AbstractGetFeatureInfoText : GetFeatureInfo
    {
        protected AbstractGetFeatureInfoText(Capabilities.WmsServiceDescription description) : 
            base(description) { }

        protected AbstractGetFeatureInfoText(Capabilities.WmsServiceDescription description,
            GetFeatureInfoParams @params) : base(description, @params) { }

        protected string GetRowsText(DataRowCollection rows, int maxFeatures)
        {
            // if featurecount < fds...count, select smallest bbox, because most likely to be clicked
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

            return FormatRows(rows, keys, maxFeatures);
        }

        protected abstract string FormatRows(DataRowCollection rows, int[] keys, int maxFeatures);
    }
}