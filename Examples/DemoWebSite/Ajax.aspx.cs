using System;
using System.Drawing;
using System.Web.UI;
using Point=GeoAPI.Geometries.Coordinate;

public partial class Ajax : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        ajaxMap.Map = MapHelper.InitializeMap(new Size(10, 10));
        if (!Page.IsPostBack && !Page.IsCallback)
        {
            //Set up the map. We use the method in the App_Code folder for initializing the map
            ajaxMap.Map.Center = new Point(0, 20);
            ajaxMap.FadeSpeed = 10;
            ajaxMap.ZoomSpeed = 10;
            ajaxMap.Map.Zoom = 360;
        }
        ajaxMap.ResponseFormat = "maphandler.ashx?MAP=SimpleWorld&Width=[WIDTH]&Height=[HEIGHT]&Zoom=[ZOOM]&X=[X]&Y=[Y]";
    }
}