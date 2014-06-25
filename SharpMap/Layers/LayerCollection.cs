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
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SharpMap.Layers
{
    /// <summary>
    /// A collection of <see cref="ILayer"/> instances.
    /// </summary>
    [Serializable]
    public class LayerCollection : System.ComponentModel.BindingList<ILayer>, INotifyCollectionChanged
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
                lock (this)
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
        }

        /// <summary>
        /// Returns a cloned copy of the LayerCollection
        /// </summary>
        /// <remarks>
        /// The layer instances are the same as in the original collection, however if a layer implements ICloneable this could not be true.
        /// </remarks>
        /// <returns></returns>
        public LayerCollection Clone()
        {
            lock (this)
            {
                var newColl = new LayerCollection();
                foreach (ILayer lay in this)
                {
                    var cloneable = lay as ICloneable;
                    if (cloneable != null)
                        newColl.Add((ILayer) cloneable.Clone());
                    else
                        newColl.Add(lay);
                }
                return newColl;
            }
        }

        /// <summary>
        /// Method to add all layers of <paramref name="other"/> to this collection
        /// </summary>
        /// <param name="other">A collection of layers</param>
        public void AddCollection(LayerCollection other)
        {
            lock (this)
            {
                foreach (var lay in other)
                {
                    Add(lay);
                }
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
        public new void Insert(int index, ILayer layer)
        {
            lock (this)
            {
                if (index > Count || index < 0)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Index not in range");
                }

                InsertItem(index, layer);
            }
        }

        /// <inheritdoc/>
        protected override void OnAddingNew(System.ComponentModel.AddingNewEventArgs e)
        {
            ILayer newLayer = (e.NewObject as ILayer);
            if (newLayer == null) throw new ArgumentNullException("value", "The passed argument is null or not an ILayer");

            lock (this)
            {
                foreach (ILayer layer in this)
                {
                    int comparison = String.Compare(layer.LayerName,
                                                    newLayer.LayerName, StringComparison.CurrentCultureIgnoreCase);

                    if (comparison == 0) throw new DuplicateLayerException(newLayer.LayerName);
                }
            }

            base.OnAddingNew(new System.ComponentModel.AddingNewEventArgs(newLayer));
        }

        /// <summary>
        /// Function to search for a layer in this collection by its name
        /// </summary>
        /// <param name="layerName">The name of the layer to search for</param>
        /// <returns>The layer if found, otherwise <value>null</value></returns>
        public ILayer GetLayerByName(string layerName)
        {
            lock (this)
            {
                LayerCollection lays = this;
                return GetLayerByNameInternal(layerName, lays);
            }
        }

        private static ILayer GetLayerByNameInternal(string layerName, System.Collections.Generic.IEnumerable<ILayer> lays)
        {
            foreach (ILayer layer in lays)
            {
                int comparison = String.Compare(layer.LayerName,
                                                layerName, StringComparison.CurrentCultureIgnoreCase);

                if (comparison == 0) return layer;

                //If this is a layergroup, check sublayers also
                if (layer is LayerGroup)
                {
                    LayerGroup lg = layer as LayerGroup;
                    ILayer lay = GetLayerByNameInternal(layerName, lg.Layers);
                    if (lay != null)
                        return lay;
                }
            }

            return null;
        }

        /// <summary>
        /// Removes all layers from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var layer in Items)
            {
                var asyncLayer = layer as ITileAsyncLayer;
                if (asyncLayer != null) asyncLayer.Cancel();
            }
            base.ClearItems();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Fires the CollectionChanged event.
        /// </summary>
        /// <param name="e">Event to fire-</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }

        protected override void InsertItem(int index, ILayer item)
        {
            base.InsertItem(index, item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            var removedItem = this[index];
            base.RemoveItem(index);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        protected override void SetItem(int index, ILayer item)
        {
            var oldItem = this[index];

            base.SetItem(index, item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }
    }
}