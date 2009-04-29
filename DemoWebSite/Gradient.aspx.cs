using System;
using System.Drawing;
using System.Globalization;
using System.Web.UI;
using SharpMap;
using Point=SharpMap.Geometries.Point;

public partial class Gradient : Page
{
    private Point Center;
    private double Zoom;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack)
        {
            //Page is post back. Restore center and zoom-values from viewstate
            Center = (Point) ViewState["mapCenter"];
            Zoom = (double) ViewState["mapZoom"];
        }
        else
        {
            //This is the initial view of the map.
            Center = new Point(12, 48);
            Zoom = 45;
            //Create the map
            GenerateMap();
        }
    }

    protected void imgMap_Click(object sender, ImageClickEventArgs e)
    {
        //Set center of the map to where the client clicked
        //We set up a simple empty map so we can use the ImageToWorld() method for easy conversion from Image to World coordinates
        Map myMap = new Map(new Size(Convert.ToInt32(imgMap.Width.Value), Convert.ToInt32(imgMap.Height.Value)));
        myMap.Center = Center;
        myMap.Zoom = Zoom;
        Center = myMap.ImageToWorld(new System.Drawing.Point(e.X, e.Y));

        //Set zoom value if any of the zoom tools were selected
        if (rblMapTools.SelectedValue == "0") //Zoom in
            Zoom = Zoom*0.5;
        else if (rblMapTools.SelectedValue == "1") //Zoom out
            Zoom = Zoom*2;
        //Create the map
        GenerateMap();
    }

    /// <summary>
    /// Creates the map, inserts it into the cache and sets the ImageButton Url
    /// </summary>
    private void GenerateMap()
    {
        //Save the current mapcenter and zoom in the viewstate
        ViewState.Add("mapCenter", Center);
        ViewState.Add("mapZoom", Zoom);

        string ResponseFormat = "maphandler.ashx?MAP=Gradient&Width=[WIDTH]&Height=[HEIGHT]&Zoom=[ZOOM]&X=[X]&Y=[Y]";
        NumberFormatInfo numberFormat_EnUS = new CultureInfo("en-US", false).NumberFormat;
        imgMap.ImageUrl = ResponseFormat.Replace("[WIDTH]", imgMap.Width.Value.ToString()).
            Replace("[HEIGHT]", imgMap.Height.Value.ToString()).
            Replace("[ZOOM]", Zoom.ToString(numberFormat_EnUS)).
            Replace("[X]", Center.X.ToString(numberFormat_EnUS)).
            Replace("[Y]", Center.Y.ToString(numberFormat_EnUS));
    }
}