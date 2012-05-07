namespace SharpMap.Converters.GeoJSON
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using GeoAPI.Geometries;
    using Newtonsoft.Json;

    public class GeoJSONWriter
    {
        private static JsonTextWriter CreateWriter(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            return new JsonTextWriter(writer) { Formatting = Formatting.None };
        }

        private static string GetOrCreateUniqueId(IDictionary<string, object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");

            string masterKey = null;
            foreach (string name in attributes.Keys)
            {
                if ((!String.Equals(name, "layer", StringComparison.InvariantCultureIgnoreCase) &&
                     !String.Equals(name, "layerName", StringComparison.InvariantCultureIgnoreCase)) &&
                     !String.Equals(name, "layer_name", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                masterKey = name;
                break;
            }

            string idKey = null;
            foreach (string name in attributes.Keys)
            {
                if ((!String.Equals(name, "id", StringComparison.InvariantCultureIgnoreCase) &&
                     !String.Equals(name, "fid", StringComparison.InvariantCultureIgnoreCase)) &&
                     !String.Equals(name, "oid", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                idKey = name;
                break;
            }

            object master = masterKey != null ? attributes[masterKey] : null;
            if (master == null)
                return Guid.NewGuid().ToString();

            string id = String.IsNullOrEmpty(idKey) ?
                Guid.NewGuid().ToString() :
                Convert.ToString(attributes[idKey], CultureInfo.InvariantCulture);
            return String.Format("{0}_{1}", master, id);
        }

        public static void WriteCoord(Coordinate coordinate, JsonTextWriter writer)
        {
            if (coordinate == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartArray();
            writer.WriteValue(coordinate.X);
            writer.WriteValue(coordinate.Y);
            writer.WriteEndArray();
        }

        public static void WriteCoord(Coordinate coordinate, TextWriter writer)
        {
            if (coordinate == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            JsonTextWriter jwriter = CreateWriter(writer);
            WriteCoord(coordinate, jwriter);
        }

        public static void WriteCoord(IEnumerable<Coordinate> coordinates, JsonTextWriter writer)
        {
            if (coordinates == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartArray();
            foreach (var entry in coordinates)
                WriteCoord(entry, writer);
            writer.WriteEndArray();
        }

        public static void WriteCoord(IEnumerable<Coordinate> coordinates, TextWriter writer)
        {
            if (coordinates == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            JsonTextWriter jwriter = CreateWriter(writer);
            WriteCoord(coordinates, jwriter);
        }

        public static void Write(IGeometry geometry, JsonTextWriter writer)
        {
            if (geometry == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            if (geometry is IPoint)
                Write(geometry as IPoint, writer);
            else if (geometry is ILineString)
                Write(geometry as ILineString, writer);
            else if (geometry is IPolygon)
                Write(geometry as IPolygon, writer);
            else if (geometry is IMultiPoint)
                Write(geometry as IMultiPoint, writer);
            else if (geometry is IMultiLineString)
                Write(geometry as IMultiLineString, writer);
            else if (geometry is IMultiPolygon)
                Write(geometry as IMultiPolygon, writer);
            else if (geometry is IGeometryCollection)
                Write(geometry as IGeometryCollection, writer);
        }

        public static void Write(IGeometry geometry, TextWriter writer)
        {
            if (geometry == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(geometry, CreateWriter(writer));
        }

        public static void Write(IPoint point, JsonTextWriter writer)
        {
            if (point == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("Point");

            writer.WritePropertyName("coordinates");
            writer.WriteStartArray();
            writer.WriteValue(point.X);
            writer.WriteValue(point.Y);
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        public static void Write(IPoint point, TextWriter writer)
        {
            if (point == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            JsonTextWriter jwriter = CreateWriter(writer);
            Write(point, jwriter);
        }

        public static void Write(IMultiPoint points, JsonTextWriter writer)
        {
            if (points == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("MultiPoint");
            writer.WritePropertyName("coordinates");
            /*
            writer.WriteStartArray();
            foreach (var entry in points.Coordinates)
                WriteCoord(entry, writer);
            writer.WriteEndArray();
             */
            WriteCoord(points.Coordinates, writer);
            writer.WriteEndObject();
        }

        public static void Write(IMultiPoint points, TextWriter writer)
        {
            if (points == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            JsonTextWriter jwriter = CreateWriter(writer);
            Write(points, jwriter);
        }

        public static void Write(ILineString line, JsonTextWriter writer)
        {
            if (line == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("LineString");
            writer.WritePropertyName("coordinates");
            /*writer.WriteStartArray();
            foreach (var entry in line.Coordinates)
                WriteCoord(entry, writer);
            writer.WriteEndArray();
             */
            WriteCoord(line.Coordinates, writer);
            writer.WriteEndObject();
        }

        public static void Write(ILineString line, TextWriter writer)
        {
            if (line == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(line, CreateWriter(writer));
        }

        public static void Write(IMultiLineString lines, JsonTextWriter writer)
        {
            if (lines == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("MultiLineString");
            writer.WritePropertyName("coordinates");
            writer.WriteStartArray();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = (ILineString)lines[i];
                WriteCoord(line.Coordinates, writer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void Write(IMultiLineString lines, TextWriter writer)
        {
            if (lines == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(lines, CreateWriter(writer));
        }

        public static void Write(IPolygon area, JsonTextWriter writer)
        {
            if (area == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("Polygon");
            writer.WritePropertyName("coordinates");
            writer.WriteStartArray();
            WriteCoord(area.ExteriorRing.Coordinates, writer);
            foreach (ILinearRing hole in area.InteriorRings)
                WriteCoord(hole.Coordinates, writer);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void Write(IPolygon area, TextWriter writer)
        {
            if (area == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(area, CreateWriter(writer));
        }

        public static void Write(IMultiPolygon areas, JsonTextWriter writer)
        {
            if (areas == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("MultiPolygon");
            writer.WritePropertyName("coordinates");
            writer.WriteStartArray();
            for ( var i = 0; i < areas.NumGeometries; i++)
            {
                var area = (IPolygon) areas[i];

                writer.WriteStartArray();
                WriteCoord(area.ExteriorRing.Coordinates, writer);
                foreach (ILinearRing hole in area.InteriorRings)
                    WriteCoord(hole.Coordinates, writer);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void Write(IMultiPolygon areas, TextWriter writer)
        {
            if (areas == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(areas, CreateWriter(writer));
        }

        public static void Write(IGeometryCollection geometries, JsonTextWriter writer)
        {
            if (geometries == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("GeometryCollection");
            writer.WritePropertyName("geometries");
            writer.WriteStartArray();
            for (var i = 0; i < geometries.NumGeometries; i++)
            {
                var geometry = geometries[i];
                Write(geometry, writer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void Write(IGeometryCollection geometries, TextWriter writer)
        {
            if (geometries == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(geometries, CreateWriter(writer));
        }

        public static void Write(GeoJSON feature, JsonTextWriter writer)
        {
            if (feature == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");
            
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("Feature");
            writer.WritePropertyName("id");
            string id = GetOrCreateUniqueId(feature.Values);
            writer.WriteValue(id);
            writer.WritePropertyName("properties");
            Write(feature.Values, writer);
            writer.WritePropertyName("geometry_name");
            writer.WriteValue("geometry");            
            writer.WritePropertyName("geometry");
            Write(feature.Geometry, writer);
            writer.WriteEndObject();
        }

        public static void Write(GeoJSON feature, TextWriter writer)
        {
            if (feature == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(feature, CreateWriter(writer));
        }

        public static void Write(IEnumerable<GeoJSON> features, JsonTextWriter writer)
        {
            if (features == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("FeatureCollection");
            writer.WritePropertyName("features");
            writer.WriteStartArray();
            foreach (GeoJSON feature in features)
                Write(feature, writer);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void Write(IEnumerable<GeoJSON> features, TextWriter writer)
        {
            if (features == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(features, CreateWriter(writer));
        }

        public static void Write(IDictionary<string, object> attributes, JsonTextWriter writer)
        {
            if (attributes == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid JSON writer object is required");

            writer.WriteStartObject();
            ICollection<string> names = attributes.Keys;
            foreach (string name in names)
            {
                writer.WritePropertyName(name);
                writer.WriteValue(attributes[name].ToString());
            }
            writer.WriteEndObject();
        }

        public static void Write(IDictionary<string, object> attributes, TextWriter writer)
        {
            if (attributes == null)
                return;
            if (writer == null)
                throw new ArgumentNullException("writer", "A valid text writer object is required");

            Write(attributes, CreateWriter(writer));
        }     
    }
}
