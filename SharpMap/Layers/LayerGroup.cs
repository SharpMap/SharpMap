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
using SharpMap.Data;
using GeoAPI.Geometries;
using SharpMap.Styles;

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
    public class LayerGroup : Layer, ICanQueryLayer
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
        public bool IsQueryEnabled
        {
            get { return _isQueryEnabled; }
            set { _isQueryEnabled = value; }
        }
        /// <summary>
        /// Sublayers in the group
        /// </summary>
        public Collection<Layer> Layers
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
                if (Layers.Count == 0)
                    return null;
                var bbox = new Envelope(Layers[0].Envelope);
                for (int i = 1; i < Layers.Count; i++)
                    bbox.ExpandToInclude(Layers[i].Envelope);
                return bbox;
            }
        }

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
        public Layer GetLayerByName(string name)
        {
            //return _Layers.Find( delegate(SharpMap.Layers.Layer layer) { return layer.LayerName.Equals(name); });

            for (int i = 0; i < _layers.Count; i++)
                if (String.Equals(_layers[i].LayerName, name, StringComparison.InvariantCultureIgnoreCase))
                    return _layers[i];

            return null;
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            for (int i = 0; i < _layers.Count; i++)
                if (_layers[i].Enabled && _layers[i].MaxVisible >= map.Zoom && _layers[i].MinVisible < map.Zoom)
                    _layers[i].Render(g, map);
        }

         #region Implementation of ICanQueryLayer

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            foreach (Layer layer in Layers)
            {
                if (layer is ICanQueryLayer)
                {
                    FeatureDataSet dsTmp = new FeatureDataSet();
                    ((ICanQueryLayer)layer).ExecuteIntersectionQuery(box, dsTmp);
                    ds.Tables.AddRange(dsTmp.Tables.ToArray());
                }
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds)
        {
            foreach (Layer layer in Layers)
            {
                if (layer is ICanQueryLayer)
                {
                    FeatureDataSet dsTmp = new FeatureDataSet();
                    ((ICanQueryLayer)layer).ExecuteIntersectionQuery(geometry, dsTmp);
                    ds.Tables.AddRange(dsTmp.Tables.ToArray());
                }
            }
        }

         #endregion
    }
}