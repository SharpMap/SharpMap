using System;
using System.ComponentModel;
using System.Diagnostics;
using Common.Logging;

namespace SharpMap.Forms
{
    
    /// <summary>
    /// Map tool
    /// </summary>
    [DesignTimeVisible(false)]
    public partial class MapToolStrip : System.Windows.Forms.ToolStrip
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MapToolStrip));

        /// <summary>
        /// Event raised when the corresponding map control has changed.
        /// </summary>
        public event EventHandler MapControlChanged;

        /// <summary>
        /// Event raised when the corresponding map control is about to change
        /// </summary>
        public event CancelEventHandler MapControlChanging;

        /// <summary>
        /// Static constructor
        /// </summary>
        static MapToolStrip()
        {
            Map.Configure();
        }


        private MapBox _mapBox;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        protected MapToolStrip()
        {
            Enabled = false;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="container">A container for components</param>
        protected MapToolStrip(IContainer container)
        {
            container.Add(this);
            Enabled = false;
        }

        /// <summary>
        /// Gets or sets the control whose properties are currently being managed.
        /// </summary>
        public MapBox MapControl
        {
            get
            {
                return _mapBox;
            }
            set
            {
                if (value != _mapBox)
                {
                    var cea = new CancelEventArgs();
                    OnMapControlChanging(cea);
                    if (cea.Cancel) return;
                    
                    _mapBox = value;
                    OnMapControlChanged(EventArgs.Empty);
                }
            }
        }

        #region Event Invocation

        /// <summary>
        /// Event invoker for the <see cref="MapControlChanging"/> event.
        /// Calls <see cref="OnMapControlChangingInternal"/>, too, if
        /// <see cref="CancelEventArgs.Cancel"/> is <c>false</c> after handling of event.
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected void OnMapControlChanging(CancelEventArgs e)
        {
            if (_mapBox == null)
                return;

            _logger.Info("MapControlChanging");
            MapControlChanging?.Invoke(this, e);
            
            if (e.Cancel)
            {
                _logger.Info("Canceled");
                return;
            }

            OnMapControlChangingInternal(e);
        }

        /// <summary>
        /// Overrideable function to release a map control from this tool-strip, if possible.
        /// </summary>
        /// <param name="e">The event arguments. If removing MapControl is not possible <see cref="CancelEventArgs.Cancel"/> must be set to <c>true</c>.</param>
        protected virtual void OnMapControlChangingInternal(CancelEventArgs e)
        {
            
            //_mapBox.MapQueried -= OnMapQueried;
            //_mapBox.MapRefreshed -= OnMapRefreshed;
            //_mapBox.MapZoomChanged -= OnMapZoomChanged;
            //_mapBox.MapZooming -= OnMapZoomChanging;
            //_mapBox.MapCenterChanged -= OnMapCenterChanged;
            //_mapBox.ActiveToolChanged -= OnActiveToolChanged;

            //_mapBox.Map.LayersChanged -= OnMapLayersChanged;
        }


        /// <summary>
        /// Event invoker for the <see cref="MapControlChanged"/> event.
        /// Calls <see cref="OnMapControlChangedInternal"/>, too.
        /// </summary>
        /// <param name="e"></param>
        protected void OnMapControlChanged(EventArgs e)
        {
            _logger.Info("Enter MapControlChanged");

            Enabled = _mapBox != null;
            OnMapControlChangedInternal(e);

            MapControlChanged?.Invoke(this, e);

            _logger.Info("Leave MapControlChanged");


        }

        /// <summary>
        /// Overrideable function to connect a map control to this tool-strip.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnMapControlChangedInternal(EventArgs e)
        {
            //_mapBox.MapQueried -= OnMapQueried;
            //_mapBox.MapRefreshed -= OnMapRefreshed;
            //_mapBox.MapZoomChanged -= OnMapZoomChanged;
            //_mapBox.MapZooming -= OnMapZoomChanging;
            //_mapBox.MapCenterChanged -= OnMapCenterChanged;
            //_mapBox.ActiveToolChanged -= OnActiveToolChanged;

            //_mapBox.Map.LayersChanged -= OnMapLayersChanged;
        }

        /// <summary>
        /// Method to set the active map tool from a button
        /// </summary>
        /// <param name="btn">A button</param>
        /// <param name="associtatedTool">The associated tool</param>
        protected virtual void TrySetActiveTool(System.Windows.Forms.ToolStripButton btn, MapBox.Tools associtatedTool)
        {
            var isChecked = btn.Checked;

            Debug.WriteLine("Trying to {0} active tool '{1}' by {2}.", isChecked ? "set" : "unset", associtatedTool, btn.Name);
            //if (Logger.IsDebugEnabled)
            //  Logger.DebugFormat("Trying to {0} active tool '{1}' by ", isChecked ? "set" : "unset", associtatedTool, btn.Name);

            if (isChecked && MapControl.ActiveTool == associtatedTool)
            {
                Debug.WriteLine(" ... not needed");
                return;
            }

            if (!isChecked && MapControl.ActiveTool == associtatedTool)
            {
                MapControl.ActiveTool = MapBox.Tools.None;
                Debug.WriteLine(" ... done");
                return;
            }

            if (isChecked && MapControl.ActiveTool != associtatedTool)
            {
                MapControl.ActiveTool = associtatedTool;
                Debug.WriteLine(" ... finally done");
            }


        }

        
        #endregion

        /*
        #region Event Handling

        private void OnMapRefreshed(object sender, EventArgs empty)
        {
            //What is to do here?
        }

        private void OnMapQueried(Data.FeatureDataTable queryResults)
        {
            //What is to do here?
        }

        private void OnMapCenterChanged(NetTopologySuite.Geometries.Coordinate geoPoint)
        {
        }
        
        private void OnMapZoomChanged(double zoom)
        {
            //Update the current scale
        }

        private void OnMapZoomChanging(double zoom)
        {
            //What to do here?
        }

        private void OnActiveToolChanged(MapBox.Tools tool)
        {
            //Pick apropriate symbol
        }

        private void OnMapLayersChanged()
        {
            var queryableLayers = new Dictionary<int, SharpMap.Layers.ILayer>();
            var i = 0;
            foreach (var layer in _mapBox.Map.Layers)
            {
                if (layer is SharpMap.Layers.ICanQueryLayer)
                    queryableLayers.Add(i, layer);
                i++;
            }


        }


        #endregion
         */
    }
}
