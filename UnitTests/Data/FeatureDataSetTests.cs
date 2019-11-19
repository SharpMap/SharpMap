using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using SharpMap.Data;

namespace UnitTests.Data
{
    public class FeatureDataSetTests : Serialization.BaseSerializationTest
    {
        [Test]
        public void TestCreateJoin()
        {
            var fds = new FeatureDataSet {DataSetName = "Join"};
            var ds = (DataSet) fds;

            var t1 = CreateFeatureDataTable("T1");
            fds.Tables.Add(t1);
            Assert.AreEqual(1, ((DataSet)fds).Tables.Count);

            var t2 = CreateDataTable("T2");
            ds.Tables.Add(t2);
            Assert.AreEqual(2, ds.Tables.Count);
            Assert.AreEqual(1, fds.Tables.Count);

            Assert.DoesNotThrow( () => fds.Relations.Add(t1.Columns[0], t2.Columns[0]));

        }

        private static FeatureDataTable CreateFeatureDataTable(string name)
        {
            var t1 = new FeatureDataTable { TableName = name };
            t1.Columns.Add(new DataColumn("oid", typeof(uint)));
            t1.Columns.Add(new DataColumn("P1", typeof(string)));
            t1.Columns.Add(new DataColumn("P2", typeof(double)));
            t1.Columns.Add(new DataColumn("P3", typeof(DateTime)));
            t1.PrimaryKey = new[] { t1.Columns[0] };
            return t1;
        }

        private static DataTable CreateDataTable(string name, bool pk = false)
        {
            var t2 = new DataTable(name);
            t2.Columns.Add("oid", typeof(uint));
            t2.Columns.Add("P1", typeof(string));
            if (pk) t2.PrimaryKey = new [] { t2.Columns[0] };
            return t2;
        }

        [Test]
        public void TestIndexing()
        {
            var fds = new FeatureDataSet { DataSetName = "Indexing" };
            var ds = (DataSet)fds;

            for (var i = 0; i < 10; i++)
                ds.Tables.Add(i%2 == 0 ? CreateFeatureDataTable("T" + i) : CreateDataTable("T" + i, true));

            Assert.AreEqual(10, ds.Tables.Count);
            Assert.AreEqual(5, fds.Tables.Count);
            for (var i = 0; i < 5; i++)
                Assert.IsTrue(ReferenceEquals(ds.Tables[2*i], fds.Tables[i]));
        }

        [Test]
        public void TestSerializationOfDataset()
        {
            var fds = new FeatureDataSet { DataSetName = "Serialization", Namespace = "ns" };
            using (var p = Serialization.ProviderTest.CreateProvider("managedspatialite"))
            {
                p.Open();
                p.ExecuteIntersectionQuery(p.GetExtents(), fds);
                p.Close();
            }

            // add second datatable for multi-layer queries
            var l2 = CreateFeatureDataTable("layer 2");
            l2.ExtendedProperties.Add("dummy", 5);

            fds.Tables.Add(l2);

            // add second datatable and relation
            var l3 = CreateFeatureDataTable("layer 3");

            fds.Tables.Add(l3);
            
            fds.Relations.Add(l2.Columns["oid"], l3.Columns["oid"]);
            
            FeatureDataSet deserializedFds = null;

            Assert.DoesNotThrow(() => deserializedFds = SandD(fds, GetFormatter()));

            Assert.That(deserializedFds.Namespace, Is.EqualTo(fds.Namespace));
            Assert.That(deserializedFds.DataSetName, Is.EqualTo(fds.DataSetName));
            Assert.That(deserializedFds.Locale, Is.EqualTo(fds.Locale));
            Assert.That(deserializedFds.EnforceConstraints, Is.EqualTo(fds.EnforceConstraints));
            Assert.That(deserializedFds.Prefix, Is.EqualTo(fds.Prefix));

            Assert.That(deserializedFds.Tables.Count, Is.EqualTo(fds.Tables.Count));
            var deserializedLayer2Table = deserializedFds.Tables.First(fdt => fdt.TableName == "layer 2");
            var deserializedLayer3Table = deserializedFds.Tables.First(fdt => fdt.TableName == "layer 3");

            Assert.That(deserializedLayer2Table.ExtendedProperties.ContainsKey("dummy"),
                "DataSet.ExtendedProperties not serialized");
            Assert.That(deserializedLayer2Table.ExtendedProperties.ContainsValue("5"));

            Assert.That(deserializedFds.Relations.Count, Is.EqualTo(1), "Relations not serialized");
            Assert.That(deserializedFds.Relations[0].ParentTable, Is.EqualTo(deserializedLayer2Table), "Wrong parent relation");
            Assert.That(deserializedFds.Relations[0].ChildTable, Is.EqualTo(deserializedLayer3Table), "Wrong child relation");

            Assert.That(deserializedLayer2Table.Constraints.Count, Is.EqualTo(1), "Constraints not serialized");
            var cons = deserializedLayer2Table.Constraints[0] as UniqueConstraint;
            Assert.NotNull(cons, "Wrong constraint");
            Assert.IsTrue(cons.IsPrimaryKey, "Constraint was a primary key");
        }

        [Test]
        public void TestSerialization()
        {
            var fds = new FeatureDataSet { DataSetName = "Serialization", Namespace = "ns"};
            using (var p = Serialization.ProviderTest.CreateProvider("managedspatialite"))
            {
                p.Open();
                p.ExecuteIntersectionQuery(p.GetExtents(), fds);
                p.Close();
            }

            FeatureDataTable deserializedFds = null;
            Assert.DoesNotThrow( () => deserializedFds = SandD(fds.Tables[0], GetFormatter()));

            //Assert.IsNotNull(deserializedFds);
            //Assert.AreEqual(fds.DataSetName, deserializedFds.DataSetName);
            Assert.AreEqual(fds.Namespace, deserializedFds.Namespace);
            Assert.AreEqual(fds.Locale, deserializedFds.Locale);
            Assert.AreEqual(fds.Prefix, deserializedFds.Prefix);
            // Maybe more to test?

            for (var i = 0; i < fds.Tables.Count; i++)
            {
                var t1 = fds.Tables[i];
                var t2 = deserializedFds;//.Tables[i];
                Assert.AreEqual(t1.Namespace, t2.Namespace);
                Assert.AreEqual(t1.TableName, t2.TableName);
                Assert.AreEqual(t1.Columns.Count, t2.Columns.Count);

                for (var j = 0; j < t1.Columns.Count; j++)
                {
                    Assert.AreEqual(t1.Columns[j].AllowDBNull, t2.Columns[j].AllowDBNull);
                    Assert.AreEqual(t1.Columns[j].AutoIncrement, t2.Columns[j].AutoIncrement);
                    Assert.AreEqual(t1.Columns[j].AutoIncrementSeed, t2.Columns[j].AutoIncrementSeed);
                    Assert.AreEqual(t1.Columns[j].AutoIncrementStep, t2.Columns[j].AutoIncrementStep);
                    Assert.AreEqual(t1.Columns[j].Caption, t2.Columns[j].Caption);
                    Assert.AreEqual(t1.Columns[j].ColumnMapping, t2.Columns[j].ColumnMapping);
                    Assert.AreEqual(t1.Columns[j].ColumnName, t2.Columns[j].ColumnName);
                    Assert.AreEqual(t1.Columns[j].DataType, t2.Columns[j].DataType);
                    Assert.AreEqual(t1.Columns[j].DateTimeMode, t2.Columns[j].DateTimeMode);
                    Assert.AreEqual(t1.Columns[j].DefaultValue, t2.Columns[j].DefaultValue);
                    Assert.AreEqual(t1.Columns[j].Expression, t2.Columns[j].Expression);
                    Assert.AreEqual(t1.Columns[j].MaxLength, t2.Columns[j].MaxLength);
                    Assert.AreEqual(t1.Columns[j].Namespace, t2.Columns[j].Namespace);
                    Assert.AreEqual(t1.Columns[j].Ordinal, t2.Columns[j].Ordinal);
                    Assert.AreEqual(t1.Columns[j].Prefix, t2.Columns[j].Prefix);
                    Assert.AreEqual(t1.Columns[j].ReadOnly, t2.Columns[j].ReadOnly);
                    Assert.AreEqual(t1.Columns[j].Unique, t2.Columns[j].Unique);
                }

                Assert.AreEqual(t1.Count, t2.Count);
                for (var j = 0; j < t1.Rows.Count; j++)
                {
                    var r1 = (FeatureDataRow)t1.Rows[j];
                    var r2 = (FeatureDataRow)t2.Rows[j];
                    Assert.AreEqual(r1.Geometry, r2.Geometry);

                    for (var k = 0; k < t1.Columns.Count; k++)
                    {
                        Assert.AreEqual(r1[k], r2[k]);
                    }
                }
            }
        }
    }
}
