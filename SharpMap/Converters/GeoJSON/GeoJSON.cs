using GeoAPI;

namespace SharpMap.Converters.GeoJSON
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using GeoAPI.Geometries;
    using Newtonsoft.Json;
    //ReSharper disable InconsistentNaming
    /// <summary>
    /// GeoJSON converter class
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptOut, Description = "featureType")]
    public class GeoJSON
    {
        /// <summary>
        /// Get an empty GeoJSON object
        /// </summary>
        public static GeoJSON Empty
        {
            get
            {
                var factory = GeometryServiceProvider.Instance.CreateGeometryFactory();
                var geometry =  factory.CreateGeometryCollection(new IGeometry[0]);

                var dictionary = new Dictionary<string, object>();
                return new GeoJSON(geometry, dictionary);
            }
        }

        /// <summary>
        /// Gets the <see cref="IGeometry"/> object
        /// </summary>
        public IGeometry Geometry { get; private set; }

        /// <summary>
        /// Gets the attributes
        /// </summary>
        public IDictionary<string, object> Values { get; private set; }

        /// <summary>
        /// Creates an instance of this class from <paramref name="geometry"/> and <paramref name="values"/>.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="values">The attribute values</param>
        public GeoJSON(IGeometry geometry, IDictionary<string, object> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            Geometry = geometry;
            Values = values;
        }

        /// <summary>
        /// Method to set <see cref="Geometry"/> to <paramref name="converted"/>
        /// </summary>
        /// <param name="converted">The converted geometry</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="converted"/> is <value>null</value></exception>
        public void SetGeometry(IGeometry converted)
        {
            if (converted == null)
                throw new ArgumentNullException("converted");

            Geometry = converted;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var text = Values.Aggregate(new StringBuilder("|"), (sb, i) =>
            {
                sb.AppendFormat("'{0}'-'{1}'|", i.Key, i.Value);
                return sb;
            });
            return String.Format("Geometry: {0}, Values: {1}", Geometry, text);
        }
    }
}
