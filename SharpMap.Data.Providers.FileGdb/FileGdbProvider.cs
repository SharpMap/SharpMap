﻿// Copyright 2012 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Data.Providers.FileGdb.
// SharpMap.Data.Providers.FileGdb is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Data.Providers.FileGdb is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
using NetTopologySuite.Geometries;
using SharpMap.Data.Providers.Converter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using EsriGdb = Esri.FileGDB.Geodatabase;
using EsriTable = Esri.FileGDB.Table;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Provider implementation for ESRI File Gdb database
    /// </summary>
    [Serializable]
    public class FileGdbProvider : IProvider
    {
        static FileGdbProvider()
        {
            var asr = new AppSettingsReader();

            var fileGdbPath = (string)asr.GetValue("FileGdbNativeDirectory", typeof(string));
            //ensure directory
            if (!Directory.Exists(fileGdbPath))
                throw new DirectoryNotFoundException();

            var path = Environment.GetEnvironmentVariable("PATH");

            var pathSet = false;
            if (!string.IsNullOrEmpty(path))
                pathSet = path.ToLowerInvariant().Contains(fileGdbPath.ToLowerInvariant());

            if (!pathSet)
            {
                path = fileGdbPath + ";" + path;
                Environment.SetEnvironmentVariable("PATH", path);
            }
        }

        [NonSerialized]
        private EsriGdb _esriGdb;

        private DirectoryInfo _esriGdbLocation;

        private EsriGdb EsriGdbInstance
        {
            get
            {
                if (_esriGdbLocation == null)
                    throw new InvalidOperationException();

                return _esriGdb ?? (_esriGdb = EsriGdb.Open(_esriGdbLocation.FullName));
            }
        }

        [NonSerialized]
        private EsriTable _esriTable;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public FileGdbProvider()
        {
        }

        /// <summary>
        /// Creates an instance of this class, assigning the location of the database
        /// </summary>
        /// <param name="path">A path to the directory containing the database files.</param>
        public FileGdbProvider(string path)
        {
            _esriGdbLocation = new DirectoryInfo(path);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Actual implementation for disposing the provider.
        /// </summary>
        /// <param name="disposing">A flag indicating if this function is called from <see cref="IDisposable.Dispose()"/> or the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                if (_esriGdb != null)
                    _esriGdb.Dispose();
                if (_esriTable != null)
                    _esriTable.Dispose();

                IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Gets a value indicating if this provider has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Implementation of IProvider

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        public string ConnectionID
        {
            get { return _esriGdbLocation.FullName; }
            set
            {
                if (_esriGdbLocation != null)
                    throw new InvalidOperationException("Only set 'ConnectionID' once!");
                _esriGdbLocation = new DirectoryInfo(value);
            }
        }

        private string _table;

        /// <summary>
        /// Gets or sets a value indicating the name of the data table.
        /// </summary>
        public string Table
        {
            get { return _table; }
            set
            {
                if (!String.Equals(value, _table, StringComparison.InvariantCultureIgnoreCase))
                {
                    var esriTable = EsriGdbInstance.OpenTable(value);
                    if (esriTable != null)
                    {
                        _esriTable = esriTable;
                        _table = value;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _esriTable != null; }
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the features within the specified <see cref="Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="Envelope"/></returns>
        public Collection<Geometry> GetGeometriesInView(Envelope bbox)
        {
            var res = new Collection<Geometry>();
            foreach (var row in _esriTable.Search("*", string.Empty, FileGdbGeometryConverter.ToEsriExtent(bbox), Esri.FileGDB.RowInstance.Recycle))
            {
                using (var buffer = row.GetGeometry())
                {
                    var geom = FileGdbGeometryConverter.ToSharpMapGeometry(buffer);
                    if (geom != null)
                        res.Add(geom);
                }
            }
            return res;
        }

        /// <summary>
        /// Returns all objects whose <see cref="Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their <see cref="Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var res = new Collection<uint>();
            foreach (var row in _esriTable.Search(string.Empty, string.Empty, FileGdbGeometryConverter.ToEsriExtent(bbox), Esri.FileGDB.RowInstance.Recycle))
                res.Add((uint)row.GetOID());
            return res;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public Geometry GetGeometryByID(uint oid)
        {
            var fdt = CreateTable(_esriTable);
            var where = string.Format(CultureInfo.InvariantCulture, "{0}={1}", fdt.PrimaryKey[0].ColumnName, oid);
            foreach (var row in _esriTable.Search("*", where, Esri.FileGDB.RowInstance.Recycle))
            {
                return FileGdbGeometryConverter.ToSharpMapGeometry(row.GetGeometry());
            }
            return null;
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(geom.EnvelopeInternal, ds);
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            var fdt = CreateTable(_esriTable);
            fdt.BeginLoadData();
            var fi = _esriTable.FieldInformation;

            foreach (var row in _esriTable.Search("*", string.Empty, FileGdbGeometryConverter.ToEsriExtent(box), Esri.FileGDB.RowInstance.Recycle))
            {
                var tmp = new List<object>(_esriTable.FieldInformation.Count - 1);
                for (var i = 0; i < fi.Count; i++)
                {
                    if (fi.GetFieldType(i) == Esri.FileGDB.FieldType.Geometry)
                        continue;
                    tmp.Add(row[fi.GetFieldName(i)]);
                }
                var fdr = (FeatureDataRow)fdt.LoadDataRow(tmp.ToArray(), true);

                using (var buffer = row.GetGeometry())
                    fdr.Geometry = FileGdbGeometryConverter.ToSharpMapGeometry(buffer);
            }
            fdt.EndLoadData();
            ds.Tables.Add(fdt);
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            return _esriTable.RowCount;
        }

        /// <summary>
        /// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>datarow</returns>
        public FeatureDataRow GetFeature(uint rowId)
        {
            var fdt = CreateTable(_esriTable);

            fdt.BeginLoadData();
            var where = string.Format(CultureInfo.InvariantCulture, "{0}={1}", fdt.PrimaryKey[0].ColumnName, rowId);
            var fi = _esriTable.FieldInformation;

            foreach (var row in _esriTable.Search("*", where, Esri.FileGDB.RowInstance.Recycle))
            {
                var tmp = new List<object>(_esriTable.FieldInformation.Count - 1);
                for (var i = 0; i < fi.Count; i++)
                {
                    if (fi.GetFieldType(i) == Esri.FileGDB.FieldType.Geometry)
                        continue;
                    tmp.Add(row[fi.GetFieldName(i)]);
                }
                var fdr = (FeatureDataRow)fdt.LoadDataRow(tmp.ToArray(), true);
                fdr.Geometry = FileGdbGeometryConverter.ToSharpMapGeometry(row.GetGeometry());
            }
            fdt.EndLoadData();

            return (FeatureDataRow)fdt.Rows[0];
        }

        private static FeatureDataTable CreateTable(EsriTable table)
        {
            var fdt = new FeatureDataTable();

            var columns = fdt.Columns;

            var fi = table.FieldInformation;
            for (var i = 0; i < fi.Count; i++)
            {
                var ft = fi.GetFieldType(i);
                Type netType;
                var primaryKey = false;
                switch (ft)
                {
                    case Esri.FileGDB.FieldType.OID:
                        netType = typeof(int);
                        primaryKey = true;
                        break;
                    case Esri.FileGDB.FieldType.Single:
                        netType = typeof(float);
                        break;
                    case Esri.FileGDB.FieldType.XML:
                        netType = typeof(string);
                        break;
                    case Esri.FileGDB.FieldType.GlobalID:
                        netType = typeof(int);
                        break;
                    case Esri.FileGDB.FieldType.GUID:
                        netType = typeof(Guid);
                        break;
                    case Esri.FileGDB.FieldType.String:
                        netType = typeof(string);
                        break;
                    case Esri.FileGDB.FieldType.Double:
                        netType = typeof(double);
                        break;
                    case Esri.FileGDB.FieldType.Blob:
                    case Esri.FileGDB.FieldType.Raster:
                        netType = typeof(byte[]);
                        break;
                    case Esri.FileGDB.FieldType.Integer:
                        netType = typeof(long);
                        break;
                    case Esri.FileGDB.FieldType.SmallInteger:
                        netType = typeof(int);
                        break;
                    case Esri.FileGDB.FieldType.Date:
                        netType = typeof(DateTime);
                        break;
                    case Esri.FileGDB.FieldType.Geometry:
                        //Do not add geometry column
                        continue;
                    default:
                        throw new InvalidOperationException("Unknown field type" + ft);
                }

                var dc = new DataColumn(fi.GetFieldName(i), netType)
                {
                    AllowDBNull = fi.GetFieldIsNullable(i),

                    //MaxLength = fi.GetFieldLength(i)
                };
                if (netType == typeof(string))
                    dc.MaxLength = fi.GetFieldLength(i);

                columns.Add(dc);

                if (primaryKey)
                    fdt.PrimaryKey = new[] { dc };
            }
            return fdt;
        }

        /// <summary>
        /// <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>Envelope</returns>
        public Envelope GetExtents()
        {
            var extent = _esriTable.Extent;
            return new Envelope(extent.xMin, extent.xMax, extent.yMin, extent.yMax);
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Nothing to do
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Nothing to do
        }

        /// <summary>
        /// Gets an array of feature dataset names
        /// </summary>
        /// <param name="path">The root path (default = "\")</param>
        /// <returns>An array of feature dataset names.</returns>
        public string[] GetFeatureDatasets(string path = "\\")
        {
            var e = EsriGdbInstance;
            return e.GetChildDatasets(path, "Feature Dataset");
        }

        /// <summary>
        /// Gets an array of feature class names
        /// </summary>
        /// <param name="path">The root path (default = "\")</param>
        /// <returns>An array of feature class names.</returns>
        public string[] GetFeatureClasses(string path = "\\")
        {
            var e = EsriGdbInstance;
            return e.GetChildDatasets(path, "Feature Class");
        }

        /// <summary>
        /// Gets an array of table names
        /// </summary>
        /// <param name="path">The root path (default = "\")</param>
        /// <returns>An array of table names.</returns>
        public string[] GetTables(string path = "\\")
        {
            var e = EsriGdbInstance;
            return e.GetChildDatasets(path, "Table");
        }

        #endregion
#if DEBUG
#pragma warning disable 1591
        public IEnumerable<string> EnumerateTables(string path = "\\")
        {
            var e = EsriGdbInstance;
            foreach (var table in e.GetChildDatasets(path, "Table"))
                yield return "Table: " + table;
            foreach (var table in e.GetChildDatasets(path, "Feature Class"))
                yield return "Feature Class: " + table;

            foreach (var fds in e.GetChildDatasets(path, "Feature Dataset"))
            {
                yield return "Feature Dataset: " + fds;
                foreach (var fd in EnumerateTables(fds))
                {
                    yield return fd;
                }
            }
        }
#pragma warning restore 1591
#endif
    }


}
