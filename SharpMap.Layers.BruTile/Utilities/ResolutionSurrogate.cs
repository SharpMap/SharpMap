// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using BruTile;

namespace SharpMap.Utilities
{
    internal class ResolutionSurrogate : ISerializationSurrogate
    {
        [System.Serializable]
        internal class ResolutionRef : IObjectReference, ISerializable
        {
            private readonly Resolution _resolution;
            /// <summary>
            /// Serialization constructor
            /// </summary>
            /// <param name="info"></param>
            /// <param name="context"></param>
            public ResolutionRef(SerializationInfo info, StreamingContext context)
            {
                _resolution = new Resolution(
                    info.GetString("id"), info.GetDouble("upp"),
                    info.GetInt32("th"), info.GetInt32("tw"),
                    info.GetDouble("t"), info.GetDouble("l"),
                    info.GetInt32("mw"), info.GetInt32("mh"),
                    info.GetDouble("sd"));
            }


            object IObjectReference.GetRealObject(StreamingContext context)
            {
                return _resolution;
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new System.NotSupportedException();
            }
        }

        

        #region Implementation of ISerializationSurrogate

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var res = (Resolution)obj;
            info.SetType(typeof(ResolutionRef));
            info.AddValue("id", res.Id);
            info.AddValue("upp", res.UnitsPerPixel);
            info.AddValue("th", res.TileHeight);
            info.AddValue("tw", res.TileWidth);
            info.AddValue("t", res.Top);
            info.AddValue("l", res.Left);
            info.AddValue("mw", res.MatrixWidth);
            info.AddValue("mh", res.MatrixHeight);
            info.AddValue("sd", res.ScaleDenominator);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        #endregion
    }
}
