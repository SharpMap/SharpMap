using System;

namespace SharpMap.Data.Providers
{
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