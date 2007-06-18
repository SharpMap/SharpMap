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

public partial class Ajax : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		ajaxMap.Map = MapHelper.InitializeMap(new System.Drawing.Size(10, 10));
		if (!Page.IsPostBack && !Page.IsCallback)
		{
			//Set up the map. We use the method in the App_Code folder for initializing the map
			ajaxMap.Map.Center = new SharpMap.Geometries.Point(0, 20);
			ajaxMap.FadeSpeed = 10;
			ajaxMap.ZoomSpeed = 10;
			ajaxMap.Map.Zoom = 360;

		}
		ajaxMap.ResponseFormat = "maphandler.ashx?MAP=SimpleWorld&Width=[WIDTH]&Height=[HEIGHT]&Zoom=[ZOOM]&X=[X]&Y=[Y]";
	}
}
