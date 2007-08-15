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
using SharpMap.Geometries;

namespace UnitTests.Data.Providers
{
    [TestFixture]
    public class DataTablePointTests
    {
        [Test]
        public void CreateDataTablePoints()
        {
            DataTable source = createDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");
            Assert.IsNotNull(provider);
            Assert.AreSame(source, provider.Table);
            Assert.AreEqual("oid", provider.ObjectIdColumn);
            Assert.AreEqual("x", provider.XColumn);
            Assert.AreEqual("y", provider.YColumn);
        }

        [Test]
        public void OpenAndCloseUpdatesIsOpenPropertyCorrectly()
        {
            DataTable source = createDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");
            Assert.AreEqual(false, provider.IsOpen);
            provider.Open();
            Assert.AreEqual(true, provider.IsOpen);
            provider.Close();
            Assert.AreEqual(false, provider.IsOpen);
        }

        [Test]
        public void GetExtentsComputation()
        {
            DataTable source = createDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");

            double minX = Double.PositiveInfinity, minY = Double.PositiveInfinity, 
                maxX = Double.NegativeInfinity, maxY = Double.NegativeInfinity;

            foreach (DataRowView rowView in source.DefaultView)
            {
                if (minX > (double)rowView["x"]) minX = (double)rowView["x"];
                if (minY > (double)rowView["y"]) minY = (double)rowView["y"];
                if (maxX < (double)rowView["x"]) maxX = (double)rowView["x"];
                if (maxY < (double)rowView["y"]) maxY = (double)rowView["y"];
            }

            BoundingBox expectedBounds = new BoundingBox(minX, minY, maxX, maxY);
            BoundingBox actualBounds = provider.GetExtents();

            Assert.AreEqual(expectedBounds, actualBounds);
        }

        [Test]
        public void GetGeometryByIDReturnsExpectedGeometry()
        {
            DataTable source = createDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");

            DataRow row = source.Select("oid = 43")[0];
            Point expected = new Point((double)row["x"], (double)row["y"]);

            Geometry actual = provider.GetGeometryByID(43);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(typeof(Point), actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExecuteIntersectionQueryReturnsExpectedFeatures()
        {
            DataTable source = createDataTableSource();
            DataTablePoint provider = new DataTablePoint(source, "oid", "x", "y");

            BoundingBox query = new BoundingBox(400, 400, 600, 600);

            FeatureDataTable expected = new FeatureDataTable();
            expected.TableName = "PointSource";

            foreach (DataColumn column in source.Columns)
            {
                expected.Columns.Add(column.ColumnName, column.DataType);
            }

            foreach (DataRowView rowView in source.DefaultView)
            {
                if (query.Contains(new Point((double)rowView["x"], (double)rowView["y"])))
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
                DataRow[] actualRows = actual.Select("oid = " + expectedRowView["oid"].ToString());
                Assert.AreEqual(1, actualRows.Length);
                Assert.AreEqual(expectedRowView["oid"], actualRows[0]["oid"]);
                Assert.AreEqual(expectedRowView["x"], actualRows[0]["x"]);
                Assert.AreEqual(expectedRowView["y"], actualRows[0]["y"]);
            }
        }

        private static DataTable createDataTableSource()
        {
            DataTable source = new DataTable("PointSource", "http://www.codeplex.com/SharpMap/v1/UnitTests");
            source.Columns.AddRange(new DataColumn[] {
                new DataColumn("oid", typeof(uint)),
                new DataColumn("name", typeof(string)),
                new DataColumn("x", typeof(double)),
                new DataColumn("y", typeof(double))});

            Random rnd = new Random();
            for (int i = 0; i < 100; i++)
            {
                DataRow row = source.NewRow();
                row["oid"] = i;
                row["name"] = "Feature #" + i;
                row["x"] = rnd.NextDouble() * 1000;
                row["y"] = rnd.NextDouble() * 1000;
                source.Rows.Add(row);
            }

            return source;
        }
    }
}
