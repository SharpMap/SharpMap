using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class MapTest
	{
		[Test]
		public void Initalize_MapInstance()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(2,1));
			Assert.IsNotNull(map);
			Assert.IsNotNull(map.Layers);
			Assert.AreEqual(2f, map.Size.Width);
			Assert.AreEqual(1f, map.Size.Height);
			Assert.AreEqual(System.Drawing.Color.Transparent, map.BackColor);
			Assert.AreEqual(double.MaxValue, map.MaximumZoom);
			Assert.AreEqual(0, map.MinimumZoom);
			Assert.AreEqual(new SharpMap.Geometries.Point(0, 0), map.Center, "map.Center should be initialized to (0,0)");
			Assert.AreEqual(1, map.Zoom, "Map zoom should be initialized to 1.0");
		}

		[Test]
		public void ImageToWorld()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(1000, 500));
			map.Zoom = 360;
			map.Center = new SharpMap.Geometries.Point(0, 0);
			Assert.AreEqual(new SharpMap.Geometries.Point(0, 0), map.ImageToWorld(new System.Drawing.PointF(500, 250)));
			Assert.AreEqual(new SharpMap.Geometries.Point(-180, 90), map.ImageToWorld(new System.Drawing.PointF(0,0)));
			Assert.AreEqual(new SharpMap.Geometries.Point(-180, -90), map.ImageToWorld(new System.Drawing.PointF(0, 500)));
			Assert.AreEqual(new SharpMap.Geometries.Point(180, 90), map.ImageToWorld(new System.Drawing.PointF(1000, 0)));
			Assert.AreEqual(new SharpMap.Geometries.Point(180, -90), map.ImageToWorld(new System.Drawing.PointF(1000, 500)));
		}
		[Test]
		public void WorldToImage()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(1000, 500));
			map.Zoom = 360;
			map.Center = new SharpMap.Geometries.Point(0, 0);
			Assert.AreEqual(new System.Drawing.PointF(500, 250), map.WorldToImage(new SharpMap.Geometries.Point(0, 0)));
			Assert.AreEqual(new System.Drawing.PointF(0, 0), map.WorldToImage(new SharpMap.Geometries.Point(-180, 90)));
			Assert.AreEqual(new System.Drawing.PointF(0, 500), map.WorldToImage(new SharpMap.Geometries.Point(-180, -90)));
			Assert.AreEqual(new System.Drawing.PointF(1000, 0), map.WorldToImage(new SharpMap.Geometries.Point(180, 90)));
			Assert.AreEqual(new System.Drawing.PointF(1000, 500), map.WorldToImage(new SharpMap.Geometries.Point(180, -90)));
		}
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetMap_RenderEmptyMap_ThrowInvalidOperationException()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(2, 1));
			map.GetMap();
		}

		[Test]
		public void GetLayerByName_ReturnCorrectLayer()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 1"));
			map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 3"));
			map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 2"));

			SharpMap.Layers.ILayer layer = map.GetLayerByName("Layer 2");
			Assert.IsNotNull(layer);
			Assert.AreEqual("Layer 2", layer.LayerName);
		}

        [Test]
        public void GetLayerByName_Indexer()
        {
            SharpMap.Map map = new SharpMap.Map();
            map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 1"));
            map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 3"));
            map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 2"));

            SharpMap.Layers.ILayer layer = map.Layers["Layer 2"];
            Assert.IsNotNull(layer);
            Assert.AreEqual("Layer 2", layer.LayerName);
        }

		[Test]
		public void FindLayer_ReturnEnumerable()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 1"));
			map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 3"));
			map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 2"));
			map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 4"));

			int count = 0;
			foreach (SharpMap.Layers.ILayer lay in map.FindLayer("Layer 3"))
			{
				Assert.AreEqual("Layer 3", lay.LayerName);
				count++;
			}
			Assert.AreEqual(1, count);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetExtents_EmptyMap_ThrowInvalidOperationException()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(2, 1));
			map.ZoomToExtents();
		}
		[Test]
		public void GetExtents_ValidDatasource()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(400, 200));
			SharpMap.Layers.VectorLayer vLayer = new SharpMap.Layers.VectorLayer("Geom layer", CreateDatasource());
			map.Layers.Add(vLayer);
			SharpMap.Geometries.BoundingBox box = map.GetExtents();
			Assert.AreEqual(new SharpMap.Geometries.BoundingBox(0, 0, 50, 346.3493254), box);
		}
		[Test]
		public void GetPixelSize_FixedZoom_Return8_75()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(400,200));
			map.Zoom = 3500;
			Assert.AreEqual(8.75,map.PixelSize);
		}
		[Test]
		public void GetMapHeight_FixedZoom_Return1750()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(400, 200));
			map.Zoom = 3500;
			Assert.AreEqual(1750, map.MapHeight);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void SetMinimumZoom_NegativeValue_ThrowException()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.MinimumZoom = -1;
		}
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void SetMaximumZoom_NegativeValue_ThrowException()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.MaximumZoom = -1;
		}
		[Test]
		public void SetMaximumZoom_OKValue()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.MaximumZoom = 100.3;
			Assert.AreEqual(100.3, map.MaximumZoom);
		}
		[Test]
		public void SetMinimumZoom_OKValue()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.MinimumZoom = 100.3;
			Assert.AreEqual(100.3, map.MinimumZoom);
		}
		[Test]
		public void SetZoom_ValueOutsideMax()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.MaximumZoom = 100;
			map.Zoom = 150;
			Assert.AreEqual(100, map.MaximumZoom);
		}
		[Test]
		public void SetZoom_ValueBelowMin()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.MinimumZoom = 100;
			map.Zoom = 50;
			Assert.AreEqual(100, map.MinimumZoom);
		}

		[Test]
		public void ZoomToBox_NoAspectCorrection()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(400,200));
			map.ZoomToBox(new SharpMap.Geometries.BoundingBox(20, 50, 100, 80));
			Assert.AreEqual(new SharpMap.Geometries.Point(60,65), map.Center);
			Assert.AreEqual(80, map.Zoom);
		}
		[Test]
		public void ZoomToBox_WithAspectCorrection()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(400, 200));
			map.ZoomToBox(new SharpMap.Geometries.BoundingBox(20, 10, 100, 180));
			Assert.AreEqual(new SharpMap.Geometries.Point(60, 95), map.Center);
			Assert.AreEqual(340, map.Zoom);
		}

		[Test]
		[ExpectedException(typeof(ApplicationException))]
		public void GetMap_RenderLayerWithoutDatasource_ThrowException()
		{
			SharpMap.Map map = new SharpMap.Map();
			map.Layers.Add(new SharpMap.Layers.VectorLayer("Layer 1"));
			map.GetMap();
		}

		[Test]
		public void WorldToMap_DefaultMap_ReturnValue()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(500,200));
			map.Center = new SharpMap.Geometries.Point(23, 34);
			map.Zoom = 1000;
			System.Drawing.PointF p = map.WorldToImage(new SharpMap.Geometries.Point(8, 50));
			Assert.AreEqual(new System.Drawing.PointF(242.5f, 92), p);
		}
		[Test]
		public void ImageToWorld_DefaultMap_ReturnValue()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(500, 200));
			map.Center = new SharpMap.Geometries.Point(23, 34);
			map.Zoom = 1000;
			SharpMap.Geometries.Point p = map.ImageToWorld(new System.Drawing.PointF(242.5f, 92));
			Assert.AreEqual(new SharpMap.Geometries.Point(8, 50), p);
		}

		[Test]
		public void GetMap_GeometryProvider_ReturnImage()
		{
			SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(400, 200));			
			SharpMap.Layers.VectorLayer vLayer = new SharpMap.Layers.VectorLayer("Geom layer",CreateDatasource());
			vLayer.Style.Outline = new System.Drawing.Pen(System.Drawing.Color.Red, 2f);
			vLayer.Style.EnableOutline = true;
			vLayer.Style.Line = new System.Drawing.Pen(System.Drawing.Color.Green, 2f);
			vLayer.Style.Fill = System.Drawing.Brushes.Yellow;
			map.Layers.Add(vLayer);

			SharpMap.Layers.VectorLayer vLayer2 = new SharpMap.Layers.VectorLayer("Geom layer 2", vLayer.DataSource);
			vLayer2.Style.SymbolOffset = new System.Drawing.PointF(3, 4);
			vLayer2.Style.SymbolRotation = 45;
			vLayer2.Style.SymbolScale = 0.4f;
			map.Layers.Add(vLayer2);

			SharpMap.Layers.VectorLayer vLayer3 = new SharpMap.Layers.VectorLayer("Geom layer 3", vLayer.DataSource);
			vLayer3.Style.SymbolOffset = new System.Drawing.PointF(3, 4);
			vLayer3.Style.SymbolRotation = 45;
			map.Layers.Add(vLayer3);

			SharpMap.Layers.VectorLayer vLayer4 = new SharpMap.Layers.VectorLayer("Geom layer 4", vLayer.DataSource);
			vLayer4.Style.SymbolOffset = new System.Drawing.PointF(3, 4);
			vLayer4.Style.SymbolScale = 0.4f;
			vLayer4.ClippingEnabled = true;
			map.Layers.Add(vLayer4);
			
			map.ZoomToExtents();

			System.Drawing.Image img = map.GetMap();
			Assert.IsNotNull(img);
			map.Dispose();
			img.Dispose();
		}

		private SharpMap.Data.Providers.IProvider CreateDatasource()
		{
			Collection<SharpMap.Geometries.Geometry> geoms = new Collection<SharpMap.Geometries.Geometry>();
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("POINT EMPTY"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), ((5 5, 7 5, 7 7, 5 7, 5 5)))"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("LINESTRING (20 20, 20 30, 30 30, 30 20, 40 20)"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("MULTILINESTRING ((10 10, 40 50), (20 20, 30 20), (20 20, 50 20, 50 60, 20 20))"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20), (21 21, 29 21, 29 29, 21 29, 21 21), (23 23, 23 27, 27 27, 27 23, 23 23))"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("POINT (20.564 346.3493254)"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("MULTIPOINT (20.564 346.3493254, 45 32, 23 54)"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("MULTIPOLYGON EMPTY"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("MULTILINESTRING EMPTY"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("MULTIPOINT EMPTY"));
			geoms.Add(SharpMap.Geometries.Geometry.GeomFromText("LINESTRING EMPTY"));
			return new SharpMap.Data.Providers.GeometryProvider(geoms);
		}
	}
}
