using System;
using System.Data;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Data
{
    public class FeatureDataRowProxy : IFeature<uint>, IFeatureAttributes
    {
        private readonly FeatureDataRow _row;

        public FeatureDataRowProxy(FeatureDataRow row)
        {
            _row = row;
        }

        public uint Oid
        {
            get { return Convert.ToUInt32(_row[0]); }
            set { _row[0] = value; }
        }

        object IEntity.Oid { get { return Oid; } set { Oid = (uint)value; } }

        public Type GetEntityType()
        {
            return typeof(FeatureDataRow);
        }

        public bool HasOidAssigned
        {
            get
            {
                return _row[0] != DBNull.Value;
                return _row.RowState == DataRowState.Detached;
            }
        }

        public object Clone()
        {
            var res = (FeatureDataRowProxy)Factory.Create((IGeometry) Geometry.Clone());
            var values = _row.ItemArray;
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i] is ICloneable)
                {
                    values[i] = ((ICloneable) values[i]).Clone();
                }
            }
            res._row.ItemArray = values;
            return res;
        }

        public void Dispose()
        {
        }

        public IFeatureFactory Factory { get { return new FeatureDataTableProxy(_row.Geometry.Factory, (FeatureDataTable)_row.Table); } }

        public IGeometry Geometry
        {
            get { return _row.Geometry; }
            set { _row.Geometry = value; }
        }

        public IFeatureAttributes Attributes { get { return this; } }

        internal DataRow DataRow
        {
            get { return _row; }
        }

        object IFeatureAttributes.this[int index]
        {
            get { return _row[index]; }
            set { _row[index] = value; }
        }

        object IFeatureAttributes.this[string key]
        {
            get { return _row[key]; }
            set { _row[key] = value; }
        }

        public object[] GetValues()
        {
            return _row.ItemArray;
        }
    }
}