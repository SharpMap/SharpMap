using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace SharpMap.Data
{
    /// <summary>
    /// Represents the collection of tables for the FeatureDataSet.
    /// </summary>
    public class FeatureTableCollection : ICollection<FeatureDataTable>
    {
        private readonly DataTableCollection _dataTables;

        internal FeatureTableCollection(DataTableCollection dataTables)
        {
            _dataTables = dataTables;
        }

        public IEnumerator<FeatureDataTable> GetEnumerator()
        {
            var it = _dataTables.GetEnumerator();
            foreach (var dataTable in _dataTables)
            {
                if (dataTable is FeatureDataTable)
                    yield return (FeatureDataTable) dataTable;
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(FeatureDataTable item)
        {
            _dataTables.Add(item);
        }

        public void Clear()
        {
            _dataTables.Clear();
        }

        public bool Contains(FeatureDataTable item)
        {
            return _dataTables.Contains(item.TableName, item.Namespace);
        }

        public void CopyTo(FeatureDataTable[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentException("Negative arrayIndex");

            var j = 0;
            for (var i = 0; i < _dataTables.Count; i++)
            {
                if (_dataTables[i] is FeatureDataTable)
                {
                    if (j >= array.Length)
                        throw new ArgumentException("Insufficient space provided for array");
                    array[j++] = (FeatureDataTable) _dataTables[i];
                }
            }
        }

        public bool Remove(FeatureDataTable item)
        {
            if (_dataTables.CanRemove(item))
            {
                _dataTables.Remove(item);
                return true;
            }
            return false;
        }

        public int Count
        {
            get
            {
                var i = 0;
                foreach (var dataTable in _dataTables)
                {
                    if (dataTable is FeatureDataTable)
                        i++;
                }
                return i;
            }
        }

        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public FeatureDataTable this[int index]
        {
            get
            {
                var i = 0;
                foreach (var dataTable in _dataTables)
                {
                    if (dataTable is FeatureDataTable)
                    {
                        if (i == index)
                            return (FeatureDataTable) dataTable;
                        i++;
                    }
                }
                throw new ArgumentOutOfRangeException("index");
            }
        }
    }
}