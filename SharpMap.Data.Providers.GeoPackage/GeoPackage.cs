using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using SharpMap.Data.Providers.Tiles;
using SharpMap.Layers;
using SharpMap.Styles;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// A class wrapping a GeoPackage
    /// </summary>
    /// <seealso href="http://www.geopackage.org/"/>
    public class GeoPackage
    {
        /// <summary>
        /// Method to open a geopackage file
        /// </summary>
        /// <param name="filename">The filename of the GeoPackage</param>
        /// <param name="password">The password to access the GeoPackage</param>
        /// <returns></returns>
        public static GeoPackage Open(string filename, string password = null)
        {
            try
            {
                GpkgUtility.CheckRequirements(filename, password);
                return new GeoPackage(GpkgUtility.CreateConnectionString(filename, password));
            }
            catch (Exception)
            {
                throw new GeoPackageException(string.Format("Failed to open GeoPackage '{0}'", filename));
            }
        }

        private readonly string _connectionString;

        /// <summary>
        /// Creates an instance of this class 
        /// </summary>
        /// <param name="connectionString"></param>
        private GeoPackage(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Gets the <see cref="GpkgContent">Features</see> stored in the GeoPackage
        /// </summary>
        public ReadOnlyCollection<GpkgContent> Features
        {
            get
            {
                return new ReadOnlyCollection<GpkgContent>(ReadContents("features"));
            }
        }

        /// <summary>
        /// Gets the <see cref="GpkgContent">Tiles</see> stored in the GeoPackage
        /// </summary>
        public ReadOnlyCollection<GpkgContent> Tiles
        {
            get
            {
                return new ReadOnlyCollection<GpkgContent>(ReadContents("tiles"));
            }
        }

        /// <summary>
        /// Method to read the content from the 'gpkg_contents' table, filtered by <see cref="kind"/>
        /// </summary>
        /// <param name="kind">The kind of content to filter for</param>
        /// <returns>A list of content</returns>
        private IList<GpkgContent> ReadContents(string kind)
        {
            var res = new List<GpkgContent>();
            using (var cn = new SQLiteConnection(_connectionString))
            {
                cn.Open();
                var cmd = new SQLiteCommand("SELECT * FROM \"gpkg_contents\" WHERE \"data_type\"='" + kind + "';", cn);
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    res.Add(new GpkgContent(rdr, _connectionString));
            }
            return res;
        }

        /// <summary>
        /// Method to create an async tile layer
        /// </summary>
        /// <param name="name">The name of the layer</param>
        /// <returns>A tile layer</returns>
        public ILayer GetTileLayer(string name)
        {
            var content = FindContent(Tiles, name);
            if (content == null)
                throw new ArgumentException(string.Format("No tile layer named '{0}'", name));

            var schema = new GpkgTileSchema(content);
            return new TileLayer(new GpkgTileSource(content, schema), content.TableName);
        }

        /// <summary>
        /// Method to create an async tile layer
        /// </summary>
        /// <param name="name">The name of the layer</param>
        /// <returns>A tile layer</returns>
        public ITileAsyncLayer GetTileAsyncLayer(string name)
        {
            var content = FindContent(Tiles, name);
            if (content == null)
                throw new ArgumentException(string.Format("No tile layer named '{0}'", name));

            var schema = new GpkgTileSchema(content);
            return new TileAsyncLayer(new GpkgTileSource(content, schema), content.TableName);
        }

        public IProvider GetFeatureProvider(GpkgContent content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            var p = new GpkgProvider(content);
            return p;
        }

        /// <summary>
        /// Method to get a feature layer for the given content
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="createLayer">A delegate function to create the layer</param>
        /// <returns>A layer</returns>
        public ILayer GetFeatureLayer(GpkgContent content, Func<GpkgContent, IProvider, ILayer> createLayer = null)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            createLayer = createLayer ?? CreateVectorLayer;
            return createLayer(content, GetFeatureProvider(content));
        }

        #region private helper methods

        private static ILayer CreateVectorLayer(GpkgContent content, IProvider provider)
        {
            return new VectorLayer(content.TableName, provider) { Style = VectorStyle.CreateRandomStyle() };
        }

        private static GpkgContent FindContent(IEnumerable<GpkgContent> contents, string contentName)
        {
            foreach (var content in contents)
            {
                if (string.Equals(content.TableName, contentName, StringComparison.CurrentCultureIgnoreCase))
                    return content;
            }
            return null;
        }
        #endregion

    }
}