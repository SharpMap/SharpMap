using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using SharpMap;
using SharpMap.Data;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using SharpMap.Web;
using Point=SharpMap.Geometries.Point;

public partial class Bins : Page
{
    private Map myMap;

    /// <summary>
    /// This method is used for determining the color of country based on attributes.
    /// It is used as a delegate for the CustomTheme class.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private VectorStyle GetCountryStyle(FeatureDataRow row)
    {
        VectorStyle style = new VectorStyle();
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
        else if (row.Geometry.GetType() == typeof (MultiPolygon) && (row.Geometry as MultiPolygon).Area < 30 ||
                 row.Geometry.GetType() == typeof (Polygon) && (row.Geometry as Polygon).Area < 30)
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
        myMap = MapHelper.InitializeMap(new Size((int) imgMap.Width.Value, (int) imgMap.Height.Value));
        //Set a gradient theme on the countries layer, based on Population density
        CustomTheme iTheme = new CustomTheme(GetCountryStyle);
        VectorStyle defaultstyle = new VectorStyle();
        defaultstyle.Fill = Brushes.Gray;
        iTheme.DefaultStyle = defaultstyle;
        (myMap.Layers[0] as VectorLayer).Theme = iTheme;
        //Turn off the river layer and label-layers
        myMap.Layers[1].Enabled = false;
        myMap.Layers[3].Enabled = false;
        myMap.Layers[4].Enabled = false;

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
            myMap.Center = new Point(0, 0);
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
        Image img = myMap.GetMap();
        string imgID = Caching.InsertIntoCache(1, img);
        imgMap.ImageUrl = "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
    }
}