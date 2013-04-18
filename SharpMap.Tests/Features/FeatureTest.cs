using NUnit.Framework;

namespace SharpMap.Features
{
    [TestFixture]
    public class FeatureTest
    {
        private readonly FeatureFactory _featureFactory;
        private readonly FeatureCollection _featureCollection;

        public FeatureTest()
        {
            _featureFactory = new FeatureFactory(
                NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326),
                "Attribut1", "Attribut2", "Attribut3");

            _featureCollection = new FeatureCollection(_featureFactory);
        }

        [Test]
        public void TestCreate()
        {
            var f = _featureCollection.Factory.Create();
            
            Assert.IsTrue(f.Geometry == null);
            Assert.IsTrue(f.Attributes != null);
            Assert.IsTrue(f.Oid == -1);
            Assert.IsTrue((int)f.Attributes[0] == -1);
            
            Assert.IsTrue(f.Attributes[2] == null);
            Assert.IsTrue(f.Attributes["Attribut2"] == null);

            f.Attributes[2] = "Test";
            Assert.IsTrue((string)f.Attributes[2] == "Test");
            Assert.IsTrue((string)f.Attributes["Attribut2"] == "Test");
        }
    }
}