using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace SharpMap.Utilities
{
    public partial class Surrogates
    {
        public class BrushSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            void ISerializationSurrogate.GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
            }
            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            object ISerializationSurrogate.SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                ISurrogateSelector selector)
            {
                return null;
            }
        }
        #region Nested type: SolidBrushSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.SolidBrush
        /// </summary>
        public class SolidBrushSurrogate : ISerializationSurrogate
        {
            [Serializable]
            private class SolidBrushRef : IObjectReference, ISerializable
            {
                private SolidBrush _brush;
                
                /// <summary>
                /// Serialization constructor
                /// </summary>
                /// <param name="info"></param>
                /// <param name="context"></param>
                public SolidBrushRef(SerializationInfo info, StreamingContext context)
                {
                    _brush = new SolidBrush((Color)info.GetValue("Color", typeof(Color)));
                }


                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _brush;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    throw new NotSupportedException();
                }
            }
            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            void ISerializationSurrogate.GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                info.SetType(typeof(SolidBrushRef));
                var brush = (SolidBrush)obj;
                info.AddValue("Color", brush.Color);
            }

            /// <summary>
            /// Populates the object using the information in the SerializationInfo
            /// </summary>
            /// <param name="obj">The object to populate.</param>
            /// <param name="info">The information to populate the object.</param>
            /// <param name="context">The source from which the object is deserialized.</param>
            /// <param name="selector">The surrogate selector where the search for a compatible surrogate begins.</param>
            /// <returns></returns>
            Object ISerializationSurrogate.SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                                        ISurrogateSelector selector)
            {
                //ReflectionUtility.SetFieldValue(ref obj, "color", ReflectionUtility.PrivateInstance, (Color)info.GetValue("Color", typeof(Color)));
                //brush.Color = (Color) info.GetValue("Color", typeof (Color));
                return null;
            }

            #endregion
        }

        #endregion

        #region Nested type: TextureBrushSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.TextureBrush
        /// </summary>
        public class TextureBrushSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// TextureBrush-Stub class for serialization
            /// </summary>
            [Serializable]
            public class TextureBrushRef : IObjectReference, ISerializable
            {
                private readonly TextureBrush _textureBrush;

                /// <summary>
                /// Serialization constructor
                /// </summary>
                /// <param name="info"></param>
                /// <param name="context"></param>
                public TextureBrushRef(SerializationInfo info, StreamingContext context)
                {
                    _textureBrush = new TextureBrush((Image)info.GetValue("Image", typeof(Image)));
                    _textureBrush.WrapMode = (WrapMode)info.GetInt32("WrapMode");
                    _textureBrush.Transform = (Matrix)info.GetValue("Transform", typeof(Matrix));
                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _textureBrush;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    throw new NotSupportedException();
                }
            }

            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            void ISerializationSurrogate.GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                var brush = (TextureBrush)obj;
                info.SetType(typeof(TextureBrushRef));
                info.AddValue("Image", brush.Image);
                info.AddValue("WrapMode", (int)brush.WrapMode);
                info.AddValue("Transform", brush.Transform);
            }

            /// <summary>
            /// Populates the object using the information in the SerializationInfo
            /// </summary>
            /// <param name="obj">The object to populate.</param>
            /// <param name="info">The information to populate the object.</param>
            /// <param name="context">The source from which the object is deserialized.</param>
            /// <param name="selector">The surrogate selector where the search for a compatible surrogate begins.</param>
            /// <returns></returns>
            Object ISerializationSurrogate.SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                                        ISurrogateSelector selector)
            {
                var brush = (TextureBrush)obj;
                //brush.Image = (Image) info.GetValue("Image", typeof (Image));
                ReflectionUtility.SetFieldValue(ref obj, "Image", ReflectionUtility.PrivateInstance, (Image)null);
                brush.Transform = (Matrix)info.GetValue("Transform", typeof(Matrix));
                brush.WrapMode = (WrapMode)info.GetValue("WrapMode", typeof(WrapMode));
                return null;
            }

            #endregion
        }

        #endregion

        #region Nested type: HatchBrushSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.TextureBrush
        /// </summary>
        public class HatchBrushSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// HatchBrush-Stub class for serialization
            /// </summary>
            [Serializable]
            public class HatchBrushRef : IObjectReference, ISerializable
            {
                private readonly HatchBrush _hatchBrush;

                /// <summary>
                /// Serialization constructor
                /// </summary>
                /// <param name="info"></param>
                /// <param name="context"></param>
                public HatchBrushRef(SerializationInfo info, StreamingContext context)
                {
                    _hatchBrush = new HatchBrush(
                        (HatchStyle)info.GetValue("HatchStyle", typeof(HatchStyle)),
                        (Color)info.GetValue("ForegroundColor", typeof(Color)),
                        (Color)info.GetValue("BackgroundColor", typeof(Color)));
                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _hatchBrush;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    throw new NotSupportedException();
                }
            }

            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            void ISerializationSurrogate.GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                var brush = (HatchBrush)obj;
                info.SetType(typeof(HatchBrushRef));
                info.AddValue("HatchStyle", brush.HatchStyle);
                info.AddValue("ForegroundColor", brush.ForegroundColor);
                info.AddValue("BackgroundColor", brush.BackgroundColor);
            }

            /// <summary>
            /// Populates the object using the information in the SerializationInfo
            /// </summary>
            /// <param name="obj">The object to populate.</param>
            /// <param name="info">The information to populate the object.</param>
            /// <param name="context">The source from which the object is deserialized.</param>
            /// <param name="selector">The surrogate selector where the search for a compatible surrogate begins.</param>
            /// <returns></returns>
            Object ISerializationSurrogate.SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                                        ISurrogateSelector selector)
            {
                throw new NotSupportedException();
                /*
                _hatchBrush = new HatchBrush(
                    (HatchStyle)info.GetValue("HatchStyle", typeof(HatchStyle)),
                    (Color)info.GetValue("ForegroundColor", typeof(Color)),
                    (Color)info.GetValue("BackgroundColor", typeof(Color)));

                return _hatchBrush;
                 */
                /*
                Utility.SetPropertyValue(ref obj, "HatchStyle", Utility.PrivateInstance, 
                    (HatchStyle)info.GetValue("HatchStyle", typeof(HatchStyle)));
                
                Console.WriteLine(((HatchBrush)obj).HatchStyle);
                Utility.SetFieldValue(ref obj, "ForegroundColor", Utility.PrivateInstance, 
                    );

                Utility.SetFieldValue(ref obj, "BackgroundColor", Utility.PrivateInstance, 
                    (Color)info.GetValue("BackgroundColor", typeof(Color)));
                
                return null;
                 */
            }

            #endregion
        }

        #endregion

        #region Nested type: LinearGradientBrushSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.TextureBrush
        /// </summary>
        public class LinearGradientBrushSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// TextureBrush-Stub class for serialization
            /// </summary>
            [Serializable]
            public class LinearGradientBrushRef : IObjectReference, ISerializable
            {
                private readonly LinearGradientBrush _lgBrush;

                /// <summary>
                /// Serialization constructor
                /// </summary>
                /// <param name="info"></param>
                /// <param name="context"></param>
                public LinearGradientBrushRef(SerializationInfo info, StreamingContext context)
                {
                    var rectangle = (RectangleF)info.GetValue("Rectangle", typeof(RectangleF));
                    var linearColors = (Color[])info.GetValue("LinearColors", typeof(Color[]));
                    _lgBrush = new LinearGradientBrush(rectangle, linearColors[0], linearColors[linearColors.Length - 1], 0f);
                    var blend = (Blend)(info.GetValue("Blend", typeof(Blend)));
                    if (blend == null)
                    {
                        _lgBrush.InterpolationColors = (ColorBlend)info.GetValue("InterpolationColors", typeof(ColorBlend));
                    }
                    else
                    {
                        _lgBrush.Blend = blend;
                    }
                    _lgBrush.GammaCorrection = info.GetBoolean("GammaCorrection");
                    _lgBrush.WrapMode = (WrapMode)info.GetInt32("WrapMode");
                    _lgBrush.Transform = (Matrix)info.GetValue("Transform", typeof(Matrix));

                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _lgBrush;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    throw new NotSupportedException();
                }
            }

            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            void ISerializationSurrogate.GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                var brush = (LinearGradientBrush)obj;
                info.SetType(typeof(LinearGradientBrushRef));
                info.AddValue("Rectangle", brush.Rectangle);

                info.AddValue("Blend", brush.Blend);
                if (brush.Blend == null)
                    info.AddValue("InterpolationColors", brush.InterpolationColors);
                info.AddValue("GammaCorrection", brush.GammaCorrection);
                info.AddValue("LinearColors", brush.LinearColors);
                info.AddValue("WrapMode", (int)brush.WrapMode);
                info.AddValue("Transform", brush.Transform);
            }

            /// <summary>
            /// Populates the object using the information in the SerializationInfo
            /// </summary>
            /// <param name="obj">The object to populate.</param>
            /// <param name="info">The information to populate the object.</param>
            /// <param name="context">The source from which the object is deserialized.</param>
            /// <param name="selector">The surrogate selector where the search for a compatible surrogate begins.</param>
            /// <returns></returns>
            Object ISerializationSurrogate.SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                                        ISurrogateSelector selector)
            {
                var brush = (LinearGradientBrush)obj;
                brush.Blend = (Blend)info.GetValue("Blend", typeof(Blend));
                brush.InterpolationColors = (ColorBlend)info.GetValue("ColorBlend", typeof(ColorBlend));
                brush.GammaCorrection = info.GetBoolean("GammaCorrection");
                brush.LinearColors = (Color[])info.GetValue("LinearColors", typeof(Color[]));
                //brush.Rectangle = (RectangleF) info.GetValue("Rectangle", typeof (RectangleF));
                brush.Transform = (Matrix)info.GetValue("Transform", typeof(Matrix));
                brush.WrapMode = (WrapMode)info.GetValue("WrapMode", typeof(WrapMode));
                return null;
            }

            #endregion
        }

        #endregion
    }
}