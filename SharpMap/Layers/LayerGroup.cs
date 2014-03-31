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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using SharpMap.Data;
using GeoAPI.Geometries;
#if DotSpatialProjections
using ICoordinateTransformation = DotSpatial.Projections.ICoordinateTransformation;
#else
using GeoAPI.CoordinateSystems.Transformations;
#endif

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
    public partial class LayerGroup : Layer, ICanQueryLayer
    {
        private Collection<Layer> _layers;
        private bool _isQueryEnabled = true;
        /// <summary>
        /// Initializes a new group layer
        /// </summary>
        /// <param name="layername">Name of layer</param>
        public LayerGroup(string layername)
        {
            LayerName = layername;
            _layers = new Collection<Layer>();
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
        public virtual Collection<Layer> Layers
        {
            get { return _layers; }
            set { _layers = value; }
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
                for (int i = 0; i < Layers.Count; i++)
                {
                    var layerEnvelope = Layers[i].Envelope;
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
        public override ICoordinateTransformation CoordinateTransformation
        {
            get { return base.CoordinateTransformation; }
            set
            {
                base.CoordinateTransformation = value;

                if (!SkipTransformationPropagation)
                {
                    var layers = Layers.ToArray();

                    foreach (var layer in layers)
                        layer.CoordinateTransformation = value;
                }
            }
        }

#if !DotSpatialProjections
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
                    var layers = Layers.ToArray();

                    foreach (var layer in layers)
                        layer.ReverseCoordinateTransformation = value;
                }
            }
        }
#endif
        #region IDisposable Members

        /// <summary>
        /// Disposes the object
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            foreach (var layer in Layers)
                if (layer != null)
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
        public virtual Layer GetLayerByName(string name)
        {
            //return _Layers.Find( delegate(SharpMap.Layers.Layer layer) { return layer.LayerName.Equals(name); });
            var layers = Layers.ToArray();

            return layers.FirstOrDefault(t => String.Equals(t.LayerName, name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            var layers = Layers.ToArray();

            foreach (var layer in layers)
            {
                if (layer.Enabled && layer.MaxVisible >= map.Zoom &&
                    layer.MinVisible < map.Zoom)
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
            var layers = Layers.ToArray();

            foreach (var layer in layers.OfType<ICanQueryLayer>())
            {
                var dsTmp = new FeatureDataSet();
                layer.ExecuteIntersectionQuery(box, dsTmp);
                ds.Tables.AddRange(dsTmp.Tables.ToArray());
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public virtual void ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds)
        {
            var layers = Layers.ToArray();

            foreach (var layer in layers.OfType<ICanQueryLayer>())
            {
                var dsTmp = new FeatureDataSet();
                layer.ExecuteIntersectionQuery(geometry, dsTmp);
                ds.Tables.AddRange(dsTmp.Tables.ToArray());
            }
        }

         #endregion
    }
}