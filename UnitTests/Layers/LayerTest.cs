using System;
using System.ComponentModel;
using System.Windows.Forms;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace UnitTests.Layers
{
    [TestFixture]
    public class LayerTest
    {
        #region DummyLayer class
        private class DummyLayer : SharpMap.Layers.Layer
        {
            public override Envelope Envelope
            {
                get { throw new NotImplementedException(); }
            }
        } 
        #endregion

        #region BindableComponent class
        private class BindableComponent : IBindableComponent
        {
            public BindableComponent()
            {
                BindingContext = new BindingContext();
                DataBindings = new ControlBindingsCollection(this);
            }

            public void Dispose()
            {
                Disposed?.Invoke(this, EventArgs.Empty);
            }

            public ISite Site { get; set; }

            public event EventHandler Disposed;

            public ControlBindingsCollection DataBindings { get; private set; }

            public BindingContext BindingContext { get; set; }

            public int IntProperty { get; set; }
        } 
        #endregion


        private bool _bcDisposed;

        [Test(Description = "SRID property must be bindable")]
        public void SRID_Winform_ShouldBeBindable()
        {
            var layer = new DummyLayer();
            layer.SRID = 0;

            var binding = new Binding("IntProperty", layer, "SRID");
            
            var targetComponent = new BindableComponent();
            targetComponent.Disposed += (sender, args) => _bcDisposed = true;
            targetComponent.DataBindings.Add(binding);

            layer.SRID = 4326;

            Assert.That(targetComponent.IntProperty, Is.EqualTo(4326),
                "Binding on Layer.SRID did not work");

            targetComponent.Dispose();
            Assert.That(_bcDisposed, Is.True);
        }
    }
}
