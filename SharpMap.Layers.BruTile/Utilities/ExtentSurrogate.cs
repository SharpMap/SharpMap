// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.Serialization;
using BruTile;

namespace SharpMap.Utilities
{
    internal class ExtentSurrogate : ISerializationSurrogate
    {
        [System.Serializable]
        internal class ExtentRef : IObjectReference, ISerializable
        {
            private readonly Extent _extent;

            public ExtentRef(SerializationInfo info, StreamingContext context)
            {
                _extent = new Extent(
                    info.GetDouble("minX"), info.GetDouble("minY"),
                    info.GetDouble("maxX"), info.GetDouble("maxY"));
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                return _extent;
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new System.NotSupportedException();
            }
        }

        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ex = (Extent)obj;
            info.SetType(typeof(ExtentRef));
            info.AddValue("minX", ex.MinX);
            info.AddValue("minY", ex.MinY);
            info.AddValue("maxX", ex.MaxX);
            info.AddValue("maxY", ex.MaxY);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        #endregion
    }
}
