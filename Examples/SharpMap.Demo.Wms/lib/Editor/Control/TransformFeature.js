/**
 * Class: OpenLayers.Editor.Control.DragFeature
 * 
 * Displays bounding box around clicked features along with handles to scale and rotate geometries.
 * Also allows to move features by dragging them.
 * 
 * Inherits from:
 *  - <OpenLayers.Control.TransformFeature>
 */
OpenLayers.Editor.Control.TransformFeature = OpenLayers.Class(OpenLayers.Control.TransformFeature, {
    CLASS_NAME: 'OpenLayers.Editor.Control.TransformFeature',
    
    /**
     * @type {OpenLayers.Layer.Vector}
     */
    editLayer: null,
    
    /**
     * @param {OpenLayers.Layer.Vector} editLayer
     */
    initialize: function(editLayer){
        OpenLayers.Control.TransformFeature.prototype.initialize.call(this, editLayer, {
            renderIntent: "transform",
            rotationHandleSymbolizer: "rotate"
        });
        
        this.editLayer = editLayer;
        
        this.addStyles();
        
        this.events.on({
            'transformcomplete': function(e){
                e.feature.state = OpenLayers.State.UPDATE;
                this.editLayer.events.triggerEvent("afterfeaturemodified", {
                    feature: e.feature
                });
            },
            scope: this
        });
        
        this.title = OpenLayers.i18n('oleTransformFeature');
    },
    
    /**
     * Adds style of box around object and handles shown during transformation
     */
    addStyles: function(){
        var control = this;
        this.editLayer.styleMap.styles.transform =new OpenLayers.Style({
            display: "${getDisplay}",
            cursor: "${role}",
            pointRadius: 5,
            fillColor: "white",
            fillOpacity: 1,
            strokeColor: "black"
        }, {
            context: {
                getDisplay: function(feature) {
                    if(control.feature===null || control.feature.geometry instanceof OpenLayers.Geometry.Point){
                        return "none";
                    }
                    // hide the resize handle at the south-east corner
                    return feature.attributes.role === "se-resize" ? "none" : "";
                }
            }
        });
        this.editLayer.styleMap.styles.rotate = new OpenLayers.Style({
            display: "${getDisplay}",
            pointRadius: 10,
            fillColor: "#ddd",
            fillOpacity: 1,
            strokeColor: "black"
        }, {
            context: {
                getDisplay: function(feature) {
                    if(control.feature===null || control.feature.geometry instanceof OpenLayers.Geometry.Point){
                        return "none";
                    }
                    // only display the rotate handle at the south-east corner
                    return feature.attributes.role === "se-rotate" ? "" : "none";
                }
            }
        });
    },
    
    activate: function(){
        OpenLayers.Control.TransformFeature.prototype.activate.call(this);
        if(this.feature===null || this.feature.geometry instanceof OpenLayers.Geometry.Point){
            // Re-render handles to hide them when control is activated initially without a feature selected so far
            this.editLayer.drawFeature(this.box, this.renderIntent);
            this.rotationHandles.forEach(function(f){
                this.editLayer.drawFeature(f, this.renderIntent);
            }, this);
            this.handles.forEach(function(f){
                this.editLayer.drawFeature(f, this.renderIntent);
            }, this);
        }
    },
    
    /**
     * Copy of superclass's implementation except that it does not collapse bounding box when a point feature is selected.
     */
    createBox: function() {
        var control = this;
        
        this.center = new OpenLayers.Geometry.Point(0, 0);
        this.box = new OpenLayers.Feature.Vector(
            new OpenLayers.Geometry.LineString([
                new OpenLayers.Geometry.Point(-1, -1),
                new OpenLayers.Geometry.Point(0, -1),
                new OpenLayers.Geometry.Point(1, -1),
                new OpenLayers.Geometry.Point(1, 0),
                new OpenLayers.Geometry.Point(1, 1),
                new OpenLayers.Geometry.Point(0, 1),
                new OpenLayers.Geometry.Point(-1, 1),
                new OpenLayers.Geometry.Point(-1, 0),
                new OpenLayers.Geometry.Point(-1, -1)
            ]), null,
            typeof this.renderIntent == "string" ? null : this.renderIntent
        );
        
        // Override for box move - make sure that the center gets updated
        this.box.geometry.move = function(x, y) {
            control._moving = true;
            OpenLayers.Geometry.LineString.prototype.move.apply(this, arguments);
            control.center.move(x, y);
            delete control._moving;
        };

        // Overrides for vertex move, resize and rotate - make sure that
        // handle and rotationHandle geometries are also moved, resized and
        // rotated.
        var vertexMoveFn = function(x, y) {
            OpenLayers.Geometry.Point.prototype.move.apply(this, arguments);
            this._rotationHandle && this._rotationHandle.geometry.move(x, y);
            this._handle.geometry.move(x, y);
        };
        var vertexResizeFn = function(scale, center, ratio) {
            OpenLayers.Geometry.Point.prototype.resize.apply(this, arguments);
            this._rotationHandle && this._rotationHandle.geometry.resize(
                scale, center, ratio);
            this._handle.geometry.resize(scale, center, ratio);
        };
        var vertexRotateFn = function(angle, center) {
            OpenLayers.Geometry.Point.prototype.rotate.apply(this, arguments);
            this._rotationHandle && this._rotationHandle.geometry.rotate(
                angle, center);
            this._handle.geometry.rotate(angle, center);
        };
        
        // Override for handle move - make sure that the box and other handles
        // are updated, and finally transform the feature.
        var handleMoveFn = function(x, y) {
            var oldX = this.x, oldY = this.y;
            OpenLayers.Geometry.Point.prototype.move.call(this, x, y);
            if(control._moving) {
                return;
            }
            var evt = control.dragControl.handlers.drag.evt;
            var preserveAspectRatio = !control._setfeature &&
                control.preserveAspectRatio;
            var reshape = !preserveAspectRatio && !(evt && evt.shiftKey);
            var oldGeom = new OpenLayers.Geometry.Point(oldX, oldY);
            var centerGeometry = control.center;
            this.rotate(-control.rotation, centerGeometry);
            oldGeom.rotate(-control.rotation, centerGeometry);
            var dx1 = this.x - centerGeometry.x;
            var dy1 = this.y - centerGeometry.y;
            var dx0 = dx1 - (this.x - oldGeom.x);
            var dy0 = dy1 - (this.y - oldGeom.y);
            if (control.irregular && !control._setfeature) {
               dx1 -= (this.x - oldGeom.x) / 2;
               dy1 -= (this.y - oldGeom.y) / 2;
            }
            this.x = oldX;
            this.y = oldY;
            var scale, ratio = 1;
            if(control.feature.geometry instanceof OpenLayers.Geometry.Point){
                scale = 1;
            } else {
                if (reshape) {
                    scale = Math.abs(dy0) < 0.00001 ? 1 : dy1 / dy0;
                    ratio = (Math.abs(dx0) < 0.00001 ? 1 : (dx1 / dx0)) / scale;
                } else {
                    var l0 = Math.sqrt((dx0 * dx0) + (dy0 * dy0));
                    var l1 = Math.sqrt((dx1 * dx1) + (dy1 * dy1));
                    scale = l1 / l0;
                }
            }

            // rotate the box to 0 before resizing - saves us some
            // calculations and is inexpensive because we don't drawFeature.
            control._moving = true;
            control.box.geometry.rotate(-control.rotation, centerGeometry);
            delete control._moving;

            control.box.geometry.resize(scale, centerGeometry, ratio);
            control.box.geometry.rotate(control.rotation, centerGeometry);
            control.transformFeature({scale: scale, ratio: ratio});
            if (control.irregular && !control._setfeature) {
               var newCenter = centerGeometry.clone();
               newCenter.x += Math.abs(oldX - centerGeometry.x) < 0.00001 ? 0 : (this.x - oldX);
               newCenter.y += Math.abs(oldY - centerGeometry.y) < 0.00001 ? 0 : (this.y - oldY);
               control.box.geometry.move(this.x - oldX, this.y - oldY);
               control.transformFeature({center: newCenter});
            }
        };
        
        // Override for rotation handle move - make sure that the box and
        // other handles are updated, and finally transform the feature.
        var rotationHandleMoveFn = function(x, y){
            var oldX = this.x, oldY = this.y;
            OpenLayers.Geometry.Point.prototype.move.call(this, x, y);
            if(control._moving) {
                return;
            }
            var evt = control.dragControl.handlers.drag.evt;
            var constrain = (evt && evt.shiftKey) ? 45 : 1;
            var centerGeometry = control.center;
            var dx1 = this.x - centerGeometry.x;
            var dy1 = this.y - centerGeometry.y;
            var dx0 = dx1 - x;
            var dy0 = dy1 - y;
            this.x = oldX;
            this.y = oldY;
            var a0 = Math.atan2(dy0, dx0);
            var a1 = Math.atan2(dy1, dx1);
            var angle = a1 - a0;
            angle *= 180 / Math.PI;
            control._angle = (control._angle + angle) % 360;
            var diff = control.rotation % constrain;
            if(Math.abs(control._angle) >= constrain || diff !== 0) {
                angle = Math.round(control._angle / constrain) * constrain -
                    diff;
                control._angle = 0;
                control.box.geometry.rotate(angle, centerGeometry);
                control.transformFeature({rotation: angle});
            } 
        };

        var handles = new Array(8);
        var rotationHandles = new Array(4);
        var geom, handle, rotationHandle;
        var positions = ["sw", "s", "se", "e", "ne", "n", "nw", "w"];
        for(var i=0; i<8; ++i) {
            geom = this.box.geometry.components[i];
            handle = new OpenLayers.Feature.Vector(geom.clone(), {
                role: positions[i] + "-resize"
            }, typeof this.renderIntent == "string" ? null :
                this.renderIntent);
            if(i % 2 == 0) {
                rotationHandle = new OpenLayers.Feature.Vector(geom.clone(), {
                    role: positions[i] + "-rotate"
                }, typeof this.rotationHandleSymbolizer == "string" ?
                    null : this.rotationHandleSymbolizer);
                rotationHandle.geometry.move = rotationHandleMoveFn;
                geom._rotationHandle = rotationHandle;
                rotationHandles[i/2] = rotationHandle;
            }
            geom.move = vertexMoveFn;
            geom.resize = vertexResizeFn;
            geom.rotate = vertexRotateFn;
            handle.geometry.move = handleMoveFn;
            geom._handle = handle;
            handles[i] = handle;
        }
        
        this.rotationHandles = rotationHandles;
        this.handles = handles;
    }
});