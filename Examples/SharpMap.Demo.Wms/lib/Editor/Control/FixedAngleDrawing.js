/**
 * Creates orthogonal guidelines while drawing features
 */
OpenLayers.Editor.Control.FixedAngleDrawing = OpenLayers.Class(OpenLayers.Control, {
    CLASS_NAME: 'OpenLayers.Editor.Control.FixedAngleDrawing',

    active: false,
    
    /**
     * @var {Number} Amount of vertices in sketch when guidelines have been last painted
     */
    sketchVerticesAmount: null,

    /**
     * @var {Array.<OpenLayers.Feature.Vector>} Guide lines that have been created by this control
     */
    guides: null,

    /**
     * @param {OpenLayers.Layer.Vector} editLayer
     * @constuctor
     */
    initialize: function(editLayer) {
        this.guides = [];
        
        OpenLayers.Control.prototype.initialize.call(this);
        this.layer = editLayer;
    },
    
    activate: function() {
        var activated = OpenLayers.Control.prototype.activate.call(this);
        if(activated) {
            this.layer.events.on({
                sketchstarted: this.onSketchStarted,
                sketchmodified: this.onSketchModified,
                sketchcomplete: this.onSketchComplete,
                scope: this
            });
        }
        return activated;
    },
    
    deactivate: function() {
        var deactivated = OpenLayers.Control.prototype.deactivate.call(this);
        if(deactivated) {
            this.layer.events.un({
                sketchstarted: this.onSketchStarted,
                sketchmodified: this.onSketchModified,
                sketchcomplete: this.onSketchComplete,
                scope: this
            });
        }
        return deactivated;
    },

    /**
     * Triggers guideline modification
     */
    onSketchModified: function(event){
        var vertices = event.feature.geometry.getVertices();
        if(vertices.length>2 && this.sketchVerticesAmount!==vertices.length){
            this.removeGuides();

            this.sketchVerticesAmount = vertices.length;
            this.updateGuideLines(
                vertices[vertices.length-3],
                vertices[vertices.length-2]
            );

            // Add a guide line that is orthogonal to the first drawn segment in order to allow closing a sketch with fixed angle
            var snappingGuideLayer = this.getSnappingGuideLayer();
            var line = snappingGuideLayer.getLine({
                x1: vertices[0].x,
                y1: vertices[0].y,
                x2: vertices[1].x,
                y2: vertices[1].y
            });
            var m2 = (-1/line.m);
            var b2 = vertices[0].y-(m2*vertices[0].x);
            this.guides.push(snappingGuideLayer.addLine({
                m: m2,
                b: b2
            }));
        }
    },

    /**
     * Removes own guides once sketch is completed
     */
    onSketchComplete: function(){
        this.removeGuides();
    },

    /**
     * Removes guides that have been created by this control from snapping layer
     */
    removeGuides: function(){
        this.getSnappingGuideLayer().removeFeatures(this.guides);
        this.guides = [];
    },
    
    /**
     * Captures sketch and registers handler to hide guidelines after sketching is done
     */
    onSketchStarted: function(event){
        // A new sketch was added to the map
        var sketch = event.feature;
        
        var sketchLayer;
        if(sketch.geometry instanceof OpenLayers.Geometry.LineString || sketch.geometry instanceof OpenLayers.Geometry.Polygon){
            // Look for the active drawing control and get its temporary sketch layer
            for(var i = 0; i<this.map.controls.length; i++){
                var control = this.map.controls[i];
                if(control.active && control.handler instanceof OpenLayers.Handler.Path){
                    sketchLayer = control.handler.layer;
                    break;
                }
            }
        } else {
            // Feature type is not supported
            return;
        }
        sketchLayer.events.on({
            featureremoved: function(event){
                if(event.feature.id===sketch.id){
                    // Sketch was removed (canceled or completed)
                    this.sketchVerticesAmount = null;
                    this.getSnappingGuideLayer().destroyFeatures();
                }
            },
            scope: this
        })
    },
    
    /**
     * @return {OpenLayers.Editor.Layer.Snapping}
     */
    getSnappingGuideLayer: function(){
        return this.map.getLayersByClass('OpenLayers.Editor.Layer.Snapping')[0];
    },

    /**
     * Draws guidelines at pointLastFixed
     * @var {OpenLayers.Geometry.Point} pointEarlierFixed Point draw before last point was drawn
     * @var {OpenLayers.Geometry.Point} pointLastFixed Last point drawn
     */
    updateGuideLines: function(pointEarlierFixed, pointLastFixed){
        var snappingGuideLayer = this.getSnappingGuideLayer();

        // Slope of segment between given points
        var m = (pointLastFixed.y-pointEarlierFixed.y)/(pointLastFixed.x-pointEarlierFixed.x);

        // Draw guide orthogonal to segment with intersection at pointLastFixed
        var m2 = (-1/m);
        var b2 = pointLastFixed.y-(m2*pointLastFixed.x);
        this.guides.push(
            snappingGuideLayer.addLine({
                m: m2,
                b: b2,
                // Pass horizontal ordinate as well so that location of guide line is known even if m is Infinity
                x: pointLastFixed.x
            })
        );
    }
});