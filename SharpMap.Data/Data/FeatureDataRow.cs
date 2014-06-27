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
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Data
{
    /// <summary>
    /// Represents a row of data in a FeatureDataTable.
    /// </summary>
    [DebuggerStepThrough]
    [Serializable]
    public class FeatureDataRow : DataRow, IFeature, IFeatureAttributes
    {
        private IGeometry _geometry;

        internal FeatureDataRow(DataRowBuilder rb) 
            : base(rb)
        {
        }

        public IFeatureFactory Factory { get { return (FeatureDataTable) Table; } }

        /// <summary>
        /// The geometry of the current feature
        /// </summary>
        public IGeometry Geometry
        {
            get { return _geometry; }
            set { _geometry = value; }
        }

        /// <summary>
        /// Returns true of the geometry is null
        /// </summary>
        /// <returns></returns>
        public bool IsFeatureGeometryNull()
        {
            return Geometry == null;
        }

        /// <summary>
        /// Sets the geometry column to null
        /// </summary>
        public void SetFeatureGeometryNull()
        {
            Geometry = null;
        }

        #region Additional IFeature implementation

        IFeatureAttributes IFeature.Attributes { get { return this; } }

        #endregion

        #region IEntity implementation
        object IUnique.Oid
        {
            get
            {
                return this[Table.PrimaryKey[0]];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                var column = Table.PrimaryKey[0];
                if (value.GetType() != column.DataType)
                    throw new ArgumentException("Invalid type","value");
                this[column] = value;
            }
        }

        Type IUnique.GetEntityType()
        {
            return typeof (uint);
        }

        bool IUnique.HasOidAssigned 
        {
            get { return ((IUnique) this).Oid != null; }
        }
        #endregion

        public object Clone()
        {
            var res = (FeatureDataRow) Table.NewRow();
            object[] itemArrray = ItemArray;
            var newItemArray = new object[itemArrray.Length];
            for (var i = 0; i < itemArrray.Length; i++)
            {
                newItemArray[i] = itemArrray[i] is ICloneable ? ((ICloneable)itemArrray[i]).Clone() : itemArrray[i];
            }
            res.ItemArray = newItemArray;
            res.Geometry = (IGeometry) (_geometry != null ? Geometry.Clone() : null);
            return res;
        }

        public void Dispose()
        {
        }

        public object[] GetValues()
        {
            return ItemArray;
        }
    }
}