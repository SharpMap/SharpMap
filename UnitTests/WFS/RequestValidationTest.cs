using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Schema;
using System.Xml;
using System.Collections.Specialized;
using NUnit.Framework;
using SharpMap.Utilities.Wfs;

namespace UnitTests.WFS
{
    [TestFixture]
    [Ignore("Tests disabled because they download external schemas from the internet")]
    public class RequestValidationTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        [Test]
        public void TestGETFilter1_1_0()
        {
            var featureTypeInfo = new WfsFeatureTypeInfo("http://localhost/", "nsPrefix", "featureTypeNamespace", "featureType", "geometryName", GeometryTypeEnum.PointPropertyType);

            WFS_1_1_0_TextResources wfs = new WFS_1_1_0_TextResources();
            string querystring = wfs.GetFeatureGETRequest(featureTypeInfo, new GeoAPI.Geometries.Envelope(1, 2, 3, 4), null, true);

            NameValueCollection qscoll = ParseQueryString(querystring);

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.Schemas.Add("http://www.opengis.net/ogc", "http://schemas.opengis.net/filter/1.1.0/filter.xsd");
            readerSettings.Schemas.Add("http://www.opengis.net/ogc", "http://schemas.opengis.net/filter/1.1.0/expr.xsd");
            readerSettings.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandler);

            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(qscoll["FILTER"]));
            
            XmlTextReader xmlReader = new XmlTextReader(ms);
            XmlReader objXmlReader = XmlReader.Create(xmlReader, readerSettings);
            while (objXmlReader.Read()) { }
        }

        [Test]
        public void TestPOSTFilter1_1_0()
        {
            var featureTypeInfo = new WfsFeatureTypeInfo("http://localhost/", "nsPrefix", "featureTypeNamespace", "featureType", "geometryName", GeometryTypeEnum.PointPropertyType);

            WFS_1_1_0_TextResources wfs = new WFS_1_1_0_TextResources();
            byte[] request = wfs.GetFeaturePOSTRequest(featureTypeInfo, "", new GeoAPI.Geometries.Envelope(1, 2, 3, 4), null, true);

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.Schemas.Add("http://www.opengis.net/wfs", "http://schemas.opengis.net/wfs/1.1.0/wfs.xsd");
            readerSettings.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandler);

            MemoryStream ms = new MemoryStream(request);

            XmlTextReader xmlReader = new XmlTextReader(ms);
            XmlReader objXmlReader = XmlReader.Create(xmlReader, readerSettings);
            while (objXmlReader.Read()) { }
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                Assert.Fail("Validation error: " + args.Message);
            }
            else if (args.Severity == XmlSeverityType.Warning)
            {
                Assert.Fail("Validation warning: " + args.Message);
            }
        }

        private static NameValueCollection ParseQueryString(string s)
        {
            NameValueCollection queryParameters = new NameValueCollection();
            s = s.TrimStart('?');
            string[] querySegments = s.Split('&');
            foreach (string segment in querySegments)
            {
                int equalIndex = segment.IndexOf('=');
                if (equalIndex > 0)
                {
                    string key = segment.Substring(0, equalIndex);
                    string val = Uri.UnescapeDataString(segment.Substring(equalIndex + 1));

                    queryParameters.Add(key, val);
                }
            }

            return queryParameters;
        }
    }
}
