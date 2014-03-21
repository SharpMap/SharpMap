// Copyright 2007 - Dan and Joel
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using GeoAPI.Features;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// The DataTablePoint provider is used for rendering point data 
    /// from a System.Data.DataTable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The data source will need to have two double-type columns, 
    /// xColumn and yColumn that contains the coordinates of the point,
    /// and an integer-type column containing a unique identifier for each row.
    /// </para>
    /// </remarks>
    public class DataTablePoint : PreparedGeometryProvider
    {
        private string _ConnectionString;
        private string _definitionQuery;
        private string _ObjectIdColumn;
        private DataTable _Table;
        private string _XColumn;
        private string _YColumn;

        /// <summary>
        /// Initializes a new instance of the DataTablePoint provider
        /// </summary>
        /// <param name="dataTable">
        /// Instance of <see cref="DataTable"/> to use as data source.
        /// </param>
        /// <param name="oidColumnName">
        /// Name of the OID column.
        /// </param>
        /// <param name="xColumn">
        /// Name of column where point's X value is stored.
        /// </param>
        /// <param name="yColumn">
        /// Name of column where point's Y value is stored.
        /// </param>
        public DataTablePoint(DataTable dataTable, string oidColumnName,
                              string xColumn, string yColumn)
        {
            Table = dataTable;
            XColumn = xColumn;
            YColumn = yColumn;
            ObjectIdColumn = oidColumnName;
        }

        /// <summary>
        /// Data table used as the data source.
        /// </summary>
        public DataTable Table
        {
            get { return _Table; }
            set { _Table = value; }
        }


        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _ObjectIdColumn; }
            set { _ObjectIdColumn = value; }
        }

        /// <summary>
        /// Name of column that contains X coordinate
        /// </summary>
        public string XColumn
        {
            get { return _XColumn; }
            set { _XColumn = value; }
        }

        /// <summary>
        /// Name of column that contains Y coordinate
        /// </summary>
        public string YColumn
        {
            get { return _YColumn; }
            set { _YColumn = value; }
        }

        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _definitionQuery; }
            set { _definitionQuery = value; }
        }

        #region IProvider Members

        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override IEnumerable<IGeometry> GetGeometriesInView(Envelope bbox, CancellationToken? cancllationToken= null)
        {

            if (Table.Rows.Count == 0)
            {
                yield break;
            }

            string strSQL = XColumn + " > " + bbox.Left().ToString(Map.NumberFormatEnUs) + " AND " +
                            XColumn + " < " + bbox.Right().ToString(Map.NumberFormatEnUs) + " AND " +
                            YColumn + " > " + bbox.Bottom().ToString(Map.NumberFormatEnUs) + " AND " +
                            YColumn + " < " + bbox.Top().ToString(Map.NumberFormatEnUs);

            var drow = Table.Select(strSQL);

            foreach (var dr in drow)
            {
                yield return Factory.CreatePoint(new Coordinate((double) dr[XColumn], (double) dr[YColumn]));
            }
        }

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override IEnumerable<object> GetOidsInView(Envelope bbox, CancellationToken? cancellationToken = null)
        {
            if (Table.Rows.Count == 0)
            {
                yield break;
            }

            var strSQL = XColumn + " > " + bbox.Left().ToString(Map.NumberFormatEnUs) + " AND " +
                         XColumn + " < " + bbox.Right().ToString(Map.NumberFormatEnUs) + " AND " +
                         YColumn + " > " + bbox.Bottom().ToString(Map.NumberFormatEnUs) + " AND " +
                         YColumn + " < " + bbox.Top().ToString(Map.NumberFormatEnUs);

            var drow = Table.Select(strSQL);

            foreach (var dr in drow)
            {
                yield return dr[ObjectIdColumn];
            }
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public override IGeometry GetGeometryByOid(object oid)
        {
            if (Table.Rows.Count == 0)
            {
                return null;
            }

            string selectStatement = ObjectIdColumn + " = " + oid;

            var rows = Table.Select(selectStatement);

            return rows.Length > 0
                    ? Factory.CreatePoint(new Coordinate((double) rows[0][XColumn], (double) rows[0][YColumn]))
                    : null;
        }

        /// <summary>
        /// Retrieves all features within the given BoundingBox.
        /// </summary>
        /// <param name="bounds">Bounds of the region to search.</param>
        /// <param name="fcs">FeatureDataSet to fill data into</param>
        public override void ExecuteIntersectionQuery(Envelope bounds, IFeatureCollectionSet fcs, CancellationToken? cancellationToken = null)
        {
            if (Table.Rows.Count == 0)
            {
                return;
            }

            string statement = XColumn + " > " + bounds.MinX.ToString(Map.NumberFormatEnUs) + " AND " +
                               XColumn + " < " + bounds.MaxX.ToString(Map.NumberFormatEnUs) + " AND " +
                               YColumn + " > " + bounds.MinY.ToString(Map.NumberFormatEnUs) + " AND " +
                               YColumn + " < " + bounds.MaxY.ToString(Map.NumberFormatEnUs);

            var rows = Table.Select(statement);

            var fdt = new FeatureDataTable(Table);

            foreach (DataColumn col in Table.Columns)
            {
                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
            }

            foreach (var dr in rows)
            {
                fdt.ImportRow(dr);
                var fdr = (FeatureDataRow)fdt.Rows[fdt.Rows.Count - 1];
                fdr.Geometry = Factory.CreatePoint(new Coordinate((double) dr[XColumn], (double) dr[YColumn]));
            }

            fcs.Add(fdt);
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>Total number of features</returns>
        public override int GetFeatureCount()
        {
            return Table.Rows.Count;
        }

        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="oid"></param>
        /// <returns>datarow</returns>
        public override IFeature GetFeatureByOid(object oid)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the full extents of the data source as a 
        /// <see cref="Envelope"/>.
        /// </summary>
        /// <returns>
        /// A BoundingBox instance which minimally bounds all the features
        /// available in this data source.
        /// </returns>
        public override Envelope GetExtents()
        {
            if (Table.Rows.Count == 0)
            {
                return null;
            }

            var box = new Envelope();

            foreach (DataRowView dr in Table.DefaultView)
            {
                box.ExpandToInclude((double) dr[XColumn], (double) dr[YColumn]);
            }

            return box;
        }

        #endregion
    }
}