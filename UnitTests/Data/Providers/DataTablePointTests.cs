// Copyright 2007 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Data;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace UnitTests.Data.Providers
{
    public class DataTablePointTests : ProviderTest
    {
        internal static DataTable CreateDataTableSource()
        {
            var source = new DataTable("PointSource", "http://www.codeplex.com/SharpMap/v1/UnitTests");
            DataColumn pk;
            source.Columns.AddRange(new[]
                                        {
                                            pk = new DataColumn("oid", typeof (uint)),
                                            new DataColumn("name", typeof (string)),
                                            new DataColumn("x", typeof (double)),
                                            new DataColumn("y", typeof (double))
                                        });

            var rnd = new Random(17);
            for (int i = 0; i < 100; i++)
            {
                DataRow row = source.NewRow();
                row["oid"] = i;
                row["name"] = "Feature #" + i;
                row["x"] = rnd.NextDouble()*1000;
                row["y"] = rnd.NextDouble()*1000;
                source.Rows.Add(row);
            }
            source.PrimaryKey = new[] {pk};

            return source;
        }

        [Test]
        public void CreateDataTablePoints()
        {
            DataTable source = CreateDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");
            Assert.IsNotNull(provider);
            Assert.AreSame(source, provider.Table);
            Assert.AreEqual("oid", provider.ObjectIdColumn);
            Assert.AreEqual("x", provider.XColumn);
            Assert.AreEqual("y", provider.YColumn);
        }

        [Test]
        public void ExecuteIntersectionQueryReturnsExpectedFeatures()
        {
            DataTable source = CreateDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");

            var query = new Envelope(400, 600, 400, 600);

            FeatureDataTable expected = new FeatureDataTable();
            expected.TableName = "PointSource";

            foreach (DataColumn column in source.Columns)
            {
                expected.Columns.Add(column.ColumnName, column.DataType);
            }

            foreach (DataRowView rowView in source.DefaultView)
            {
                if (query.Contains(new Coordinate((double) rowView["x"], (double) rowView["y"])))
                {
                    expected.ImportRow(rowView.Row);
                }
            }

            FeatureDataSet dataSet = new FeatureDataSet();
            provider.ExecuteIntersectionQuery(query, dataSet);
            Assert.IsNotNull(dataSet);
            Assert.IsNotNull(dataSet.Tables);
            Assert.AreEqual(1, dataSet.Tables.Count);

            FeatureDataTable actual = dataSet.Tables[0];

            Assert.AreEqual(expected.Rows.Count, actual.Rows.Count);

            foreach (DataRowView expectedRowView in expected.DefaultView)
            {
                DataRow[] actualRows = actual.Select("oid = " + expectedRowView["oid"]);
                Assert.AreEqual(1, actualRows.Length);
                Assert.AreEqual(expectedRowView["oid"], actualRows[0]["oid"]);
                Assert.AreEqual(expectedRowView["x"], actualRows[0]["x"]);
                Assert.AreEqual(expectedRowView["y"], actualRows[0]["y"]);
            }
        }

        [Test]
        public void GetExtentsComputation()
        {
            DataTable source = CreateDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");

            double minX = Double.PositiveInfinity,
                   minY = Double.PositiveInfinity,
                   maxX = Double.NegativeInfinity,
                   maxY = Double.NegativeInfinity;

            foreach (DataRowView rowView in source.DefaultView)
            {
                if (minX > (double) rowView["x"]) minX = (double) rowView["x"];
                if (minY > (double) rowView["y"]) minY = (double) rowView["y"];
                if (maxX < (double) rowView["x"]) maxX = (double) rowView["x"];
                if (maxY < (double) rowView["y"]) maxY = (double) rowView["y"];
            }

            Envelope expectedBounds = new Envelope(minX, maxX, minY, maxY);
            Envelope actualBounds = provider.GetExtents();

            Assert.AreEqual(expectedBounds, actualBounds);
        }

        [Test]
        public void GetGeometryByIDReturnsExpectedGeometry()
        {
            DataTable source = CreateDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");

            DataRow row = source.Select("oid = 43")[0];
            Point expected = new Point((double) row["x"], (double) row["y"]);

            IGeometry actual = provider.GetGeometryByID(43);

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual is Point);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFeatureReturnsExpectedFeature()
        {
            DataTable source = CreateDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");

            DataRow row = source.Select("oid = 43")[0];
            Point expected = new Point((double)row["x"], (double)row["y"]);

            var actual = provider.GetFeature(43);

            Assert.That(actual, Is.Not.Null, "actual != null");
            Assert.That(actual.ItemArray.Length, Is.EqualTo(4), "actual.ItemArray.Length == 4");
            Assert.That(actual[0], Is.EqualTo(row[0]));
            Assert.That(actual[1], Is.EqualTo(row[1]));
            Assert.That(actual[2], Is.EqualTo(row[2]));
            Assert.That(actual[3], Is.EqualTo(row[3]));
            Assert.That(actual.Geometry, Is.EqualTo(expected));

        }

        [Test]
        public void OpenAndCloseUpdatesIsOpenPropertyCorrectly()
        {
            DataTable source = CreateDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");
            Assert.AreEqual(false, provider.IsOpen);
            provider.Open();
            Assert.AreEqual(true, provider.IsOpen);
            provider.Close();
            Assert.AreEqual(false, provider.IsOpen);
        }
    }
}
