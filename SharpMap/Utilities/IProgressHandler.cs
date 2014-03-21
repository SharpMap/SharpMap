using System;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Interface for all progress handling classes
    /// </summary>
    public interface IProgressHandler
    {
         
    }

    /// <summary>
    /// A no-operation progress handler
    /// </summary>
    public class NoopProgressHandler : IProgressHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public static Lazy<NoopProgressHandler> Instance { get { return new Lazy<NoopProgressHandler>(); } }
    }

}