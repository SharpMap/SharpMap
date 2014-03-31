using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Layers;

namespace UnitTests.Layers
{
    [TestFixture]
    public class LayerGroupTest : UnitTestsFixture
    {
        #region DummyLayer class
        private class DummyLayer : Layer
        {
            public override Envelope Envelope
            {
                get { return null; }
            }
        } 
        #endregion

        #region FooLayer class
        private class FooLayer : Layer
        {
            public override Envelope Envelope
            {
                get { return new Envelope(5, 10, 5, 10); }
            }
        } 
        #endregion

#if !DotSpatialProjections
        private GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation CreateTransformation()
        {
            var ctf = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            return ctf.CreateFromCoordinateSystems(
                ProjNet.CoordinateSystems.GeocentricCoordinateSystem.WGS84,
                ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84);
        }
#else
        private DotSpatial.Projections.ICoordinateTransformation CreateTransformation()
        {
            return new DotSpatial.Projections.CoordinateTransformation();
        }
#endif
        [Test(Description = "Setting a CoordinateTransformation to LayerGroup propagates to inner layers")]
        public void CoordinateTransformation_SettingValue_PropagatesTransformation()
        {
            var group = new LayerGroup("group");
            group.Layers.Add(new DummyLayer());

            var transf = CreateTransformation();

            group.CoordinateTransformation = transf;

            Assert.That(group.Layers[0].CoordinateTransformation, Is.EqualTo(transf),
                "LayerGroup.CoordinateTransformation should propagate to inner layers");
        }

        [Test(Description = "Setting a CoordinateTransformation to LayerGroup when SkipTransformationPropagation is true does NOT propagate to inner layers")]
        public void CoordinateTransformation_SettingValueWhenSkipIsOn_DoesNotPropagateTransformation()
        {
            var group = new LayerGroup("group");
            group.Layers.Add(new DummyLayer());

            var transf = CreateTransformation();

            group.SkipTransformationPropagation = true;
            group.CoordinateTransformation = transf;

            Assert.That(group.Layers[0].CoordinateTransformation, Is.Not.EqualTo(transf),
                "LayerGroup.CoordinateTransformation should NOT propagate to inner layers because SkipTransformationPropagation was true");
        }

#if !DotSpatialProjections
        [Test(Description = "Setting a ReverseCoordinateTransformation to LayerGroup propagates to inner layers")]
        public void ReverseCoordinateTransformation_SettingValue_PropagatesTransformation()
        {
            var group = new LayerGroup("group");
            group.Layers.Add(new DummyLayer());

            var transf = CreateTransformation();

            group.ReverseCoordinateTransformation = transf;

            Assert.That(group.Layers[0].ReverseCoordinateTransformation, Is.EqualTo(transf),
                "LayerGroup.ReverseCoordinateTransformation should propagate to inner layers");
        }

        [Test(Description = "Setting a ReverseCoordinateTransformation to LayerGroup when SkipTransformationPropagation is true does NOT propagate to inner layers")]
        public void ReverseCoordinateTransformation_SettingValueWhenSkipIsOn_DoesNotPropagateTransformation()
        {
            var group = new LayerGroup("group");
            group.Layers.Add(new DummyLayer());

            var transf = CreateTransformation();

            group.SkipTransformationPropagation = true;
            group.ReverseCoordinateTransformation = transf;

            Assert.That(group.Layers[0].ReverseCoordinateTransformation, Is.Not.EqualTo(transf),
                "LayerGroup.ReverseCoordinateTransformation should NOT propagate to inner layers because SkipTransformationPropagation was true");
        }
#endif

        [Test(Description = "Envelope returns null when the inner layer Envelope is null")]
        public void Envelope_WhenInnerLayerEnvelopeIsNull_ReturnsNull()
        {
            var group = new LayerGroup("group");
            group.Layers.Add(new DummyLayer());

            Assert.That(group.Envelope, Is.Null,
                "LayerGroup envelope should be null since we added a layer with a null envelope");
        }

        [Test(Description = "Envelope returns null when there are no layers")]
        public void Envelope_NoInnerLayers_ReturnsNull()
        {
            var group = new LayerGroup("group");
            Assert.IsNull(group.Envelope);
        }

        [Test(Description = "Envelope skips inner layers which return null envelope")]
        public void Envelope_SomeInnerLayerWithNullEnvelope_SkipNulls()
        {
            var group = new LayerGroup("group");
            group.Layers.Add(new DummyLayer());
            group.Layers.Add(new FooLayer());

            Assert.That(group.Envelope, Is.EqualTo(new Envelope(5, 10, 5, 10)));
        }
    }
}
