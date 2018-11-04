using System;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using System.Data.SqlClient;

namespace WinFormSamples.Samples
{
    static class SqlServerSample
    {

        public const string GeogSpatialIndex = "SIDX_CitiesSqlGeog_Geog4326";
        public const string GeomSpatialIndex = "SIDX_CitiesSqlGeom_Geom4326";

        public static SharpMap.Map InitializeMap(float angle)
        {
            //Initialize a new map of size 'imagesize'
            SharpMap.Map map = new SharpMap.Map
            {
                SRID = 3857
            };

            // Add bru-tile map background
            var cacheFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BruTileCache");
            var lyrBruTile = new TileAsyncLayer(
                BruTile.Predefined.KnownTileSources.Create(BruTile.Predefined.KnownTileSource.OpenStreetMap),
                "Tiles", Color.Transparent, true, cacheFolder)
            {
                SRID = 3857
            };

            map.BackgroundLayer.Add(lyrBruTile);
            map.BackColor = System.Drawing.Color.LightBlue;
            map.ZoomToBox(new Envelope(6943700, 19849100, -1472900, 6198900));

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }

        public  static void InitialiseSpatialIndexes(string connStr)
        {
            var table = "dbo.CitiesSqlGeom";
            var idx = GeomSpatialIndex;

            if (!DoesSpatialIndexExist(connStr, table, idx))
            {
                CreateSpatialIndex(connStr,
                    $"CREATE SPATIAL INDEX [{idx}] ON {table}(Geom4326) WITH(BOUNDING_BOX = (96, -7, 140, 40))");
            }

            table = "dbo.CitiesSqlGeog";
            idx = GeogSpatialIndex;
            if (!DoesSpatialIndexExist(connStr, table, idx))
            {
                CreateSpatialIndex(connStr, $"CREATE SPATIAL INDEX [{idx}] ON {table}(Geog4326);");
            }

        }

        private static bool DoesSpatialIndexExist(string connStr, string tableName, string indexName)
        {
            int indexId = 0;

            var sql = $"SELECT object_id FROM sys.indexes WHERE object_id = OBJECT_ID('{tableName}') AND name='{indexName}'";

            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            indexId = reader.GetInt32(0);
                        }
                    }
                }
            }
            return (indexId > 0);
        }

        private static void CreateSpatialIndex(string connStr, string sql)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
