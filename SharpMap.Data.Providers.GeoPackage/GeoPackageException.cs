using System;
using System.Runtime.Serialization;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// An exception related to 
    /// </summary>
    [Serializable]
    public class GeoPackageException : Exception
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public GeoPackageException()
        {
        }

        /// <summary>
        /// Creates an instance of this class assigning the provided <paramref name="message"/>.
        /// </summary>
        /// <param name="message">An error message</param>
        public GeoPackageException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of this class assigning the provided <paramref name="message"/> and passing an inner <paramref name="inner">exception</paramref>.
        /// </summary>
        /// <param name="message">An error message</param>
        /// <param name="inner">An inner exception</param>
        public GeoPackageException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of this class from <paramref name="info"/> and <paramref name="context"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected GeoPackageException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
