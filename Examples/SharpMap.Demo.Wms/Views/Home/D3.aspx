<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="cssContent" ContentPlaceHolderID="CustomCssContent" runat="server">
    <link rel="stylesheet" href="<%=this.Url.Content("~/Content/d3.style.css?v=1")%>" />        
</asp:Content>
<asp:Content ID="jsContent" ContentPlaceHolderID="CustomJsContent" runat="server">
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/d3.v2.js?v=1")%>"> </script>
    <script type="text/javascript" src="<%=this.Url.Content("~/Scripts/script_d3.js?v=1")%>"> </script>    
</asp:Content>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="map"></div>
</asp:Content>