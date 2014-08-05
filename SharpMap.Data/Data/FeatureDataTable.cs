// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using GeoAPI;
using GeoAPI.Features;
using GeoAPI.Geometries;
using GeoAPI.SpatialReference;

namespace SharpMap.Data
{
    /// <summary>
    /// Interface for all classes that have a spatial reference
    /// </summary>
    public interface IHasSpatialReference
    {
        /// <summary>
        /// Event raised when the <see cref="SpatialReference"/> has changed
        /// </summary>
        event EventHandler SpatialReferenceChanged;
        
        /// <summary>
        /// Gets or sets the spatial reference
        /// </summary>
        ISpatialReference SpatialReference { get; set; }
    }
    
    /// <summary>
    /// Represents one feature table of in-memory spatial data. 
    /// </summary>
    //[DebuggerStepThrough]
    [Serializable]
    public class FeatureDataTable : DataTable, IFeatureCollection, IFeatureFactory, IHasFeatureFactory, IHasSpatialReference
    {
        private IGeometryFactory _geometryFactory;
        private ISpatialReference _spatialReference;

        /// <summary>
        /// Event raised when the <see cref="SpatialReference"/> has changed
        /// </summary>
        public event EventHandler SpatialReferenceChanged;

        /// <summary>
        /// Method invoker for the <see cref="SpatialReferenceChanged"/> event
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnSpatialReferenceChanged(EventArgs e)
        {
            if (SpatialReferenceChanged != null)
                SpatialReferenceChanged(this, e);
        }

        /// <summary>
        /// Gets or sets the spatial reference
        /// </summary>
        public ISpatialReference SpatialReference
        {
            get { return _spatialReference; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _spatialReference = value;
                OnSpatialReferenceChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Initializes a new instance of the FeatureDataTable class with no arguments.
        /// </summary>
        public FeatureDataTable()
        {}

        /// <summary>
        /// Initializes a new instance of the FeatureDataTable class with the specified table name.
        /// </summary>
        /// <param name="table"></param>
        public FeatureDataTable(DataTable table)
            : base(table.TableName)
        {
            if (table.DataSet != null)
            {
                if ((table.CaseSensitive != table.DataSet.CaseSensitive))
                {
                    CaseSensitive = table.CaseSensitive;
                }
                if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))
                {
                    Locale = table.Locale;
                }
                if ((table.Namespace != table.DataSet.Namespace))
                {
                    Namespace = table.Namespace;
                }
            }

            Prefix = table.Prefix;
            MinimumCapacity = table.MinimumCapacity;
            DisplayExpression = table.DisplayExpression;
        }

        /// <summary>
        /// Gets the number of rows in the table
        /// </summary>
        [Browsable(false)]
        public int Count
        {
            get { return Rows.Count; }
        }

        #region IEnumerable Members

        IEnumerator<IFeature> IEnumerable<IFeature>.GetEnumerator()
        {
            foreach (FeatureDataRow row in Rows)
            {
                yield return row;
            }
        }

        /// <summary>
        /// Returns an enumerator for enumerating the rows of the FeatureDataTable
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Occurs after a FeatureDataRow has been changed successfully. 
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowChanged;

        /// <summary>
        /// Occurs when a FeatureDataRow is changing. 
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowChanging;

        /// <summary>
        /// Occurs after a row in the table has been deleted.
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowDeleted;

        /// <summary>
        /// Occurs before a row in the table is about to be deleted.
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowDeleting;

        /// <summary>
        /// Adds a row to the FeatureDataTable
        /// </summary>
        /// <param name="row"></param>
        public void AddRow(FeatureDataRow row)
        {
            Rows.Add(row);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        IFeature IFeatureCollection.this[object oid]
        {
            get
            {
                if (PrimaryKey.Length == 0)
                    return (FeatureDataRow) Rows[Convert.ToInt32(oid)];
                var fdr = (FeatureDataRow)Rows.Find(oid);
                return fdr;
            }
        }

        /*

        /// <summary>
        /// Gets the feature by its identifier
        /// </summary>
        /// <param name="oid">The features identifier</param>
        /// <returns>A feature if present</returns>
        public IFeature GetFeatureByOid(object oid)
        {
            var fdr = (FeatureDataRow)Rows.Find(oid);
            return fdr;
        }

        IFeatureCollection IFeatureCollection.Clone()
        {
            return Clone();
        }
         */

        void IFeatureCollection.AddRange(IEnumerable<IFeature> features)
        {
            if (features == null)
                return;

            BeginLoadData();
            foreach (FeatureDataRow fdr in features)
            {
                var fdrNew = (FeatureDataRow)LoadDataRow(fdr.ItemArray, true);
                //ToDo: Isn't this the correct approach?
                //if (fdr.Geometry != null)
                //    fdrNew.Geometry = (IGeometry)fdr.Geometry.Clone();
                fdrNew.Geometry = fdr.Geometry;
            }
            EndLoadData();
        }

        /// <summary>
        /// Gets the feature data row at the specified index
        /// </summary>
        /// <param name="index">row index</param>
        /// <returns>FeatureDataRow</returns>
        IFeature IFeatureCollection.this[int index]
        {
            get { return (FeatureDataRow)Rows[index]; }
        }

        /// <summary>
        /// Clones the structure of the FeatureDataTable, including all FeatureDataTable schemas and constraints. 
        /// </summary>
        /// <returns></returns>
        public new FeatureDataTable Clone()
        {
            var cln = (FeatureDataTable) base.Clone();
            return cln;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DataTable CreateInstance()
        {
            return new FeatureDataTable();
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table.
        /// </summary>
        /// <returns></returns>
        public new FeatureDataRow NewRow()
        {
            return (FeatureDataRow) base.NewRow();
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table, based on a datarow builder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new FeatureDataRow(builder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Type GetRowType()
        {
            return typeof (FeatureDataRow);
        }

        /// <summary>
        /// Raises the FeatureDataRowChanged event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowChanged(DataRowChangeEventArgs e)
        {
            base.OnRowChanged(e);
            if ((FeatureDataRowChanged != null))
            {
                FeatureDataRowChanged(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow) (e.Row)), e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowChanging event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowChanging(DataRowChangeEventArgs e)
        {
            base.OnRowChanging(e);
            if ((FeatureDataRowChanging != null))
            {
                FeatureDataRowChanging(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow) (e.Row)), e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowDeleted event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDeleted(DataRowChangeEventArgs e)
        {
            base.OnRowDeleted(e);
            if ((FeatureDataRowDeleted != null))
            {
                FeatureDataRowDeleted(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow) (e.Row)), e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowDeleting event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDeleting(DataRowChangeEventArgs e)
        {
            base.OnRowDeleting(e);
            if ((FeatureDataRowDeleting != null))
            {
                FeatureDataRowDeleting(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow) (e.Row)), e.Action));
            }
        }

        ///// <summary>
        ///// Gets the collection of rows that belong to this table.
        ///// </summary>
        //public new DataRowCollection Rows
        //{
        //    get { throw (new NotSupportedException()); }
        //    set { throw (new NotSupportedException()); }
        //}

        /// <summary>
        /// Removes the row from the table
        /// </summary>
        /// <param name="row">Row to remove</param>
        public void RemoveRow(FeatureDataRow row)
        {
            Rows.Remove(row);
        }

        /// <summary>
        /// Method called when the table is about to be serialized
        /// </summary>
        /// <param name="context">The streaming context</param>
        [OnSerialized]
        protected void OnSerializing(StreamingContext context)
        {
            
        }

        /// <summary>
        /// Method called when the table has been serialized
        /// </summary>
        /// <param name="context">The streaming context</param>
        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context)
        {

        }

        IGeometryFactory IFeatureFactory.GeometryFactory
        {
            get
            {
                if (_geometryFactory == null)
                {
                    var gsp = GeometryServiceProvider.Instance;
                    _geometryFactory = gsp.CreateGeometryFactory(SpatialReference.Oid);
                }
                return _geometryFactory;
            }
        }

        IList<IFeatureAttributeDefinition> IFeatureFactory.AttributesDefinition { get { return new AttributeDefinitionWrapper(Columns); } }
        IList<IFeatureAttributeDefinition> IFeatureCollection.AttributesDefinition { get { return new AttributeDefinitionWrapper(Columns); } }

        IFeature IFeatureFactory.Create()
        {
            return NewRow();
        }

        IFeature IFeatureFactory.Create(IGeometry geometry)
        {
            var f = NewRow();
            f.Geometry = geometry;
            return f;
        }

        IFeatureFactory IFeatureFactory.Clone()
        {
            return Clone();
        }

        /*
        IFeatureFactory IFeatureCollection.Factory { get { return this; } }
         */
        bool ICollection<IFeature>.IsReadOnly { get { return false; } }

        void ICollection<IFeature>.Add(IFeature item)
        {
            if (item is FeatureDataRow)
            {
                Rows.Add((FeatureDataRow) item);
                return;
            }
            throw new ArgumentException("item");
        }

        bool ICollection<IFeature>.Contains(IFeature item)
        {
            if (item is FeatureDataRow)
                return Rows.Contains(item.Oid);
            throw new ArgumentException("item");
        }

        void ICollection<IFeature>.CopyTo(IFeature[] array, int arrayIndex)
        {
            for (var i = 0; i < Rows.Count; i++)
            {
                var item = (IFeature) Rows[i];                
                array[i + arrayIndex] = item;
            }
        }

        bool ICollection<IFeature>.Remove(IFeature item)
        {
            if (item is FeatureDataRow)
            {
                try
                {
                    Rows.Remove((FeatureDataRow) item);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            throw new ArgumentException("item");
        }

        string IFeatureCollection.Name { get { return TableName; } set { TableName = value; }}

        IFeatureCollection IFeatureCollection.Clone()
        {
            return (FeatureDataTable)Clone();
        }

        IFeatureFactory IHasFeatureFactory.Factory { get { return this; } }

        #region Nested utility classes
        private class AttributeDefinitionWrapper : IList<IFeatureAttributeDefinition>
        {
            private DataColumnCollection _collection;

            public AttributeDefinitionWrapper(DataColumnCollection collection)
            {
                _collection = collection;
            }


            public IEnumerator<IFeatureAttributeDefinition> GetEnumerator()
            {
                foreach (DataColumn column in _collection)
                {
                    yield return new FeatureAttributeDefinitionWrapper(column);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _collection.GetEnumerator();
            }

            public void Add(IFeatureAttributeDefinition item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(IFeatureAttributeDefinition item)
            {
                return _collection.Contains(item.AttributeName);
            }

            public void CopyTo(IFeatureAttributeDefinition[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public bool Remove(IFeatureAttributeDefinition item)
            {
                throw new NotSupportedException();
            }

            public int Count { get { return _collection.Count; } }
            public bool IsReadOnly { get { return true; } }
            public int IndexOf(IFeatureAttributeDefinition item)
            {
                return _collection.IndexOf(item.AttributeName);
            }

            public void Insert(int index, IFeatureAttributeDefinition item)
            {
                throw new NotSupportedException("Adding feature attribute definition is currently not supported");
            }

            public void RemoveAt(int index)
            {
                _collection.RemoveAt(index);
            }

            public IFeatureAttributeDefinition this[int index]
            {
                get { return new FeatureAttributeDefinitionWrapper(_collection[index]); }
                set { throw new NotSupportedException(); }
            }
        }

        private class FeatureAttributeDefinitionWrapper : IFeatureAttributeDefinition
        {
            private readonly DataColumn _column;

            public FeatureAttributeDefinitionWrapper(DataColumn column)
            {
                _column = column;
            }

            public string AttributeName { get { return _column.ColumnName; } set { _column.ColumnName = value; } }
            public string AttributeDescription { get { return _column.Caption; } set { _column.Caption = value; } }
            public Type AttributeType { get { return _column.DataType; } set { _column.DataType = value; } }
            public bool IsNullable { get { return _column.AllowDBNull; } set { _column.AllowDBNull = value; }}
            public object Default { get { return _column.DefaultValue; } set { _column.DefaultValue = value; } }
        }
        #endregion
    }

    
}