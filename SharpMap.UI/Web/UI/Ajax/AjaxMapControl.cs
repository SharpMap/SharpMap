// Copyright 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Image=System.Web.UI.WebControls.Image;
using Point=SharpMap.Geometries.Point;

namespace SharpMap.Web.UI.Ajax
{
    /// <summary>
    /// The Ajax Map Control is a javascript controlled map that is able to refresh
    /// the map without the whole webpage has to do a roundtrip to the server.
    /// </summary>
    /// <remarks>
    /// <para>This webcontrol is tested with both IE and FireFox.</para>
    /// <para>The webcontrol creates a client-side javascript object named after
    /// the ClientID of this control
    /// and appends "Obj" to it. Below are a list of some of the properties
    /// and methods of the client-side object. The <see cref="OnViewChanging"/> 
    /// and <see cref="OnViewChange"/> client-side events
    /// are also is parsing a reference to this object.</para>
    /// <list type="table">
    /// <listheader><term>Method/Property</term><description>Description</description></listheader>
    /// <item><term>.minX</term><description>World coordinate of the left side of the current view</description></item>
    /// <item><term>.maxY</term><description>World coordinate of the top of the current view</description></item>
    /// <item><term>.GetCenter()</term><description>Gets a center point object with the current view (use the .x and .y properties of the returned object for the coordinates)</description></item>
    /// <item><term>.zoom</term><description>The current zoom level of the map (map width)</description></item>
    /// <item><term>.zoomAmount</term><description>The amount to zoom on a zoom-in event (negative values equals zoom out)</description></item>
    /// <item><term>.container</term><description>Reference to the map box element</description></item>
    /// <item><term>.statusbar</term><description>Reference to the statusbar element</description></item>
    /// </list>
    /// </remarks>
    [DefaultProperty("Map")]
    [ToolboxData("<{0}:AjaxMapControl runat=\"server\"></{0}:AjaxMapControl>")]
    [Designer(typeof (AjaxMapControlDesigner))]
    public class AjaxMapControl : WebControl, INamingContainer, ICallbackEventHandler
    {
        internal static NumberFormatInfo numberFormat_EnUS = new CultureInfo("en-US", false).NumberFormat;
        private bool _DisplayStatusBar;
        private int _FadeSpeed;
        private string _OnClickEvent;
        private string _OnViewChange;
        private string _OnViewChanging;
        private string _ResponseFormat = "myMapHandler.aspx?Width=[WIDTH]&Height=[HEIGHT]&Zoom=[ZOOM]&X=[X]&Y=[Y]";
        private string _StatusBarText = "[X], [Y] - Map width=[ZOOM]";
        private bool _UseCache;
        private int _ZoomSpeed;
        private string callbackArg = "";
        private HtmlGenericControl divTopBar;

        private Image imgMap1;
        private Image imgMap2;
        private Map map;
        private HtmlGenericControl spanCursorLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxMapControl"/>
        /// </summary>
        public AjaxMapControl()
        {
            ZoomSpeed = 15;
            FadeSpeed = 10;
            _DisplayStatusBar = true;
        }

        /// <summary>
        /// Sets the speed which the zoom is (lower = faster).
        /// The default value is 15
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(15)]
        [Description("Sets the speed which the zoom is (lower = faster).")]
        public int ZoomSpeed
        {
            get { return _ZoomSpeed; }
            set { _ZoomSpeed = value; }
        }

        /// <summary>
        /// Sets the speed of the fade (lower = faster).
        /// The default value is 10
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(10)]
        [Description("Sets the speed of the fade (lower = faster).")]
        public int FadeSpeed
        {
            get { return _FadeSpeed; }
            set { _FadeSpeed = value; }
        }

        /// <summary>
        /// Client-side method to call when map view have changed
        /// </summary>
        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("Client-side method to call when map view have changed")]
        public string OnViewChange
        {
            get { return _OnViewChange; }
            set { _OnViewChange = value; }
        }

        /// <summary>
        /// Client-side method to call when map are starting to update
        /// </summary>
        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("Client-side method to call when map are starting to update")]
        public string OnViewChanging
        {
            get { return _OnViewChanging; }
            set { _OnViewChanging = value; }
        }

        /// <summary>
        /// Gets or sets the clientside method to call when custom click-event is active.
        /// </summary>
        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Description("Clientside method to call when custom click-event is active")]
        public string OnClickEvent
        {
            get { return _OnClickEvent; }
            set { _OnClickEvent = value; }
        }

        /// <summary>
        /// Gets the name of the clientside ClickEvent property on the map object.
        /// </summary>
        public string ClickEventPropertyName
        {
            get { return ClientID + "Obj.clickEvent"; }
        }

        /// <summary>
        /// Gets the name of the clientside ToogleClickEvent method to enable or disable 
        /// the custom click-event on the map object.
        /// </summary>
        public string ToogleClickEventMethodName
        {
            get { return ClientID + "Obj.toogleClickEvent"; }
        }

        /// <summary>
        /// Gets the name of the clientside DisableClickEvent method to disable 
        /// the custom click-event on the map object.
        /// </summary>
        public string DisableClickEventMethodName
        {
            get { return ClientID + "Obj.disableClickEvent"; }
        }

        /// <summary>
        /// Gets the name of the clientside EnableClickEvent method to enable
        /// the custom click-event on the map object.
        /// </summary>
        public string EnableClickEventMethodName
        {
            get { return ClientID + "Obj.enableClickEvent"; }
        }

        /// <summary>
        /// Sets whether the control should use the http cache or call a specific maphandler
        /// </summary>
        [Bindable(false)]
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Sets whether the control should use the http cache or call a specific maphandler")]
        public bool UseCache
        {
            get { return _UseCache; }
            set { _UseCache = value; }
        }

        /// <summary>
        /// Text shown on the map status bar.
        /// </summary>
        /// <remarks>
        /// <para>Use [X] and [Y] to display cursor position in world coordinates and [ZOOM] for displaying the zoom value.</para>
        /// <para>The default value is "[X], [Y] - Map width=[ZOOM]"</para>
        /// </remarks>
        [Bindable(false)]
        [Category("Appearance")]
        [DefaultValue("[X], [Y] - Map width=[ZOOM]")]
        [Description("Text shown on the map status bar.")]
        public string StatusBarText
        {
            get { return _StatusBarText; }
            set { _StatusBarText = value; }
        }

        /// <summary>
        /// Formatting of the callback response used when <see cref="UseCache"/> is false.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use [X] and [Y] for center position, [ZOOM] for zoom value,
        /// [WIDTH] for image width and [WIDTH] for image height. These values will automatically
        /// be replaced by the current values. The return-result should correspond to the url of
        /// a maphandler that renders the map from these values
        /// </para>
        /// <para>myMapHandler.aspx?Width=[WIDTH]&amp;Height=[HEIGHT]&amp;Zoom=[ZOOM]&amp;X=[X]&amp;Y=[Y]</para>
        /// </remarks>
        [Bindable(false)]
        [Category("Data")]
        [DefaultValue("myMapHandler.aspx?Width=[WIDTH]&Height=[HEIGHT]&Zoom=[ZOOM]&X=[X]&Y=[Y]")]
        [Description("Formatting of the callback response used when UseCache property is false.")]
        public string ResponseFormat
        {
            get { return _ResponseFormat; }
            set { _ResponseFormat = value; }
        }

        /// <summary>
        /// Specifies whether the statusbar is visible or not.
        /// </summary>
        [Bindable(false)]
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Specifies whether the statusbar is visible or not.")]
        public bool DisplayStatusBar
        {
            get { return _DisplayStatusBar; }
            set { _DisplayStatusBar = value; }
        }

        /// <summary>
        /// The <see cref="SharpMap.Map"/> that is to be rendered in the control
        /// </summary>
        [Bindable(false)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        [Description("The map instance that is to be rendered in the control")]
        public Map Map
        {
            get { return map; }
            set { map = value; }
        }

        #region ICallbackEventHandler Members

        /// <summary>
        /// Returns the result of the callback event that targets <see cref="SharpMap.Web.UI.Ajax.AjaxMapControl"/>
        /// </summary>
        /// <returns></returns>
        public string GetCallbackResult()
        {
            EnsureChildControls();
            if (callbackArg.Trim() == "") return String.Empty;
            string[] vals = callbackArg.Split(new char[] {';'});
            try
            {
                map.Zoom = double.Parse(vals[2], numberFormat_EnUS);
                map.Center = new Point(double.Parse(vals[0], numberFormat_EnUS),
                                       double.Parse(vals[1], numberFormat_EnUS));
                map.Size = new Size(int.Parse(vals[3]), int.Parse(vals[4]));
                return GenerateMap();
                //If you want to use the Cache for storing the map, instead of a maphandler,
                //uncomment the following lines, and comment the above return statement
                /*System.Drawing.Image img = map.GetMap();
				string imgID = SharpMap.Web.Caching.InsertIntoCache(1, img);
				return "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);*/
            }
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Creates the arguments for the callback handler in the
        /// <see cref="System.Web.UI.ClientScriptManager.GetCallbackEventReference(System.Web.UI.Control,string,string,string)"/> method. 
        /// </summary>
        /// <param name="eventArgument"></param>
        public void RaiseCallbackEvent(string eventArgument)
        {
            callbackArg = eventArgument;
        }

        #endregion

        /// <summary>
        /// Sends server control content to a provided HtmlTextWriter object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter object that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use
        /// composition-based implementation to create any child controls they
        /// contain in preparation for posting back or rendering
        /// </summary>
        protected override void CreateChildControls()
        {
            if (!Page.IsCallback)
            {
                GenerateMapBox();
                GenerateClientScripts();
            }
            //base.CreateChildControls();
        }

        /// <summary>
        /// Returns a Url to the map
        /// </summary>
        private string GenerateMap()
        {
            if (_UseCache)
            {
                System.Drawing.Image img = Map.GetMap();
                string imgID = Caching.InsertIntoCache(1, img);
                return "getmap.aspx?ID=" + HttpUtility.UrlEncode(imgID);
            }
            else
                return _ResponseFormat.Replace("[WIDTH]", map.Size.Width.ToString()).
                    Replace("[HEIGHT]", map.Size.Height.ToString()).
                    Replace("[ZOOM]", map.Zoom.ToString(numberFormat_EnUS)).
                    Replace("[X]", map.Center.X.ToString(numberFormat_EnUS)).
                    Replace("[Y]", map.Center.Y.ToString(numberFormat_EnUS));
        }

        /// <summary>
        /// Registers the client-side scripts and creates an initialize script for the current map
        /// </summary>
        private void GenerateClientScripts()
        {
            string newline = Environment.NewLine;
            //Include scriptresource
            string scriptLocation = Page.ClientScript.GetWebResourceUrl(GetType(), "SharpMap.Web.UI.Ajax.AjaxMap.js");
            Page.ClientScript.RegisterClientScriptInclude("SharpMap.Web.UI.AjaxMap.js", scriptLocation);
            string obj = ClientID + "Obj";
            string setvarsScript = "SetVars_" + ClientID + "();" + newline +
                                   "function SetVars_" + ClientID + "() {" + newline +
                                   obj + " = SharpMap_Init('" + ClientID + "','"
                                   + imgMap1.ClientID + "','" + imgMap2.ClientID + "','" +
                                   (_DisplayStatusBar ? spanCursorLocation.ClientID : "") + "','" +
                                   (_DisplayStatusBar ? _StatusBarText : "") + "','" + UniqueID + "');" + newline;
            setvarsScript +=
                obj + ".zoom = " + map.Zoom.ToString(numberFormat_EnUS) + ";" + newline +
                obj + ".minX = " + map.Envelope.Left.ToString(numberFormat_EnUS) + ";" + newline +
                obj + ".maxY = " + map.Center.Y.ToString(numberFormat_EnUS) + "+" + obj + ".zoom/" + obj +
                ".container.offsetWidth*" + obj + ".container.offsetHeight*0.5;" + newline +
                obj + ".minZoom = " + map.MinimumZoom.ToString(numberFormat_EnUS) + ";" + newline +
                obj + ".maxZoom = " + map.MaximumZoom.ToString(numberFormat_EnUS) + ";" + newline +
                obj + ".zoomAmount = 3.0;" + newline +
                obj + ".zoomSpeed = " + _ZoomSpeed.ToString() + ";" + newline +
                obj + ".fadeSpeed = " + _FadeSpeed.ToString() + ";" + newline;

            if (_UseCache)
                setvarsScript += obj + ".map1.src = '" + GenerateMap() + "';\r\n";
            else
                setvarsScript += obj + ".map1.src = '" +
                                 _ResponseFormat.Replace("[WIDTH]", "'+" + obj + ".container.offsetWidth+'").
                                     Replace("[HEIGHT]", "'+" + obj + ".container.offsetHeight+'").
                                     Replace("[ZOOM]", "'+" + obj + ".zoom+'").
                                     Replace("[X]", map.Center.X.ToString(numberFormat_EnUS)).
                                     Replace("[Y]", map.Center.Y.ToString(numberFormat_EnUS)) + "';\r\n";

            if (!String.IsNullOrEmpty(_OnViewChange))
                setvarsScript += obj + ".onViewChange = function() { " + _OnViewChange + "(" + obj + "); }" + newline;
            if (!String.IsNullOrEmpty(_OnViewChanging))
                setvarsScript += obj + ".onViewChanging = function() { " + _OnViewChanging + "(" + obj + "); }" +
                                 newline;
            if (!String.IsNullOrEmpty(_OnClickEvent))
                setvarsScript += ClickEventPropertyName + " = function(event) { " + OnClickEvent + "(event," + obj +
                                 ");};";

            //setvarsScript += "SharpMap_BeginRefreshMap(" + obj + ",1);" + newline;

            setvarsScript += "}";

            //Register scripts in page
            ClientScriptManager cm = Page.ClientScript;
            //cm.RegisterClientScriptBlock(this.GetType(), "SetVars_" + this.ClientID, setvarsScript, true);
            cm.RegisterStartupScript(GetType(), "SetVars_" + ClientID, setvarsScript, true);
            //The following doesn't really do anything, but it cheats ASP.NET to include its callback scripts
            cm.GetCallbackEventReference(this, "SharpMap_MapOnClick(event,this)", "SharpMap_RefreshMap", "null",
                                         "SharpMap_AjaxOnError", true);

            //this.Controls.Add(new LiteralControl("<script type=\"text/javascript\">SetVars_" + this.ClientID + "();</script>\r\n"));
        }

        private void GenerateMapBox()
        {
            Style.Add("overflow", "hidden");
            Style.Add("z-index", "101");
            Style.Add("cursor", "pointer");
            Style.Add("position", "relative");
            Style.Add("display", "block");
            if (Style["BackColor"] != null)
                Style.Add("background", ColorTranslator.ToHtml(map.BackColor));

            imgMap1 = new Image();
            imgMap2 = new Image();
            imgMap1.Attributes["galleryimg"] = "false"; //Disable Internet Explorer image toolbar
            imgMap2.Attributes["galleryimg"] = "false"; //Disable Internet Explorer image toolbar						

            imgMap1.Style.Add("position", "absolute");
            imgMap1.Style.Add("Z-index", "10");
            imgMap2.Style.Add("position", "absolute");
            imgMap2.Style.Add("visibility", "hidden");
            imgMap2.Style.Add("opacity", "0");
            imgMap2.Style.Add("filter", "'ALPHA(opacity=0)'");
            imgMap2.Style.Add("Z-index", "9");

            Controls.Add(imgMap1);
            Controls.Add(imgMap2);

            if (_DisplayStatusBar)
            {
                spanCursorLocation = new HtmlGenericControl("span");
                spanCursorLocation.InnerText = "";
                spanCursorLocation.Style.Add("filter", "ALPHA(opacity=100)");
                divTopBar = new HtmlGenericControl("div");
                divTopBar.Style.Clear();

                divTopBar.Style.Add("Z-index", "20");
                divTopBar.Style.Add("border-bottom", "1px solid #000");
                divTopBar.Style.Add("position", "absolute");
                divTopBar.Style.Add("filter", "ALPHA(opacity=50)");
                divTopBar.Style.Add("opacity ", "0.5");
                divTopBar.Style.Add("background", "#fff");
                divTopBar.Style.Add("width", "100%");

                divTopBar.Controls.Add(spanCursorLocation);
                Controls.Add(divTopBar);
            }
        }
    }
}