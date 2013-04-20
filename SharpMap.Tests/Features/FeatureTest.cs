using System;
using GeoAPI.Features;
using NUnit.Framework;


namespace SharpMap.Features
{
    using IntFeatureFactory = FeatureFactory<int>;
    using IntFeatureCollection = FeatureCollection<int>;
    
    [TestFixture]
    public abstract class FeatureTest<TEntity> where TEntity : struct, IComparable<TEntity>, IEquatable<TEntity>
    {
        
        private readonly FeatureFactory<TEntity> _featureFactory;
        private readonly FeatureCollection<TEntity> _featureCollection;

        protected FeatureTest(FeatureFactory<TEntity> featureFactory)
        {
            _featureFactory = featureFactory;
            _featureCollection = new FeatureCollection<TEntity>(featureFactory);
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
            Assert.IsTrue(fg.Oid.Equals(_featureFactory.UnassignedOid));
            Assert.IsTrue((int)f.Attributes[0] == -1);
            
            Assert.IsTrue(f.Attributes[2] == null);
            Assert.IsTrue(f.Attributes["Attribut2"] == null);

            f.Attributes[2] = "Test";
            Assert.IsTrue((string)f.Attributes[2] == "Test");
            Assert.IsTrue((string)f.Attributes["Attribut2"] == "Test");
        }
    }

    public class Int32FeatureTest : FeatureTest<int>
    {
        public Int32FeatureTest()
            : base(FeatureFactory.CreateInt32(NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326),
                                              new FeatureAttributeDefinition
                                                  {
                                                      AttributeName = "Attribute1",
                                                      AttributeDescription = "1st Attribute",
                                                      AttributeType = typeof (int)
                                                  },
                                              new FeatureAttributeDefinition
                                                  {
                                                      AttributeName = "Attribute2",
                                                      AttributeDescription = "2nd Attribute",
                                                      AttributeType = typeof (double)
                                                  },
                                              new FeatureAttributeDefinition
                                                  {
                                                      AttributeName = "Attribute3",
                                                      AttributeDescription = "3rd Attribute",
                                                      AttributeType = typeof (string)
                                                  }))
        {
        }
    }

    public class Int64FeatureTest : FeatureTest<long>
    {
        public Int64FeatureTest()
            : base(FeatureFactory.CreateInt64(NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326),
                new FeatureAttributeDefinition { AttributeName = "Attribute1", AttributeDescription = "1st Attribute", AttributeType = typeof(int) },
                new FeatureAttributeDefinition { AttributeName = "Attribute2", AttributeDescription = "2nd Attribute", AttributeType = typeof(double) },
                new FeatureAttributeDefinition { AttributeName = "Attribute3", AttributeDescription = "3rd Attribute", AttributeType = typeof(string) } ))
        {
        }
    }
}