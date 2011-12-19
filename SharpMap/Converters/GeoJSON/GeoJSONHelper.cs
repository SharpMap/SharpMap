namespace SharpMap.Converters.GeoJSON
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Data;
    using Geometries;

    public static class GeoJSONHelper
    {
        public static IEnumerable<GeoJSON> GetData(FeatureDataSet data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            using (data)
            {
                foreach (FeatureDataTable table in data.Tables)
                {
                    DataColumnCollection columns = table.Columns;
                    string[] keys = new string[columns.Count];
                    for (int i = 0; i < columns.Count; i++)
                        keys[i] = columns[i].ColumnName;

                    DataRowCollection rows = table.Rows;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        FeatureDataRow row = (FeatureDataRow)rows[i];
                        Geometry geometry = row.Geometry;
                        Dictionary<string, object> values = new Dictionary<string, object>();
                        for (int j = 0; j < keys.Length; j++)
                            values.Add(keys[j], row[j]);
                        yield return new GeoJSON(geometry, values);
                    }
                }
            }
        }        
    }
}