#if DotSpatialProjections

namespace UnitTests.CoordinateSystems.Transformations
{
    
    public class GeometryTransformDotSpatialTest
    {
        [NUnit.Framework.Test]
        public void TransformBoxTest()
        {
            var from = DotSpatial.Projections.KnownCoordinateSystems.Geographic.World.WGS1984;
            var to = DotSpatial.Projections.KnownCoordinateSystems.Projected.NationalGrids.GermanyZone2;

            var env0 = new GeoAPI.Geometries.Envelope(5, 5.1, 52, 52.1);
            System.Console.WriteLine(env0);
            var env1 = DotSpatial.Projections.GeometryTransform.TransformBox(env0, from, to);
            System.Console.WriteLine(env1);
            var env2 = DotSpatial.Projections.GeometryTransform.TransformBox(env1, to, from);
            System.Console.WriteLine(env2);

            NUnit.Framework.Assert.AreEqual(env0.MinX, env2.MinX, 0.01d);
            NUnit.Framework.Assert.AreEqual(env0.MaxX, env2.MaxX, 0.01d);
            NUnit.Framework.Assert.AreEqual(env0.MinY, env2.MinY, 0.01d);
            NUnit.Framework.Assert.AreEqual(env0.MaxY, env2.MaxY, 0.01d);
        }
    }
}

#endif