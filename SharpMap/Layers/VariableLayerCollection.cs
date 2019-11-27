using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Timers;

namespace SharpMap.Layers
{
    /// <summary>
    /// Types of layer collections
    /// </summary>
    public enum LayerCollectionType
    {
        /// <summary>
        /// Layer collection for layers with datasources that are more or less static (e.g ShapeFiles)
        /// </summary>
        Static,

        /// <summary>
        /// Layer collection for layers with datasources that update frequently (e.g. moving vehicle)
        /// </summary>
        Variable,

        /// <summary>
        /// Layer collection for layers are completely opaque and serve as Background (e.g. WMS, OSM)
        /// </summary>
        Background,
    }
    /// <summary>
    /// Signature of function to handle VariableLayerCollectionRequery event
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The arguments, <c>EventArgs.Empty</c> in all cases</param>
    public delegate void VariableLayerCollectionRequeryHandler(object sender, EventArgs e);

    /// <summary>
    /// Layer collection 
    /// </summary>
    /// TODO:REEVALUEATE
    [Serializable]
    public class VariableLayerCollection : LayerCollection
    {
        private readonly LayerCollection _variableLayers;
        
        [NonSerialized]
        private Timer _timer = null;

        private volatile bool _isQuerying;

        [NonSerialized]
        private EventHandler<ElapsedEventArgs> _handler;

        private bool _pause;

        /// <summary>
        /// Method to restart the internal Timer
        /// </summary>
        public void TouchTimer()
        {
            // check for pending re-draw (eg after map pan/zoom completed)
            //if (_timer.Enabled) return;
            if (_isQuerying) return;

            //_timer.Start();
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OnRequery));
        }

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            //_timer = new Timer();
        }

        /// <summary>
        /// Event fired every <see cref="Interval"/> to force requery;
        /// </summary>
        public event VariableLayerCollectionRequeryHandler VariableLayerCollectionRequery;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="variableLayers">Layer collection that holds layers with data sources updating frequently</param>
        public VariableLayerCollection(LayerCollection variableLayers)
        {
            _variableLayers = variableLayers;
            if (_handler == null)
            {
                //_timer = new Timer();
                //_timer.Interval = 500;
                //_timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            OnRequery(null);
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, ILayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException("layer", "The passed argument is null or not an ILayer");

            lock (((ICollection)_variableLayers).SyncRoot)
            {
                TestLayerPresent(_variableLayers, layer);
                base.InsertItem(index, layer);
            }
        }

        /*
        protected override void OnAddingNew(System.ComponentModel.AddingNewEventArgs e)
        {
            ILayer newLayer = (e.NewObject as ILayer);
            if (newLayer == null) throw new ArgumentNullException("value", "The passed argument is null or not an ILayer");

            TestLayerPresent(_variableLayers, newLayer);

            base.OnAddingNew(e);
        }
         */


        private static void TestLayerPresent(IEnumerable<ILayer> layers, ILayer newLayer)
        {
            foreach (var layer in layers)
            {
                var comparison = String.Compare(layer.LayerName,
                                                newLayer.LayerName, StringComparison.CurrentCultureIgnoreCase);

                if (comparison == 0) throw new DuplicateLayerException(newLayer.LayerName);
            }

        }

        private void OnRequery(object obj)
        {
            // if pan/zoom operation in progress then retry on next _timer.Elapsed
            if (Pause) return;

            // check for race condition when timer has been stopped while event has just been submitted on threadpool.QueueUserWorkItem
            //if (!_timer.Enabled) return;
            if (_isQuerying) return;

            //_timer.Stop();

            _isQuerying = true;
            VariableLayerCollectionRequery?.Invoke(this, EventArgs.Empty);
            _isQuerying = false;
        }

        /// <summary>
        /// Gets/sets the interval in which to update layers
        /// </summary>
        public double Interval
        {
            get
            {
                return 0;//_timer.Interval;
            }
            set
            {
                // map sets Interval == 0 when disposing, to prevent race condition
                /*
                if (value <= 0)
                    _timer.Stop();
                else
                    _timer.Interval = value;
                 */
            }
        }

        /// <summary>
        /// Gets/Sets whether this collection should currently be updated or not
        /// </summary>
        public bool Pause
        {
            get { return _pause; }
            set { _pause = value; }
        }
    }
}
