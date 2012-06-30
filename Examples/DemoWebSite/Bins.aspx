<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Bins.aspx.cs" Inherits="Bins" Title="Custom Theme using Styling Delegate" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <div>   
    	<asp:RadioButtonList ID="rblMapTools" runat="server" RepeatDirection="Horizontal">
            <asp:ListItem Value="0">Zoom in</asp:ListItem>
            <asp:ListItem Value="1">Zoom out</asp:ListItem>
            <asp:ListItem Value="2" Selected="True">Pan</asp:ListItem>
        </asp:RadioButtonList>
        <asp:ImageButton Width="600" Height="300" ID="imgMap" runat="server" OnClick="imgMap_Click" style="border: 1px solid #000;" />
    </div>
	Style-pseudo code:<br />
	<pre>
	If country Name="Denmark" => Green fill
	else if country name="United Stated" => Blue fill, red outline
	else if country name="China" => Red fill
	else if country name starts with 'S' => Yellow fill
	else if (geometry is polygon or multipolygon) and area is less then 30 => cyan fill
	else gray fill
	</pre>
</asp:Content>