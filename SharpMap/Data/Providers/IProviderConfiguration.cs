using System;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Interface for all classes that create a provider
    /// </summary>
    public interface IProviderConfiguration
    {
        /// <summary>
        /// Create the provider provider
        /// </summary>
        /// <returns>The created provider</returns>
        IProvider Create();
    }

    /// <summary>
    /// Shapefile provider configuration class
    /// </summary>
    [Serializable]
    public class ShapeFileProviderConfiguration : IProviderConfiguration
    {
        /// <summary>
        /// Gets or sets the filename of the ShapeFile
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a spatial index should be reused
        /// </summary>
        public bool UseFilebasedIndex { get; set; }

        /// <summary>
        /// Gets or sets a value if the shapefile should be used as a <see cref="System.IO.MemoryMappedFiles.MemoryMappedFile"/>
        /// </summary>
        public bool UseMemoryCache { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how to create 
        /// </summary>
        public ShapeFile.SpatialIndexCreation SpatialIndexCreationOption { get; set; }

        /// <summary>
        /// Creates a Shapefile provider
        /// </summary>
        /// <returns></returns>
        public IProvider Create()
        {
            ShapeFile.SpatialIndexCreationOption = SpatialIndexCreationOption;
            return new ShapeFile(Filename, UseFilebasedIndex, UseMemoryCache);
        }
    }

    /// <summary>
    /// Configuration class for SqlServer 2008 providers
    /// </summary>
    [Serializable]
    public class SqlServer2008Configuration : IProviderConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string for the database
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Gets or sets the schema name
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// Gets or sets the TableName
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Gets or sets the ObjectIdColumn
        /// </summary>
        public string ObjectIdColumnName { get; set; }
        /// <summary>
        /// Gets or sets the Geometry column name
        /// </summary>
        public string GeometryColumnName { get; set; }
        /// <summary>
        /// Gets or sets the spatial object type
        /// </summary>
        public SqlServerSpatialObjectType SpatialObjectType { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether or not to use the spatial index
        /// </summary>
        public bool UseSpatialIndexForEnvelope { get; set; }

        /// <summary>
        /// Create the provider provider
        /// </summary>
        /// <returns>The created provider</returns>
        public IProvider Create()
        {
            return new SqlServer2008(ConnectionString, TableName, GeometryColumnName, ObjectIdColumnName, SpatialObjectType,
                UseSpatialIndexForEnvelope
                );
        }
    }
}