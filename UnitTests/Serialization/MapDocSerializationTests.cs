using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Xml.Serialization;
using SharpMap.Serialization;
using System.IO;
using System.Net;
using System.Resources;
using SharpMap.Layers;

namespace UnitTests.Serialization
{
    [TestFixture]
    class MapDocSerializationTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            try
            {
                var l = new SharpMap.Layers.WmsLayer("testwms",
                    "http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer");
                l.Dispose();
            }
            catch (Exception e)
            {
                throw new IgnoreException("Creation of WMS Layer failed", e);

            }
        }

        [Test]
        [Ignore("Xml somehow different than on Windows")]
        public void TestSerializeWmsLayer()
        {
            SharpMap.Map m = new SharpMap.Map();
            SharpMap.Layers.WmsLayer l = new SharpMap.Layers.WmsLayer("testwms", "http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer");
            l.AddChildLayers(l.RootLayer, false);
            m.Layers.Add(l);
            MemoryStream ms = new MemoryStream();
            SharpMap.Serialization.MapSerialization.SaveMapToStream(m, ms);
            string txt = Encoding.ASCII.GetString(ms.ToArray());
            txt = txt.Replace("\r\n", "");
            System.Diagnostics.Trace.WriteLine(txt);
            Assert.IsTrue(txt.Contains(@"<Layers><MapLayer xsi:type=""WmsLayer"">
      <Name>testwms</Name>
      <MinVisible>0</MinVisible>
      <MaxVisible>1.7976931348623157E+308</MaxVisible>
      <OnlineURL>http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer?SERVICE=WMS&amp;REQUEST=GetCapabilities&amp;</OnlineURL>
      <WmsLayers>0,1,2</WmsLayers>
    </MapLayer>
  </Layers>"));
            ms.Close();
        }

        [Test]
        public void TestDeSerializeWmsLayer()
        {

            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(@"<?xml version=""1.0""?>
<MapDefinition xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BackGroundColor>Transparent</BackGroundColor>
  <Extent>
    <Xmin>-0.5</Xmin>
    <Ymin>-0.375</Ymin>
    <Xmax>0.5</Xmax>
    <Ymax>0.375</Ymax>
  </Extent>
  <Layers>
    <MapLayer xsi:type=""WmsLayer"">
      <Name>testwms</Name>
      <MinVisible>0</MinVisible>
      <MaxVisible>1.7976931348623157E+308</MaxVisible>
      <OnlineURL>http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer?SERVICE=WMS&amp;REQUEST=GetCapabilities&amp;</OnlineURL>
    </MapLayer>
  </Layers>
  <SRID>4326</SRID>
</MapDefinition>
"));
            SharpMap.Map m = SharpMap.Serialization.MapSerialization.LoadMapFromStream(ms);
            Assert.AreEqual(4326, m.SRID);
            Assert.AreEqual(1, m.Layers.Count);
            Assert.IsTrue(m.Layers[0] is SharpMap.Layers.WmsLayer);
            Assert.AreEqual("http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer?SERVICE=WMS&REQUEST=GetCapabilities&", (m.Layers[0] as SharpMap.Layers.WmsLayer).CapabilitiesUrl);
            ms.Close();
        }

        [Test]
        public void TestDeSerializeWmsLayerWithBaseURL()
        {

            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(@"<?xml version=""1.0""?>
<MapDefinition xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BackGroundColor>Transparent</BackGroundColor>
  <Extent>
    <Xmin>-0.5</Xmin>
    <Ymin>-0.375</Ymin>
    <Xmax>0.5</Xmax>
    <Ymax>0.375</Ymax>
  </Extent>
  <Layers>
    <MapLayer xsi:type=""WmsLayer"">
      <Name>testwms</Name>
      <MinVisible>0</MinVisible>
      <MaxVisible>1.7976931348623157E+308</MaxVisible>
      <OnlineURL>http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer</OnlineURL>
    </MapLayer>
  </Layers>
  <SRID>4326</SRID>
</MapDefinition>
"));
            SharpMap.Map m = SharpMap.Serialization.MapSerialization.LoadMapFromStream(ms);
            Assert.AreEqual(4326, m.SRID);
            Assert.AreEqual(1, m.Layers.Count);
            Assert.IsTrue(m.Layers[0] is SharpMap.Layers.WmsLayer);
            Assert.AreEqual("http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer?SERVICE=WMS&REQUEST=GetCapabilities&", (m.Layers[0] as SharpMap.Layers.WmsLayer).CapabilitiesUrl);
            ms.Close();
        }

        [Test]
        public void TestDeSerializeWmsLayerWithCredentials()
        {

            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(@"<?xml version=""1.0""?>
<MapDefinition xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BackGroundColor>Transparent</BackGroundColor>
  <Extent>
    <Xmin>-0.5</Xmin>
    <Ymin>-0.375</Ymin>
    <Xmax>0.5</Xmax>
    <Ymax>0.375</Ymax>
  </Extent>
  <Layers>
    <MapLayer xsi:type=""WmsLayer"">
      <Name>testwms</Name>
      <MinVisible>0</MinVisible>
      <MaxVisible>1.7976931348623157E+308</MaxVisible>
      <OnlineURL>http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer</OnlineURL>
      <WmsUser>test</WmsUser>
      <WmsPassword>pw</WmsPassword>
    </MapLayer>
  </Layers>
  <SRID>4326</SRID>
</MapDefinition>
"));
            SharpMap.Map m = SharpMap.Serialization.MapSerialization.LoadMapFromStream(ms);
            Assert.AreEqual(4326, m.SRID, "m.SRID");
            Assert.AreEqual(1, m.Layers.Count, "m.Layers.Count");
            Assert.IsTrue(m.Layers[0] is SharpMap.Layers.WmsLayer, "m.Layers[0] is WmsLayer");
            Assert.AreEqual("http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer?SERVICE=WMS&REQUEST=GetCapabilities&", (m.Layers[0] as SharpMap.Layers.WmsLayer).CapabilitiesUrl, "m.Layers[0].CapabilitiesUrl");
            Assert.IsNotNull((m.Layers[0] as SharpMap.Layers.WmsLayer).Credentials, "m.Layers[0].Credentials != null");
            ICredentials c = null;
            Assert.DoesNotThrow(() => c = ((SharpMap.Layers.WmsLayer) m.Layers[0]).Credentials);
            Assert.IsTrue(c is NetworkCredential);
            var nc = (NetworkCredential)c;
            Assert.AreEqual("test", nc.UserName, "nc.UserName");
            Assert.AreEqual("pw", nc.Password, "nc.Password");
            ms.Close();
        }


        [Test]
#if LINUX
        [Ignore("Xml somehow different than on Windows")]
#endif
        public void TestSerializeWmsLayerWithCredentials()
        {
            SharpMap.Map m = new SharpMap.Map();
            SharpMap.Layers.WmsLayer l = new SharpMap.Layers.WmsLayer("testwms", "http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer", TimeSpan.MaxValue, 
                System.Net.WebRequest.DefaultWebProxy, new NetworkCredential("test", "pw"));
            l.AddChildLayers(l.RootLayer, false);
            m.Layers.Add(l);
            MemoryStream ms = new MemoryStream();
            SharpMap.Serialization.MapSerialization.SaveMapToStream(m, ms);
            string txt = System.Text.ASCIIEncoding.ASCII.GetString(ms.ToArray());
            System.Diagnostics.Trace.WriteLine(txt);
            Assert.IsTrue(txt.Contains(@"<Layers>
    <MapLayer xsi:type=""WmsLayer"">
      <Name>testwms</Name>
      <MinVisible>0</MinVisible>
      <MaxVisible>1.7976931348623157E+308</MaxVisible>
      <OnlineURL>http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer?SERVICE=WMS&amp;REQUEST=GetCapabilities&amp;</OnlineURL>
      <WmsUser>test</WmsUser>
      <WmsPassword>pw</WmsPassword>
      <WmsLayers>0,1,2</WmsLayers>
    </MapLayer>
  </Layers>"));
            ms.Close();
        }
    }
}
