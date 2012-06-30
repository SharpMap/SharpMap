<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="WmsClient.aspx.cs" Inherits="WmsClient" Title="WmsClient demo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
<table >   
	<tr>
		<td valign="top">
    		<asp:RadioButtonList ID="rblMapTools" runat="server" RepeatDirection="Horizontal">
				<asp:ListItem Value="0">Zoom in</asp:ListItem>
				<asp:ListItem Value="1">Zoom out</asp:ListItem>
				<asp:ListItem Value="2" Selected="True">Pan</asp:ListItem>
			</asp:RadioButtonList>
			<asp:ImageButton Width="600px" Height="300px" ID="imgMap" 
							 runat="server" OnClick="imgMap_Click" 
							 style="border: 1px solid #000;" /><br />
			<asp:HyperLink ID="hlCurrentImage" runat="server" Target="_blank">Link to current map</asp:HyperLink><br />
			<asp:HyperLink ID="hlWmsImage" runat="server" Target="_blank">Link to active WMS map</asp:HyperLink><br />
		</td>
		<td style="width: 300px">
			<asp:Literal ID="litLayers" runat="server" />
		</td>
	</tr>
</table>
	- Countries and labels are local datasource<br/>
	- Water bodies are from the Demis WMS Server
</asp:Content>