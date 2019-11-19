using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace UnitTests.Data.Providers
{
    [NUnit.Framework.TestFixture]
    public class DbaseReaderTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NetTopologySuite.NtsGeometryServices();
        }

        private int GetNumberOfRecords(DbaseReader reader)
        {
            var numberOfRecordsField = typeof(DbaseReader).GetField("_numberOfRecords", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (int)numberOfRecordsField.GetValue(reader);
        }

        [NUnit.Framework.Test()]
        public void TestDbaseReader()
        {
            using (DbaseReader reader = new DbaseReader(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.dbf")))
            {
                reader.Open();

                int numberOfRecords = GetNumberOfRecords(reader);

                // read entire file
                for (uint rowid = 0; rowid < numberOfRecords; rowid++)
                {
                    var values = reader.GetValues(rowid);
                }
            }
        }

        [NUnit.Framework.Test()]
        public void TestDbaseBinaryTree()
        {
            using (DbaseReader reader = new DbaseReader(TestUtility.GetPathToTestFile("SPATIAL_F_SKARVMUFF.dbf")))
            {
                reader.Open();

                int numberOfRecords = GetNumberOfRecords(reader);

                // Create index on OBJECTNO column
                var indexOBJECTNO = reader.CreateDbfIndex<int>(0);

                // Test if index contains all records
                Assert.AreEqual(numberOfRecords, indexOBJECTNO.InOrder.Count());

                // Test searching for all records individually
                for (uint rowid = 0; rowid < numberOfRecords; rowid++)
                {
                    int value = (int)reader.GetValues(rowid)[1];
                    Assert.AreEqual(1, indexOBJECTNO.Find(value).Count());
                }
            }
        }
    }
}
