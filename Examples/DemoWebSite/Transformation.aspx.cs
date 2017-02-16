using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Web.UI;
using SharpMap;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using GeoAPI.Geometries;
using SharpMap.Layers;
using SharpMap.Web;
using Point=GeoAPI.Geometries.Coordinate;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

public partial class Transformation : Page
{
    private ICoordinateSystem datacoordsys;
    private Map myMap;

    protected void Page_Load(object sender, EventArgs e)
    {
        //Set up the map. We use the method in the App_Code folder for initializing the map
        myMap = InitializeMap(new Size((int) imgMap.Width.Value, (int) imgMap.Height.Value));
        if (Page.IsPostBack)
        {
            //Page is post back. Restore center and zoom-values from viewstate
            myMap.Center = (Point) ViewState["mapCenter"];
            myMap.Zoom = (double) ViewState["mapZoom"];
        }
        else
        {
            //This is the initial view of the map. Zoom to the extents of the map:
            myMap.Zoom = 80;
            myMap.Center = new Point(-95, 37);
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
        ViewState.Add("currentProj", ddlProjection.SelectedValue);
        //Render the map
        Image img = myMap.GetMap();
        string imgID = Caching.InsertIntoCache(1, img);
        imgMap.ImageUrl = "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
        litEnvelope.Text = myMap.Envelope.MinX.ToString("#.##") + "," + myMap.Envelope.MinY.ToString("#.##") + " -> " +
                           myMap.Envelope.MaxX.ToString("#.##") + "," + myMap.Envelope.MaxY.ToString("#.##") +
                           " (Projected coordinate system)";
    }

    protected void ddlProjection_SelectedIndexChanged(object sender, EventArgs e)
    {
        //Transform current view to new coordinate system and zoom to the transformed box
        string PreviousProj = ViewState["currentProj"].ToString();
        string SelectedProj = ddlProjection.SelectedValue;

        //Points defining the current view 
        Point left = new Point(myMap.Envelope.MinX, myMap.Center.Y);
        Point right = new Point(myMap.Envelope.MaxX, myMap.Center.Y);
        Point center = myMap.Center;

        if (PreviousProj != "Pseudo")
        {
            //Transform current view back to geographic coordinates
            ICoordinateTransformation trans = GetTransform(PreviousProj);
            left = GeometryTransform.TransformCoordinate(new Point(myMap.Envelope.MinX, myMap.Center.Y),
                                                    trans.MathTransform.Inverse());
            right = GeometryTransform.TransformCoordinate(new Point(myMap.Envelope.MaxX, myMap.Center.Y),
                                                     trans.MathTransform.Inverse());
            center = GeometryTransform.TransformCoordinate(myMap.Center, trans.MathTransform.Inverse());
        }
        //If both PreviousSRID and SelectedSRID are projected coordsys, first transform to geographic

        if (SelectedProj == "Pseudo")
        {
            myMap.Center = center;
            myMap.Zoom = Math.Abs(right.X - left.X);
        }
        else //Project coordinates to new projection
        {
            //Transform back to geographic and over to new projection
            ICoordinateTransformation trans = GetTransform(SelectedProj);
            left = GeometryTransform.TransformCoordinate(left, trans.MathTransform);
            right = GeometryTransform.TransformCoordinate(right, trans.MathTransform);
            center = GeometryTransform.TransformCoordinate(center, trans.MathTransform);
            myMap.Center = center;
            myMap.Zoom = Math.Abs(right.X - left.X);
            var envelopeGcs = GeometryTransform.TransformBox(myMap.Envelope, trans.MathTransform.Inverse());
            litEnvelopeLatLong.Text = envelopeGcs.ToString();
        }
        GenerateMap();
    }

    public Map InitializeMap(Size size)
    {
        HttpContext.Current.Trace.Write("Initializing map...");

        //Initialize a new map of size 'imagesize'
        Map map = new Map(size);

        //Set up the countries layer
        VectorLayer layCountries = new VectorLayer("Countries");
        //Set the datasource to a shapefile in the App_data folder
        ShapeFile datasource = new ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\USA\states.shp"), true);
        layCountries.DataSource = datasource;
        datacoordsys = datasource.CoordinateSystem;

        //Set fill-style to green
        layCountries.Style.Fill = new SolidBrush(Color.Green);
        //Set the polygons to have a black outline
        layCountries.Style.Outline = Pens.Black;
        layCountries.Style.EnableOutline = true;
        layCountries.CoordinateTransformation = GetTransform(ddlProjection.SelectedValue);
        if (layCountries.CoordinateTransformation != null)
        {
            litInputCoordsys.Text = layCountries.CoordinateTransformation.TargetCS.WKT;
            litCoordsys.Text = layCountries.CoordinateTransformation.SourceCS.WKT;
            litTransform.Text = layCountries.CoordinateTransformation.MathTransform.WKT;
        }
        else
        {
            
            litInputCoordsys.Text = datasource.CoordinateSystem.WKT;
            litCoordsys.Text = "None";
            litTransform.Text = "None";
        }
        VectorLayer layGrid = new VectorLayer("Grid");
        //Set the datasource to a shapefile in the App_data folder
        layGrid.DataSource = new ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\USA\latlong.shp"), true);
        layGrid.CoordinateTransformation = layCountries.CoordinateTransformation;
        layGrid.Style.Line = new Pen(Color.FromArgb(127, 255, 0, 0), 1);

        //Add the layers to the map object.
        map.Layers.Add(layCountries);
        map.Layers.Add(layGrid);

        map.BackColor = Color.LightBlue;

        HttpContext.Current.Trace.Write("Map initialized");
        return map;
    }

    public ICoordinateTransformation GetTransform(string name)
    {
        switch (name)
        {
            case "Mercator":
                return Transform2Mercator(datacoordsys);
            case "Albers":
                return Transform2Albers(datacoordsys);
            case "Lambert":
                return Transform2Lambert(datacoordsys);
            default:
                return null;
        }
    }

    public static ICoordinateTransformation Transform2Albers(ICoordinateSystem source)
    {
        if (source == null)
            throw new ArgumentException("Source coordinate system is null");
        if (!(source is IGeographicCoordinateSystem))
            throw new ArgumentException("Source coordinate system must be geographic");

        CoordinateSystemFactory cFac = new CoordinateSystemFactory();

        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
        parameters.Add(new ProjectionParameter("central_meridian", -95));
        parameters.Add(new ProjectionParameter("latitude_of_origin", 50));
        parameters.Add(new ProjectionParameter("standard_parallel_1", 29.5));
        parameters.Add(new ProjectionParameter("standard_parallel_2", 45.5));
        parameters.Add(new ProjectionParameter("false_easting", 0));
        parameters.Add(new ProjectionParameter("false_northing", 0));
        IProjection projection = cFac.CreateProjection("Albers_Conic_Equal_Area", "albers", parameters);

        IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Albers_Conic_Equal_Area",
                                                                                   source as IGeographicCoordinateSystem,
                                                                                   projection, ProjNet.CoordinateSystems.LinearUnit.Metre,
                                                                                   new AxisInfo("East",
                                                                                                AxisOrientationEnum.East),
                                                                                   new AxisInfo("North",
                                                                                                AxisOrientationEnum.
                                                                                                    North));

        return new CoordinateTransformationFactory().CreateFromCoordinateSystems(source, coordsys);
    }

    public static ICoordinateTransformation Transform2Mercator(ICoordinateSystem source)
    {
        CoordinateSystemFactory cFac = new CoordinateSystemFactory();

        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
        parameters.Add(new ProjectionParameter("latitude_of_origin", 0));
        parameters.Add(new ProjectionParameter("central_meridian", 0));
        parameters.Add(new ProjectionParameter("false_easting", 0));
        parameters.Add(new ProjectionParameter("false_northing", 0));
        IProjection projection = cFac.CreateProjection("Mercator", "Mercator_2SP", parameters);

        IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Mercator",
                                                                                   source as IGeographicCoordinateSystem,
                                                                                   projection, ProjNet.CoordinateSystems.LinearUnit.Metre,
                                                                                   new AxisInfo("East",
                                                                                                AxisOrientationEnum.East),
                                                                                   new AxisInfo("North",
                                                                                                AxisOrientationEnum.
                                                                                                    North));

        return new CoordinateTransformationFactory().CreateFromCoordinateSystems(source, coordsys);
    }


    public static ICoordinateTransformation Transform2Lambert(ICoordinateSystem source)
    {
        if (source == null)
            throw new ArgumentException("Source coordinate system is null");
        if (!(source is IGeographicCoordinateSystem))
            throw new ArgumentException("Source coordinate system must be geographic");

        CoordinateSystemFactory cFac = new CoordinateSystemFactory();

        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
        parameters.Add(new ProjectionParameter("latitude_of_origin", 50));
        parameters.Add(new ProjectionParameter("central_meridian", -95));
        parameters.Add(new ProjectionParameter("standard_parallel_1", 33));
        parameters.Add(new ProjectionParameter("standard_parallel_2", 45));
        parameters.Add(new ProjectionParameter("false_easting", 0));
        parameters.Add(new ProjectionParameter("false_northing", 0));
        IProjection projection = cFac.CreateProjection("Lambert Conformal Conic 2SP", "lambert_conformal_conic_2sp",
                                                       parameters);

        IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Lambert Conformal Conic 2SP",
                                                                                   source as IGeographicCoordinateSystem,
                                                                                   projection, ProjNet.CoordinateSystems.LinearUnit.Metre,
                                                                                   new AxisInfo("East",
                                                                                                AxisOrientationEnum.East),
                                                                                   new AxisInfo("North",
                                                                                                AxisOrientationEnum.
                                                                                                    North));

        return new CoordinateTransformationFactory().CreateFromCoordinateSystems(source, coordsys);
    }
}