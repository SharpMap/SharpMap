using System;
using System.Data;
using System.Drawing;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations; 

public partial class Transformation : System.Web.UI.Page
{
	private SharpMap.Map myMap;
	ICoordinateSystem datacoordsys;

	protected void Page_Load(object sender, EventArgs e)
	{
		//Set up the map. We use the method in the App_Code folder for initializing the map
		myMap = InitializeMap(new System.Drawing.Size((int)imgMap.Width.Value, (int)imgMap.Height.Value));
		if (Page.IsPostBack)
		{
			//Page is post back. Restore center and zoom-values from viewstate
			myMap.Center = (SharpMap.Geometries.Point)ViewState["mapCenter"];
			myMap.Zoom = (double)ViewState["mapZoom"];
		}
		else
		{
			//This is the initial view of the map. Zoom to the extents of the map:
			myMap.Zoom = 80;
			myMap.Center = new SharpMap.Geometries.Point(-95, 37);
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
		ViewState.Add("currentProj", ddlProjection.SelectedValue);
		//Render the map
		System.Drawing.Image img = myMap.GetMap();
		string imgID = SharpMap.Web.Caching.InsertIntoCache(1, img);
		imgMap.ImageUrl = "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
		litEnvelope.Text = myMap.Envelope.Left.ToString("#.##") + "," + myMap.Envelope.Bottom.ToString("#.##") + " -> " +
			myMap.Envelope.Right.ToString("#.##") + "," + myMap.Envelope.Top.ToString("#.##") + " (Projected coordinate system)";
	}

	protected void ddlProjection_SelectedIndexChanged(object sender, EventArgs e)
	{
		//Transform current view to new coordinate system and zoom to the transformed box
		string PreviousProj = ViewState["currentProj"].ToString();
		string SelectedProj = ddlProjection.SelectedValue;

		//Points defining the current view 
		SharpMap.Geometries.Point left = new SharpMap.Geometries.Point(myMap.Envelope.Left, myMap.Center.Y);
		SharpMap.Geometries.Point right = new SharpMap.Geometries.Point(myMap.Envelope.Right, myMap.Center.Y);
		SharpMap.Geometries.Point center = myMap.Center;

		if (PreviousProj != "Pseudo")
		{
			//Transform current view back to geographic coordinates
			ICoordinateTransformation trans = GetTransform(PreviousProj);
			left = GeometryTransform.TransformPoint(new SharpMap.Geometries.Point(myMap.Envelope.Left, myMap.Center.Y), trans.MathTransform.Inverse());
			right = GeometryTransform.TransformPoint(new SharpMap.Geometries.Point(myMap.Envelope.Right, myMap.Center.Y), trans.MathTransform.Inverse());
			center = GeometryTransform.TransformPoint(myMap.Center, trans.MathTransform.Inverse());
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
			left = GeometryTransform.TransformPoint(left, trans.MathTransform);
			right = GeometryTransform.TransformPoint(right, trans.MathTransform);
			center = GeometryTransform.TransformPoint(center, trans.MathTransform);
			myMap.Center = center;
			myMap.Zoom = Math.Abs(right.X - left.X);
			SharpMap.Geometries.BoundingBox envelopeGcs =GeometryTransform.TransformBox(myMap.Envelope, trans.MathTransform.Inverse());
			litEnvelopeLatLong.Text = envelopeGcs.ToString();
		}
		GenerateMap();
	}

	public SharpMap.Map InitializeMap(System.Drawing.Size size)
	{
		HttpContext.Current.Trace.Write("Initializing map...");

		//Initialize a new map of size 'imagesize'
		SharpMap.Map map = new SharpMap.Map(size);

		//Set up the countries layer
		SharpMap.Layers.VectorLayer layCountries = new SharpMap.Layers.VectorLayer("Countries");
		//Set the datasource to a shapefile in the App_data folder
		SharpMap.Data.Providers.ShapeFile datasource = new SharpMap.Data.Providers.ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\USA\states.shp"), true);
		layCountries.DataSource = datasource;
		datacoordsys = datasource.CoordinateSystem;

		//Set fill-style to green
		layCountries.Style.Fill = new SolidBrush(Color.Green);
		//Set the polygons to have a black outline
		layCountries.Style.Outline = System.Drawing.Pens.Black;
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
		SharpMap.Layers.VectorLayer layGrid = new SharpMap.Layers.VectorLayer("Grid");
		//Set the datasource to a shapefile in the App_data folder
		layGrid.DataSource = new SharpMap.Data.Providers.ShapeFile(HttpContext.Current.Server.MapPath(@"~\App_data\USA\latlong.shp"), true);
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
			case "Mercator": return Transform2Mercator(datacoordsys);
			case "Albers": return Transform2Albers(datacoordsys);
			case "Lambert": return Transform2Lambert(datacoordsys);
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

		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
		parameters.Add(new ProjectionParameter("central_meridian", -95));
		parameters.Add(new ProjectionParameter("latitude_of_origin", 50));
		parameters.Add(new ProjectionParameter("standard_parallel_1", 29.5));
		parameters.Add(new ProjectionParameter("standard_parallel_2", 45.5));
		parameters.Add(new ProjectionParameter("false_easting", 0));
		parameters.Add(new ProjectionParameter("false_northing", 0));
		IProjection projection = cFac.CreateProjection("Albers_Conic_Equal_Area", "albers", parameters);

		IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Albers_Conic_Equal_Area", source as IGeographicCoordinateSystem, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

		return new CoordinateTransformationFactory().CreateFromCoordinateSystems(source, coordsys);
	}
	public static ICoordinateTransformation Transform2Mercator(ICoordinateSystem source)
	{
		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
		parameters.Add(new ProjectionParameter("latitude_of_origin", 0));
		parameters.Add(new ProjectionParameter("central_meridian", 0));
		parameters.Add(new ProjectionParameter("false_easting", 0));
		parameters.Add(new ProjectionParameter("false_northing", 0));
		IProjection projection = cFac.CreateProjection("Mercator", "Mercator_2SP", parameters);

		IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Mercator", source as IGeographicCoordinateSystem, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

		return new CoordinateTransformationFactory().CreateFromCoordinateSystems(source, coordsys);
	}


	public static ICoordinateTransformation Transform2Lambert(ICoordinateSystem source)
	{
		if (source == null)
			throw new ArgumentException("Source coordinate system is null");
		if (!(source is IGeographicCoordinateSystem))
			throw new ArgumentException("Source coordinate system must be geographic");

		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
		parameters.Add(new ProjectionParameter("latitude_of_origin", 50));
		parameters.Add(new ProjectionParameter("central_meridian", -95));
		parameters.Add(new ProjectionParameter("standard_parallel_1", 33));
		parameters.Add(new ProjectionParameter("standard_parallel_2", 45));
		parameters.Add(new ProjectionParameter("false_easting", 0));
		parameters.Add(new ProjectionParameter("false_northing", 0));
		IProjection projection = cFac.CreateProjection("Lambert Conformal Conic 2SP", "lambert_conformal_conic_2sp", parameters);

		IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Lambert Conformal Conic 2SP", source as IGeographicCoordinateSystem, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

		return new CoordinateTransformationFactory().CreateFromCoordinateSystems(source, coordsys);
	}
}
