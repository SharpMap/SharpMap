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
using System.Collections;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using SharpMap.Data;
using GeoAPI.Geometries;
using SharpMap.Styles;
using GeoAPI.CoordinateSystems.Transformations;

namespace SharpMap.Layers
{
    /// <summary>
    /// Class for holding a group of layers.
    /// </summary>
    /// <remarks>
    /// The Group layer is useful for grouping a set of layers,
    /// for instance a set of image tiles, and expose them as a single layer
    /// </remarks>
    [Serializable]
    public partial class LayerGroup : Layer, ICanQueryLayer, ICloneable, ILayersContainer
    {
        private ObservableCollection<ILayer> _layers;
        private bool _isQueryEnabled = true;

        /// <summary>
        /// Event fired when the Layers collection is replaced.
        /// </summary>
        public event EventHandler LayersChanged;

        /// <summary>
        /// Fires the LayersChanged event.
        /// </summary>
        protected virtual void OnLayersChanged()
        {
            EventHandler handler = LayersChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event fires when the Layers collection is going to be replaced.
        /// </summary>
        public event EventHandler LayersChanging;

        /// <summary>
        /// Fires the LayersChanging event.
        /// </summary>
        protected virtual void OnLayersChanging()
        {
            EventHandler handler = LayersChanging;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Initializes a new group layer
        /// </summary>
        /// <param name="layername">Name of layer</param>
        public LayerGroup(string layername)
        {
            LayerName = layername;
            _layers = new ObservableCollection<ILayer>();
        }
        /// <summary>
        /// Whether the layer is queryable when used in a SharpMap.Web.Wms.WmsServer, ExecuteIntersectionQuery() will be possible in all other situations when set to FALSE
        /// </summary>
        public virtual bool IsQueryEnabled
        {
            get { return _isQueryEnabled; }
            set { _isQueryEnabled = value; }
        }
        /// <summary>
        /// Sublayers in the group
        /// </summary>
        public virtual ObservableCollection<ILayer> Layers
        {
            get { return _layers; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (!Equals(value, _layers))
                {
                    OnLayersChanging();
                    _layers = value;
                    OnLayersChanged();
                }
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
                Envelope bbox = null;
                var layers = GetSnapshot();

                for (int i = 0; i < layers.Length; i++)
                {
                    var layerEnvelope = layers[i].Envelope;
                    if (layerEnvelope != null)
                    {
                        if(bbox == null)
                            bbox = new Envelope(layerEnvelope);
                        else
                            bbox.ExpandToInclude(layerEnvelope);
                    }
                }
                    
                return bbox;
            }
        }

        /// <summary>
        /// Gets or sets whether coordinate transformations applied to the group should propagate to inner layers.
        /// </summary>
        /// <remarks>
        /// Default is false, transformations are propagated to children layers.
        /// </remarks>
        public virtual bool SkipTransformationPropagation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation"/> applied 
        /// to this vectorlayer prior to rendering
        /// </summary>

        public override ICoordinateTransformation CoordinateTransformation
        {
            get { return base.CoordinateTransformation; }
            set
            {
                base.CoordinateTransformation = value;

                if (!SkipTransformationPropagation)
                {
                    var layers = GetSnapshot();

                    foreach (var layer in layers.OfType<Layer>())
                        layer.CoordinateTransformation = value;
                }
            }
        }

        /// <summary>
        /// Certain Transformations cannot be inverted in ProjNet, in those cases use this property to set the reverse <see cref="GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation"/> (of CoordinateTransformation) to fetch data from Datasource
        /// 
        /// If your CoordinateTransformation can be inverted you can leave this property to null
        /// </summary>
        public override ICoordinateTransformation ReverseCoordinateTransformation
        {
            get { return base.ReverseCoordinateTransformation; }
            set
            {
                base.ReverseCoordinateTransformation = value;

                if (!SkipTransformationPropagation)
                {
                    var layers = GetSnapshot();

                    foreach (var layer in layers.OfType<Layer>())
                        layer.ReverseCoordinateTransformation = value;
                }
            }
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// Propogation to child layers is dependent on <see cref="LayerGroup.SkipTransformationPropagation"/>
        /// Changes to SRID with propogation enabled will cause both <see cref="CoordinateTransformation"/> and <see cref="ReverseCoordinateTransformation"/> to be reset
        /// </summary>
        public override int SRID
        {
            get { return base.SRID; }
            set
            {
                base.SRID = value;
                if (!SkipTransformationPropagation)
                {
                    var layers = GetSnapshot();

                    foreach (var layer in layers.OfType<Layer>())
                        layer.SRID = value;
                }
            }
        }

        /// <summary>
        /// The target spatial reference id
        /// Propogation to child layers is dependent on <see cref="LayerGroup.SkipTransformationPropagation"/>
        /// Changes to TargetSRID with propogation enabled will cause both <see cref="CoordinateTransformation"/> and <see cref="ReverseCoordinateTransformation"/> to be reset
        /// </summary>
        public override int TargetSRID
        {
            get { return base.TargetSRID; }
            set
            {
                base.TargetSRID = value;
                if (!SkipTransformationPropagation)
                {
                    var layers = GetSnapshot();

                    foreach (var layer in layers.OfType<Layer>())
                        layer.TargetSRID = value;
                }
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the object
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            var layers = GetSnapshot();
            foreach (var layer in layers.OfType<IDisposable>().Where(layer => layer != null))
                layer.Dispose();
            
            Layers.Clear();
            base.ReleaseManagedResources();
        }

        #endregion

        /// <summary>
        /// Returns a layer by its name
        /// </summary>
        /// <param name="name">Name of layer</param>
        /// <returns>Layer</returns>
        public virtual ILayer GetLayerByName(string name)
        {
            var layers = GetSnapshot();

            return layers.FirstOrDefault(t => String.Equals(t.LayerName, name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, MapViewport map)
        {
            var layers = GetSnapshot();
            var compare = VisibilityUnits == VisibilityUnits.ZoomLevel 
                ? map.Zoom 
                : map.GetMapScale((int)g.DpiX);

            foreach (var layer in layers)
            {
                if (layer.Enabled && layer.MaxVisible >= compare &&
                    layer.MinVisible < compare)
                    layer.Render(g, map);
            }
        }

         #region Implementation of ICanQueryLayer

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public virtual void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            var layers = GetSnapshot();

            foreach (var layer in layers.OfType<ICanQueryLayer>())
            {
                layer.ExecuteIntersectionQuery(box, ds);
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public virtual void ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds)
        {
            var layers = GetSnapshot();

            foreach (var layer in layers.OfType<ICanQueryLayer>())
            {
                layer.ExecuteIntersectionQuery(geometry, ds);
            }
        }

         #endregion

        /// <summary>
        /// Create an empty new LayerGroup instance.
        /// </summary>
        /// <remarks>This is used by the Clone() method, inheritors must override this method.</remarks>
        /// <returns>Returns an empty LayerGroup.</returns>
        protected virtual LayerGroup CreateUninitializedInstance()
        {
            return new LayerGroup(LayerName);
        }

        /// <summary>
        /// Returns a cloned copy of the group.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            var clonedGroup = CreateUninitializedInstance();

            clonedGroup.Enabled = Enabled;
            clonedGroup.IsQueryEnabled = IsQueryEnabled;
            clonedGroup.MaxVisible = MaxVisible;
            clonedGroup.VisibilityUnits = VisibilityUnits;
            clonedGroup.MinVisible = MinVisible;
            clonedGroup.Proj4Projection = Proj4Projection;
            clonedGroup.Style = Style;
            // setting SRIDs resets Transformations
            clonedGroup.SRID = SRID;
            clonedGroup.TargetSRID = TargetSRID;
            // do NOT set NULL CoordinateTransformation, as this will cause SRID, SourceFactory, TargetSRID, and TargetFactory to be reset 
            if (CoordinateTransformation != null)
            {
                // restore defined CoordinateTransformation and associated ReverseCoordinateTransformation (causes SRID / TargetSRID to reset appropriately)
                clonedGroup.CoordinateTransformation = CoordinateTransformation;
                clonedGroup.ReverseCoordinateTransformation = ReverseCoordinateTransformation;
            }

            var layers = GetSnapshot();
            foreach (var layer in layers)
            {
                var cloneable = layer as ICloneable;
                if (cloneable != null)
                    clonedGroup.Layers.Add((ILayer) cloneable.Clone());
                else
                    clonedGroup.Layers.Add(layer);
            }

            return clonedGroup;
        }

        private ILayer[] GetSnapshot()
        {
            ILayer[] layers;
            lock (((ICollection)Layers).SyncRoot)
            {
                layers = Layers.ToArray();
            }

            return layers;
        }

        System.Collections.Generic.IList<ILayer> ILayersContainer.Layers
        {
            get { return Layers; }
        }
    }
}
