namespace ExampleCodeSnippets
{
    [NUnit.Framework.TestFixture]
    public class NtsTests
    {
        [NUnit.Framework.TestFixtureSetUp]
        public void FixtureSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance =
                NetTopologySuite.NtsGeometryServices.Instance;
        }

        [NUnit.Framework.Test]
        public void TestDiscussionNtsAndBaffeled()
        {
            var reader = new NetTopologySuite.IO.WKTReader();
            var poly = reader.Read(
                @"POLYGON ((428999.76819468878 360451.93329044303, 428998.25517286535 360420.80827007542,
429023.1119599645 360406.75878171506, 429004.52340613387 360451.71714446822, 
429004.52340613387 360451.71714446822, 428999.76819468878 360451.93329044303))");

            var points = new System.Collections.Generic.List<GeoAPI.Geometries.IGeometry>(new []
                {
                    reader.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(429012.5, 360443.18)),
                    reader.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(429001.59, 360446.98)),
                    reader.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(429003.31, 360425.45)),
                    reader.Factory.CreatePoint(new GeoAPI.Geometries.Coordinate(429016.9, 360413.04))
                });

            var inside = new System.Collections.Generic.List<bool>(new[] {false, true, true, true});

            for (var i = 0; i < points.Count; i++)
            {
                NUnit.Framework.Assert.AreEqual(inside[i], poly.Intersects(points[i]));
            }

            var prepPoly = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(poly);
            for (var i = 0; i < points.Count; i++)
            {
                NUnit.Framework.Assert.AreEqual(inside[i], prepPoly.Intersects(points[i]));
            }
        }
    }
}