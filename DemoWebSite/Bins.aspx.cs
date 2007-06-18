using System;
using System.Data;
using System.Drawing;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using SharpMap.Rendering.Thematics;

public partial class Bins : System.Web.UI.Page
{
	private SharpMap.Map myMap;

	/// <summary>
	/// This method is used for determining the color of country based on attributes.
	/// It is used as a delegate for the CustomTheme class.
	/// </summary>
	/// <param name="row"></param>
	/// <returns></returns>
	private SharpMap.Styles.VectorStyle GetCountryStyle(SharpMap.Data.FeatureDataRow row)
	{
		SharpMap.Styles.VectorStyle style = new SharpMap.Styles.VectorStyle();
		switch (row["NAME"].ToString().ToLower())
		{
			case "denmark": //If country name is Danmark, fill it with green
				style.Fill = Brushes.Green;
				return style;
			case "united states": //If country name is USA, fill it with Blue and add a red outline
				style.Fill = Brushes.Blue;
				style.Outline = Pens.Red;
				return style;
			case "china": //If country name is China, fill it with red
				style.Fill = Brushes.Red;
				return style;
			default:
				break;
		}
		//If country name starts with S make it yellow
		if (row["NAME"].ToString().StartsWith("S"))
		{
			style.Fill = Brushes.Yellow;
			return style;
		}
		// If geometry is a (multi)polygon and the area of the polygon is less than 30, make it cyan
		else if (row.Geometry.GetType() == typeof(SharpMap.Geometries.MultiPolygon) && (row.Geometry as SharpMap.Geometries.MultiPolygon).Area < 30 ||
			row.Geometry.GetType() == typeof(SharpMap.Geometries.Polygon) && (row.Geometry as SharpMap.Geometries.Polygon).Area < 30 )
		{
			style.Fill = Brushes.Cyan;
			return style;
		}
		else //None of the above -> Use the default style
			return null;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		//Set up the map. We use the method in the App_Code folder for initializing the map
		myMap = MapHelper.InitializeMap(new System.Drawing.Size((int)imgMap.Width.Value,(int)imgMap.Height.Value));
		//Set a gradient theme on the countries layer, based on Population density
		SharpMap.Rendering.Thematics.CustomTheme iTheme = new SharpMap.Rendering.Thematics.CustomTheme(GetCountryStyle);
		SharpMap.Styles.VectorStyle defaultstyle = new SharpMap.Styles.VectorStyle();
		defaultstyle.Fill = Brushes.Gray;
		iTheme.DefaultStyle = defaultstyle;
		(myMap.Layers[0] as SharpMap.Layers.VectorLayer).Theme = iTheme;
		//Turn off the river layer and label-layers
		myMap.Layers[1].Enabled = false;
		myMap.Layers[3].Enabled = false;
		myMap.Layers[4].Enabled = false;
		
		if (Page.IsPostBack) 
		{
			//Page is post back. Restore center and zoom-values from viewstate
			myMap.Center = (SharpMap.Geometries.Point)ViewState["mapCenter"];
			myMap.Zoom = (double)ViewState["mapZoom"];
		}
		else
		{
			//This is the initial view of the map. Zoom to the extents of the map:
			//myMap.ZoomToExtents();
			myMap.Center = new SharpMap.Geometries.Point(0,0);
			myMap.Zoom = 360;
			//Create the map
			GenerateMap();
		}
	}
  
	protected void imgMap_Click(object sender, ImageClickEventArgs e)
	{
		//Set center of the map to where the client clicked
		myMap.Center = myMap.ImageToWorld(new System.Drawing.Point(e.X, e.Y));
		//Set zoom value if any of the zoom tools were selected
		if (rblMapTools.SelectedValue == "0") //Zoom in
			myMap.Zoom = myMap.Zoom * 0.5;
		else if (rblMapTools.SelectedValue == "1") //Zoom out
			myMap.Zoom = myMap.Zoom * 2;
		//Create the map
		GenerateMap();
	}
  
	/// <summary>
	/// Creates the map, inserts it into the cache and sets the ImageButton Url
	/// </summary>
	private void GenerateMap()
	{
		//Save the current mapcenter and zoom in the viewstate
		ViewState.Add("mapCenter", myMap.Center);
		ViewState.Add("mapZoom", myMap.Zoom);
		System.Drawing.Image img = myMap.GetMap();
		string imgID = SharpMap.Web.Caching.InsertIntoCache(1, img);
		imgMap.ImageUrl = "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
	}
}
