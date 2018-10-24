#if !NETSTANDARD
// Copyright 2006 - Morten Nielsen (www.iter.dk)
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
using System.Data.OleDb;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// The OleDbPoint provider is used for rendering point data from an OleDb compatible data source.
    /// </summary>
    /// <remarks>
    /// <para>The data source will need to have two double-type columns, xColumn and yColumn that contains the coordinates of the point,
    /// and an integer-type column containing a unique identifier for each row.</para>
    /// <para>To get good performance, make sure you have applied indexes on ID, xColumn and yColumns in your data source table.</para>
    /// </remarks>
    [Serializable]
    public class OleDbPoint : XYColumnPoint
    {
        /// <summary>
        /// Initializes a new instance of the OleDbPoint provider
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="tablename">The name of the table</param>
        /// <param name="oidColumnName">The name of the object id column</param>
        /// <param name="xColumn">The name of the x-ordinates column</param>
        /// <param name="yColumn">The name of the y-ordinates column</param>
        public OleDbPoint(string connectionString, string tableName, string oidColumnName, string xColumn, string yColumn)
            : base(OleDbFactory.Instance, connectionString, tableName, oidColumnName, xColumn, yColumn)
        {
        }

        #region Disposers and finalizers

        #endregion
    }
}
#endif
