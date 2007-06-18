<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Transformation.aspx.cs" Inherits="Transformation" Title="On-the-fly transformation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <div>
    	<asp:RadioButtonList ID="rblMapTools" runat="server" RepeatDirection="Horizontal">
            <asp:ListItem Value="0">Zoom in</asp:ListItem>
            <asp:ListItem Value="1">Zoom out</asp:ListItem>
            <asp:ListItem Value="2" Selected="True">Pan</asp:ListItem>
        </asp:RadioButtonList>
        <asp:DropDownList ID="ddlProjection" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlProjection_SelectedIndexChanged">
			<asp:ListItem Value="Pseudo">Pseudo Plate Carree (default / no projection applied)</asp:ListItem>
			<asp:ListItem Value="Mercator">Mercator</asp:ListItem>
	        <asp:ListItem Value="Albers">Albers</asp:ListItem>
	        <asp:ListItem Value="Lambert">Lambert Conformal Conic 2SP</asp:ListItem>
        </asp:DropDownList><br />
        <asp:ImageButton Width="600" Height="300" ID="imgMap" runat="server" OnClick="imgMap_Click" style="border: 1px solid #000;" />
         <br />
        <b>Map envelope:</b><br />
        <asp:Literal ID="litEnvelope" runat="server" /><br />
        <asp:Literal ID="litEnvelopeLatLong" runat="server" /><br />
        <b>Input coordinate system:</b><br />
        <asp:Literal ID="litInputCoordsys" runat="server" /><br />
        <b>Map coordinate system:</b><br />
        <asp:Literal ID="litCoordsys" runat="server" /><br />
        <b>Active transformation:</b><br />
        <asp:Literal ID="litTransform" runat="server" /><br />
    </div>
    
    <div style="border: solid 1px #000; padding:10px; background-color:#eee; margin-top: 20px;">
		<b>Projections:</b>
		<ul>
			<li>
				<b>The plate carree projection</b> or geographic projection or equirectangular projection, is a very simple map projection that has been in use since the earliest days of spherical cartography. The name is from the French for "flat and square". It is a special case of the equidistant cylindrical projection in which the horizontal coordinate is the longitude and the vertical coordinate is the latitude.
			</li>
			<li>
				<b>The Mercator projection</b> is a cylindrical map projection. Like in all cylindric projections, parallels and meridians are straight and perpendicular to each other. But the unavoidable east-west stretching away from the equator is here accompanied by a corresponding north-south stretching, so that at every location the east-west scale is the same as the north-south scale, making the projection conformal.
			</li>
			<li><b>Lambert conformal conic projection.</b> Often used for aeronautical charts, a Lambert conformal conic projection in essence superimposes a cone over the sphere of the Earth, with two reference parallels secant to the globe and intersecting it. This minimizes distortion from projecting a three dimensional surface to a two-dimensional surface. Distortion is least along the standard parallels, and increases further from the chosen parallels. As the name indicates, maps using this projection are conformal.
				Pilots favor these charts because a straight line drawn on a Lambert conformal conic projection approximates a great circle route, which is the shortest distance between two points on the surface of a sphere.
			</li>
			<li>
			<b>The Albers equal-area conic</b> projection, or Albers projection, is a conic, equal area map projection that uses two standard parallels. Although scale and shape are not preserved, distortion is minimal between the standard parallels.
			</li>
		</ul>
		Source: <a href="http://en.wikipedia.org/">Wikipedia</a>. For an in-depth description of map projections see <a href="http://pubs.er.usgs.gov/usgspubs/pp/pp1395">John Snyder, Map projections; a working manual"</a>.
    </div>
</asp:Content>