
namespace UnitTests.Data.Providers
{

    [NUnit.Framework.TestFixture]
    public class ShapeFileProviderTests : ProviderTest
    {
        
        private long _msLineal;
        private long _msVector;
        private const int NumberOfRenderCycles = 1;

        [NUnit.Framework.OneTimeTearDown]
        public void OneTimeTearDown()
        {
            System.Diagnostics.Trace.WriteLine("Speed comparison:");
            System.Diagnostics.Trace.WriteLine("VectorLayer\tLinealLayer\tRatio");
            System.Diagnostics.Trace.WriteLine(string.Format("{0}\t{1}\t{2:N}", _msVector, _msLineal,
                                              ((double)_msLineal / _msVector * 100)));
        }

        [NUnit.Framework.Test]
        public void TestReadValueZFromPointZShapeFile()
        {
            string file = TestUtility.GetPathToTestFile("Point_With_Z.shp");
            var sh = new SharpMap.Data.Providers.ShapeFile(file, true);
            sh.Open();
            int fc = sh.GetFeatureCount();
            NUnit.Framework.Assert.AreEqual(1149, fc);
            NUnit.Framework.Assert.AreEqual(0, sh.GetObjectIDsInView(sh.GetExtents())[0]);
            var featsInView = sh.GetGeometriesInView(new GeoAPI.Geometries.Envelope(sh.GetExtents()));
            NUnit.Framework.Assert.AreEqual(1149, featsInView.Count);
            foreach (var item in featsInView)
            {
                NUnit.Framework.Assert.IsNotNull(item.Coordinate.Z);
            }
            NUnit.Framework.Assert.AreEqual(featsInView[0].Coordinate.Z, 146.473);
            NUnit.Framework.Assert.AreEqual(featsInView[1].Coordinate.Z, 181.374);
            NUnit.Framework.Assert.AreEqual(featsInView[2].Coordinate.Z, 146.676);
            NUnit.Framework.Assert.AreEqual(featsInView[3].Coordinate.Z, 181.087);
            NUnit.Framework.Assert.AreEqual(featsInView[4].Coordinate.Z, 169.948);
            NUnit.Framework.Assert.AreEqual(featsInView[5].Coordinate.Z, 169.916);

            sh.Close();
        }

        [NUnit.Framework.Test]
        public void TestReadValueZFromLineStringZShapeFile()
        {
            string file = TestUtility.GetPathToTestFile("LineString_With_Z.shp");
            var sh = new SharpMap.Data.Providers.ShapeFile(file, true);
            sh.Open();
            int fc = sh.GetFeatureCount();
            NUnit.Framework.Assert.AreEqual(1221, fc);
            NUnit.Framework.Assert.AreEqual(0, sh.GetObjectIDsInView(sh.GetExtents())[0]);
            var featsInView = sh.GetGeometriesInView(new GeoAPI.Geometries.Envelope(sh.GetExtents()));
            NUnit.Framework.Assert.AreEqual(1221, featsInView.Count);
            foreach (var item in featsInView)
            {
                NUnit.Framework.Assert.IsNotNull(item.Coordinate.Z);
            }
            NUnit.Framework.Assert.AreEqual(featsInView[0].Coordinates[0].Z, 35.865);
            NUnit.Framework.Assert.AreEqual(featsInView[0].Coordinates[1].Z, 35.743);
            
            NUnit.Framework.Assert.AreEqual(featsInView[1].Coordinates[0].Z, 35.518);
            NUnit.Framework.Assert.AreEqual(featsInView[1].Coordinates[1].Z, 35.518);
            
            NUnit.Framework.Assert.AreEqual(featsInView[2].Coordinates[0].Z, 37.438);
            NUnit.Framework.Assert.AreEqual(featsInView[2].Coordinates[1].Z, 37.441);
            
            NUnit.Framework.Assert.AreEqual(featsInView[3].Coordinates[0].Z, 37.441);
            NUnit.Framework.Assert.AreEqual(featsInView[3].Coordinates[1].Z, 37.441);

            sh.Close();
        }

        [NUnit.Framework.Test]
        public void UsingTest()
        {
            using (var s = System.IO.File.Open(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp"), System.IO.FileMode.Open))
            {
                using (var reader = new System.IO.BinaryReader(s))
                {
                    System.Diagnostics.Trace.WriteLine(reader.ReadInt32());
                }
                NUnit.Framework.Assert.Throws<System.ObjectDisposedException>(() => System.Diagnostics.Trace.WriteLine(s.Position));
            } 
        }

        private static void CopyShapeFile(string path, out string tmp)
        {
            tmp = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(),".shp");
            if (!System.IO.File.Exists(path)) throw new NUnit.Framework.IgnoreException("File not found");
            foreach (var file in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".*"))
            {
                var copyFile = System.IO.Path.ChangeExtension(tmp, System.IO.Path.GetExtension(file));
                if (System.IO.File.Exists(copyFile)) System.IO.File.Delete(copyFile);
                System.IO.File.Copy(file, copyFile);
            }
        }

        [NUnit.Framework.Test]
        public void TestDeleteAfterClose()
        {
            string test;
            CopyShapeFile(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp"), out test);

            var shp = new SharpMap.Data.Providers.ShapeFile(test);
            shp.Open();
            shp.Close();
            var succeeded = true;
            foreach (var file in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(test), System.IO.Path.GetFileNameWithoutExtension(test) + ".*"))
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (System.Exception)
                {
                    System.Diagnostics.Trace.WriteLine("Failed to delete '{0}'", file);
                    succeeded = false;
                }
            }
            NUnit.Framework.Assert.IsTrue(succeeded);
        }

        [NUnit.Framework.Test]
        public void TestReadPointZShapeFile()
        {
            string file = TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp");
            var sh = new SharpMap.Data.Providers.ShapeFile(file, true);
            sh.Open();
            int fc = sh.GetFeatureCount();
            NUnit.Framework.Assert.AreEqual(4342, fc);
            NUnit.Framework.Assert.AreEqual(0, sh.GetObjectIDsInView(sh.GetExtents())[0]);
            var featsInView = sh.GetGeometriesInView(new GeoAPI.Geometries.Envelope(sh.GetExtents()));
            NUnit.Framework.Assert.AreEqual(4342, featsInView.Count);
            sh.Close();
        }

        [NUnit.Framework.Test]
        public void TestPerformanceVectorLayer()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp")),
                                          "Specified shapefile is not present!");

            var map = new SharpMap.Map(new System.Drawing.Size(1024, 768));

            var shp = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp"), false, false);
            var lyr = new SharpMap.Layers.VectorLayer("Roads", shp);

            map.Layers.Add(lyr);
            map.ZoomToExtents();

            RepeatedRendering(map, shp.GetFeatureCount(), NumberOfRenderCycles, out _msVector);

            string path = System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "SPATIAL_F_SKARVMUFF.shp");
            path = System.IO.Path.ChangeExtension(path, ".vector.png");
            using (var res = map.GetMap())
                res.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            System.Diagnostics.Trace.WriteLine("\nResult saved at file://" + path.Replace('\\', '/'));
        }

        [NUnit.Framework.Test]
        public void TestPerformanceLinealLayer()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(TestUtility.GetPathToTestFile("roads_ugl.shp")),
                                          "Specified shapefile is not present!");

            var map = new SharpMap.Map(new System.Drawing.Size(1024, 768));

            var shp = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("roads_ugl.shp"), false, false);
            var lyr = new SharpMap.Layers.Symbolizer.LinealVectorLayer("Roads", shp)
                          {
                              Symbolizer =
                                  new SharpMap.Rendering.Symbolizer.BasicLineSymbolizer
                                      {Line = new System.Drawing.Pen(System.Drawing.Color.Black)}
                          };
            map.Layers.Add(lyr);
            map.ZoomToExtents();

            RepeatedRendering(map, shp.GetFeatureCount(), NumberOfRenderCycles, out _msLineal);
            System.Diagnostics.Trace.WriteLine("\nWith testing if record is deleted ");
            
            shp.CheckIfRecordIsDeleted = true;
            long tmp;
            RepeatedRendering(map, shp.GetFeatureCount(), 1, out tmp);

            var path = System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "SPATIAL_F_SKARVMUFF.shp");
            path = System.IO.Path.ChangeExtension(path, "lineal.png");
            using (var res = map.GetMap())
                res.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            System.Diagnostics.Trace.WriteLine("\nResult saved at file://" + path.Replace('\\', '/'));
        }

        private static void RepeatedRendering(SharpMap.Map map, int numberOfFeatures, int numberOfTimes, out long avgRenderTime)
        {
            System.Diagnostics.Trace.WriteLine("Rendering Map with " + numberOfFeatures + " features");
            var totalRenderTime = 0L;
            var sw = new System.Diagnostics.Stopwatch();
            for (var i = 1; i <= numberOfTimes; i++)
            {
                System.Diagnostics.Trace.Write(string.Format("Rendering {0}x time(s)", i));
                sw.Start();
                map.GetMap();
                sw.Stop();
                System.Diagnostics.Trace.WriteLine(" in " +
                                         sw.ElapsedMilliseconds.ToString(
                                             System.Globalization.NumberFormatInfo.CurrentInfo) + "ms.");
                totalRenderTime += sw.ElapsedMilliseconds;
                sw.Reset();
            }

            avgRenderTime = totalRenderTime/numberOfTimes;
            System.Diagnostics.Trace.WriteLine("\n Average rendering time:" + avgRenderTime.ToString(
                                         System.Globalization.NumberFormatInfo.CurrentInfo) + "ms.");
        }

        [NUnit.Framework.Test]
        public void TestGetFeature()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp")),
                                          "Specified shapefile is not present!");

            var shp = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp"), false, false);
            shp.Open();
            var feat = shp.GetFeature(0);
            NUnit.Framework.Assert.IsNotNull(feat);
            shp.Close();
        }

        [NUnit.Framework.Test]
        public void TestExecuteIntersectionQuery()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp")),
                                          "Specified shapefile is not present!");

            var shp = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp"), false, false);
            shp.Open();

            var fds = new SharpMap.Data.FeatureDataSet();
            var bbox = shp.GetExtents();
            //narrow it down
            bbox.ExpandBy(-0.425*bbox.Width, -0.425*bbox.Height);

            //Just to avoid that initial query does not impose performance penalty
            shp.DoTrueIntersectionQuery = false;
            shp.ExecuteIntersectionQuery(bbox, fds);
            fds.Tables.RemoveAt(0);

            //Perform query once more
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            shp.ExecuteIntersectionQuery(bbox, fds);
            sw.Stop();
            System.Diagnostics.Trace.WriteLine("Queried using just envelopes:\n" + fds.Tables[0].Rows.Count + " in " +
                                     sw.ElapsedMilliseconds + "ms.");
            fds.Tables.RemoveAt(0);
            
            shp.DoTrueIntersectionQuery = true;
            sw.Reset();
            sw.Start();
            shp.ExecuteIntersectionQuery(bbox, fds);
            sw.Stop();
            System.Diagnostics.Trace.WriteLine("Queried using prepared geometries:\n" + fds.Tables[0].Rows.Count + " in " +
                                     sw.ElapsedMilliseconds + "ms.");
        }

        [NUnit.Framework.Test]
        public void TestExecuteIntersectionQueryWithFilterDelegate()
        {
            NUnit.Framework.Assert.IsTrue(System.IO.File.Exists(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp")),
                                          "Specified shapefile is not present!");

            var shp = new SharpMap.Data.Providers.ShapeFile(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.shp"), false, false);
            shp.Open();

            var fds = new SharpMap.Data.FeatureDataSet();
            var bbox = shp.GetExtents();
            //narrow it down
            bbox.ExpandBy(-0.425*bbox.Width, -0.425*bbox.Height);

            //Just to avoid that initial query does not impose performance penalty
            shp.DoTrueIntersectionQuery = false;
            shp.FilterDelegate = JustTracks;

            shp.ExecuteIntersectionQuery(bbox, fds);
            fds.Tables.RemoveAt(0);

            //Perform query once more
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            shp.ExecuteIntersectionQuery(bbox, fds);
            sw.Stop();
            System.Diagnostics.Trace.WriteLine("Queried using just envelopes:\n" + fds.Tables[0].Rows.Count + " in " +
                                     sw.ElapsedMilliseconds + "ms.");
            fds.Tables.RemoveAt(0);
            
            shp.DoTrueIntersectionQuery = true;
            sw.Reset();
            sw.Start();
            shp.ExecuteIntersectionQuery(bbox, fds);
            sw.Stop();
            System.Diagnostics.Trace.WriteLine("Queried using prepared geometries:\n" + fds.Tables[0].Rows.Count + " in " +
                                     sw.ElapsedMilliseconds + "ms.");
        }

        public static bool JustTracks(SharpMap.Data.FeatureDataRow fdr)
        {
            //System.Diagnostics.Trace.WriteLine(fdr [0] + ";"+ fdr[4]);
            var s = fdr[4] as string;
            if (s != null)
                return s == "track";
            return true;
        }
    }
}
