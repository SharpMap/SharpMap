using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using BruTile;

namespace SharpMap.Data.Providers.Tiles
{
    internal class GpkgTileSchema : ITileSchema
    {
        private const string SelectTileMatrixSet =
            "SELECT * FROM \"gpkg_tile_matrix_set\" WHERE \"table_name\"=?;";

        private const string SelectTileMatrix =
            "SELECT * FROM \"gpkg_tile_matrix\" WHERE \"table_name\"=? ORDER by \"zoom_level\";";

        private readonly Dictionary<int, Resolution> _resolutions;

        private readonly string _name;

        public GpkgTileSchema(GpkgContent content)
        {
            Debug.Assert(content.DataType == "tiles", "Not a 'tiles' GeoPackageContent");
            _name = content.TableName;

            var tms = GpkgTileMatrixSet.Read(content);
            Format = "image/png";
            Extent = new Extent(content.Extent.MinX, content.Extent.MinY, 
                                content.Extent.MaxX, content.Extent.MaxY);
            Srs = "EPSG:" + content.SRID;
            _resolutions = tms.ToResolutions();
        }

        public int GetTileWidth(int levelId)
        {
            return _resolutions[levelId].TileWidth;
        }

        public int GetTileHeight(int levelId)
        {
            return _resolutions[levelId].TileHeight;
        }

        public double GetOriginX(int levelId)
        {
            return _resolutions[levelId].Left;
        }

        public double GetOriginY(int levelId)
        {
            return _resolutions[levelId].Top; 
        }

        public long GetMatrixWidth(int levelId)
        {
            return _resolutions[levelId].MatrixWidth;
        }

        public long GetMatrixHeight(int levelId)
        {
            return _resolutions[levelId].MatrixHeight;
        }

        public IEnumerable<TileInfo> GetTileInfos(Extent extent, int levelId)
        {
            // todo: move this method elsewhere.
            var range = TileTransform.WorldToTile(extent, levelId, this);

            // todo: use a method to get tilerange for full schema and intersect with requested tilerange.
            var startX = Math.Max(range.FirstCol, GetMatrixFirstCol(levelId));
            var stopX = Math.Min(range.FirstCol + range.ColCount, GetMatrixFirstCol(levelId) + GetMatrixWidth(levelId));
            var startY = Math.Max(range.FirstRow, GetMatrixFirstRow(levelId));
            var stopY = Math.Min(range.FirstRow + range.RowCount, GetMatrixFirstRow(levelId) + GetMatrixHeight(levelId));

            for (var x = startX; x < stopX; x++)
            {
                for (var y = startY; y < stopY; y++)
                {
                    yield return new TileInfo
                    {
                        Extent = TileTransform.TileToWorld(new TileRange(x, y), levelId, this),
                        Index = new TileIndex(x, y, levelId)
                    };
                }
            }
        }

        public IEnumerable<TileInfo> GetTileInfos(Extent extent, double resolution)
        {
            var level = BruTile.Utilities.GetNearestLevel(_resolutions, resolution);
            return GetTileInfos(extent, level);
        }

        public Extent GetExtentOfTilesInView(Extent extent, int levelId)
        {
            return TileSchema.GetExtentOfTilesInView(this, extent, levelId);
        }

        public int GetMatrixFirstCol(int levelId)
        {
            return 0;
        }

        public int GetMatrixFirstRow(int levelId)
        {
            return 0;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Srs { get; private set; }

        public Extent Extent { get; private set; }

        public IDictionary<int, Resolution> Resolutions
        {
            get { return _resolutions; }
        }

        public string Format
        {
            get; private set;
        }

        public YAxis YAxis
        {
            get { return YAxis.OSM; }
        }

        #region Gpkg Tile matrix utility classes
        private class GpkgTileMatrix
        {
            public GpkgTileMatrix(IDataRecord tmRecord)
            {
                ZoomLevel = tmRecord.GetInt32(tmRecord.GetOrdinal("zoom_level"));
                MatrixWidth = tmRecord.GetInt32(tmRecord.GetOrdinal("matrix_width"));
                MatrixHeight = tmRecord.GetInt32(tmRecord.GetOrdinal("matrix_height"));
                TileWidth = tmRecord.GetInt32(tmRecord.GetOrdinal("tile_width"));
                TileHeight = tmRecord.GetInt32(tmRecord.GetOrdinal("tile_height"));
                PixelXSize = tmRecord.GetDouble(tmRecord.GetOrdinal("pixel_x_size"));
                PixelYSize = tmRecord.GetDouble(tmRecord.GetOrdinal("pixel_y_size"));
            }

            public int ZoomLevel { get; private set; }
            public int MatrixWidth { get; private set; }
            public int MatrixHeight { get; private set; }
            public int TileWidth { get; private set; }
            public int TileHeight { get; private set; }
            public double PixelXSize { get; private set; }
            public double PixelYSize { get; private set; }
        }

        private class GpkgTileMatrixSet
        {
            public GpkgTileMatrixSet(IDataRecord tmsRecord)
            {
                TableName = tmsRecord.GetString(tmsRecord.GetOrdinal("table_name"));
                SRID = tmsRecord.GetInt32(tmsRecord.GetOrdinal("srs_id"));
                MinX = tmsRecord.GetDouble(tmsRecord.GetOrdinal("minx_x"));
                MinY = tmsRecord.GetDouble(tmsRecord.GetOrdinal("minx_y"));
                MaxX = tmsRecord.GetDouble(tmsRecord.GetOrdinal("minx_x"));
                MaxY = tmsRecord.GetDouble(tmsRecord.GetOrdinal("minx_y"));
                TileMatrices = new List<GpkgTileMatrix>();
            }

            public string TableName { get; private set; }

            public int SRID { get; private set; }

            public double MinX { get; private set; }

            public double MinY { get; private set; }

            public double MaxX { get; private set; }

            public double MaxY { get; private set; }

            public List<GpkgTileMatrix> TileMatrices { get; private set; }

            public Dictionary<int, Resolution> ToResolutions()
            {
                var res = new Dictionary<int, Resolution>(TileMatrices.Count);
                foreach (var tileMatrix in TileMatrices)
                {
                    var tmp = new Resolution(
                        tileMatrix.ZoomLevel, tileMatrix.PixelXSize,
                        tileMatrix.TileWidth, tileMatrix.TileHeight,
                        MinX, MaxY,
                        tileMatrix.MatrixWidth, tileMatrix.MatrixHeight);
                    res.Add(tmp.Level, tmp);
                }
                return res;
            }

            public static GpkgTileMatrixSet Read(GpkgContent content)
            {
                GpkgTileMatrixSet tms;
                using (var cn = new SQLiteConnection(content.ConnectionString).OpenAndReturn())
                {
                    // Read the tile matrix set
                    var cmd = new SQLiteCommand(SelectTileMatrixSet, cn);
                    cmd.Parameters.AddWithValue(null, content.TableName);
                    var rdr = cmd.ExecuteReader();
                    rdr.Read();
                    tms = new GpkgTileMatrixSet(rdr);

                    // Read the tile matrices for the tile matrix set
                    cmd = new SQLiteCommand(SelectTileMatrix, cn);
                    cmd.Parameters.AddWithValue(null, content.TableName);
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        tms.TileMatrices.Add(new GpkgTileMatrix(rdr));
                    }
                }
                return tms;
            }
        }

        #endregion

    }
}
