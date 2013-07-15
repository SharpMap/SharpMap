using SharpMap.Utilities;

namespace SharpMap.Data.Providers
{
    public class WebShapeFileProviderConfiguration : ShapeFileProviderConfiguration
    {
        public override IProvider Create()
        {
            return Create(new WebCacheUtility());
        }
    }
}
