// Copyright 2007 - Christian Gräfe (SharpMap@SharpTools.de)
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

namespace SharpMap.Layers
{
    /// <summary>
    /// A collection of <see cref="ILayer"/> instances.
    /// </summary>
    public class LayerCollection : System.ComponentModel.BindingList<ILayer>
    {

        /// <summary>
        /// Gets or sets the layer with the given <paramref name="layerName"/>.
        /// </summary>
        /// <param name="layerName">
        /// Name of the layer to replace, if it exists.
        /// </param>
        public virtual ILayer this[string layerName]
        {
            get { return GetLayerByName(layerName); }
            set
            {
                for (int i = 0; i < Count; i++)
                {
                    int comparison = String.Compare(this[i].LayerName,
                                                    layerName, StringComparison.CurrentCultureIgnoreCase);

                    if (comparison == 0)
                    {
                        this[i] = value;
                        return;
                    }
                }

                Add(value);
            }
        }


        /// <summary>
        /// Inserts the layer at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index at which to add the layer.</param>
        /// <param name="layer">The layer to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="index"/> is less than 0 or is 
        /// greater or equal to <see cref="CollectionBase.Count"/>.
        /// </exception>
        public void Insert(int index, ILayer layer)
        {
            if (index > Count || index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "Index not in range");
            }


            //Items.Insert(index, layer);
            //Raise the Add New Event
            base.InsertItem(0, layer);

        }

        protected override void  OnAddingNew(System.ComponentModel.AddingNewEventArgs e)
        {
            ILayer newLayer = (e.NewObject as ILayer);
            if (newLayer == null) throw new ArgumentNullException("value","The passed argument is null or not an ILayer");

            foreach (ILayer layer in this)
            {
                int comparison = String.Compare(layer.LayerName,
                                                newLayer.LayerName, StringComparison.CurrentCultureIgnoreCase);

                if (comparison == 0) throw new DuplicateLayerException(newLayer.LayerName);
            }

            base.OnAddingNew(new System.ComponentModel.AddingNewEventArgs(newLayer));
        }

        private ILayer GetLayerByName(string layerName)
        {
            foreach (ILayer layer in this)
            {
                int comparison = String.Compare(layer.LayerName,
                                                layerName, StringComparison.CurrentCultureIgnoreCase);

                if (comparison == 0) return layer;
            }

            return null;
        }

    }
}