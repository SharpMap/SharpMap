using GeoAPI.Features;
using NUnit.Framework;


namespace SharpMap.Features
{
    using IntFeatureFactory = FeatureFactory<int>;
    using IntFeatureCollection = FeatureCollection<int>;
    
    [TestFixture]
    public class FeatureTest
    {
        private readonly IntFeatureFactory _featureFactory;
        private readonly IntFeatureCollection _featureCollection;


        public FeatureTest()
        {
            var eg = new EntityOidGenerator<int>(-1, 1000, (t) => t + 1);
            _featureFactory = new IntFeatureFactory(eg, 
                NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326),
                new FeatureAttributeDefinition { AttributeName = "Attribute1", AttributeDescription = "1st Attribute", AttributeType = typeof(int) },
                new FeatureAttributeDefinition { AttributeName = "Attribute2", AttributeDescription = "2nd Attribute", AttributeType = typeof(double) },
                new FeatureAttributeDefinition { AttributeName = "Attribute3", AttributeDescription = "3rd Attribute", AttributeType = typeof(string) }
                );

            _featureCollection = new IntFeatureCollection(_featureFactory);
        }

        [Test]
        public void TestCreate()
        {
            var f = _featureCollection.Factory.Create();
            Assert.IsNotNull(f);
            var fg = f as IFeature<int>;
            Assert.IsNotNull(fg);

            Assert.IsTrue(f.Geometry == null);
            Assert.IsTrue(f.Attributes != null);
            Assert.IsTrue(fg.Oid == _featureFactory.UnassignedOid);
            Assert.IsTrue((int)f.Attributes[0] == -1);
            
            Assert.IsTrue(f.Attributes[2] == null);
            Assert.IsTrue(f.Attributes["Attribut2"] == null);

            f.Attributes[2] = "Test";
            Assert.IsTrue((string)f.Attributes[2] == "Test");
            Assert.IsTrue((string)f.Attributes["Attribut2"] == "Test");
        }
    }
}