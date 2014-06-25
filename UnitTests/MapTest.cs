using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Emit;
using GeoAPI.Geometries;
using Microsoft.SqlServer.Server;
using Moq;
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
            
            // ZoomToBox
            map.ZoomToBox(new BoundingBox(20, 100, 10, 180));
            Assert.IsTrue(raised, "MapViewOnChange not fired when calling Map.ZoomToBox(...).");
            raised = false;
            map.ZoomToBox(new BoundingBox(20, 100, 10, 180));
            Assert.IsFalse(raised, "MapViewOnChange fired when calling Map.ZoomToBox(map.Envelope).");

            // ZoomToExtents
            // Note: Not needed as Map.ZoomToExtents() calls Map.ZoomToBox(Map.GetExtents())
            //raised = false;
            //map.ZoomToExtents();
            //Assert.IsTrue(raised, "MapViewOnChange not fired when calling Map.ZoomToExtents()");
            //raised = false;
            //map.ZoomToExtents();
            //Assert.IsFalse(raised, "MapViewOnChange fired when calling Map.ZoomToExtents() twice");

            // Set Zoom
            map.Zoom = map.Zoom * 0.9;
            Assert.IsTrue(raised, "MapViewOnChange not fired when setting Map.Zoom.");
            raised = false;
            map.Zoom = map.Zoom;
            Assert.IsFalse(raised, "MapViewOnChange not fired when setting Map.Zoom = Map.Zoom");

            // Set Center
            map.Center = new Coordinate(map.Center.X + 1, map.Center.Y);
            Assert.IsTrue(raised, "MapViewOnChange not fired when setting Map.Center.");
            raised = false;
            map.Center = map.Center;
            Assert.IsFalse(raised, "MapViewOnChange fired when setting Map.Center = Map.Center.");
        
        }
    
        
        [TestCase(LayerCollectionType.Background, Description = "The map fires MapNewTileAvailable event when an ITileAsyncLayer added to background collection, fires the MapNewTileAvailable event")]
        [TestCase(LayerCollectionType.Static, Description = "The map fires MapNewTileAvailable event when an ITileAsyncLayer added to static collection, fires the MapNewTileAvailable event")]
        public void AddingTileAsyncLayers_HookItsMapNewTileAvaliableEvent(LayerCollectionType collectionType)
        {
            var map = new Map();

            var layer = CreateTileAsyncLayer();

            AddTileLayerToMap(layer, map, collectionType);
            
            var eventSource = map.GetMapNewTileAvailableAsObservable();

            RaiseMapNewtileAvailableOn(layer);
            
            Assert.That(eventSource.Count().First(), Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "The map should not fire MapNewTileAvailable event for removed ITileAsyncLayers, case: layer from StaticLayers")]
        [TestCase(LayerCollectionType.Background, Description = "The map should not fire MapNewTileAvailable event for removed ITileAsyncLayers, case: layer from BackgroundLayers")]
        public void AfterRemovingTileAsyncLayer_MapDoesNotHookAnymoreItsMapNewTileAvailableEvent(LayerCollectionType collectionType)
        {
            var map = new Map();

            var tileAsyncLayer = CreateTileAsyncLayer();

            AddTileLayerToMap(tileAsyncLayer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            map.GetCollection(collectionType).RemoveAt(0); 

            RaiseMapNewtileAvailableOn(tileAsyncLayer);

            Assert.That(eventSource.Count().First(), Is.EqualTo(0), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "The map should not fire MapNewTileAvailable event for replaced TileAsyncLayers from Layer collection")]
        [TestCase(LayerCollectionType.Background, Description = "The map should not fire MapNewTileAvailable event for replaced TileAsyncLayers from BackgroundLayer collection")]
        public void MapDoesNotGenerateMapNewTile_ReplacedLayers(LayerCollectionType collectionType)
        {
            var map = new Map();

            var layer = CreateTileAsyncLayer();

            AddTileLayerToMap(layer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            var newLayer = CreateTileAsyncLayer();
            map.GetCollection(collectionType)[0] = newLayer.Item1.Object;

            RaiseMapNewtileAvailableOn(layer);

            Assert.That(eventSource.IsEmpty().First(), Is.EqualTo(true), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "The map should fire MapNewTileAvailable event for new added by replace TileAsyncLayers, case: Layer")]
        [TestCase(LayerCollectionType.Background, Description = "The map should fire MapNewTileAvailable event for new added by replace TileAsyncLayers, case: BackgroundLayer")]
        public void MapGeneratesMapNewTile_NewReplacedLayers(LayerCollectionType collectionType)
        {
            var map = new Map();

            var tileAsyncLayer = CreateTileAsyncLayer();

            AddTileLayerToMap(tileAsyncLayer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            var newLayer = CreateTileAsyncLayer();
            map.GetCollection(collectionType)[0] = newLayer.Item1.Object;

            RaiseMapNewtileAvailableOn(newLayer);

            Assert.That(eventSource.Count().First(), Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "The map should not fire MapNewTileAvailable event after the Layers are cleared from Layers collection")]
        [TestCase(LayerCollectionType.Background, Description = "The map should not fire MapNewTileAvailable event after the Layers are cleared from Background collection")]
        public void MapDoesNoGenerateMapNewTile_AfterClear(LayerCollectionType collectionType)
        {
            var map = new Map();

            var tileAsyncLayer = CreateTileAsyncLayer();

            AddTileLayerToMap(tileAsyncLayer, map, collectionType);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            map.GetCollection(collectionType).Clear();

            RaiseMapNewtileAvailableOn(tileAsyncLayer);

            Assert.That(eventSource.IsEmpty().First(), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewtileAvailable event for TileAsyncLayers contained inside of a LayerGroup, case Layer")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewtileAvailable event for TileAsyncLayers contained inside of a LayerGroup, case BackgroundLayer")]
        public void Map_TileAsyncInsideGroup_FiresMapNewtileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            RaiseMapNewtileAvailableOn(tileLayer);

            Assert.That(eventSource.Count().First(), Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should NOT fire MapNewtileAvailable event for TileAsyncLayers removed from a group belonging to Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should NOT fire MapNewtileAvailable event for TileAsyncLayers removed from a group belonging to BackgroundLayers")]
        public void Map_TileAsyncRemovedFromGroup_DoesNotFiredMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            RemoveTileLayerFromGroup(group, tileLayer);

            var eventSource = map.GetMapNewTileAvailableAsObservable();

            RaiseMapNewtileAvailableOn(tileLayer);
            Assert.That(eventSource.IsEmpty().First(), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewTileAvailable event for new TileAsyncLayer replaced from a group and not for the old layer, case: Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewTileAvailable event for new TileAsyncLayer replaced from a group and not for the old layer, case: BackgroundLayers")]
        public void Map_TileAsyncReplacedFromGroup_DoesNotFireMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            var newTileLayer = CreateTileAsyncLayer();
            ReplaceExistingAsyncLayerFromGroup(group, tileLayer, newTileLayer);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileLayer);

            Assert.That(eventSource.IsEmpty().First(),
                "Map should NOT fire MapNewTileAvailable event for TileAsyncLayers replaced from a group");

            RaiseMapNewtileAvailableOn(newTileLayer);

            Assert.That(eventSource.Count().First(), Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayer belonging to a group that has been cleared, case: Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayer belonging to a group that has been cleared, case: BackgroundLayers")]
        public void Map_TileAsyncFromClearedGroup_DoesNotFireMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            group.Layers.Clear();

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileLayer);

            Assert.That(eventSource.IsEmpty().First(), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewTileAvailable event for TileAsyncLayers belonging to an added group, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewTileAvailable event for TileAsyncLayers belonging to an added group, case BackgroundLayers")]
        public void Map_TileAsyncInsideAddedGroup_FiresMapNewTileAvail(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            var tileLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileLayer, group);

            map.GetCollection(collectionType).Add(group);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileLayer);
            Assert.That(eventSource.Count().First(), Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewtileAvailable event for TileAsyncLayers contained inside nested group, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewtileAvailable event for TileAsyncLayers contained inside nested group, case BackgroundLayers")]
        public void Map_TilAsyncInsideNephew_FiresMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var subGroup = CreateLayerGroup("subgroup");

            var tileAsyncLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileAsyncLayer, subGroup);

            group.Layers.Add(subGroup);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsyncLayer);
            Assert.That(eventSource.Count().First(), Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should not fire MapNewtileAvailable event for TileAsyncLayers removed from a nested group, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should not fire MapNewtileAvailable event for TileAsyncLayers removed from a nested group, case BackgroundLayers")]
        public void Map_TileAsyncRemovedFromNephew_DoesNotFireMapNewtileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var subGroup = CreateLayerGroup("subgroup");

            var tileAsyncLayer = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileAsyncLayer, subGroup);

            group.Layers.Add(subGroup);

            // test
            RemoveTileLayerFromGroup(subGroup, tileAsyncLayer);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsyncLayer);
            Assert.That(eventSource.Count().First(), Is.EqualTo(0), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayers belonging to a group collection replaced, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayers belonging to a group collection replaced, case BackgroundLayers")]
        public void Map_TileAsyncInsideReplacedCollection_DoesNotFireMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var tileAsync = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileAsync, group);

            group.Layers = new ObservableCollection<ILayer>();

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsync);
            Assert.That(eventSource.Count().First(), Is.EqualTo(0), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Static, Description = "Map should fire MapNewTileAvailable event for TileAsyncLayers added to a new replaced collection, case Layers")]
        [TestCase(LayerCollectionType.Background, Description = "Map should fire MapNewTileAvailable event for TileAsyncLayers added to a new replaced collection, case BackgroundLayers")]
        public void Map_TileAsyncAddedToReplacedCollection_FiresMapNewtileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            group.Layers = new ObservableCollection<ILayer>();

            var tileAsync = CreateTileAsyncLayer();
            AddTileLayerToLayerGroup(tileAsync, group);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsync);
            Assert.That(eventSource.Count().First(), Is.EqualTo(1), TestContext.CurrentContext.Test.GetDescription());
        }

        [TestCase(LayerCollectionType.Background, Description = "Map should not fire MapNewTileAvailable event for TileAsyncLayers added to detached collections, case BackgroundLayers")]
        public void Map_TileAsyncAddedToDetachedCollection_DoesNotFireMapNewTileAvailable(LayerCollectionType collectionType)
        {
            var map = new Map();

            var group = CreateLayerGroup();
            map.GetCollection(collectionType).Add(group);

            var detachedCollection = group.Layers;

            group.Layers = new ObservableCollection<ILayer>();

            var tileAsync = CreateTileAsyncLayer();
            detachedCollection.Add(tileAsync.Item1.Object);

            var eventSource = map.GetMapNewTileAvailableAsObservable();
            RaiseMapNewtileAvailableOn(tileAsync);
            Assert.That(eventSource.Count().First(), Is.EqualTo(0), TestContext.CurrentContext.Test.GetDescription());
        }

        [Test(Description = "Removing a non empty group from layers empties the collection")]
        public void MapLayers_AfterRemovingNotEmptyGroup_IsEmpty()
        {
            var map = new Map();
            
            var group = CreateLayerGroup();
            group.Layers.Add(new LabelLayer("labels"));
            
            map.Layers.Add(group);

            map.Layers.Remove(group);

            Assert.That(map.Layers, Is.Empty);
        }

        [Test(Description = "The cloning should clone also the LayerGroups")]
        public void Clone_ShoulCloneTheGroups()
        {
            // LayerGroups must be cloned because we want that each instance of the map listens to only the notification events of its children.

            var map = new Map();

            var group = CreateLayerGroup();

            map.Layers.Add(group);

            var clonedMap = map.Clone();

            Assert.That(clonedMap.Layers[0], Is.Not.EqualTo(group), TestContext.CurrentContext.Test.GetDescription());
        }

        private Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> CreateTileAsyncLayer()
        {
            var tileAsync = new Mock<ITileAsyncLayer>();
            tileAsync.SetupAllProperties();
            var layer = tileAsync.As<ILayer>();
            layer.SetupAllProperties();

            return new Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>>(layer, tileAsync);
        }
        private void ReplaceExistingAsyncLayerFromGroup(LayerGroup group, Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> oldTileLayer,
            Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> newTileLayer)
        {
            var oldLayer = oldTileLayer.Item1.Object;
            var idx = group.Layers.IndexOf(oldLayer);
            group.Layers[idx] = newTileLayer.Item1.Object;
        }
        private void RemoveTileLayerFromGroup(LayerGroup group, Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> tileLayer)
        {
            group.Layers.Remove(tileLayer.Item1.Object);
        }
        private void AddTileLayerToLayerGroup(Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> tileLayer, LayerGroup group)
        {
            group.Layers.Add(tileLayer.Item1.Object);
        }
        private void AddTileLayerToMap(Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> tileLayer, Map map, LayerCollectionType collectionType = LayerCollectionType.Background)
        {
            var layer = tileLayer.Item1.Object;

            map.GetCollection(collectionType).Add(layer);
        }
        private LayerGroup CreateLayerGroup(string layerName = "group")
        {
            return new LayerGroup(layerName);
        }
        private void RaiseMapNewtileAvailableOn(Tuple<Mock<ILayer>, Mock<ITileAsyncLayer>> tileAsync)
        {
            tileAsync.Item2.Raise(tal => tal.MapNewTileAvaliable += null, (TileLayer) null, (Envelope) null, (Bitmap) null, 0,
                0, (ImageAttributes) null);
        }
    }
}