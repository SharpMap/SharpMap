using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using GeoAPI.Geometries;
using SharpSbn;
using SbnEnvelope = GeoAPI.Geometries.Envelope;

namespace SharpMap.Utilities.Indexing
{
    public class SharpSbnIndexFactory : ISpatialIndexFactory<uint>
    {
        ISpatialIndexItem<uint> ISpatialIndexFactory<uint>.Create(uint oid, Envelope box)
        {
            return new SbnSpatialIndexItem(oid + 1, box);
        }

        ISpatialIndex<uint> ISpatialIndexFactory<uint>.Create(Envelope extent, int expectedNumberOfEntries, IEnumerable<ISpatialIndexItem<uint>> entries)
        {
            return new SbnTreeWrapper(SbnTree.Create(ToCollection(entries)));
        }

        private static ICollection<Tuple<uint, Envelope>> ToCollection(IEnumerable<ISpatialIndexItem<uint>> entries)
        {
            var res = new List<Tuple<uint, Envelope>>();
            foreach (var sii in entries)
                res.Add(Tuple.Create(sii.ID, sii.Box));
            return res;
        }

        ISpatialIndex<uint> ISpatialIndexFactory<uint>.Load(string fileName)
        {
            try
            {
                var tree = SbnTree.Load(Path.ChangeExtension(fileName, "sbn"));
                return new SbnTreeWrapper(tree);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        string ISpatialIndexFactory<uint>.Extension
        {
            get { return ".sbn"; }
        }

        #region Nested classes

        private class SbnTreeWrapper : ISpatialIndex<uint>
        {
            private readonly SbnTree _sbnTree;

            public SbnTreeWrapper(SbnTree sbnTree)
            {
                _sbnTree = sbnTree;
            }

            public Collection<uint> Search(Envelope e)
            {
                var list = new List<uint>();
                foreach (var queryFid in _sbnTree.QueryFids(e))
                {
                    var oid = queryFid - 1;
                    if (oid < _sbnTree.FeatureCount) list.Add(oid);
                }
                return new Collection<uint>(list);
            }

            public Envelope Box
            {
                get { return _sbnTree.Extent; }
            }

            void ISpatialIndex<uint>.SaveIndex(string filename)
            {
                _sbnTree.Save(Path.ChangeExtension(filename, "sbn"));
                //throw new NotImplementedException();
            }

            void ISpatialIndex<uint>.DeleteIndex(string filename)
            {
                File.Delete(Path.ChangeExtension(filename, "sbn"));
                File.Delete(Path.ChangeExtension(filename, "sbx"));
            }
        }

        private class SbnSpatialIndexItem : Tuple<uint, Envelope>, ISpatialIndexItem<uint>
        {
            public SbnSpatialIndexItem(uint item1, Envelope item2)
                : base(item1, item2)
            {
            }

            uint ISpatialIndexItem<uint>.ID
            {
                get { return Item1; }
            }

            Envelope ISpatialIndexItem<uint>.Box
            {
                get { return Item2; }
            }
        }
#endregion
    }
}
