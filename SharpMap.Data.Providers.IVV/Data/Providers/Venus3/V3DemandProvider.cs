/*
 * Copyright © Ingenieurgruppe IVV GmbH & Co. KG - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * 
 * Written by Felix Obermaier, 11.2016
 * 
 * This file is part of SharpMap.Data.Providers.IVV.
 *
 * Revision History:
 * Date       | Change                                     | by                                           
 * -----------+--------------------------------------------+----------------------
 * 2016.11.14 | Initial version                            | Felix Obermaier
 *
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Data.Providers.Venus3
{
    /// <summary>
    /// Abstract base class for a V3Demand database provider
    /// </summary>
    public abstract class V3DemandProvider : BaseProvider
    {
        protected const string RegionId = "REGIONID";

        protected string _selectGetFeatureCount;
        protected string _selectGetExtents;
        protected string _selectDataDefinition;

        /// <summary>
        /// 
        /// </summary>
        protected string _selectFeatures;
        protected string _selectFeature;
        protected string _selectDataSubQuery;

        /// <summary>
        /// sql command to get 
        /// </summary>
        protected string _selectObjectIds;
        protected string _selectGeometries;
        protected string _selectGeometry;

        protected readonly Dictionary<string, int> _attributes = new Dictionary<string, int>(); 
        protected readonly List<string> _selectedAttributes = new List<string>();

        private Envelope _extents;
        private FeatureDataTable _baseTable ;
        
        protected V3DemandProvider(int srid, string connectionId)
            : base(srid)
        {
            ConnectionID = connectionId;
        }

        public void Clear()
        {
            _selectedAttributes.Clear();
            _selectDataSubQuery = null;
            _baseTable = null;
        }

        public void Include(string dataLabel)
        {
            if (_selectedAttributes.Contains(dataLabel))
                return;
            if (!_attributes.ContainsKey(dataLabel))
                return;
            _selectedAttributes.Add(dataLabel);
            _selectDataSubQuery = null;
            _baseTable = null;
        }

        public void Exclude(string dataLabel)
        {
            if (_selectedAttributes.Contains(dataLabel))
                return;
            _selectedAttributes.Add(dataLabel);
            _selectDataSubQuery = null;
            _baseTable = null;
        }

        protected void FillDataDictionary()
        {
            using (var cn = CreateConnection())
            using (var cmd = CreateCommand(_selectDataDefinition, cn))
            {
                var rdr = cmd.ExecuteReader();
                Debug.Assert(rdr.HasRows);

                while (rdr.Read())
                {
                    if (!_attributes.ContainsKey(Convert.ToString(rdr[1])))
                        _attributes.Add(Convert.ToString(rdr[1]), Convert.ToInt32(rdr[0]));
#if DEBUG
                    else
                    {
                        Trace.WriteLine(string.Format("First    : {0}/{1} ", Convert.ToString(rdr[1]), _attributes[Convert.ToString(rdr[1])]));
                        Trace.WriteLine(string.Format("Duplicate: {0}/{1} ", Convert.ToString(rdr[1]), Convert.ToInt32(rdr[0])));
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Method to create a command
        /// </summary>
        /// <param name="sql">The command</param>
        /// <param name="connection">The connection</param>
        /// <returns>A command</returns>
        protected abstract DbCommand CreateCommand(string sql, DbConnection connection);

        /// <summary>
        /// Method to create a connection based on <see cref="IBaseProvider.ConnectionID"/>.
        /// </summary>
        /// <param name="open"></param>
        /// <returns>A connection</returns>
        protected abstract DbConnection CreateConnection(bool open = true);

        protected abstract DbParameter CreateParameter(string name, Type type, object value);

        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var res = new Collection<IGeometry>();

            using (var cn = CreateConnection())
            using (var cmd = CreateCommand(_selectGeometries, cn))
            {
                AddEnvelopeParameters(cmd, bbox);
                var rdr = cmd.ExecuteReader(CommandBehavior.Default);
                Debug.Assert(rdr.HasRows, "No Rows!");
                var wkbReader = new WKBReader();
                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(0))
                        res.Add(wkbReader.Read((byte[])rdr[0]));
                }
            }
            return res;
        }

        protected void AddEnvelopeParameters(DbCommand cmd, Envelope bbox)
        {
            cmd.Parameters.Add(CreateParameter("maxX", typeof(double), bbox.MaxX));
            cmd.Parameters.Add(CreateParameter("minX", typeof(double), bbox.MinX));
            cmd.Parameters.Add(CreateParameter("maxY", typeof(double), bbox.MaxY));
            cmd.Parameters.Add(CreateParameter("minY", typeof(double), bbox.MinY));
        }

        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var res = new Collection<uint>();

            using (var cn = CreateConnection())
            using (var cmd = CreateCommand(_selectObjectIds, cn))
            {
                AddEnvelopeParameters(cmd, bbox);
                var rdr = cmd.ExecuteReader(CommandBehavior.Default);
                Debug.Assert(rdr.HasRows, "No Rows!");
                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(0))
                        res.Add(Convert.ToUInt32(rdr[0]));
                }
            }
            return res;
        }

        public override IGeometry GetGeometryByID(uint oid)
        {
            using (var cn = CreateConnection())
            using (var cmd = CreateCommand(_selectGeometry, cn))
            {
                cmd.Parameters.Add(CreateParameter("fid", typeof (int), oid));
                var val = cmd.ExecuteScalar();
                if (val != null)
                    return new WKBReader().Read((byte[])val);
                return null;
            }
            throw new Exception("Should never reach here!");
        }

        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            var p = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            
            using (var cn = CreateConnection())
            using (var cmd = CreateCommand(_selectFeatures, cn))
            {
                AddEnvelopeParameters(cmd, geom.EnvelopeInternal);
                
                var rdr = cmd.ExecuteReader();
                Debug.Assert(rdr.HasRows);
                FeatureDataTable fdt = null;
                var wkbReader = new WKBReader();
                while (rdr.Read())
                {
                    if (fdt == null)
                    {
                        fdt = GetBaseTable(rdr);
                        fdt.BeginLoadData();
                    }

                    var tmpGeom = wkbReader.Read((byte[]) rdr[0]);
                    if (!p.Intersects(tmpGeom)) continue;

                    var items = new object[rdr.FieldCount - 1];
                    for (var i = 1; i < rdr.FieldCount; i++)
                        items[i - 1] = rdr.GetValue(i);

                    var fdr = (FeatureDataRow)fdt.LoadDataRow(items, true);
                    fdr.ItemArray = items;
                    fdr.Geometry = tmpGeom;
                }

                if (fdt != null)
                {
                    fdt.EndLoadData();
                    ds.Tables.Add(fdt);
                }
            }
        }

        public override void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(Factory.ToGeometry(box), ds);
        }

        public override int GetFeatureCount()
        {
            using (var cn = CreateConnection())
            using (var cmd = CreateCommand(_selectGetFeatureCount, cn))
                return (int) cmd.ExecuteScalar();
        }

        public override FeatureDataRow GetFeature(uint rowId)
        {
            using (var cn = CreateConnection())
            using (var cmd = CreateCommand(_selectFeature, cn))
            {
                cmd.Parameters.Add(CreateParameter("fid", typeof (int), rowId));
                var rdr = cmd.ExecuteReader(CommandBehavior.SingleRow);
                Debug.Assert(rdr.HasRows);
                rdr.Read();

                var fdt = GetBaseTable(rdr);
                var items = new object[rdr.FieldCount-1];
                for (var i = 1; i < rdr.FieldCount; i++)
                    items[i-1] = rdr.GetValue(i);


                var fdr = fdt.NewRow();
                fdr.ItemArray = items;
                fdr.Geometry = new WKBReader().Read((byte[])rdr[0]);

                return fdr;
            }
        }

        private FeatureDataTable GetBaseTable(DbDataReader rdr)
        {
            if (_baseTable == null)
            { 
            _baseTable = new FeatureDataTable();
            for (var i = 1; i < rdr.FieldCount; i++)
                _baseTable.Columns.Add(rdr.GetName(i), rdr.GetFieldType(i));
            }
            return (FeatureDataTable)_baseTable.Copy();
        }

        public override Envelope GetExtents()
        {
            if (_extents != null)
                return _extents;

            using (var cn = CreateConnection())
            using (var cmd = CreateCommand(_selectGetExtents, cn))
            {
                var rdr = cmd.ExecuteReader(CommandBehavior.SingleRow);
                Debug.Assert(rdr.HasRows);
                rdr.Read();
                return _extents = new Envelope(rdr.GetDouble(0), rdr.GetDouble(2), rdr.GetDouble(1), rdr.GetDouble(3));
            }
        }

        protected string GetGeometryColumn()
        {
            switch (Geometry)
            {
                case V3DemandGeometry.Centroid:
                    return "CENTER";
                case V3DemandGeometry.Area:
                    return "AREA";
            }
            throw new InvalidOperationException();
        }

        public V3DemandGeometry Geometry { get; set; }

        protected abstract string GetDataJoin();

        protected abstract string GetData();

        protected virtual string DecorateEntity(string entity)
        {
            Debug.Assert(!string.IsNullOrEmpty(entity));
            return "\"" + entity + "\"";
        }

        #region insert or update geometry
#region Import

        public void ImportVenus2(string filename)
        { }
#endregion

        public void Insert(FeatureDataTable fdt)
        {
            
        }

        /// <summary>
        /// Method to set a geometry
        /// </summary>
        /// <param name="fid">The feature id</param>
        /// <param name="area">The areal geometry</param>
        public virtual void SetGeometryByID(uint fid, IGeometry area)
        {
            var extent = area.EnvelopeInternal;
            var wkb = area.AsBinary();

            using (var con = CreateConnection())
            {
                if (!area.IsValid)
                {
                    area = area.Buffer(0);
                    Debug.Assert(area.IsValid);
                }

                using (var cmd = con.CreateCommand())
                {
                    var sb = new StringBuilder();
                    var parameters = cmd.Parameters;

                    if (HasGeometry(fid, con))
                    {
                        sb.AppendFormat("UPDATE {0} SET {1}={2}", DecorateEntity("tblRegionGeom"),
                            DecorateEntity(GetGeometryColumn()), DecorateParameter("PGeom", area.AsBinary(), parameters));

                        if (Geometry == V3DemandGeometry.Area)
                        {
                            sb.AppendFormat(", {0}={1}", DecorateEntity("SIZE"), DecorateParameter("PSize", wkb.Length, parameters));
                            sb.AppendFormat(", {0}={1}", DecorateEntity("envx1"), DecorateParameter("Penvx1", extent.MinX, parameters));
                            sb.AppendFormat(", {0}={1}", DecorateEntity("envx2"), DecorateParameter("Penvx2", extent.MaxX, parameters));
                            sb.AppendFormat(", {0}={1}", DecorateEntity("envy1"), DecorateParameter("Penvy1", extent.MinY, parameters));
                            sb.AppendFormat(", {0}={1}", DecorateEntity("envy2"), DecorateParameter("Penvy2", extent.MaxY, parameters));
                        }
                        sb.AppendFormat(" WHERE {0}={1};", DecorateEntity(RegionId),
                            DecorateParameter("PId", (int) fid, parameters));
                    }
                    else
                    {
                        var wkbCenter = Geometry == V3DemandGeometry.Centroid ? wkb : area.PointOnSurface.AsBinary();

                        sb.AppendFormat("INSERT INTO {0} ( {1}, {2}, {3}, {4}, {5}, {6}", DecorateEntity("tblRegionGeom"),
                            DecorateEntity(RegionId), DecorateEntity("CENTER"),
                            DecorateEntity("envX1"), DecorateEntity("envX2"),
                            DecorateEntity("envY1"), DecorateEntity("envY2"));

                        if (Geometry == V3DemandGeometry.Area)
                            sb.AppendFormat(",{0}, {1}", DecorateEntity("SIZE"), DecorateEntity("AREA"));

                        sb.AppendFormat(") VALUES ({0}, {1}, {2}, {3}, {5}, {4}",
                            DecorateParameter("PId", (int) fid, cmd.Parameters),
                            DecorateParameter("PCenter", wkbCenter, cmd.Parameters),
                            DecorateParameter("PenvX1", extent.MinX, cmd.Parameters),
                            DecorateParameter("PenvX2", extent.MaxX, cmd.Parameters),
                            DecorateParameter("PenvY1", extent.MinY, cmd.Parameters),
                            DecorateParameter("PenvY2", extent.MaxY, cmd.Parameters));

                        if (Geometry == V3DemandGeometry.Area)
                            sb.AppendFormat(", {0}, {1}", DecorateParameter("PSize", wkb.Length, cmd.Parameters),
                                DecorateParameter("PArea", wkb, cmd.Parameters));

                        sb.Append(");");
                    }

                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();
                }
            }

        }

        private bool HasGeometry(uint fid, DbConnection con)
        {
            using (var cmd = CreateCommand("", con))
            {
                cmd.CommandText = string.Format("SELECT COUNT(*) FROM {0} WHERE {1}={2}",
                    DecorateEntity("tblRegionGeom"),
                    DecorateEntity(RegionId),
                    DecorateParameter("PId", (int) fid, cmd.Parameters));
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }


        #endregion

        protected abstract string DecorateParameter(string name, object value, DbParameterCollection parameters);
    }
}