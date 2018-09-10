using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Web;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using ColorBlend=SharpMap.Rendering.Thematics.ColorBlend;
using Point=GeoAPI.Geometries.Coordinate;

/// <summary>
/// Summary description for CreateMap
/// </summary>
public class MapHelper
{
    static MapHelper()
    {

        var gss = GeoAPI.GeometryServiceProvider.Instance;
        var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
            new ProjNet.CoordinateSystems.CoordinateSystemFactory(System.Text.Encoding.ASCII),
            new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory(),
            SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

        GeoAPI.GeometryServiceProvider.Instance = gss;
        SharpMap.Session.Instance
            .SetGeometryServices(gss)
            .SetCoordinateSystemServices(css)
            .SetCoordinateSystemRepository(css);
    }
    public static Map InitializeMap(Size size)
    {
			HttpContext.Current.Trace.Write("Initializing map...");

			//Initialize a new map of size 'imagesize'
        Map map = new Map(size);

			//Set up the countries layer
        VectorLayer layCountries = new VectorLayer("Countries");
			//Set the datasource to a shapefile in the App_data folder
        layCountries.DataSource = new ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\countries.shp"), true);

			//Set fill-style to green
			layCountries.Style.Fill = new SolidBrush(Color.Green);
			//Set the polygons to have a black outline
        layCountries.Style.Outline = Pens.Black;
			layCountries.Style.EnableOutline = true;
			layCountries.SRID = 4326;

			//Set up a river layer
        VectorLayer layRivers = new VectorLayer("Rivers");
			//Set the datasource to a shapefile in the App_data folder
        layRivers.DataSource = new ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\rivers.shp"), true);
			//Define a blue 1px wide pen
        layRivers.Style.Line = new Pen(Color.Blue, 1);
			layRivers.SRID = 4326;

			//Set up a river layer
        VectorLayer layCities = new VectorLayer("Cities");
			//Set the datasource to a shapefile in the App_data folder
        layCities.DataSource = new ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\cities.shp"), true);
			//Define a blue 1px wide pen
			//layCities.Style.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
			layCities.Style.SymbolScale = 0.8f;
			layCities.MaxVisible = 40;
			layCities.SRID = 4326;

			//Set up a country label layer
        LabelLayer layLabel = new LabelLayer("Country labels");
			layLabel.DataSource = layCountries.DataSource;
			layLabel.Enabled = true;
			layLabel.LabelColumn = "Name";
        layLabel.Style = new LabelStyle();
			layLabel.Style.ForeColor = Color.White;
			layLabel.Style.Font = new Font(FontFamily.GenericSerif, 12);
        layLabel.Style.BackColor = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
			layLabel.MaxVisible = 90;
			layLabel.MinVisible = 30;
        layLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
			layLabel.SRID = 4326;
        layLabel.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest;

			//Set up a city label layer
        LabelLayer layCityLabel = new LabelLayer("City labels");
			layCityLabel.DataSource = layCities.DataSource;
			layCityLabel.Enabled = true;
			layCityLabel.LabelColumn = "Name";
        layCityLabel.Style = new LabelStyle();
			layCityLabel.Style.ForeColor = Color.Black;
			layCityLabel.Style.Font = new Font(FontFamily.GenericSerif, 11);
			layCityLabel.MaxVisible = layLabel.MinVisible;
        layCityLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
        layCityLabel.Style.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
			layCityLabel.Style.Offset = new PointF(3, 3);
			layCityLabel.Style.Halo = new Pen(Color.Yellow, 2);
        layCityLabel.TextRenderingHint = TextRenderingHint.AntiAlias;
			layCityLabel.SmoothingMode = SmoothingMode.AntiAlias;
			layCityLabel.SRID = 4326;
        layCityLabel.LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection;
			layCityLabel.Style.CollisionDetection = true;

			//Add the layers to the map object.
			//The order we add them in are the order they are drawn, so we add the rivers last to put them on top
			map.Layers.Add(layCountries);
			map.Layers.Add(layRivers);
			map.Layers.Add(layCities);
			map.Layers.Add(layLabel);
			map.Layers.Add(layCityLabel);


			//limit the zoom to 360 degrees width
			map.MaximumZoom = 360;
			map.BackColor = Color.LightBlue;

			map.Zoom = 360;
        map.Center = new Point(0, 0);

			HttpContext.Current.Trace.Write("Map initialized");
			return map;
    }

    public static Map InitializeGradientMap(Size size)
	{
		//Initialize a new map based on the simple map
        Map map = InitializeMap(size);
		//Set a gradient theme on the countries layer, based on Population density
		//First create two styles that specify min and max styles
		//In this case we will just use the default values and override the fill-colors
		//using a colorblender. If different line-widths, line- and fill-colors where used
		//in the min and max styles, these would automatically get linearly interpolated.
        VectorStyle min = new VectorStyle();
        VectorStyle max = new VectorStyle();
		//Create theme using a density from 0 (min) to 400 (max)
        GradientTheme popdens = new GradientTheme("PopDens", 0, 400, min, max);
		//We can make more advanced coloring using the ColorBlend'er.
		//Setting the FillColorBlend will override any fill-style in the min and max fills.
		//In this case we just use the predefined Rainbow colorscale
        popdens.FillColorBlend = ColorBlend.Rainbow5;
        (map.Layers[0] as VectorLayer).Theme = popdens;

		//Lets scale the labels so that big countries have larger texts as well
        LabelStyle lblMin = new LabelStyle();
        LabelStyle lblMax = new LabelStyle();
		lblMin.ForeColor = Color.Black;
		lblMin.Font = new Font(FontFamily.GenericSerif, 6);
		lblMax.ForeColor = Color.Blue;
		lblMax.BackColor = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
		lblMin.BackColor = lblMax.BackColor;
		lblMax.Font = new Font(FontFamily.GenericSerif, 9);
        (map.Layers[3] as LabelLayer).Theme = new GradientTheme("PopDens", 0, 400, lblMin, lblMax);

		//Lets scale city icons based on city population
		//cities below 1.000.000 gets the smallest symbol, and cities with more than 5.000.000 the largest symbol
        VectorStyle citymin = new VectorStyle();
        VectorStyle citymax = new VectorStyle();
		citymin.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
		citymin.SymbolScale = 0.5f;
		citymax.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
		citymax.SymbolScale = 1f;
        (map.Layers[2] as VectorLayer).Theme = new GradientTheme("Population", 1000000, 5000000, citymin, citymax);

		//Turn off the river layer
		map.Layers[1].Enabled = false;
		return map;
	}

    public static WmsLayer GetWmsLayer()
	{
		string wmsUrl = "http://www2.demis.nl/worldmap/wms.asp";
        WmsLayer layWms = new WmsLayer("Demis Map", wmsUrl);
        layWms.AddLayer("Bathymetry");
        //layWms.AddLayer("Coastlines");
        //layWms.AddLayer("Countries");
        //layWms.AddLayer("Rivers");
        //layWms.AddLayer("Streams");
        layWms.AddLayer("Ocean features");
		layWms.SetImageFormat(layWms.OutputFormats[0]);
        layWms.ContinueOnError = true;
            //Skip rendering the WMS Map if the server couldn't be requested (if set to false such an event would crash the app)
		layWms.TimeOut = 5000; //Set timeout to 5 seconds
		layWms.SRID = 4326;
		return layWms;
	}

    public static Map InitializeWmsMap(Size size)
	{
		HttpContext.Current.Trace.Write("Initializing Wms map...");

			//Initialize a new map of size 'imagesize'
        Map map = new Map(size);
        WmsLayer layWms = GetWmsLayer();
		
        //Set up the countries layer
        VectorLayer layCountries = new VectorLayer("Countries");
			//Set the datasource to a shapefile in the App_data folder
        layCountries.DataSource = new ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\countries.shp"), true);
			//Set fill-style to green
			layCountries.Style.Fill = new SolidBrush(Color.Green);
			//Set the polygons to have a black outline
        layCountries.Style.Outline = Pens.Yellow;
			layCountries.Style.EnableOutline = true;
			layCountries.SRID = 4326;

			//Set up a country label layer
        LabelLayer layLabel = new LabelLayer("Country labels");
			layLabel.DataSource = layCountries.DataSource;
			layLabel.Enabled = true;
			layLabel.LabelColumn = "Name";
        layLabel.Style = new LabelStyle();
			layLabel.Style.ForeColor = Color.White;
			layLabel.Style.Font = new Font(FontFamily.GenericSerif, 8);
        layLabel.Style.BackColor = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
			layLabel.MaxVisible = 90;
			layLabel.MinVisible = 30;
        layLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
			layLabel.SRID = 4326;
        
			//Add the layers to the map object.
			//The order we add them in are the order they are drawn, so we add the rivers last to put them on top
			map.Layers.Add(layWms);
			map.Layers.Add(layCountries);
			map.Layers.Add(layLabel);

			//limit the zoom to 360 degrees width
			map.MaximumZoom = 360;
			map.BackColor = Color.LightBlue;

			map.Zoom = 360;
        map.Center = new Point(0, 0);

			HttpContext.Current.Trace.Write("Map initialized");
			return map;
		}

    public static Map InitializeMapOgr(Size size)
    {
        HttpContext.Current.Trace.Write("Initializing map...");

        //Initialize a new map of size 'imagesize'
        Map map = new Map(size);

        //Set up the countries layer
        VectorLayer layCountries = new VectorLayer("Countries");
        //Set the datasource to a shapefile in the App_data folder
        try
        {
            layCountries.DataSource =
                new Ogr(HttpContext.Current.Server.MapPath(@"~\App_data\MapInfo\countriesMapInfo.tab"));
        }
        catch (TypeInitializationException ex)
        {
            if (ex.GetType() == typeof (TypeInitializationException))
                throw new Exception(
                    "Please copy the umanaged dll's into your bin folder from javascript:window.location.href='http://www.codeplex.com/SharpMap/Wiki/View.aspx?title=Extensions';.");
        }
        catch (Exception ex)
        {
            throw;
        }

        //Set fill-style to green
        layCountries.Style.Fill = new SolidBrush(Color.Green);
        //Set the polygons to have a black outline
        layCountries.Style.Outline = Pens.Black;
        layCountries.Style.EnableOutline = true;
        layCountries.SRID = 4326;

        //Set up a river layer
        VectorLayer layRivers = new VectorLayer("Rivers");
        //Set the datasource to a shapefile in the App_data folder
        layRivers.DataSource = new Ogr(HttpContext.Current.Server.MapPath(@"~\App_data\MapInfo\riversMapInfo.tab"));
        //Define a blue 1px wide pen
        layRivers.Style.Line = new Pen(Color.Blue, 1);
        layRivers.SRID = 4326;

        //Set up a river layer
        VectorLayer layCities = new VectorLayer("Cities");
        //Set the datasource to a shapefile in the App_data folder
        layCities.DataSource = new Ogr(HttpContext.Current.Server.MapPath(@"~\App_data\MapInfo\citiesMapInfo.tab"));
        //Define a blue 1px wide pen
        //layCities.Style.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
        layCities.Style.SymbolScale = 0.8f;
        layCities.MaxVisible = 40;
        layCities.SRID = 4326;

        //Set up a country label layer
        LabelLayer layLabel = new LabelLayer("Country labels");
        layLabel.DataSource = layCountries.DataSource;
        layLabel.Enabled = true;
        layLabel.LabelColumn = "Name";
        layLabel.Style = new LabelStyle();
        layLabel.Style.ForeColor = Color.White;
        layLabel.Style.Font = new Font(FontFamily.GenericSerif, 12);
        layLabel.Style.BackColor = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        layLabel.MaxVisible = 90;
        layLabel.MinVisible = 30;
        layLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
        layLabel.SRID = 4326;
        layLabel.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest;

        //Set up a city label layer
        LabelLayer layCityLabel = new LabelLayer("City labels");
        layCityLabel.DataSource = layCities.DataSource;
        layCityLabel.Enabled = true;
        layCityLabel.LabelColumn = "Name";
        layCityLabel.Style = new LabelStyle();
        layCityLabel.Style.ForeColor = Color.Black;
        layCityLabel.Style.Font = new Font(FontFamily.GenericSerif, 11);
        layCityLabel.MaxVisible = layLabel.MinVisible;
        layCityLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
        layCityLabel.Style.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
        layCityLabel.Style.Offset = new PointF(3, 3);
        layCityLabel.Style.Halo = new Pen(Color.Yellow, 2);
        layCityLabel.TextRenderingHint = TextRenderingHint.AntiAlias;
        layCityLabel.SmoothingMode = SmoothingMode.AntiAlias;
        layCityLabel.SRID = 4326;
        layCityLabel.LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection;
        layCityLabel.Style.CollisionDetection = true;

        //Add the layers to the map object.
        //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
        map.Layers.Add(layCountries);
        map.Layers.Add(layRivers);
        map.Layers.Add(layCities);
        map.Layers.Add(layLabel);
        map.Layers.Add(layCityLabel);


        //limit the zoom to 360 degrees width
        map.MaximumZoom = 360;
        map.BackColor = Color.LightBlue;

        map.ZoomToExtents(); // = 360;
        map.Center = new Point(0, 0);

        HttpContext.Current.Trace.Write("Map initialized");
        return map;
    }

    public static Map InitializeMapMsSqlSpatial(Size size)
    {
        HttpContext.Current.Trace.Write("Initializing map...");

        string connectionString = ConfigurationManager.ConnectionStrings["mssqlspatial"].ConnectionString;

        //Initialize a new map of size 'imagesize'
        Map map = new Map(size);
        if ( connectionString.Contains("server=XXXXXXXXXXX" ) )
            throw new ArgumentException("You need to adjust the connectionstring to MsSqlSpatial Server");

        //Set up the countries layer
        VectorLayer layCountries = new VectorLayer("Countries");
        //Set the datasource to a new MsSqlSpatialProvider
        layCountries.DataSource = new MsSqlSpatial(connectionString, "Countries", "Geometry", "oid");

        //Set fill-style to green
        layCountries.Style.Fill = new SolidBrush(Color.Green);
        //Set the polygons to have a black outline
        layCountries.Style.Outline = Pens.Black;
        layCountries.Style.EnableOutline = true;
        layCountries.SRID = 4326;

        //Set up a river layer
        VectorLayer layRivers = new VectorLayer("Rivers");
        //Set the datasource to a new MsSqlSpatialProvider
        layRivers.DataSource = new MsSqlSpatial(connectionString, "Rivers", "Geometry", "oid");
        //Define a blue 1px wide pen
        layRivers.Style.Line = new Pen(Color.Blue, 1);
        layRivers.SRID = 4326;

        //Set up a river layer
        VectorLayer layCities = new VectorLayer("Cities");
        //Set the datasource to a new MsSqlSpatialProvider
        layCities.DataSource = new MsSqlSpatial(connectionString, "Cities", "Geometry", "oid");
        ;
        //Define a blue 1px wide pen
        //layCities.Style.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
        layCities.Style.SymbolScale = 0.8f;
        layCities.MaxVisible = 40;
        layCities.SRID = 4326;

        //Set up a country label layer
        LabelLayer layLabel = new LabelLayer("Country labels");
        layLabel.DataSource = layCountries.DataSource;
        layLabel.Enabled = true;
        layLabel.LabelColumn = "Name";
        layLabel.Style = new LabelStyle();
        layLabel.Style.ForeColor = Color.White;
        layLabel.Style.Font = new Font(FontFamily.GenericSerif, 12);
        layLabel.Style.BackColor = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        layLabel.MaxVisible = 90;
        layLabel.MinVisible = 30;
        layLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
        layLabel.SRID = 4326;
        layLabel.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest;

        //Set up a city label layer
        LabelLayer layCityLabel = new LabelLayer("City labels");
        layCityLabel.DataSource = layCities.DataSource;
        layCityLabel.Enabled = true;
        layCityLabel.LabelColumn = "Name";
        layCityLabel.Style = new LabelStyle();
        layCityLabel.Style.ForeColor = Color.Black;
        layCityLabel.Style.Font = new Font(FontFamily.GenericSerif, 11);
        layCityLabel.MaxVisible = layLabel.MinVisible;
        layCityLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
        layCityLabel.Style.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
        layCityLabel.Style.Offset = new PointF(3, 3);
        layCityLabel.Style.Halo = new Pen(Color.Yellow, 2);
        layCityLabel.TextRenderingHint = TextRenderingHint.AntiAlias;
        layCityLabel.SmoothingMode = SmoothingMode.AntiAlias;
        layCityLabel.SRID = 4326;
        layCityLabel.LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection;
        layCityLabel.Style.CollisionDetection = true;

        //Add the layers to the map object.
        //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
        map.Layers.Add(layCountries);
        map.Layers.Add(layRivers);
        map.Layers.Add(layCities);
        map.Layers.Add(layLabel);
        map.Layers.Add(layCityLabel);


        //limit the zoom to 360 degrees width
        map.MaximumZoom = 360;
        map.BackColor = Color.LightBlue;

        map.Zoom = 360;
        map.Center = new Point(0, 0);

        HttpContext.Current.Trace.Write("Map initialized");
        return map;
    }

    public static Map InitializeMapPostGis(Size size)
    {

        HttpContext.Current.Trace.Write("Initializing map...");

        string connectionString = ConfigurationManager.ConnectionStrings["postgis"].ConnectionString;

        //Initialize a new map of size 'imagesize'
        SharpMap.Map map = new SharpMap.Map(size);

        //Set up the countries layer
        SharpMap.Layers.VectorLayer layCountries = new SharpMap.Layers.VectorLayer("Countries");
        //Set the datasource to a new MsSqlSpatialProvider
        layCountries.DataSource = new SharpMap.Data.Providers.PostGIS(connectionString, "countries", "wkb_geometry", "ogc_fid");

        //Set fill-style to green
        layCountries.Style.Fill = new SolidBrush(Color.Green);
        //Set the polygons to have a black outline
        layCountries.Style.Outline = System.Drawing.Pens.Black;
        layCountries.Style.EnableOutline = true;

        //Set up a river layer
        SharpMap.Layers.VectorLayer layRivers = new SharpMap.Layers.VectorLayer("Rivers");
        //Set the datasource to a new MsSqlSpatialProvider
        layRivers.DataSource = new SharpMap.Data.Providers.PostGIS(connectionString, "rivers", "wkb_geometry", "ogc_fid");
        //Define a blue 1px wide pen
        layRivers.Style.Line = new Pen(Color.Blue, 1);

        //Set up a river layer
        SharpMap.Layers.VectorLayer layCities = new SharpMap.Layers.VectorLayer("Cities");
        //Set the datasource to a new MsSqlSpatialProvider
        layCities.DataSource = new SharpMap.Data.Providers.PostGIS(connectionString, "cities", "wkb_geometry", "ogc_fid"); ;
        //Define a blue 1px wide pen
        //layCities.Style.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
        layCities.Style.SymbolScale = 0.8f;
        layCities.MaxVisible = 40;

        //Set up a country label layer
        SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels");
        layLabel.DataSource = layCountries.DataSource;
        layLabel.Enabled = true;
        layLabel.LabelColumn = "Name";
        layLabel.MaxVisible = 90;
        layLabel.MinVisible = 30;
        layLabel.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;
        layLabel.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
        layLabel.PriorityColumn = "popdens";
        layLabel.Style = new SharpMap.Styles.LabelStyle();
        layLabel.Style.ForeColor = Color.White;
        layLabel.Style.Font = new Font(FontFamily.GenericSerif, 12);
        layLabel.Style.BackColor = new System.Drawing.SolidBrush(Color.FromArgb(128, 255, 0, 0));
        layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
        layLabel.Style.CollisionDetection = true;

        //Set up a city label layer
        SharpMap.Layers.LabelLayer layCityLabel = new SharpMap.Layers.LabelLayer("City labels");
        layCityLabel.DataSource = layCities.DataSource;
        layCityLabel.Enabled = true;
        layCityLabel.LabelColumn = "name";
        layCityLabel.PriorityColumn = "population";
        layCityLabel.PriorityDelegate = delegate(SharpMap.Data.FeatureDataRow fdr)
        {
            Int32 retVal = 10000000 * (Int32)((String)fdr["capital"] == "Y" ? 1 : 0);
            return retVal + Convert.ToInt32(fdr["population"]);
        };
        layCityLabel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        layCityLabel.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        layCityLabel.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
        layCityLabel.Style = new SharpMap.Styles.LabelStyle();
        layCityLabel.Style.ForeColor = Color.Black;
        layCityLabel.Style.Font = new Font(FontFamily.GenericSerif, 11);
        layCityLabel.Style.MaxVisible = layLabel.MinVisible;
        layCityLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left;
        layCityLabel.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Bottom;
        layCityLabel.Style.Offset = new PointF(3, 3);
        layCityLabel.Style.Halo = new Pen(Color.Yellow, 2);
        layCityLabel.Style.CollisionDetection = true;
        

        //Add the layers to the map object.
        //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
        map.Layers.Add(layCountries);
        map.Layers.Add(layRivers);
        map.Layers.Add(layCities);
        map.Layers.Add(layLabel);
        map.Layers.Add(layCityLabel);


        //limit the zoom to 360 degrees width
        map.MaximumZoom = 360;
        map.BackColor = Color.LightBlue;

        map.Zoom = 360;
        map.Center = new Point(0, 0);

        HttpContext.Current.Trace.Write("Map initialized");
        return map;
    }

    public static Map InitializeMapGdal(Size size)
    {
        HttpContext.Current.Trace.Write("Initializing map...");

        //Initialize a new map of size 'imagesize'
        Map map = new Map(size);

        //Set up the gdal layer
        LayerGroup g = new LayerGroup("OS");
        g.SRID = 27700;
        //D:\Raster\Ordnance Survey\OS Street View SM\data\sm
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(HttpContext.Current.Server.MapPath(@"~\App_Data\Gdal"));
        foreach (System.IO.FileInfo fi in di.GetFiles("*.tif"))
        {
            try
            {
                SharpMap.Layers.GdalRasterLayer layer = 
                    new SharpMap.Layers.GdalRasterLayer(
                        fi.Name, HttpContext.Current.Server.MapPath(@"~\App_Data\Gdal\" + fi.Name));
                //layer.SRID = 27700;
                g.Layers.Add(layer);
            }
            catch (TypeInitializationException ex)
            {
                throw new Exception(
                    "Please copy the umanaged dll's into your bin folder from javascript:window.location.href='http://www.codeplex.com/SharpMap/Wiki/View.aspx?title=Extensions';.");
            }
        }
        map.Layers.Add(g);
        map.ZoomToExtents();

        HttpContext.Current.Trace.Write("Map initialized");
        return map;
    }
}
