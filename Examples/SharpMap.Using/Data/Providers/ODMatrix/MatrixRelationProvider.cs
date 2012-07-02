using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.ODMatrix
{
    public class MatrixRelationProvider : MatrixProviderBase
    {
        public MatrixRelationProvider(IODMatrix matrix)
            : base(matrix)
        {
        }

        public override string ProviderName
        {
            get { return "Relation"; }
        }

        public ushort? RestrictId { get; set; }

        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var res = new Collection<IGeometry>();

            foreach (var relation in Matrix.Relations(RestrictId))
            {
                var origin = relation.Key;
                var destin = relation.Value;

                var box = origin.Value.EnvelopeInternal;
                box.ExpandToInclude(destin.Value.EnvelopeInternal);

                if (!bbox.Intersects(box))
                    continue;

                var val = Matrix[origin.Key, destin.Key];
                if (!Valid(val)) continue;

                val = Scale(val);
                if (origin.Key == destin.Key)
                {
                    res.Add(CreateCircle(origin.Value, val));
                }
                else
                {
                    res.Add(CreateLoad(origin.Value, destin.Value, val));
                    val = Matrix[destin.Key, origin.Key];
                    if (Valid(val))
                    {
                        val = Scale(val);
                        res.Add(CreateLoad(destin.Value, origin.Value, val));
                    }
                }
            }
            return res;
        }

        private static IGeometry CreateLoad(IPoint origin, IPoint destination, double d)
        {
            //get difference
            var dx = destination.X - origin.X;
            var dy = destination.Y - origin.Y;

            //normalize
            var length = Math.Sqrt(dx * dx + dy * dy);
            dx /= length;
            dy /= length;

            //scale and flip since we want perpendicular
            var offX = dy * d;
            var offY = -dx * d;

            //compute points
            var origin2 = new Coordinate(origin.X + offX, origin.Y + offY);
            var destination2 = new Coordinate(destination.X + offX, destination.Y + offY);
            var f = origin.Factory;
            var shell = f.CreateLinearRing(f.CoordinateSequenceFactory.Create(new[] { origin.Coordinate, destination.Coordinate, destination2, origin2, origin.Coordinate }));

            return f.CreatePolygon(shell, null);
        }

        private static uint CreateOid(ushort origin, ushort destin)
        {
            var iorig = (uint)origin;
            var idest = (uint)destin;

            return (iorig << 16) | idest;
        }

        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var res = new Collection<uint>();

            foreach (var relation in Matrix.Relations(RestrictId))
            {
                var origin = relation.Key;
                var destin = relation.Value;

                var box = origin.Value.EnvelopeInternal;
                    box.ExpandToInclude(destin.Value.EnvelopeInternal);
                if (!bbox.Intersects(box))
                    continue;

                var val = Matrix[origin.Key, destin.Key];
                if (!Valid(val)) continue;

                if (origin.Key == destin.Key)
                    res.Add(CreateOid(origin.Key, origin.Key));
                else
                {
                    res.Add(CreateOid(origin.Key, destin.Key));
                    val = Matrix[destin.Key, origin.Key];
                    if (Valid(val))
                        res.Add(CreateOid(destin.Key, origin.Key));
                }
            }
            return res;
        }

        public override IGeometry GetGeometryByID(uint oid)
        {
            var row = (ushort)(oid >> 16);
            var col = (ushort)(0xffff & oid);

            var val = Scale(Matrix[row, col]);
            if (row == col)
                return CreateCircle(Matrix[row], val);
            return CreateLoad(Matrix[row], Matrix[col], val);
        }

        public override void ExecuteIntersectionQuery(Envelope bbox, FeatureDataSet ds)
        {
            var fdt = CreateTable();
            fdt.BeginLoadData();

            foreach (var relation in Matrix.Relations(RestrictId))
            {
                var origin = relation.Key;
                var destin = relation.Value;

                var box = origin.Value.EnvelopeInternal;
                box.ExpandToInclude(destin.Value.EnvelopeInternal);
                if (!bbox.Intersects(box))
                    continue;

                var val = Matrix[origin.Key, destin.Key];
                if (!Valid(val)) continue;

                var fdr = (FeatureDataRow)fdt.LoadDataRow(new object[] { CreateOid(origin.Key, destin.Key), val }, true);

                var sval = Scale(val);
                if (origin.Key == destin.Key)
                {
                    fdr.Geometry = CreateCircle(origin.Value, sval);
                }
                else
                {
                    fdr.Geometry = CreateLoad(origin.Value, destin.Value, sval);
                    val = Matrix[destin.Key, origin.Key];
                    if (Valid(val))
                    {
                        sval = Scale(val);
                        fdr = (FeatureDataRow)fdt.LoadDataRow(new object[] { CreateOid(destin.Key, origin.Key), val }, true);
                        fdr.Geometry = CreateLoad(destin.Value, origin.Value, sval);
                    }
                }
            }

            fdt.EndLoadData();
            ds.Tables.Add(fdt);
        }

        public override int GetFeatureCount()
        {
            return RestrictId.HasValue ? 2 * Matrix.Size - 1 : Matrix.Size * Matrix.Size;
        }

        public override FeatureDataRow GetFeature(uint oid)
        {
            var row = (ushort)(oid >> 16);
            var col = (ushort)(0xffff & oid);

            var fdt = CreateTable();
            fdt.BeginLoadData();

            var val = Matrix[row, col];
            var sval = Scale(val);

            var fdr = (FeatureDataRow)fdt.LoadDataRow(new object[] { oid, val }, true);
            fdr.Geometry = row == col
                               ? CreateCircle(Matrix[row], sval)
                               : CreateLoad(Matrix[row], Matrix[col], sval);

            fdt.EndLoadData();

            return fdr;
        }
    }
}