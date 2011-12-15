<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SharpMapServer.Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>SharpMapServer : Administration</title>
    <link href="css/custom-theme/jquery-ui-1.8.16.custom.css" type="text/css" rel="stylesheet" />
    <style>
        INPUT 
        {
            width: 400px;
        }
    </style>
    <script language="javascript" type="text/javascript" src="js/jquery-1.6.4.js"></script>
    <script language="javascript" type="text/javascript" src="js/jquery-ui-1.8.16.custom.min.js"></script>
    <script language="javascript" type="text/javascript" src="js/SharpMapServer.js"></script>
</head>
<body>
<img src="images/SharpMapBanner.png" />
<div id="tabs">
     <ul>
		<li><a href="#tabs-1">Server Status</a></li>
		<li><a href="#tabs-2">General settings</a></li>
		<li><a href="#tabs-3">WMS Settings</a></li>
	</ul>
     <div id="tabs-1">
		<p><b>Server</b><br />Running <span id="serverStatus"></span><br /><br />
        <b>WMS</b><br />
        <a href="wms?REQUEST=GetCapabilities&VERSION=1.3.0&SERVICE=WMS">WMS 1.3.0 Capabilities</a>
        </p>
	</div>
	<div id="tabs-2">
		<p><b>General Settings</b></p>
        <fieldset>
		<label for="settsTitle">Title</label><br />
		<input type="text" id="settsTitle" class="text ui-widget-content ui-corner-all" /><br />
		<label for="settsAbstract">Abstract</label><br />
		<input type="text" id="settsAbstract" value="" class="text ui-widget-content ui-corner-all" /><br />
		<label for="settsAccessConstraints">AccessConstraints</label><br />
		<input type="text" id="settsAccessConstraints" value="" class="text ui-widget-content ui-corner-all" /><br />
        <label for="settsContactInformation">ContactInformation</label><br />
		<input type="text" id="settsContactInformation" value="" class="text ui-widget-content ui-corner-all" /><br />
        <label for="settsFees">Fees</label><br />
		<input type="text" id="settsFees" value="" class="text ui-widget-content ui-corner-all" /><br />
        <label for="settsKeyWords">Keywords</label><br />
		<input type="text" id="settsKeyWords" value="" class="text ui-widget-content ui-corner-all" /><br />
        <label for="settsOnlineResource">OnlineResource</label><br />
		<input type="text" id="settsOnlineResource" value="" class="text ui-widget-content ui-corner-all" /><br />
	</fieldset>
	</div>
	<div id="tabs-3">
		<p>WMS Layers</p>
        <p><span id="wmsLayerList"></span></p>
	</div>
</div>
 <script type="text/javascript">
     var server = new SharpMap.Server();
     var $tabs;
     $(function () {
         $tabs = $("#tabs").tabs({
             select: function (event, ui) {
                 switch (ui.index) {
                     case 0:
                         server.UpdateStatus();
                         break;
                     case 1:
                         server.UpdateBasicSettings();
                         break;
                     case 2:
                         server.UpdateWMSLayers();
                         break;
                     default:
                         alert("unknown selection: " + selected);
                         break;
                 }
             }
         });
         server.UpdateStatus();
     });
 </script>
</body>
</html>
