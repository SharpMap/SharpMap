using System;
using System.Collections.ObjectModel;
using System.Drawing;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Geometries;
using SharpMap.Layers;
using Point=SharpMap.Geometries.Point;

namespace UnitTests
{
    [TestFixture]
    public class MapTest
    {
        private IProvider CreateDatasource()
        {
            Collection<Geometry> geoms = new Collection<Geometry>();
            geoms.Add(Geometry.GeomFromText("POINT EMPTY"));
            geoms.Add(
                Geometry.GeomFromText("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))"));
            geoms.Add(
                Geometry.GeomFromText("MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), ((5 5, 7 5, 7 7, 5 7, 5 5)))"));
            geoms.Add(Geometry.GeomFromText("LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)"));
            geoms.Add(
                Geometry.GeomFromText("MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))"));
            geoms.Add(
                Geometry.GeomFromText(
                    "POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 29 21, 29 29, 21 29, 21 21), (23 23, 23 27, 27 27, 27 23, 23 23))"));
            geoms.Add(Geometry.GeomFromText("POINT (20.564 346.3493254)"));
            geoms.Add(Geometry.GeomFromText("MULTIPOINT (20.564 346.3493254, 45 32, 23 54)"));
            geoms.Add(Geometry.GeomFromText("MULTIPOLYGON EMPTY"));
            geoms.Add(Geometry.GeomFromText("MULTILINESTRING EMPTY"));
            geoms.Add(Geometry.GeomFromText("MULTIPOINT EMPTY"));
            geoms.Add(Geometry.GeomFromText("LINESTRING EMPTY"));
            return new GeometryProvider(geoms);
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
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetExtents_EmptyMap_ThrowInvalidOperationException()
        {
            Map map = new Map(new Size(2, 1));
            map.ZoomToExtents();
        }

        [Test]
        public void GetExtents_ValidDatasource()
        {
            Map map = new Map(new Size(400, 200));
            VectorLayer vLayer = new VectorLayer("Geom layer", CreateDatasource());
            map.Layers.Add(vLayer);
            BoundingBox box = map.GetExtents();
            Assert.AreEqual(new BoundingBox(0, 0, 50, 346.3493254), box);
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
            Assert.AreEqual(0, map.MinimumZoom);
            Assert.AreEqual(new Point(0, 0), map.Center, "map.Center should be initialized to (0,0)");
            Assert.AreEqual(1, map.Zoom, "Map zoom should be initialized to 1.0");
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void SetMaximumZoom_NegativeValue_ThrowException()
        {
            Map map = new Map();
            map.MaximumZoom = -1;
        }

        [Test]
        public void SetMaximumZoom_OKValue()
        {
            Map map = new Map();
            map.MaximumZoom = 100.3;
            Assert.AreEqual(100.3, map.MaximumZoom);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void SetMinimumZoom_NegativeValue_ThrowException()
        {
            Map map = new Map();
            map.MinimumZoom = -1;
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
            map.ZoomToBox(new BoundingBox(20, 50, 100, 80));
            Assert.AreEqual(new Point(60, 65), map.Center);
            Assert.AreEqual(80, map.Zoom);
        }

        [Test]
        public void ZoomToBox_WithAspectCorrection()
        {
            Map map = new Map(new Size(400, 200));
            map.ZoomToBox(new BoundingBox(20, 10, 100, 180));
            Assert.AreEqual(new Point(60, 95), map.Center);
            Assert.AreEqual(340, map.Zoom);
        }
    }
}