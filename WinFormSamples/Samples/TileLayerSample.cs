using System;
using System.Collections.Generic;
using System.Text;
using BruTile.PreDefined;
using SharpMap.Layers;
using BruTile;
using BruTile.Web;
using SharpMap;
using SharpMap.Geometries;

namespace WinFormSamples.Samples
{
    class TileLayerSample
    {
        private static Int32 _num = 0;
        
        public static Map InitializeMap(float angle)
        {
            switch (_num++ % 10)
            {
                case 0:
                    return InitializeMapOsm();
                case 1:
                    return InitializeMapBing(BingMapType.Roads);
                case 2:
                    return InitializeMapBing(BingMapType.Aerial);
                case 3:
                    return InitializeMapBing(BingMapType.Hybrid);
                case 4:
                    return InitializeMapGoogle(GoogleMapType.GoogleMap);
                case 5:
                    return InitializeMapGoogle(GoogleMapType.GoogleSatellite);
                case 6:
                    return InitializeMapGoogle(GoogleMapType.GoogleSatellite | GoogleMapType.GoogleLabels);
                case 7:
                    return InitializeMapGoogle(GoogleMapType.GoogleTerrain);
                case 8:
                    return InitializeMapGoogle(GoogleMapType.GoogleLabels);
                case 9:
                    _num = 0;
                    return InitializeMapOsmWithXls(angle);

            }
            return InitializeMapOsm();
        }

        private static Map InitializeMapOsm()
        {
            Map map = new Map();

            TileLayer tileLayer = new TileLayer(new OsmTileSource(), "TileLayer - OSM");
            map.Layers.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }

        private const string XlsConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\{1};Extended Properties=""Excel 8.0;HDR=Yes;IMEX=1""";
        private static Map InitializeMapOsmWithXls(float angle)
        {
            Map map = new Map();

            TileLayer tileLayer = new TileLayer(new OsmTileSource(), "TileLayer - OSM with XLS");
            map.Layers.Add(tileLayer);

            //Get data from excel
            var xlsPath = string.Format(XlsConnectionString, System.IO.Directory.GetCurrentDirectory(), "GeoData\\Cities.xls");
            var ds = new System.Data.DataSet("XLS");
            using (var cn = new System.Data.OleDb.OleDbConnection(xlsPath))
            {
                cn.Open();
                using (var da = new System.Data.OleDb.OleDbDataAdapter(new System.Data.OleDb.OleDbCommand("SELECT * FROM [Cities$]", cn)))
                    da.Fill(ds);
            }

            //Add Rotation Column
            ds.Tables[0].Columns.Add("Rotation", typeof (float));
            foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                row["Rotation"] = -angle;

            //Set up provider
            var xlsProvider = new SharpMap.Data.Providers.DataTablePoint(ds.Tables[0], "OID", "X", "Y");
            var xlsLayer = new SharpMap.Layers.VectorLayer("XLS", xlsProvider);
            xlsLayer.Style.Symbol = SharpMap.Styles.VectorStyle.DefaultSymbol;
            
            //The SRS for this datasource is EPSG:4326, therefore we need to transfrom it to OSM projection
            var ctf = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var cf = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
            var epsg4326 = cf.CreateFromWkt("GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]");
            var epsg3785 = cf.CreateFromWkt("PROJCS[\"Popular Visualisation CRS / Mercator\", GEOGCS[\"Popular Visualisation CRS\", DATUM[\"Popular Visualisation Datum\", SPHEROID[\"Popular Visualisation Sphere\", 6378137, 0, AUTHORITY[\"EPSG\",\"7059\"]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[\"EPSG\",\"6055\"]],PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9102\"]], AXIS[\"E\", EAST], AXIS[\"N\", NORTH], AUTHORITY[\"EPSG\",\"4055\"]], PROJECTION[\"Mercator\"], PARAMETER[\"False_Easting\", 0], PARAMETER[\"False_Northing\", 0], PARAMETER[\"Central_Meridian\", 0], PARAMETER[\"Latitude_of_origin\", 0], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], AXIS[\"East\", EAST], AXIS[\"North\", NORTH], AUTHORITY[\"EPSG\",\"3785\"]]");
            xlsLayer.CoordinateTransformation = ctf.CreateFromCoordinateSystems(epsg4326, epsg3785);

            //Add layer to map
            map.Layers.Add(xlsLayer);
            var xlsLabelLayer = new SharpMap.Layers.LabelLayer("XLSLabel");
            xlsLabelLayer.DataSource = xlsProvider;
            xlsLabelLayer.LabelColumn = "Name";
            xlsLabelLayer.PriorityColumn = "Population";
            xlsLabelLayer.Style.CollisionBuffer = new System.Drawing.SizeF(2f, 2f);
            xlsLabelLayer.Style.CollisionDetection = true;
            xlsLabelLayer.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
            xlsLabelLayer.CoordinateTransformation = xlsLayer.CoordinateTransformation;
            map.Layers.Add(xlsLabelLayer);

            map.ZoomToBox(tileLayer.Envelope);

            return map;
        }

        private static Map InitializeMapBing(BingMapType mt)
        {
            Map map = new Map();

            TileLayer tileLayer = new TileLayer(new BingTileSource(BingRequest.UrlBingStaging, "", mt) , "TileLayer - Bing " + mt);
            map.Layers.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }

        private static Map InitializeMapGoogle(GoogleMapType mt)
        {
            Map map = new Map();

            GoogleRequest req;
            ITileSource tileSource;
            TileLayer tileLayer;
            if (mt == (GoogleMapType.GoogleSatellite | GoogleMapType.GoogleLabels))
            {
                req = new GoogleRequest(GoogleMapType.GoogleSatellite);
                tileSource = new GoogleTileSource(req);
                tileLayer = new TileLayer(tileSource, "TileLayer - " + GoogleMapType.GoogleSatellite);
                map.Layers.Add(tileLayer);
                req = new GoogleRequest(GoogleMapType.GoogleLabels);
                tileSource = new GoogleTileSource(req);
                mt = GoogleMapType.GoogleLabels;
            }
            else
            {
                req = new GoogleRequest(mt);
                tileSource = new GoogleTileSource(req);
            }

            tileLayer = new TileLayer(tileSource, "TileLayer - " + mt);
            map.Layers.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }

    }
}
