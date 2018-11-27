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
        public const string GeogTable = "dbo.CitiesSqlGeog";
        public const string GeogSpatialIndex = "SIDX_CitiesSqlGeog_Geog4326";

        public const string GeomTable = "dbo.CitiesSqlGeom";
        public const string GeomSpatialIndex = "SIDX_CitiesSqlGeom_Geom4326";

        public static SharpMap.Map InitializeMap(float angle)
        {
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
            // SE Asia
            map.ZoomToBox(new Envelope(6943700, 19849100, -1472900, 6198900));

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }

        public static void InitialiseTables(string connStr)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // GEOMETRY table, spatial index, and data
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT OBJECT_ID (N'{GeomTable}', N'U')";
                    if (cmd.ExecuteScalar() == DBNull.Value)
                    {
                        cmd.CommandText = $"CREATE TABLE {GeomTable} (ID int PRIMARY KEY, NAME nvarchar(50), Geom4326 geometry)";
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT object_id FROM sys.indexes WHERE object_id = OBJECT_ID('{GeomTable}') AND name='{GeomSpatialIndex}'";
                    if (cmd.ExecuteScalar() == null)
                    {
                        cmd.CommandText = $"CREATE SPATIAL INDEX [{GeomSpatialIndex}] ON {GeomTable}(Geom4326) WITH(BOUNDING_BOX = (96, -7, 140, 40))";
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT COUNT(ID) FROM {GeomTable}";
                    if ((int)cmd.ExecuteScalar() == 0)
                    {
                        cmd.CommandText = "INSERT INTO [dbo].[CitiesSqlGeom] ([Id], [Name], [Geom4326]) VALUES " +
                            "(1, N'Bandar Seri Bagawan', geometry::Point(114.942222, 4.890278, 4326))," +
                            "(2, N'Bangkok', geometry::Point(100.501765, 13.756331, 4326))," +
                            "(3, N'Beijing', geometry::Point(116.407395, 39.904211, 4326))," +
                            "(4, N'Hanoi', geometry::Point(105.83416, 21.027764, 4326))," +
                            "(5, N'Hong Kong', geometry::Point(114.109497, 22.396428, 4326))," +
                            "(6, N'Jakarta', geometry::Point(106.845599, -6.208763, 4326))," +
                            "(7, N'Kuala Lumpur', geometry::Point(101.686855, 3.139003, 4326))," +
                            "(8, N'Manila', geometry::Point(120.984219, 14.599512, 4326))," +
                            "(9, N'Phnom Penh', geometry::Point(104.892167, 11.544873, 4326))," +
                            "(10, N'Pyong Yang', geometry::Point(125.762524, 39.039219, 4326))," +
                            "(11, N'Seoul', geometry::Point(126.977969, 37.566535, 4326))," +
                            "(12, N'Singapore', geometry::Point(103.819836, 1.352083, 4326))," +
                            "(13, N'Taipei', geometry::Point(121.565418, 25.032969, 4326))," +
                            "(14, N'Tokoyo', geometry::Point(139.691706, 35.689487, 4326))," +
                            "(15, N'Vientienne', geometry::Point(102.633104, 17.975706, 4326))," +
                            "(16, N'Yangoon', geometry::Point(96.195132, 16.866069, 4326))";
                        cmd.ExecuteNonQuery();
                    }
                }

                // GEOGRAPHY table, spatial index, and data
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT OBJECT_ID (N'{GeogTable}', N'U')";
                    if (cmd.ExecuteScalar() == DBNull.Value)
                    {
                        cmd.CommandText = $"CREATE TABLE {GeogTable} (ID int PRIMARY KEY, NAME nvarchar(50), Geog4326 geography)";
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT object_id FROM sys.indexes WHERE object_id = OBJECT_ID('{GeogTable}') AND name='{GeogSpatialIndex}'";
                    if (cmd.ExecuteScalar() == null)
                    {
                        cmd.CommandText = $"CREATE SPATIAL INDEX [{GeogSpatialIndex}] ON {GeogTable}(Geog4326);";
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT COUNT(ID) FROM {GeogTable}";
                    if ((int)cmd.ExecuteScalar() == 0)
                    {
                        cmd.CommandText = "INSERT INTO [dbo].[CitiesSqlGeog] ([Id], [Name], [Geog4326]) VALUES " +
                            "(1, N'Bandar Seri Bagawan', geography::Point(4.890278, 114.942222, 4326))," +
                            "(2, N'Bangkok', geography::Point(13.756331, 100.501765, 4326))," +
                            "(3, N'Beijing', geography::Point(39.904211, 116.407395, 4326))," +
                            "(4, N'Hanoi', geography::Point(21.027764, 105.83416, 4326))," +
                            "(5, N'Hong Kong', geography::Point(22.396428, 114.109497, 4326))," +
                            "(6, N'Jakarta', geography::Point(-6.208763, 106.845599, 4326))," +
                            "(7, N'Kuala Lumpur', geography::Point(3.139003, 101.686855, 4326))," +
                            "(8, N'Manila', geography::Point(14.599512, 120.984219, 4326))," +
                            "(9, N'Phnom Penh', geography::Point(11.544873, 104.892167, 4326))," +
                            "(10, N'Pyong Yang', geography::Point(39.039219, 125.762524, 4326))," +
                            "(11, N'Seoul', geography::Point(37.566535, 126.977969, 4326))," +
                            "(12, N'Singapore', geography::Point(1.352083, 103.819836, 4326))," +
                            "(13, N'Taipei', geography::Point(25.032969, 121.565418, 4326))," +
                            "(14, N'Tokoyo', geography::Point(35.689487, 139.691706, 4326))," +
                            "(15, N'Vientienne', geography::Point(17.975706, 102.633104, 4326))," +
                            "(16, N'Yangoon', geography::Point(16.866069, 96.195132, 4326));";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
