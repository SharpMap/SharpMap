using System;
using GeoAPI;

namespace SharpMap.CoordinateSystems
{
    public static class CoordinateSystemServicesProvider
    {
        private static class InstanceHolder
        {
            public volatile static Func<ICoordinateSystemServices> GetInstance;

            static InstanceHolder()
            {
                GetInstance = () => { throw new InvalidOperationException("ICoordinateSystemServices not initialized, please use CoordinateSystemServiceProvider.Instance to set an implementation"); };
            }
        }

        /// <summary>
        /// Gets or sets the ICoordinateSystemService instance.
        /// </summary>
        public static ICoordinateSystemServices Instance
        {
            get
            {
                return InstanceHolder.GetInstance();
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");


                InstanceHolder.GetInstance = () => value;
            }
        }
    }
}
