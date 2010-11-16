using System;
using System.Timers;

namespace SharpMap.Layers
{
    public enum LayerCollectionType
    {
        /// <summary>
        /// Layer collection for layers with datasources that are more or less static
        /// </summary>
        Static,

        /// <summary>
        /// Layer collection for layers with datasources that update frequently
        /// </summary>
        Variable,
    }
    /// <summary>
    /// Signature of function to handle VariableLayerCollectionRequery event
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The argments, <c>EventArgs.Empty</c> in all cases</param>
    public delegate void VariableLayerCollectionRequeryHandler(object sender, EventArgs e);
    
    /// <summary>
    /// Layer collection 
    /// </summary>
    public class VariableLayerCollection : LayerCollection
    {
        private readonly LayerCollection _staticLayers;
        private readonly Timer _timer = new Timer();
        
        /// <summary>
        /// Event fired every <see cref="Interval"/> to force requery;
        /// </summary>
        public event VariableLayerCollectionRequeryHandler VariableLayerCollectionRequery;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="staticLayers">Layer collection that holds static layers</param>
        public VariableLayerCollection(LayerCollection staticLayers)
        {
            _staticLayers = staticLayers;
            _timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            OnRequery();
        }

        protected override void OnInsert(int index, object value)
        {
            ILayer newLayer = (value as ILayer);
            if (newLayer == null) throw new ArgumentNullException("value", "The passed argument is null or not an ILayer");

            TestLayerPresent(_staticLayers, newLayer);

            base.OnInsert(index, value);
        }

        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            if (Count > 0) _timer.Start();
        }

        protected override void OnClearComplete()
        {
            base.OnClearComplete();
            _timer.Stop();
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            if (Count == 0) _timer.Stop();
        }

        private static void TestLayerPresent(LayerCollection layers, ILayer newLayer)
        {
            foreach (ILayer layer in layers)
            {
                int comparison = String.Compare(layer.LayerName,
                                                newLayer.LayerName, StringComparison.CurrentCultureIgnoreCase);

                if (comparison == 0) throw new DuplicateLayerException(newLayer.LayerName);
            }
            
        }

        private void OnRequery()
        {
            if (Count == 0 || Pause) return;
            if (VariableLayerCollectionRequery != null)
                VariableLayerCollectionRequery(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets/sets the interval in which to update layers
        /// </summary>
        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        private bool _pause;
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