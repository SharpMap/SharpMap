using System;
using SharpMap.Utilities;

namespace SharpMap.Data.Providers
{
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
        public virtual IProvider Create()
        {
            return Create(new NullCacheUtility());
        }

        protected IProvider Create(ICacheUtility cacheUtility)
        {
            ShapeFile.SpatialIndexCreationOption = SpatialIndexCreationOption;
            return new ShapeFile(Filename, UseFilebasedIndex, cacheUtility, UseMemoryCache);
        }
    }
}