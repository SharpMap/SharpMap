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
	private static Random rand = new Random();

	
	protected void Page_Load(object sender, EventArgs e)
	{
		//Set up the map. We use the method in the App_Code folder for initializing the map and alter it afterwards
		myMap = MapHelper.InitializeMap(new System.Drawing.Size((int)imgMap.Width.Value,(int)imgMap.Height.Value));
		//Remove the river layer and label-layers
		myMap.Layers.RemoveAt(4);
		myMap.Layers.RemoveAt(3);
		myMap.Layers.RemoveAt(1);

		//Create Pie Layer
		SharpMap.Layers.VectorLayer pieLayer = new SharpMap.Layers.VectorLayer("Pie charts");
		pieLayer.DataSource = (myMap.Layers[0] as SharpMap.Layers.VectorLayer).DataSource;
		SharpMap.Rendering.Thematics.CustomTheme iTheme = new SharpMap.Rendering.Thematics.CustomTheme(GetCountryStyle);
		pieLayer.Theme = iTheme;
		myMap.Layers.Add(pieLayer);

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
			myMap.Center = new SharpMap.Geometries.Point(10,50);
			myMap.Zoom = 60;
			//Create the map
			GenerateMap();
		}
	}

	/// <summary>
	/// This method is used for determining the style
	/// It is used as a delegate for the CustomTheme class.
	/// </summary>
	/// <param name="row"></param>
	/// <returns></returns>
	private SharpMap.Styles.VectorStyle GetCountryStyle(SharpMap.Data.FeatureDataRow row)
	{

		SharpMap.Styles.VectorStyle s = new SharpMap.Styles.VectorStyle();
		s.Fill = new SolidBrush(Color.Green);
		s.Symbol = GetPieChart(row);
		return s;
	}


	/// <summary>
	/// Method for creating pie chart symbols
	/// </summary>
	/// <remarks>
	/// <para>In this example we just create some random pie charts, 
	/// but it probably should be based on attributes read from the row.</para>
	///	<para>Credits goes to gonzalo_ar for posting this in the forum</para></remarks>
	/// <param name="row"></param>
	/// <returns></returns>
	private static Bitmap GetPieChart(SharpMap.Data.FeatureDataRow row)
	{

		// Replace polygon with a center point (this is where we place the symbol
		row.Geometry = row.Geometry.GetBoundingBox().GetCentroid();

		// Just for the example I use random values 
		int size = rand.Next(20, 35);
		int angle1 = rand.Next(60, 180);
		int angle2 = rand.Next(angle1 + 60, 300);
		Rectangle rect = new Rectangle(0, 0, size, size);
		System.Drawing.Bitmap b = new Bitmap(size,size);
		Graphics g = Graphics.FromImage(b);

		// Draw Pie 
		g.FillPie(Brushes.LightGreen, rect, 0, angle1);
		g.FillPie(Brushes.Pink, rect, angle1, angle2 - angle1);
		g.FillPie(Brushes.PeachPuff, rect, angle2, 360 - angle2);

		// Draw Borders 
		g.DrawPie(Pens.Green, rect, 0, angle1);
		g.DrawPie(Pens.Red, rect, angle1, angle2 - angle1);
		g.DrawPie(Pens.Orange, rect, angle2, 360 - angle2);
		g.Dispose();
		return b;
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
