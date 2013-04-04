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

// Based on ngpsql: http://pgfoundry.org/projects/npgsql
// The following is only proof-of-concept. A final implementation should be based
// on "src\Npgsql\NpgsqlConnector.cs"


using System;

namespace SharpMap.Data.Providers.Pooling
{
    /// <summary>
    /// The Connector class implements the logic for the Connection 
    /// Objects to access the physical connection to the data source, and 
    /// isolate the application developer from connection pooling 
    /// internals.
    /// </summary>
    internal class Connector
    {
        /// <summary>Instance Counting</summary>
        /// <remarks>!!! for debugging only</remarks>
        private static int InstanceCounter;

        private IProvider _Provider;

        /// <summary>Counts the numbers of Connections that share
        /// this Connector. Used in Release() to decide whether this
        /// connector is to be moved to the PooledConnectors list.</summary>
        internal int _ShareCount;

        /// <summary>Buffer for the public Shared property</summary>
        private bool _Shared;

        internal int InstanceNumber;

        /// <summary>True if the physical connection is in used 
        /// by a Connection Object. That is, if the connector is
        /// not contained in the PooledConnectors List.</summary>
        internal bool InUse;

        /// <summary>Buffer for the public Pooled property</summary>
        /// <remarks>Pooled will be ignored if Shared is set!</remarks>
        internal bool Pooled;

        /// <summary>
        /// Constructor, initializes the Connector object.
        /// </summary>
        internal Connector(IProvider provider, bool Shared)
        {
            Provider = provider;
            _Shared = Shared;
            Pooled = true;
            InstanceCounter++;
            InstanceNumber = InstanceCounter;
        }

        /// <summary>Controls the physical connection sharing.</summary>
        /// <remarks>Can only be set via ConnectorPool.Request().</remarks>
        internal bool Shared
        {
            get { return _Shared; }
            set { if (!InUse) _Shared = value; }
        }

        /// <summary>Share count, read only</summary>
        /// <remarks>!!! for debugging only</remarks>
        internal int ShareCount
        {
            get { return _ShareCount; }
        }

        /// <summary>
        /// Used to connect to the data source. 
        /// </summary>
        internal IProvider Provider
        {
            get { return _Provider; }
            set
            {
                if (InUse)
                {
                    throw new ApplicationException("Provider cannot be modified if connection is open.");
                }
                _Provider = value;
            }
        }

        /// <summary>
        /// Opens the physical connection to the server.
        /// </summary>
        /// <remarks>Usually called by the RequestConnector
        /// Method of the connection pool manager.</remarks>
        internal void Open()
        {
            Provider.Open();
        }

        internal void Release()
        {
            if (_Shared)
            {
                // A shared connector is returned to the pooled connectors
                // list only if it is not used by any Connection object.
                // Otherwise the job is done by simply decrementing the
                // usage counter:
                if (--_ShareCount == 0)
                {
                    // Shared connectors are *always* pooled after usage.
                    // Depending on the Pooled property at this point
                    // might introduce a lot of trouble into an application...
                    ConnectorPool.ConnectorPoolManager.SharedConnectors.Remove(this);
                    ConnectorPool.ConnectorPoolManager.PooledConnectors.Add(this);
                    Pooled = true;
                    InUse = false;
                }
            }
            else // it is a nonshared connector
            {
                if (Pooled)
                {
                    // Pooled connectors are simply put in the
                    // PooledConnectors list for later recycling
                    InUse = false;
                    ConnectorPool.ConnectorPoolManager.PooledConnectors.Add(this);
                }
                else
                {
                    // Unpooled private connectors get the physical
                    // connection closed, they are *not* recyled later.
                    // Instead they are (implicitly) handed over to the
                    // garbage collection.
                    // !!! to be fixed
                    Provider.Close();
                    Provider.Dispose();
                }
            }
        }
    }
}