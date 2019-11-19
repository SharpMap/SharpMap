#if !LINUX
using System;
using System.Drawing;
using System.IO;
using BruTile.Predefined;
using BruTile.Web;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;

namespace UnitTests.Serialization
{
    public class MapTest : BaseSerializationTest
    {
        private readonly Size _mapSize = new Size(1200, 800);

        [Test, Description("Just a line")] 
        public void TestMap1()
        {
            var m = new Map(_mapSize);
            m.Layers.Add(new VectorLayer("tmp", new GeometryProvider(
                new LineString(new [] {new Coordinate(0, 0), new Coordinate(10, 10), }))));

            m.ZoomToExtents();

            Map mD = null;
            Assert.DoesNotThrow(()=>mD=SandD(m, GetFormatter()));
            
            TestMaps("Test1", m, mD);
        }

        private static void TestMaps(string name, Map m, Map mD)
        {
            Assert.NotNull(mD);

            Assert.AreEqual(m.Size, mD.Size);
            Assert.AreEqual(m.Layers.Count, mD.Layers.Count);
            var c = new LayerTest.VectorLayerEqualityComparer();
            for (var i = 0; i < m.Layers.Count; i++)
            {
                Assert.IsTrue(c.Equals((VectorLayer)m.Layers[i], 
                                       (VectorLayer)mD.Layers[i]), 
                                       "Layer {0}, '{1}' Differs at {2}",
                                       i, m.Layers[i].LayerName, string.Join(", ", c.DifferAt));
            }

            Assert.AreEqual(m.PixelAspectRatio, mD.PixelAspectRatio);
            Assert.AreEqual(m.PixelHeight, mD.PixelHeight);
            Assert.AreEqual(m.PixelWidth, mD.PixelWidth);
            Assert.AreEqual(m.PixelSize, mD.PixelSize);

            Assert.AreEqual(m.BackColor, mD.BackColor);
            Assert.IsTrue(m.Center.Equals(mD.Center));
            Assert.IsTrue(m.GetExtents().Equals(mD.GetExtents()));
            Assert.IsTrue(m.Envelope.Equals(mD.Envelope));

            Assert.AreEqual(m.Decorations.Count, mD.Decorations.Count);
            Assert.AreEqual(m.SRID, mD.SRID);
            Assert.AreEqual(m.Zoom, mD.Zoom);

            Assert.DoesNotThrow(() => m.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(new MapTest()), name + "-S.bmp")));
            Assert.DoesNotThrow(() => mD.GetMap().Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(new MapTest()), name + "-D.bmp")));
        }



        [Test, Description("BingHybridStaging base map, OSM of Aurich, randomly styled"), Ignore("Need to fix BruTile serialization")]
        public void TestMap2()
        {
            var m = new Map(_mapSize);
            m.BackgroundLayer.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingHybridStaging), "BingHybridStaging"));
            
            string cn = $"Data Source={TestUtility.GetPathToTestFile("osm_aurich.sqlite")};";
            
            var ct = Wgs84ToWebMercator;
            //Env[7,45731445821406 : 7,53454260528903, 53,4342695512313 : 53,478793942147]
            var box = new Envelope(7.45731445821406, 7.53454260528903, 53.4342695512313, 53.478793942147);
            var box3857 = GeometryTransform.TransformBox(box, ct.MathTransform);

            m.ZoomToBox(box3857);

            foreach (var msp in ManagedSpatiaLite.GetSpatialTables(cn))
            {
                var l = new VectorLayer(msp.Table, msp);
                switch (msp.Table.Substring(0, 2).ToLower())
                {
                    case "pt":
                        l.Style = VectorStyle.CreateRandomPuntalStyle();
                        break;
                    case "ln":
                        l.Style = VectorStyle.CreateRandomLinealStyle();
                        break;
                    case "pg":
                        l.Style = VectorStyle.CreateRandomPolygonalStyle();
                        break;
                    default:
                        l.Style = VectorStyle.CreateRandomStyle();
                        break;
                }

                l.CoordinateTransformation = ct;
                m.Layers.Add(l);
            }

            var f = GetFormatter();
            //BruTile.Utility.AddBruTileSurrogates(GetFormatter());

            Map mD = null;
            Assert.DoesNotThrow(() => mD = SandD(m, f));

            TestMaps("Test2", m, mD);
        }

        private static ICoordinateTransformation Wgs84ToWebMercator
        {
            get
            {
                return new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory()
                    .CreateFromCoordinateSystems(ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84,
                                                 ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator);
            }
        }
    }
}
#endif
