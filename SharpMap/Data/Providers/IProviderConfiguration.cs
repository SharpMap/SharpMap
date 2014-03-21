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
}