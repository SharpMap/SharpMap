<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Demo.aspx.cs" Inherits="SharpMapServer.Demo" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
    <title>SharpMapServer Demo</title>
    <link rel="Stylesheet" type="text/css" href="css/openlayers.css" />
    <script type="text/javascript" src="js/OpenLayers.js"></script>
    <script language="javascript" type="text/javascript" src="js/jquery-1.6.4.js"></script>
    <script type="text/javascript">
        var parameters = <%=parameters %>
        var map;
        var layer;
        $(window).ready(function () {
            OpenLayers.ImgPath = 'images/';
            var mapOpts = {
                projection: parameters.projection,
                maxExtent: new OpenLayers.Bounds(parameters.maxExtent[0],parameters.maxExtent[1],parameters.maxExtent[2],parameters.maxExtent[3])
            };
            map = new OpenLayers.Map('map', mapOpts);
            layer = new OpenLayers.Layer.WMS('WMS', 'wms?', { layers: '<%= layerName %>', version: '1.3.0',format: "image/png" }, {singleTile: true, yx: []});
            map.addLayer(layer);
            map.zoomToMaxExtent();
        });

        function updatetiled()
        {
            if ($("input[@name=tiled]:checked").attr('id') == "single")
            {
                map.removeLayer(layer);
                 layer = new OpenLayers.Layer.WMS('WMS', 'wms?', { layers: '<%= layerName %>', version: '1.3.0',format: "image/png" }, {singleTile: true, yx: []});
                 map.addLayer(layer);
            }
            else 
            {
                map.removeLayer(layer);
                 layer = new OpenLayers.Layer.WMS('WMS', 'wms?', { layers: '<%= layerName %>', version: '1.3.0',format: "image/png" }, {singleTile: false, yx: []});
                 map.addLayer(layer);
            }
        }

    </script>
</head>
<body>
<img src="images/SharpMapBanner.png" />
<h1>Demo of <%= layerName %></h1>
<div id="map" style="width: 640px; height:480px">
</div>
<input type="radio" id="single" name="tiled" checked="checked" value="SingleTile" onclick="updatetiled()" /> Single Tile | <input type="radio" name="tiled" value="Tiled" onclick="updatetiled()"  /> Tiled
</body>
</html>
