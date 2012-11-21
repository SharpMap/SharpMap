namespace SharpMap.Converters.GeoJSON
{
    using System;
    using System.Collections.Generic;
    using Data;

    /// <summary>
    /// GeoJSON helper class
    /// </summary>
    public static class GeoJSONHelper
    {
        /// <summary>
        /// Method to convert a <see cref="T:SharpMap.Data.FeatureDataSet"/> to a series of <see cref="GeoJSON"/> objects
        /// </summary>
        /// <param name="data">The feature dataset</param>
        /// <returns>A series of <see cref="GeoJSON"/> objects</returns>
        public static IEnumerable<GeoJSON> GetData(FeatureDataSet data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            using (data)
            {
                foreach (FeatureDataTable table in data.Tables)
                {
                    var columns = table.Columns;
                    var keys = new string[columns.Count];
                    for (var i = 0; i < columns.Count; i++)
                        keys[i] = columns[i].ColumnName;

                    var rows = table.Rows;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var row = (FeatureDataRow)rows[i];
                        var geometry = row.Geometry;
                        var values = new Dictionary<string, object>();
                        for (var j = 0; j < keys.Length; j++)
                            values.Add(keys[j], row[j]);
                        yield return new GeoJSON(geometry, values);
                    }
                }
            }
        }        
    }
}