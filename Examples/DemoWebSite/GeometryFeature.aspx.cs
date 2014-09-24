using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using SharpMap;
using SharpMap.Data;
using SharpMap.Data.Providers;
using GeoAPI.Geometries;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.Web;
using Point=GeoAPI.Geometries.Coordinate;

public partial class GeometryFeature : Page
{
    private Map myMap;

    public VectorLayer CreateGeometryLayer()
    {
        var gf = new NetTopologySuite.Geometries.GeometryFactory();
        var fdt = new FeatureDataTable();
        fdt.Columns.Add(new DataColumn("Name", typeof (String)));

        fdt.BeginLoadData();
        var fdr = (FeatureDataRow)fdt.LoadDataRow(new[] {(object) "Mayence"}, true);
        fdr.Geometry = gf.CreatePoint(new Point(8.1, 50.0));
        fdt.EndLoadData();

        var vLayer = new VectorLayer("GeometryProvider");
        vLayer.DataSource = new GeometryFeatureProvider(fdt);
        vLayer.SRID = 4326;

        return vLayer;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //Set up the map. We use the method in the App_Code folder for initializing the map
        myMap = MapHelper.InitializeMap(new Size((int) imgMap.Width.Value, (int) imgMap.Height.Value));

        VectorLayer GeomLayer = CreateGeometryLayer();

        LabelLayer layGeomProviderLabel = new LabelLayer("LabelOfTheCityMayence");
        layGeomProviderLabel.DataSource = GeomLayer.DataSource;
        layGeomProviderLabel.Enabled = true;
        layGeomProviderLabel.LabelColumn = "Name";
        layGeomProviderLabel.Style = new LabelStyle();
        layGeomProviderLabel.Style.ForeColor = Color.AliceBlue;
        layGeomProviderLabel.Style.Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
        layGeomProviderLabel.Style.Offset = new PointF(4, 4);
        layGeomProviderLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
        layGeomProviderLabel.Style.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
        layGeomProviderLabel.SRID = 4326;

        myMap.Layers.Add(GeomLayer);
        myMap.Layers.Add(layGeomProviderLabel);

        myMap.Center = new Point(8, 50);
        myMap.Zoom = 10;


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
        imgMap.ImageUrl = "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
    }
}