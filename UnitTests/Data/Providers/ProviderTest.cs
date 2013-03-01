using System;
using System.IO;
using NUnit.Framework;

namespace UnitTests.Data.Providers
{
    [TestFixture]
    public abstract class  ProviderTest
    {
        [TestFixtureSetUp]
        public virtual void TestFixtureSetup()
        {
            GeoAPI.GeometryServiceProvider.Instance =
                NetTopologySuite.NtsGeometryServices.Instance;
        }

        protected string TestDataPath
        {
            get
            {
                var codeBase = Path.GetDirectoryName(GetType().Assembly.CodeBase);
                return Path.Combine(new Uri(codeBase).LocalPath, "TestData");
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