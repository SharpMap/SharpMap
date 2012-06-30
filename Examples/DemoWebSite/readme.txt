To make these demos run, make sure you have set IIS to run ASP.NET v2.0 on the web-application (default is v1.1!)

Demos:

/Simple.aspx:
Creates a simple map which can be zoomed and pan'ed

/Gradient.aspx
Renders the colors of each country based on the population density

/bins.aspx
Renders the colors based on a custom delegate 

/ajax.aspx
AJAX style zooming and panning 

/WmsClient.aspx
Combines local data with data from an external WMS server. 

/wms.aspx
Creates a WMS Server from a simple map
To get capabilities access:  /wms.aspx?SERVICE=Map&REQUEST=GetCapabilities

/PieCharts.aspx
Shows how to add custom symbols to a layer (in this case pie charts) and render them on top of each country

/TransformTests.aspx
Performs forward and reverse coordinate transformation of points and checks the result. 

--------------------
Copyright (c) 2005-2006 Morten Nielsen

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies of this Software or works derived from this Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
SOFTWARE.

