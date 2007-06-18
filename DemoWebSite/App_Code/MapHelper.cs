using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Drawing.Drawing2D;

/// <summary>
/// Summary description for CreateMap
/// </summary>
public class MapHelper
{
    public static SharpMap.Map InitializeMap(System.Drawing.Size size)
    {
			HttpContext.Current.Trace.Write("Initializing map...");
				
			//Initialize a new map of size 'imagesize'
			SharpMap.Map map = new SharpMap.Map(size);
					
			//Set up the countries layer
			SharpMap.Layers.VectorLayer layCountries = new SharpMap.Layers.VectorLayer("Countries");
			//Set the datasource to a shapefile in the App_data folder
			layCountries.DataSource = new SharpMap.Data.Providers.ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\countries.shp"), true);
			
			//Set fill-style to green
			layCountries.Style.Fill = new SolidBrush(Color.Green);
			//Set the polygons to have a black outline
			layCountries.Style.Outline = System.Drawing.Pens.Black;
			layCountries.Style.EnableOutline = true;
			layCountries.SRID = 4326;
			
			//Set up a river layer
			SharpMap.Layers.VectorLayer layRivers = new SharpMap.Layers.VectorLayer("Rivers");
			//Set the datasource to a shapefile in the App_data folder
			layRivers.DataSource = new SharpMap.Data.Providers.ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\rivers.shp"), true);
			//Define a blue 1px wide pen
			layRivers.Style.Line = new Pen(Color.Blue,1);
			layRivers.SRID = 4326;

			//Set up a river layer
			SharpMap.Layers.VectorLayer layCities = new SharpMap.Layers.VectorLayer("Cities");
			//Set the datasource to a shapefile in the App_data folder
			layCities.DataSource = new SharpMap.Data.Providers.ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\cities.shp"), true);
			//Define a blue 1px wide pen
			//layCities.Style.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
			layCities.Style.SymbolScale = 0.8f;
			layCities.MaxVisible = 40;
			layCities.SRID = 4326;

			//Set up a country label layer
			SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels");
			layLabel.DataSource = layCountries.DataSource;
			layLabel.Enabled = true;
			layLabel.LabelColumn = "Name";
			layLabel.Style = new SharpMap.Styles.LabelStyle();
			layLabel.Style.ForeColor = Color.White;
			layLabel.Style.Font = new Font(FontFamily.GenericSerif, 12);
			layLabel.Style.BackColor = new System.Drawing.SolidBrush(Color.FromArgb(128,255,0,0));
			layLabel.MaxVisible = 90;
			layLabel.MinVisible = 30;
			layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
			layLabel.SRID = 4326;
			layLabel.MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest;

			//Set up a city label layer
			SharpMap.Layers.LabelLayer layCityLabel = new SharpMap.Layers.LabelLayer("City labels");
			layCityLabel.DataSource = layCities.DataSource;
			layCityLabel.Enabled = true;
			layCityLabel.LabelColumn = "Name";
			layCityLabel.Style = new SharpMap.Styles.LabelStyle();
			layCityLabel.Style.ForeColor = Color.Black;
			layCityLabel.Style.Font = new Font(FontFamily.GenericSerif, 11);
			layCityLabel.MaxVisible = layLabel.MinVisible;
			layCityLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left;
			layCityLabel.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Bottom;
			layCityLabel.Style.Offset = new PointF(3, 3);
			layCityLabel.Style.Halo = new Pen(Color.Yellow, 2);
			layCityLabel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			layCityLabel.SmoothingMode = SmoothingMode.AntiAlias;
			layCityLabel.SRID = 4326;
			layCityLabel.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
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
			map.Center = new SharpMap.Geometries.Point(0,0);
				
			HttpContext.Current.Trace.Write("Map initialized");
			return map;
    }

	public static SharpMap.Map InitializeGradientMap(System.Drawing.Size size)
	{
		//Initialize a new map based on the simple map
		SharpMap.Map map = InitializeMap(size);
		//Set a gradient theme on the countries layer, based on Population density
		//First create two styles that specify min and max styles
		//In this case we will just use the default values and override the fill-colors
		//using a colorblender. If different line-widths, line- and fill-colors where used
		//in the min and max styles, these would automatically get linearly interpolated.
		SharpMap.Styles.VectorStyle min = new SharpMap.Styles.VectorStyle();
		SharpMap.Styles.VectorStyle max = new SharpMap.Styles.VectorStyle();
		//Create theme using a density from 0 (min) to 400 (max)
		SharpMap.Rendering.Thematics.GradientTheme popdens = new SharpMap.Rendering.Thematics.GradientTheme("PopDens", 0, 400, min, max);
		//We can make more advanced coloring using the ColorBlend'er.
		//Setting the FillColorBlend will override any fill-style in the min and max fills.
		//In this case we just use the predefined Rainbow colorscale
		popdens.FillColorBlend = SharpMap.Rendering.Thematics.ColorBlend.Rainbow5;
		(map.Layers[0] as SharpMap.Layers.VectorLayer).Theme = popdens;

		//Lets scale the labels so that big countries have larger texts as well
		SharpMap.Styles.LabelStyle lblMin = new SharpMap.Styles.LabelStyle();
		SharpMap.Styles.LabelStyle lblMax = new SharpMap.Styles.LabelStyle();
		lblMin.ForeColor = Color.Black;
		lblMin.Font = new Font(FontFamily.GenericSerif, 6);
		lblMax.ForeColor = Color.Blue;
		lblMax.BackColor = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
		lblMin.BackColor = lblMax.BackColor;
		lblMax.Font = new Font(FontFamily.GenericSerif, 9);
		(map.Layers[3] as SharpMap.Layers.LabelLayer).Theme = new SharpMap.Rendering.Thematics.GradientTheme("PopDens", 0, 400, lblMin, lblMax);		
		
		//Lets scale city icons based on city population
		//cities below 1.000.000 gets the smallest symbol, and cities with more than 5.000.000 the largest symbol
		SharpMap.Styles.VectorStyle citymin = new SharpMap.Styles.VectorStyle();
		SharpMap.Styles.VectorStyle citymax = new SharpMap.Styles.VectorStyle();
		citymin.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
		citymin.SymbolScale = 0.5f;
		citymax.Symbol = new Bitmap(HttpContext.Current.Server.MapPath(@"~\App_data\icon.png"));
		citymax.SymbolScale = 1f;
		(map.Layers[2] as SharpMap.Layers.VectorLayer).Theme = new SharpMap.Rendering.Thematics.GradientTheme("Population", 1000000, 5000000, citymin, citymax);
	
		//Turn off the river layer
		map.Layers[1].Enabled = false;
		return map;
	}

	public static SharpMap.Layers.WmsLayer GetWmsLayer()
	{
		string wmsUrl = "http://www2.demis.nl/mapserver/request.asp";
		SharpMap.Layers.WmsLayer layWms = new SharpMap.Layers.WmsLayer("Demis Map", wmsUrl);
		layWms.SpatialReferenceSystem = "EPSG:4326";		
		layWms.AddLayer("Bathymetry");
		layWms.AddLayer("Ocean features");
		layWms.SetImageFormat(layWms.OutputFormats[0]);
		layWms.ContinueOnError = true; //Skip rendering the WMS Map if the server couldn't be requested (if set to false such an event would crash the app)
		layWms.TimeOut = 5000; //Set timeout to 5 seconds
		layWms.SRID = 4326;
		return layWms;
	}
	
	public static SharpMap.Map InitializeWmsMap(System.Drawing.Size size)
	{
		HttpContext.Current.Trace.Write("Initializing Wms map...");
				
			//Initialize a new map of size 'imagesize'
			SharpMap.Map map = new SharpMap.Map(size);
			SharpMap.Layers.WmsLayer layWms = GetWmsLayer();
			//Set up the countries layer
			SharpMap.Layers.VectorLayer layCountries = new SharpMap.Layers.VectorLayer("Countries");
			//Set the datasource to a shapefile in the App_data folder
			layCountries.DataSource = new SharpMap.Data.Providers.ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\countries.shp"), true);
			//Set fill-style to green
			layCountries.Style.Fill = new SolidBrush(Color.Green);
			//Set the polygons to have a black outline
			layCountries.Style.Outline = System.Drawing.Pens.Yellow;
			layCountries.Style.EnableOutline = true;
			layCountries.SRID = 4326;
						
			//Set up a country label layer
			SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("Country labels");
			layLabel.DataSource = layCountries.DataSource;
			layLabel.Enabled = true;
			layLabel.LabelColumn = "Name";
			layLabel.Style = new SharpMap.Styles.LabelStyle();
			layLabel.Style.ForeColor = Color.White;
			layLabel.Style.Font = new Font(FontFamily.GenericSerif, 8);
			layLabel.Style.BackColor = new System.Drawing.SolidBrush(Color.FromArgb(128,255,0,0));
			layLabel.MaxVisible = 90;
			layLabel.MinVisible = 30;
			layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
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
			map.Center = new SharpMap.Geometries.Point(0,0);
				
			HttpContext.Current.Trace.Write("Map initialized");
			return map;
		}
}
