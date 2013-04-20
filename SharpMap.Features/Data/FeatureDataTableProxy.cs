using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Data
{
    public class FeatureDataTableProxy : IFeatureFactory, IFeatureCollection
    {
        #region NestedClasses
        
        private class DataColumnProxy : IFeatureAttributeDefinition
        {
            private readonly DataColumn _column;

            public DataColumnProxy(DataColumn column)
            {
                _column = column;
            }

            public string AttributeName
            {
                get { return _column.ColumnName; }
                set { _column.ColumnName = value; }
            }

            public string AttributeDescription { get { return _column.Caption; } set { _column.Caption = value; } }

            public Type AttributeType
            {
                get { return _column.DataType; }
                set
                {
                    throw new NotSupportedException();
                } 
            }

            public bool IsNullable { get { return _column.AllowDBNull; }
                set { _column.AllowDBNull = value; }
            }
        }

        private class FdrEnumerator : IEnumerator<IFeature<uint>>
        {
            private int _index = -1;
            private readonly DataRow[] _rows;

            public FdrEnumerator(DataRow[] rows)
            {
                _rows = rows;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                _index++;
                return Current != null;
            }

            public void Reset()
            {
                _index = -1;
            }

            public IFeature<uint> Current
            {
                get
                {
                    if (_index < 0) return null;
                    if (_index >= _rows.Length) return null;
                    return new FeatureDataRowProxy((FeatureDataRow)_rows[_index]);
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }


        #endregion

        private readonly FeatureDataTable _table;

        public FeatureDataTableProxy(IGeometryFactory factory, FeatureDataTable table)
        {
            GeometryFactory = factory;
            _table = table;
        }

        public IGeometryFactory GeometryFactory { get; private set; }

        public IList<IFeatureAttributeDefinition> AttributesDefinition
        {
            get
            {
                var res = new List<IFeatureAttributeDefinition>(_table.Columns.Count);
                foreach (DataColumn c in _table.Columns)
                {
                    res.Add(new DataColumnProxy(c));
                }
                return res;
            }
        }

        public IFeature Create()
        {
            var row = _table.NewRow();
            return new FeatureDataRowProxy(row);
        }

        public IFeature Create(IGeometry geometry)
        {
            var res = Create();
            res.Geometry = geometry;
            return res;
        }

        public IEnumerator<IFeature> GetEnumerator()
        {
            return new FdrEnumerator(_table.Select()); 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IFeature item)
        {
            if (Contains(item))
                throw new ArgumentException("This item is already in the table", "item");

            var fdrp = item as FeatureDataRowProxy;
            if (fdrp == null)
            {
                throw new NotImplementedException();
            }

            if (fdrp.DataRow.Table != _table)
            {
                throw new ArgumentException("The item to add was not created by this factory", "item");
            }
            _table.AddRow((FeatureDataRow)fdrp.DataRow);
        }

        public void Clear()
        {
            _table.Clear();
        }

        public bool Contains(IFeature item)
        {
            return _table.Rows.Find(item.Oid) != null;
        }

        public void CopyTo(IFeature[] array, int arrayIndex)
        {
            var length = Math.Min(_table.Rows.Count, array.Length - arrayIndex);
            var rows = _table.Rows;
            for (var i = 0; i < length; i++)
            {
                array[arrayIndex+i] = new FeatureDataRowProxy((FeatureDataRow)rows[i]);
            }
        }

        public bool Remove(IFeature item)
        {
            var row = item as FeatureDataRowProxy;
            if (row == null)
                throw new ArgumentException();

            _table.Rows.Remove(row.DataRow);
            return true;
        }

        public int Count { get { return _table.Rows.Count; } }

        public bool IsReadOnly { get { return false; } }
        
        public IFeatureFactory Factory { get { return this; } }
        
        public IFeature New()
        {
            return Create();
        }
    }
}