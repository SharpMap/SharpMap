using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Decoration;
using SharpMap.Rendering.Decoration.ScaleBar;
using Unit = SharpMap.Rendering.Decoration.ScaleBar.Unit;

namespace UnitTests.Rendering.Decoration.ScaleBar
{
    [TestFixture]
    public class ScaleBarTests
    {
        private Map _map;

        //private readonly List<int> _testLatitudes = new List<int> {0,5,10,15,20,25,30,35,40,45,50,55,60,65,70,75,80,85};
        private readonly List<int> _testLatitudes = new List<int> {0, 10, 20, 30, 40, 50, 60, 70, 80};

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //System.IO.Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "BruTileCache", "Osm"));

//            var files = System.IO.Directory.GetFiles(Path.Combine(Path.GetTempPath(), "SharpMap"));
//            foreach (var file in files)
//                System.IO.File.Delete(file);

            var gss = new NtsGeometryServices();
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new CoordinateSystemFactory(),
                new CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

            // plug-in WebMercator so that correct spherical definition is directly available to Layer Transformations using SRID
            var pcs = (ProjectedCoordinateSystem) ProjectedCoordinateSystem.WebMercator;
            css.AddCoordinateSystem((int) pcs.AuthorityCode, pcs);

            GeoAPI.GeometryServiceProvider.Instance = gss;
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;

            _map = new Map
            {
                Size = new System.Drawing.Size(1440, 1080),
                BackColor = System.Drawing.Color.LemonChiffon,
                SRID = 3857
            };

            var lyr = new VectorLayer("Test Data", CreateScaleBarDataSource())
            {
                SRID = 4326,
                TargetSRID = _map.SRID,
                CacheExtent = false
            };

            _map.Layers.Add(lyr);

            // Add bru-tile map background 
//            var cacheFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BruTileCache", "Osm");
//            var lyrBruTile = new TileLayer(
//                BruTile.Predefined.KnownTileSources.Create(BruTile.Predefined.KnownTileSource.OpenStreetMap),
//                "Tiles", Color.Transparent, true, cacheFolder)
//            {
//                SRID = 3857,
//                TargetSRID = _map.SRID
//            };
//            _map.BackgroundLayer.Add(lyrBruTile);

            var scaleBar = new SharpMap.Rendering.Decoration.ScaleBar.ScaleBar()
            {
                MapUnit = (int) Unit.Meter,
                BarUnitLargeScale = (int) Unit.Kilometer,
                BarUnitSmallScale = (int) Unit.Meter,
                Anchor = MapDecorationAnchor.Center,
                BarStyle = ScaleBarStyle.Meridian,
                BarColor1 = Color.Transparent,
                BarColor2 = Color.DodgerBlue,
                BarWidth = 20
            };
            _map.Decorations.Add(scaleBar);

            _map.ZoomToExtents();
        }

        private GeometryFeatureProvider CreateScaleBarDataSource()
        {
            var fdt = new FeatureDataTable {TableName = "ScaleBarTests"};
            fdt.Columns.Add(new DataColumn("oid", typeof(uint)));
            fdt.Columns[0].AutoIncrement = true;

            fdt.Columns.Add(new DataColumn("Latitude", typeof(int))); // start latitude for filtering
            fdt.Columns.Add(new DataColumn("Interval", typeof(int))); // distance interval for filtering

            fdt.PrimaryKey = new[] {fdt.Columns[0]};

            LoadData(fdt);

            return new GeometryFeatureProvider(fdt);
        }

        private void LoadData(FeatureDataTable fdt)
        {
            fdt.BeginLoadData();
            LoadVincentyData(fdt);
            fdt.EndLoadData();
        }

        private void LoadVincentyData(FeatureDataTable fdt)
        {
            //Common Meridian = 99deg East
            //Points generated using Vincenty calcs (direct) west (270deg) and east (90deg)
            //at intervals of 100, 1000, 10000m (limited to +/3 degrees of common meridian)
            //CentreLat and Interval columns provided for filtering FeatureDataSet in SharpMap

            //Longitude,Latitude,Interval,Multiple
            AddRecord(fdt, 99.00000000, 0.00000000, 0, 0);
            AddRecord(fdt, 98.99910168, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99820337, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99730505, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99640674, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99550842, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99461011, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99371179, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99281348, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99191516, 0.00000000, 0, 100);
            AddRecord(fdt, 98.99101685, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.98203369, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.97305054, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.96406739, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.95508424, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.94610108, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.93711793, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.92813478, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.91915162, 0.00000000, 0, 1000);
            AddRecord(fdt, 98.91016847, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.82033694, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.73050541, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.64067389, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.55084236, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.46101083, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.37117930, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.28134777, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.19151624, 0.00000000, 0, 10000);
            AddRecord(fdt, 98.10168472, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.00089832, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00179663, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00269495, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00359326, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00449158, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00538989, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00628821, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00718652, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00808484, 0.00000000, 0, 100);
            AddRecord(fdt, 99.00898315, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.01796631, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.02694946, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.03593261, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.04491576, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.05389892, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.06288207, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.07186522, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.08084838, 0.00000000, 0, 1000);
            AddRecord(fdt, 99.08983153, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.17966306, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.26949459, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.35932611, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.44915764, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.53898917, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.62882070, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.71865223, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.80848376, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.89831528, 0.00000000, 0, 10000);
            AddRecord(fdt, 99.00000000, 5.00000000, 5, 0);
            AddRecord(fdt, 98.99909828, 5.00000000, 5, 100);
            AddRecord(fdt, 98.99819655, 5.00000000, 5, 100);
            AddRecord(fdt, 98.99729483, 4.99999999, 5, 100);
            AddRecord(fdt, 98.99639310, 4.99999999, 5, 100);
            AddRecord(fdt, 98.99549138, 4.99999998, 5, 100);
            AddRecord(fdt, 98.99458966, 4.99999998, 5, 100);
            AddRecord(fdt, 98.99368793, 4.99999997, 5, 100);
            AddRecord(fdt, 98.99278621, 4.99999996, 5, 100);
            AddRecord(fdt, 98.99188449, 4.99999995, 5, 100);
            AddRecord(fdt, 98.99098276, 4.99999994, 5, 1000);
            AddRecord(fdt, 98.98196552, 4.99999975, 5, 1000);
            AddRecord(fdt, 98.97294829, 4.99999944, 5, 1000);
            AddRecord(fdt, 98.96393105, 4.99999901, 5, 1000);
            AddRecord(fdt, 98.95491381, 4.99999845, 5, 1000);
            AddRecord(fdt, 98.94589657, 4.99999777, 5, 1000);
            AddRecord(fdt, 98.93687934, 4.99999696, 5, 1000);
            AddRecord(fdt, 98.92786210, 4.99999603, 5, 1000);
            AddRecord(fdt, 98.91884486, 4.99999498, 5, 1000);
            AddRecord(fdt, 98.90982762, 4.99999380, 5, 10000);
            AddRecord(fdt, 98.81965525, 4.99997519, 5, 10000);
            AddRecord(fdt, 98.72948288, 4.99994418, 5, 10000);
            AddRecord(fdt, 98.63931053, 4.99990077, 5, 10000);
            AddRecord(fdt, 98.54913818, 4.99984495, 5, 10000);
            AddRecord(fdt, 98.45896586, 4.99977673, 5, 10000);
            AddRecord(fdt, 98.36879355, 4.99969611, 5, 10000);
            AddRecord(fdt, 98.27862127, 4.99960308, 5, 10000);
            AddRecord(fdt, 98.18844901, 4.99949765, 5, 10000);
            AddRecord(fdt, 98.09827679, 4.99937982, 5, 10000);
            AddRecord(fdt, 99.00090172, 5.00000000, 5, 100);
            AddRecord(fdt, 99.00180345, 5.00000000, 5, 100);
            AddRecord(fdt, 99.00270517, 4.99999999, 5, 100);
            AddRecord(fdt, 99.00360690, 4.99999999, 5, 100);
            AddRecord(fdt, 99.00450862, 4.99999998, 5, 100);
            AddRecord(fdt, 99.00541034, 4.99999998, 5, 100);
            AddRecord(fdt, 99.00631207, 4.99999997, 5, 100);
            AddRecord(fdt, 99.00721379, 4.99999996, 5, 100);
            AddRecord(fdt, 99.00811551, 4.99999995, 5, 100);
            AddRecord(fdt, 99.00901724, 4.99999994, 5, 1000);
            AddRecord(fdt, 99.01803448, 4.99999975, 5, 1000);
            AddRecord(fdt, 99.02705171, 4.99999944, 5, 1000);
            AddRecord(fdt, 99.03606895, 4.99999901, 5, 1000);
            AddRecord(fdt, 99.04508619, 4.99999845, 5, 1000);
            AddRecord(fdt, 99.05410343, 4.99999777, 5, 1000);
            AddRecord(fdt, 99.06312066, 4.99999696, 5, 1000);
            AddRecord(fdt, 99.07213790, 4.99999603, 5, 1000);
            AddRecord(fdt, 99.08115514, 4.99999498, 5, 1000);
            AddRecord(fdt, 99.09017238, 4.99999380, 5, 10000);
            AddRecord(fdt, 99.18034475, 4.99997519, 5, 10000);
            AddRecord(fdt, 99.27051712, 4.99994418, 5, 10000);
            AddRecord(fdt, 99.36068947, 4.99990077, 5, 10000);
            AddRecord(fdt, 99.45086182, 4.99984495, 5, 10000);
            AddRecord(fdt, 99.54103414, 4.99977673, 5, 10000);
            AddRecord(fdt, 99.63120645, 4.99969611, 5, 10000);
            AddRecord(fdt, 99.72137873, 4.99960308, 5, 10000);
            AddRecord(fdt, 99.81155099, 4.99949765, 5, 10000);
            AddRecord(fdt, 99.90172321, 4.99937982, 5, 10000);
            AddRecord(fdt, 99.00000000, 10.00000000, 10, 0);
            AddRecord(fdt, 98.99908792, 10.00000000, 10, 100);
            AddRecord(fdt, 98.99817584, 10.00000000, 10, 100);
            AddRecord(fdt, 98.99726376, 9.99999999, 10, 100);
            AddRecord(fdt, 98.99635168, 9.99999998, 10, 100);
            AddRecord(fdt, 98.99543959, 9.99999997, 10, 100);
            AddRecord(fdt, 98.99452751, 9.99999996, 10, 100);
            AddRecord(fdt, 98.99361543, 9.99999994, 10, 100);
            AddRecord(fdt, 98.99270335, 9.99999992, 10, 100);
            AddRecord(fdt, 98.99179127, 9.99999990, 10, 100);
            AddRecord(fdt, 98.99087919, 9.99999988, 10, 1000);
            AddRecord(fdt, 98.98175838, 9.99999950, 10, 1000);
            AddRecord(fdt, 98.97263756, 9.99999888, 10, 1000);
            AddRecord(fdt, 98.96351675, 9.99999800, 10, 1000);
            AddRecord(fdt, 98.95439594, 9.99999688, 10, 1000);
            AddRecord(fdt, 98.94527513, 9.99999550, 10, 1000);
            AddRecord(fdt, 98.93615432, 9.99999388, 10, 1000);
            AddRecord(fdt, 98.92703351, 9.99999200, 10, 1000);
            AddRecord(fdt, 98.91791270, 9.99998988, 10, 1000);
            AddRecord(fdt, 98.90879188, 9.99998750, 10, 10000);
            AddRecord(fdt, 98.81758378, 9.99995002, 10, 10000);
            AddRecord(fdt, 98.72637571, 9.99988754, 10, 10000);
            AddRecord(fdt, 98.63516768, 9.99980007, 10, 10000);
            AddRecord(fdt, 98.54395970, 9.99968761, 10, 10000);
            AddRecord(fdt, 98.45275180, 9.99955015, 10, 10000);
            AddRecord(fdt, 98.36154397, 9.99938771, 10, 10000);
            AddRecord(fdt, 98.27033625, 9.99920028, 10, 10000);
            AddRecord(fdt, 98.17912864, 9.99898786, 10, 10000);
            AddRecord(fdt, 98.08792115, 9.99875044, 10, 10000);
            AddRecord(fdt, 99.00091208, 10.00000000, 10, 100);
            AddRecord(fdt, 99.00182416, 10.00000000, 10, 100);
            AddRecord(fdt, 99.00273624, 9.99999999, 10, 100);
            AddRecord(fdt, 99.00364832, 9.99999998, 10, 100);
            AddRecord(fdt, 99.00456041, 9.99999997, 10, 100);
            AddRecord(fdt, 99.00547249, 9.99999996, 10, 100);
            AddRecord(fdt, 99.00638457, 9.99999994, 10, 100);
            AddRecord(fdt, 99.00729665, 9.99999992, 10, 100);
            AddRecord(fdt, 99.00820873, 9.99999990, 10, 100);
            AddRecord(fdt, 99.00912081, 9.99999988, 10, 1000);
            AddRecord(fdt, 99.01824162, 9.99999950, 10, 1000);
            AddRecord(fdt, 99.02736244, 9.99999888, 10, 1000);
            AddRecord(fdt, 99.03648325, 9.99999800, 10, 1000);
            AddRecord(fdt, 99.04560406, 9.99999688, 10, 1000);
            AddRecord(fdt, 99.05472487, 9.99999550, 10, 1000);
            AddRecord(fdt, 99.06384568, 9.99999388, 10, 1000);
            AddRecord(fdt, 99.07296649, 9.99999200, 10, 1000);
            AddRecord(fdt, 99.08208730, 9.99998988, 10, 1000);
            AddRecord(fdt, 99.09120812, 9.99998750, 10, 10000);
            AddRecord(fdt, 99.18241622, 9.99995002, 10, 10000);
            AddRecord(fdt, 99.27362429, 9.99988754, 10, 10000);
            AddRecord(fdt, 99.36483232, 9.99980007, 10, 10000);
            AddRecord(fdt, 99.45604030, 9.99968761, 10, 10000);
            AddRecord(fdt, 99.54724820, 9.99955015, 10, 10000);
            AddRecord(fdt, 99.63845603, 9.99938771, 10, 10000);
            AddRecord(fdt, 99.72966375, 9.99920028, 10, 10000);
            AddRecord(fdt, 99.82087136, 9.99898786, 10, 10000);
            AddRecord(fdt, 99.91207885, 9.99875044, 10, 10000);
            AddRecord(fdt, 99.00000000, 15.00000000, 15, 0);
            AddRecord(fdt, 98.99907020, 15.00000000, 15, 100);
            AddRecord(fdt, 98.99814041, 14.99999999, 15, 100);
            AddRecord(fdt, 98.99721061, 14.99999998, 15, 100);
            AddRecord(fdt, 98.99628082, 14.99999997, 15, 100);
            AddRecord(fdt, 98.99535102, 14.99999995, 15, 100);
            AddRecord(fdt, 98.99442122, 14.99999993, 15, 100);
            AddRecord(fdt, 98.99349143, 14.99999991, 15, 100);
            AddRecord(fdt, 98.99256163, 14.99999988, 15, 100);
            AddRecord(fdt, 98.99163184, 14.99999985, 15, 100);
            AddRecord(fdt, 98.99070204, 14.99999981, 15, 1000);
            AddRecord(fdt, 98.98140408, 14.99999924, 15, 1000);
            AddRecord(fdt, 98.97210612, 14.99999829, 15, 1000);
            AddRecord(fdt, 98.96280817, 14.99999696, 15, 1000);
            AddRecord(fdt, 98.95351021, 14.99999526, 15, 1000);
            AddRecord(fdt, 98.94421225, 14.99999317, 15, 1000);
            AddRecord(fdt, 98.93491429, 14.99999070, 15, 1000);
            AddRecord(fdt, 98.92561633, 14.99998785, 15, 1000);
            AddRecord(fdt, 98.91631838, 14.99998463, 15, 1000);
            AddRecord(fdt, 98.90702042, 14.99998102, 15, 10000);
            AddRecord(fdt, 98.81404087, 14.99992408, 15, 10000);
            AddRecord(fdt, 98.72106139, 14.99982918, 15, 10000);
            AddRecord(fdt, 98.62808200, 14.99969633, 15, 10000);
            AddRecord(fdt, 98.53510275, 14.99952552, 15, 10000);
            AddRecord(fdt, 98.44212366, 14.99931674, 15, 10000);
            AddRecord(fdt, 98.34914477, 14.99907002, 15, 10000);
            AddRecord(fdt, 98.25616610, 14.99878533, 15, 10000);
            AddRecord(fdt, 98.16318770, 14.99846269, 15, 10000);
            AddRecord(fdt, 98.07020960, 14.99810210, 15, 10000);
            AddRecord(fdt, 99.00092980, 15.00000000, 15, 100);
            AddRecord(fdt, 99.00185959, 14.99999999, 15, 100);
            AddRecord(fdt, 99.00278939, 14.99999998, 15, 100);
            AddRecord(fdt, 99.00371918, 14.99999997, 15, 100);
            AddRecord(fdt, 99.00464898, 14.99999995, 15, 100);
            AddRecord(fdt, 99.00557878, 14.99999993, 15, 100);
            AddRecord(fdt, 99.00650857, 14.99999991, 15, 100);
            AddRecord(fdt, 99.00743837, 14.99999988, 15, 100);
            AddRecord(fdt, 99.00836816, 14.99999985, 15, 100);
            AddRecord(fdt, 99.00929796, 14.99999981, 15, 1000);
            AddRecord(fdt, 99.01859592, 14.99999924, 15, 1000);
            AddRecord(fdt, 99.02789388, 14.99999829, 15, 1000);
            AddRecord(fdt, 99.03719183, 14.99999696, 15, 1000);
            AddRecord(fdt, 99.04648979, 14.99999526, 15, 1000);
            AddRecord(fdt, 99.05578775, 14.99999317, 15, 1000);
            AddRecord(fdt, 99.06508571, 14.99999070, 15, 1000);
            AddRecord(fdt, 99.07438367, 14.99998785, 15, 1000);
            AddRecord(fdt, 99.08368162, 14.99998463, 15, 1000);
            AddRecord(fdt, 99.09297958, 14.99998102, 15, 10000);
            AddRecord(fdt, 99.18595913, 14.99992408, 15, 10000);
            AddRecord(fdt, 99.27893861, 14.99982918, 15, 10000);
            AddRecord(fdt, 99.37191800, 14.99969633, 15, 10000);
            AddRecord(fdt, 99.46489725, 14.99952552, 15, 10000);
            AddRecord(fdt, 99.55787634, 14.99931674, 15, 10000);
            AddRecord(fdt, 99.65085523, 14.99907002, 15, 10000);
            AddRecord(fdt, 99.74383390, 14.99878533, 15, 10000);
            AddRecord(fdt, 99.83681230, 14.99846269, 15, 10000);
            AddRecord(fdt, 99.92979040, 14.99810210, 15, 10000);
            AddRecord(fdt, 99.00000000, 20.00000000, 20, 0);
            AddRecord(fdt, 98.99904441, 20.00000000, 20, 100);
            AddRecord(fdt, 98.99808881, 19.99999999, 20, 100);
            AddRecord(fdt, 98.99713322, 19.99999998, 20, 100);
            AddRecord(fdt, 98.99617763, 19.99999996, 20, 100);
            AddRecord(fdt, 98.99522204, 19.99999994, 20, 100);
            AddRecord(fdt, 98.99426644, 19.99999991, 20, 100);
            AddRecord(fdt, 98.99331085, 19.99999987, 20, 100);
            AddRecord(fdt, 98.99235526, 19.99999984, 20, 100);
            AddRecord(fdt, 98.99139966, 19.99999979, 20, 100);
            AddRecord(fdt, 98.99044407, 19.99999974, 20, 1000);
            AddRecord(fdt, 98.98088814, 19.99999897, 20, 1000);
            AddRecord(fdt, 98.97133222, 19.99999768, 20, 1000);
            AddRecord(fdt, 98.96177629, 19.99999588, 20, 1000);
            AddRecord(fdt, 98.95222036, 19.99999356, 20, 1000);
            AddRecord(fdt, 98.94266444, 19.99999073, 20, 1000);
            AddRecord(fdt, 98.93310851, 19.99998738, 20, 1000);
            AddRecord(fdt, 98.92355258, 19.99998351, 20, 1000);
            AddRecord(fdt, 98.91399666, 19.99997913, 20, 1000);
            AddRecord(fdt, 98.90444073, 19.99997424, 20, 10000);
            AddRecord(fdt, 98.80888153, 19.99989695, 20, 10000);
            AddRecord(fdt, 98.71332245, 19.99976813, 20, 10000);
            AddRecord(fdt, 98.61776355, 19.99958778, 20, 10000);
            AddRecord(fdt, 98.52220491, 19.99935591, 20, 10000);
            AddRecord(fdt, 98.42664657, 19.99907252, 20, 10000);
            AddRecord(fdt, 98.33108861, 19.99873760, 20, 10000);
            AddRecord(fdt, 98.23553108, 19.99835116, 20, 10000);
            AddRecord(fdt, 98.13997405, 19.99791319, 20, 10000);
            AddRecord(fdt, 98.04441759, 19.99742371, 20, 10000);
            AddRecord(fdt, 99.00095559, 20.00000000, 20, 100);
            AddRecord(fdt, 99.00191119, 19.99999999, 20, 100);
            AddRecord(fdt, 99.00286678, 19.99999998, 20, 100);
            AddRecord(fdt, 99.00382237, 19.99999996, 20, 100);
            AddRecord(fdt, 99.00477796, 19.99999994, 20, 100);
            AddRecord(fdt, 99.00573356, 19.99999991, 20, 100);
            AddRecord(fdt, 99.00668915, 19.99999987, 20, 100);
            AddRecord(fdt, 99.00764474, 19.99999984, 20, 100);
            AddRecord(fdt, 99.00860034, 19.99999979, 20, 100);
            AddRecord(fdt, 99.00955593, 19.99999974, 20, 1000);
            AddRecord(fdt, 99.01911186, 19.99999897, 20, 1000);
            AddRecord(fdt, 99.02866778, 19.99999768, 20, 1000);
            AddRecord(fdt, 99.03822371, 19.99999588, 20, 1000);
            AddRecord(fdt, 99.04777964, 19.99999356, 20, 1000);
            AddRecord(fdt, 99.05733556, 19.99999073, 20, 1000);
            AddRecord(fdt, 99.06689149, 19.99998738, 20, 1000);
            AddRecord(fdt, 99.07644742, 19.99998351, 20, 1000);
            AddRecord(fdt, 99.08600334, 19.99997913, 20, 1000);
            AddRecord(fdt, 99.09555927, 19.99997424, 20, 10000);
            AddRecord(fdt, 99.19111847, 19.99989695, 20, 10000);
            AddRecord(fdt, 99.28667755, 19.99976813, 20, 10000);
            AddRecord(fdt, 99.38223645, 19.99958778, 20, 10000);
            AddRecord(fdt, 99.47779509, 19.99935591, 20, 10000);
            AddRecord(fdt, 99.57335343, 19.99907252, 20, 10000);
            AddRecord(fdt, 99.66891139, 19.99873760, 20, 10000);
            AddRecord(fdt, 99.76446892, 19.99835116, 20, 10000);
            AddRecord(fdt, 99.86002595, 19.99791319, 20, 10000);
            AddRecord(fdt, 99.95558241, 19.99742371, 20, 10000);
            AddRecord(fdt, 99.00000000, 25.00000000, 25, 0);
            AddRecord(fdt, 98.99900941, 25.00000000, 25, 100);
            AddRecord(fdt, 98.99801882, 24.99999999, 25, 100);
            AddRecord(fdt, 98.99702823, 24.99999997, 25, 100);
            AddRecord(fdt, 98.99603765, 24.99999995, 25, 100);
            AddRecord(fdt, 98.99504706, 24.99999992, 25, 100);
            AddRecord(fdt, 98.99405647, 24.99999988, 25, 100);
            AddRecord(fdt, 98.99306588, 24.99999984, 25, 100);
            AddRecord(fdt, 98.99207529, 24.99999979, 25, 100);
            AddRecord(fdt, 98.99108470, 24.99999973, 25, 100);
            AddRecord(fdt, 98.99009411, 24.99999967, 25, 1000);
            AddRecord(fdt, 98.98018823, 24.99999868, 25, 1000);
            AddRecord(fdt, 98.97028235, 24.99999703, 25, 1000);
            AddRecord(fdt, 98.96037646, 24.99999472, 25, 1000);
            AddRecord(fdt, 98.95047058, 24.99999175, 25, 1000);
            AddRecord(fdt, 98.94056469, 24.99998813, 25, 1000);
            AddRecord(fdt, 98.93065881, 24.99998384, 25, 1000);
            AddRecord(fdt, 98.92075293, 24.99997889, 25, 1000);
            AddRecord(fdt, 98.91084705, 24.99997329, 25, 1000);
            AddRecord(fdt, 98.90094117, 24.99996702, 25, 10000);
            AddRecord(fdt, 98.80188244, 24.99986808, 25, 10000);
            AddRecord(fdt, 98.70282392, 24.99970318, 25, 10000);
            AddRecord(fdt, 98.60376572, 24.99947232, 25, 10000);
            AddRecord(fdt, 98.50470795, 24.99917550, 25, 10000);
            AddRecord(fdt, 98.40565070, 24.99881272, 25, 10000);
            AddRecord(fdt, 98.30659409, 24.99838399, 25, 10000);
            AddRecord(fdt, 98.20753821, 24.99788930, 25, 10000);
            AddRecord(fdt, 98.10848319, 24.99732866, 25, 10000);
            AddRecord(fdt, 98.00942911, 24.99670207, 25, 10000);
            AddRecord(fdt, 99.00099059, 25.00000000, 25, 100);
            AddRecord(fdt, 99.00198118, 24.99999999, 25, 100);
            AddRecord(fdt, 99.00297177, 24.99999997, 25, 100);
            AddRecord(fdt, 99.00396235, 24.99999995, 25, 100);
            AddRecord(fdt, 99.00495294, 24.99999992, 25, 100);
            AddRecord(fdt, 99.00594353, 24.99999988, 25, 100);
            AddRecord(fdt, 99.00693412, 24.99999984, 25, 100);
            AddRecord(fdt, 99.00792471, 24.99999979, 25, 100);
            AddRecord(fdt, 99.00891530, 24.99999973, 25, 100);
            AddRecord(fdt, 99.00990589, 24.99999967, 25, 1000);
            AddRecord(fdt, 99.01981177, 24.99999868, 25, 1000);
            AddRecord(fdt, 99.02971765, 24.99999703, 25, 1000);
            AddRecord(fdt, 99.03962354, 24.99999472, 25, 1000);
            AddRecord(fdt, 99.04952942, 24.99999175, 25, 1000);
            AddRecord(fdt, 99.05943531, 24.99998813, 25, 1000);
            AddRecord(fdt, 99.06934119, 24.99998384, 25, 1000);
            AddRecord(fdt, 99.07924707, 24.99997889, 25, 1000);
            AddRecord(fdt, 99.08915295, 24.99997329, 25, 1000);
            AddRecord(fdt, 99.09905883, 24.99996702, 25, 10000);
            AddRecord(fdt, 99.19811756, 24.99986808, 25, 10000);
            AddRecord(fdt, 99.29717608, 24.99970318, 25, 10000);
            AddRecord(fdt, 99.39623428, 24.99947232, 25, 10000);
            AddRecord(fdt, 99.49529205, 24.99917550, 25, 10000);
            AddRecord(fdt, 99.59434930, 24.99881272, 25, 10000);
            AddRecord(fdt, 99.69340591, 24.99838399, 25, 10000);
            AddRecord(fdt, 99.79246179, 24.99788930, 25, 10000);
            AddRecord(fdt, 99.89151681, 24.99732866, 25, 10000);
            AddRecord(fdt, 99.99057089, 24.99670207, 25, 10000);
            AddRecord(fdt, 99.00000000, 30.00000000, 30, 0);
            AddRecord(fdt, 98.99896358, 30.00000000, 30, 100);
            AddRecord(fdt, 98.99792717, 29.99999998, 30, 100);
            AddRecord(fdt, 98.99689075, 29.99999996, 30, 100);
            AddRecord(fdt, 98.99585433, 29.99999993, 30, 100);
            AddRecord(fdt, 98.99481792, 29.99999990, 30, 100);
            AddRecord(fdt, 98.99378150, 29.99999985, 30, 100);
            AddRecord(fdt, 98.99274508, 29.99999980, 30, 100);
            AddRecord(fdt, 98.99170867, 29.99999974, 30, 100);
            AddRecord(fdt, 98.99067225, 29.99999967, 30, 100);
            AddRecord(fdt, 98.98963583, 29.99999959, 30, 1000);
            AddRecord(fdt, 98.97927166, 29.99999837, 30, 1000);
            AddRecord(fdt, 98.96890750, 29.99999633, 30, 1000);
            AddRecord(fdt, 98.95854333, 29.99999347, 30, 1000);
            AddRecord(fdt, 98.94817916, 29.99998980, 30, 1000);
            AddRecord(fdt, 98.93781500, 29.99998531, 30, 1000);
            AddRecord(fdt, 98.92745084, 29.99998001, 30, 1000);
            AddRecord(fdt, 98.91708667, 29.99997389, 30, 1000);
            AddRecord(fdt, 98.90672251, 29.99996696, 30, 1000);
            AddRecord(fdt, 98.89635835, 29.99995921, 30, 10000);
            AddRecord(fdt, 98.79271687, 29.99983682, 30, 10000);
            AddRecord(fdt, 98.68907573, 29.99963285, 30, 10000);
            AddRecord(fdt, 98.58543510, 29.99934728, 30, 10000);
            AddRecord(fdt, 98.48179514, 29.99898014, 30, 10000);
            AddRecord(fdt, 98.37815604, 29.99853140, 30, 10000);
            AddRecord(fdt, 98.27451795, 29.99800109, 30, 10000);
            AddRecord(fdt, 98.17088104, 29.99738919, 30, 10000);
            AddRecord(fdt, 98.06724550, 29.99669572, 30, 10000);
            AddRecord(fdt, 97.96361148, 29.99592067, 30, 10000);
            AddRecord(fdt, 99.00103642, 30.00000000, 30, 100);
            AddRecord(fdt, 99.00207283, 29.99999998, 30, 100);
            AddRecord(fdt, 99.00310925, 29.99999996, 30, 100);
            AddRecord(fdt, 99.00414567, 29.99999993, 30, 100);
            AddRecord(fdt, 99.00518208, 29.99999990, 30, 100);
            AddRecord(fdt, 99.00621850, 29.99999985, 30, 100);
            AddRecord(fdt, 99.00725492, 29.99999980, 30, 100);
            AddRecord(fdt, 99.00829133, 29.99999974, 30, 100);
            AddRecord(fdt, 99.00932775, 29.99999967, 30, 100);
            AddRecord(fdt, 99.01036417, 29.99999959, 30, 1000);
            AddRecord(fdt, 99.02072834, 29.99999837, 30, 1000);
            AddRecord(fdt, 99.03109250, 29.99999633, 30, 1000);
            AddRecord(fdt, 99.04145667, 29.99999347, 30, 1000);
            AddRecord(fdt, 99.05182084, 29.99998980, 30, 1000);
            AddRecord(fdt, 99.06218500, 29.99998531, 30, 1000);
            AddRecord(fdt, 99.07254916, 29.99998001, 30, 1000);
            AddRecord(fdt, 99.08291333, 29.99997389, 30, 1000);
            AddRecord(fdt, 99.09327749, 29.99996696, 30, 1000);
            AddRecord(fdt, 99.10364165, 29.99995921, 30, 10000);
            AddRecord(fdt, 99.20728313, 29.99983682, 30, 10000);
            AddRecord(fdt, 99.31092427, 29.99963285, 30, 10000);
            AddRecord(fdt, 99.41456490, 29.99934728, 30, 10000);
            AddRecord(fdt, 99.51820486, 29.99898014, 30, 10000);
            AddRecord(fdt, 99.62184396, 29.99853140, 30, 10000);
            AddRecord(fdt, 99.72548205, 29.99800109, 30, 10000);
            AddRecord(fdt, 99.82911896, 29.99738919, 30, 10000);
            AddRecord(fdt, 99.93275450, 29.99669572, 30, 10000);
            AddRecord(fdt, 100.03638852, 29.99592067, 30, 10000);
            AddRecord(fdt, 99.00000000, 35.00000000, 35, 0);
            AddRecord(fdt, 98.99890457, 35.00000000, 35, 100);
            AddRecord(fdt, 98.99780914, 34.99999998, 35, 100);
            AddRecord(fdt, 98.99671370, 34.99999996, 35, 100);
            AddRecord(fdt, 98.99561827, 34.99999992, 35, 100);
            AddRecord(fdt, 98.99452284, 34.99999988, 35, 100);
            AddRecord(fdt, 98.99342741, 34.99999982, 35, 100);
            AddRecord(fdt, 98.99233197, 34.99999976, 35, 100);
            AddRecord(fdt, 98.99123654, 34.99999968, 35, 100);
            AddRecord(fdt, 98.99014111, 34.99999960, 35, 100);
            AddRecord(fdt, 98.98904568, 34.99999951, 35, 1000);
            AddRecord(fdt, 98.97809136, 34.99999802, 35, 1000);
            AddRecord(fdt, 98.96713704, 34.99999555, 35, 1000);
            AddRecord(fdt, 98.95618272, 34.99999209, 35, 1000);
            AddRecord(fdt, 98.94522840, 34.99998764, 35, 1000);
            AddRecord(fdt, 98.93427408, 34.99998221, 35, 1000);
            AddRecord(fdt, 98.92331976, 34.99997578, 35, 1000);
            AddRecord(fdt, 98.91236545, 34.99996837, 35, 1000);
            AddRecord(fdt, 98.90141113, 34.99995997, 35, 1000);
            AddRecord(fdt, 98.89045682, 34.99995058, 35, 10000);
            AddRecord(fdt, 98.78091391, 34.99980231, 35, 10000);
            AddRecord(fdt, 98.67137153, 34.99955519, 35, 10000);
            AddRecord(fdt, 98.56182993, 34.99920923, 35, 10000);
            AddRecord(fdt, 98.45228939, 34.99876443, 35, 10000);
            AddRecord(fdt, 98.34275017, 34.99822079, 35, 10000);
            AddRecord(fdt, 98.23321252, 34.99757831, 35, 10000);
            AddRecord(fdt, 98.12367673, 34.99683700, 35, 10000);
            AddRecord(fdt, 98.01414303, 34.99599686, 35, 10000);
            AddRecord(fdt, 97.90461171, 34.99505790, 35, 10000);
            AddRecord(fdt, 99.00109543, 35.00000000, 35, 100);
            AddRecord(fdt, 99.00219086, 34.99999998, 35, 100);
            AddRecord(fdt, 99.00328630, 34.99999996, 35, 100);
            AddRecord(fdt, 99.00438173, 34.99999992, 35, 100);
            AddRecord(fdt, 99.00547716, 34.99999988, 35, 100);
            AddRecord(fdt, 99.00657259, 34.99999982, 35, 100);
            AddRecord(fdt, 99.00766803, 34.99999976, 35, 100);
            AddRecord(fdt, 99.00876346, 34.99999968, 35, 100);
            AddRecord(fdt, 99.00985889, 34.99999960, 35, 100);
            AddRecord(fdt, 99.01095432, 34.99999951, 35, 1000);
            AddRecord(fdt, 99.02190864, 34.99999802, 35, 1000);
            AddRecord(fdt, 99.03286296, 34.99999555, 35, 1000);
            AddRecord(fdt, 99.04381728, 34.99999209, 35, 1000);
            AddRecord(fdt, 99.05477160, 34.99998764, 35, 1000);
            AddRecord(fdt, 99.06572592, 34.99998221, 35, 1000);
            AddRecord(fdt, 99.07668024, 34.99997578, 35, 1000);
            AddRecord(fdt, 99.08763455, 34.99996837, 35, 1000);
            AddRecord(fdt, 99.09858887, 34.99995997, 35, 1000);
            AddRecord(fdt, 99.10954318, 34.99995058, 35, 10000);
            AddRecord(fdt, 99.21908609, 34.99980231, 35, 10000);
            AddRecord(fdt, 99.32862847, 34.99955519, 35, 10000);
            AddRecord(fdt, 99.43817007, 34.99920923, 35, 10000);
            AddRecord(fdt, 99.54771061, 34.99876443, 35, 10000);
            AddRecord(fdt, 99.65724983, 34.99822079, 35, 10000);
            AddRecord(fdt, 99.76678748, 34.99757831, 35, 10000);
            AddRecord(fdt, 99.87632327, 34.99683700, 35, 10000);
            AddRecord(fdt, 99.98585697, 34.99599686, 35, 10000);
            AddRecord(fdt, 100.09538829, 34.99505790, 35, 10000);
            AddRecord(fdt, 99.00000000, 40.00000000, 40, 0);
            AddRecord(fdt, 98.99882896, 39.99999999, 40, 100);
            AddRecord(fdt, 98.99765791, 39.99999998, 40, 100);
            AddRecord(fdt, 98.99648687, 39.99999995, 40, 100);
            AddRecord(fdt, 98.99531582, 39.99999991, 40, 100);
            AddRecord(fdt, 98.99414478, 39.99999985, 40, 100);
            AddRecord(fdt, 98.99297373, 39.99999979, 40, 100);
            AddRecord(fdt, 98.99180269, 39.99999971, 40, 100);
            AddRecord(fdt, 98.99063164, 39.99999962, 40, 100);
            AddRecord(fdt, 98.98946060, 39.99999952, 40, 100);
            AddRecord(fdt, 98.98828956, 39.99999941, 40, 1000);
            AddRecord(fdt, 98.97657911, 39.99999763, 40, 1000);
            AddRecord(fdt, 98.96486867, 39.99999468, 40, 1000);
            AddRecord(fdt, 98.95315823, 39.99999053, 40, 1000);
            AddRecord(fdt, 98.94144779, 39.99998521, 40, 1000);
            AddRecord(fdt, 98.92973735, 39.99997870, 40, 1000);
            AddRecord(fdt, 98.91802691, 39.99997101, 40, 1000);
            AddRecord(fdt, 98.90631648, 39.99996214, 40, 1000);
            AddRecord(fdt, 98.89460605, 39.99995208, 40, 1000);
            AddRecord(fdt, 98.88289563, 39.99994084, 40, 10000);
            AddRecord(fdt, 98.76579165, 39.99976336, 40, 10000);
            AddRecord(fdt, 98.64868849, 39.99946756, 40, 10000);
            AddRecord(fdt, 98.53158654, 39.99905345, 40, 10000);
            AddRecord(fdt, 98.41448621, 39.99852102, 40, 10000);
            AddRecord(fdt, 98.29738790, 39.99787028, 40, 10000);
            AddRecord(fdt, 98.18029201, 39.99710124, 40, 10000);
            AddRecord(fdt, 98.06319895, 39.99621390, 40, 10000);
            AddRecord(fdt, 97.94610913, 39.99520827, 40, 10000);
            AddRecord(fdt, 97.82902294, 39.99408435, 40, 10000);
            AddRecord(fdt, 99.00117104, 39.99999999, 40, 100);
            AddRecord(fdt, 99.00234209, 39.99999998, 40, 100);
            AddRecord(fdt, 99.00351313, 39.99999995, 40, 100);
            AddRecord(fdt, 99.00468418, 39.99999991, 40, 100);
            AddRecord(fdt, 99.00585522, 39.99999985, 40, 100);
            AddRecord(fdt, 99.00702627, 39.99999979, 40, 100);
            AddRecord(fdt, 99.00819731, 39.99999971, 40, 100);
            AddRecord(fdt, 99.00936836, 39.99999962, 40, 100);
            AddRecord(fdt, 99.01053940, 39.99999952, 40, 100);
            AddRecord(fdt, 99.01171044, 39.99999941, 40, 1000);
            AddRecord(fdt, 99.02342089, 39.99999763, 40, 1000);
            AddRecord(fdt, 99.03513133, 39.99999468, 40, 1000);
            AddRecord(fdt, 99.04684177, 39.99999053, 40, 1000);
            AddRecord(fdt, 99.05855221, 39.99998521, 40, 1000);
            AddRecord(fdt, 99.07026265, 39.99997870, 40, 1000);
            AddRecord(fdt, 99.08197309, 39.99997101, 40, 1000);
            AddRecord(fdt, 99.09368352, 39.99996214, 40, 1000);
            AddRecord(fdt, 99.10539395, 39.99995208, 40, 1000);
            AddRecord(fdt, 99.11710437, 39.99994084, 40, 10000);
            AddRecord(fdt, 99.23420835, 39.99976336, 40, 10000);
            AddRecord(fdt, 99.35131151, 39.99946756, 40, 10000);
            AddRecord(fdt, 99.46841346, 39.99905345, 40, 10000);
            AddRecord(fdt, 99.58551379, 39.99852102, 40, 10000);
            AddRecord(fdt, 99.70261210, 39.99787028, 40, 10000);
            AddRecord(fdt, 99.81970799, 39.99710124, 40, 10000);
            AddRecord(fdt, 99.93680105, 39.99621390, 40, 10000);
            AddRecord(fdt, 100.05389087, 39.99520827, 40, 10000);
            AddRecord(fdt, 100.17097706, 39.99408435, 40, 10000);
            AddRecord(fdt, 99.00000000, 45.00000000, 45, 0);
            AddRecord(fdt, 98.99873172, 44.99999999, 45, 100);
            AddRecord(fdt, 98.99746344, 44.99999997, 45, 100);
            AddRecord(fdt, 98.99619515, 44.99999994, 45, 100);
            AddRecord(fdt, 98.99492687, 44.99999989, 45, 100);
            AddRecord(fdt, 98.99365859, 44.99999982, 45, 100);
            AddRecord(fdt, 98.99239031, 44.99999975, 45, 100);
            AddRecord(fdt, 98.99112203, 44.99999965, 45, 100);
            AddRecord(fdt, 98.98985375, 44.99999955, 45, 100);
            AddRecord(fdt, 98.98858546, 44.99999943, 45, 100);
            AddRecord(fdt, 98.98731718, 44.99999930, 45, 1000);
            AddRecord(fdt, 98.97463437, 44.99999718, 45, 1000);
            AddRecord(fdt, 98.96195155, 44.99999366, 45, 1000);
            AddRecord(fdt, 98.94926874, 44.99998873, 45, 1000);
            AddRecord(fdt, 98.93658593, 44.99998239, 45, 1000);
            AddRecord(fdt, 98.92390312, 44.99997465, 45, 1000);
            AddRecord(fdt, 98.91122031, 44.99996549, 45, 1000);
            AddRecord(fdt, 98.89853752, 44.99995493, 45, 1000);
            AddRecord(fdt, 98.88585472, 44.99994296, 45, 1000);
            AddRecord(fdt, 98.87317193, 44.99992958, 45, 10000);
            AddRecord(fdt, 98.74634448, 44.99971831, 45, 10000);
            AddRecord(fdt, 98.61951828, 44.99936620, 45, 10000);
            AddRecord(fdt, 98.49269394, 44.99887326, 45, 10000);
            AddRecord(fdt, 98.36587208, 44.99823948, 45, 10000);
            AddRecord(fdt, 98.23905334, 44.99746487, 45, 10000);
            AddRecord(fdt, 98.11223832, 44.99654945, 45, 10000);
            AddRecord(fdt, 97.98542764, 44.99549321, 45, 10000);
            AddRecord(fdt, 97.85862194, 44.99429617, 45, 10000);
            AddRecord(fdt, 97.73182183, 44.99295835, 45, 10000);
            AddRecord(fdt, 99.00126828, 44.99999999, 45, 100);
            AddRecord(fdt, 99.00253656, 44.99999997, 45, 100);
            AddRecord(fdt, 99.00380485, 44.99999994, 45, 100);
            AddRecord(fdt, 99.00507313, 44.99999989, 45, 100);
            AddRecord(fdt, 99.00634141, 44.99999982, 45, 100);
            AddRecord(fdt, 99.00760969, 44.99999975, 45, 100);
            AddRecord(fdt, 99.00887797, 44.99999965, 45, 100);
            AddRecord(fdt, 99.01014625, 44.99999955, 45, 100);
            AddRecord(fdt, 99.01141454, 44.99999943, 45, 100);
            AddRecord(fdt, 99.01268282, 44.99999930, 45, 1000);
            AddRecord(fdt, 99.02536563, 44.99999718, 45, 1000);
            AddRecord(fdt, 99.03804845, 44.99999366, 45, 1000);
            AddRecord(fdt, 99.05073126, 44.99998873, 45, 1000);
            AddRecord(fdt, 99.06341407, 44.99998239, 45, 1000);
            AddRecord(fdt, 99.07609688, 44.99997465, 45, 1000);
            AddRecord(fdt, 99.08877969, 44.99996549, 45, 1000);
            AddRecord(fdt, 99.10146248, 44.99995493, 45, 1000);
            AddRecord(fdt, 99.11414528, 44.99994296, 45, 1000);
            AddRecord(fdt, 99.12682807, 44.99992958, 45, 10000);
            AddRecord(fdt, 99.25365552, 44.99971831, 45, 10000);
            AddRecord(fdt, 99.38048172, 44.99936620, 45, 10000);
            AddRecord(fdt, 99.50730606, 44.99887326, 45, 10000);
            AddRecord(fdt, 99.63412792, 44.99823948, 45, 10000);
            AddRecord(fdt, 99.76094666, 44.99746487, 45, 10000);
            AddRecord(fdt, 99.88776168, 44.99654945, 45, 10000);
            AddRecord(fdt, 100.01457236, 44.99549321, 45, 10000);
            AddRecord(fdt, 100.14137806, 44.99429617, 45, 10000);
            AddRecord(fdt, 100.26817817, 44.99295835, 45, 10000);
            AddRecord(fdt, 99.00000000, 50.00000000, 50, 0);
            AddRecord(fdt, 98.99860522, 49.99999999, 50, 100);
            AddRecord(fdt, 98.99721043, 49.99999997, 50, 100);
            AddRecord(fdt, 98.99581565, 49.99999992, 50, 100);
            AddRecord(fdt, 98.99442087, 49.99999987, 50, 100);
            AddRecord(fdt, 98.99302609, 49.99999979, 50, 100);
            AddRecord(fdt, 98.99163130, 49.99999970, 50, 100);
            AddRecord(fdt, 98.99023652, 49.99999959, 50, 100);
            AddRecord(fdt, 98.98884174, 49.99999946, 50, 100);
            AddRecord(fdt, 98.98744696, 49.99999932, 50, 100);
            AddRecord(fdt, 98.98605217, 49.99999916, 50, 1000);
            AddRecord(fdt, 98.97210435, 49.99999665, 50, 1000);
            AddRecord(fdt, 98.95815652, 49.99999246, 50, 1000);
            AddRecord(fdt, 98.94420870, 49.99998659, 50, 1000);
            AddRecord(fdt, 98.93026088, 49.99997904, 50, 1000);
            AddRecord(fdt, 98.91631307, 49.99996982, 50, 1000);
            AddRecord(fdt, 98.90236526, 49.99995892, 50, 1000);
            AddRecord(fdt, 98.88841746, 49.99994635, 50, 1000);
            AddRecord(fdt, 98.87446967, 49.99993210, 50, 1000);
            AddRecord(fdt, 98.86052189, 49.99991617, 50, 10000);
            AddRecord(fdt, 98.72104474, 49.99966469, 50, 10000);
            AddRecord(fdt, 98.58156954, 49.99924555, 50, 10000);
            AddRecord(fdt, 98.44209725, 49.99865877, 50, 10000);
            AddRecord(fdt, 98.30262884, 49.99790435, 50, 10000);
            AddRecord(fdt, 98.16316527, 49.99698230, 50, 10000);
            AddRecord(fdt, 98.02370753, 49.99589264, 50, 10000);
            AddRecord(fdt, 97.88425657, 49.99463537, 50, 10000);
            AddRecord(fdt, 97.74481337, 49.99321051, 50, 10000);
            AddRecord(fdt, 97.60537890, 49.99161808, 50, 10000);
            AddRecord(fdt, 99.00139478, 49.99999999, 50, 100);
            AddRecord(fdt, 99.00278957, 49.99999997, 50, 100);
            AddRecord(fdt, 99.00418435, 49.99999992, 50, 100);
            AddRecord(fdt, 99.00557913, 49.99999987, 50, 100);
            AddRecord(fdt, 99.00697391, 49.99999979, 50, 100);
            AddRecord(fdt, 99.00836870, 49.99999970, 50, 100);
            AddRecord(fdt, 99.00976348, 49.99999959, 50, 100);
            AddRecord(fdt, 99.01115826, 49.99999946, 50, 100);
            AddRecord(fdt, 99.01255304, 49.99999932, 50, 100);
            AddRecord(fdt, 99.01394783, 49.99999916, 50, 1000);
            AddRecord(fdt, 99.02789565, 49.99999665, 50, 1000);
            AddRecord(fdt, 99.04184348, 49.99999246, 50, 1000);
            AddRecord(fdt, 99.05579130, 49.99998659, 50, 1000);
            AddRecord(fdt, 99.06973912, 49.99997904, 50, 1000);
            AddRecord(fdt, 99.08368693, 49.99996982, 50, 1000);
            AddRecord(fdt, 99.09763474, 49.99995892, 50, 1000);
            AddRecord(fdt, 99.11158254, 49.99994635, 50, 1000);
            AddRecord(fdt, 99.12553033, 49.99993210, 50, 1000);
            AddRecord(fdt, 99.13947811, 49.99991617, 50, 10000);
            AddRecord(fdt, 99.27895526, 49.99966469, 50, 10000);
            AddRecord(fdt, 99.41843046, 49.99924555, 50, 10000);
            AddRecord(fdt, 99.55790275, 49.99865877, 50, 10000);
            AddRecord(fdt, 99.69737116, 49.99790435, 50, 10000);
            AddRecord(fdt, 99.83683473, 49.99698230, 50, 10000);
            AddRecord(fdt, 99.97629247, 49.99589264, 50, 10000);
            AddRecord(fdt, 100.11574343, 49.99463537, 50, 10000);
            AddRecord(fdt, 100.25518663, 49.99321051, 50, 10000);
            AddRecord(fdt, 100.39462110, 49.99161808, 50, 10000);
            AddRecord(fdt, 99.00000000, 55.00000000, 55, 0);
            AddRecord(fdt, 98.99843736, 54.99999999, 55, 100);
            AddRecord(fdt, 98.99687471, 54.99999996, 55, 100);
            AddRecord(fdt, 98.99531207, 54.99999991, 55, 100);
            AddRecord(fdt, 98.99374943, 54.99999984, 55, 100);
            AddRecord(fdt, 98.99218678, 54.99999975, 55, 100);
            AddRecord(fdt, 98.99062414, 54.99999964, 55, 100);
            AddRecord(fdt, 98.98906150, 54.99999951, 55, 100);
            AddRecord(fdt, 98.98749885, 54.99999936, 55, 100);
            AddRecord(fdt, 98.98593621, 54.99999919, 55, 100);
            AddRecord(fdt, 98.98437357, 54.99999900, 55, 1000);
            AddRecord(fdt, 98.96874714, 54.99999599, 55, 1000);
            AddRecord(fdt, 98.95312071, 54.99999097, 55, 1000);
            AddRecord(fdt, 98.93749428, 54.99998395, 55, 1000);
            AddRecord(fdt, 98.92186787, 54.99997491, 55, 1000);
            AddRecord(fdt, 98.90624146, 54.99996388, 55, 1000);
            AddRecord(fdt, 98.89061506, 54.99995083, 55, 1000);
            AddRecord(fdt, 98.87498867, 54.99993578, 55, 1000);
            AddRecord(fdt, 98.85936229, 54.99991872, 55, 1000);
            AddRecord(fdt, 98.84373593, 54.99989966, 55, 10000);
            AddRecord(fdt, 98.68747341, 54.99959863, 55, 10000);
            AddRecord(fdt, 98.53121402, 54.99909693, 55, 10000);
            AddRecord(fdt, 98.37495930, 54.99839456, 55, 10000);
            AddRecord(fdt, 98.21871082, 54.99749153, 55, 10000);
            AddRecord(fdt, 98.06247014, 54.99638786, 55, 10000);
            AddRecord(fdt, 97.90623882, 54.99508357, 55, 10000);
            AddRecord(fdt, 97.75001841, 54.99357868, 55, 10000);
            AddRecord(fdt, 97.59381047, 54.99187321, 55, 10000);
            AddRecord(fdt, 97.43761655, 54.98996720, 55, 10000);
            AddRecord(fdt, 99.00156264, 54.99999999, 55, 100);
            AddRecord(fdt, 99.00312529, 54.99999996, 55, 100);
            AddRecord(fdt, 99.00468793, 54.99999991, 55, 100);
            AddRecord(fdt, 99.00625057, 54.99999984, 55, 100);
            AddRecord(fdt, 99.00781322, 54.99999975, 55, 100);
            AddRecord(fdt, 99.00937586, 54.99999964, 55, 100);
            AddRecord(fdt, 99.01093850, 54.99999951, 55, 100);
            AddRecord(fdt, 99.01250115, 54.99999936, 55, 100);
            AddRecord(fdt, 99.01406379, 54.99999919, 55, 100);
            AddRecord(fdt, 99.01562643, 54.99999900, 55, 1000);
            AddRecord(fdt, 99.03125286, 54.99999599, 55, 1000);
            AddRecord(fdt, 99.04687929, 54.99999097, 55, 1000);
            AddRecord(fdt, 99.06250572, 54.99998395, 55, 1000);
            AddRecord(fdt, 99.07813213, 54.99997491, 55, 1000);
            AddRecord(fdt, 99.09375854, 54.99996388, 55, 1000);
            AddRecord(fdt, 99.10938494, 54.99995083, 55, 1000);
            AddRecord(fdt, 99.12501133, 54.99993578, 55, 1000);
            AddRecord(fdt, 99.14063771, 54.99991872, 55, 1000);
            AddRecord(fdt, 99.15626407, 54.99989966, 55, 10000);
            AddRecord(fdt, 99.31252659, 54.99959863, 55, 10000);
            AddRecord(fdt, 99.46878598, 54.99909693, 55, 10000);
            AddRecord(fdt, 99.62504070, 54.99839456, 55, 10000);
            AddRecord(fdt, 99.78128918, 54.99749153, 55, 10000);
            AddRecord(fdt, 99.93752986, 54.99638786, 55, 10000);
            AddRecord(fdt, 100.09376118, 54.99508357, 55, 10000);
            AddRecord(fdt, 100.24998159, 54.99357868, 55, 10000);
            AddRecord(fdt, 100.40618953, 54.99187321, 55, 10000);
            AddRecord(fdt, 100.56238345, 54.98996720, 55, 10000);
            AddRecord(fdt, 99.00000000, 60.00000000, 60, 0);
            AddRecord(fdt, 98.99820789, 59.99999999, 60, 100);
            AddRecord(fdt, 98.99641577, 59.99999995, 60, 100);
            AddRecord(fdt, 98.99462366, 59.99999989, 60, 100);
            AddRecord(fdt, 98.99283154, 59.99999981, 60, 100);
            AddRecord(fdt, 98.99103943, 59.99999970, 60, 100);
            AddRecord(fdt, 98.98924731, 59.99999956, 60, 100);
            AddRecord(fdt, 98.98745520, 59.99999940, 60, 100);
            AddRecord(fdt, 98.98566308, 59.99999922, 60, 100);
            AddRecord(fdt, 98.98387097, 59.99999902, 60, 100);
            AddRecord(fdt, 98.98207885, 59.99999878, 60, 1000);
            AddRecord(fdt, 98.96415771, 59.99999514, 60, 1000);
            AddRecord(fdt, 98.94623657, 59.99998906, 60, 1000);
            AddRecord(fdt, 98.92831544, 59.99998055, 60, 1000);
            AddRecord(fdt, 98.91039432, 59.99996961, 60, 1000);
            AddRecord(fdt, 98.89247322, 59.99995624, 60, 1000);
            AddRecord(fdt, 98.87455213, 59.99994043, 60, 1000);
            AddRecord(fdt, 98.85663105, 59.99992220, 60, 1000);
            AddRecord(fdt, 98.83871000, 59.99990153, 60, 1000);
            AddRecord(fdt, 98.82078897, 59.99987843, 60, 10000);
            AddRecord(fdt, 98.64158058, 59.99951374, 60, 10000);
            AddRecord(fdt, 98.46237744, 59.99890593, 60, 10000);
            AddRecord(fdt, 98.28318219, 59.99805501, 60, 10000);
            AddRecord(fdt, 98.10399746, 59.99696101, 60, 10000);
            AddRecord(fdt, 97.92482587, 59.99562396, 60, 10000);
            AddRecord(fdt, 97.74567006, 59.99404388, 60, 10000);
            AddRecord(fdt, 97.56653263, 59.99222081, 60, 10000);
            AddRecord(fdt, 97.38741623, 59.99015480, 60, 10000);
            AddRecord(fdt, 97.20832346, 59.98784591, 60, 10000);
            AddRecord(fdt, 99.00179211, 59.99999999, 60, 100);
            AddRecord(fdt, 99.00358423, 59.99999995, 60, 100);
            AddRecord(fdt, 99.00537634, 59.99999989, 60, 100);
            AddRecord(fdt, 99.00716846, 59.99999981, 60, 100);
            AddRecord(fdt, 99.00896057, 59.99999970, 60, 100);
            AddRecord(fdt, 99.01075269, 59.99999956, 60, 100);
            AddRecord(fdt, 99.01254480, 59.99999940, 60, 100);
            AddRecord(fdt, 99.01433692, 59.99999922, 60, 100);
            AddRecord(fdt, 99.01612903, 59.99999902, 60, 100);
            AddRecord(fdt, 99.01792115, 59.99999878, 60, 1000);
            AddRecord(fdt, 99.03584229, 59.99999514, 60, 1000);
            AddRecord(fdt, 99.05376343, 59.99998906, 60, 1000);
            AddRecord(fdt, 99.07168456, 59.99998055, 60, 1000);
            AddRecord(fdt, 99.08960568, 59.99996961, 60, 1000);
            AddRecord(fdt, 99.10752678, 59.99995624, 60, 1000);
            AddRecord(fdt, 99.12544787, 59.99994043, 60, 1000);
            AddRecord(fdt, 99.14336895, 59.99992220, 60, 1000);
            AddRecord(fdt, 99.16129000, 59.99990153, 60, 1000);
            AddRecord(fdt, 99.17921103, 59.99987843, 60, 10000);
            AddRecord(fdt, 99.35841942, 59.99951374, 60, 10000);
            AddRecord(fdt, 99.53762256, 59.99890593, 60, 10000);
            AddRecord(fdt, 99.71681781, 59.99805501, 60, 10000);
            AddRecord(fdt, 99.89600254, 59.99696101, 60, 10000);
            AddRecord(fdt, 100.07517413, 59.99562396, 60, 10000);
            AddRecord(fdt, 100.25432994, 59.99404388, 60, 10000);
            AddRecord(fdt, 100.43346737, 59.99222081, 60, 10000);
            AddRecord(fdt, 100.61258377, 59.99015480, 60, 10000);
            AddRecord(fdt, 100.79167654, 59.98784591, 60, 10000);
            AddRecord(fdt, 99.00000000, 65.00000000, 65, 0);
            AddRecord(fdt, 98.99788026, 64.99999998, 65, 100);
            AddRecord(fdt, 98.99576051, 64.99999994, 65, 100);
            AddRecord(fdt, 98.99364077, 64.99999986, 65, 100);
            AddRecord(fdt, 98.99152103, 64.99999976, 65, 100);
            AddRecord(fdt, 98.98940129, 64.99999962, 65, 100);
            AddRecord(fdt, 98.98728154, 64.99999946, 65, 100);
            AddRecord(fdt, 98.98516180, 64.99999926, 65, 100);
            AddRecord(fdt, 98.98304206, 64.99999904, 65, 100);
            AddRecord(fdt, 98.98092231, 64.99999878, 65, 100);
            AddRecord(fdt, 98.97880257, 64.99999850, 65, 1000);
            AddRecord(fdt, 98.95760515, 64.99999399, 65, 1000);
            AddRecord(fdt, 98.93640773, 64.99998647, 65, 1000);
            AddRecord(fdt, 98.91521033, 64.99997594, 65, 1000);
            AddRecord(fdt, 98.89401295, 64.99996241, 65, 1000);
            AddRecord(fdt, 98.87281559, 64.99994587, 65, 1000);
            AddRecord(fdt, 98.85161827, 64.99992632, 65, 1000);
            AddRecord(fdt, 98.83042097, 64.99990376, 65, 1000);
            AddRecord(fdt, 98.80922371, 64.99987820, 65, 1000);
            AddRecord(fdt, 98.78802650, 64.99984963, 65, 10000);
            AddRecord(fdt, 98.57605776, 64.99939853, 65, 10000);
            AddRecord(fdt, 98.36409856, 64.99864671, 65, 10000);
            AddRecord(fdt, 98.15215365, 64.99759420, 65, 10000);
            AddRecord(fdt, 97.94022780, 64.99624104, 65, 10000);
            AddRecord(fdt, 97.72832577, 64.99458728, 65, 10000);
            AddRecord(fdt, 97.51645231, 64.99263297, 65, 10000);
            AddRecord(fdt, 97.30461217, 64.99037819, 65, 10000);
            AddRecord(fdt, 97.09281011, 64.98782302, 65, 10000);
            AddRecord(fdt, 96.88105085, 64.98496755, 65, 10000);
            AddRecord(fdt, 99.00211974, 64.99999998, 65, 100);
            AddRecord(fdt, 99.00423949, 64.99999994, 65, 100);
            AddRecord(fdt, 99.00635923, 64.99999986, 65, 100);
            AddRecord(fdt, 99.00847897, 64.99999976, 65, 100);
            AddRecord(fdt, 99.01059871, 64.99999962, 65, 100);
            AddRecord(fdt, 99.01271846, 64.99999946, 65, 100);
            AddRecord(fdt, 99.01483820, 64.99999926, 65, 100);
            AddRecord(fdt, 99.01695794, 64.99999904, 65, 100);
            AddRecord(fdt, 99.01907769, 64.99999878, 65, 100);
            AddRecord(fdt, 99.02119743, 64.99999850, 65, 1000);
            AddRecord(fdt, 99.04239485, 64.99999399, 65, 1000);
            AddRecord(fdt, 99.06359227, 64.99998647, 65, 1000);
            AddRecord(fdt, 99.08478967, 64.99997594, 65, 1000);
            AddRecord(fdt, 99.10598705, 64.99996241, 65, 1000);
            AddRecord(fdt, 99.12718441, 64.99994587, 65, 1000);
            AddRecord(fdt, 99.14838173, 64.99992632, 65, 1000);
            AddRecord(fdt, 99.16957903, 64.99990376, 65, 1000);
            AddRecord(fdt, 99.19077629, 64.99987820, 65, 1000);
            AddRecord(fdt, 99.21197350, 64.99984963, 65, 10000);
            AddRecord(fdt, 99.42394224, 64.99939853, 65, 10000);
            AddRecord(fdt, 99.63590144, 64.99864671, 65, 10000);
            AddRecord(fdt, 99.84784635, 64.99759420, 65, 10000);
            AddRecord(fdt, 100.05977220, 64.99624104, 65, 10000);
            AddRecord(fdt, 100.27167423, 64.99458728, 65, 10000);
            AddRecord(fdt, 100.48354769, 64.99263297, 65, 10000);
            AddRecord(fdt, 100.69538783, 64.99037819, 65, 10000);
            AddRecord(fdt, 100.90718989, 64.98782302, 65, 10000);
            AddRecord(fdt, 101.11894915, 64.98496755, 65, 10000);
            AddRecord(fdt, 99.00000000, 70.00000000, 70, 0);
            AddRecord(fdt, 98.99738128, 69.99999998, 70, 100);
            AddRecord(fdt, 98.99476255, 69.99999992, 70, 100);
            AddRecord(fdt, 98.99214383, 69.99999983, 70, 100);
            AddRecord(fdt, 98.98952511, 69.99999969, 70, 100);
            AddRecord(fdt, 98.98690638, 69.99999952, 70, 100);
            AddRecord(fdt, 98.98428766, 69.99999931, 70, 100);
            AddRecord(fdt, 98.98166893, 69.99999906, 70, 100);
            AddRecord(fdt, 98.97905021, 69.99999877, 70, 100);
            AddRecord(fdt, 98.97643149, 69.99999844, 70, 100);
            AddRecord(fdt, 98.97381276, 69.99999808, 70, 1000);
            AddRecord(fdt, 98.94762554, 69.99999230, 70, 1000);
            AddRecord(fdt, 98.92143833, 69.99998268, 70, 1000);
            AddRecord(fdt, 98.89525116, 69.99996920, 70, 1000);
            AddRecord(fdt, 98.86906402, 69.99995188, 70, 1000);
            AddRecord(fdt, 98.84287693, 69.99993070, 70, 1000);
            AddRecord(fdt, 98.81668989, 69.99990568, 70, 1000);
            AddRecord(fdt, 98.79050293, 69.99987681, 70, 1000);
            AddRecord(fdt, 98.76431604, 69.99984408, 70, 1000);
            AddRecord(fdt, 98.73812924, 69.99980751, 70, 10000);
            AddRecord(fdt, 98.47626814, 69.99923006, 70, 10000);
            AddRecord(fdt, 98.21442637, 69.99826767, 70, 10000);
            AddRecord(fdt, 97.95261356, 69.99692041, 70, 10000);
            AddRecord(fdt, 97.69083937, 69.99518835, 70, 10000);
            AddRecord(fdt, 97.42911344, 69.99307159, 70, 10000);
            AddRecord(fdt, 97.16744540, 69.99057025, 70, 10000);
            AddRecord(fdt, 96.90584486, 69.98768447, 70, 10000);
            AddRecord(fdt, 96.64432141, 69.98441443, 70, 10000);
            AddRecord(fdt, 96.38288464, 69.98076031, 70, 10000);
            AddRecord(fdt, 99.00261872, 69.99999998, 70, 100);
            AddRecord(fdt, 99.00523745, 69.99999992, 70, 100);
            AddRecord(fdt, 99.00785617, 69.99999983, 70, 100);
            AddRecord(fdt, 99.01047489, 69.99999969, 70, 100);
            AddRecord(fdt, 99.01309362, 69.99999952, 70, 100);
            AddRecord(fdt, 99.01571234, 69.99999931, 70, 100);
            AddRecord(fdt, 99.01833107, 69.99999906, 70, 100);
            AddRecord(fdt, 99.02094979, 69.99999877, 70, 100);
            AddRecord(fdt, 99.02356851, 69.99999844, 70, 100);
            AddRecord(fdt, 99.02618724, 69.99999808, 70, 1000);
            AddRecord(fdt, 99.05237446, 69.99999230, 70, 1000);
            AddRecord(fdt, 99.07856167, 69.99998268, 70, 1000);
            AddRecord(fdt, 99.10474884, 69.99996920, 70, 1000);
            AddRecord(fdt, 99.13093598, 69.99995188, 70, 1000);
            AddRecord(fdt, 99.15712307, 69.99993070, 70, 1000);
            AddRecord(fdt, 99.18331011, 69.99990568, 70, 1000);
            AddRecord(fdt, 99.20949707, 69.99987681, 70, 1000);
            AddRecord(fdt, 99.23568396, 69.99984408, 70, 1000);
            AddRecord(fdt, 99.26187076, 69.99980751, 70, 10000);
            AddRecord(fdt, 99.52373186, 69.99923006, 70, 10000);
            AddRecord(fdt, 99.78557363, 69.99826767, 70, 10000);
            AddRecord(fdt, 100.04738644, 69.99692041, 70, 10000);
            AddRecord(fdt, 100.30916063, 69.99518835, 70, 10000);
            AddRecord(fdt, 100.57088656, 69.99307159, 70, 10000);
            AddRecord(fdt, 100.83255460, 69.99057025, 70, 10000);
            AddRecord(fdt, 101.09415514, 69.98768447, 70, 10000);
            AddRecord(fdt, 101.35567859, 69.98441443, 70, 10000);
            AddRecord(fdt, 101.61711536, 69.98076031, 70, 10000);
            AddRecord(fdt, 99.00000000, 75.00000000, 75, 0);
            AddRecord(fdt, 98.99654003, 74.99999997, 75, 100);
            AddRecord(fdt, 98.99308007, 74.99999990, 75, 100);
            AddRecord(fdt, 98.98962010, 74.99999976, 75, 100);
            AddRecord(fdt, 98.98616013, 74.99999958, 75, 100);
            AddRecord(fdt, 98.98270016, 74.99999935, 75, 100);
            AddRecord(fdt, 98.97924020, 74.99999906, 75, 100);
            AddRecord(fdt, 98.97578023, 74.99999872, 75, 100);
            AddRecord(fdt, 98.97232026, 74.99999833, 75, 100);
            AddRecord(fdt, 98.96886030, 74.99999788, 75, 100);
            AddRecord(fdt, 98.96540033, 74.99999739, 75, 1000);
            AddRecord(fdt, 98.93080068, 74.99998955, 75, 1000);
            AddRecord(fdt, 98.89620108, 74.99997648, 75, 1000);
            AddRecord(fdt, 98.86160155, 74.99995819, 75, 1000);
            AddRecord(fdt, 98.82700212, 74.99993468, 75, 1000);
            AddRecord(fdt, 98.79240280, 74.99990593, 75, 1000);
            AddRecord(fdt, 98.75780362, 74.99987197, 75, 1000);
            AddRecord(fdt, 98.72320461, 74.99983277, 75, 1000);
            AddRecord(fdt, 98.68860579, 74.99978835, 75, 1000);
            AddRecord(fdt, 98.65400718, 74.99973871, 75, 10000);
            AddRecord(fdt, 98.30803790, 74.99895487, 75, 10000);
            AddRecord(fdt, 97.96211569, 74.99764855, 75, 10000);
            AddRecord(fdt, 97.61626407, 74.99581990, 75, 10000);
            AddRecord(fdt, 97.27050652, 74.99346910, 75, 10000);
            AddRecord(fdt, 96.92486648, 74.99059640, 75, 10000);
            AddRecord(fdt, 96.57936735, 74.98720211, 75, 10000);
            AddRecord(fdt, 96.23403246, 74.98328656, 75, 10000);
            //AddRecord(fdt,95.88888509,74.97885017,75,10000);
            //AddRecord(fdt,95.54394842,74.97389341,75,10000);
            AddRecord(fdt, 99.00345997, 74.99999997, 75, 100);
            AddRecord(fdt, 99.00691993, 74.99999990, 75, 100);
            AddRecord(fdt, 99.01037990, 74.99999976, 75, 100);
            AddRecord(fdt, 99.01383987, 74.99999958, 75, 100);
            AddRecord(fdt, 99.01729984, 74.99999935, 75, 100);
            AddRecord(fdt, 99.02075980, 74.99999906, 75, 100);
            AddRecord(fdt, 99.02421977, 74.99999872, 75, 100);
            AddRecord(fdt, 99.02767974, 74.99999833, 75, 100);
            AddRecord(fdt, 99.03113970, 74.99999788, 75, 100);
            AddRecord(fdt, 99.03459967, 74.99999739, 75, 1000);
            AddRecord(fdt, 99.06919932, 74.99998955, 75, 1000);
            AddRecord(fdt, 99.10379892, 74.99997648, 75, 1000);
            AddRecord(fdt, 99.13839845, 74.99995819, 75, 1000);
            AddRecord(fdt, 99.17299788, 74.99993468, 75, 1000);
            AddRecord(fdt, 99.20759720, 74.99990593, 75, 1000);
            AddRecord(fdt, 99.24219638, 74.99987197, 75, 1000);
            AddRecord(fdt, 99.27679539, 74.99983277, 75, 1000);
            AddRecord(fdt, 99.31139421, 74.99978835, 75, 1000);
            AddRecord(fdt, 99.34599282, 74.99973871, 75, 10000);
            AddRecord(fdt, 99.69196210, 74.99895487, 75, 10000);
            AddRecord(fdt, 100.03788431, 74.99764855, 75, 10000);
            AddRecord(fdt, 100.38373593, 74.99581990, 75, 10000);
            AddRecord(fdt, 100.72949348, 74.99346910, 75, 10000);
            AddRecord(fdt, 101.07513352, 74.99059640, 75, 10000);
            AddRecord(fdt, 101.42063265, 74.98720211, 75, 10000);
            AddRecord(fdt, 101.76596754, 74.98328656, 75, 10000);
            //AddRecord(fdt,102.11111491,74.97885017,75,10000);
            //AddRecord(fdt,102.45605158,74.97389341,75,10000);
            AddRecord(fdt, 99.00000000, 80.00000000, 80, 0);
            AddRecord(fdt, 98.99484363, 79.99999996, 80, 100);
            AddRecord(fdt, 98.98968726, 79.99999984, 80, 100);
            AddRecord(fdt, 98.98453089, 79.99999964, 80, 100);
            AddRecord(fdt, 98.97937452, 79.99999937, 80, 100);
            AddRecord(fdt, 98.97421815, 79.99999901, 80, 100);
            AddRecord(fdt, 98.96906178, 79.99999857, 80, 100);
            AddRecord(fdt, 98.96390541, 79.99999806, 80, 100);
            AddRecord(fdt, 98.95874904, 79.99999746, 80, 100);
            AddRecord(fdt, 98.95359267, 79.99999679, 80, 100);
            AddRecord(fdt, 98.94843631, 79.99999603, 80, 1000);
            AddRecord(fdt, 98.89687269, 79.99998413, 80, 1000);
            AddRecord(fdt, 98.84530924, 79.99996428, 80, 1000);
            AddRecord(fdt, 98.79374604, 79.99993650, 80, 1000);
            AddRecord(fdt, 98.74218315, 79.99990078, 80, 1000);
            AddRecord(fdt, 98.69062068, 79.99985713, 80, 1000);
            AddRecord(fdt, 98.63905868, 79.99980554, 80, 1000);
            AddRecord(fdt, 98.58749726, 79.99974601, 80, 1000);
            AddRecord(fdt, 98.53593648, 79.99967854, 80, 1000);
            AddRecord(fdt, 98.48437643, 79.99960314, 80, 10000);
            AddRecord(fdt, 97.96883385, 79.99841266, 80, 10000);
            AddRecord(fdt, 97.45345317, 79.99642883, 80, 10000);
            AddRecord(fdt, 96.93831514, 79.99365213, 80, 10000);
            AddRecord(fdt, 96.42350029, 79.99008323, 80, 10000);
            //AddRecord(fdt,95.90908884,79.98572295,80,10000);
            //AddRecord(fdt,95.39516063,79.98057235,80,10000);
            //AddRecord(fdt,94.88179503,79.97463263,80,10000);
            //AddRecord(fdt,94.36907090,79.96790520,80,10000);
            //AddRecord(fdt,93.85706649,79.96039163,80,10000);
            AddRecord(fdt, 99.00515637, 79.99999996, 80, 100);
            AddRecord(fdt, 99.01031274, 79.99999984, 80, 100);
            AddRecord(fdt, 99.01546911, 79.99999964, 80, 100);
            AddRecord(fdt, 99.02062548, 79.99999937, 80, 100);
            AddRecord(fdt, 99.02578185, 79.99999901, 80, 100);
            AddRecord(fdt, 99.03093822, 79.99999857, 80, 100);
            AddRecord(fdt, 99.03609459, 79.99999806, 80, 100);
            AddRecord(fdt, 99.04125096, 79.99999746, 80, 100);
            AddRecord(fdt, 99.04640733, 79.99999679, 80, 100);
            AddRecord(fdt, 99.05156369, 79.99999603, 80, 1000);
            AddRecord(fdt, 99.10312731, 79.99998413, 80, 1000);
            AddRecord(fdt, 99.15469076, 79.99996428, 80, 1000);
            AddRecord(fdt, 99.20625396, 79.99993650, 80, 1000);
            AddRecord(fdt, 99.25781685, 79.99990078, 80, 1000);
            AddRecord(fdt, 99.30937932, 79.99985713, 80, 1000);
            AddRecord(fdt, 99.36094132, 79.99980554, 80, 1000);
            AddRecord(fdt, 99.41250274, 79.99974601, 80, 1000);
            AddRecord(fdt, 99.46406352, 79.99967854, 80, 1000);
            AddRecord(fdt, 99.51562357, 79.99960314, 80, 10000);
            AddRecord(fdt, 100.03116615, 79.99841266, 80, 10000);
            AddRecord(fdt, 100.54654683, 79.99642883, 80, 10000);
            AddRecord(fdt, 101.06168486, 79.99365213, 80, 10000);
            AddRecord(fdt, 101.57649971, 79.99008323, 80, 10000);
            //AddRecord(fdt,102.09091116,79.98572295,80,10000);
            //AddRecord(fdt,102.60483937,79.98057235,80,10000);
            //AddRecord(fdt,103.11820497,79.97463263,80,10000);
            //AddRecord(fdt,103.63092910,79.96790520,80,10000);
            //AddRecord(fdt,104.14293351,79.96039163,80,10000);
            AddRecord(fdt, 99.00000000, 85.00000000, 85, 0);
            AddRecord(fdt, 98.98972728, 84.99999992, 85, 100);
            AddRecord(fdt, 98.97945457, 84.99999968, 85, 100);
            AddRecord(fdt, 98.96918185, 84.99999928, 85, 100);
            AddRecord(fdt, 98.95890914, 84.99999872, 85, 100);
            AddRecord(fdt, 98.94863643, 84.99999800, 85, 100);
            AddRecord(fdt, 98.93836372, 84.99999712, 85, 100);
            AddRecord(fdt, 98.92809102, 84.99999608, 85, 100);
            AddRecord(fdt, 98.91781832, 84.99999488, 85, 100);
            AddRecord(fdt, 98.90754562, 84.99999352, 85, 100);
            AddRecord(fdt, 98.89727293, 84.99999200, 85, 1000);
            AddRecord(fdt, 98.79454652, 84.99996802, 85, 1000);
            AddRecord(fdt, 98.69182143, 84.99992804, 85, 1000);
            AddRecord(fdt, 98.58909829, 84.99987206, 85, 1000);
            AddRecord(fdt, 98.48637778, 84.99980010, 85, 1000);
            AddRecord(fdt, 98.38366055, 84.99971215, 85, 1000);
            AddRecord(fdt, 98.28094724, 84.99960820, 85, 1000);
            AddRecord(fdt, 98.17823853, 84.99948827, 85, 1000);
            AddRecord(fdt, 98.07553505, 84.99935235, 85, 1000);
            AddRecord(fdt, 97.97283747, 84.99920045, 85, 10000);
            AddRecord(fdt, 96.94632975, 84.99680257, 85, 10000);
            //AddRecord(fdt,95.92112913,84.99280864,85,10000);
            //AddRecord(fdt,94.89788293,84.98722250,85,10000);
            //AddRecord(fdt,93.87723102,84.98004944,85,10000);
            //AddRecord(fdt,92.85980349,84.97129626,85,10000);
            //AddRecord(fdt,91.84621835,84.96097120,85,10000);
            //AddRecord(fdt,90.83707941,84.94908388,85,10000);
            //AddRecord(fdt,89.83297419,84.93564531,85,10000);
            //AddRecord(fdt,88.83447205,84.92066781,85,10000);
            AddRecord(fdt, 99.01027272, 84.99999992, 85, 100);
            AddRecord(fdt, 99.02054543, 84.99999968, 85, 100);
            AddRecord(fdt, 99.03081815, 84.99999928, 85, 100);
            AddRecord(fdt, 99.04109086, 84.99999872, 85, 100);
            AddRecord(fdt, 99.05136357, 84.99999800, 85, 100);
            AddRecord(fdt, 99.06163628, 84.99999712, 85, 100);
            AddRecord(fdt, 99.07190898, 84.99999608, 85, 100);
            AddRecord(fdt, 99.08218168, 84.99999488, 85, 100);
            AddRecord(fdt, 99.09245438, 84.99999352, 85, 100);
            AddRecord(fdt, 99.10272707, 84.99999200, 85, 1000);
            AddRecord(fdt, 99.20545348, 84.99996802, 85, 1000);
            AddRecord(fdt, 99.30817857, 84.99992804, 85, 1000);
            AddRecord(fdt, 99.41090171, 84.99987206, 85, 1000);
            AddRecord(fdt, 99.51362222, 84.99980010, 85, 1000);
            AddRecord(fdt, 99.61633945, 84.99971215, 85, 1000);
            AddRecord(fdt, 99.71905276, 84.99960820, 85, 1000);
            AddRecord(fdt, 99.82176147, 84.99948827, 85, 1000);
            AddRecord(fdt, 99.92446495, 84.99935235, 85, 1000);
            AddRecord(fdt, 100.02716253, 84.99920045, 85, 10000);
            AddRecord(fdt, 101.05367025, 84.99680257, 85, 10000);
            //AddRecord(fdt,102.07887087,84.99280864,85,10000);
            //AddRecord(fdt,103.10211707,84.98722250,85,10000);
            //AddRecord(fdt,104.12276898,84.98004944,85,10000);
            //AddRecord(fdt,105.14019651,84.97129626,85,10000);
            //AddRecord(fdt,106.15378165,84.96097120,85,10000);
            //AddRecord(fdt,107.16292059,84.94908388,85,10000);
            //AddRecord(fdt,108.16702581,84.93564531,85,10000);
            //AddRecord(fdt,109.16552795,84.92066781,85,10000);
        }

        private void AddRecord(FeatureDataTable fdt, double longitude, double latitude, int startLatitude, int interval)
        {
            var dr = fdt.NewRow();
            dr["Latitude"] = startLatitude;
            dr["Interval"] = interval;
            dr.Geometry = new NetTopologySuite.Geometries.Point(longitude, latitude);

            fdt.AddRow(dr);
        }

        [TestCase(10000)]
        [TestCase(1000)]
        [TestCase(100)]
        public void TestWebMercator(int interval)
        {
            _map.SRID = 3857;

            foreach (var lyr in _map.Layers)
            {
                var layer = lyr as VectorLayer;
                if (layer != null)
                    layer.TargetSRID = _map.SRID;
            }

            foreach (var lyr in _map.BackgroundLayer)
                lyr.Enabled = true;

            var scaleBar = (SharpMap.Rendering.Decoration.ScaleBar.ScaleBar) _map.Decorations[0];
            scaleBar.MapUnit = (int) Unit.Meter;

            GenerateScaleBarImages("WebMerc", null, interval);

            foreach (var testLat in _testLatitudes)
                GenerateScaleBarImages("WebMerc", testLat, interval);
        }

        [TestCase(10000)]
        [TestCase(1000)]
        [TestCase(100)]
        public void TestPcs(int interval)
        {
            _map.SRID = 24047;

            foreach (var lyr in _map.Layers)
                if (lyr is VectorLayer)
                    ((VectorLayer) lyr).TargetSRID = _map.SRID;

            foreach (var lyr in _map.BackgroundLayer)
                lyr.Enabled = false;

            var scaleBar = (SharpMap.Rendering.Decoration.ScaleBar.ScaleBar) _map.Decorations[0];
            scaleBar.MapUnit = (int) Unit.Meter;

            //GenerateScaleBarImages("Utm47N", null, interval);

            foreach (var testLat in _testLatitudes)
                GenerateScaleBarImages("Utm47N", testLat, interval);
        }

        [TestCase(10000)]
        [TestCase(1000)]
        [TestCase(100)]
        public void TestWgs84(int interval)
        {
            _map.SRID = 4326;

            foreach (var lyr in _map.Layers)
                if (lyr is VectorLayer)
                    ((VectorLayer) lyr).TargetSRID = _map.SRID;

            foreach (var lyr in _map.BackgroundLayer)
                lyr.Enabled = false;

            var scaleBar = (SharpMap.Rendering.Decoration.ScaleBar.ScaleBar) _map.Decorations[0];
            scaleBar.MapUnit = (int) Unit.Degree;

            GenerateScaleBarImages("Wgs84", null, interval);

            foreach (var testLat in _testLatitudes)
                GenerateScaleBarImages("Wgs84", testLat, interval);
        }


        private void GenerateScaleBarImages(string srDescr, int? centerLat, int interval)
        {
            var vectorLyr = (VectorLayer) _map.Layers[0];

            // apply Filter Delegate and zoom to resulting extents
            // interval = distance in metres between points stepping out from common meridian
            var gfp = (GeometryFeatureProvider) vectorLyr.DataSource;
            if (!centerLat.HasValue)
                gfp.FilterDelegate = row => ((int) row["Interval"] == interval || (int) row["Interval"] == 0);
            else
                gfp.FilterDelegate = row => (((int) row["Latitude"]) == (int) centerLat.Value &&
                                             ((int) row["Interval"] == interval || (int) row["Interval"] == 0));

            var geometries = vectorLyr.DataSource.GetGeometriesInView(new Envelope(-180, 180, -90, 90));
            var box = new Envelope();
            foreach (var geometry in geometries)
                box.ExpandToInclude(geometry.EnvelopeInternal);

            if (vectorLyr.CoordinateTransformation != null)
                box = GeometryTransform.TransformBox(box, vectorLyr.CoordinateTransformation.MathTransform);

            _map.ZoomToBox(box);

            // generate image
            var strLat = (centerLat.HasValue) ? $"Lat_{centerLat:D2}" : "AllPoints";
            using (var img = _map.GetMap())
                img.Save(Path.Combine(UnitTestsFixture.GetImageDirectory(this), $"ScaleBarTest_{srDescr}_{strLat}_Interval_{interval:D5}.png"),
                    System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
