using System.Collections.Generic;
using System.Linq;
using GeoAPI.Features;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Features;
using SharpMap.Layers;
using UnitTests.WMS.Server;

namespace UnitTests.Data
{
    [TestFixture]
    public class FeatureDataSetTests : AbstractFixture
    {
        [Test]
        public void linq_should_work_with_feature_collections()
        {
            Envelope world = new Envelope(-180, 180, -90, 90);

            ILayer layer = Map.GetLayerByName("poly_landmarks");
            Assert.That(layer, Is.Not.Null);
            Assert.That(layer, Is.InstanceOf<ICanQueryLayer>());

            IFeatureCollectionSet data = new FeatureCollectionSet();
            ICanQueryLayer query = (ICanQueryLayer)layer;
            query.ExecuteIntersectionQuery(world, data);

            foreach (IFeatureCollection table in data)
            {
                Assert.That(table, Is.Not.Null);
                Assert.That(table, Is.Not.Empty);                
                IList<IFeature> list = table.ToList();
                Assert.That(list, Is.Not.Null);
            }
        }
    }
}