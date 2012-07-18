<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="cssContent" ContentPlaceHolderID="CustomCssContent" runat="server">
    <link rel="stylesheet" href="<%=Url.Content("~/Content/leaflet.css?v=1")%>" />
    <!--[if lte IE 8]><link rel="stylesheet" href="<%=Url.Content("~/Content/leaflet.ie.css?v=1")%>" /><![endif]-->
</asp:Content>
<asp:Content ID="jsContent" ContentPlaceHolderID="CustomJsContent" runat="server">
    <script type="text/javascript" src="<%=Url.Content("~/Scripts/leaflet.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=Url.Content("~/Scripts/ll/buildings.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=Url.Content("~/Scripts/script_buildings.js?v=1")%>"></script>
</asp:Content>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <div>
        <a id="link1" href="#">alexanderplatz</a> 
        <a id="link2" href="#">kurfurstendamm</a>
        <a id="link3" href="#">potsdamerplatz</a>
    </div>
    <div id="map">
    </div>
</asp:Content>
