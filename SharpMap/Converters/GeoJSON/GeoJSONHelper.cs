using System;
using System.Collections.Generic;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Converters.GeoJSON
{
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

            foreach (IFeatureCollection table in data)
            {
                IList<IFeatureAttributeDefinition> columns = table.AttributesDefinition;
                string[] keys = new string[columns.Count];
                for (int i = 0; i < columns.Count; i++)
                    keys[i] = columns[i].AttributeName;

                foreach (IFeature row in table)
                {
                    IGeometry geometry = row.Geometry;
                    Dictionary<string, object> values = new Dictionary<string, object>();
                    for (int j = 0; j < keys.Length; j++)
                        values.Add(keys[j], row.Attributes[j]);
                    yield return new GeoJSON(geometry, values);
                }
            }
        }        
    }
}