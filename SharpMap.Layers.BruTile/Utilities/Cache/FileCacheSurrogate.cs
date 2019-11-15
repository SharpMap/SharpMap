using System;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile.Cache;

namespace SharpMap.Utilities.Cache
{
    [Serializable]
    internal class FileCacheSurrogate : ISerializationSurrogate
    {
        [Serializable]
        internal class FileCacheRef : IObjectReference, ISerializable
        {
            private readonly FileCache _fileCache;

            public FileCacheRef(SerializationInfo info, StreamingContext context)
            {
                _fileCache = new FileCache(info.GetString("dir"), info.GetString("fmt"),
                    (TimeSpan)info.GetValue("exp", typeof(TimeSpan)));
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotSupportedException();
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                return _fileCache;
            }
        }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var fileCache = (FileCache) obj;
            info.SetType(typeof(FileCacheRef));
            info.AddValue("dir", Utility.GetFieldValue<string>(fileCache, "_directory"));
            info.AddValue("fmt", Utility.GetFieldValue<string>(fileCache, "_format"));
            info.AddValue("exp", Utility.GetFieldValue<TimeSpan>(fileCache, "_cacheExpireTime"));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }
    }
}
