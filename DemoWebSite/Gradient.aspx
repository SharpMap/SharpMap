<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Gradient.aspx.cs" Inherits="Gradient" Title="Gradient theme" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
	<div>   
		<asp:RadioButtonList ID="rblMapTools" runat="server" RepeatDirection="Horizontal">
			<asp:ListItem Value="0">Zoom in</asp:ListItem>
			<asp:ListItem Value="1">Zoom out</asp:ListItem>
			<asp:ListItem Value="2" Selected="True">Pan</asp:ListItem>
		</asp:RadioButtonList>
		<asp:ImageButton Width="600" Height="300" ID="imgMap" runat="server" OnClick="imgMap_Click" style="border: 1px solid #000;" />
	</div>
	This demo uses the following thematics styles:<br />
	- Country color by population density density<br />
	- Country labelsize by population density<br />
	- City Symbol size by city popultion
</asp:Content>