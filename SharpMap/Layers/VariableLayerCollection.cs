using System;
using System.Collections.Generic;
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
        /// Layer collection for layers are completely opaque and use as Background (e.g. WMS, OSM)
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
        private readonly LayerCollection _staticLayers;
        private static Timer _timer = null;

        private static bool touchTest = false;
        
        /// <summary>
        /// Method to restart the internal Timer
        /// </summary>
        public static void TouchTimer()
        {
            if (touchTest == true)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OnRequery));
                _timer.Start();
            }
            else
            {
                touchTest = true;
            }
        }
        
        /// <summary>
        /// Event fired every <see cref="Interval"/> to force requery;
        /// </summary>
        public static event VariableLayerCollectionRequeryHandler VariableLayerCollectionRequery;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="staticLayers">Layer collection that holds static layers</param>
        public VariableLayerCollection(LayerCollection staticLayers)
        {
            _staticLayers = staticLayers;
            if (_timer == null)
            {
                _timer = new Timer();
                _timer.Interval = 500;
                _timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            }
        }

        static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            touchTest = false;
            OnRequery(null);
            if (touchTest == false)
            {
                _timer.Stop();
                touchTest = true;
            }
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, ILayer layer)
        {
            if (layer == null) 
                throw new ArgumentNullException("layer", "The passed argument is null or not an ILayer");

            TestLayerPresent(_staticLayers, layer);
            base.InsertItem(index, layer);
        }

        /*
        protected override void OnAddingNew(System.ComponentModel.AddingNewEventArgs e)
        {
            ILayer newLayer = (e.NewObject as ILayer);
            if (newLayer == null) throw new ArgumentNullException("value", "The passed argument is null or not an ILayer");

            TestLayerPresent(_staticLayers, newLayer);

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

        private static void OnRequery(object obj)
        {
            if (Pause) return;
            if (VariableLayerCollectionRequery != null)
                VariableLayerCollectionRequery(null, EventArgs.Empty);
        }

        /// <summary>
        /// Gets/sets the interval in which to update layers
        /// </summary>
        public static double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        private static bool _pause;
        /// <summary>
        /// Gets/Sets whether this collection should currently be updated or not
        /// </summary>
        public static bool Pause
        {
            get { return _pause; }
            set { _pause = value; }
        }
    }
}