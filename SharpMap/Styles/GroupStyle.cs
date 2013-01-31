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
using System.Collections.Generic;

namespace SharpMap.Styles
{
    /// <summary>
    /// GroupStyle is a holder where several styles can be applied in order
    /// </summary>
    [Serializable]
    public class GroupStyle : VectorStyle
    {
        readonly List<VectorStyle> _styles = new List<VectorStyle>();

        /// <summary>
        /// Indexer to the <see cref="VectorStyle"/>s
        /// </summary>
        /// <param name="idx">The index of the <see cref="VectorStyle"/></param>
        /// <returns>A <see cref="VectorStyle"/></returns>
        public VectorStyle this[int idx]
        {
            get
            {
                return _styles[idx];
            }
        }

        /// <summary>
        /// Gets a value indicating the number of <see cref="VectorStyle"/>s contained in this instance.
        /// </summary>
        public int Count
        {
            get
            {
                return _styles.Count;
            }
        }

        /// <summary>
        /// Method to add a <see cref="VectorStyle"/>
        /// </summary>
        /// <param name="style"></param>
        public void AddStyle(VectorStyle style)
        {
            _styles.Add(style);
        }
    
    }
}
