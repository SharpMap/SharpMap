using System.ComponentModel;
using GeoAPI.Features;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;

namespace SharpMap.Features.Poco
{
    [TestFixture]
    public class PoIFeatureTest
    {
        readonly IGeometryFactory _factory = new GeometryFactory();
        
        [Test]
        public void TestCreate()
        {
            var t = new PoIFeature();
            Assert.AreEqual(0, t.Oid, "Oid is not set to unassinged oid");
            Assert.IsTrue(string.IsNullOrEmpty(t.Name), "Assigned name is not null or empty");
            Assert.AreEqual(PoIKind.Undefined, t.Kind, "Assigned kind is not undefined");
            Assert.IsNull(t.Geometry);
            Assert.IsNull(t.GeometryFactory);

            var g = _factory.CreatePoint(new Coordinate(10, 10));
            t = new PoIFeature { Oid = 887, Kind = PoIKind.Restaurant, Name = "Test", Geometry = g};
            Assert.AreEqual(887, t.Oid, "Oid is not set to unassinged oid");
            Assert.AreEqual("Test", t.Name, "Assigned name is not null or correct");
            Assert.AreEqual(PoIKind.Restaurant, t.Kind, "Assigned kind is not undefined");
            Assert.IsNotNull(t.Geometry);
            Assert.IsNotNull(t.GeometryFactory);
        }

        [Test]
        public void TestClone()
        {
            var g = _factory.CreatePoint(new Coordinate(10, 10));
            var t1 = new PoIFeature { Oid = 887, Kind = PoIKind.Restaurant, Name = "Test", Geometry = g};
            var t2 = new PoIFeature(t1);

            Assert.AreEqual(t1.Oid, t2.Oid, "Oids are not equal.");
            Assert.AreEqual(t1.Kind, t2.Kind, "Kinds are not equal.");
            Assert.AreEqual(t1.Name, t2.Name, "Names are not equal.");
            Assert.AreEqual(t1.Geometry, t2.Geometry);
        }

        [Test]
        public void TestFeatureAttributes()
        {
            var g = _factory.CreatePoint(new Coordinate(10, 10));
            var t = new PoIFeature { Oid = 887, Kind = PoIKind.Restaurant, Name = "Test", Geometry = g };

            var attDef = t.AttributesDefinition;
            Assert.AreEqual(4, attDef.Count);

            IFeatureAttributes att = null;
            Assert.DoesNotThrow(() => att = ((IFeature)t).Attributes);

            Assert.AreEqual(t.Oid, (long)att["Oid"]);
            Assert.AreEqual(t.Kind, (PoIKind)att["Kind"]);
            Assert.AreEqual(t.Name, (string)att["Name"]);
            Assert.AreEqual(t.Geometry, (IGeometry)att["Geometry"]);

            g = _factory.CreatePoint(new Coordinate(20, 20));
            att["Geometry"] = g;

            Assert.AreEqual(g, t.Geometry);
        }

        private string _propertyName;

        [Test]
        public void TestNotifyPropertyChanged()
        {
            var g = _factory.CreatePoint(new Coordinate(10, 10));
            var t = new PoIFeature { Oid = 887, Kind = PoIKind.Restaurant, Name = "Test", Geometry = g };
            t.PropertyChanged += NotifyPropertyChangedHandler;

            Test(t.Attributes, "Oid", 887, false);
            Test(t.Attributes, "Oid", 886, true);
            Test(t.Attributes, "Name", "Test", false);
            Test(t.Attributes, "Name", "NewValue", true);
            Test(t.Attributes, "Kind", PoIKind.Restaurant, false);
            Test(t.Attributes, "Kind", PoIKind.Bar, true);
            Test(t.Attributes, "Geometry", g, false);
            Test(t.Attributes, "Geometry", _factory.CreatePoint(new Coordinate(20, 20)), true);
        }

        private void Test(IFeatureAttributes p, string property, object value, bool expectEvent)
        {
            _propertyName = null;
            p[property] = value;

            if (expectEvent)
            {
                Assert.AreEqual(property, _propertyName);
            }
            else
            {
                Assert.IsNull(_propertyName);
            }

        }

        public void NotifyPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            _propertyName = e.PropertyName;
        }

    }
}