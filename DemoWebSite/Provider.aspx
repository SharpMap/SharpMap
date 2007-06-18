<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Provider.aspx.cs" Inherits="Provider" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <div style="margin-left:25px;">
        <asp:Label ID="Label1" runat="server" style="font-weight:bold;" Text="List of available data providers" />
        <asp:BulletedList ID="ProviderList" runat="server">
        </asp:BulletedList>
    </div>
</asp:Content>

