using NetTopologySuite.Geometries;
using SharpSbn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using SbnEnvelope = SharpSbn.DataStructures.Envelope;

namespace SharpMap.Utilities.Indexing
{
    /// <summary>
    /// A spatial index factory base on ESRI's spatial index for Shapefiles
    /// </summary>
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

        private static ICollection<Tuple<uint, SbnEnvelope>> ToCollection(IEnumerable<ISpatialIndexItem<uint>> entries)
        {
            var res = new List<Tuple<uint, SbnEnvelope>>();
            foreach (var sii in entries)
            {
                var box = new SbnEnvelope(sii.Box.MinX, sii.Box.MaxX, sii.Box.MinY, sii.Box.MaxY);
                res.Add(Tuple.Create(sii.ID, box));
            }
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
                var box = new SbnEnvelope(e.MinX, e.MaxX, e.MinY, e.MaxY);
                foreach (var queryFid in _sbnTree.QueryFids(box))
                {
                    var oid = queryFid - 1;
                    if (oid < _sbnTree.FeatureCount) list.Add(oid);
                }
                return new Collection<uint>(list);
            }

            public Envelope Box
            {
                get
                {
                    var box = new Envelope(_sbnTree.Extent.MinX, _sbnTree.Extent.MaxX, _sbnTree.Extent.MinY, _sbnTree.Extent.MaxY);
                    return box;
                }
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
