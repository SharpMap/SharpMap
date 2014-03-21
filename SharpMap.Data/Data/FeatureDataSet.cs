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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using GeoAPI.Features;

namespace SharpMap.Data
{
    /// <summary>
    /// Represents an in-memory cache of spatial data. The FeatureDataSet is an extension of System.Data.DataSet
    /// </summary>
    [Serializable]
    public class FeatureDataSet : DataSet, IFeatureCollectionSet
    {
        private FeatureTableCollection _featureTables;

        /// <summary>
        /// Initializes a new instance of the FeatureDataSet class.
        /// </summary>
        public FeatureDataSet()
        {
            InitClass();
            //CollectionChangeEventHandler schemaChangedHandler = new CollectionChangeEventHandler(SchemaChanged);
            ////this.Tables.CollectionChanged += schemaChangedHandler;
            //Relations.CollectionChanged += schemaChangedHandler;
            InitClass();
        }

        /// <summary>
        /// Initializes a new instance of the FeatureDataSet class.
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">streaming context</param>
        protected FeatureDataSet(SerializationInfo info, StreamingContext context)
        {
            string strSchema = ((string) (info.GetValue("XmlSchema", typeof (string))));
            if ((strSchema != null))
            {
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(new XmlTextReader(new StringReader(strSchema)));
                if ((ds.Tables["FeatureTable"] != null))
                {
                    Tables.Add(new FeatureDataTable(ds.Tables["FeatureTable"]));
                }
                DataSetName = ds.DataSetName;
                Prefix = ds.Prefix;
                Namespace = ds.Namespace;
                Locale = ds.Locale;
                CaseSensitive = ds.CaseSensitive;
                EnforceConstraints = ds.EnforceConstraints;
                Merge(ds, false, MissingSchemaAction.Add);
            }
            else
            {
                InitClass();
            }
            GetSerializationData(info, context);
            //CollectionChangeEventHandler schemaChangedHandler = new CollectionChangeEventHandler(SchemaChanged);
            ////this.Tables.CollectionChanged += schemaChangedHandler;
            //Relations.CollectionChanged += schemaChangedHandler;
        }

        /// <summary>
        /// Gets the collection of tables contained in the FeatureDataSet
        /// </summary>
        public new FeatureTableCollection Tables
        {
            get { return _featureTables; }
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

        /// <summary>
        /// Gets a value indicating whether Tables property should be persisted.
        /// </summary>
        /// <returns></returns>
        protected override bool ShouldSerializeTables()
        {
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether Relations property should be persisted.
        /// </summary>
        /// <returns></returns>
        protected override bool ShouldSerializeRelations()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        protected override void ReadXmlSerializable(XmlReader reader)
        {
            Reset();
            var ds = new DataSet();
            ds.ReadXml(reader);
            //if ((ds.Tables["FeatureTable"] != null))
            //{
            //    this.Tables.Add(new FeatureDataTable(ds.Tables["FeatureTable"]));
            //}
            DataSetName = ds.DataSetName;
            Prefix = ds.Prefix;
            Namespace = ds.Namespace;
            Locale = ds.Locale;
            CaseSensitive = ds.CaseSensitive;
            EnforceConstraints = ds.EnforceConstraints;
            Merge(ds, false, MissingSchemaAction.Add);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override XmlSchema GetSchemaSerializable()
        {
            MemoryStream stream = new MemoryStream();
            WriteXmlSchema(new XmlTextWriter(stream, null));
            stream.Position = 0;
            return XmlSchema.Read(new XmlTextReader(stream), null);
        }


        private void InitClass()
        {
            _featureTables = new FeatureTableCollection(base.Tables);
            //this.DataSetName = "FeatureDataSet";
            Prefix = "";
            Namespace = "http://tempuri.org/FeatureDataSet.xsd";
            Locale = new CultureInfo("en-US");
            CaseSensitive = false;
            EnforceConstraints = true;
        }

        #region IFeatureCollectionSet implementation
        IFeatureCollection IFeatureCollectionSet.this[string name]
    {
            get
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");

                return base.Tables.Contains(name) 
                    ? (FeatureDataTable)base.Tables[name]
                    : null;
            }
    }

        IFeatureCollection IFeatureCollectionSet.this[int index]
        {
            get
            {
                return Tables[index];
            }
        }

        bool IFeatureCollectionSet.Contains(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            return base.Tables.Contains(name);
        }

        bool IFeatureCollectionSet.Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var dt = base.Tables[name];
            if (base.Tables.CanRemove(dt))
            {
                base.Tables.Remove(dt);
                return true;
            }
            return false;
        }

        public IEnumerator<IFeatureCollection> GetEnumerator()
        {
            foreach (var table in base.Tables)
            {
                yield return (FeatureDataTable) table;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<IFeatureCollection>.Add(IFeatureCollection item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (!(item is FeatureDataTable))
                throw new ArgumentException("Not of type FeatureDataType", "item");

            if (base.Tables.Contains(item.Name))
                throw new DuplicateNameException(string.Format(""));

            base.Tables.Add((DataTable) item);

        }

        bool ICollection<IFeatureCollection>.Contains(IFeatureCollection item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (item is FeatureDataTable)
            {
                var dt = (DataTable) item;
                return base.Tables.Contains(dt.TableName, dt.Namespace);
            }
            return false;
        }

        void ICollection<IFeatureCollection>.CopyTo(IFeatureCollection[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0)
                throw new ArgumentException("Negative array index", "arrayIndex");

            if (arrayIndex + base.Tables.Count >= array.Length)
                throw new ArgumentException("Size of provided array is insufficent", "array");

            for (var i = 0; i < base.Tables.Count; i++)
            {
                array[i + arrayIndex] = (FeatureDataTable)base.Tables[i];
            }
        }

        bool ICollection<IFeatureCollection>.Remove(IFeatureCollection item)
        {
            if (item == null)
                return false;

            if (item is FeatureDataTable)
                if (base.Tables.CanRemove((DataTable) item))
                {
                    base.Tables.Remove((DataTable) item);
                    return true;
                }
            return false;
        }

        int ICollection<IFeatureCollection>.Count { get { return base.Tables.Count; } }

        bool ICollection<IFeatureCollection>.IsReadOnly { get { return false; } }
        #endregion

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _featureTables = new FeatureTableCollection(base.Tables);
        }

    }
}