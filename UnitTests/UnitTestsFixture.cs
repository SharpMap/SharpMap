using System;

namespace UnitTests
{
    [NUnit.Framework.SetUpFixture]
    public class UnitTestsFixture
    {
        private System.Diagnostics.Stopwatch _stopWatch;
        private const string ImageBase = "Images"; 
        
        [NUnit.Framework.OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            var gss = new NetTopologySuite.NtsGeometryServices();
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new ProjNet.CoordinateSystems.CoordinateSystemFactory(),
                new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

            GeoAPI.GeometryServiceProvider.Instance = gss;
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);

            // plug-in WebMercator so that correct spherical definition is directly available to Layer Transformations using SRID
            var pcs = (ProjNet.CoordinateSystems.ProjectedCoordinateSystem)ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;
            css.AddCoordinateSystem((int)pcs.AuthorityCode, pcs);
           
            _stopWatch = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Trace.WriteLine("Starting tests");
            _stopWatch.Start();
        }

        [NUnit.Framework.OneTimeTearDown]
        public void RunAfterAllTests()
        {
            _stopWatch.Stop();
            System.Diagnostics.Trace.WriteLine(
                string.Format("All tests accomplished in {0}ms", _stopWatch.ElapsedMilliseconds));
        }

        internal static string GetImageDirectory(object item)
        {
            string imgPath = System.IO.Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, ImageBase, item?.GetType()?.FullName ?? string.Empty);
            if (!System.IO.Directory.Exists(imgPath))
                System.IO.Directory.CreateDirectory(imgPath);
            return imgPath;
        }
    }
}
