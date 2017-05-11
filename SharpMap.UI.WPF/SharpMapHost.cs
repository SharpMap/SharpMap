// Copyright 2014-, SharpMapTeam
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

using System.ComponentModel;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace SharpMap.UI.WPF
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms.Integration;
    using System.Windows.Input;

    using GeoAPI.Geometries;

    using Data.Providers;
    using Forms;
    using Layers;
    using Rendering.Decoration;
    using Rendering.Decoration.ScaleBar;

    using KeyEventArgs = KeyEventArgs;
    using MouseEventArgs = MouseEventArgs;

    /// <summary>
    /// Extends WindowsFormsHost and encapsulates SharpMap specific code.
    /// </summary>
    public class SharpMapHost : WindowsFormsHost, INotifyPropertyChanged
    {
        // Dependency Property to store MapLayers.
        public static readonly DependencyProperty MapLayersProperty =
            DependencyProperty.Register("MapLayers", typeof (ObservableCollection<ILayer>), typeof (SharpMapHost), new PropertyMetadata(SetMapLayersCallback));

        // Dependency Property store store BackgroundLayer.
        public static readonly DependencyProperty BackgroundLayerProperty =
            DependencyProperty.Register("BackgroundLayer", typeof (Layer), typeof (SharpMapHost), new PropertyMetadata(SetBackgroundLayerCallback));

        // Dependency Property to store ActiveTool.
        public static readonly DependencyProperty ActiveToolProperty =
            DependencyProperty.Register("ActiveTool", typeof (MapBox.Tools), typeof (SharpMapHost), new PropertyMetadata(SetActiveToolCallback));

        // Dependency Property to store MaxExtent.
        public static readonly DependencyProperty MaxExtentProperty =
            DependencyProperty.Register("MaxExtent", typeof (Envelope), typeof (SharpMapHost), new PropertyMetadata(SetMaxExtentCallback));

        // Dependency Property to store MapExtent.
        public static readonly DependencyProperty MapExtentProperty =
            DependencyProperty.Register("MapExtent", typeof (Envelope), typeof (SharpMapHost), new PropertyMetadata(MapExtentCallback));

        // Dependency Property used when a new geometry is defined.
        public static readonly DependencyProperty DefinedGeometryProperty =
            DependencyProperty.Register("DefinedGeometry", typeof (IGeometry), typeof (SharpMapHost), new PropertyMetadata(GeometryDefinedCallback));

        // Dependency Property used when right click in a MapFeature.
        public static readonly DependencyProperty FeatureRightClickedCommandProperty =
            DependencyProperty.Register("FeatureRightClickedCommand", typeof (ICommand), typeof (SharpMapHost));

        private readonly MapBox _mapBox;

        private VectorLayer _editLayer;

        private GeometryProvider _editLayerGeoProvider;

        private Coordinate _currentMouseCoordinate;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharpMapHost"/> class. 
        /// </summary>
        public SharpMapHost()
        {
            _mapBox = new MapBox{
                BackColor = Color.White,
                Map = new Map{
                    SRID = 900913
                }
            };
            Child = _mapBox;

            MapLayers = new ObservableCollection<ILayer>();

            var scaleBar = new ScaleBar{
                Anchor = MapDecorationAnchor.LeftBottom
            };
            _mapBox.Map.Decorations.Add(scaleBar);
            _mapBox.PanOnClick = false;

            KeyDown += OnKeyDown;

            _mapBox.MouseMove += MapBoxOnMouseMove;
        }

        public ObservableCollection<ILayer> MapLayers
        {
            get
            {
                return (ObservableCollection<ILayer>) GetValue(MapLayersProperty);
            }
            set
            {
                SetValue(MapLayersProperty, value);
            }
        }

        public Layer BackgroundLayer
        {
            get
            {
                return (Layer) GetValue(BackgroundLayerProperty);
            }
            set
            {
                SetValue(BackgroundLayerProperty, value);
            }
        }

        public MapBox.Tools ActiveTool
        {
            get
            {
                return (MapBox.Tools) GetValue(ActiveToolProperty);
            }
            set
            {
                SetValue(ActiveToolProperty, value);
            }
        }

        public string CurrentMouseCoordinateString
        {
            get
            {
                return _currentMouseCoordinate != null ? string.Format("{0:0}, {1:0}", _currentMouseCoordinate.X, _currentMouseCoordinate.Y) : "";
            }
        }

        public Coordinate CurrentMouseCoordinate
        {
            get
            {
                return _currentMouseCoordinate;
            }
        }

        public Envelope MaxExtent
        {
            get
            {
                return (Envelope) GetValue(MaxExtentProperty);
            }

            set
            {
                SetValue(MaxExtentProperty, value);
            }
        }

        public Envelope MapExtent
        {
            get
            {
                return _mapBox.Map.Envelope;
            }

            set
            {
                SetValue(MapExtentProperty, value);
            }
        }

        public IGeometry DefinedGeometry
        {
            get
            {
                return (IGeometry) GetValue(DefinedGeometryProperty);
            }

            set
            {
                SetValue(DefinedGeometryProperty, value);
            }
        }

        /// <summary>
        /// The command that is invoked when a feature is right clicked
        /// </summary>
        public ICommand FeatureRightClickedCommand
        {
            get
            {
                return GetValue(FeatureRightClickedCommandProperty) as ICommand;
            }

            set
            {
                SetValue(FeatureRightClickedCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets called when changes on MapLayers
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">The event arguments</param>
        private static void SetMapLayersCallback(object sender, DependencyPropertyChangedEventArgs args)
        {
            var host = sender as SharpMapHost;
            if (host == null)
            {
                return;
            }

            var oldLayers = args.OldValue as ObservableCollection<ILayer>;
            if (oldLayers != null)
            {
                oldLayers.CollectionChanged -= host.OnMapLayerChanged;
            }

            var newLayers = args.NewValue as ObservableCollection<ILayer>;
            if (newLayers != null)
            {
                newLayers.CollectionChanged += host.OnMapLayerChanged;
            }
        }

        /// <summary>
        /// Gets called when changes on BackgroundLayer
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">The event arguments</param>
        private static void SetBackgroundLayerCallback(object sender, DependencyPropertyChangedEventArgs args)
        {
            var host = sender as SharpMapHost;
            if (host == null)
            {
                return;
            }

            var mapBox = host._mapBox;
            var layer = args.NewValue as Layer;
            if (layer != null)
            {
                mapBox.Map.BackgroundLayer.Add(layer);
            }

            mapBox.Refresh();
        }

        /// <summary>
        /// Gets called when changes on ActiveTool
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">The event arguments</param>
        private static void SetActiveToolCallback(object sender, DependencyPropertyChangedEventArgs args)
        {
            var host = sender as SharpMapHost;
            if (host == null)
            {
                return;
            }

            var mapBox = host._mapBox;
            var newTool = (MapBox.Tools) args.NewValue;
            mapBox.ActiveTool = newTool;
        }

        /// <summary>
        /// Gets called when changes on MaxExtent
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">The event arguments</param>
        private static void SetMaxExtentCallback(object sender, DependencyPropertyChangedEventArgs args)
        {
            var host = sender as SharpMapHost;
            if (host == null)
            {
                return;
            }

            var mapBox = host._mapBox;
            var extent = (Envelope) args.NewValue;
            if (extent != null)
            {
                mapBox.Map.EnforceMaximumExtents = true;
                mapBox.Map.MaximumExtents = extent;
            }
        }

        /// <summary>
        /// Gets called when changes on MapExtent
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">The event arguments</param>
        private static void MapExtentCallback(object sender, DependencyPropertyChangedEventArgs args)
        {
            var host = sender as SharpMapHost;
            if (host == null)
            {
                return;
            }

            var mapBox = host._mapBox;
            var extent = (Envelope) args.NewValue;
            mapBox.Map.ZoomToBox(extent);
            mapBox.Refresh();
        }


        /// <summary>
        /// Gets called when changes on GeometryDefined
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="args">The event arguments</param>
        private static void GeometryDefinedCallback(object sender, DependencyPropertyChangedEventArgs args)
        {
            var host = sender as SharpMapHost;
            if (host == null)
            {
                return;
            }

            if (host._editLayer == null)
            {
                host._editLayer = new VectorLayer("EditLayer");
                host._editLayerGeoProvider = new GeometryProvider(new List<IGeometry>());
                host._editLayer.DataSource = host._editLayerGeoProvider;
                host.MapLayers.Add(host._editLayer);
            }

            host._editLayerGeoProvider.Geometries.Clear();
            var geom = (IGeometry) args.NewValue;
            if (geom != null)
            {
                host._editLayerGeoProvider.Geometries.Add(geom);
            }

            host._mapBox.Refresh();
        }

        /// <summary>
        /// Gets called when keyboard key pressed. Pans the map according to arrow keys.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="keyEventArgs">The event arguments</param>
        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            var currentEnvelope = _mapBox.Map.Envelope;
            var dX = currentEnvelope.Width/2;
            var dY = currentEnvelope.Height/2;

            var x = _mapBox.Map.Center.X;
            var y = _mapBox.Map.Center.Y;

            switch (keyEventArgs.Key)
            {
                case Key.Left:
                    x -= dX;
                    keyEventArgs.Handled = true;
                    break;
                case Key.Right:
                    x += dX;
                    keyEventArgs.Handled = true;
                    break;
                case Key.Up:
                    y += dY;
                    keyEventArgs.Handled = true;
                    break;
                case Key.Down:
                    y -= dY;
                    keyEventArgs.Handled = true;
                    break;
            }

            _mapBox.Map.Center = new Coordinate(x, y);
            _mapBox.Refresh();
        }

        /// <summary>
        /// Gets called when changes in MapLayers
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event arguments</param>
        private void OnMapLayerChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var layers = e.NewItems;
                        if (layers != null)
                        {
                            foreach (var layer in layers)
                            {
                                var castedLayer = layer as ILayer;
                                if (castedLayer != null && _mapBox.Map.Layers.All(l => l.LayerName != castedLayer.LayerName))
                                    _mapBox.Map.Layers.Add(castedLayer);
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var layers = e.OldItems;
                        if (layers != null)
                        {
                            foreach (var layer in layers)
                            {
                                var castedLayer = layer as ILayer;
                                if (castedLayer != null && _mapBox.Map.Layers.Any(l => l.LayerName == castedLayer.LayerName))
                                    _mapBox.Map.Layers.Remove(castedLayer);
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    _mapBox.Map.Layers.Clear();
                    break;
            }

            _mapBox.Refresh();
        }

        public void ZoomToExtents()
        {
            _mapBox.Map.ZoomToExtents();
            _mapBox.Refresh();
        }

        public void ZoomToEnvelope(Envelope env)
        {
            _mapBox.Map.ZoomToBox(env);
            _mapBox.Refresh();
        }

        /// <summary>
        /// Gets called when mouse moves over map
        /// </summary>
        /// <param name="worldPos">The click coordinate</param>
        /// <param name="mouseEventArgs">The event arguments</param>
        private void MapBoxOnMouseMove(Coordinate worldPos, MouseEventArgs mouseEventArgs)
        {
            _currentMouseCoordinate = worldPos;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentMouseCoordinate"));
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentMouseCoordinateString"));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

    }
}
