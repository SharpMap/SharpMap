using System.Linq;
using GeoAPI.Features;

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
        public static IEnumerable<GeoJSON> GetData(IFeatureCollectionSet data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            foreach (var table in data)
            {
                var columns = table.AttributesDefinition;
                var keys = new string[columns.Count];
                for (var i = 0; i < columns.Count; i++)
                    keys[i] = columns[i].AttributeName;

                var rows = table.ToList();
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var geometry = row.Geometry;
                    var values = new Dictionary<string, object>();
                    for (var j = 0; j < keys.Length; j++)
                        values.Add(keys[j], row.Attributes[j]);
                    yield return new GeoJSON(geometry, values);
                }
            }
        }        
    }
}