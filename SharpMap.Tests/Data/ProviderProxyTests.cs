using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace SharpMap.Data
{
    [TestFixture]
    public class ProviderProxyTests
    {
        private readonly ProviderProxy _providerProxy;

        protected ProviderProxyTests(IProvider provider)
        {
            _providerProxy = new ProviderProxy(provider);
        }

        [Test]
        void TestGetExtents()
        {
            Envelope extents = null;
            Assert.DoesNotThrow(() => extents = _providerProxy.GetExtents());
            Assert.AreEqual(extents, _providerProxy.Provider.GetExtents());

            Console.WriteLine(extents);
        }

        [Test]
        void TestGetFeatures()
        {
            Envelope extents = null;
            Assert.DoesNotThrow(() => extents = _providerProxy.GetExtents());

            Console.WriteLine(extents);
        }
    }

}