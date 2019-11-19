using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BruTile;
using BruTile.Cache;
using BruTile.MbTiles;
using BruTile.Predefined;
using BruTile.Web;
using BruTile.Wmts;
using NUnit.Framework;
using SharpMap.Utilities;

namespace UnitTests.Serialization
{
    [Category("BruTile")]
    public class SerializationTests
    {
        [Test]
        public void TestExtent()
        {
            var e1 = new Extent(-10, -10, 10, 10);
            var e2 = SandD(e1);
            Assert.AreEqual(e1.CenterX, e2.CenterX);
            Assert.AreEqual(e1.CenterY, e2.CenterY);
            Assert.AreEqual(e1.Width, e2.Width);
            Assert.AreEqual(e1.Height, e2.Height);
        }

        [Test]
        public void TestResolution()
        {
            var r1 = new Resolution("XXX", 1245, 999,999, 5, 5, 4, 4, 0.1);
            var r2 = SandD(r1);
            Assert.AreEqual(r1.Id, r2.Id);
            Assert.AreEqual(r1.UnitsPerPixel, r2.UnitsPerPixel);
            Assert.AreEqual(r1.TileWidth, r2.TileWidth);
            Assert.AreEqual(r1.TileHeight, r2.TileHeight);
            Assert.AreEqual(r1.MatrixWidth, r2.MatrixWidth);
            Assert.AreEqual(r1.MatrixHeight, r2.MatrixHeight);
            Assert.AreEqual(r1.Top, r2.Top);
            Assert.AreEqual(r1.Left, r2.Left);
            Assert.AreEqual(r1.ScaleDenominator, r2.ScaleDenominator);
        }

        [Test]
        public void TestGlobalMercatorSchema()
        {
            string message;
            var s1 = new GlobalMercator("png", 2, 11);
            var s2 = SandD(s1);
            var equal = EqualTileSchemas(s1, s2, out message);
            Assert.IsTrue(equal, message);
        }

        [Test]
        public void TestGlobalSphericalMercatorSchema()
        {
            string message;
            var s1 = new GlobalSphericalMercator("png", YAxis.OSM, 2, 11);
            var s2 = SandD(s1);
            var equal = EqualTileSchemas(s1, s2, out message);
            Assert.IsTrue(equal, message);
        }

        [Test]
        public void TestWkstNederlandSchema()
        {
            string message;
            var s1 = new WkstNederlandSchema();
            var s2 = SandD(s1);
            var equal = EqualTileSchemas(s1, s2, out message);
            Assert.IsTrue(equal, message);
        }

        #region Caches

        [Test]
        public void TestMemoryCacheBytes()
        {
            var c1 = new MemoryCache<byte[]>(17, 34);
            var c2 = SandD(c1);

            Assert.NotNull(c2);
#if DEBUG
            Assert.AreEqual(c1.MinTiles, c2.MinTiles);
            Assert.AreEqual(c1.MaxTiles, c2.MaxTiles);
#endif

        }

        
        [Test]
        public void TestMemoryCacheBitmap()
        {
            var c1 = new BruTile.Cache.MemoryCache<System.Drawing.Bitmap>(17, 34);
            var c2 = SandD(c1);

            Assert.NotNull(c2);
#if DEBUG
            Assert.AreEqual(c1.MinTiles, c2.MinTiles);
            Assert.AreEqual(c1.MaxTiles, c2.MaxTiles);

            //Assert.IsTrue(c1.EqualSetup(c2));
#endif
        }
        
        [Test]
        public void FileCache()
        {
            var c1 = new FileCache(Path.Combine(Path.GetTempPath(), "_test"), "jpg", new TimeSpan(8, 22, 19, 35));
            var c2 = SandD(c1);
            Assert.NotNull(c2);
#if DEBUG
            Assert.AreEqual(ReflectionTests.GetFieldValue(c1, "_cacheExpireTime"),
                            ReflectionTests.GetFieldValue(c2, "_cacheExpireTime"), "CacheExpireTimes not equal");
            Assert.AreEqual(ReflectionTests.GetFieldValue(c1, "_directory"),
                            ReflectionTests.GetFieldValue(c2, "_directory"), "Directories not equal");
            Assert.AreEqual(ReflectionTests.GetFieldValue(c1, "_format"),
                            ReflectionTests.GetFieldValue(c2, "_format"), "Formats not equal");


            //ToDo: Test by reflection
            //Assert.IsTrue(c1.EqualSetup(c2));
#endif
        }

        #endregion

        #region Tile sources

        [Ignore("Needs internet connection")]
        [Category("Internet")]
        [TestCase("http://tiles.geoservice.dlr.de/service/wmts?SERVICE=WMTS&REQUEST=GetCapabilities")]
        public void TestWmtsTileSource(string url)
        {
            IEnumerable<ITileSource> sources;
            try
            {
                var req = WebRequest.Create(url);
                using (var resp = req.GetResponse())
                    sources = WmtsParser.Parse(resp.GetResponseStream());

            }
            catch (Exception ex)
            {
                throw new IgnoreException("Getting the response stream failed", ex);
            }
            
            if (sources == null)
                throw new IgnoreException("Failed to get capabilities");

            var equal = true;
            var messages = new List<string>();
            System.Diagnostics.Trace.WriteLine($"Testing {url}");
            foreach (var srcS in sources)
            {
                var srcD = SandD(srcS);
                Assert.NotNull(srcD);
                System.Diagnostics.Trace.WriteLine($"{srcS}");

                string message;
                if (!EqualTileSources(srcS, srcD, out message))
                {
                    messages.Add(message);
                    equal = false;
                }
            }
            Assert.IsTrue(equal, string.Join("\n", messages));
        }

        [Test]
        public void TestOsmTileSource()
        {
            var ts1 = KnownTileSources.Create(KnownTileSource.StamenTonerLite, null, new FakePersistentCache<byte[]>());
            var ts2 = SandD(ts1);

            Assert.NotNull(ts2);
            string message;
            var equal = EqualTileSources(ts1, ts2, out message);
            Assert.IsTrue(equal, message);
        }

        [TestCase(@"C:\Users\obe.IVV-AACHEN\Downloads\geography-class.mbtiles")]
        [Ignore("Test a path to a folder on a specific machine")]
        public void TestMbTiles(string mbTilesFile)
        {
            if (!File.Exists(mbTilesFile))
                throw new IgnoreException(string.Format("File '{0}' does not exist.", mbTilesFile));

            var cn = new SQLite.SQLiteConnectionString(mbTilesFile, false);
            var p1 = new MbTilesTileSource(cn);
            var p2 = SandD(p1);
            Assert.IsNotNull(p2);
            //Assert.AreEqual(p1.Format, p2.Format, "MbTiles Format not equal");
            Assert.AreEqual(p1.Type, p2.Type, "MbTiles Type not equal");
            string msg;
            Assert.IsTrue(EqualTileSources(p1, p2, out msg), msg);
            //Assert.IsTrue(EqualTileSchemas(p1.Schema, p2.Schema, out msg), msg);
        }

        [Test]
        public void TestKnownTileSources()
        {
            foreach (KnownTileSource kts in Enum.GetValues(typeof(KnownTileSource)))
            {
                try
                {
                    var srcS = KnownTileSources.Create(kts);
                    var srcD = SandD(srcS);
                    Assert.IsNotNull(srcD);
                    string message;
                    Assert.IsTrue(EqualTileSources(srcS, srcD, out message));
                }
                catch
                {
                }
            }

        }

        [Test]
        public void TestHttpTileProvider()
        {
            var srcS = new HttpTileProvider(new BasicRequest("http://{s}.tile.opencyclemap.org/cycle/{z}/{x}/{y}.png", new [] {"a", "b"}), userAgent: "HUHU");
            var srcD = SandD(srcS);
            Assert.IsNotNull(srcD);
        }

        [Test]
        public void TestNullCache()
        {
            var srcS = new NullCache();
            var srcD = SandD(srcS);
            Assert.IsNotNull(srcD);
        }

        [TestCase("http://{s}.tile.opencyclemap.org/cycle/{z}/{x}/{y}.png", new[] { "a", "b" }, null)]
        public void TestBasicRequest(string urlFormatter, IEnumerable<string> serverNodes, string apiKey)
        {
            var srcS = new BasicRequest(urlFormatter, serverNodes, apiKey);
            var srcD = SandD(srcS);
            Assert.IsNotNull(srcD);
        }
        #endregion

        #region private helper methods

        private static bool EqualTileSources(ITileSource ts1, ITileSource ts2, out string message)
        {
            if (!ReferenceEquals(ts1, ts2))
            {
                if (ts1 == null)
                {
                    message = "The reference tile source is null!";
                    return false;
                }
                if (ts2 == null)
                {
                    message = "One of the tile sources is null, and the other not";
                    return false;
                }

                if (!EqualTileSchemas(ts1.Schema, ts2.Schema, out message))
                    return false;

                if (!EqualTileProviders(ts1.Schema, (ITileProvider)ts1, (ITileProvider)ts2, out message))
                    return false;
            }
            
            message = "Tile sources seem to be equal";
            return true;
        }

        private static bool EqualTileProviders(ITileSchema schema, ITileProvider tp1, ITileProvider tp2, out string message)
        {
            if (!ReferenceEquals(tp1, tp2))
            {
                if (tp1 == null)
                {
                    message = "The reference tile provider is null!";
                    return false;
                }
                if (tp2 == null)
                {
                    message = "One of the tile providers is null, and the other not";
                    return false;
                }

                if (tp1.GetType() != tp2.GetType())
                {
                    message = "Tile providers are of different type";
                    return false;
                }

                var keys = schema.Resolutions.Keys.ToArray();
                var minkey = GetIndex(keys[0]);
                var maxKey = GetIndex(keys[keys.Length - 1]);
                var prefix = GetPrefix(keys[0]);
                for (var i = 0; i < 3; i++)
                {
                    TileInfo ti = null;
                    try
                    {
                        ti = RandomTileInfo(prefix + "{0}", minkey, Math.Min(maxKey, 12));
                        var req1 = tp1 as IRequest;
                        var req2 = tp2 as IRequest;
                        if (req1 != null && req2 != null)
                        {
                            if (req1.GetUri(ti) != req2.GetUri(ti))
                            {
                                message = "Request builders produce different uris for the same request";
                                return false;
                            }
                        }
                        else
                        {
                            var t1 = tp1.GetTile(ti);
                            var t2 = tp2.GetTile(ti);
                            if (!TilesEqual(t1, t2))
                            {
                                message = "Request builders produce different results for same tile request";
                                return false;
                            }
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format(
                            "TileInfo({4}): {0}, {1}, {2}\n{3}", 
                            ti.Index.Level, ti.Index.Col, ti.Index.Row, ex.Message, i));
                    }
                    catch(WebException ex)
                    {
                        if (ti == null)
                            System.Diagnostics.Trace.WriteLine("No tile info!");
                        else
                            System.Diagnostics.Trace.WriteLine(string.Format(
                                "TileInfo({5}): {0}, {1}, {2}\n{3}\n{4}", 
                                ti.Index.Level, ti.Index.Col, ti.Index.Row,
                                ex.Message, ex.Response.ResponseUri, i));
                    }
                }
            }

            message = "Tile providers appear to be equal";
            return true;
        }

        private static int GetIndex(string s)
        {
            var pos = s.LastIndexOf(":", StringComparison.Ordinal);
            if (pos == 0)
                return int.Parse(s, NumberFormatInfo.InvariantInfo);
            return int.Parse(s.Substring(pos + 1), NumberFormatInfo.InvariantInfo);
        }

        private static string GetPrefix(string s)
        {
            var pos = s.LastIndexOf(":", StringComparison.Ordinal);
            if (pos == -1)
                return string.Empty;

            return s.Substring(0, pos+1);

        }

        private static bool TilesEqual(IEnumerable<byte> t1, IList<byte> t2)
        {
            if (ReferenceEquals(t1, t2))
                return true;

            if (t1 == null ^ t2 == null)
                return false;

            if (t1 == null)
                return false;

            return !t1.Where((t, i) => t != t2[i]).Any();
        }

        /*
        private static bool EqualRequestBuilders(IRequest ts1, IRequest ts2, out string message)
        {
            if (!ReferenceEquals(ts1, ts2))
            {
                if (ts1 == null ^ ts2 == null)
                {
                    message = "One of the request builders is null, and the other not";
                    return false;
                }

                if (ts1 == null)
                {
                    message = "Reference request builder is null";
                    return false;
                }

                for (var i = 0; i < 50; i++)
                {
                    var rnd = RandomTileInfo();
                    var uri1 = ts1.GetUri(rnd);
                    var uri2 = ts2.GetUri(rnd);

                    if (uri1 != uri2)
                    {
                        message = "Request builders produce different results for same tile request";
                        return false;
                    }
                }
            }
            message = "Request builder seem to be equal";
            return true;
        }
         */

        private static readonly Random Rnd = new Random();
        private static TileInfo RandomTileInfo(string format = "{0}", int minLevel = 3, int maxLevel = 12)
        {
            var level = Rnd.Next(minLevel, maxLevel);
            var max = (int)Math.Pow(2, level) - 1;
            return new TileInfo
            {
                Extent = new Extent(),
                Index = new TileIndex(Rnd.Next(0, max), Rnd.Next(0, max), string.Format(CultureInfo.InvariantCulture, format, level))
            };
        }

        private static bool EqualTileSchemas(ITileSchema ts1, ITileSchema ts2, out string message)
        {
            if (ts1.Name != ts2.Name)
            {
                message = "Names don't match";
                return false;
            }

            if (ts1.Srs != ts2.Srs)
            {
                message = "Srs' don't match";
                return false;
            }

            if (ts1.Format != ts2.Format)
            {
                message = "Formats don't match";
                return false;
            }

            if (ts1.Extent != ts2.Extent)
            {
                message = "Extents don't match";
                return false;
            }

            var tts1 = ts1 as TileSchema;
            var tts2 = ts2 as TileSchema;
            if (tts1 != null && tts2 != null)
            {
                if (tts1.Resolutions.Count != tts2.Resolutions.Count)
                {
                    message = "Number of resolutions don't match";
                    return false;
                }

                if (tts1.OriginX != tts2.OriginX)
                {
                    message = "OriginX' don't match";
                    return false;
                }

                if (tts1.OriginY != tts2.OriginY)
                {
                    message = "OriginYs don't match";
                    return false;
                }

                foreach(var res1 in tts1.Resolutions.Values)
                {
                    var res2 = tts2.Resolutions[res1.Id];
                    if (tts1.GetTileHeight(res1.Id)  != tts2.GetTileHeight(res2.Id))
                    {
                        message = "Heights don't match";
                        return false;
                    }

                    if (tts1.GetTileWidth(res1.Id) != tts2.GetTileWidth(res2.Id))
                    {
                        message = "Widths don't match";
                        return false;
                    }
                }

            }

            var wts1 = ts1 as WmtsTileSchema;
            var wts2 = ts2 as WmtsTileSchema;
            if (wts1 != null && wts2 != null)
            {
                if (wts1.Abstract != wts2.Abstract)
                {
                    message = "Abstracts don't match";
                    return false;
                }
                if (wts1.Style != wts2.Style)
                {
                    message = "Styles don't match";
                    return false;
                }
                if (wts1.Identifier != wts2.Identifier)
                {
                    message = "Identifiers don't match";
                    return false;
                }
                if (wts1.SupportedSRS.ToString() != wts2.SupportedSRS.ToString())
                {
                    message = "SupportedSRS' don't match";
                    return false;
                }
            }

            if (ts1.Resolutions.Count != ts2.Resolutions.Count)
            {
                message = "Number of resolutions doesn't match";
                return false;
            }

            foreach (var key in ts1.Resolutions.Keys)
            {
                var r1 = ts1.Resolutions[key];
                var r2 = ts2.Resolutions[key];

                //!!! compare other resultion parameters.
                
                if (r1.Id != r2.Id || r1.UnitsPerPixel != r2.UnitsPerPixel)
                {
                    message = string.Format("Resolution doesn't match at index {0}", key);
                    return false;
                }
            }

            message = "Schemas are equal!";
            return true;
        }

        private static T SandD<T>(T obj, IFormatter formatter = null)
        {
            if (formatter == null)
            {
                formatter = new BinaryFormatter();
                formatter.AddBruTileSurrogates();
            }

            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(ms);
            }
        }

        #endregion
    }
}
