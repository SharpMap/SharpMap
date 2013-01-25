namespace ExampleCodeSnippets
{
    public class ReadXmlExample
    {
        /// <summary>
        /// Creates an enumeration of <see cref="GeoAPI.Geometries.Coordinate"/>s from an xml string
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="xml">the xml string</param>
        /// <returns>Coordinates</returns>
        public static System.Collections.Generic.IEnumerable<GeoAPI.Geometries.IGeometry> PointsFromXml(
            GeoAPI.Geometries.IGeometryFactory factory,
            System.IO.Stream xml)
        {
            foreach (var coordinate in CoordinatesFromXml(xml))
                yield return factory.CreatePoint(coordinate);
        }

        /// <summary>
        /// Creates an enumeration of <see cref="GeoAPI.Geometries.Coordinate"/>s from an xml string
        /// </summary>
        /// <param name="xml">the xml string</param>
        /// <returns>Coordinates</returns>
        public static System.Collections.Generic.IEnumerable<GeoAPI.Geometries.Coordinate> CoordinatesFromXml(System.IO.Stream xml)
        {
            var reader = System.Xml.XmlReader.Create(xml);
            var doc = System.Xml.Linq.XDocument.Load(reader);
            if (doc.Root == null) yield break;

            var ptsName = System.Xml.Linq.XName.Get("Points");
            var ptName = System.Xml.Linq.XName.Get("Point");
            var xName = System.Xml.Linq.XName.Get("X");
            var yName = System.Xml.Linq.XName.Get("Y");

            foreach (var ptsElement in doc.Root.Elements(ptsName))
            {
                foreach (var ptElement in ptsElement.Elements(ptName))
                {

                    var element = ptElement.Element(xName);
                    if (element == null || element.IsEmpty) continue;
                    var x = double.Parse(element.Value,
                                         System.Globalization.NumberFormatInfo.InvariantInfo);

                    element = ptElement.Element(yName);
                    if (element == null || element.IsEmpty) continue;
                    var y = double.Parse(element.Value,
                                         System.Globalization.NumberFormatInfo.InvariantInfo);

                    yield return new GeoAPI.Geometries.Coordinate(x, y);
                }
            }
        }
        
        [NUnit.Framework.Test]
        public void TestXml1()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<root>
  <Points>
    <Point>
    <X>13457786.5961983</X>
    <Y>1629064.58490612</Y>
    </Point>
  </Points>
</root>";
            var xmlFileName = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "xml");
            
            using (var sw = new System.IO.StreamWriter(System.IO.File.OpenWrite(xmlFileName)))
                sw.Write(xml);

            var factory = new NetTopologySuite.Geometries.GeometryFactory();

            SharpMap.Data.Providers.GeometryProvider p = null;
            using (var fs = System.IO.File.OpenRead(xmlFileName))
            {
                p = new SharpMap.Data.Providers.GeometryProvider(PointsFromXml(factory, fs));
                NUnit.Framework.Assert.IsNotNull(p);
                NUnit.Framework.Assert.AreEqual(1, p.Geometries.Count);
            }

            System.IO.File.Delete(xmlFileName);
        }
    }
}