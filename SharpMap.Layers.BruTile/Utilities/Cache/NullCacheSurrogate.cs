// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace SharpMap.Utilities.Cache
{
    internal class NullCacheSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            // Nothing to do
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            // Again, nothing to do
            return obj;
        }

        #endregion
    }
}