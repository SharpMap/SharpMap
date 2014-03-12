using System;
using System.IO;
using System.Reflection;
using System.Xml;
using GeoAPI.CoordinateSystems;
using GeoAPI.SpatialReference;
using ProjNet.CoordinateSystems;

namespace SharpMap.SpatialReference
{
    public class ProjNetSpatialReferenceFactory : ISpatialReferenceFactory
    {
        private readonly CoordinateSystemFactory _factory = new CoordinateSystemFactory();

        private ICoordinateSystem CreateCoordinateSystem(string definition)
        {
            ICoordinateSystem cs = null;
            switch (definition)
            {
                case "EPSG:4326":
                    cs = GeographicCoordinateSystem.WGS84;
                    break;
                case "EPSG:3857":
                    cs = ProjectedCoordinateSystem.WebMercator;
                    break;
            }

            if (cs == null && definition.Contains(":"))
            {
                var parts = definition.Split(new[] { ':' }, 2);
                var wkt = GetWellKnownText(parts[0], parts[1]);
                if (!string.IsNullOrEmpty(wkt))
                {
                    cs = _factory.CreateFromWkt(definition);
                }
            }

            if (cs == null)
                cs = _factory.CreateFromWkt(definition);

            return cs;
        }
        
        public ISpatialReference Create(string definition)
        {
            return new ProjNetSpatialReference(CreateCoordinateSystem(definition));
        }

        public ISpatialReference Create(string oid, string definition)
        {
            return new ProjNetSpatialReference(oid, 
                CreateCoordinateSystem(definition));
        }

        private static string GetWellKnownText(string authority, string code)
        {
            var xmldoc = new XmlDocument();

            var file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) + "\\SpatialRefSys.xml";
            xmldoc.Load(file);
            XmlNode node = xmldoc.DocumentElement.SelectSingleNode("/SpatialReference/"+authority+"/ReferenceSystem[SRID='" + code + "']");
            if (node != null)
                return node.LastChild.InnerText;
            return "";
        }
    }
}