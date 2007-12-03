using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using SharpMap.Geometries;

public partial class GeometryFeature : System.Web.UI.Page
{
    public SharpMap.Layers.VectorLayer CreateGeometryLayer()
    {
        SharpMap.Data.FeatureDataTable fdt = new SharpMap.Data.FeatureDataTable();
        fdt.Columns.Add(new DataColumn("Name", typeof(String)));

        SharpMap.Data.FeatureDataRow fdr;

        fdr = fdt.NewRow();

        fdr["Name"] = "Mayence";
        fdr.Geometry = (Geometry)new Point(8.1, 50.0);

        fdt.AddRow(fdr);


        SharpMap.Layers.VectorLayer vLayer = new SharpMap.Layers.VectorLayer("GeometryProvider");
        vLayer.DataSource = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);
        vLayer.SRID = 4326;

        return vLayer;
    }



    private SharpMap.Map myMap;

    protected void Page_Load(object sender, EventArgs e)
    {
        //Set up the map. We use the method in the App_Code folder for initializing the map
        myMap = MapHelper.InitializeMap(new System.Drawing.Size((int)imgMap.Width.Value, (int)imgMap.Height.Value));

        SharpMap.Layers.VectorLayer GeomLayer = this.CreateGeometryLayer();

        SharpMap.Layers.LabelLayer layGeomProviderLabel = new SharpMap.Layers.LabelLayer("LabelOfTheCityMayence");
        layGeomProviderLabel.DataSource = GeomLayer.DataSource;
        layGeomProviderLabel.Enabled = true;
        layGeomProviderLabel.LabelColumn = "Name";
        layGeomProviderLabel.Style = new SharpMap.Styles.LabelStyle();
        layGeomProviderLabel.Style.ForeColor = System.Drawing.Color.AliceBlue;
        layGeomProviderLabel.Style.Font = new System.Drawing.Font("ArialB", 14);
        layGeomProviderLabel.Style.Offset = new System.Drawing.PointF(4, 4);
        layGeomProviderLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left;
        layGeomProviderLabel.Style.VerticalAlignment = SharpMap.Styles.LabelStyle.VerticalAlignmentEnum.Bottom;
        layGeomProviderLabel.SRID = 4326;

        myMap.Layers.Add(GeomLayer);
        myMap.Layers.Add(layGeomProviderLabel);

        myMap.Center = new SharpMap.Geometries.Point(8, 50);
        myMap.Zoom = 10;


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
            //or center on 0,0 and zoom to full earth (360 degrees)
            //myMap.Center = new SharpMap.Geometries.Point(0,0);
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
        //Render map
        System.Drawing.Image img = myMap.GetMap();
        string imgID = SharpMap.Web.Caching.InsertIntoCache(1, img);
        imgMap.ImageUrl = "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
    }
}
