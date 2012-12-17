using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace SharpMap.Utilities
{
    public partial class Surrogates
    {

        #region Nested type: MatrixSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.Drawing2D.Matrix
        /// </summary>
        public class MatrixSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// Matrix-Stub class
            /// </summary>
            [Serializable]
            public class MatrixRef : IObjectReference, ISerializable
            {
                private readonly Matrix _matrix;

                /// <summary>
                /// Serialization constructor
                /// </summary>
                /// <param name="info">The streaming information</param>
                /// <param name="context">The streaming context</param>
                public MatrixRef(SerializationInfo info, StreamingContext context)
                {
                    var elements = (float[])info.GetValue("Elements", typeof(float[]));
                    _matrix = new Matrix(elements[0], elements[1], elements[2],
                        elements[3], elements[4], elements[5]);
                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _matrix;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                { }

            }

            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            public void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                var mat = (Matrix)obj;
                info.SetType(typeof(MatrixRef));
                info.AddValue("Elements", mat.Elements);
            }

            /// <summary>
            /// Populates the object using the information in the SerializationInfo
            /// </summary>
            /// <param name="obj">The object to populate.</param>
            /// <param name="info">The information to populate the object.</param>
            /// <param name="context">The source from which the object is deserialized.</param>
            /// <param name="selector">The surrogate selector where the search for a compatible surrogate begins.</param>
            /// <returns></returns>
            public Object SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                                        ISurrogateSelector selector)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        #endregion

        #region "Nested type: BlendSurrogate"
        /// <summary>
        /// Surrogate class for <see cref="T:System.Drawing.Drawing2D.Blend"/>
        /// </summary>
        public class BlendSurrogate : ISerializationSurrogate
        {

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var blend = (Blend)obj;
                info.AddValue("Factors", blend.Factors);
                info.AddValue("Positions", blend.Positions);
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                var blend = (Blend)obj;
                blend.Factors = (float[])info.GetValue("Factors", typeof(float[]));
                blend.Positions = (float[])info.GetValue("Positions", typeof(float[]));
                return null;
            }
        }
        #endregion

        #region "Nested type: ColorBlendSurrogate"
        /// <summary>
        /// Surrogate class for <see cref="T:System.Drawing.Drawing2D.ColorBlend"/>
        /// </summary>
        public class ColorBlendSurrogate : ISerializationSurrogate
        {

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var blend = (ColorBlend)obj;
                info.AddValue("Colors", blend.Colors);
                info.AddValue("Positions", blend.Positions);
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                var blend = (ColorBlend)obj;
                blend.Colors = (Color[])info.GetValue("Colors", typeof(Color[]));
                blend.Positions = (float[])info.GetValue("Positions", typeof(float[]));
                return null;
            }
        }
        #endregion

        #region Nested type: GraphicsPathSurrogate

        /// <summary>
        /// Serialization surrogate class for <see cref="GraphicsPath"/>
        /// </summary>
        public class GraphicsPathSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// Object reference class for <see cref="T:System.Drawing.Drawing2D.GraphicsPath"/>
            /// </summary>
            [Serializable]
            public class GraphicsPathRef : IObjectReference, ISerializable
            {
                private readonly GraphicsPath _gp;

                /// <summary>
                /// Serialization constructor
                /// </summary>
                /// <param name="info">The streaming information</param>
                /// <param name="context">The streaming context</param>
                public GraphicsPathRef(SerializationInfo info, StreamingContext context)
                {
                    var fillmode = (FillMode)info.GetInt32("FillMode");
                    var points = (PointF[])info.GetValue("PathPoints", typeof(PointF[]));
                    var types = (Byte[])info.GetValue("PathTypes", typeof(byte[]));
                    _gp = new GraphicsPath(points, types, fillmode);
                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _gp;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    throw new NotSupportedException();
                }
            }

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var gp = (GraphicsPath)obj;
                info.SetType(typeof(GraphicsPathRef));
                info.AddValue("FillMode", (int)gp.FillMode);
                info.AddValue("PathData", gp.PathPoints);
                info.AddValue("PathTypes", gp.PathTypes);
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}