using System;
using System.ComponentModel;
using System.Diagnostics;
using Common.Logging;

namespace SharpMap.Forms
{
    [DesignTimeVisible(false)]
    public partial class MapToolStrip : System.Windows.Forms.ToolStrip
    {
        static ILog Logger = LogManager.GetLogger(typeof(MapToolStrip));

        /// <summary>
        /// Event raised when the corresponding map control has changed.
        /// </summary>
        public event EventHandler MapControlChanged;

        /// <summary>
        /// Event raised when the corresponding map control is about to change
        /// </summary>
        public event CancelEventHandler MapControlChanging;

        private MapBox _mapBox;

        protected MapToolStrip()
        {
            Enabled = false;
        }

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

        protected void OnMapControlChanging(CancelEventArgs e)
        {
            if (_mapBox == null)
                return;

            Logger.Info("MapControlChanging");
            if (MapControlChanging != null)
                MapControlChanging(this, e);
            
            if (e.Cancel)
            {
                Logger.Info("Canceled");
                return;
            }

            OnMapControlChangingInternal(e);
        }

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


        protected void OnMapControlChanged(EventArgs e)
        {
            Logger.Info("Enter MapControlChanged");

            Enabled = _mapBox != null;
            OnMapControlChangedInternal(e);

            if (MapControlChanged != null)
                MapControlChanged(this, e);

            Logger.Info("Leave MapControlChanged");


        }

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

        private void OnMapCenterChanged(GeoAPI.Geometries.Coordinate geoPoint)
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
