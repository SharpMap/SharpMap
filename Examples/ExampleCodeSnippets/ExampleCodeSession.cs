namespace ExampleCodeSnippets
{
    public class ExampleCodeSession
    {
        [NUnit.Framework.OneTimeSetUp]
        public void SetUp()
        {
            var gss = NetTopologySuite.NtsGeometryServices.Instance;
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new ProjNet.CoordinateSystems.CoordinateSystemFactory(),
                new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);
        }
    }
}
