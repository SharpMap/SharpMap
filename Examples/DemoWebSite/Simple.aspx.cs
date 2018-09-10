using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using SharpMap;
using SharpMap.Web;
using Point=GeoAPI.Geometries.Coordinate;

public partial class Simple : Page
{
    private Map myMap;

    protected void Page_Load(object sender, EventArgs e)
    {
        //Set up the map. We use the method in the App_Code folder for initializing the map
        myMap = MapHelper.InitializeMap(new Size((int) imgMap.Width.Value, (int) imgMap.Height.Value));
        if (Page.IsPostBack)
        {
            //Page is post back. Restore center and zoom-values from viewstate
            myMap.Center = (Point) ViewState["mapCenter"];
            myMap.Zoom = (double) ViewState["mapZoom"];
        }
        else
        {
            //This is the initial view of the map. Zoom to the extents of the map:
            //myMap.ZoomToExtents();
            //or center on 0,0 and zoom to full earth (360 degrees)
            //myMap.Center = new GeoAPI.Geometries.Coordinate(0,0);
            //myMap.Zoom = 360;
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
            myMap.Zoom = myMap.Zoom*0.5;
        else if (rblMapTools.SelectedValue == "1") //Zoom out
            myMap.Zoom = myMap.Zoom*2;
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
        //Render map
        Image img = myMap.GetMap();
        string imgID = Caching.InsertIntoCache(1, img);
        imgMap.ImageUrl = "GetMap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
    }
}
