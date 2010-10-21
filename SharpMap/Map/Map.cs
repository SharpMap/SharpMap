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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Utilities;
using Point=SharpMap.Geometries.Point;

namespace SharpMap
{
    /// <summary>
    /// Map class
    /// </summary>
    /// <example>
    /// Creating a new map instance, adding layers and rendering the map:
    /// <code lang="C#">
    /// SharpMap.Map myMap = new SharpMap.Map(picMap.Size);
    /// myMap.MinimumZoom = 100;
    /// myMap.BackgroundColor = Color.White;
    /// 
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    ///	string ConnStr = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=password;Database=myGisDb;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.PostGIS(ConnStr, "myTable", "the_geom", 32632);
    /// myLayer.FillStyle = new SolidBrush(Color.FromArgb(240,240,240)); //Applies to polygon types only
    ///	myLayer.OutlineStyle = new Pen(Color.Blue, 1); //Applies to polygon and linetypes only
    /// //Setup linestyle (applies to line types only)
    ///	myLayer.Style.Line.Width = 2;
    ///	myLayer.Style.Line.Color = Color.Black;
    ///	myLayer.Style.Line.EndCap = System.Drawing.Drawing2D.LineCap.Round; //Round end
    ///	myLayer.Style.Line.StartCap = layRailroad.LineStyle.EndCap; //Round start
    ///	myLayer.Style.Line.DashPattern = new float[] { 4.0f, 2.0f }; //Dashed linestyle
    ///	myLayer.Style.EnableOutline = true;
    ///	myLayer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; //Render smooth lines
    ///	myLayer.MaxVisible = 40000;
    /// 
    /// myMap.Layers.Add(myLayer);
    /// // [add more layers...]
    /// 
    /// myMap.Center = new SharpMap.Geometries.Point(725000, 6180000); //Set center of map
    ///	myMap.Zoom = 1200; //Set zoom level
    /// myMap.Size = new System.Drawing.Size(300,200); //Set output size
    /// 
    /// System.Drawing.Image imgMap = myMap.GetMap(); //Renders the map
    /// </code>
    /// </example>
    public class Map : IDisposable
    {
        /// <summary>
        /// Used for converting numbers to/from strings
        /// </summary>
        public static NumberFormatInfo NumberFormatEnUs = new CultureInfo("en-US", false).NumberFormat;

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public Map() : this(new Size(300, 150))
        {
        }

        /// <summary>
        /// Initializes a new map
        /// </summary>
        /// <param name="size">Size of map in pixels</param>
        public Map(Size size)
        {
            Size = size;
            Layers = new LayerCollection();
            BackColor = Color.Transparent;
            _MaximumZoom = double.MaxValue;
            _MinimumZoom = 0;
            _MapTransform = new Matrix();
            MapTransformInverted = new Matrix();
            _Center = new Point(0, 0);
            _Zoom = 1;
            _PixelAspectRatio = 1.0;
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the map object
        /// </summary>
        public void Dispose()
        {
            foreach (Layer layer in Layers)
                if (layer is IDisposable)
                    ((IDisposable) layer).Dispose();
            Layers.Clear();
        }

        #endregion

        #region Events

        #region Delegates

        /// <summary>
        /// EventHandler for event fired when the maps layer list has been changed
        /// </summary>
        public delegate void LayersChangedEventHandler();

        /// <summary>
        /// EventHandler for event fired when all layers have been rendered
        /// </summary>
        public delegate void MapRenderedEventHandler(Graphics g);

        /// <summary>
        /// EventHandler for event fired when the zoomlevel or the center point has been changed
        /// </summary>
        public delegate void MapViewChangedHandler();

        #endregion

        /// <summary>
        /// Event fired when the maps layer list have been changed
        /// </summary>
        public event LayersChangedEventHandler LayersChanged;

        /// <summary>
        /// Event fired when the zoomlevel or the center point has been changed
        /// </summary>
        public event MapViewChangedHandler MapViewOnChange;


        /// <summary>
        /// Event fired when all layers have been rendered
        /// </summary>
        public event MapRenderedEventHandler MapRendered;

        #endregion

        #region Methods

        /// <summary>
        /// Renders the map to an image
        /// </summary>
        /// <returns>the map image</returns>
        public Image GetMap()
        {
            Image img = new Bitmap(Size.Width, Size.Height);
            Graphics g = Graphics.FromImage(img);
            RenderMap(g);
            g.Dispose();
            return img;
            /*
            if (Layers == null || Layers.Count == 0)
                throw new InvalidOperationException("No layers to render");

            Image img = new Bitmap(Size.Width, Size.Height);
            Graphics g = Graphics.FromImage(img);
            g.Transform = MapTransform;
            g.Clear(BackColor);
            g.PageUnit = GraphicsUnit.Pixel;
            int SRID = (Layers.Count > 0 ? Layers[0].SRID : -1); //Get the SRID of the first layer
            for (int i = 0; i < _Layers.Count; i++)
            {
                if (_Layers[i].Enabled && _Layers[i].MaxVisible >= Zoom && _Layers[i].MinVisible < Zoom)
                    _Layers[i].Render(g, this);
            }
            if (MapRendered != null) MapRendered(g); //Fire render event
            g.Dispose();
            return img;
             */
        }
        /// <summary>
        /// Renders the map using the provided <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="g">the <see cref="Graphics"/> object to use</param>
        /// <exception cref="ArgumentNullException">if <see cref="Graphics"/> object is null.</exception>
        /// <exception cref="InvalidOperationException">if there are no layers to render.</exception>
        public void RenderMap(Graphics g)
        {
            if (g == null)
                throw new ArgumentNullException("g", "Cannot render map with null graphics object!");

            if (Layers == null || Layers.Count == 0)
                throw new InvalidOperationException("No layers to render");

            g.Transform = MapTransform;
            g.Clear(BackColor);
            g.PageUnit = GraphicsUnit.Pixel;

            //int srid = (Layers.Count > 0 ? Layers[0].SRID : -1); //Get the SRID of the first layer
            for (int i = 0; i < _Layers.Count; i++)
            {
                if (_Layers[i].Enabled && _Layers[i].MaxVisible >= Zoom && _Layers[i].MinVisible < Zoom)
                    _Layers[i].Render(g, this);
            }

            RenderDisclaimer(g);

            if (MapRendered != null) MapRendered(g); //Fire render event

        }

        private void RenderDisclaimer(Graphics g)
        {

            StringFormat sf;
            //Disclaimer
            if (!String.IsNullOrEmpty(_disclaimer))
            {
                SizeF size = VectorRenderer.SizeOfString(g, _disclaimer, _disclaimerFont);
                size.Width = (Single)Math.Ceiling(size.Width);
                size.Height = (Single)Math.Ceiling(size.Height);
                switch (DisclaimerLocation)
                {
                    case 0: //Right-Bottom
                        sf = new StringFormat();
                        sf.Alignment = StringAlignment.Far;
                        g.DrawString(Disclaimer, DisclaimerFont, Brushes.Black,
                            g.VisibleClipBounds.Width,
                            g.VisibleClipBounds.Height - size.Height - 2, sf);
                        break;
                    case 1: //Right-Top
                        sf = new StringFormat();
                        sf.Alignment = StringAlignment.Far;
                        g.DrawString(Disclaimer, DisclaimerFont, Brushes.Black,
                            g.VisibleClipBounds.Width, 0f, sf);
                        break;
                    case 2: //Left-Top
                        g.DrawString(Disclaimer, DisclaimerFont, Brushes.Black, 0f, 0f);
                        break;
                    case 3://Left-Bottom
                        g.DrawString(Disclaimer, DisclaimerFont, Brushes.Black, 0f,
                            g.VisibleClipBounds.Height - size.Height - 2);
                        break;
                }
            }
        }

        /// <summary>
        /// Returns an enumerable for all layers containing the search parameter in the LayerName property
        /// </summary>
        /// <param name="layername">Search parameter</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<ILayer> FindLayer(string layername)
        {
            foreach (ILayer l in Layers)
                if (l.LayerName.Contains(layername))
                    yield return l;
        }

        /// <summary>
        /// Returns a layer by its name
        /// </summary>
        /// <param name="name">Name of layer</param>
        /// <returns>Layer</returns>
        public ILayer GetLayerByName(string name)
        {
            //return _Layers.Find(delegate(SharpMap.Layers.ILayer layer) { return layer.LayerName.Equals(name); });
            for (int i = 0; i < _Layers.Count; i++)
                if (String.Equals(_Layers[i].LayerName, name, StringComparison.InvariantCultureIgnoreCase))
                    return _Layers[i];

            return null;
        }

        /// <summary>
        /// Zooms to the extents of all layers
        /// </summary>
        public void ZoomToExtents()
        {
            ZoomToBox(GetExtents());
        }

        /// <summary>
        /// Zooms the map to fit a bounding box
        /// </summary>
        /// <remarks>
        /// NOTE: If the aspect ratio of the box and the aspect ratio of the mapsize
        /// isn't the same, the resulting map-envelope will be adjusted so that it contains
        /// the bounding box, thus making the resulting envelope larger!
        /// </remarks>
        /// <param name="bbox"></param>
        public void ZoomToBox(BoundingBox bbox)
        {
            _Zoom = bbox.Width; //Set the private center value so we only fire one MapOnViewChange event
            if (Envelope.Height < bbox.Height)
                _Zoom *= bbox.Height/Envelope.Height;
            Center = bbox.GetCentroid();
        }

        /// <summary>
        /// Converts a point from world coordinates to image coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in world coordinates</param>
        /// <returns>Point in image coordinates</returns>
        public PointF WorldToImage(Point p)
        {
            PointF pTmp=  Transform.WorldtoMap(p, this);
            if (!MapTransform.IsIdentity)
            {
                PointF[] pts = new PointF[] { pTmp };
                MapTransform.TransformPoints(pts);
                pTmp = pts[0];
            }
            return pTmp;
        }

        /// <summary>
        /// Converts a point from image coordinates to world coordinates based on the current
        /// zoom, center and mapsize.
        /// </summary>
        /// <param name="p">Point in image coordinates</param>
        /// <returns>Point in world coordinates</returns>
        public Point ImageToWorld(PointF p)
        {
            if (!MapTransform.IsIdentity)
            {
                PointF[] pts = new PointF[] { p };
                MapTransformInverted.TransformPoints(pts);
                p = pts[0];
            }            
            return Transform.MapToWorld(p, this);
        }

        #endregion

        #region Properties

        private Color _BackgroundColor;
        private Point _Center;
        private LayerCollection _Layers;
        private Matrix _MapTransform;
        private double _MaximumZoom;
        private double _MinimumZoom;
        private double _PixelAspectRatio = 1.0;
        private Size _Size;
        private double _Zoom;
        internal Matrix MapTransformInverted;

        /// <summary>
        /// Gets the extents of the current map based on the current zoom, center and mapsize
        /// </summary>
        public BoundingBox Envelope
        {
            get
            {
                return new BoundingBox(
                    new Point(Center.X - Zoom*.5, Center.Y - MapHeight*.5),
                    new Point(Center.X + Zoom*.5, Center.Y + MapHeight*.5));
            }
        }

        /// <summary>
        /// Using the <see cref="MapTransform"/> you can alter the coordinate system of the map rendering.
        /// This makes it possible to rotate or rescale the image, for instance to have another direction than north upwards.
        /// </summary>
        /// <example>
        /// Rotate the map output 45 degrees around its center:
        /// <code lang="C#">
        /// System.Drawing.Drawing2D.Matrix maptransform = new System.Drawing.Drawing2D.Matrix(); //Create transformation matrix
        ///	maptransform.RotateAt(45,new PointF(myMap.Size.Width/2,myMap.Size.Height/2)); //Apply 45 degrees rotation around the center of the map
        ///	myMap.MapTransform = maptransform; //Apply transformation to map
        /// </code>
        /// </example>
        public Matrix MapTransform
        {
            get { return _MapTransform; }
            set
            {
                _MapTransform = value;
                if (_MapTransform.IsInvertible)
                {
                    MapTransformInverted = value.Clone();
                    MapTransformInverted.Invert();
                }
                else
                    MapTransformInverted.Reset();
            }
        }

        /// <summary>
        /// A collection of layers. The first layer in the list is drawn first, the last one on top.
        /// </summary>
        public LayerCollection Layers
        {
            get { return _Layers; }
            set
            {
                int iBefore = 0;
                if (_Layers != null)
                    iBefore = _Layers.Count;
                _Layers = value;
                if (value != null)
                {
                    if (LayersChanged != null) //Layers changed. Fire event
                        LayersChanged();
                    if (MapViewOnChange != null)
                        MapViewOnChange();
                }
            }
        }

        /// <summary>
        /// Map background color (defaults to transparent)
        /// </summary>
        public Color BackColor
        {
            get { return _BackgroundColor; }
            set
            {
                _BackgroundColor = value;
                if (MapViewOnChange != null)
                    MapViewOnChange();
            }
        }

        /// <summary>
        /// Center of map in WCS
        /// </summary>
        public Point Center
        {
            get { return _Center; }
            set
            {
                _Center = value;
                if (MapViewOnChange != null)
                    MapViewOnChange();
            }
        }

        /// <summary>
        /// Gets or sets the zoom level of map.
        /// </summary>
        /// <remarks>
        /// <para>The zoom level corresponds to the width of the map in WCS units.</para>
        /// <para>A zoomlevel of 0 will result in an empty map being rendered, but will not throw an exception</para>
        /// </remarks>
        public double Zoom
        {
            get { return _Zoom; }
            set
            {
                if (value < _MinimumZoom)
                    _Zoom = _MinimumZoom;
                else if (value > _MaximumZoom)
                    _Zoom = _MaximumZoom;
                else
                    _Zoom = value;
                if (MapViewOnChange != null)
                    MapViewOnChange();
            }
        }

        /// <summary>
        /// Returns the size of a pixel in world coordinate units
        /// </summary>
        public double PixelSize
        {
            get { return Zoom/Size.Width; }
        }

        /// <summary>
        /// Returns the width of a pixel in world coordinate units.
        /// </summary>
        /// <remarks>The value returned is the same as <see cref="PixelSize"/>.</remarks>
        public double PixelWidth
        {
            get { return PixelSize; }
        }

        /// <summary>
        /// Returns the height of a pixel in world coordinate units.
        /// </summary>
        /// <remarks>The value returned is the same as <see cref="PixelSize"/> unless <see cref="PixelAspectRatio"/> is different from 1.</remarks>
        public double PixelHeight
        {
            get { return PixelSize*_PixelAspectRatio; }
        }

        /// <summary>
        /// Gets or sets the aspect-ratio of the pixel scales. A value less than 
        /// 1 will make the map streach upwards, and larger than 1 will make it smaller.
        /// </summary>
        /// <exception cref="ArgumentException">Throws an argument exception when value is 0 or less.</exception>
        public double PixelAspectRatio
        {
            get { return _PixelAspectRatio; }
            set
            {
                if (_PixelAspectRatio <= 0)
                    throw new ArgumentException("Invalid Pixel Aspect Ratio");
                _PixelAspectRatio = value;
            }
        }

        /// <summary>
        /// Height of map in world units
        /// </summary>
        /// <returns></returns>
        public double MapHeight
        {
            get { return (Zoom*Size.Height)/Size.Width*PixelAspectRatio; }
        }

        /// <summary>
        /// Size of output map
        /// </summary>
        public Size Size
        {
            get { return _Size; }
            set { _Size = value; }
        }

        /// <summary>
        /// Minimum zoom amount allowed
        /// </summary>
        public double MinimumZoom
        {
            get { return _MinimumZoom; }
            set
            {
                if (value < 0)
                    throw (new ArgumentException("Minimum zoom must be 0 or more"));
                _MinimumZoom = value;
            }
        }

        /// <summary>
        /// Maximum zoom amount allowed
        /// </summary>
        public double MaximumZoom
        {
            get { return _MaximumZoom; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentException("Maximum zoom must larger than 0"));
                _MaximumZoom = value;
            }
        }

        /// <summary>
        /// Gets the extents of the map based on the extents of all the layers in the layers collection
        /// </summary>
        /// <returns>Full map extents</returns>
        public BoundingBox GetExtents()
        {
            if (Layers == null || Layers.Count == 0)
                throw (new InvalidOperationException("No layers to zoom to"));
            BoundingBox bbox = null;
            for (int i = 0; i < Layers.Count; i++)
            {
                if (bbox == null)
                    bbox = Layers[i].Envelope;
                else
                    bbox = bbox.Join(Layers[i].Envelope);
            }
            return bbox;
        }

        #endregion

        #region Disclaimer

        private String _disclaimer;
        /// <summary>
        /// Copyright notice to be placed on the map
        /// </summary>
        public String Disclaimer
        {
            get { return _disclaimer; }
            set {
                //only set disclaimer if not already done
                if (String.IsNullOrEmpty(_disclaimer))
                {
                    _disclaimer = value;
                    //Ensure that Font for disclaimer is set
                    if (_disclaimerFont == null)
                        _disclaimerFont = new Font(FontFamily.GenericSansSerif, 8f);
                }
            }
        }

        private Font _disclaimerFont;
        /// <summary>
        /// Font to use for the Disclaimer
        /// </summary>
        public Font DisclaimerFont
        {
            get { return _disclaimerFont; }
            set
            {
                if (value == null) return;
                _disclaimerFont = value;
            }
        }

        private Int32 _disclaimerLocation;
        /// <summary>
        /// Location for the disclaimer
        /// 2|1
        /// -+-
        /// 3|0
        /// </summary>
        public Int32 DisclaimerLocation
        {
            get { return _disclaimerLocation; }
            set { _disclaimerLocation = value%4; }
        }

        #endregion

        //#region ISerializable Members

        ///// <summary>
        ///// Populates a SerializationInfo with the data needed to serialize the target object.
        ///// </summary>
        ///// <param name="info"></param>
        ///// <param name="context"></param>
        //public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        //{
        //    System.Runtime.Serialization.SurrogateSelector ss = SharpMap.Utilities.Surrogates.GetSurrogateSelectors();
        //    info.AddValue("BackgroundColor", this._BackgroundColor);
        //    info.AddValue("Center", this._Center);
        //    info.AddValue("Layers", this._Layers);
        //    info.AddValue("MapTransform", this._MapTransform);
        //    info.AddValue("MaximumZoom", this._MaximumZoom);
        //    info.AddValue("MinimumZoom", this._MinimumZoom);
        //    info.AddValue("Size", this._Size);
        //    info.AddValue("Zoom", this._Zoom);

        //}
        ///// <summary>
        ///// Deserialization constructor.
        ///// </summary>
        ///// <param name="info"></param>
        ///// <param name="ctxt"></param>
        //internal Map(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext ctxt)
        //{
        //    this._BackgroundColor = (System.Drawing.Color)info.GetValue("BackgroundColor", typeof(System.Drawing.Color));
        //    this._Center = (SharpMap.Geometries.Point)info.GetValue("Center", typeof(SharpMap.Geometries.Point));
        //    this._Layers = (List<SharpMap.Layers.ILayer>)info.GetValue("Layers", typeof(List<SharpMap.Layers.ILayer>));
        //    this._MapTransform = (System.Drawing.Drawing2D.Matrix)info.GetValue("MapTransform", typeof(System.Drawing.Drawing2D.Matrix));
        //    this._MaximumZoom = info.GetDouble("MaximumZoom");
        //    this._MinimumZoom = info.GetDouble("MinimumZoom");
        //    this._Size = (System.Drawing.Size)info.GetValue("Size", typeof(System.Drawing.Size));
        //    this._Zoom = info.GetDouble("Zoom");
        //}

        //#endregion
    }
}