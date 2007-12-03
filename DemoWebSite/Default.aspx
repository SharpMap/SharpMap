<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" Title="SharpMap v0.9 demo examples" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
<div style="padding: 20px;">
<h3>SharpMap v0.9 demos</h3>

<p>
	To make these demos run, make sure you have set IIS to run ASP.NET v2.0 on the web-application (default is v1.1 on most IIS installations)
</p>

<h4>Demos:</h4>

<p>
	<a href="Simple.aspx">/Simple.aspx</a><br/>
	Creates a simple map which can be zoomed and pan'ed
</p>

<p>
	<a href="Gradient.aspx">/Gradient.aspx</a><br/>
	Renders the colors, pens and symbols of each country based on the population density and city population
</p>


<p>
	<a href="Bins.aspx">/Bins.aspx</a><br/>
	Renders the colors based on a custom delegate
</p>

<p>
	<a href="PieCharts.aspx">/PieCharts.aspx</a><br/>
	Shows how to add custom symbols to a layer (in this case pie charts) and render them on top of each country
</p>

<p>
	<a href="Ajax.aspx">/Ajax.aspx</a><br/>
	AJAX style zooming and panning
</p>

<p>
	<a href="GeometryFeature.aspx">/GeometryFeature.aspx</a><br/>
	Shows how to add some custom geometries with features via the GeometryFeatureProvider class.<br />
	This map is centered to Mayence/Germany, my home town since 4 years.
</p>

<p>
	<a href="WmsClient.aspx">/WmsClient.aspx</a><br/>
	Combines local data with data from an external WMS server.
</p>


<p>
	<a href="wms.aspx">/wms.aspx</a><br />
	Creates a WMS Server from a simple map<br/>
	Note: The link above will correctly throw a WmsException. See links below for some valid requests:<br/>
	- <a href="wms.ashx?SERVICE=WMS&REQUEST=GetCapabilities">Request capabilities</a><br/>
	- <a href="wms.ashx?REQUEST=GetMap&Layers=Countries,Rivers,Country labels&STYLES=&CRS=EPSG:4326&BBOX=-180,-90,180,90&WIDTH=600&HEIGHT=300&FORMAT=image/png&VERSION=1.3.0">Request a map</a>
</p>

<p>
	<a href="TransformTests.aspx">/TransformTests.aspx</a><br/>
	Performs forward and reverse coordinate transformation of points and checks the result.
</p>

<hr/>

<p>
	<b>Copyright (c) 2005-2006 <a href="http://www.iter.dk">Morten Nielsen</a></b>
</p>
<p>Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:</p>

<p>The above copyright notice and this permission notice shall be included in all 
copies of this Software or works derived from this Software.</p>

<p>THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
SOFTWARE.</p>
</div>
</asp:Content>

