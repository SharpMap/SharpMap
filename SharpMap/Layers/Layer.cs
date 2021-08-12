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
using SharpMap.CoordinateSystems.Transformations;

using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap.Base;
using SharpMap.Rendering;
using SharpMap.Styles;
using NetTopologySuite;

namespace SharpMap.Layers
{
    /// <summary>
    /// Abstract class for common layer properties
    /// Implement this class instead of the ILayer interface to save a lot of common code.
    /// </summary>
    [Serializable]
    public abstract partial class Layer : DisposableObject, ILayerEx
    {
        #region Events

        #region Delegates

        /// <summary>
        /// EventHandler for event fired when the layer has been rendered
        /// </summary>
        /// <param name="layer">Layer rendered</param>
        /// <param name="g">Reference to graphics object used for rendering</param>
        public delegate void LayerRenderedEventHandler(Layer layer, Graphics g);

        #endregion

        /// <summary>
        /// Event fired when the layer has been rendered
        /// </summary>
        public event LayerRenderedEventHandler LayerRendered;

        /// <summary>
        /// Event raised when the layer's <see cref="SRID"/> property has changed
        /// </summary>
        public event EventHandler SRIDChanged;

        /// <summary>
        /// Method called when <see cref="SRID"/> has changed, to invoke <see cref="E:SharpMap.Layers.Layer.SRIDChanged"/>
        /// </summary>
        /// <param name="eventArgs">The arguments associated with the event</param>
        protected virtual void OnSridChanged(EventArgs eventArgs)
        {
            var handler = SRIDChanged;
            if (handler != null) handler(this, eventArgs);
        }

        /// <summary>
        /// Event raised when the layer's <see cref="Style"/> property has changed
        /// </summary>
        public event EventHandler StyleChanged;

        /// <summary>
        /// Method called when <see cref="Style"/> has changed, to invoke <see cref="E:SharpMap.Layers.Layer.StyleChanged"/>
        /// </summary>
        /// <param name="eventArgs">The arguments associated with the event</param>
        protected virtual void OnStyleChanged(EventArgs eventArgs)
        {
            var handler = StyleChanged;
            if (handler != null) handler(this, eventArgs);
        }

        /// <summary>
        /// Event raised when the layers's <see cref="LayerName"/> property has changed
        /// </summary>
        public event EventHandler LayerNameChanged;

        /// <summary>
        /// Method called when <see cref="LayerName"/> has changed, to invoke <see cref="E:SharpMap.Layers.Layer.LayerNameChanged"/>
        /// </summary>
        /// <param name="eventArgs">The arguments associated with the event</param>
        protected virtual void OnLayerNameChanged(EventArgs eventArgs)
        {
            var handler = LayerNameChanged;
            if (handler != null) handler(this, eventArgs);
        }

        #endregion

        private ICoordinateTransformation _coordinateTransform;
        private ICoordinateTransformation _reverseCoordinateTransform;
        private GeometryFactory _sourceFactory;
        private GeometryFactory _targetFactory;

        private string _layerName;
        private string _layerTitle;
        private IStyle _style;
        private int _srid = -1;
        private int? _targetSrid;
        [field: NonSerialized]
        private bool _shouldNotResetCt;
        
        /// <summary>
        /// The area of the map that was covered by this layer
        /// </summary>
        [field: NonSerialized]
        protected RectangleF CanvasArea = RectangleF.Empty;
        
        // ReSharper disable PublicConstructorInAbstractClass
        ///<summary>
        /// Creates an instance of this class using the given Style
        ///</summary>
        ///<param name="style"></param>
        public Layer(Style style)
        // ReSharper restore PublicConstructorInAbstractClass
        {
            _style = style;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        protected Layer() //Style style)
        {
            _style = new Style();
        }

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            _coordinateTransform = null;
            _reverseCoordinateTransform = null;
            _style = null;

            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.CoordinateSystems.Transformations.ICoordinateTransformation"/> applied 
        /// to this vectorlayer prior to rendering
        /// </summary>
        public virtual ICoordinateTransformation CoordinateTransformation
        {
            get
            {
                if (_coordinateTransform == null && NeedsTransformation)
                {
                    var css = Session.Instance.CoordinateSystemServices;
                    _coordinateTransform = css.CreateTransformation(
                        css.GetCoordinateSystem(SRID), css.GetCoordinateSystem(TargetSRID));
                }
                return _coordinateTransform;
            }
            set
            {
                if (value == _coordinateTransform && value != null)
                    return;

                _coordinateTransform = value;

                try
                {
                    // we don't want that by setting SRID we get the CoordinateTransformation resetted
                    _shouldNotResetCt = true;

                    if (_coordinateTransform != null)
                    {
                        // causes sourceFactory/targetFactory to reset to new SRID/TargetSRID
                        SRID = Convert.ToInt32(CoordinateTransformation.SourceCS.AuthorityCode);
                        TargetSRID = Convert.ToInt32(CoordinateTransformation.TargetCS.AuthorityCode);
                    }
                    else
                    {
                        _sourceFactory = NtsGeometryServices.Instance.CreateGeometryFactory(SRID);
                        // causes targetFactory to be cleared
                        TargetSRID = 0;
                    }
                }
                finally
                {
                    _shouldNotResetCt = false;
                }

                // check if ReverseTransform is required
                if (_coordinateTransform == null || !NeedsTransformation)
                    _reverseCoordinateTransform = null;

                // check if existing ReverseTransform is compatible with CoordinateTransform
                if (_reverseCoordinateTransform != null)
                {
                    //clear if not compatible with CoordinateTransformation
                    if (_coordinateTransform.SourceCS.AuthorityCode != _coordinateTransform.TargetCS.AuthorityCode ||
                        _coordinateTransform.TargetCS.AuthorityCode != _coordinateTransform.SourceCS.AuthorityCode)
                        _reverseCoordinateTransform = null;
                }

                OnCoordinateTransformationChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event raised when the <see cref="CoordinateTransformation"/> has changed
        /// </summary>
        public event EventHandler CoordinateTransformationChanged;

        /// <summary>
        /// Event invoker for the <see cref="CoordinateTransformationChanged"/> event
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnCoordinateTransformationChanged(EventArgs e)
        {
            if (CoordinateTransformationChanged != null)
                CoordinateTransformationChanged(this, e);
        }

        /// <summary>
        /// Gets the geometry factory to create source geometries
        /// </summary>
        protected internal GeometryFactory SourceFactory { get { return _sourceFactory ?? (_sourceFactory = NtsGeometryServices.Instance.CreateGeometryFactory(SRID)); } }

        /// <summary>
        /// Gets the geometry factory to create target geometries
        /// </summary>
        protected internal GeometryFactory TargetFactory { get { return _targetFactory ?? _sourceFactory; } }

        /// <summary>
        /// Certain Transformations cannot be inverted in ProjNet, in those cases use this property to set the reverse <see cref="SharpMap.CoordinateSystems.Transformations.ICoordinateTransformation"/> (of CoordinateTransformation) to fetch data from Datasource
        /// 
        /// If your CoordinateTransformation can be inverted you can leave this property to null
        /// </summary>
        public virtual ICoordinateTransformation ReverseCoordinateTransformation
        {
            get
            {
                if (_reverseCoordinateTransform == null && NeedsTransformation)
                {
                    var css = Session.Instance.CoordinateSystemServices;
                    _reverseCoordinateTransform = css.CreateTransformation(
                        css.GetCoordinateSystem(TargetSRID), css.GetCoordinateSystem(SRID));
                }
                return _reverseCoordinateTransform;
            }
            set
            {
                if (value == _reverseCoordinateTransform)
                    return;
                _reverseCoordinateTransform = value;
            }
        }

        /// <summary>
        /// Gets a flag indicating if the layer needs coordinate transformation
        /// </summary>
        protected bool NeedsTransformation
        {
            get { return SRID != 0 && TargetSRID != 0 && SRID != TargetSRID; }
        }

        #region ILayer Members

        /// <summary>
        /// Gets or sets the name of the layer
        /// </summary>
        public string LayerName
        {
            get { return _layerName; }
            set { _layerName = value; }
        }

        /// <summary>
        /// Gets or sets the title of the layer
        /// </summary>
        public string LayerTitle
        {
            get { return _layerTitle; }
            set { _layerTitle = value; }
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public virtual int SRID
        {
            get { return _srid; }
            set
            {
                if (value != _srid)
                {
                    _srid = value;

                    _sourceFactory = NtsGeometryServices.Instance.CreateGeometryFactory(value);
                    if (!_shouldNotResetCt)
                        _coordinateTransform = _reverseCoordinateTransform = null;

                    OnSridChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the target spatial reference id
        /// </summary>
        public virtual int TargetSRID
        {
            get { return _targetSrid.HasValue ? _targetSrid.Value : SRID; }
            set
            {
                if (value == SRID || value == 0)
                {
                    _targetSrid = null;
                    _targetFactory = null;
                }
                else if (_targetSrid != value)
                {
                    _targetSrid = value;
                    _targetFactory = NtsGeometryServices.Instance.CreateGeometryFactory(value);
                }
                if (!_shouldNotResetCt)
                    _coordinateTransform = _reverseCoordinateTransform = null;
            }
        }

        //public abstract SharpMap.CoordinateSystems.CoordinateSystem CoordinateSystem { get; set; }


        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        [Obsolete("Use Render(Graphics, MapViewport, out Envelope affectedArea)")]
        public virtual void Render(Graphics g, Map map)
        {
            Render(g, (MapViewport)map, out _);
        }

        /// <summary>
        /// Renders the layer using the current viewport
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public virtual void Render(Graphics g, MapViewport map)
        {
            Render(g, map, out _);
        }

        /// <summary>
        /// Renders the layer using the current viewport
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        /// <returns>Rectangle enclosing the actual area rendered on the graphics canvas</returns>
        Rectangle ILayerEx.Render(Graphics g, MapViewport map)
        {
            Render(g, map, out var canvasArea);
            return  canvasArea;
        }

        private bool _renderCalled;
        /// <summary>
        /// Renders the layer using the given graphics object and viewport. The <paramref name="affectedArea"/> is an additional result.
        /// </summary>
        /// <param name="g">A graphics object</param>
        /// <param name="mvp">A map viewport</param>
        /// <param name="affectedArea">The affected area.</param>
        protected virtual void Render(Graphics g, MapViewport mvp, out Rectangle affectedArea)
        {
            if (_renderCalled)
            {
                affectedArea = default;
                return;
            }
            
            _renderCalled = true;
            Render(g, mvp);
            _renderCalled = false;

            var mapRect = new Rectangle(new System.Drawing.Point(0, 0), mvp.Size);
            if (CanvasArea.IsEmpty)
            {
                affectedArea = mapRect;
            }
            else
            {
                affectedArea = ToGraphicsCanvas(CanvasArea, g.Transform);

                // clip to graphics canvas
                affectedArea.Intersect(mapRect);

                CanvasArea = RectangleF.Empty;
            }
            
            OnLayerRendered(g);
        }

        /// <summary>
        /// Transforms cavas area to untransformed graphics canvas
        /// </summary>
        /// <param name="area"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        protected static Rectangle ToGraphicsCanvas(RectangleF area, System.Drawing.Drawing2D.Matrix matrix)
        {
            if (!matrix.IsIdentity)
            {
                var pts = area.ToPointArray();
                matrix.TransformPoints(pts);
                // Enclosing rectangle aligned with graphics canvas and inflated to nearest integer values.
                area = pts.ToRectangleF();
            }

            // This is the area of the graphics canvas that needs to be refreshed when invalidating the image. 
            var affectedArea = area.ToRectangle();

//                // proof of concept: draw affected area to screen aligned with graphics canvas
//                using (var orig = g.Transform.Clone())
//                {
//                    var areaToBeRendered = affectedArea;
//                    areaToBeRendered.Intersect(mapRect);
//                    g.ResetTransform();
//                    g.DrawRectangle(new Pen(Color.Red, 3f) {Alignment = System.Drawing.Drawing2D.PenAlignment.Inset},
//                        areaToBeRendered);
//                    g.Transform = orig;
//                }

            // allow for bleed and/or minor labelling misdemeanours
            affectedArea.Inflate(1, 1);

            return affectedArea;
        }

        /// <summary>
        /// Event invoker for the <see cref="LayerRendered"/> event.
        /// </summary>
        /// <param name="g">The graphics object</param>
        protected virtual void OnLayerRendered(Graphics g)
        {
            if (LayerRendered != null)
                LayerRendered(this, g);
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public abstract Envelope Envelope { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Proj4 projection definition string
        /// </summary>
        public string Proj4Projection { get; set; }
        /*
        private bool _Enabled = true;
        private double _MaxVisible = double.MaxValue;
        private double _MinVisible = 0;
        */
        /// <summary>
        /// Minimum visibility zoom, including this value
        /// </summary>
        public double MinVisible
        {
            get
            {
                return _style.MinVisible; // return _MinVisible;
            }
            set
            {
                _style.MinVisible = value; // _MinVisible = value; 
            }
        }

        /// <summary>
        /// Maximum visibility zoom, excluding this value
        /// </summary>
        public double MaxVisible
        {
            get
            {
                //return _MaxVisible; 
                return _style.MaxVisible;
            }
            set
            {
                //_MaxVisible = value;
                _style.MaxVisible = value;
            }
        }

        /// <summary>
        /// Gets or Sets what kind of units the Min/Max visible properties are defined in
        /// </summary>
        public VisibilityUnits VisibilityUnits
        {
            get
            {
                return _style.VisibilityUnits;
            }
            set
            {
                _style.VisibilityUnits = value;
            }
        }

        /// <summary>
        /// Specified whether the layer is rendered or not
        /// </summary>
        public bool Enabled
        {
            get
            {
                //return _Enabled;
                return _style.Enabled;
            }
            set
            {
                if (value == _style.Enabled)
                    return;

                _style.Enabled = value;
                RaiseRenderRequired();
            }
        }

        /// <summary>
        /// Gets or sets the Style for this Layer
        /// </summary>
        public virtual IStyle Style
        {
            get { return _style; }
            set
            {
                if (value != _style && !_style.Equals(value))
                {
                    _style = value;
                    OnStyleChanged(EventArgs.Empty);
                    RaiseRenderRequired();
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns the name of the layer.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return LayerName;
        }

        /// <summary>
        /// Invokes <see cref="RenderRequired"/> event on this layer
        /// </summary>
        public void RaiseRenderRequired()
        {
            RenderRequired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when the layer needs to be rendered.
        /// </summary>
        public event EventHandler RenderRequired;

        #region Reprojection utility functions

        /// <summary>
        /// Utility function to transform given envelope using a specific transformation
        /// </summary>
        /// <param name="envelope">The source envelope</param>
        /// <param name="coordinateTransformation">The <see cref="SharpMap.CoordinateSystems.Transformations.ICoordinateTransformation"/> to use.</param>
        /// <returns>The target envelope</returns>
        protected virtual Envelope ToTarget(Envelope envelope, ICoordinateTransformation coordinateTransformation)
        {
            if (coordinateTransformation == null)
                return envelope;

            return GeometryTransform.TransformBox(envelope, coordinateTransformation.MathTransform);
        }

        /// <summary>
        /// Utility function to transform given envelope to the target envelope
        /// </summary>
        /// <param name="envelope">The source envelope</param>
        /// <returns>The target envelope</returns>
        protected Envelope ToTarget(Envelope envelope)
        {
            return ToTarget(envelope, CoordinateTransformation);
        }

        /// <summary>
        /// Utility function to transform given envelope to the source envelope
        /// </summary>
        /// <param name="envelope">The target envelope</param>
        /// <returns>The source envelope</returns>
        protected virtual Envelope ToSource(Envelope envelope)
        {
            if (ReverseCoordinateTransformation != null)
            {
                return GeometryTransform.TransformBox(envelope, ReverseCoordinateTransformation.MathTransform);
            }

            if (CoordinateTransformation != null)
            {
                var mt = CoordinateTransformation.MathTransform;
                mt.Invert();
                var res = GeometryTransform.TransformBox(envelope, mt);
                mt.Invert();
                return res;
            }

            // no transformation
            return envelope;
        }

        /// <summary>
        /// Function to transform <paramref name="geometry"/> to the coordinate system defined by <see cref="TargetSRID"/>
        /// </summary>
        /// <param name="geometry">A geometry</param>
        /// <returns>The transformed geometry</returns>
        protected virtual Geometry ToTarget(Geometry geometry)
        {
            if (geometry.SRID == TargetSRID)
                return geometry;

            if (CoordinateTransformation != null)
            {
                return GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.MathTransform, TargetFactory);
            }

            return geometry;
        }

        /// <summary>
        /// Function to transform <paramref name="geometry"/> to the coordinate system defined by <see cref="SRID"/>
        /// </summary>
        /// <param name="geometry">A geometry</param>
        /// <returns>The transformed geometry</returns>
        protected virtual Geometry ToSource(Geometry geometry)
        {
            if (geometry.SRID == SRID)
                return geometry;

            if (ReverseCoordinateTransformation != null)
            {
                return GeometryTransform.TransformGeometry(geometry,
                    ReverseCoordinateTransformation.MathTransform, SourceFactory);
            }
            if (CoordinateTransformation != null)
            {
                var mt = CoordinateTransformation.MathTransform;
                mt.Invert();
                var res = GeometryTransform.TransformGeometry(geometry, mt, SourceFactory);
                mt.Invert();
                return res;
            }

            return geometry;
        }

        #endregion
    }
}
