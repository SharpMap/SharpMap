using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    internal class xxSProvider<T> : BaseProvider where T : xxS
    {
        private readonly xKKProvider _provider;
        private int _numValid;
        private T[] _xxS;

        protected xxSProvider(xKKProvider p, T[] xxS, int numUsed)
        {
            _provider = p;
            _xxS = xxS;
            _numValid = numUsed;
        }

        public override Envelope GetExtents()
        {
            return _provider.GetExtents();
        }

        private FeatureDataTable _basetable;
        protected FeatureDataTable GetBasetable()
        {
            if (_basetable == null)
            {
                _basetable = GetBasetableInternal();
            }
            return _basetable;
        }

        protected virtual FeatureDataTable GetBasetableInternal()
        {
            var res = new FeatureDataTable();
            var fid = res.Columns.Add("FID", typeof (int));
            fid.AllowDBNull = false;
            var vk = res.Columns.Add("VK", typeof(int));
            vk.AllowDBNull = false;
            var nk = res.Columns.Add("NK", typeof(int));
            nk.AllowDBNull = false;
            var art = res.Columns.Add("ART", typeof(int));
            art.AllowDBNull = false;

            foreach (var fieldInfo in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.DeclaredOnly ))
            {
                var col = res.Columns.Add(fieldInfo.Name, fieldInfo.FieldType);
                col.AllowDBNull = false;
            }
            res.Constraints.Add("pk_FID", fid, true);
            return res;
        }

        public override FeatureDataRow GetFeature(uint rowId)
        {
            var f = GetBasetable().NewRow();
            f.Geometry = GetGeometryByID(rowId);
            f.ItemArray = GetItems(rowId);

            return f;
        }

        protected object[] GetItems(uint rowId)
        {
            var res = new List<object>();
            res.Add(rowId--);
            _xxS[rowId].AddItems(res);
            return res.ToArray();
        }

        public override IGeometry GetGeometryByID(uint oid)
        {
            var xKK1 = _provider[_xxS[--oid].VK];
            var xKK2 = _provider[_xxS[oid].NK];

            return Factory.CreateLineString(new [] {xKK1.Coordinate, xKK2.Coordinate});
        }

        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            var p = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            var fdt = (FeatureDataTable)GetBasetable().Copy();
            fdt.BeginLoadData();
            for (var i = 0; i < _xxS.Length; i++)
            {
                if (_xxS[i] != null && _xxS[i].Valid)
                {
                    var id = (uint) i + 1;
                    var tmpGeom =GetGeometryByID(id);
                    if (p.Intersects(tmpGeom))
                    {
                        var fdr = (FeatureDataRow)fdt.LoadDataRow(GetItems(id), true);
                        fdr.Geometry = tmpGeom;
                    }
                }
            }
            fdt.EndLoadData();
            ds.Tables.Add(fdt);
        }

        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var res = new Collection<uint>();
            for (uint i = 1; i <= _xxS.Length; i++)
            {
                if (bbox.Intersects(GetGeometryByID(i).EnvelopeInternal))
                    res.Add(i);
            }
            return res;
        }

        public override void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(Factory.ToGeometry(box), ds);
        }

        public override int GetFeatureCount()
        {
            return _numValid;
        }

        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var res = new Collection<IGeometry>();
            for (uint i = 1; i <= _xxS.Length; i++)
            {
                var geom = GetGeometryByID(i);
                if (bbox.Intersects(geom.EnvelopeInternal))
                    res.Add(geom);
            }
            return res;
        }

        internal T this[int index] { get { return _xxS[index]; } }

        public static xxSProvider<T> Load(string filename, Func<BinaryReader, T> readerFn, xKKProvider p)
        {
            using (var s = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Load(s, readerFn, p);
            }
        }

        public static xxSProvider<T> Load(Stream stream, Func<BinaryReader, T> readerFn, xKKProvider p)
        {
            var reader = new BinaryReader(stream);
            var version = Utilities.Utilities.Win1252.Value.GetString(reader.ReadBytes(6));
            var numLinks = reader.ReadInt32();
            var numValid = 0;
            var xxSs = new T[numLinks];

            for (var i = 0; i < numLinks; i++)
            {
                var xxSItem = readerFn(reader);
                if (xxSItem.Valid)
                {
                    numValid++;
                    xxSs[i] = xxSItem;
                }

            }
            return new xxSProvider<T>(p, xxSs, numValid);
        }
    }

    internal abstract class xxS
    {
        public int VK;
        public int NK;
        public short Art;

        internal bool Valid { get { return /*Art > 0 && */ VK > 0 && NK > 0; } }

        public virtual void Read(BinaryReader reader)
        {
            VK = reader.ReadInt32();
            NK = reader.ReadInt32();
            Art = reader.ReadInt16();
        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.Write(VK);
            writer.Write(NK);
            writer.Write(Art);
        }

        public virtual void AddItems(IList<object> list)
        {
            list.Add(VK);
            list.Add(NK);
            list.Add(Art);
        }
    }

    internal class ZGS : xxS
    {
        public short ZNrL;
        public short ZNrR;
        
        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            ZNrL = reader.ReadInt16();
            ZNrR = reader.ReadInt16();
            reader.ReadBytes(6);
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(ZNrL);
            writer.Write(ZNrR);
            writer.Write(new byte[6]);
        }

        public override void AddItems(IList<object> list)
        {
            base.AddItems(list);
            list.Add(ZNrL);
            list.Add(ZNrR);
        }

        internal static ZGS Create(BinaryReader reader)
        {
            var res = new ZGS();
            res.Read(reader);
            return res;
        }
    }
}