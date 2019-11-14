using System;
using System.IO;
using NUnit.Framework;

namespace UnitTests.Data.Providers
{
    [TestFixture]
    public abstract class  ProviderTest
    {
        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance =
                NetTopologySuite.NtsGeometryServices.Instance;
        }

        protected string TestDataPath
        {
            get
            {
                return "TestData";
            }
        }

        protected string GetTestDataFilePath(string fileName)
        {
            try
            {
                var uri = new Uri(fileName);
                if (uri.IsAbsoluteUri)
                    return uri.LocalPath;
                return Path.Combine(TestDataPath, fileName);
            }
            catch (Exception)
            {
                return Path.Combine(TestDataPath, fileName);
            }
        }
    }
}
