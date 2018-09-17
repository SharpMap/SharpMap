using System.Data.SQLite;
using BruTile;

namespace SharpMap.Data.Providers.Tiles
{
    /// <summary>
    /// A BruTile tile source implementation for GeoPackage
    /// </summary>
    internal class GpkgTileSource : ITileSource
    {
        private const string SelectTemplate =
            "SELECT \"tile_data\" FROM \"{0}\" WHERE \"zoom_level\"=? AND \"tile_column\"=? AND \"tile_row\"=?;";
        
        private readonly GpkgContent _content;
        private readonly GpkgTileSchema _schema;
        private readonly string _selectSql;

        public GpkgTileSource(GpkgContent content, GpkgTileSchema tileSchema)
        {
            _content = content;
            _schema = tileSchema;
            _selectSql = string.Format(SelectTemplate, content.TableName);
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            using (var cn = new SQLiteConnection(_content.ConnectionString).OpenAndReturn())
            {
                var cmd = new SQLiteCommand(_selectSql, cn);
                cmd.Parameters.AddRange(new object[] { int.Parse(tileInfo.Index.Level), tileInfo.Index.Col, tileInfo.Index.Row });
                return (byte[]) cmd.ExecuteScalar();
            }
        }

        public ITileSchema Schema
        {
            get { return _schema; }
        }

        public string Name
        {
            get { return string.Format("{0} ({1})", _content.Identifier, _content.TableName); }
        }

#if !NET40
        public Attribution Attribution { get; set; }
#endif
    }
}
