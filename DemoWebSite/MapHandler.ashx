<%@ WebHandler Language="C#" Class="MapHandler" %>

using System;
using System.Web;

/// <summary>
/// The maphandler class takes a set of GET or POST parameters and returns a map as PNG (this reminds in many ways of the way a WMS server work).
/// Required parameters are: WIDTH, HEIGHT, ZOOM, X, Y, MAP
/// </summary>
public class MapHandler : IHttpHandler
{

	internal static System.Globalization.NumberFormatInfo numberFormat_EnUS = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

    public void ProcessRequest (HttpContext context) {
		int Width = 0;
		int Height = 0;
		double centerX = 0;
		double centerY = 0;
		double Zoom = 0;

		//Parse request parameters
		if (!int.TryParse(context.Request.Params["WIDTH"], out Width))
			throw (new ArgumentException("Invalid parameter"));
		if (!int.TryParse(context.Request.Params["HEIGHT"], out Height))
			throw (new ArgumentException("Invalid parameter"));
		if (!double.TryParse(context.Request.Params["ZOOM"], System.Globalization.NumberStyles.Float, numberFormat_EnUS, out Zoom))
			throw (new ArgumentException("Invalid parameter"));
		if (!double.TryParse(context.Request.Params["X"], System.Globalization.NumberStyles.Float, numberFormat_EnUS, out centerX))
			throw (new ArgumentException("Invalid parameter"));
		if (!double.TryParse(context.Request.Params["Y"], System.Globalization.NumberStyles.Float, numberFormat_EnUS, out centerY))
			throw (new ArgumentException("Invalid parameter"));
		if (context.Request.Params["MAP"] == null)
			throw (new ArgumentException("Invalid parameter"));
		//Params OK

		SharpMap.Map map = InitializeMap(context.Request.Params["MAP"], new System.Drawing.Size(Width, Height));
		if (map == null)
			throw (new ArgumentException("Invalid map"));

		//Set visible map extents
		map.Center = new SharpMap.Geometries.Point(centerX, centerY);
		map.Zoom = Zoom;
		//Generate map
		System.Drawing.Bitmap img = (System.Drawing.Bitmap)map.GetMap();

		//Stream the image to the client
		context.Response.ContentType = "image/png";
		System.IO.MemoryStream MS = new System.IO.MemoryStream();
		img.Save(MS, System.Drawing.Imaging.ImageFormat.Png);
		// tidy up  
		img.Dispose();
		byte[] buffer = MS.ToArray();
		context.Response.OutputStream.Write(buffer, 0, buffer.Length);
    }

	private SharpMap.Map InitializeMap(string MapID, System.Drawing.Size size)
	{
		//Set up the map. We use the method in the App_Code folder for initializing the map
		switch (MapID)
		{
			//Our simple world map was requested 
			case "SimpleWorld":
				return MapHelper.InitializeMap(size);
			//Gradient theme layer requested. Based on simplemap
			case "Gradient":
				return MapHelper.InitializeGradientMap(size);
			case "WmsClient":
				return MapHelper.InitializeWmsMap(size);
			default:
				throw new ArgumentException("Invalid map '" + MapID + "' requested"); ;
		}
	}
	
    public bool IsReusable {
        get {
            return false;
        }
    }

}