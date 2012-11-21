using System;

namespace SharpMap.Base
{
    
    /// <summary>
    /// 
    /// </summary>
    public interface IDisposableEx : IDisposable
    {
        /// <summary>
        /// Gets whether this object was already disposed
        /// </summary>
        bool IsDisposed { get; }
    }

    /// <summary>
    /// Disposable object template
    /// </summary>
    /// <remarks>
    /// This template was taken from phil haack's blog (
    /// <see href="http://haacked.com/archive/2005/11/18/ACloserLookAtDisposePattern.aspx"/>) and further enhanced
    /// </remarks>
    [Serializable]
    public abstract class DisposableObject : IDisposableEx
    {
        #region Implementation of IDisposable

        /// <summary>
        /// Finalizer
        /// </summary>
        ~DisposableObject()
        {
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Executes specific tasks that are concerned with freeing or initializing resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
                ReleaseManagedResources();

        }

        /// <summary>
        /// Releases unmanaged resources
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {}

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {}
        
        #endregion

        /// <summary>
        /// Method to check if this object has already been disposed
        /// </summary>
        protected void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        [NonSerialized]
        private bool _isDisposed;

        /// <summary>
        /// Gets whether this object is disposed
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
            private set { _isDisposed = value; }
        }
    }
}