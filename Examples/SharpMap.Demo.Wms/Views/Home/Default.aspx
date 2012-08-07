<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="cssContent" ContentPlaceHolderID="CustomCssContent" runat="server">
    <link rel="stylesheet" href="<%=this.Url.Content("~/theme/default/style.css?v=1")%>" />
    <link rel="stylesheet" href="<%=this.Url.Content("~/Content/openlayers.css?v=1")%>" />
</asp:Content>
<asp:Content ID="jsContent" ContentPlaceHolderID="CustomJsContent" runat="server">
    <script type="text/javascript" src="<%=this.Url.Content("~/lib/OpenLayers.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ol/LoadingPanel.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/script_def.js?v=1")%>"></script>
</asp:Content>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="map">
    </div>
</asp:Content>
