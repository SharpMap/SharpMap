namespace SharpMap.Converters.GeoJSON
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Geometries;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptOut, Description = "featureType")]
    public class GeoJSON
    {
        public static GeoJSON Empty
        {
            get
            {
                GeometryCollection geometry = new GeometryCollection();
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                return new GeoJSON(geometry, dictionary);
            }
        }

        public Geometry Geometry { get; private set; }

        public IDictionary<string, object> Values { get; private set; }

        public GeoJSON(Geometry geometry, IDictionary<string, object> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            this.Geometry = geometry;
            this.Values = values;
        }

        public void SetGeometry(Geometry converted)
        {
            if (converted == null)
                throw new ArgumentNullException("converted");

            this.Geometry = converted;
        }

        public override string ToString()
        {
            StringBuilder text = this.Values.Aggregate(new StringBuilder("|"), (sb, i) =>
            {
                sb.AppendFormat("'{0}'-'{1}'|", i.Key, i.Value);
                return sb;
            });
            return String.Format("Geometry: {0}, Values: {1}", this.Geometry, text);
        }
    }
}
