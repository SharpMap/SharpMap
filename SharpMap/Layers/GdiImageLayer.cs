// Copyright 2013 - 2014 Robert Smart, CNL Software (www.cnlsoftware.com)
// Copyright 2014 -      SharpMap-Team
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using RTools_NTS.Util;
using Point = System.Drawing.Point;

namespace SharpMap.Layers
{

    /// <summary>
    /// Image to 
    /// </summary>
    [Serializable]
    public class GdiImageLayer : Layer
    {
        private Image _image;

        [NonSerialized] 
        private Image _tmpImage;

        [NonSerialized]
        private WorldFile _worldFile;
        
        [NonSerialized]
        private Envelope _envelope;

        private float _transparency;

        /// <summary>
        /// Creates an instance of this class using the provided layer
        /// </summary>
        /// <param name="fileName">The path to the file</param>
        public GdiImageLayer(string fileName)
            : this(Path.GetFileName(fileName), fileName)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided layer
        /// </summary>
        /// <param name="layerName">The name of the layer</param>
        /// <param name="fileName">The path to the file</param>
        public GdiImageLayer(string layerName, string fileName)
            :this(layerName, Image.FromFile(fileName))
        {
            ImageFilename = fileName;
            SetEnvelope(fileName);
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="layerName"/> and <paramref name="image"/>.
        /// </summary>
        /// <param name="layerName">The layer name</param>
        /// <param name="image"></param>
        public GdiImageLayer(string layerName, Image image)
        {
            InterpolationMode = InterpolationMode.HighQualityBicubic;

            LayerName = layerName;
            _image = image;
            SetEnvelope();
        }

        /// <summary>
        /// Method to set the <see cref="Envelope"/>
        /// </summary>
        private void SetEnvelope()
        {
            _worldFile = _worldFile ?? new WorldFile(1, 0, 0, -1, 0, _image.Height);
            _envelope = _worldFile.ToGroundBounds(_image.Width, _image.Height).EnvelopeInternal;
        }

        /// <summary>
        /// Gets or sets the filename for the image
        /// </summary>
        public string ImageFilename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the image to display
        /// </summary>
        public Image Image
        {
            get { return _image; }
            set
            {
                _image = value;
                _envelope = new Envelope(0, _image.Width, 0, _image.Height);
            }
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override Envelope Envelope
        {
            get
            {
                // Avoid manipulation, return a copy
                return new Envelope(_envelope);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the transparency level
        /// </summary>
        public float Transparency
        {
            get { return _transparency; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("value", "Must be in the range [0, 1]");

                _transparency = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Drawing.Drawing2D.InterpolationMode"/> to use
        /// </summary>
        public InterpolationMode InterpolationMode { get; set; }

        protected override void ReleaseManagedResources()
        {
            base.ReleaseManagedResources();
            _image.Dispose();
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, MapViewport map)
        {
            if (map.Center == null)
                throw (new ApplicationException("Cannot render map. View center not specified"));

            if (_image == null)
                throw new Exception("Image not set");

        
            // View to render
            var mapView = map.Envelope;
            
            // Layer view
            var lyrView = _envelope;

            // Get the view intersection
            var vi = mapView.Intersection(lyrView);
            if (!vi.IsNull)
            {
                // Image part
// ReSharper disable InconsistentNaming
                var imgLT = Clip(_worldFile.ToRaster(new Coordinate(vi.MinX, vi.MaxY)));
                var imgRB = Clip(_worldFile.ToRaster(new Coordinate(vi.MaxX, vi.MinY)));
                var imgRect = new Rectangle(imgLT, PointDiff(imgLT, imgRB, 1));

                // Map Part
                var mapLT = Point.Truncate(map.WorldToImage(new Coordinate(vi.MinX, vi.MaxY)));
                var mapRB = Point.Ceiling(map.WorldToImage(new Coordinate(vi.MaxX, vi.MinY)));
                var mapRect = new Rectangle(mapLT, PointDiff(mapLT, mapRB, 1));
// ReSharper restore InconsistentNaming

                // Set the interpolation mode
                var tmpInterpolationMode = g.InterpolationMode;
                g.InterpolationMode = InterpolationMode;

                // Render the image
                using (var ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(new ColorMatrix { Matrix44 = 1 - Transparency },
                        ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    g.DrawImage(_image, mapRect, imgRect.X, imgRect.Y, imgRect.Width, imgRect.Height,
                        GraphicsUnit.Pixel, ia);
                }

                // reset the interpolation mode
                g.InterpolationMode = tmpInterpolationMode;

            }

            // Obsolete (and will cause infinite loop) 
            //base.Render(g, map);
        }

        /// <summary>
        /// Clip to the extent of the image
        /// </summary>
        /// <param name="pt">The point</param>
        /// <returns>The clipped point</returns>
        private Point Clip(Point pt)
        {
            var x = pt.X;
            if (x < 0) x = 0;
            if (x > _image.Width) x = Image.Width;

            var y = pt.Y;
            if (y < 0) y = 0;
            if (y > _image.Height) y = Image.Height;

            return new Point(x, y);
        }

        private static Size PointDiff(Point p1, Point p2, int invertY = -1)
        {
            
            return new Size(p2.X - p1.X, invertY * (p2.Y - p1.Y));
        }

        /// <summary>
        /// Set the envelope by the definition in the world file
        /// </summary>
        /// <param name="fileName">Filename to the image</param>
        private void SetEnvelope(string fileName)
        {
            var wld = Path.ChangeExtension(fileName, ".wld");

            if (File.Exists(wld))
            {
                _worldFile = ReadWorldFile(wld);
            }
            else
            {
                var ext = CreateWorldFileExtension(Path.GetExtension(fileName));
                if (string.IsNullOrEmpty(ext)) return;
                {
                    wld = Path.ChangeExtension(fileName, ext);
                    if (File.Exists(wld))
                    {
                        _worldFile = ReadWorldFile(wld);
                    }
                }
            }

            SetEnvelope();
        }

        /// <summary>
        /// Function to read a <see cref="WorldFile"/> for the <see cref="Image"/> and the provided <paramref name="wld">world file </paramref>
        /// </summary>
        /// <param name="wld">The world file</param>
        /// <returns>The <see cref="WorldFile"/></returns>
        private static WorldFile ReadWorldFile(string wld)
        {
            var coefficients = new double[6];

            try
            {
                using (var sr = new StreamReader(wld))
                {
                    for (var i = 0; i < coefficients.Length; i++)
                    {
                        var line = sr.ReadLine();
                        while (string.IsNullOrEmpty(line))
                            line = sr.ReadLine();
                        coefficients[i] = double.Parse(line, NumberFormatInfo.InvariantInfo);
                    }
                }

                // Test for rotation or shear
                if (coefficients[1] != 0d || coefficients[2] != 0d)
                {
                    throw new NotSupportedException("World files with rotation and/or shear are not supported");
                }

                return new WorldFile(coefficients[0], coefficients[1],
                                     coefficients[2], coefficients[3],
                                     coefficients[4], coefficients[5]);
            }
            catch (Exception)
            {
                return new WorldFile();
            }
        }

        private static string CreateWorldFileExtension(string ext)
        {
            if (!ext.StartsWith(".")) ext = "." + ext;
            if (ext.Length != 4) return null;

            var caExt = ext.ToCharArray();
            caExt[2] = caExt[3];
            caExt[3] = 'w';
            return new string(caExt);
        }

        #region Serialization

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            _tmpImage = _image;
            _image = null;
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            _image = _tmpImage;
            _tmpImage = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (!string.IsNullOrEmpty(ImageFilename))
            {
                Image = Image.FromFile(ImageFilename);
                SetEnvelope(ImageFilename);
            }
        }

        #endregion

        #region WorldFile

        internal class WorldFile
        {
            private readonly Matrix2D _matrix = new Matrix2D();
            private Matrix2D _inverse;

            /// <summary>
            /// Creates an instance of this class
            /// </summary>
            /// <param name="a11">x-component of the pixel width</param>
            /// <param name="a21">y-component of the pixel width</param>
            /// <param name="a12">x-component of the pixel height</param>
            /// <param name="a22">y-component of the pixel height</param>
            /// <param name="b1">x-ordinate of the center of the top left pixel</param>
            /// <param name="b2">y-ordinate of the center of the top left pixel</param>
            public WorldFile(double a11 = 1d, double a21 = 0d, double a12 = 0d, double a22 = -1, double b1 = 0d,
                double b2 = 0d)
            {
                _matrix.A11 = a11;
                _matrix.A21 = a21;
                _matrix.A12 = a12;
                _matrix.A22 = a22;
                _inverse = _matrix.Inverse();

                B1 = b1;
                B2 = b2;
            }

            /// <summary>
            /// Loads a world file
            /// </summary>
            /// <param name="file">The filename</param>
            /// <exception cref="ArgumentNullException"/>
            /// <exception cref="ArgumentException"/>
            public void Load(string file)
            {
                if (string.IsNullOrEmpty(file))
                    throw new ArgumentNullException("file");
                if (File.Exists(file))
                    throw new ArgumentException(string.Format("File '{0}' not found", file), "file");

                using (var sr = new StreamReader(file))
                {
// ReSharper disable AssignNullToNotNullAttribute
                    _matrix.A11 = double.Parse(sr.ReadLine(), NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    _matrix.A21 = double.Parse(sr.ReadLine(), NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    _matrix.A12 = double.Parse(sr.ReadLine(), NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    _matrix.A22 = double.Parse(sr.ReadLine(), NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    B1 = double.Parse(sr.ReadLine(), NumberStyles.Float, NumberFormatInfo.InvariantInfo);
                    B2 = double.Parse(sr.ReadLine(), NumberStyles.Float, NumberFormatInfo.InvariantInfo);
// ReSharper restore AssignNullToNotNullAttribute
                }
                _inverse = _matrix.Inverse();
            }

            /// <summary>
            /// Saves a world file
            /// </summary>
            /// <param name="file">The filename</param>
            /// <exception cref="ArgumentNullException"/>
            public void Save(string file)
            {
                if (string.IsNullOrEmpty(file))
                    throw new ArgumentNullException("file");

                using (var sw = new StreamWriter(file))
                {
                    sw.WriteLine(A11.ToString("R", NumberFormatInfo.InvariantInfo));
                    sw.WriteLine(A21.ToString("R", NumberFormatInfo.InvariantInfo));
                    sw.WriteLine(A12.ToString("R", NumberFormatInfo.InvariantInfo));
                    sw.WriteLine(A22.ToString("R", NumberFormatInfo.InvariantInfo));
                    sw.WriteLine(B1.ToString("R", NumberFormatInfo.InvariantInfo));
                    sw.WriteLine(B2.ToString("R", NumberFormatInfo.InvariantInfo));
                }
            }

            /// <summary>
            /// x-component of the pixel width
            /// </summary>
            public double A11
            {
                get { return _matrix.A11; }
            }

            /// <summary>
            /// y-component of the pixel width
            /// </summary>
            public double A21
            {
                get { return _matrix.A21; }
            }

            /// <summary>
            /// x-component of the pixel height
            /// </summary>
            public double A12
            {
                get { return _matrix.A12; }
            }

            /// <summary>
            /// y-component of the pixel height (negative most of the time)
            /// </summary>
            public double A22
            {
                get { return _matrix.A22; }
            }

            /// <summary>
            /// x-ordinate of the center of the top left pixel
            /// </summary>
            public double B1 { get; private set; }

            /// <summary>
            /// y-ordinate of the center of the top left pixel
            /// </summary>
            public double B2 { get; private set; }

            /// <summary>
            /// Gets a value indicating the point (<see cref="B1"/>, <see cref="B2"/>).
            /// </summary>
            public Coordinate Location
            {
                get { return new Coordinate(B1, B2); }
            }

            /// <summary>
            /// Function to compute the ground coordinate for a given <paramref name="x"/>, <paramref name="y"/> pair.
            /// </summary>
            /// <param name="x">The x pixel</param>
            /// <param name="y">The y pixel</param>
            /// <returns>The ground coordinate</returns>
            public Coordinate ToGround(int x, int y)
            {
                var resX = B1 + (A11*x + A21*y);
                var resY = B2 + (A12*x + A22*y);

                return new Coordinate(resX, resY);
            }

            /// <summary>
            /// Function to compute the ground x-ordinate for a given <paramref name="x"/>, <paramref name="y"/> pair.
            /// </summary>
            /// <param name="x">The x pixel</param>
            /// <param name="y">The y pixel</param>
            /// <returns>The ground coordinate</returns>
            public double ToGroundX(int x, int y)
            {
                return B1 + (A11*x + A21*y);
            }

            /// <summary>
            /// Function to compute the ground y-ordinate for a given <paramref name="x"/>, <paramref name="y"/> pair.
            /// </summary>
            /// <param name="x">The x pixel</param>
            /// <param name="y">The y pixel</param>
            /// <returns>The ground coordinate</returns>
            public double ToGroundY(int x, int y)
            {
                return B2 + (A12*x + A22*y);
            }

            /// <summary>
            /// Function to compute the ground bounding-ordinate for a given <paramref name="width"/>, <paramref name="height"/> pair.
            /// </summary>
            /// <param name="width">The width pixel</param>
            /// <param name="height">The height pixel</param>
            /// <returns>The ground coordinate</returns>
            public IPolygon ToGroundBounds(int width, int height)
            {
                var ringCoordinates = new List<Coordinate>(5);
                var leftTop = ToGround(0, 0);
                ringCoordinates.AddRange(new[]
                {
                    leftTop,
                    ToGround(0, height),
                    ToGround(width, 0),
                    ToGround(width, height),
                    leftTop
                });

                var ring = GeometryFactory.Default.CreateLinearRing(ringCoordinates.ToArray());
                return GeometryFactory.Default.CreatePolygon(ring, null);
            }

            public Point ToRaster(Coordinate point)
            {
                point.X -= B1;
                point.Y -= B2;

                var x = (int) Math.Round(_inverse.A11*point.X + _inverse.A21*point.Y,
                    MidpointRounding.AwayFromZero);
                var y = (int) Math.Round(_inverse.A12*point.X + _inverse.A22*point.Y,
                    MidpointRounding.AwayFromZero);

                return new Point(x, y);
            }

            public int ToRasterX(Coordinate point)
            {
                point.X -= B1;
                point.Y -= B2;

                return (int) Math.Round(_inverse.A11*point.X + _inverse.A21*point.Y,
                    MidpointRounding.AwayFromZero);
            }

            public int ToRasterY(Coordinate point)
            {
                point.X -= B1;
                point.Y -= B2;

                return (int) Math.Round(_inverse.A12*point.X + _inverse.A22*point.Y,
                    MidpointRounding.AwayFromZero);
            }

            private class Matrix2D
            {
                /// <summary>
                /// x-component of the pixel width
                /// </summary>
                public double A11 { get; set; }

                /// <summary>
                /// y-component of the pixel width
                /// </summary>
                public double A21 { get; set; }

                /// <summary>
                /// x-component of the pixel height
                /// </summary>
                public double A12 { get; set; }

                /// <summary>
                /// y-component of the pixel height (negative most of the time)
                /// </summary>
                public double A22 { get; set; }

                /// <summary>
                /// Gets a value indicating the determinant of this matrix
                /// </summary>
                private double Determinant
                {
                    get { return A22*A11 - A21*A12; }
                }

                /// <summary>
                /// Gets a value indicating that <see cref="Inverse()"/> can be computed.
                /// </summary>
                /// <remarks>
                /// Shortcut for <c><see cref="Determinant"/> != 0d</c>
                /// </remarks>
                private bool IsInvertible
                {
                    get { return Determinant != 0d; }
                }

                /// <summary>
                /// Method to compute the inverse Matrix of this matrix
                /// </summary>
                /// <returns>The inverse matrix</returns>
                /// <exception cref="Exception"/>
                public Matrix2D Inverse()
                {
                    if (!IsInvertible)
                        throw new Exception("Matrix not invertible");

                    var det = Determinant;

                    return new Matrix2D
                    {
                        A11 = A22/det,
                        A21 = -A21/det,
                        A12 = -A12/det,
                        A22 = A11/det
                    };
                }
            }

            #endregion
        }
    }
}
