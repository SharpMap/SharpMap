using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
        internal struct xKK
        {
            public int ID;
            public float X;
            public float Y;
            public short Ken;

            public Coordinate Coordinate
            {
                get { return new Coordinate(X, Y); }
            }

            public bool Valid
            {
                get
                {
                    if (Ken > 0)
                        return X != 0f && Y != 0f;
                    return false;
                }
            }

            /// <summary>
            /// Method to read a xKK item using the provided reader
            /// </summary>
            /// <param name="reader"></param>
            /// <returns></returns>
            public static xKK Read(BinaryReader reader)
            {
                var res = new xKK
                {
                    Ken = reader.ReadInt16(),
                    ID = reader.ReadInt32(),
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle()
                };

                return res;
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(Ken);
                writer.Write(ID);
                writer.Write(X);
                writer.Write(Y);
            }
        }

    /// <summary>
    /// 
    /// </summary>
    public class xKKProvider : BaseProvider
    {
        private xKK[] _points;
        private int _numUsed;
        private Envelope _extent;

        private xKKProvider(xKK[] points, int? numValid = null, Envelope extent= null)
        {
            _points = points;
            _extent = extent;
            if (numValid.HasValue)
                _numUsed = numValid.Value;
            else 
                EvaluatePoints(points);
        }

        private void EvaluatePoints(xKK[] points)
        {
            _numUsed = 0;
            _extent = new Envelope();
            for (var i = 0; i < points.Length; i++)
            {
                if (points[i].Valid)
                {
                    _numUsed++;
                    _extent.ExpandToInclude(points[i].Coordinate);
                }
            }
        }

        public static xKKProvider Load(string filename)
        {
            Debug.Assert(!string.IsNullOrEmpty(filename));
            Debug.Assert(File.Exists(filename));

            using (var s = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var res = Load(s);
                res.ConnectionID = filename;
                return res;
            }
        }

        public static xKKProvider Load(Stream stream)
        {
            Debug.Assert(stream != null);
            Debug.Assert(stream.CanRead);
            Debug.Assert(stream.Position == 0);

            var reader = new BinaryReader(stream);
            var version = Utilities.Utilities.ReadVersion(reader);
            if (version != " 2.2o ")
                throw new Exception("Invalid version");
            var numPoints = reader.ReadInt32();
            
            var points = new xKK[numPoints];
            var numValid = 0;
            var extent = new Envelope();
            for (var i = 0; i < numPoints; i ++)
            {
                var item = xKK.Read(reader);
                if (item.Valid)
                {
                    points[i] = item;
                    numValid++;
                    extent.ExpandToInclude(item.Coordinate);
                }
            }

            return new xKKProvider(points, numValid, extent);
        }

        private FeatureDataTable _baseTable;
        protected FeatureDataTable GetBaseTable()
        {
            if (_baseTable != null)
            {
                _baseTable = new FeatureDataTable();
                var fid = _baseTable.Columns.Add("FID", typeof (int));
                fid.AllowDBNull = false;
                var ken = _baseTable.Columns.Add("KEN", typeof (short));
                ken.AllowDBNull = false;
                _baseTable.Constraints.Add("pk_fid", fid, true);
            } 
            return _baseTable;

        }

        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var res = new Collection<IGeometry>();
            for (var i = 0; i < _points.Length; i++)
            {
                if (!_points[i].Valid) continue;
                
                var location = _points[i].Coordinate;
                if (bbox.Intersects(location))
                    res.Add(Factory.CreatePoint(location));
            }
            return res;
        }

        public override void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(Factory.ToGeometry(box), ds);
        }

        public override int GetFeatureCount()
        {
            return _numUsed;
        }

        public override Envelope GetExtents()
        {
            return _extent;
        }

        public override FeatureDataRow GetFeature(uint rowId)
        {
            if (rowId >= _points.Length)
                throw new ArgumentOutOfRangeException("rowId");

            if (!_points[rowId].Valid)
                throw new ArgumentException("rowId");

            var tbl = GetBaseTable();
            var res = tbl.NewRow();
            res[0] = (int) rowId;
            res[1] = _points[rowId].Ken;
            res.Geometry = Factory.CreatePoint(_points[rowId].Coordinate);

            return res;
        }

        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var res = new Collection<uint>();
            foreach (var point in _points)
            {
                if (point.Valid &&
                    _extent.Intersects(point.Coordinate))
                    res.Add((uint)point.ID);
            }
            return res;
        }

        public override IGeometry GetGeometryByID(uint rowId)
        {
            if (rowId >= _points.Length)
                throw new ArgumentOutOfRangeException("rowId");

            if (!_points[rowId].Valid)
                throw new ArgumentException("rowId");

            return Factory.CreatePoint(_points[rowId].Coordinate);
        }

        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            var pgeom = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            var res = (FeatureDataTable)GetBaseTable().Copy();
            res.BeginLoadData();

            foreach (var point in _points)
            {
                if (point.Valid)
                {
                    var tmpGeom = Factory.CreatePoint(point.Coordinate);
                    if (pgeom.Intersects(tmpGeom))
                    {
                        var fdr = (FeatureDataRow)
                            res.LoadDataRow(new[] {(object) point.ID, point.Ken}, 
                                            LoadOption.OverwriteChanges);
                        fdr.Geometry = tmpGeom;
                    }
                }
            }

            res.EndLoadData();
            ds.Tables.Add(res);
        }

        /// <summary>
        /// Method to get the <see cref="xKK"/> of a geometry
        /// </summary>
        /// <param name="index">The (1-based) index</param>
        /// <returns>The xKK object</returns>
        internal xKK this[int index] { get { return _points[index - 1]; } }
    }
}