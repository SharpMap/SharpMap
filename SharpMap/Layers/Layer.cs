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
#if !DotSpatialProjections
using GeoAPI.CoordinateSystems.Transformations;
#else
using DotSpatial.Projections;
#endif
using GeoAPI.Geometries;
using SharpMap.Base;
using SharpMap.Styles;

namespace SharpMap.Layers
{
    /// <summary>
    /// Abstract class for common layer properties
    /// Implement this class instead of the ILayer interface to save a lot of common code.
    /// </summary>
    [Serializable]
    public abstract class Layer : DisposableObject, ILayer
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
        public event EventHandler SridChanged;

        /// <summary>
        /// Method called when <see cref="SRID"/> has changed, to invoke <see cref="E:SharpMap.Layers.Layer.SridChanged"/>
        /// </summary>
        /// <param name="eventArgs">The arguments associated with the event</param>
        protected virtual void OnSridChanged(EventArgs eventArgs)
        {
            _sourceFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(SRID);
            
            if (SridChanged != null)
                SridChanged(this, eventArgs);
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
            if (StyleChanged != null)
                StyleChanged(this, eventArgs);
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
            if (LayerNameChanged != null)
                LayerNameChanged(this, eventArgs);
        }

        #endregion

        private ICoordinateTransformation _coordinateTransform;
        private ICoordinateTransformation _reverseCoordinateTransform;
        private IGeometryFactory _sourceFactory;
        private IGeometryFactory _targetFactory;

        private string _layerName;
        private IStyle _style;
        private int _srid = -1;
        private int? _targetSrid;

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

#if !DotSpatialProjections
        /// <summary>
        /// Gets or sets the <see cref="GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation"/> applied 
        /// to this vectorlayer prior to rendering
        /// </summary>
#else
        /// <summary>
        /// Gets or sets the <see cref="DotSpatial.Projections.ICoordinateTransformation"/> applied 
        /// to this vectorlayer prior to rendering
        /// </summary>
#endif
        public virtual ICoordinateTransformation CoordinateTransformation
        {
            get { return _coordinateTransform; }
            set
            {
                if (value == _coordinateTransform)
                    return;
                _coordinateTransform = value;
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
            _sourceFactory = _targetFactory = GeoAPI.GeometryServiceProvider.Instance
                .CreateGeometryFactory(SRID);

#if !DotSpatialProjections
            if (CoordinateTransformation != null)
            {
                SRID = Convert.ToInt32(CoordinateTransformation.SourceCS.AuthorityCode);
                TargetSRID = Convert.ToInt32(CoordinateTransformation.TargetCS.AuthorityCode);
            }
#endif
            if (CoordinateTransformationChanged != null)
                CoordinateTransformationChanged(this, e);
        }

        /// <summary>
        /// Gets the geometry factory to create source geometries
        /// </summary>
        protected internal IGeometryFactory SourceFactory { get { return _sourceFactory ?? (_sourceFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(SRID)); } }

        /// <summary>
        /// Gets the geometry factory to create target geometries
        /// </summary>
        protected internal IGeometryFactory TargetFactory { get { return _targetFactory ?? _sourceFactory; } }

#if !DotSpatialProjections
        /// <summary>
        /// Certain Transformations cannot be inverted in ProjNet, in those cases use this property to set the reverse <see cref="GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation"/> (of CoordinateTransformation) to fetch data from Datasource
        /// 
        /// If your CoordinateTransformation can be inverted you can leave this property to null
        /// </summary>
        public virtual ICoordinateTransformation ReverseCoordinateTransformation
        {
            get { return _reverseCoordinateTransform; }
            set { _reverseCoordinateTransform= value; }
        }
#endif

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
                _targetSrid = value;
                _targetFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(value);
            }
        }

        //public abstract SharpMap.CoordinateSystems.CoordinateSystem CoordinateSystem { get; set; }


        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public virtual void Render(Graphics g, Map map)
        {
            OnLayerRendered(g);
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
                //_Enabled = value;
                _style.Enabled = value;
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

        #region Reprojection utility functions

        /// <summary>
        /// Utility function to transform given envelope to the target envelope
        /// </summary>
        /// <param name="envelope">The source envelope</param>
        /// <returns>The target envelope</returns>
        protected virtual Envelope ToTarget(Envelope envelope)
        {
            if (CoordinateTransformation == null)
                return envelope;
#if !DotSpatialProjections
            return GeometryTransform.TransformBox(envelope, CoordinateTransformation.MathTransform);
#else
            return GeometryTransform.TransformBox(box, CoordinateTransformation.Source, CoordinateTransformation.Target);
#endif
        }

        /// <summary>
        /// Utility function to transform given envelope to the source envelope
        /// </summary>
        /// <param name="envelope">The target envelope</param>
        /// <returns>The source envelope</returns>
        protected virtual Envelope ToSource(Envelope envelope)
        {
#if !DotSpatialProjections
            if (ReverseCoordinateTransformation != null)
            {
                return GeometryTransform.TransformBox(envelope, ReverseCoordinateTransformation.MathTransform);
            }
#endif
            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                var mt = CoordinateTransformation.MathTransform;
                mt.Invert();
                var res = GeometryTransform.TransformBox(envelope, mt);
                mt.Invert();
                return res;
#else
                return GeometryTransform.TransformBox(envelope, CoordinateTransformation.Target, CoordinateTransformation.Source);
#endif
            }

            // no transformation
            return envelope;
        }

        protected virtual IGeometry ToTarget(IGeometry geometry)
        {
            if (geometry.SRID == TargetSRID)
                return geometry;

            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                return GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.MathTransform, TargetFactory);
#else
                return GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.Source, CoordinateTransformation.Target, TargetFactory);
#endif
            }

            return geometry;
        }

        protected virtual IGeometry ToSource(IGeometry geometry)
        {
            if (geometry.SRID == SRID)
                return geometry;

#if !DotSpatialProjections
            if (ReverseCoordinateTransformation != null)
            {
                return GeometryTransform.TransformGeometry(geometry,
                    ReverseCoordinateTransformation.MathTransform, SourceFactory);
            }
#endif
            if (CoordinateTransformation != null)
            {
#if !DotSpatialProjections
                var mt = CoordinateTransformation.MathTransform;
                mt.Invert();
                var res = GeometryTransform.TransformGeometry(geometry, mt, SourceFactory);
                mt.Invert();
                return res;
#else
                return GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.Target, CoordinateTransformation.Source, SourceFactory);
#endif
            }

            return geometry;
        }

        #endregion
    }
}