using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;
using NetTopologySuite;
using SharpMap.Layers;
using SharpMap.Rendering.Symbolizer;
using UnitTests.Properties;
using BruTile.Predefined;
using Assembly = System.Reflection.Assembly;

namespace UnitTests.Serialization
{
    [TestFixture]
    public class LayerTest : BaseSerializationTest
    {
       
        [Test] 
        public void TestVectorLayer()
        {
            var vlayerS = new VectorLayer("Test", ProviderTest.CreateProvider());
            VectorLayer vlayerD = null;

            Assert.DoesNotThrow(() => vlayerD = SandD(vlayerS, GetFormatter()), "Exception");
            Assert.IsNotNull(vlayerD, "Deserialized VectorLayer is null");
            
            var vlec = new VectorLayerEqualityComparer();
            Assert.IsTrue(vlec.Equals(vlayerS, vlayerD), vlec.ToString());
        }

        [Test]
        public void TestLabelLayer()
        {
            var vlayerS = new LabelLayer("Test") {DataSource = ProviderTest.CreateProvider() };

            LabelLayer vlayerD = null;

            Assert.DoesNotThrow(() => vlayerD = SandD(vlayerS, GetFormatter()), "Exception");
            Assert.IsNotNull(vlayerD, "Deserialized VectorLayer is null");

            var vlec = new LayerEqualityComparer<LabelLayer>();
            Assert.IsTrue(vlec.Equals(vlayerS, vlayerD), vlec.ToString());
        }

        [Test]
        public void TestGdiImageLayer()
        {
            var tmp = Path.ChangeExtension(Path.GetTempFileName(), "png");
            var wmnStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UnitTests.Resources.Women.png");
            var wmnBmp = new System.Drawing.Bitmap(wmnStream);
            wmnBmp.Save(tmp, ImageFormat.Png);

            var gdiS = new GdiImageLayer("Frau", tmp);
            GdiImageLayer gdiD = null;

            Assert.DoesNotThrow(() => gdiD = SandD(gdiS, GetFormatter()), "Exception");
            Assert.IsNotNull(gdiD, "GdiImageLayer is null");

            Assert.IsTrue(new LayerEqualityComparer<GdiImageLayer>().Equals(gdiS, gdiD));

            gdiS.Dispose();
            gdiD.Dispose();

            File.Delete(tmp);
        }

        [Test]
        public void TestLayerCollection()
        {
            var lcS = new LayerCollection();
            LayerCollection lcD = null;
            Assert.DoesNotThrow(() => lcD = SandD(lcS, GetFormatter()));
            
            TestLayerCollections(lcS,lcD);
        }

        [Test]
        public void TestPuntalLayer()
        {
            var lS = new SharpMap.Layers.Symbolizer.PuntalVectorLayer("PuntalTest", ProviderTest.CreateProvider());
            var rsS = (RasterPointSymbolizer) lS.Symbolizer;
            rsS.Transparency = 0.2f;

            SharpMap.Layers.Symbolizer.PuntalVectorLayer lD = null;

            Assert.DoesNotThrow(() => lD = SandD(lS, GetFormatter()));
            Assert.IsNotNull(lD);

            Assert.IsInstanceOf(lS.Symbolizer.GetType(), lD.Symbolizer);
            var rsD = (RasterPointSymbolizer)lD.Symbolizer;
            Assert.AreEqual(rsS.Transparency, rsD.Transparency);
        }

        [Test, Ignore("This requires BruTile.Serialization")]
        public void TestTileLayer()
        {
            var tlS =
                new TileLayer(KnownTileSources.Create(KnownTileSource.BingHybridStaging, string.Empty), 
                    "Name", System.Drawing.Color.MediumTurquoise, true, Path.Combine(Path.GetTempPath(), "Bing", "Hybrid"));

            TileLayer tlD = null;
            Assert.DoesNotThrow(() => tlD = SandD(tlS, GetFormatter()));
            Assert.IsNotNull(tlD);
        }

        private static void TestLayerCollections(LayerCollection lhs, LayerCollection rhs)
        {
            Assert.IsNotNull(rhs);

            Assert.AreEqual(lhs.Count, rhs.Count);
            for (var i = 0; i < lhs.Count; i++)
                Assert.AreEqual(lhs[i].LayerName, rhs[i].LayerName);
        }

        internal class ILayerEqualityComparer<T> : EqualityComparer<T> where T : ILayer
        {
            public readonly List<string> DifferAt = new List<string>();

            public override bool Equals(T lhs, T rhs)
            {
                var result = true;
                if (lhs.Enabled != rhs.Enabled)
                {
                    result = false;
                    DifferAt.Add("Enabled");
                }

                if (!lhs.Envelope.Equals(rhs.Envelope))
                {
                    result = false;
                    DifferAt.Add("Envelope");
                }

                if (!string.Equals(lhs.LayerName, rhs.LayerName))
                {
                    DifferAt.Add("LayerName");
                    result = false;
                }

                if (lhs.MaxVisible != rhs.MaxVisible)
                {
                    DifferAt.Add("MaxVisible");
                    result = false;
                }

                if (lhs.MinVisible != rhs.MinVisible)
                {
                    DifferAt.Add("MaxVisible");
                    result = false;
                }

                if (!string.Equals(lhs.Proj4Projection, rhs.Proj4Projection))
                {
                    DifferAt.Add("Proj4Projection");
                    result = false;
                }

                if (lhs.SRID != rhs.SRID)
                {
                    DifferAt.Add("SRID");
                    result = false;
                }

                if (lhs.TargetSRID != rhs.TargetSRID)
                {
                    DifferAt.Add("TargetSRID");
                    result = false;
                }

                return result;
            }

            public sealed override int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }

            public override string ToString()
            {
                if (DifferAt.Count == 0)
                    return typeof(T).Name + "s are equal!";
                return string.Format("{0}s differ at \n - {1}", typeof(T).Name, string.Join(", \n - ", DifferAt));
            }
        }

        internal class LayerEqualityComparer<T> : ILayerEqualityComparer<T> where T:Layer
        {
            public override bool Equals(T lhs, T rhs)
            {
                var res = base.Equals(lhs, rhs);

                if (lhs.CoordinateTransformation == null ^ rhs.CoordinateTransformation == null)
                {
                    DifferAt.Add("CoordinateTransformation (== null)");
                    res = false;
                }

                //ToDo compare transformation settings
                if (lhs.ReverseCoordinateTransformation == null ^ rhs.ReverseCoordinateTransformation == null)
                {
                    DifferAt.Add("ReverseCoordinateTransformation (== null)");
                    res = false;
                }

                //ToDo compare transformation settings

                if (lhs.Style.Equals(rhs.Style))
                {
                    DifferAt.Add("Style");
                    res = false;
                }

                return res;

            }
        }

        internal class VectorLayerEqualityComparer : LayerEqualityComparer<VectorLayer>
        {
            public override bool Equals(VectorLayer lhs, VectorLayer rhs)
            {
                var result = base.Equals(lhs, rhs);
                
                if (lhs.ClippingEnabled != rhs.ClippingEnabled)
                {
                    DifferAt.Add("ClippingEnabled");
                    result = false;
                }

                if (lhs.IsQueryEnabled != rhs.IsQueryEnabled)
                {
                    DifferAt.Add("IsQueryEnabled");
                    result = false;
                }

                if (lhs.DataSource == null ^ rhs.DataSource == null)
                {
                    DifferAt.Add("DataSource");
                    result = false;
                }

                if (lhs.SmoothingMode != rhs.SmoothingMode)
                {
                    DifferAt.Add("SmoothingMode");
                    result = false;
                }

                if (lhs.Theme == null ^ rhs.Theme == null)
                {
                    DifferAt.Add("Theme");
                    result = false;
                }

                //ToDO Compare Theme settings

                if (lhs.Themes == null ^ rhs.Themes == null)
                {
                    DifferAt.Add("Themes");
                    result = false;
                }

                return result;
            }
        }
    }
}
