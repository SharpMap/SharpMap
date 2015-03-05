using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Text;
using GeoAPI.Geometries;
using SharpMap.Data.Providers.IO;

namespace SharpMap.Data.Providers
{
    [Serializable]
    internal class GpkgProvider : BaseProvider
    {
        private const string SqlBuildRtreeConstraint = 
            "SELECT 'rtree_' || \"table_name\" || '_' || \"column_name\" FROM \"gpkg_extensions\" WHERE \"table_name\"=? AND \"extension_name\"='gpkg_rtree_index';";

        private readonly GpkgContent _content;
        private readonly GpkgStandardBinaryReader _reader;
        private readonly Envelope _extent;
        private readonly string _rtreeConstraint;
        private readonly FeatureDataTable _baseTable;

        /// <summary>
        /// Creates an instanc of this class
        /// </summary>
        /// <param name="content">The geopackage content</param>
        public GpkgProvider(GpkgContent content)
            :base(content.SRID)
        {
            _content = content;
            ConnectionID = content.ConnectionString;
            _reader = new GpkgStandardBinaryReader(GeoAPI.GeometryServiceProvider.Instance);
            _extent = content.Extent;
            _baseTable = content.GetBaseTable();
            _rtreeConstraint = BuildRtreeConstraint(content);
        }

        private string BuildRtreeConstraint(GpkgContent content)
        {
            string rtreeName;
            using (var cn = new SQLiteConnection(_content.ConnectionString).OpenAndReturn())
            {
                var cmd = new SQLiteCommand(SqlBuildRtreeConstraint, cn);
                cmd.Parameters.AddWithValue(null, content.TableName);
                var tmp = cmd.ExecuteScalar();
                if (tmp == null) 
                    return string.Empty;
                rtreeName = (string) tmp;
            }

            var sb = new StringBuilder();
            sb.AppendFormat(" \"{0}\" IN (", _content.OidColumn);
            sb.AppendFormat("SELECT \"id\" FROM \"{0}\" WHERE minX>=? AND maxX<=? AND minY>=? AND maxY<=?)", rtreeName);
            
            return sb.ToString();
        }

        private SQLiteDataReader CreateReader(int i, Envelope extent = null, string definitionQuery = null)
        {
            var cn = new SQLiteConnection(_content.ConnectionString).OpenAndReturn();
            var cmd = new SQLiteCommand(string.Empty, cn);

            var sql = new StringBuilder("SELECT ");

            // make sure that we get object id and geometry if we want feature data table!
            if ((i & 4) == 4) i |= 1 + 2;
            // make sure that we get dont get anything else if we want the feature count!
            if ((i & 8) == 8) i = 8;

            // Select columns
            var columns = new List<string>();
            if ((i & 1) == 1)
                columns.Add(string.Format("\"{0}\"", _content.OidColumn));

            if ((i & 4) == 4)
            {
                for (var j = 1; j < _baseTable.Columns.Count; j++)
                    columns.Add(string.Format("\"{0}\"", _baseTable.Columns[j].ColumnName));
            }

            if ((i & 2) == 2)
                columns.Add(string.Format("\"{0}\"", _content.GeometryColumn));

            if ((i & 8) == 8)
                columns.Add("COUNT(*)");

            // FROM
            sql.AppendFormat("{0} FROM \"{1}\"", string.Join(",", columns), _content.TableName);

            var whereAdded = false;
            definitionQuery = definitionQuery ?? DefinitionQuery;
            if (!string.IsNullOrEmpty(definitionQuery))
            {
                sql.AppendFormat(" WHERE {0}", definitionQuery);
                whereAdded = true;
            }

            // spatial Constraint
            if ((i & 8) == 0 && extent != null)
            {
                if (!string.IsNullOrEmpty(_rtreeConstraint))
                {
                    var addX = extent.Width*0.000012 + Double.Epsilon;
                    var addY = extent.Height*0.000012 + Double.Epsilon;
                    sql.AppendFormat(" {0} {1}", whereAdded ? "AND" : "WHERE", _rtreeConstraint);
                    cmd.Parameters.AddWithValue(null, extent.MinX-addX); 
                    cmd.Parameters.AddWithValue(null, extent.MaxX+addX);
                    cmd.Parameters.AddWithValue(null, extent.MinY-addY);
                    cmd.Parameters.AddWithValue(null, extent.MaxY+addY);
                }
            }

            // Terminate statment
            sql.Append(";");
            cmd.CommandText = sql.ToString();

            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Gets or sets a value indicating an amendment to the sql
        /// </summary>
        public string DefinitionQuery { get; set; }

        /// <summary>
        /// Gets the features within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="GeoAPI.Geometries.Envelope"/></returns>
        public override Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var res = new Collection<IGeometry>();
            using (var reader = CreateReader(2, bbox))
            {
                while (reader.Read())
                {
                    var gpkg = _reader.Read((byte[]) reader.GetValue(0));
                    if (gpkg.Header.IsEmpty) continue;

                    if (bbox.Intersects(gpkg.Header.Extent))
                        res.Add(gpkg.GetGeometry());
                    //else
                    //{
                    //    System.Threading.Thread.Sleep(1);
                    //}
                }
            }
            return res;
        }

        /// <summary>
        /// Returns all objects whose <see cref="GeoAPI.Geometries.Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their <see cref="GeoAPI.Geometries.Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
        public override Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var res = new Collection<uint>();
            using (var rdr = CreateReader(1, bbox))
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                        res.Add((uint) rdr.GetInt64(0));
                }
            }
            return res;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public override IGeometry GetGeometryByID(uint oid)
        {
            using (var rdr = CreateReader(2, null, string.Format("\"{0}\"={1}", _content.OidColumn, oid)))
            {
                if (rdr.HasRows)
                {
                    rdr.Read();
                    var geom = _reader.Read((byte[]) rdr.GetValue(0));
                    if (!geom.Header.IsEmpty) 
                        return geom.GetGeometry();
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public override void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            var table = (FeatureDataTable)_baseTable.Copy();
            table.TableName = _content.TableName;

            table.BeginLoadData();
            using (var rdr = CreateReader(7, box))
            {
                if (rdr.HasRows)
                {
                    var geometryIndex = rdr.FieldCount - 1;
                    while (rdr.Read())
                    {
                        var geom = _reader.Read((byte[])rdr.GetValue(geometryIndex));
                        if (geom.Header.IsEmpty) continue;
                        if (!box.Intersects(geom.Header.Extent)) continue;

                        var data = new object[geometryIndex];
                        rdr.GetValues(data);
                        var fdr = (FeatureDataRow) table.LoadDataRow(data, true);
                        fdr.Geometry = geom.GetGeometry();
                    }
                }
            }
            table.EndLoadData();

            if (table.Rows.Count > 0)
                ds.Tables.Add(table);
        }

        /// <summary>
        /// Function to return the total number of features in the dataset
        /// </summary>
        /// <returns>The number of features</returns>
        public override int GetFeatureCount()
        {
            using (var rdr = CreateReader(8))
            {
                if (!rdr.HasRows) return 0;
                rdr.Read();
                return rdr.GetInt32(0);
            }
        }

        /// <summary>
        /// Function to return a <see cref="SharpMap.Data.FeatureDataRow"/> based on <paramref name="oid">RowID</paramref>
        /// </summary>
        /// <param name="oid">The unique identifier of the row</param>
        /// <returns>datarow</returns>
        public override FeatureDataRow GetFeature(uint oid)
        {
            using (var rdr = CreateReader(7, definitionQuery: string.Format("\"{0}\"={1}", _content.OidColumn, oid)))
            {
                if (rdr.HasRows)
                {
                    var data = new object[rdr.FieldCount - 1];
                    rdr.Read();
                    rdr.GetValues(data);
                    var row = _baseTable.NewRow();
                    row.ItemArray = data;
                    row.Geometry = _reader.Read((byte[]) rdr.GetValue(rdr.FieldCount - 1)).GetGeometry();
                    return row;
                }
            }
            return null;
        }

        /// <summary>
        /// Function to return the <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>The extent of the dataset</returns>
        public override Envelope GetExtents()
        {
            return _extent;
        }

        /// <summary>
        /// Method to perform the intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter</param>
        /// <param name="ds">The feature data set to store the results in</param>
        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            var prepGeom = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);

            var table = (FeatureDataTable)_baseTable.Copy();
            table.TableName = _content.TableName;

            table.BeginLoadData();
            using (var rdr = CreateReader(7, geom.EnvelopeInternal))
            {
                if (rdr.HasRows)
                {
                    var geometryIndex = rdr.FieldCount - 1;
                    while (rdr.Read())
                    {
                        var gpkggeom = _reader.Read((byte[])rdr.GetValue(geometryIndex));
                        if (gpkggeom.Header.IsEmpty) continue;
                        var tmpGeom = gpkggeom.GetGeometry();
                        if (prepGeom.Intersects(tmpGeom))
                        {
                            var data = new object[geometryIndex];
                            rdr.GetValues(data);
                            var fdr = (FeatureDataRow) table.LoadDataRow(data, true);
                            fdr.Geometry = tmpGeom;
                        }
                    }
                }
            }
            table.EndLoadData();

            if (table.Rows.Count > 0)
                ds.Tables.Add(table);
        }
    }
}