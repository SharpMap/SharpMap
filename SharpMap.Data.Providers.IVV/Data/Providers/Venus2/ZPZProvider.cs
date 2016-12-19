using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Configuration;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    public class ZPZProvider : BaseProvider
    {
        private xKKProvider _provider;
        private ZPZ[] _zpz;

        internal ZPZProvider(ZPZ[] zpz, xKKProvider provider)
        {
            _zpz = zpz;
            _provider = provider;
        }

        public static ZPZProvider Load(string filename, xKKProvider provider)
        {
            
            using (var s = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Load(s, provider);
            }
        }

        public static ZPZProvider Load(Stream stream, xKKProvider provider)
        {
            var reader = new BinaryReader(stream);
            var version = Utilities.Utilities.Win1252.Value.GetString(reader.ReadBytes(6));
            var numRegions = reader.ReadInt16();
            var zpzs = new ZPZ[numRegions];

            for (var i = 0; i < numRegions; i++)
            {
                zpzs[i] = new ZPZ();
                zpzs[i].Zellnr = i + 1;
                zpzs[i].Read(reader);
            }

            return new ZPZProvider(zpzs, provider);
        }

        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var res = new Collection<IGeometry>();
            for (var i = 0; i < _zpz.Length; i++)
            {
                var box = _zpz[i].Envelope;
                if (box == null || bbox.Intersects(box))
                {
                    var tmp = GetGeometryByID((uint) i + 1);
                    if (tmp != null) res.Add(tmp);
                }
            }
            return res;
        }

        public override void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(Factory.ToGeometry(box), ds);
        }

        public override int GetFeatureCount()
        {
            return _zpz.Length;
        }

        public override Envelope GetExtents()
        {
            return _provider.GetExtents();
        }

        public override FeatureDataRow GetFeature(uint rowId)
        {
            return null;
        }

        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var res = new Collection<uint>();
            for (var i = 0; i < _zpz.Length; i++)
            {
                var box = _zpz[i].Envelope;
                if (box == null || bbox.Intersects(box))
                {
                    var tmp = GetGeometryByID((uint)i + 1);
                    if (tmp != null) res.Add((uint)i + 1);
                }
            }
            return res;
        }

        public override IGeometry GetGeometryByID(uint oid)
        {
            var zpz = _zpz[oid - 1];
            Debug.Assert(zpz.Zellnr == (int)oid);

            if (zpz.PolygonPunkte == null) return null;

            var seq = Factory.CoordinateSequenceFactory.Create(zpz.PolygonPunkte.Length, Ordinates.XY);
            for (var i = 0; i < zpz.PolygonPunkte.Length; i++)
            {
                var coord = _provider[zpz.PolygonPunkte[i]].Coordinate;
                seq.SetOrdinate(i, Ordinate.X, coord.X);
                seq.SetOrdinate(i, Ordinate.Y, coord.Y);
            }
            Debug.Assert(NetTopologySuite.Geometries.CoordinateSequences.IsRing(seq), "Ring is not closed!");
            var ring = Factory.CreateLinearRing(seq);
            var res = Factory.CreatePolygon(ring);
            if (zpz.Envelope == null) zpz.Envelope = res.EnvelopeInternal;
            return res;
        }

        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            //throw new System.NotImplementedException();
        }
    }

    internal class ZPZ
    {
        public int Zellnr;
        public int[] PolygonPunkte;
        public Envelope Envelope { get; set; }

        public void Read(BinaryReader reader)
        {
            int numPoints = reader.ReadInt32();
            if (numPoints > 0)
            {
                PolygonPunkte = new int[numPoints];
                for (var i = 0; i < numPoints; i++)
                {
                    var pktId = reader.ReadInt32();
                    /*
                    var pktIdBytes = BitConverter.GetBytes(pktId);
                    Array.Reverse(pktIdBytes);
                    pktId = BitConverter.ToInt32(pktIdBytes, 0);
                     */
                    PolygonPunkte[i] = pktId;
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            if (PolygonPunkte == null)
                writer.Write((int) 0);
            else
            {
                writer.Write(PolygonPunkte.Length);
                for (var i = 0; i < PolygonPunkte.Length; i++)
                    writer.Write(PolygonPunkte[i]);
            }
        }
    }
}