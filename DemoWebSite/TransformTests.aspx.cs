using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using SharpMap.Geometries;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;

public partial class TransformTests : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		TestMercator_1SP();
		TestMercator_2SP();
		TestTransverseMercator();
		TestLambertConicConformal_2SP();
		TestAlbers();
		TestGeocentric();
	}

	private void TestMercator_1SP()
	{
		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

		IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Bessel 1840", 6377397.155, 299.15281, LinearUnit.Metre);

		IHorizontalDatum datum = cFac.CreateHorizontalDatum("Bessel 1840", DatumType.HD_Geocentric, ellipsoid, null);
		IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Bessel 1840", AngularUnit.Degrees, datum,
			PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
			new AxisInfo("Lat", AxisOrientationEnum.North));
        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
		parameters.Add(new ProjectionParameter("latitude_of_origin", 0));
		parameters.Add(new ProjectionParameter("central_meridian", 110));
		parameters.Add(new ProjectionParameter("scale_factor", 0.997));
		parameters.Add(new ProjectionParameter("false_easting", 3900000));
		parameters.Add(new ProjectionParameter("false_northing", 900000));
		IProjection projection = cFac.CreateProjection("Mercator_1SP", "Mercator_1SP", parameters);

		IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Makassar / NEIEZ", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

		ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

		SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(120, -3);
		SharpMap.Geometries.Point pUtm = new Point(trans.MathTransform.Transform(pGeo.ToDoubleArray()));
		SharpMap.Geometries.Point pGeo2 = new Point(trans.MathTransform.Inverse().Transform(pUtm.ToDoubleArray()));

		result.Text += PrintResultTable(gcs, coordsys, pGeo, pUtm, new Point(5009726.58, 569150.82), pGeo2, "Mercator_1SP test");
	}

	private void TestMercator_2SP()
	{
		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

		IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Krassowski 1940", 6378245.0, 298.3, LinearUnit.Metre);

		IHorizontalDatum datum = cFac.CreateHorizontalDatum("Krassowski 1940", DatumType.HD_Geocentric, ellipsoid, null);
		IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Krassowski 1940", AngularUnit.Degrees, datum, 
			PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
			new AxisInfo("Lat", AxisOrientationEnum.North));
        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
		parameters.Add(new ProjectionParameter("latitude_of_origin", 42));
		parameters.Add(new ProjectionParameter("central_meridian", 51));
		parameters.Add(new ProjectionParameter("false_easting", 0));
		parameters.Add(new ProjectionParameter("false_northing", 0));
		IProjection projection = cFac.CreateProjection("Mercator_2SP", "Mercator_2SP", parameters);

		IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Pulkovo 1942 / Mercator Caspian Sea", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

		ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

		SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(53,53);
		SharpMap.Geometries.Point pUtm = new Point(trans.MathTransform.Transform(pGeo.ToDoubleArray()));
        SharpMap.Geometries.Point pGeo2 = new Point(trans.MathTransform.Inverse().Transform(pUtm.ToDoubleArray()));

		result.Text += PrintResultTable(gcs, coordsys, pGeo, pUtm, new Point(165704.29, 5171848.07), pGeo2, "Mercator_2SP test");
	}

	private void TestTransverseMercator()
	{
		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

		IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Airy 1830", 6377563.396, 299.32496, LinearUnit.Metre);

		IHorizontalDatum datum = cFac.CreateHorizontalDatum("Airy 1830", DatumType.HD_Geocentric, ellipsoid, null);
		IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Airy 1830", AngularUnit.Degrees, datum,
			PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
			new AxisInfo("Lat", AxisOrientationEnum.North));
        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
		parameters.Add(new ProjectionParameter("latitude_of_origin", 49));
		parameters.Add(new ProjectionParameter("central_meridian", -2));
		parameters.Add(new ProjectionParameter("scale_factor", 0.9996012717));
		parameters.Add(new ProjectionParameter("false_easting", 400000));
		parameters.Add(new ProjectionParameter("false_northing", -100000));
		IProjection projection = cFac.CreateProjection("Transverse Mercator", "Transverse_Mercator", parameters);

		IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("OSGB 1936 / British National Grid", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

		ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

		SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(0.5, 50.5);
		SharpMap.Geometries.Point pUtm = new Point(trans.MathTransform.Transform(pGeo.ToDoubleArray()));
		SharpMap.Geometries.Point pGeo2 = new Point(trans.MathTransform.Inverse().Transform(pUtm.ToDoubleArray()));

		result.Text += PrintResultTable(gcs, coordsys, pGeo, pUtm, new Point(577274.99, 69740.50), pGeo2, "Transverse Mercator test");
	}
	private void TestLambertConicConformal_2SP()
	{
		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

		IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Clarke 1866", 20925832.16, 294.97470, LinearUnit.USSurveyFoot);

		IHorizontalDatum datum = cFac.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
		IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
			PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
			new AxisInfo("Lat", AxisOrientationEnum.North));
        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
		parameters.Add(new ProjectionParameter("latitude_of_origin", 27.833333333));
		parameters.Add(new ProjectionParameter("central_meridian", -99));
		parameters.Add(new ProjectionParameter("standard_parallel_1", 28.3833333333));
		parameters.Add(new ProjectionParameter("standard_parallel_2", 30.2833333333));
		parameters.Add(new ProjectionParameter("false_easting", 2000000));
		parameters.Add(new ProjectionParameter("false_northing", 0));
		IProjection projection = cFac.CreateProjection("Lambert Conic Conformal (2SP)", "lambert_conformal_conic_2sp", parameters);

		IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("NAD27 / Texas South Central", gcs, projection, LinearUnit.USSurveyFoot, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

		ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

		SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(-96, 28.5);
		SharpMap.Geometries.Point pUtm = new Point(trans.MathTransform.Transform(pGeo.ToDoubleArray()));
        SharpMap.Geometries.Point pGeo2 = new Point(trans.MathTransform.Inverse().Transform(pUtm.ToDoubleArray()));

		result.Text += PrintResultTable(gcs, coordsys, pGeo, pUtm, new Point(2963503.91, 254759.80), pGeo2, "Lambert Conic Conformal 2SP test");
	}

	private void TestAlbers()
	{
		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

		IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Clarke 1866", 6378206.4, 294.9786982138982,LinearUnit.USSurveyFoot);

		IHorizontalDatum datum = cFac.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
		IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
			PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
			new AxisInfo("Lat", AxisOrientationEnum.North));
        List<ProjectionParameter> parameters = new List<ProjectionParameter>();
		parameters.Add(new ProjectionParameter("central_meridian", -96));
		parameters.Add(new ProjectionParameter("latitude_of_origin", 23));
		parameters.Add(new ProjectionParameter("standard_parallel_1", 29.5));
		parameters.Add(new ProjectionParameter("standard_parallel_2", 45.5));
		parameters.Add(new ProjectionParameter("false_easting", 0));
		parameters.Add(new ProjectionParameter("false_northing", 0));
		IProjection projection = cFac.CreateProjection("Albers Conical Equal Area", "albers", parameters);

		IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Albers Conical Equal Area", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

		ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

		SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(-75,35);
		SharpMap.Geometries.Point pUtm = new Point(trans.MathTransform.Transform(pGeo.ToDoubleArray()));
		SharpMap.Geometries.Point pGeo2 = new Point(trans.MathTransform.Inverse().Transform(pUtm.ToDoubleArray()));

		result.Text += PrintResultTable(gcs, coordsys, pGeo, pUtm, new Point(1885472.7,1535925), pGeo2, "Albers Conical Equal Area test");
	}

	private void TestGeocentric()
	{
		CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

		IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("WGS84", AngularUnit.Degrees, HorizontalDatum.WGS84,
			PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),	new AxisInfo("Lat", AxisOrientationEnum.North));
		IGeocentricCoordinateSystem geoccs = cFac.CreateGeocentricCoordinateSystem("WGS84 geocentric", gcs.HorizontalDatum, LinearUnit.Metre, PrimeMeridian.Greenwich);

		ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, geoccs);

		SharpMap.Geometries.Point3D pGeo = new SharpMap.Geometries.Point3D(2.12955, 53.80939444, 73);
        SharpMap.Geometries.Point3D pGc = new Point3D(trans.MathTransform.Transform(pGeo.ToDoubleArray()));
		SharpMap.Geometries.Point3D pGeo2 = new Point3D(trans.MathTransform.Inverse().Transform(pGc.ToDoubleArray()));

		result.Text += PrintResultTable(gcs, geoccs, pGeo, pGc, new Point3D(3771793.97, 140253.34, 5124304.35), pGeo2, "Geocentric test");
		
		return;
	}
	private string PrintResultTable(ICoordinateSystem fromCoordSys, ICoordinateSystem toCoordSys, Point fromPnt, Point toPnt, Point refPnt, Point backPnt, string header)
	{
		string table = "<table style=\"border: 1px solid #000; margin: 10px;\">";
		table += "<tr><td colspan=\"2\"><h3>" + header + "</h3></td></tr>";
		table += "<tr><td width=\"130px\" valign=\"top\">Input coordsys:</td><td>" + fromCoordSys.WKT + "</td></tr>";
		table += "<tr><td valign=\"top\">Output coordsys:</td><td>" + toCoordSys.WKT + "</td></tr>";
		table += "<tr><td>Input coordinate:</td><td>" + fromPnt.ToString() + "</td></tr>";
		table += "<tr><td>Ouput coordinate:</td><td>" + toPnt.ToString() + "</td></tr>";
		table += "<tr><td>Expected coordinate:</td><td>" + refPnt.ToString() + "</td></tr>";
		table += "<tr><td>Difference:</td><td>" + (refPnt-toPnt).ToString() + "</td></tr>";
		table += "<tr><td>Reverse transform:</td><td>" + backPnt.ToString() + "</td></tr>";
		table += "<tr><td>Difference:</td><td>" + (backPnt - fromPnt).ToString() + "</td></tr>";
		table += "</table>";
		return table;
	}
}
