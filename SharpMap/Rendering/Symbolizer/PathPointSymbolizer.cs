// Copyright 2011 - Felix Obermaier (www.ivv-aachen.de)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SharpMap.Rendering.Symbolizer
{
    ///<summary>
    /// Symbolizer class that renders custom symbols made up of custom paths
    ///</summary>
    [Serializable]
    public class PathPointSymbolizer : PointSymbolizer
    {
        
        ///<summary>
        /// Creates a <see cref="PathPointSymbolizer"/> that renders circles.
        ///</summary>
        ///<param name="line">The pen to outline the circle</param>
        ///<param name="fill">the brush to fill the circle</param>
        ///<param name="size">The size of the circle</param>
        ///<returns>The PathPointSymbolizer object</returns>
        public static PathPointSymbolizer CreateCircle(Pen line, Brush fill, float size)
        {
            return CreateEllipse(line, fill, size, size);
        }


        ///<summary>
        /// Creates a <see cref="PathPointSymbolizer"/> that renders ellipses.
        ///</summary>
        ///<param name="line">The pen to outline the ellipse</param>
        ///<param name="fill">the brush to fill the ellipse</param>
        ///<param name="a">The x-axis radius of the ellipse</param>
        ///<param name="b">The x-axis radius of the ellipse</param>
        ///<returns>The PathPointSymbolizer object</returns>
        public static PathPointSymbolizer CreateEllipse(Pen line, Brush fill, float a, float b)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, a, b);
            return new PathPointSymbolizer(
                new[] { new PathDefinition { Line = line, Fill = fill, Path = path } });
        }

        ///<summary>
        /// Creates a <see cref="PathPointSymbolizer"/> that renders rectangles.
        ///</summary>
        ///<param name="line">The pen to outline the rectangle</param>
        ///<param name="fill">the brush to fill the rectangle</param>
        ///<param name="width">The width of the rectangle</param>
        ///<param name="height">The height of the rectangle</param>
        ///<returns>The PathPointSymbolizer object</returns>
        public static PathPointSymbolizer CreateRectangle(Pen line, Brush fill, float width, float height)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new RectangleF(-0.5f * width, -0.5f * height, width, height));
            return new PathPointSymbolizer(
                new[] { new PathDefinition { Line = line, Fill = fill, Path = path } });
        }

        ///<summary>
        /// Creates a <see cref="PathPointSymbolizer"/> that renders squares.
        ///</summary>
        ///<param name="line">The pen to outline the square</param>
        ///<param name="fill">the brush to fill the square</param>
        ///<param name="size">The size of the square</param>
        ///<returns>The PathPointSymbolizer object</returns>
        public static PathPointSymbolizer CreateSquare(Pen line, Brush fill, float size)
        {
            
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new RectangleF(-0.5f * size, -0.5f * size, size, size));
            return new PathPointSymbolizer(
                new[] { new PathDefinition { Line = line, Fill = fill, Path = path } });
        }

        ///<summary>
        /// Creates a <see cref="PathPointSymbolizer"/> that renders bottom-down-triangless.
        ///</summary>
        ///<param name="line">The pen to outline the triangle</param>
        ///<param name="fill">the brush to fill the triangle</param>
        ///<param name="size">The size of the triangle</param>
        ///<returns>The PathPointSymbolizer object</returns>
        public static PathPointSymbolizer CreateTriangle(Pen line, Brush fill, float size)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(new[]
                                {
                                    new PointF(-0.5f*size, size/3f), new PointF(0, 2f*size/3f),
                                    new PointF(0.5f*size, size/3f), new PointF(-0.5f*size, size/3f),
                                }
                );
            return new PathPointSymbolizer(
                new[] { new PathDefinition { Line = line, Fill = fill, Path = path } });
        }

        /// <summary>
        /// Path definition class
        /// </summary>
        [Serializable]
        public class PathDefinition : IDisposable, ICloneable
        {
            /// <summary>
            /// Gets or sets the <see cref="Pen"/> to draw the path
            /// </summary>
            public Pen Line { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Brush"/> to fill the path
            /// </summary>
            public Brush Fill { get; set; }

            /// <summary>
            /// The <see cref="GraphicsPath"/> to be drawn and/or filled
            /// </summary>
            public GraphicsPath Path { get; set; }

            #region Implementation of IDisposable

            /// <summary>
            /// Method to perform cleanup methods for unmanaged resources
            /// </summary>
            public void Dispose()
            {
                if (Fill != null) Fill.Dispose();
                if (Line != null) Line.Dispose();
                if (Path != null) Path.Dispose();
            }

            #endregion

            #region Implementation of ICloneable

            /// <summary>
            /// Creates a new object that is a copy of the current instance.
            /// </summary>
            /// <returns>
            /// A new object that is a copy of this instance.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public object Clone()
            {
                return new PathDefinition
                           {
                               Fill = (Brush) (Fill != null ? Fill.Clone() : null),
                               Line = (Pen) (Line != null ? Line.Clone() : null),
                               Path = (GraphicsPath) (Path != null ? Path.Clone() : null),
                           };
            }

            #endregion
        }


        private readonly PathDefinition[] _paths;

        
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="paths"></param>
        public PathPointSymbolizer(PathDefinition[] paths)
        {
            _paths = paths;
        }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (_paths != null)
            {
                foreach (var t in _paths)
                {
                    if (t != null) t.Dispose();
                }
            }
            base.ReleaseManagedResources();
        }

        /// <inheritdoc/>
        public override object Clone()
        {
            var pathDefinitions = new PathDefinition[_paths.Length];
            for (var i = 0; i < _paths.Length; i++)
                pathDefinitions[i] = (PathDefinition) _paths[i].Clone();
            
            return new PathPointSymbolizer(pathDefinitions);
        }

        /// <summary>
        /// Gets or sets the Size of the symbol
        /// <para>
        /// Implementations may ignore the setter, the getter must return a <see cref="PointSymbolizer.Size"/> with positive width and height values.
        /// </para>
        /// </summary>
        public override Size Size
        {
            get
            {
                var size = new Size();
                foreach (PathDefinition pathDefinition in _paths)
                {
                    var bounds = pathDefinition.Path.GetBounds();
                    size = new Size(Math.Max(size.Width, (int)bounds.Width),
                                    Math.Max(size.Height, (int)bounds.Height));
                }
                return size;
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Function that does the actual rendering
        /// </summary>
        /// <param name="pt">The point</param>
        /// <param name="g">The graphics object</param>
        internal override void OnRenderInternal(PointF pt, Graphics g)
        {
            var f = new SizeF(pt);
            foreach (var pathDefinition in _paths)
            {
                var ppts = pathDefinition.Path.PathPoints;
                var pptsnew = new PointF[pathDefinition.Path.PointCount];
                for (int i = 0; i < pptsnew.Length; i++)
                    pptsnew[i] = PointF.Add(ppts[i], f);

                GraphicsPath ptmp = new GraphicsPath(pptsnew, pathDefinition.Path.PathTypes, pathDefinition.Path.FillMode);
                if (pathDefinition.Fill != null)
                    g.FillPath(pathDefinition.Fill, ptmp);
                if (pathDefinition.Line != null)
                    g.DrawPath(pathDefinition.Line, ptmp);

            }
        }
    }
}