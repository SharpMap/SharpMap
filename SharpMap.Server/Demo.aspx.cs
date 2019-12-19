using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace SharpMapServer
{
    public partial class Demo : System.Web.UI.Page
    {
        public string layerName;
        public string parameters;
        protected void Page_Load(object sender, EventArgs e)
        {
            
            layerName = Request.QueryString["layerName"];
            var lay  = WMSServer.m_Map.GetLayerByName(layerName);
            var ext = lay.Envelope;
            parameters = new JavaScriptSerializer().Serialize(new
            {
                projection = "EPSG:4326",
                maxExtent = new double[] { ext.MinX, ext.MinY, ext.MaxX, ext.MaxY }
            });
        }
    }
}