// Copyright 2012 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Data.Providers.FileGdb.
// SharpMap.Data.Providers.FileGdb is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Data.Providers.FileGdb is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
#if DEBUG

using System;
//using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using SharpMap.Layers;

namespace SharpMap.Data.Providers
{

    [TestFixture]
    public class FileGdbProviderTest
    {
        /*
        [Test]
        public void TestEnsureNativePathSet()
        {
            var p = new FileGdbProvider();
            p.Dispose();

            var asr = new AppSettingsReader();
            var fileGdbPath = (string)asr.GetValue("FileGdbNativeDirectory", typeof (string));
            var path = Environment.GetEnvironmentVariable("PATH");
            Assert.IsFalse(string.IsNullOrEmpty(path));
            Assert.IsTrue(path.Contains(fileGdbPath));
        }
        */

        [Test]
        public void TestSampleSources()
        {
            var di = new DirectoryInfo(@"D:\GIS\FileGDB\samples\data");

            foreach (var tmp in di.EnumerateDirectories())
            {
                var p = new FileGdbProvider(tmp.FullName);
                Console.WriteLine("\n" + p.ConnectionID);
                foreach (var table in p.EnumerateTables())
                {
                    Console.WriteLine("\t" + table);
                }
            }
        }

        private static readonly Random Rnd = new Random();
        private static Color RandomColor()
        {
            return Color.FromArgb(Rnd.Next(31,128), Rnd.Next(256), Rnd.Next(256), Rnd.Next(256));
        }

        private static Brush RandomBrush()
        {
            return new SolidBrush(RandomColor());
        }

        private static Pen RandomPen()
        {
            return new Pen(RandomColor(), Rnd.Next(1, 3));
        }

        [Test]
        public void TestMap()
        {
            var m = new Map(new Size(1024, 786)) {BackColor = Color.FloralWhite};
            const string samplePath = @"D:\GIS\FileGDB\samples\data\Topo.gdb";

            var p = new FileGdbProvider(samplePath);

            foreach (var fc in p.GetFeatureClasses("\\USA"))
            {
                if (fc.StartsWith("\\USA\\T"))
                    continue;

                Console.WriteLine(fc);
                var pUse = new FileGdbProvider(samplePath) { Table = fc };
                var vl = new VectorLayer("Layer:" + fc, pUse)
                             {
                                 SmoothingMode = SmoothingMode.HighQuality,
                                 Style = {Fill = RandomBrush(), Line = RandomPen()}
                             };
                m.Layers.Add(vl);

                var fds = new FeatureDataSet();
                vl.ExecuteIntersectionQuery(vl.Envelope, fds);
                fds.Tables[0].TableName = fc;
                var res = fds.Tables[0].Rows[0].ItemArray;
                foreach (DataColumn col in fds.Tables[0].Columns)
                    Console.Write(string.Format("{0} [{1}], ", col.ColumnName, col.DataType));
                Console.WriteLine();

                foreach (var item in res)
                    Console.Write(string.Format(CultureInfo.InvariantCulture, "{0}, ", item));
                Console.WriteLine();

                Console.WriteLine(pUse.GetGeometryByID(1));

                var r = pUse.GetFeature(1);
                foreach (var item in r.ItemArray)
                    Console.Write(string.Format(CultureInfo.InvariantCulture, "{0}, ", item));

                Console.WriteLine();
                Console.WriteLine();
            }
            Console.WriteLine();

            p.Dispose();
            

            m.ZoomToExtents();
            var b = m.GetMap();
            b.Save("fgdb-usa-states.png");
            
            //var fds = new FeatureDataSet();
            //lc.ExecuteIntersectionQuery(m.GetExtents().GetCentroid(), fds);
            //fds.Tables[0].TableName = lc.LayerName;
            //fds.Tables[0].WriteXml(Console.Out);
        }
    }
}
#endif
