<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="cssContent" ContentPlaceHolderID="CustomCssContent" runat="server">
    <link rel="stylesheet" href="<%=this.Url.Content("~/theme/default/style.css?v=1")%>" />
    <link rel="stylesheet" href="<%=this.Url.Content("~/Content/geosilk.css?v=1")%>" />
</asp:Content>
<asp:Content ID="jsContent" ContentPlaceHolderID="CustomJsContent" runat="server">
    <script type="text/javascript" src="<%=this.Url.Content("~/lib/OpenLayers.js?v=1")%>"></script>    
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ol/LoadingPanel.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/CleanFeature.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/DeleteFeature.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/DragFeature.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/DrawPoint.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/DrawHole.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/DrawPath.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/DrawPolygon.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/EditorPanel.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/FilterFeature.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/ImportFeature.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/MergeFeature.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/SplitFeature.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/SaveFeature.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/LayerSettings.js?v=1")%>"></script>    
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/SnappingSettings.js?v=1")%>"></script>    
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/UndoRedo.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Control/Dialog.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/ole/Editor/Lang/en.js?v=1")%>"></script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/script_editor.js?v=1")%>"></script>
</asp:Content>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="map">
    </div>
</asp:Content>
