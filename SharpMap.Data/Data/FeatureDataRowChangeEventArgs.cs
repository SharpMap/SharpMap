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

namespace SharpMap.Data
{
    /// <summary>
    /// Represents the method that will handle the RowChanging, RowChanged, RowDeleting, and RowDeleted events of a FeatureDataTable. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void FeatureDataRowChangeEventHandler(object sender, FeatureDataRowChangeEventArgs e);

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