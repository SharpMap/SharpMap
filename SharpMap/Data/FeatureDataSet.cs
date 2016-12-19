// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
// Copyright 2014       - Spartaco Giubbolini, Felix Obermaier
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Data
{
    /// <summary>
    /// Represents an in-memory cache of spatial data. The FeatureDataSet is an extension of System.Data.DataSet
    /// </summary>
    /// <remarks>Serialization is achieved using the approach described in http://support.microsoft.com/kb/829740/en-us
    /// </remarks>
    [Serializable] 
    public class FeatureDataSet : DataSet, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the FeatureDataSet class.
        /// </summary>
        public FeatureDataSet()
        {}

        /// <summary>
        /// Initializes a new instance of the FeatureDataSet class.
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">streaming context</param>
        protected FeatureDataSet(SerializationInfo info, StreamingContext context)
        {
            DataSetName = info.GetString("name");
            Namespace = info.GetString("ns");
            Prefix = info.GetString("prefix");
            CaseSensitive = info.GetBoolean("case");
            Locale = new CultureInfo(info.GetInt32("locale"));
            EnforceConstraints = info.GetBoolean("enforceCons");
            
            var tables = (DataTable[]) info.GetValue("tables", typeof (DataTable[]));
            base.Tables.AddRange(tables);

            var constraints = (ArrayList)info.GetValue("constraints", typeof(ArrayList));
            SetForeignKeyConstraints(constraints);

            var relations = (ArrayList)info.GetValue("relations", typeof(ArrayList));
            SetRelations(relations);

            var extendedProperties = (PropertyCollection)info.GetValue("extendedProperties", typeof (PropertyCollection));
            if (extendedProperties.Count > 0) // otherwise the next foreach throws exception... weird.
                foreach (DictionaryEntry keyPair in extendedProperties)
                    ExtendedProperties.Add(keyPair.Key, keyPair.Value);        }

        /// <summary>
        /// Gets the collection of tables contained in the FeatureDataSet
        /// </summary>
        public new FeatureTableCollection Tables
        {
            get { return new FeatureTableCollection(base.Tables); }
        }

        /// <summary>
        /// Copies the structure of the FeatureDataSet, including all FeatureDataTable schemas, relations, and constraints. Does not copy any data. 
        /// </summary>
        /// <returns></returns>
        public new FeatureDataSet Clone()
        {
            var cln = ((FeatureDataSet) (base.Clone()));
            return cln;
        }

        //private void InitClass()
        //{
        //    Prefix = "";
        //    Namespace = "sm";
        //    Locale = new CultureInfo("en-US");
        //    CaseSensitive = false;
        //    EnforceConstraints = true;
        //}

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", DataSetName);
            info.AddValue("ns", Namespace);
            info.AddValue("prefix", Prefix);
            info.AddValue("case", CaseSensitive);
            info.AddValue("locale", Locale.LCID);
            info.AddValue("enforceCons", EnforceConstraints);
            info.AddValue("tables", base.Tables.OfType<DataTable>().ToArray());
            info.AddValue("constraints", GetForeignKeyConstraints());
            info.AddValue("relations", GetRelations());
            info.AddValue("extendedProperties", ExtendedProperties);
        }

        private ArrayList GetForeignKeyConstraints()
         {
            var constraintList = new ArrayList();
            var tables = base.Tables;
            for (int i = 0; i < tables.Count; i++)
            {
                DataTable dt = tables[i];
                for (int j = 0; j < dt.Constraints.Count; j++)
                {
                    Constraint c = dt.Constraints[j];
                    var fk = c as ForeignKeyConstraint;
                    if (fk != null)
                    {
                        string constraintName = c.ConstraintName;
                        var parentInfo = new int[fk.RelatedColumns.Length + 1];
                        parentInfo[0] = tables.IndexOf(fk.RelatedTable);
                        for (int k = 1; k < parentInfo.Length; k++)
                        {
                            parentInfo[k] = fk.RelatedColumns[k - 1].Ordinal;
                        }

                        int[] childInfo = new int[fk.Columns.Length + 1];
                        childInfo[0] = i;//Since the constraint is on the current table, this is the child table.
                        for (int k = 1; k < childInfo.Length; k++)
                        {
                            childInfo[k] = fk.Columns[k - 1].Ordinal;
                        }

                        var list = new ArrayList
                        {
                            constraintName,
                            parentInfo,
                            childInfo,
                            new[] {(int) fk.AcceptRejectRule, (int) fk.UpdateRule, (int) fk.DeleteRule}
                        };
                        var extendedProperties = new Hashtable();
                        if (fk.ExtendedProperties.Keys.Count > 0)
                        {
                            foreach (object propertyKey in fk.ExtendedProperties.Keys)
                            {
                                extendedProperties.Add(propertyKey, fk.ExtendedProperties[propertyKey]);
                            }
                        }
                        list.Add(extendedProperties);

                        constraintList.Add(list);
                    }
                }
            }
            return constraintList;
         }

        private ArrayList GetRelations()
        {
            var relationList = new ArrayList();

            var tables = base.Tables;
            foreach (DataRelation rel in Relations)
            {
                string relationName = rel.RelationName;
                var parentInfo = new int[rel.ParentColumns.Length + 1];
                parentInfo[0] = tables.IndexOf(rel.ParentTable);
                for (int j = 1; j < parentInfo.Length; j++)
                {
                    parentInfo[j] = rel.ParentColumns[j - 1].Ordinal;
                }

                var childInfo = new int[rel.ChildColumns.Length + 1];
                childInfo[0] = tables.IndexOf(rel.ChildTable);
                for (int j = 1; j < childInfo.Length; j++)
                {
                    childInfo[j] = rel.ChildColumns[j - 1].Ordinal;
                }

                var list = new ArrayList {relationName, parentInfo, childInfo, rel.Nested};
                var extendedProperties = new Hashtable();
                if (rel.ExtendedProperties.Keys.Count > 0)
                {
                    foreach (object propertyKey in rel.ExtendedProperties.Keys)
                    {
                        extendedProperties.Add(propertyKey, rel.ExtendedProperties[propertyKey]);
                    }
                }
                list.Add(extendedProperties);

                relationList.Add(list);
            }
            return relationList;
        }

        private void SetForeignKeyConstraints(ArrayList constraintList)
        {
            Debug.Assert(constraintList != null);

            var tables = base.Tables;

            foreach (ArrayList list in constraintList)
            {
                Debug.Assert(list.Count == 5);
                string constraintName = (string)list[0];
                int[] parentInfo = (int[])list[1];
                int[] childInfo = (int[])list[2];
                int[] rules = (int[])list[3];
                Hashtable extendedProperties = (Hashtable)list[4];

                //ParentKey Columns.
                Debug.Assert(parentInfo.Length >= 1);
                DataColumn[] parentkeyColumns = new DataColumn[parentInfo.Length - 1];
                for (int i = 0; i < parentkeyColumns.Length; i++)
                {
                    Debug.Assert(tables.Count > parentInfo[0]);
                    Debug.Assert(tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
                    parentkeyColumns[i] = tables[parentInfo[0]].Columns[parentInfo[i + 1]];
                }

                //ChildKey Columns.
                Debug.Assert(childInfo.Length >= 1);
                DataColumn[] childkeyColumns = new DataColumn[childInfo.Length - 1];
                for (int i = 0; i < childkeyColumns.Length; i++)
                {
                    Debug.Assert(tables.Count > childInfo[0]);
                    Debug.Assert(tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
                    childkeyColumns[i] = tables[childInfo[0]].Columns[childInfo[i + 1]];
                }

                //Create the Constraint.
                ForeignKeyConstraint fk = new ForeignKeyConstraint(constraintName, parentkeyColumns, childkeyColumns);
                Debug.Assert(rules.Length == 3);
                fk.AcceptRejectRule = (AcceptRejectRule)rules[0];
                fk.UpdateRule = (Rule)rules[1];
                fk.DeleteRule = (Rule)rules[2];

                //Extended Properties.
                Debug.Assert(extendedProperties != null);
                if (extendedProperties.Keys.Count > 0)
                {
                    foreach (object propertyKey in extendedProperties.Keys)
                    {
                        fk.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
                    }
                }

                //Add the constraint to the child datatable.
                Debug.Assert(tables.Count > childInfo[0]);
                tables[childInfo[0]].Constraints.Add(fk);
            }
        }

        private void SetRelations(ArrayList relationList)
        {
            Debug.Assert(relationList != null);

            var tables = base.Tables;
            foreach (ArrayList list in relationList)
            {
                Debug.Assert(list.Count == 5);
                var relationName = (string)list[0];
                var parentInfo = (int[])list[1];
                var childInfo = (int[])list[2];
                var isNested = (bool)list[3];
                var extendedProperties = (Hashtable)list[4];

                //ParentKey Columns.
                Debug.Assert(parentInfo.Length >= 1);
                var parentkeyColumns = new DataColumn[parentInfo.Length - 1];
                for (int i = 0; i < parentkeyColumns.Length; i++)
                {
                    Debug.Assert(tables.Count > parentInfo[0]);
                    Debug.Assert(tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
                    parentkeyColumns[i] = tables[parentInfo[0]].Columns[parentInfo[i + 1]];
                }

                //ChildKey Columns.
                Debug.Assert(childInfo.Length >= 1);
                var childkeyColumns = new DataColumn[childInfo.Length - 1];
                for (int i = 0; i < childkeyColumns.Length; i++)
                {
                    Debug.Assert(tables.Count > childInfo[0]);
                    Debug.Assert(tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
                    childkeyColumns[i] = tables[childInfo[0]].Columns[childInfo[i + 1]];
                }

                //Create the Relation, without any constraints[Assumption: The constraints are added earlier than the relations]
                var rel = new DataRelation(relationName, parentkeyColumns, childkeyColumns, false);
                rel.Nested = isNested;

                //Extended Properties.
                Debug.Assert(extendedProperties != null);
                if (extendedProperties.Keys.Count > 0)
                {
                    foreach (object propertyKey in extendedProperties.Keys)
                    {
                        rel.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
                    }
                }

                //Add the relations to the dataset.
                Relations.Add(rel);
            }
        }
   }

    /// <summary>
    /// Represents the method that will handle the RowChanging, RowChanged, RowDeleting, and RowDeleted events of a FeatureDataTable. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void FeatureDataRowChangeEventHandler(object sender, FeatureDataRowChangeEventArgs e);

    /// <summary>
    /// Represents one feature table of in-memory spatial data. 
    /// </summary>
    [DebuggerStepThrough]
    [Serializable]
    public class FeatureDataTable : DataTable, IEnumerable
    {
        /// <summary>
        /// Initializes a new instance of the FeatureDataTable class with no arguments.
        /// </summary>
        public FeatureDataTable() 
        {
            //InitClass();
        }

        /// <summary>
        /// Creates an instance of this class from serialization
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public FeatureDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            using (var ms = new MemoryStream((byte[]) info.GetValue("geometries", typeof(byte[]))))
            {
                using (var reader = new BinaryReader(ms))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var rowIndex = reader.ReadInt32();
                    var row = (FeatureDataRow) Rows[rowIndex];
                    var srid = reader.ReadInt32();
                    var wkbReader = new WKBReader(GeometryServiceProvider.Instance.CreateGeometryFactory(srid));
                    var wkbSize = reader.ReadInt32();
                    var wkb = reader.ReadBytes(wkbSize);
                    row.Geometry = wkbReader.Read(wkb);
                }
            }}
        }
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

        /// <summary>
        /// Gets the feature data row at the specified index
        /// </summary>
        /// <param name="index">row index</param>
        /// <returns>FeatureDataRow</returns>
        public FeatureDataRow this[int index]
        {
            get { return (FeatureDataRow) Rows[index]; }
        }

        #region IEnumerable Members

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
        /// Clones the structure of the FeatureDataTable, including all FeatureDataTable schemas and constraints. 
        /// </summary>
        /// <returns></returns>
        public new FeatureDataTable Clone()
        {
            var cln = ((FeatureDataTable) (base.Clone()));
            //cln.InitVars();
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

        //internal void InitVars()
        //{
        //    //this.columnFeatureGeometry = this.Columns["FeatureGeometry"];
        //}

        //private void InitClass()
        //{
        //    //this.columnFeatureGeometry = new DataColumn("FeatureGeometry", typeof(GeoAPI.Geometries.IGeometry), null, System.Data.MappingType.Element);
        //    //this.Columns.Add(this.columnFeatureGeometry);
        //}

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
        /// Populates a serialization information object with the data needed to serialize the <see cref="T:System.Data.DataTable"/>.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object that holds the serialized data associated with the <see cref="T:System.Data.DataTable"/>.</param><param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> object that contains the source and destination of the serialized stream associated with the <see cref="T:System.Data.DataTable"/>.</param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic).</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            var rowIndex = 0;
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    foreach (FeatureDataRow row in Rows)
                    {
                        if (row.IsFeatureGeometryNull()) continue;
                        writer.Write(rowIndex++);
                        writer.Write(row.Geometry.SRID);
                        var wkb = row.Geometry.AsBinary();
                        writer.Write(wkb.Length);
                        writer.Write(wkb);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    info.AddValue("geometries", ms.ToArray(), typeof(byte[]));
                }
            }
        }
    }

    /*
    /// <summary>
    /// Represents the collection of tables for the FeatureDataSet.
    /// </summary>
    [Serializable()]
    public class FeatureTableCollection : List<FeatureDataTable>
    {
    }
     */

    /// <summary>
    /// Represents the collection of tables for the FeatureDataSet.
    /// It is a proxy to the <see cref="DataSet.Tables"/> object. 
    /// It filters out those <see cref="T:System.Data.DataTable"/> 
    /// that are <see cref="T:SharpMap.Data.FeatureDataTable"/>s.
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
            var dataTables = new DataTable[_dataTables.Count];
            _dataTables.CopyTo(dataTables, 0);

            foreach (var dataTable in dataTables)
            {
                if (dataTable is FeatureDataTable)
                    yield return (FeatureDataTable) dataTable;
            }

        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Method to add a <see cref="FeatureDataTable"/> to this set.
        /// </summary>
        /// <remarks>If <paramref name="item"/> belongs to a different <see cref="FeatureDataSet"/>,
        /// this method attempts to remove it from that. If that is not possible, <paramref name="item"/> 
        /// is copied (<see cref="DataTable.Copy"/>) and the copy is then added.
        /// </remarks>
        /// <param name="item">The feature data table to add</param>
        public void Add(FeatureDataTable item)
        {
            var itemDataSet = item.DataSet;
            if (itemDataSet != null)
            {
                if (itemDataSet.Tables.CanRemove(item))
                    itemDataSet.Tables.Remove(item);
                else
                    item = (FeatureDataTable) item.Copy();
            }
            _dataTables.Add(item);
        }

        /// <summary>
        /// Method to add a range of <see cref="FeatureDataTable"/>s to the (Feature)DataTableCollection.
        /// </summary>
        /// <param name="items">The tables to add</param>
        public void AddRange(IEnumerable<FeatureDataTable> items)
        {
            foreach (var item in items)
            {
                _dataTables.Add(item);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            _dataTables.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(FeatureDataTable item)
        {
            return _dataTables.Contains(item.TableName, item.Namespace);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
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

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(FeatureDataTable item)
        {
            if (_dataTables.CanRemove(item))
            {
                _dataTables.Remove(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove the feature data table at the provided index
        /// </summary>
        /// <param name="index">The index of the table to remove</param>
        /// <returns><c>true</c> if the table was successfully removed</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool RemoveAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            var tmp = 0;

            DataTable tableToRemove = null;
            foreach (DataTable dataTable in _dataTables)
            {
                if (dataTable is FeatureDataTable)
                {
                    if (tmp == index)
                    {
                        tableToRemove = dataTable;
                        break;
                    }
                    tmp++;
                }        
            }

            if (tableToRemove != null)
                return Remove((FeatureDataTable)tableToRemove);

            return false;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
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

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// An indexer to the feature data tables in this set
        /// </summary>
        /// <param name="index">The index of the feature data table to get</param>
        /// <returns>The feature data table at index <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown, if the index is not in the valid range.</exception>
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

    /// <summary>
    /// Represents a row of data in a FeatureDataTable.
    /// </summary>
    [DebuggerStepThrough]
    [Serializable]
    public class FeatureDataRow : DataRow
    {
        //private FeatureDataTable tableFeatureTable;

        private IGeometry _geometry;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="rb">The row builder</param>
        public FeatureDataRow(DataRowBuilder rb)
            : base(rb)
        {
        }

        /// <summary>
        /// The geometry of the current feature
        /// </summary>
        public IGeometry Geometry
        {
            get { return _geometry; }
            set
            {
                if (_geometry == null) {
                    _geometry = value;
                } else {
                    if (ReferenceEquals(_geometry, value))
                        return;
                    if (_geometry != null && _geometry.EqualsTopologically(value))
                        return;
                    _geometry = value;
                    if (RowState == DataRowState.Unchanged) SetModified();
                }
            }
        }

        /// <summary>
        /// Returns true of the geometry is null
        /// </summary>
        /// <returns></returns>
        public bool IsFeatureGeometryNull()
        {
            return _geometry == null;
        }

        /// <summary>
        /// Sets the geometry column to null
        /// </summary>
        public void SetFeatureGeometryNull()
        {
            _geometry = null;
        }
    }

    /// <summary>
    /// Occurs after a FeatureDataRow has been changed successfully.
    /// </summary>
    [DebuggerStepThrough]
    public class FeatureDataRowChangeEventArgs : EventArgs
    {
        private readonly DataRowAction _eventAction;
        private readonly FeatureDataRow _eventRow;

        /// <summary>
        /// Initializes a new instance of the FeatureDataRowChangeEventArgs class.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="action"></param>
        public FeatureDataRowChangeEventArgs(FeatureDataRow row, DataRowAction action)
        {
            _eventRow = row;
            _eventAction = action;
        }

        /// <summary>
        /// Gets the row upon which an action has occurred.
        /// </summary>
        public FeatureDataRow Row
        {
            get { return _eventRow; }
        }

        /// <summary>
        /// Gets the action that has occurred on a FeatureDataRow.
        /// </summary>
        public DataRowAction Action
        {
            get { return _eventAction; }
        }
    }
}