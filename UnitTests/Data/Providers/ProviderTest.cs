namespace UnitTests.Data.Providers
{
    [NUnit.Framework.TestFixture]
    public abstract class ProviderTest
    {
        [NUnit.Framework.OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance =
                NetTopologySuite.NtsGeometryServices.Instance;
        }
    }
}
