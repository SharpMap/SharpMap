using System;
using System.Drawing;
using System.Globalization;
using System.Web.UI;
using GeoAPI.Geometries;
using SharpMap;
using SharpMap.Layers;
using SharpMap.Web.Wms;

public partial class WmsClient : Page
{
    private Coordinate Center;
    private double Zoom;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack)
        {
            //Page is post back. Restore center and zoom-values from viewstate
            Center = (Coordinate) ViewState["mapCenter"];
            Zoom = (double) ViewState["mapZoom"];
        }
        else
        {
            Center = new Coordinate(0, 0);
            Zoom = 360;
            //Create the map
            GenerateMap();
        }
        PrintWmsInfo();
    }

    private void PrintWmsInfo()
    {
        WmsLayer layWms = MapHelper.GetWmsLayer();
        //Get request url for WMS
        hlWmsImage.NavigateUrl = layWms.GetRequestUrl(
            new Envelope(Center.X - Zoom*0.5, Center.X + Zoom*0.5, 
                         Center.Y - Zoom*0.25, Center.Y + Zoom*0.25),
            new Size((int) imgMap.Width.Value, (int) imgMap.Height.Value));

        litLayers.Text = "<p><b>WMS Title</b>: " + layWms.ServiceDescription.Title + "<br/>Abstract: <i>" +
                         layWms.ServiceDescription.Abstract + "</i>";
        litLayers.Text += "<br/><b>WMS Layers:</b><br/>";

        foreach (Client.WmsServerLayer layer in layWms.RootLayer.ChildLayers)
            PrintLayers(layer, layWms);
        litLayers.Text += "</ul></p>";
    }

    /// <summary>
    /// Recursive function for retriving layer names
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="layWms"></param>
    private void PrintLayers(Client.WmsServerLayer layer, WmsLayer layWms)
    {
        litLayers.Text += "<li>" + layer.Name;
        if (layWms.LayerList.Contains(layer.Name))
            litLayers.Text += " (Enabled)";
        litLayers.Text += "</li>";

        if (layer.ChildLayers != null && layer.ChildLayers.Length > 0)
        {
            litLayers.Text += "<ul>";
            foreach (Client.WmsServerLayer childlayer in layer.ChildLayers)
                PrintLayers(childlayer, layWms);
            litLayers.Text += "</ul>";
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
        string ResponseFormat = "maphandler.ashx?MAP=WmsClient&Width=[WIDTH]&Height=[HEIGHT]&Zoom=[ZOOM]&X=[X]&Y=[Y]";
        NumberFormatInfo numberFormat_EnUS = new CultureInfo("en-US", false).NumberFormat;
        imgMap.ImageUrl = ResponseFormat.Replace("[WIDTH]", imgMap.Width.Value.ToString()).
            Replace("[HEIGHT]", imgMap.Height.Value.ToString()).
            Replace("[ZOOM]", Zoom.ToString(numberFormat_EnUS)).
            Replace("[X]", Center.X.ToString(numberFormat_EnUS)).
            Replace("[Y]", Center.Y.ToString(numberFormat_EnUS));
        hlCurrentImage.NavigateUrl = imgMap.ImageUrl;
    }
}