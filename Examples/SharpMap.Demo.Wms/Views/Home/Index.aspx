<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Root.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="SharpMap.Demo.Wms.Models" %>

<asp:Content ID="content" ContentPlaceHolderID="CustomContent" runat="server">
    <ul data-role="listview" data-inset="true">
        <% var items = (IEnumerable<DemoItem>)this.ViewData["DemoItems"];
           foreach (DemoItem item in items)
           {%>
            <li><a href="<%=item.Url %>" rel="external"><%=item.Name%></a></li>
        <% }%>
    </ul>      
</asp:Content>