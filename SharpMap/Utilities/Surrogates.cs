// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Helper class for serializing System.Drawing.Pen and System.Drawing.Brush
    /// </summary>
    public class Surrogates
    {
        /// <summary>
        /// Gets the surrogate selecteds for System.Drawing.Pen and System.Drawing.Brush
        /// </summary>
        /// <returns>SurrogateSelector</returns>
        public static SurrogateSelector GetSurrogateSelectors()
        {
            SurrogateSelector ss = new SurrogateSelector();
            ss.AddSurrogate(typeof (Pen), new StreamingContext(StreamingContextStates.All), new PenSurrogate());
            ss.AddSurrogate(typeof (SolidBrush), new StreamingContext(StreamingContextStates.All),
                            new SolidBrushSurrogate());
            ss.AddSurrogate(typeof (TextureBrush), new StreamingContext(StreamingContextStates.All),
                            new TextureBrushSurrogate());
            ss.AddSurrogate(typeof (Matrix), new StreamingContext(StreamingContextStates.All), new MatrixSurrogate());
            return ss;
        }

        #region Nested type: MatrixSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.Drawing2D.Matrix
        /// </summary>
        public class MatrixSurrogate : ISerializationSurrogate
        {
            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            public void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                Matrix mat = (Matrix) obj;
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
                float[] elements = (float[]) info.GetValue("Elements", typeof (float[]));
                Matrix mat = new Matrix(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
                return null;
            }

            #endregion
        }

        #endregion

        #region Nested type: PenSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.Pen
        /// </summary>
        public class PenSurrogate : ISerializationSurrogate
        {
            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            public void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                Pen pen = (Pen) obj;
                if (pen.Color != Color.Empty)
                {
                    info.AddValue("Color", pen.Color);


                    info.AddValue("Width", pen.Width);
                    info.AddValue("Alignment", pen.Alignment);
                    //info.AddValue("Brush", pen.Brush);
                    info.AddValue("CompoundArray", pen.CompoundArray);
                    //Todo: 
                    //info.AddValue("CustomEndCap", pen.CustomEndCap);
                    //info.AddValue("CustomStartCap", pen.CustomStartCap);
                    //pen.DashCap;
                    //pen.DashOffset;
                    info.AddValue("DashPattern", pen.DashPattern);
                    //pen.DashStyle;
                    //pen.EndCap;
                    //pen.LineJoin;
                    //pen.MiterLimit;
                    //pen.PenType;
                    //pen.StartCap;
                    info.AddValue("Transform", pen.Transform);
                }
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
                Pen pen = new Pen((Color) info.GetValue("Color", typeof (Color)));
                pen.Width = (float) info.GetValue("Width", typeof (float));
                pen.Alignment = (PenAlignment) info.GetValue("Alignment", typeof (PenAlignment));
                //pen.Brush = (Brush)info.GetValue("Brush", typeof(Brush));
                try
                {
                    pen.CompoundArray = (float[]) info.GetValue("CompoundArray", typeof (float[]));
                }
                catch
                {
                }
                //pen.CustomEndCap = (CustomLineCap)info.GetValue("CustomEndCap", typeof(CustomLineCap));
                //pen.CustomStartCap = (CustomLineCap)info.GetValue("CustomStartCap", typeof(CustomLineCap));
                pen.DashPattern = (float[]) info.GetValue("DashPattern", typeof (float[]));
                try
                {
                    pen.Transform = (Matrix) info.GetValue("Transform", typeof (Matrix));
                }
                catch
                {
                }
                return null;
            }

            #endregion
        }

        #endregion

        #region Nested type: SolidBrushSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.SolidBrush
        /// </summary>
        public class SolidBrushSurrogate : ISerializationSurrogate
        {
            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            public void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                SolidBrush brush = (SolidBrush) obj;
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
            public Object SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                                        ISurrogateSelector selector)
            {
                SolidBrush brush = new SolidBrush((Color) info.GetValue("Color", typeof (Color)));
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
            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            public void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
                TextureBrush brush = (TextureBrush) obj;
                info.AddValue("Image", brush.Image);
                info.AddValue("Transform", brush.Transform);
                info.AddValue("WrapMode", brush.WrapMode);
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
                TextureBrush brush = new TextureBrush((Image) info.GetValue("Image", typeof (Image)));
                brush.Transform = (Matrix) info.GetValue("Transform", typeof (Matrix));
                brush.WrapMode = (WrapMode) info.GetValue("WrapMode", typeof (WrapMode));
                return null;
            }

            #endregion
        }

        #endregion
    }
}