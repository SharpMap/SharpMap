// Copyright 2014 - SharpMap - Team
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Runtime.Serialization;

namespace SharpMap
{
    /// <summary>
    /// A base exception for SharpMap related exceptions
    /// </summary>
    [Serializable]
    public class SharpMapException : Exception
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public SharpMapException()
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="message"/>
        /// </summary>
        /// <param name="message">A message for the exception</param>
        public SharpMapException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="message"/> and <paramref name="inner"/>.
        /// </summary>
        /// <param name="message">A message for the exception</param>
        /// <param name="inner">The inner exception</param>
        public SharpMapException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance from this class 
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected SharpMapException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}