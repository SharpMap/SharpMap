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
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;
using System.Text;

namespace SharpMap.Data.Providers.Venus3
{
    public class V3DemandProviderAccess : V3DemandProvider
    {
        public V3DemandProviderAccess(string connectionId)
            :this(0, connectionId)
        {
        }

        public V3DemandProviderAccess(int srid, string connectionId)
            : base(srid, connectionId)
        {

            _selectGetFeatureCount = "SELECT COUNT(*) FROM [tblRegionGeom];";
            _selectGetExtents = "SELECT MIN(envX1), MIN(envY1), MAX(envX2), MAX(envY2) FROM [tblRegionGeom];";
            _selectDataDefinition = "SELECT DATAID, LABEL FROM [tblDataDef];";

            FillDataDictionary();
            //return !(other.MinX > _maxx || other.MaxX < _minx || other.MinY > _maxy || other.MaxY < _miny);
            _selectObjectIds = "SELECT [REGIONID] FROM [tblRegionGeom] WHERE NOT(envX1 > ? OR envX2 <= ? OR envY1 > ? OR envY2 <= ? );";
            _selectGeometries = "SELECT %GEOM% FROM [tblRegionGeom] WHERE NOT(envX1 > ? OR envX2 <= ? OR envY1 > ? OR envY2 <= ? );";
            _selectGeometry = "SELECT %GEOM% FROM [tblRegionGeom] WHERE ([REGIONID] = ?);";

            _selectFeatures =
                "SELECT G.%GEOM% AS GEOM, G.[REGIONID], R.[LABEL], R.[MODELID], R.[LOCATION], R.[AREA], R.[REGIONTYPE], R.[FNP] %DATA%" +
                "FROM (([tblRegionGeom] as G " +
                "LEFT JOIN [tblRegion] as R on G.[REGIONID] = R.[REGIONID]) " +
                "%DATAJOIN%) " +
                "WHERE NOT(envX1 > ? OR envX2 <= ? OR envY1 > ? OR envY2 <= ? );";

            _selectFeature =
                "SELECT G.%GEOM% AS GEOM, G.[REGIONID], R.[LABEL], R.[MODELID], R.[LOCATION], R.[AREA], R.[REGIONTYPE], R.[FNP] %DATA%" +
                "FROM (([tblRegionGeom] as G " +
                "LEFT JOIN [tblRegion] as R on G.[REGIONID] = R.[REGIONID]) "+
                "%DATAJOIN%) " +
                "WHERE (G.REGIONID = ?);";
        }

        protected override string GetData()
        {
            if (_selectedAttributes.Count == 0)
                return string.Empty;
            var sb = new StringBuilder();
            foreach (var selectedAttribute in _selectedAttributes)
            {
                sb.AppendFormat(", [_{0}]", selectedAttribute);
            }
            return sb.ToString();
        }

        protected override string GetDataJoin()
        {
            if (_selectedAttributes.Count == 0)
                return string.Empty;
            
            if (_selectDataSubQuery == null)
            {
                var sb = new StringBuilder(string.Format("SELECT {0}", DecorateEntity(RegionId)));
                foreach (var selectedAttribute in _selectedAttributes)
                    sb.AppendFormat(", SUM({0}) AS {1}", 
                        DecorateEntity(selectedAttribute),
                        DecorateEntity("_" + selectedAttribute));
                sb.AppendLine(" FROM (");

                sb.Append("SELECT REGIONID");
                foreach (var selectedAttribute in _selectedAttributes)
                    sb.AppendFormat(", {0} AS {1}", 0, DecorateEntity(selectedAttribute));
                sb.AppendFormat(" FROM {0}", DecorateEntity("tblRegion"));
                //foreach (var selectedAttribute in _selectedAttributes)
                {
                    for (var i = 0; i < _selectedAttributes.Count; i++)
                    {
                        sb.AppendLine();
                        sb.AppendFormat("UNION SELECT {0}", DecorateEntity(RegionId));

                        for (var j = 0; j < i; j++)
                            sb.AppendFormat(", {0} AS {1}", 0, DecorateEntity(_selectedAttributes[j]));
                        sb.AppendFormat(", {0} AS {1}", DecorateEntity("VALUE"), DecorateEntity(_selectedAttributes[i]));
                        for (var j = i + 1; j < _selectedAttributes.Count; j++)
                            sb.AppendFormat(", {0} AS {1}", 0, DecorateEntity(_selectedAttributes[j]));
                        sb.AppendFormat(" FROM {0} WHERE {1}={2}", 
                            DecorateEntity("tblData"), DecorateEntity("DATAID"),
                            _attributes[_selectedAttributes[i]]);
                    }
                }
                sb.AppendFormat(") GROUP BY {0}", DecorateEntity(RegionId));

                _selectDataSubQuery = sb.ToString();
                Trace.WriteLine(string.Format("\n{0};", _selectDataSubQuery));
            }

            return "LEFT JOIN (" + _selectDataSubQuery + string.Format(") AS D ON D.{0} = G.{0} ", DecorateEntity(RegionId));
        }

        protected override string DecorateEntity(string entity)
        {
            return "[" + entity + "]";
        }

        protected override DbCommand CreateCommand(string sql, DbConnection connection)
        {
            sql = sql.Replace("%GEOM%", GetGeometryColumn());
            sql = sql.Replace("%DATA%", GetData());
            sql = sql.Replace("%DATAJOIN%", GetDataJoin());
            var res = new OleDbCommand(sql, (OleDbConnection)connection);
            return res;
        }

        protected override DbConnection CreateConnection(bool open = true)
        {
            var res = new OleDbConnection(ConnectionID);
            res.Open();
            return res;
        }

        protected override DbParameter CreateParameter(string name, Type type, object value)
        {
            var res = new OleDbParameter(name, ToOleDbType(type));
            res.Value = value;
            return res;
            
        }

        private static OleDbType ToOleDbType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    return OleDbType.VarChar;
                case TypeCode.Boolean:
                    return OleDbType.Boolean;
                case TypeCode.Byte:
                    return OleDbType.UnsignedSmallInt;
                case TypeCode.Char:
                    return OleDbType.TinyInt;
                case TypeCode.SByte:
                    return OleDbType.Char;
                case TypeCode.DateTime:
                    return OleDbType.Date;
                case TypeCode.Decimal:
                    return OleDbType.Decimal;
                case TypeCode.Double:
                    return OleDbType.Double;
                case TypeCode.Single:
                    return OleDbType.Single;
                case TypeCode.Int16:
                    return OleDbType.SmallInt;
                case TypeCode.Int32:
                    return OleDbType.Integer;
                case TypeCode.Int64:
                    return OleDbType.BigInt;
                case TypeCode.UInt16:
                    return OleDbType.UnsignedSmallInt;
                case TypeCode.UInt32:
                    return OleDbType.UnsignedInt;
                case TypeCode.UInt64:
                    return OleDbType.UnsignedBigInt;
                case TypeCode.Object:
                    return OleDbType.VarBinary;
            }
            throw new NotSupportedException();
        }

        protected override string DecorateParameter(string name, object value, DbParameterCollection parameters)
        {
            var par = CreateParameter(name, value.GetType(), value);
            parameters.Add(par);

            return "?";
        }
    }
}