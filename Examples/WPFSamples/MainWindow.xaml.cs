using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using GeoAPI.Geometries;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Controls.MenuItem;

namespace WPFSamples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var gss = GeoAPI.GeometryServiceProvider.Instance;
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new ProjNet.CoordinateSystems.CoordinateSystemFactory(),
                new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

            GeoAPI.GeometryServiceProvider.Instance = gss;
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);

        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BgOSM_OnClick(object sender, RoutedEventArgs e)
        {
            WpfMap.BackgroundLayer = new SharpMap.Layers.TileAsyncLayer(BruTile.Predefined.KnownTileSources.Create(), "OSM");

            foreach (var menuItem in Menu.Items.OfType<MenuItem>())
            {
                menuItem.IsChecked = false;
            }
            BgOsm.IsChecked = true;

            WpfMap.ZoomToExtents();
            e.Handled = true;
        }

        private void BgStamenWaterColor_Click(object sender, RoutedEventArgs e)
        {
            WpfMap.BackgroundLayer = new SharpMap.Layers.TileAsyncLayer(
              BruTile.Predefined.KnownTileSources.Create(
                BruTile.Predefined.KnownTileSource.StamenWatercolor), "Stamen Watercolor");

            foreach (var menuItem in Menu.Items.OfType<MenuItem>())
            {
                menuItem.IsChecked = false;
            }
            BgStamenWaterColor.IsChecked = true;

            WpfMap.ZoomToExtents();
            e.Handled = true;
        }

        private void AddShapeLayer_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = @"Shapefiles (*.shp)|*.shp";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var ds = new SharpMap.Data.Providers.ShapeFile(ofd.FileName);
                var lay = new SharpMap.Layers.VectorLayer(System.IO.Path.GetFileNameWithoutExtension(ofd.FileName), ds);
                if (ds.CoordinateSystem != null)
                {
                    GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformationFactory fact =
                        new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

                    lay.CoordinateTransformation = fact.CreateFromCoordinateSystems(ds.CoordinateSystem,
                        ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator);
                    lay.ReverseCoordinateTransformation = fact.CreateFromCoordinateSystems(ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator,
                        ds.CoordinateSystem);
                }
                WpfMap.MapLayers.Add(lay);
                if (WpfMap.MapLayers.Count == 1)
                {
                    Envelope env = lay.Envelope;
                    WpfMap.ZoomToEnvelope(env);
                }
            }
            e.Handled = true;
        }
    }
}
