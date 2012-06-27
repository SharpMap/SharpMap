using System.Collections.ObjectModel;
using System.Data;
using GeoAPI.Geometries;
using Geometry = GeoAPI.Geometries.IGeometry;
using BoundingBox = GeoAPI.Geometries.Envelope;

namespace SharpMap.Data.Providers.ODMatrix
{
    public class MatrixODSumProvider : MatrixProviderBase
    {
        public MatrixODSumProvider(IODMatrix matrix, ODMatrixVector vector)
            : base(matrix)
        {
            MatrixVector = vector;
        }

        public override string ProviderName
        {
            get { return "OdSum"; }
        }

        public ODMatrixVector MatrixVector { get; private set; }

        public override Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            var res = new Collection<Geometry>();
            foreach (var origin in Matrix)
            {
                if (!bbox.Intersects(origin.Value.EnvelopeInternal))
                    continue;

                var val = Matrix[origin.Key, MatrixVector];
                if (!Valid(val))
                    continue;
                var sval = Scale(val);
                res.Add(CreateCircle(origin.Value, sval));
            }
            return res;
        }

        public override Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            var res = new Collection<uint>();
            foreach (var kvp in Matrix)
            {
                if (bbox.Intersects(kvp.Value.EnvelopeInternal))
                    res.Add(kvp.Key);
            }
            return res;
        }

        public override Geometry GetGeometryByID(uint oid)
        {
            return Matrix[(ushort)oid];
        }

        public override void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            var table = CreateTable();
            table.BeginLoadData();

            foreach (var kvp in Matrix)
            {
                var id = kvp.Key;
                if (box.Intersects(kvp.Value.EnvelopeInternal))
                {
                    var val = Matrix[kvp.Key, MatrixVector];
                    if (!Valid(val))
                        continue;

                    var row =
                        (FeatureDataRow)
                        table.LoadDataRow(new object[] { id, val }, LoadOption.Upsert);

                    var sval = Scale(val);
                    row.Geometry = CreateCircle(kvp.Value, sval);
                }
            }
            table.EndLoadData();

            ds.Tables.Add(table);
        }

        public override int GetFeatureCount()
        {
            return Matrix.Size;
        }

        public override FeatureDataRow GetFeature(uint rowId)
        {
            var id = (ushort)rowId;

            var table = CreateTable();
            table.BeginLoadData();
            table.LoadDataRow(new object[] { rowId, Matrix[id, MatrixVector] }, LoadOption.Upsert);
            table.EndLoadData();

            var res = (FeatureDataRow)table.Rows[0];
            res.Geometry = Matrix[id];

            return res;
        }
    }
}