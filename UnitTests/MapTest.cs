using System;
using System.Collections.ObjectModel;
using System.Drawing;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SharpMap;
using SharpMap.Data.Providers;
using Geometry = GeoAPI.Geometries.IGeometry;
using SharpMap.Layers;
using Point=GeoAPI.Geometries.Coordinate;
using BoundingBox = GeoAPI.Geometries.Envelope;

namespace UnitTests
{
    [TestFixture]
    public class MapTest
    {
        private static readonly IGeometryFactory Factory = new GeometryFactory();
        private static readonly WKTReader WktReader = new WKTReader(Factory);

        public static IGeometry GeomFromText(string wkt)
        {
            return WktReader.Read(wkt);
        }

        private static IProvider CreateDatasource()
        {
            var geoms = new Collection<Geometry>
                            {
                                GeomFromText("POINT EMPTY"),
                                GeomFromText(
                                    "GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))"),
                                GeomFromText(
                                    "MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), ((5 5, 7 5, 7 7, 5 7, 5 5)))"),
                                GeomFromText("LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)"),
                                GeomFromText(
                                    "MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))"),
                                GeomFromText(
                                    "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 29 21, 29 29, 21 29, 21 21), (23 23, 23 27, 27 27, 27 23, 23 23))"),
                                GeomFromText("POINT (20.564 346.3493254)"),
                                GeomFromText("MULTIPOINT (20.564 346.3493254, 45 32, 23 54)"),
                                GeomFromText("MULTIPOLYGON EMPTY"),
                                GeomFromText("MULTILINESTRING EMPTY"),
                                GeomFromText("MULTIPOINT EMPTY"),
                                GeomFromText("LINESTRING EMPTY")
                            };
            return new GeometryProvider(geoms);
        }

        [Test]
        public void MapSridEqualsFactorySrid()
        {
            var m = new Map();
            Assert.AreEqual(m.SRID, m.Factory.SRID);
            m.SRID = 10;
            Assert.AreEqual(m.SRID, m.Factory.SRID);
        }

        [Test]
        public void FindLayer_ReturnEnumerable()
        {
            Map map = new Map();
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.Layers.Add(new VectorLayer("Layer 3"));
            map.Layers.Add(new VectorLayer("Layer 2"));
            map.Layers.Add(new VectorLayer("Layer 4"));

            int count = 0;
            foreach (ILayer lay in map.FindLayer("Layer 3"))
            {
                Assert.AreEqual("Layer 3", lay.LayerName);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void TestClone()
        {
            Map map = new Map(new Size(2, 1));
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.Layers.Add(new VectorLayer("Layer 3"));
            map.Layers.Add(new VectorLayer("Layer 2"));
            map.Layers.Add(new VectorLayer("Layer 4"));

            var clone = map.Clone();

            Assert.AreEqual(map.BackgroundLayer.Count, clone.BackgroundLayer.Count);
            Assert.AreEqual(map.Layers.Count, clone.Layers.Count);
            Assert.AreEqual(map.VariableLayers.Count, clone.VariableLayers.Count);
            Assert.AreEqual(map.Decorations.Count, clone.Decorations.Count);
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetExtents_EmptyMap_ThrowInvalidOperationException()
        {
            Map map = new Map(new Size(2, 1));
            map.ZoomToExtents();
        }

        [Test]
        public void Map_InsertLayer()
        {
            Map m = new Map();
            VectorLayer vlay1 = new VectorLayer("1");
            m.Layers.Add(vlay1);
            VectorLayer vlay2 = new VectorLayer("2");
            m.Layers.Add(vlay2);

            VectorLayer vlay3 = new VectorLayer("3");
            m.Layers.Insert(1, vlay3);

            Assert.AreEqual("1", m.Layers[0].LayerName);
            Assert.AreEqual("3", m.Layers[1].LayerName);
            Assert.AreEqual("2", m.Layers[2].LayerName);
        }

        [Test]
        public void Map_GetLayerByNameInGroupLayer()
        {
            Map m = new Map();
            VectorLayer vlay1 = new VectorLayer("1");
            VectorLayer vlay2 = new VectorLayer("2");
            VectorLayer vlay3 = new VectorLayer("3");
            m.Layers.Add(vlay1);

            LayerGroup lg = new LayerGroup("Group");
            lg.Layers.Add(vlay2);
            lg.Layers.Add(vlay3);
            m.Layers.Add(lg);


            var lay = m.GetLayerByName("1");
            Assert.IsNotNull(lay);
            Assert.AreEqual("1",lay.LayerName);
            lay = m.GetLayerByName("2");
            Assert.IsNotNull(lay);
            Assert.AreEqual("2", lay.LayerName);
            lay = m.GetLayerByName("3");
            Assert.IsNotNull(lay);
            Assert.AreEqual("3", lay.LayerName);
            lay = m.GetLayerByName("Group");
            Assert.IsNotNull(lay);
            Assert.AreEqual("Group", lay.LayerName);
            
        }

        [Test]
        public void GetExtents_ValidDatasource()
        {
            Map map = new Map(new Size(400, 200));
            VectorLayer vLayer = new VectorLayer("Geom layer", CreateDatasource());
            map.Layers.Add(vLayer);
            BoundingBox box = map.GetExtents();
            Assert.AreEqual(new BoundingBox(0, 50, 0, 346.3493254), box);
        }

        [Test]
        public void GetLayerByName_Indexer()
        {
            Map map = new Map();
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.Layers.Add(new VectorLayer("Layer 3"));
            map.Layers.Add(new VectorLayer("Layer 2"));

            ILayer layer = map.Layers["Layer 2"];
            Assert.IsNotNull(layer);
            Assert.AreEqual("Layer 2", layer.LayerName);
        }

        [Test]
        public void GetLayerByName_ReturnCorrectLayer()
        {
            Map map = new Map();
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.Layers.Add(new VectorLayer("Layer 3"));
            map.Layers.Add(new VectorLayer("Layer 2"));

            ILayer layer = map.GetLayerByName("Layer 2");
            Assert.IsNotNull(layer);
            Assert.AreEqual("Layer 2", layer.LayerName);
        }

        [Test]
        public void GetMap_GeometryProvider_ReturnImage()
        {
            Map map = new Map(new Size(400, 200));
            VectorLayer vLayer = new VectorLayer("Geom layer", CreateDatasource());
            vLayer.Style.Outline = new Pen(Color.Red, 2f);
            vLayer.Style.EnableOutline = true;
            vLayer.Style.Line = new Pen(Color.Green, 2f);
            vLayer.Style.Fill = Brushes.Yellow;
            map.Layers.Add(vLayer);

            VectorLayer vLayer2 = new VectorLayer("Geom layer 2", vLayer.DataSource);
            vLayer2.Style.SymbolOffset = new PointF(3, 4);
            vLayer2.Style.SymbolRotation = 45;
            vLayer2.Style.SymbolScale = 0.4f;
            map.Layers.Add(vLayer2);

            VectorLayer vLayer3 = new VectorLayer("Geom layer 3", vLayer.DataSource);
            vLayer3.Style.SymbolOffset = new PointF(3, 4);
            vLayer3.Style.SymbolRotation = 45;
            map.Layers.Add(vLayer3);

            VectorLayer vLayer4 = new VectorLayer("Geom layer 4", vLayer.DataSource);
            vLayer4.Style.SymbolOffset = new PointF(3, 4);
            vLayer4.Style.SymbolScale = 0.4f;
            vLayer4.ClippingEnabled = true;
            map.Layers.Add(vLayer4);

            map.ZoomToExtents();

            Image img = map.GetMap();
            Assert.IsNotNull(img);
            map.Dispose();
            img.Dispose();
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetMap_RenderEmptyMap_ThrowInvalidOperationException()
        {
            Map map = new Map(new Size(2, 1));
            map.GetMap();
        }

        [Test]
        [ExpectedException(typeof (ApplicationException))]
        public void GetMap_RenderLayerWithoutDatasource_ThrowException()
        {
            Map map = new Map();
            map.Layers.Add(new VectorLayer("Layer 1"));
            map.GetMap();
        }

        [Test]
        public void GetMapHeight_FixedZoom_Return1750()
        {
            Map map = new Map(new Size(400, 200));
            map.Zoom = 3500;
            Assert.AreEqual(1750, map.MapHeight);
        }

        [Test]
        public void GetPixelSize_FixedZoom_Return8_75()
        {
            Map map = new Map(new Size(400, 200));
            map.Zoom = 3500;
            Assert.AreEqual(8.75, map.PixelSize);
        }

        [Test]
        public void ImageToWorld()
        {
            Map map = new Map(new Size(1000, 500));
            map.Zoom = 360;
            map.Center = new Point(0, 0);
            Assert.AreEqual(new Point(0, 0), map.ImageToWorld(new PointF(500, 250)));
            Assert.AreEqual(new Point(-180, 90), map.ImageToWorld(new PointF(0, 0)));
            Assert.AreEqual(new Point(-180, -90), map.ImageToWorld(new PointF(0, 500)));
            Assert.AreEqual(new Point(180, 90), map.ImageToWorld(new PointF(1000, 0)));
            Assert.AreEqual(new Point(180, -90), map.ImageToWorld(new PointF(1000, 500)));
        }

        [Test]
        public void ImageToWorld_DefaultMap_ReturnValue()
        {
            Map map = new Map(new Size(500, 200));
            map.Center = new Point(23, 34);
            map.Zoom = 1000;
            Point p = map.ImageToWorld(new PointF(242.5f, 92));
            Assert.AreEqual(new Point(8, 50), p);
        }

        [Test]
        public void Initalize_MapInstance()
        {
            Map map = new Map(new Size(2, 1));
            Assert.IsNotNull(map);
            Assert.IsNotNull(map.Layers);
            Assert.AreEqual(2f, map.Size.Width);
            Assert.AreEqual(1f, map.Size.Height);
            Assert.AreEqual(Color.Transparent, map.BackColor);
            Assert.AreEqual(double.MaxValue, map.MaximumZoom);
            Assert.IsTrue(map.MinimumZoom > 0);
            Assert.AreEqual(new Point(0, 0), map.Center, "map.Center should be initialized to (0,0)");
            Assert.AreEqual(1, map.Zoom, "Map zoom should be initialized to 1.0");
        }

        [Test]
        public void SetMaximumZoom_ValueLessThanMinimumZoom()
        {
            Map map = new Map();
            map.MaximumZoom = -1;
            Assert.IsTrue(map.MaximumZoom >= map.MinimumZoom);
        }

        [Test]
        public void SetMaximumZoom_OKValue()
        {
            Map map = new Map();
            map.MaximumZoom = 100.3;
            Assert.AreEqual(100.3, map.MaximumZoom);
        }

        [Test]
        public void SetMinimumZoom_ValueLessThanTwoEpsilon()
        {
            Map map = new Map();
            map.MinimumZoom = -1;
            Assert.IsTrue(map.MinimumZoom > 0);
            map.MinimumZoom = Double.Epsilon;
            Assert.IsTrue(map.MinimumZoom > Double.Epsilon);
        }

        [Test]
        public void SetMinimumZoom_OKValue()
        {
            Map map = new Map();
            map.MinimumZoom = 100.3;
            Assert.AreEqual(100.3, map.MinimumZoom);
        }

        [Test]
        public void SetZoom_ValueBelowMin()
        {
            Map map = new Map();
            map.MinimumZoom = 100;
            map.Zoom = 50;
            Assert.AreEqual(100, map.MinimumZoom);
        }

        [Test]
        public void SetZoom_ValueOutsideMax()
        {
            Map map = new Map();
            map.MaximumZoom = 100;
            map.Zoom = 150;
            Assert.AreEqual(100, map.MaximumZoom);
        }

        [Test]
        public void ZoomToBoxWithEnforcedMaximumExtents()
        {
            Map map = new Map();
            //map.MaximumZoom = 100;
            map.MaximumExtents = new Envelope(-180, 180, -90, 90);
            map.EnforceMaximumExtents = true;
            map.ZoomToBox(new Envelope(-200, 200, -100, 100));
            Assert.IsTrue(map.MaximumExtents.Contains(map.Envelope));
            Assert.AreEqual(new BoundingBox(-120, 120, -90, 90), map.Envelope);
        }



        [Test]
        public void WorldToImage()
        {
            Map map = new Map(new Size(1000, 500));
            map.Zoom = 360;
            map.Center = new Point(0, 0);
            Assert.AreEqual(new PointF(500, 250), map.WorldToImage(new Point(0, 0)));
            Assert.AreEqual(new PointF(0, 0), map.WorldToImage(new Point(-180, 90)));
            Assert.AreEqual(new PointF(0, 500), map.WorldToImage(new Point(-180, -90)));
            Assert.AreEqual(new PointF(1000, 0), map.WorldToImage(new Point(180, 90)));
            Assert.AreEqual(new PointF(1000, 500), map.WorldToImage(new Point(180, -90)));
        }

        [Test]
        public void WorldToMap_DefaultMap_ReturnValue()
        {
            Map map = new Map(new Size(500, 200));
            map.Center = new Point(23, 34);
            map.Zoom = 1000;
            PointF p = map.WorldToImage(new Point(8, 50));
            Assert.AreEqual(new PointF(242.5f, 92), p);
        }

        [Test]
        public void ZoomToBox_NoAspectCorrection()
        {
            Map map = new Map(new Size(400, 200));
            map.ZoomToBox(new BoundingBox(20, 100, 50, 80));
            Assert.AreEqual(new Point(60, 65), map.Center);
            Assert.AreEqual(80, map.Zoom);
        }

        [Test]
        public void ZoomToBox_WithAspectCorrection()
        {
            Map map = new Map(new Size(400, 200));
            map.ZoomToBox(new BoundingBox(20, 100, 10, 180));
            Assert.AreEqual(new Point(60, 95), map.Center);
            Assert.AreEqual(340, map.Zoom);
        }

        [Test]
        public void TestZoomToBoxRaisesMapViewOnChange()
        {
            var raised = false;
            var map = new Map(new Size(400, 200));
            map.MapViewOnChange += () => raised = true;

            map.ZoomToBox(new BoundingBox(20, 100, 10, 180));
            Assert.IsTrue(raised, "MapViewOnChange not fired when calling Map.ZoomToBox(...).");
            raised = false;
            map.ZoomToBox(new BoundingBox(20, 100, 10, 180));
            Assert.IsFalse(raised, "MapViewOnChange fired when calling Map.ZoomToBox(map.Envelope).");

            raised = false;
            map.Center = new Coordinate(map.Center.X + 10, map.Center.Y);
            Assert.IsTrue(raised, "MapViewOnChange not fired when setting Map.Center");
            raised = false;
            map.Center = map.Center;
            Assert.IsFalse(raised, "MapViewOnChange fired when setting Map.Center = Map.Center");
        }
    }
}