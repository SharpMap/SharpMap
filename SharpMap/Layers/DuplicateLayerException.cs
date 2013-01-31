using System;
using System.Runtime.Serialization;

namespace SharpMap.Layers
{
    /// <summary>
    /// Exception thrown when a layer with a name which is the same
    /// as an existing layer is added to a <see cref="LayerCollection"/>.
    /// </summary>
    [Serializable]
    public class DuplicateLayerException : InvalidOperationException
    {
        private readonly string _duplicateLayerName;

        /// <summary>
        /// Creates a new instance of DuplicateLayerException, indicating
        /// the duplicate layer name by <paramref name="duplicateLayerName"/>.
        /// </summary>
        /// <param name="duplicateLayerName">
        /// The existing layer name which was duplicated.
        /// </param>
        public DuplicateLayerException(string duplicateLayerName)
            : this(duplicateLayerName, null)
        {
        }

        /// <summary>
        /// Creates a new instance of DuplicateLayerException, indicating
        /// the duplicate layer name by <paramref name="duplicateLayerName"/>
        /// and including a message.
        /// </summary>
        /// <param name="duplicateLayerName">
        /// The existing layer name which was duplicated.
        /// </param>
        /// <param name="message">Additional information about the exception.</param>
        public DuplicateLayerException(string duplicateLayerName, string message)
            : this(duplicateLayerName, message, null)
        {
        }

        /// <summary>
        /// Creates a new instance of DuplicateLayerException, indicating
        /// the duplicate layer name by <paramref name="duplicateLayerName"/>
        /// and including a message.
        /// </summary>
        /// <param name="duplicateLayerName">
        /// The existing layer name which was duplicated.
        /// </param>
        /// <param name="message">
        /// Additional information about the exception.
        /// </param>
        /// <param name="inner">
        /// An exception which caused this exception, if any.
        /// </param>
        public DuplicateLayerException(string duplicateLayerName, string message, Exception inner)
            : base(message, inner)
        {
            _duplicateLayerName = duplicateLayerName;
        }

        /// <summary>
        /// Creates a new instance of DuplicateLayerException from serialized data,
        /// <paramref name="info"/>.
        /// </summary>
        /// <param name="info">The serialization data.</param>
        /// <param name="context">Serialization context.</param>
        protected DuplicateLayerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _duplicateLayerName = info.GetString("_duplicateLayerName");
        }

        /// <summary>
        /// Gets the existing layer name which was duplicated.
        /// </summary>
        public string DuplicateLayerName
        {
            get { return _duplicateLayerName; }
        }

        /// <summary>
        /// Serialization function
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_duplicateLayerName", _duplicateLayerName);
        }
    }
}