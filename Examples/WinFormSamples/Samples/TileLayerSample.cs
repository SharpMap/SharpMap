using System;

namespace WinFormSamples.Samples
{
    class TileLayerSample
    {
        private static Int32 _num;
        
        public static SharpMap.Map InitializeMap(float angle)
        {
            switch (_num++ % 11)
            {
                case 2:
                    return InitializeMapOsm();
                case 3:
                    return InitializeMapBing(BruTile.Web.BingMapType.Roads);
                case 4:
                    return InitializeMapBing(BruTile.Web.BingMapType.Aerial);
                case 5:
                    return InitializeMapBing(BruTile.Web.BingMapType.Hybrid);
                case 6:
                    return InitializeMapGoogle(BruTile.Web.GoogleMapType.GoogleMap);
                case 7:
                    return InitializeMapGoogle(BruTile.Web.GoogleMapType.GoogleSatellite);
                case 8:
                    return InitializeMapGoogle(BruTile.Web.GoogleMapType.GoogleSatellite | BruTile.Web.GoogleMapType.GoogleLabels);
                case 9:
                    return InitializeMapGoogle(BruTile.Web.GoogleMapType.GoogleTerrain);
                case 10:
                    _num = 0;
                    return InitializeMapGoogle(BruTile.Web.GoogleMapType.GoogleLabels);
                case 0:
                    _num++;
                    return InitializeMapOsmWithXls(angle);
                    
                    //Does not work anymore!
                    //return InitializeMapOsmWithVariableLayerCollection(angle);
                case 1:
                    return InitializeMapOsmWithXls(angle);

            }
            return InitializeMapOsm();
        }

        private static SharpMap.Map InitializeMapOsm()
        {
            var map = new SharpMap.Map();

            var tileLayer = new SharpMap.Layers.TileAsyncLayer(new BruTile.Web.OsmTileSource(), "TileLayer - OSM");
            map.BackgroundLayer.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            
            return map;
        }

        private const string XlsConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\{1};Extended Properties=""Excel 8.0;HDR=Yes;IMEX=1""";

        private static SharpMap.Map InitializeMapOsmWithXls(float angle)
        {
            var map = new SharpMap.Map();

            var tileLayer = new SharpMap.Layers.TileAsyncLayer(new BruTile.Web.OsmTileSource(), "TileLayer - OSM with XLS");
            map.BackgroundLayer.Add(tileLayer);

            //Get data from excel
            var xlsPath = string.Format(XlsConnectionString, System.IO.Directory.GetCurrentDirectory(), "GeoData\\Cities.xls");
            var ds = new System.Data.DataSet("XLS");
            using (var cn = new System.Data.OleDb.OleDbConnection(xlsPath))
            {
                cn.Open();
                using (var da = new System.Data.OleDb.OleDbDataAdapter(new System.Data.OleDb.OleDbCommand("SELECT * FROM [Cities$]", cn)))
                    da.Fill(ds);
            }

#if !DotSpatialProjections

            //The SRS for this datasource is EPSG:4326, therefore we need to transfrom it to OSM projection
            var ctf = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var cf = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
            var epsg4326 = cf.CreateFromWkt("GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]");
            var epsg3857 = cf.CreateFromWkt("PROJCS[\"Popular Visualisation CRS / Mercator\", GEOGCS[\"Popular Visualisation CRS\", DATUM[\"Popular Visualisation Datum\", SPHEROID[\"Popular Visualisation Sphere\", 6378137, 0, AUTHORITY[\"EPSG\",\"7059\"]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[\"EPSG\",\"6055\"]],PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9102\"]], AXIS[\"E\", EAST], AXIS[\"N\", NORTH], AUTHORITY[\"EPSG\",\"4055\"]], PROJECTION[\"Mercator\"], PARAMETER[\"False_Easting\", 0], PARAMETER[\"False_Northing\", 0], PARAMETER[\"Central_Meridian\", 0], PARAMETER[\"Latitude_of_origin\", 0], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], AXIS[\"East\", EAST], AXIS[\"North\", NORTH], AUTHORITY[\"EPSG\",\"3857\"]]");
            var ct = ctf.CreateFromCoordinateSystems(epsg4326, epsg3857);
            foreach (System.Data.DataRow row in ds.Tables[0].Rows)
            {
                if (row["X"] == DBNull.Value || row["Y"] == DBNull.Value) continue;
                var coords = new[] { Convert.ToDouble(row["X"]), Convert.ToDouble(row["Y"])};
                coords = ct.MathTransform.Transform(coords);
                row["X"] = coords[0];
                row["Y"] = coords[1];
            }

#else
            var epsg4326 = DotSpatial.Projections.KnownCoordinateSystems.Geographic.World.WGS1984;
            var epsg3857 = DotSpatial.Projections.ProjectionInfo.FromEsriString("PROJCS[\"Popular Visualisation CRS / Mercator\", GEOGCS[\"Popular Visualisation CRS\", DATUM[\"Popular Visualisation Datum\", SPHEROID[\"Popular Visualisation Sphere\", 6378137, 0, AUTHORITY[\"EPSG\",\"7059\"]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[\"EPSG\",\"6055\"]],PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9102\"]], AXIS[\"E\", EAST], AXIS[\"N\", NORTH], AUTHORITY[\"EPSG\",\"4055\"]], PROJECTION[\"Mercator\"], PARAMETER[\"False_Easting\", 0], PARAMETER[\"False_Northing\", 0], PARAMETER[\"Central_Meridian\", 0], PARAMETER[\"Latitude_of_origin\", 0], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], AXIS[\"East\", EAST], AXIS[\"North\", NORTH], AUTHORITY[\"EPSG\",\"3857\"]]");
            foreach (System.Data.DataRow row in ds.Tables[0].Rows)
            {
                if (row["X"] == DBNull.Value || row["Y"] == DBNull.Value) continue;
                var coords = new[] { Convert.ToDouble(row["X"]), Convert.ToDouble(row["Y"])};
                DotSpatial.Projections.Reproject.ReprojectPoints(coords, null, epsg4326, epsg3857, 0, 1);
                row["X"] = coords[0];
                row["Y"] = coords[1];
            }

#endif
            //Add Rotation Column
            ds.Tables[0].Columns.Add("Rotation", typeof (float));
            foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                row["Rotation"] = -angle;

            //Set up provider
            var xlsProvider = new SharpMap.Data.Providers.DataTablePoint(ds.Tables[0], "OID", "X", "Y");
            var xlsLayer = new SharpMap.Layers.VectorLayer("XLS", xlsProvider)
                               {Style = {Symbol = SharpMap.Styles.VectorStyle.DefaultSymbol}};

            //Add layer to map
            map.Layers.Add(xlsLayer);
            var xlsLabelLayer = new SharpMap.Layers.LabelLayer("XLSLabel")
                                    {
                                        DataSource = xlsProvider,
                                        LabelColumn = "Name",
                                        PriorityColumn = "Population",
                                        Style =
                                            {
                                                CollisionBuffer = new System.Drawing.SizeF(2f, 2f),
                                                CollisionDetection = true
                                            },
                                        LabelFilter =
                                            SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection
                                    };
            map.Layers.Add(xlsLabelLayer);

            map.ZoomToBox(tileLayer.Envelope);

            return map;
        }

        [Obsolete("Web service no longer available")]
        private static SharpMap.Map InitializeMapOsmWithVariableLayerCollection(float angle)
        {
            var map = new SharpMap.Map();

            var tileLayer = new SharpMap.Layers.TileAsyncLayer(new BruTile.Web.OsmTileSource(), "TileLayer - OSM with VLC");
            map.BackgroundLayer.Add(tileLayer);

            var vl = new SharpMap.Layers.VectorLayer("Vilnius Transport Data - Bus", 
                new VilniusTransportData(VilniusTransportData.TransportType.Bus));
            var pttBus = new PublicTransportTheme(System.Drawing.Brushes.DarkGreen);
            vl.Theme = new SharpMap.Rendering.Thematics.CustomTheme(pttBus.GetStyle);
            vl.CoordinateTransformation = GetCoordinateTransformation();
            map.VariableLayers.Add(vl);
            vl = new SharpMap.Layers.VectorLayer("Vilnius Transport Data - Trolley", 
                new VilniusTransportData(VilniusTransportData.TransportType.TrolleyBus));
            var pttTrolley = new PublicTransportTheme(System.Drawing.Brushes.Red);
            vl.Theme = new SharpMap.Rendering.Thematics.CustomTheme(pttTrolley.GetStyle);
            vl.CoordinateTransformation = GetCoordinateTransformation();
            map.VariableLayers.Add(vl);
            SharpMap.Layers.VariableLayerCollection.Interval = 5000;

            map.ZoomToBox(vl.Envelope);

            return map;
        }

#if !DotSpatialProjections

        private static GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation GetCoordinateTransformation()
        {

            //The SRS for this datasource is EPSG:4326, therefore we need to transfrom it to OSM projection
            var ctf = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var cf = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
            var epsg4326 = cf.CreateFromWkt("GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]");
            var epsg3857 = cf.CreateFromWkt("PROJCS[\"Popular Visualisation CRS / Mercator\", GEOGCS[\"Popular Visualisation CRS\", DATUM[\"Popular Visualisation Datum\", SPHEROID[\"Popular Visualisation Sphere\", 6378137, 0, AUTHORITY[\"EPSG\",\"7059\"]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[\"EPSG\",\"6055\"]],PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9102\"]], AXIS[\"E\", EAST], AXIS[\"N\", NORTH], AUTHORITY[\"EPSG\",\"4055\"]], PROJECTION[\"Mercator\"], PARAMETER[\"False_Easting\", 0], PARAMETER[\"False_Northing\", 0], PARAMETER[\"Central_Meridian\", 0], PARAMETER[\"Latitude_of_origin\", 0], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], AXIS[\"East\", EAST], AXIS[\"North\", NORTH], AUTHORITY[\"EPSG\",\"3857\"]]");
            return ctf.CreateFromCoordinateSystems(epsg4326, epsg3857);
#else
        private static DotSpatial.Projections.ICoordinateTransformation GetCoordinateTransformation()
        {
            var epsg4326 = DotSpatial.Projections.KnownCoordinateSystems.Geographic.World.WGS1984;
            var epsg3857 = DotSpatial.Projections.ProjectionInfo.FromEsriString("PROJCS[\"Popular Visualisation CRS / Mercator\", GEOGCS[\"Popular Visualisation CRS\", DATUM[\"Popular Visualisation Datum\", SPHEROID[\"Popular Visualisation Sphere\", 6378137, 0, AUTHORITY[\"EPSG\",\"7059\"]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[\"EPSG\",\"6055\"]],PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9102\"]], AXIS[\"E\", EAST], AXIS[\"N\", NORTH], AUTHORITY[\"EPSG\",\"4055\"]], PROJECTION[\"Mercator\"], PARAMETER[\"False_Easting\", 0], PARAMETER[\"False_Northing\", 0], PARAMETER[\"Central_Meridian\", 0], PARAMETER[\"Latitude_of_origin\", 0], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], AXIS[\"East\", EAST], AXIS[\"North\", NORTH], AUTHORITY[\"EPSG\",\"3857\"]]");
            return new DotSpatial.Projections.CoordinateTransformation { Source = epsg4326, Target = epsg3857 };
#endif
        }

        private static SharpMap.Map InitializeMapBing(BruTile.Web.BingMapType mt)
        {
            var map = new SharpMap.Map();

            var tileLayer = new SharpMap.Layers.TileLayer(new BruTile.Web.BingTileSource(BruTile.Web.BingRequest.UrlBingStaging, "", mt), "TileLayer - Bing " + mt);
            map.BackgroundLayer.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }



        private static SharpMap.Map InitializeMapGoogle(BruTile.Web.GoogleMapType mt)
        {
            var map = new SharpMap.Map();

            BruTile.Web.GoogleRequest req;
            BruTile.ITileSource tileSource;
            SharpMap.Layers.TileLayer tileLayer;

            if (mt == (BruTile.Web.GoogleMapType.GoogleSatellite | BruTile.Web.GoogleMapType.GoogleLabels))
            {
                req = new BruTile.Web.GoogleRequest(BruTile.Web.GoogleMapType.GoogleSatellite);
                tileSource = new BruTile.Web.GoogleTileSource(req);
                tileLayer = new SharpMap.Layers.TileLayer(tileSource, "TileLayer - " + BruTile.Web.GoogleMapType.GoogleSatellite);
                map.Layers.Add(tileLayer);
                req = new BruTile.Web.GoogleRequest(BruTile.Web.GoogleMapType.GoogleLabels);
                tileSource = new BruTile.Web.GoogleTileSource(req);
                mt = BruTile.Web.GoogleMapType.GoogleLabels;
            }
            else
            {
                req = new BruTile.Web.GoogleRequest(mt);
                tileSource = new BruTile.Web.GoogleTileSource(req);
            }

            tileLayer = new SharpMap.Layers.TileLayer(tileSource, "TileLayer - " + mt);
            map.Layers.Add(tileLayer);
            map.ZoomToBox(tileLayer.Envelope);
            return map;
        }

        private class PublicTransportTheme
        {

            private static readonly System.Drawing.PointF[] ArrowPoints =
                new[]
                    {
                        new System.Drawing.PointF(0, 35), new System.Drawing.PointF(6, 0),
                        new System.Drawing.PointF(12, 35), new System.Drawing.PointF(0, 35)
                    };
            private static System.Drawing.Image ColoredArrow(System.Drawing.Brush c)
            {
                var bmp = new System.Drawing.Bitmap(13, 36);

                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.Clear(System.Drawing.Color.Wheat);
                    g.FillPolygon(c, ArrowPoints);
                    g.DrawPolygon(System.Drawing.Pens.Black, ArrowPoints);
                }

                bmp.MakeTransparent(System.Drawing.Color.Wheat);
                return bmp;
            }

            readonly System.Drawing.Brush _brush;

            public PublicTransportTheme(System.Drawing.Brush brush)
            {
                _brush = brush;
            }
            public SharpMap.Styles.IStyle GetStyle(SharpMap.Data.FeatureDataRow fdr)
            {
                var retval = new SharpMap.Styles.VectorStyle();

                if (fdr["Bearing"] == DBNull.Value)
                {
                    var bmp = new System.Drawing.Bitmap(36, 36);
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        g.Clear(System.Drawing.Color.Wheat);
                        g.FillEllipse(System.Drawing.Brushes.Green, 0, 0, 36, 36);
                        g.DrawEllipse(new System.Drawing.Pen(System.Drawing.Brushes.Yellow, 3), 4, 4, 28, 28);
                        g.DrawString("H", new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold),
                                     System.Drawing.Brushes.Yellow,
                                     new System.Drawing.RectangleF(2, 2, 34, 34),
                                     new System.Drawing.StringFormat
                                         {
                                             Alignment = System.Drawing.StringAlignment.Center,
                                             LineAlignment = System.Drawing.StringAlignment.Center
                                         });
                    }
                    bmp.MakeTransparent(System.Drawing.Color.Wheat);
                    retval.Symbol = bmp;
                }
                else
                {
                    retval.Symbol = ColoredArrow(_brush);
                    var rot =  (Single)(Double)fdr["Bearing"];
                    retval.SymbolRotation = rot % 360f;
                }
                return retval;

            }
        }

        /// <summary>
        /// This class is directly derived from GreatMaps
        /// http://gmaps.codeplex.com
        /// </summary>
        private class VilniusTransportData : SharpMap.Data.Providers.GeometryFeatureProvider
        {

            private bool _isActive;
            private readonly System.Timers.Timer _reQuery = new System.Timers.Timer(5000);

            public enum TransportType
            {
                Bus, TrolleyBus,
            }

            private static SharpMap.Data.FeatureDataTable VehicleDataTable()
            {
                var dt = new SharpMap.Data.FeatureDataTable { TableName = "VilniusTransportData" };
                System.Data.DataColumnCollection dcc = dt.Columns;
                dcc.AddRange(new[]
                                  {
                                      new System.Data.DataColumn("Id", typeof(int)), 
                                      //new System.Data.DataColumn("Lat", typeof(double)), 
                                      //new System.Data.DataColumn("Lng", typeof(double)), 
                                      new System.Data.DataColumn("Line", typeof(string)), 
                                      new System.Data.DataColumn("LastStop", typeof(string)), 
                                      new System.Data.DataColumn("TrackType", typeof(string)), 
                                      new System.Data.DataColumn("AreaName", typeof(string)), 
                                      new System.Data.DataColumn("StreetName", typeof(string)), 
                                      new System.Data.DataColumn("Time", typeof(string)), 
                                      new System.Data.DataColumn("Bearing", typeof(double)) 
                                  });
                return dt;
            }

            private readonly TransportType _transportType;

            public VilniusTransportData(TransportType transportType)
                :base(VehicleDataTable())
            {
                _transportType = transportType;
                _reQuery.Elapsed += HandleTimerElapsed;
                _reQuery.Start();
                GetVilniusTransportData(String.Empty);
            }

            private void HandleTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                GetVilniusTransportData(string.Empty);
            }

            /// <summary>
            /// timeout for map connections
            /// </summary>
            private const int Timeout = 30*1000;

            private readonly GeoAPI.Geometries.IGeometryFactory _factory =
                GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);

            /// <summary>
            /// gets realtime data from public transport in city vilnius of lithuania
            /// </summary>
            private void GetVilniusTransportData(string line)
            {
                if (_isActive) return;
                _isActive = true;

                //List<FeatureDataRow> newFeatures = new List<FeatureDataRow>();
                var fdt = VehicleDataTable();

                string url = "http://www.troleibusai.lt/puslapiai/services/vehiclestate.php?type=";

                switch (_transportType)
                {
                    case TransportType.Bus:
                        {
                            url += "bus";
                        }
                        break;

                    case TransportType.TrolleyBus:
                        {
                            url += "trolley";
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(line))
                {
                    url += "&line=" + line;
                }

                url += "&app=SharpMap.WinFormSamples";

                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                request.Timeout = Timeout;
                request.ReadWriteTimeout = request.Timeout;
                request.Accept = "*/*";
                request.KeepAlive = false;

                string xml;

                using (var response = request.GetResponse() as System.Net.HttpWebResponse)
                {
                    if (response == null)
                        return;

                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                            return;
                        using (var read = new System.IO.StreamReader(responseStream))
                        {
                            xml = read.ReadToEnd();
                        }
                    }
                }

                var doc = new System.Xml.XmlDocument();
                {
                    doc.LoadXml(xml);

                    var devices = doc.GetElementsByTagName("Device");
                    foreach (System.Xml.XmlNode dev in devices)
                    {
                        if (dev.Attributes == null) continue;

                        double? lat = null, lng = null;
                        SharpMap.Data.FeatureDataRow dr = fdt.NewRow();
                        dr["Id"] = int.Parse(dev.Attributes["ID"].InnerText);
                        foreach (System.Xml.XmlElement elem in dev.ChildNodes)
                        {
                            // Debug.WriteLine(d.Id + "->" + elem.Name + ": " + elem.InnerText);

                            switch (elem.Name)
                            {
                                case "Lat":
                                    lat = double.Parse(elem.InnerText, System.Globalization.CultureInfo.InvariantCulture);
                                    break;

                                case "Lng":
                                    lng = double.Parse(elem.InnerText, System.Globalization.CultureInfo.InvariantCulture);
                                    break;

                                case "Bearing":
                                    if (!string.IsNullOrEmpty(elem.InnerText))
                                        dr["Bearing"] = double.Parse(elem.InnerText, System.Globalization.CultureInfo.InvariantCulture);
                                    break;

                                case "LineNum":
                                    dr["Line"] = elem.InnerText;
                                    break;

                                case "AreaName":
                                    dr["AreaName"] = elem.InnerText;
                                    break;

                                case "StreetName":
                                    dr["StreetName"] = elem.InnerText;
                                    break;

                                case "TrackType":
                                    dr["TrackType"] = elem.InnerText;
                                    break;

                                case "LastStop":
                                    dr["LastStop"] = elem.InnerText;
                                    break;

                                case "Time":
                                    dr["Time"] = elem.InnerText;
                                    break;
                            }
                        }

                        if (lat.HasValue && lng.HasValue)
                        {
                            dr.Geometry = _factory.CreatePoint(new GeoAPI.Geometries.Coordinate(lng.Value, lat.Value));
                            fdt.Rows.Add(dr);
                        }
                    }
                }

                Features.Clear();

                foreach (SharpMap.Data.FeatureDataRow featureDataRow in fdt.Rows)
                {
                    var fdr = Features.NewRow();
                    fdr.ItemArray = featureDataRow.ItemArray;
                    fdr.Geometry = featureDataRow.Geometry;
                    Features.AddRow(fdr);
                }
                Features.AcceptChanges();

                _isActive = false;
            }
        }

    }
}
