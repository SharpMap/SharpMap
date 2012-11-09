// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

//Notes: Event compatibility tables: http://www.quirksmode.org/js/events_compinfo.html

/* Sets up the map objects events, properties and methods */
function SharpMap_Init(container,map1,map2,statusbar,statustext,uniqueId)
{
	var obj = new Object();
	//Properties
	obj.currMap = 1;
	obj.mapReady = 1;
	obj.zoomEnded = 1;
	obj.clickEvent = null;
	obj.clickEventActive = false;
	obj.toogleClickEvent = function() { obj.clickEventActive = (!obj.clickEventActive); }
	obj.disableClickEvent = function() { obj.clickEventActive = false; }
	obj.enableClickEvent = function() { obj.clickEventActive = true; }
	obj.setClickEvent = function(fnc) { obj.clickEvent = fnc; }
	obj.container = WebForm_GetElementById(container);
	obj.uniqueId = uniqueId;
	obj.map1 = WebForm_GetElementById(map1);
	obj.map2 = WebForm_GetElementById(map2);
	if(statusbar!='') { obj.statusbar = WebForm_GetElementById(statusbar); obj.statusText = statustext; }
	//Methods
	obj.VisibleMap = function() { if(obj.currMap==1) return obj.map1; else return obj.map2; }
	obj.HiddenMap =  function() { if(obj.currMap==2) return obj.map1; else return obj.map2; }
	obj.GetCenter =  function() { return SharpMap_GetCenter(obj); }
	//Events
	obj.container.onmousemove = function(event) { SharpMap_MapMouseOver(event,obj); }
	if(statusbar!='') obj.container.onmouseout = function(event) { obj.statusbar.innerHTML = ''; }
	obj.container.onmousewheel = function(event) { SharpMap_MouseWheel(event,obj); return false;}
	if(obj.container.addEventListener)
		obj.container.addEventListener('DOMMouseScroll', function(event) { SharpMap_MozillaMouseWheel(event,obj); }, false);
	obj.container.onresize = function(event) { SharpMap_ResizeTimeout(event,obj); }
	obj.container.onselectstart = function() { return false; }
	obj.container.ondrag = function(event) { return false; }
	obj.container.onmousedown = function(event) { SharpMap_MouseDown(event,obj); return false; }
	obj.container.onmouseup = function(event) { SharpMap_MouseUp(event,obj); return false; }

	return obj;
}

/* Called when the mousewheel-scroll event occurs on the map  */
function SharpMap_MouseWheel(event,obj) {
	var e = event || window.event;
	if (e.type == 'mousewheel' && obj.mapReady && obj.zoomEnded==1) {
		var zoomval = obj.zoomAmount;
		if(e.wheelDelta<0) { zoomval = 1/obj.zoomAmount; }
		SharpMap_BeginZoom(obj,e.clientX,e.clientY,zoomval);
	}
}
/* this intermediate wheel function is used for mousewheel compatibility in Mozilla browsers */
function SharpMap_MozillaMouseWheel(event,obj)
{
	var e = new Object;
	e.type = 'mousewheel';
	e.wheelDelta = -event.detail;
	e.clientX = event.clientX;
	e.clientY = event.clientY;
	SharpMap_MouseWheel(e,obj);
}
var startDrag = null;
/* MouseDown - Occurs when potentially starting a drag event */ 
function SharpMap_MouseDown(event,obj) {
	if(obj.zoomEnded==1 && obj.mapReady==1) { 
		var e = event || window.event;
		startDrag=SharpMap_GetRelativePosition(e.clientX,e.clientY,obj.container);
	}
}
/* MouseUp - Occurs during a drag event or when doing a click */ 
function SharpMap_MouseUp(event,obj) {
	if(obj.zoomEnded==1 && obj.mapReady==1 && SharpMap_IsDefined(startDrag)) {
		var e = event || window.event;
		var endDrag=SharpMap_GetRelativePosition(e.clientX,e.clientY,obj.container);
		var dx=endDrag.x-startDrag.x;
		var dy=endDrag.y-startDrag.y;
		if(dx!=0 || dy!=0) { //we are dragging
			var center = SharpMap_PixelToMap(obj.container.offsetWidth*0.5-dx,obj.container.offsetHeight*0.5-dy,obj);
			obj.minX=center.x-obj.zoom*0.5;
			obj.maxY=center.y+obj.zoom/obj.container.offsetWidth*obj.container.offsetHeight*0.5;
			obj.mapReady = 0;
			SharpMap_BeginRefreshMap(obj,1);
		}			
		else if(obj.clickEventActive && obj.clickEvent!=null)
			obj.clickEvent(e,obj);
		else
			SharpMap_BeginZoom(obj,e.clientX,e.clientY,obj.zoomAmount);
	}
	startDrag=null;		
	return false;
}

function SharpMap_MapMouseOver(event,obj)
{
	if(SharpMap_IsDefined(startDrag)) {
		var e = event || window.event;
		var endDrag=SharpMap_GetRelativePosition(e.clientX,e.clientY,obj.container);
		var dx=endDrag.x-startDrag.x;
		var dy=endDrag.y-startDrag.y;
		var img=obj.map1;
		if(obj.currMap==2) img=obj.map2;
		img.style.left=dx+'px';
		img.style.top=dy+'px';
		obj.container.style.cursor='move';
	}
	else
	{
		//var position = WebForm_GetElementPosition(obj.container);
		var e = event || window.event;
		var mousePos = SharpMap_GetRelativePosition(e.clientX,e.clientY,obj.container);
		//var pos = SharpMap_PixelToMap(e.clientX-position.x,e.clientY-position.y,obj);
		var pos = SharpMap_PixelToMap(mousePos.x,mousePos.y,obj);
		var round = Math.floor(-Math.log(obj.zoom/obj.container.offsetWidth));
		var zoom = obj.zoom;
		if(round>0) {
			round = Math.pow(10,round);
			pos.x = Math.round(pos.x*round)/round;
			pos.y = Math.round(pos.y*round)/round;
			zoom = Math.round(zoom*round)/round;
		}
		else {
			pos.x = Math.round(pos.x);
			pos.y = Math.round(pos.y);
			zoom = Math.round(zoom);
		}
		if(SharpMap_IsDefined(obj.statusbar)) obj.statusbar.innerHTML = obj.statusText.replace('[X]',pos.x).replace('[Y]',pos.y).replace('[ZOOM]',zoom);
	}
}

/* Begins zooming around the point x,y */
function SharpMap_BeginZoom(obj,x,y,zoomval)
{
	if(obj.zoomEnded==0) return;
	obj.zoomEnded=0;
	obj.container.style.cursor = 'wait';
	var position = WebForm_GetElementPosition(obj.container);
	var imgX = x-position.x;
	var imgY = y-position.y;	
	if(obj.zoom/zoomval<obj.minZoom) zoomval = obj.zoom/obj.minZoom;
	if(obj.zoom/zoomval>obj.maxZoom) zoomval = obj.zoom/obj.maxZoom;
	var center = SharpMap_PixelToMap(imgX+(obj.container.offsetWidth*0.5-imgX)/zoomval,imgY+(obj.container.offsetHeight*0.5-imgY)/zoomval,obj);
	obj.zoom = obj.zoom/zoomval;
	obj.minX = center.x - obj.zoom*0.5;
	obj.maxY = center.y + obj.zoom*obj.container.offsetHeight/obj.container.offsetWidth*0.5;
	SharpMap_BeginRefreshMap(obj,1); //Start refreshing the map while we're zooming
	obj.zoomEnded = 0;
	SharpMap_DynamicZoom((position.x-x)*(zoomval-1),(position.y-y)*(zoomval-1),zoomval,0.0,obj);
}
/* loop method started by SharpMap_BeginZoom */
function SharpMap_DynamicZoom(tox,toy,toscale,step,obj)
{    
	step = step + 0.2;
	var imgd = obj.VisibleMap();
	var width = Math.round(obj.container.offsetWidth * ((toscale-1.0)*step+1.0)) +'px';
	var height = Math.round(obj.container.offsetHeight * ((toscale-1.0)*step+1.0))+'px';
	var left = Math.round(tox*step)+'px';
	var top = Math.round(toy*step)+'px';
	imgd.style.width = width;
	imgd.style.height = height;
	imgd.style.left = left;
	imgd.style.top = top;
	if(step < 0.99) {
		var delegate = function() { SharpMap_DynamicZoom(tox,toy,toscale,step,obj); };
		setTimeout(delegate,obj.zoomSpeed);
	}
	else {
		obj.zoomEnded=1;
		if(obj.mapReady==1) { SharpMap_BeginFade(obj);  }
	}
}
/* Starts the fading from one image to the other */
function SharpMap_BeginFade(obj)
{
	obj.container.style.cursor = 'wait';
	var to=obj.HiddenMap();
	var from=obj.VisibleMap();
	to.style.zIndex = 10;
	from.style.zIndex = 9;
	to.style.width = '';
	to.style.height = '';
	to.style.left = '';
	to.style.top = '';
	from.onload = ''; //Clear the onload event
	SharpMap_SetOpacity(to,0);
	to.style.visibility='visible';
	if(obj.onViewChange)
		obj.onViewChange();
	if(obj.currMap==2) { obj.currMap=1; } else { obj.currMap=2; }
	SharpMap_Fade(20,20,from,to,obj);
}
/* Recursive method started from SharpMap_BeginFade */
function SharpMap_Fade(value,step,from,to,obj)
{
	SharpMap_SetOpacity(to,value);
	if(value < 100) { 
		var delegate = function() { SharpMap_Fade((value+step),step,from,to,obj); };
		setTimeout(delegate,obj.fadeSpeed);
	}
	else {
		from.style.visibility='hidden';
		obj.container.style.cursor = 'auto';
	}
}
/* Resize handle and method of responding to window/map resizing */
var resizeHandle;
function SharpMap_ResizeTimeout(event,obj)
{
	/*
	if (resizeHandle!=0) { clearTimeout(resizeHandle); }
	var delegate = function() { SharpMap_BeginRefreshMap(obj,1); };
	resizeHandle = setTimeout(delegate,500);
	*/
}

/* Processes the response from the callback -
   The function sets up an onload-even for when the image should start fading if dofade==1 */
function SharpMap_GetCallbackResponse(url,obj,dofade){
	if(url=='') return;
	if(dofade==1)
	{
		var imgdnew = obj.HiddenMap();
		//set the onload function before setting the src
		//Marc A. Tidd (Tidd Consulting) 01/16/2008
		imgdnew.onload = function(){ obj.mapReady=1; imgdnew.onload=''; if(obj.zoomEnded==1) { SharpMap_BeginFade(obj); } }
		imgdnew.src = url;
	}
	else
	{
		obj.VisibleMap().src = url;
		obj.container.style.cursor = 'auto';
		obj.VisibleMap().onload = function(){ obj.mapReady=1; }
	}
}

/* Requests a new map from the server using async callback and starts fading when the image have been retrieved*/
function SharpMap_BeginRefreshMap(obj,dofade)
{
	var center = SharpMap_GetCenter(obj);
	var delegate = function(url) { SharpMap_GetCallbackResponse(url,obj,dofade); };
	WebForm_DoCallback(obj.uniqueId ,center.x+';'+center.y+';'+obj.zoom+';'+obj.container.offsetWidth+';'+obj.container.offsetHeight,delegate,null,SharpMap_AjaxOnError,true)
	obj.mapReady=0;
	obj.container.style.cursor = 'wait';
	if(obj.onViewChanging) obj.onViewChanging();
}

/* Returns the center of the current view */
function SharpMap_GetCenter(obj)
{
   var center = new Object();
   center.x = obj.minX+obj.zoom*0.5;
   center.y = obj.maxY-obj.zoom*obj.container.offsetHeight/obj.container.offsetWidth*0.5;
   return center;
}
/* Sets the opacity of an object (x-browser) */
function SharpMap_SetOpacity(obj,value)
{
	obj.style.opacity = value/100.0;
	obj.style.mozopacity = value/100.0;
	obj.style.filter = 'ALPHA(opacity=' + value + ')';
}
function SharpMap_AjaxOnError(arg) { alert('Map refresh failed: ' + arg); }
/* Transforms from pixels coordinates to world coordinates */
function SharpMap_PixelToMap(x,y,obj)
{
	 var p=new Object();
	 p.x = obj.minX+x*obj.zoom/obj.container.offsetWidth; p.y = obj.maxY-y*obj.zoom/obj.container.offsetWidth;
	 return p;
}
/* Returns the relative position of a point to an object */
function SharpMap_GetRelativePosition(x,y,obj)
{
	var position=WebForm_GetElementPosition(obj);
	var p=new Object();
	p.x=x-position.x;
	p.y=y-position.y;
	return p;
}
function SharpMap_IsDefined(obj)
{
	if (null == obj) { return false; }
	if ('undefined' == typeof(obj) ) { return false; }
	return true;
}
