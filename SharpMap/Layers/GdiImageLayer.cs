// Copyright 2013 - 2014 Robert Smart, CNL Software (www.cnlsoftware.com)
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
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mail;
using System.Runtime.Serialization;
using GeoAPI.Geometries;

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

        private Envelope _envelope;
        private float _transparency;

        /// <summary>
        /// Creates an instance of this class using the provided layer
        /// </summary>
        /// <param name="fileName">The path to the file</param>
        public GdiImageLayer(string fileName)
            : this(Path.GetFileName(fileName), Image.FromFile(fileName))
        {
            ImageFilename = fileName;
            SetEnvelope(fileName);
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
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="layerName"/> and <paramref name="image"/>.
        /// </summary>
        /// <param name="layerName">The layer name</param>
        /// <param name="image"></param>
        public GdiImageLayer(string layerName, Image image)
        {
            LayerName = layerName;
            _image = image;
            SetEnvelope();
        }

        /// <summary>
        /// Method to set the <see cref="Envelope"/>
        /// </summary>
        private void SetEnvelope()
        {
            _envelope = new Envelope(0, _image.Width, _image.Height, 0);
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
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            if (map.Center == null)
                throw (new ApplicationException("Cannot render map. View center not specified"));

            if (_image == null)
                throw new Exception("Image not set");

            // View to render
            var worldPos1 = Point.Truncate(map.WorldToImage(_envelope.TopLeft()));
            var worldPos2 = Point.Ceiling(map.WorldToImage(_envelope.BottomRight()));

            var rect = new Rectangle(worldPos1, new Size(worldPos2.X - worldPos1.X, 
                                                         worldPos2.Y - worldPos1.Y));

            // ToDo clip image to the required bounds
            //rect = Rectangle.Intersect(new Rectangle(new Point(), map.Size), rect);

            // Set the interpolation mode
            var smoothingMode = g.InterpolationMode;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Render the image
            using (var ia = new ImageAttributes())
            {
                ia.SetColorMatrix(new ColorMatrix { Matrix44 = 1 - Transparency }, 
                                  ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(_image, rect, 0, 0, _image.Size.Width, _image.Size.Height, 
                            GraphicsUnit.Pixel, ia);
            }

            // reset the interpolation mode
            g.InterpolationMode = smoothingMode;

            base.Render(g, map);
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
                _envelope = CreateEnvelope(wld);
                return;
            }

            var ext = CreateWorldFileExtension(Path.GetExtension(fileName));
            if (string.IsNullOrEmpty(ext)) return;
            {
                wld = Path.ChangeExtension(fileName, ext);
                if (File.Exists(wld))
                {
                    _envelope = CreateEnvelope(wld);
                    return;
                }
            }
            _envelope = new Envelope(new Coordinate(0, 0), new Coordinate(Image.Size.Width, Image.Size.Height));

        }

        /// <summary>
        /// Function to create an envelope for the <see cref="Image"/> and the provided <paramref name="wld">world file </paramref>
        /// </summary>
        /// <param name="wld">The world file</param>
        /// <returns>The envelope</returns>
        private Envelope CreateEnvelope(string wld)
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
                        coefficients[i] = double.Parse(line);
                    }
                }

                // Test for rotation or shear
                if (coefficients[2] != 0d || coefficients[3] != 0d)
                {
                    throw new NotSupportedException("World files with rotation and/or shear are not supported");
                }

                return new Envelope(coefficients[4], coefficients[4] + coefficients[0] * Image.Size.Width,
                                    coefficients[5], coefficients[5] + coefficients[3] * Image.Size.Height);
            }
            catch (Exception)
            {
                return new Envelope(new Coordinate(), new Coordinate(Image.Size.Width, Image.Size.Height));
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
    }
}