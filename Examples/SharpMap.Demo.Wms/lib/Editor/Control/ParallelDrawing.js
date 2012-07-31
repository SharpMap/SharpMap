OpenLayers.Editor.Control.ParallelDrawing = OpenLayers.Class(OpenLayers.Control, {
    CLASS_NAME: 'OpenLayers.Editor.Control.ParallelDrawing',

    active: false,

    /**
     * @var {OpenLayers.Feature.Vector} Guide line that has been created by this control
     */
    guideLine: null,

    /**
     * @var {Object} Segment that has been used to create current guide line
     */
    guideLineSegment: null,

    /**
     * @param {OpenLayers.Layer.Vector} editLayer
     * @constuctor
     */
    initialize: function(editLayer) {
        OpenLayers.Control.prototype.initialize.call(this);
        this.layer = editLayer;
    },

    activate: function() {
        var activated = OpenLayers.Control.prototype.activate.call(this);
        if(activated) {
            this.layer.events.on({
                pointadded: this.closestSegment,
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
                pointadded: this.closestSegment,
                sketchcomplete: this.onSketchComplete,
                scope: this
            });
        }
        return deactivated;
    },

    /**
     * Adds a guide line running through the last added vertex of the sketch and being parallel to the closest segment of the editLayer.
     * @param {Object} event pointadded
     */
    closestSegment: function(event){
        // Find the closest segment to the event's location
        var closestSegment = null;
        var closestDistance = Number.MAX_VALUE;
        this.layer.features.forEach(function(feature){
            var segments = [];
            if(feature.geometry instanceof OpenLayers.Geometry.Curve){
                segments = feature.geometry.getSortedSegments();
            } else if(feature.geometry instanceof OpenLayers.Geometry.Polygon){
                feature.geometry.components.forEach(function(component){
                    component.getSortedSegments().forEach(function(segment){
                        segments.push(segment);
                    });
                });
            }
            segments.forEach(function(segment){
                if(this.guideLineSegment!==null && this.guideLineSegment.x1===segment.x1 && this.guideLineSegment.y1===segment.y1 && this.guideLineSegment.x2===segment.x2 && this.guideLineSegment.y2===segment.y2){
                    // Do not propose the same guide line twice
                    return;
                }
                
                var segmentLine = new OpenLayers.Geometry.LineString([
                    new OpenLayers.Geometry.Point(segment.x1, segment.y1),
                    new OpenLayers.Geometry.Point(segment.x2, segment.y2)
                ]);
                var distance = segmentLine.distanceTo(new OpenLayers.Geometry.Point(event.point.x, event.point.y));
                if(distance<closestDistance){
                    closestDistance = distance;
                    closestSegment = segment;
                }
            }, this);
        }, this);
        
        // Do nothing if no segment found (because no feature existed)
        if(closestSegment){
            this.guideLineSegment = closestSegment;
            this.removeGuides();

            var snappingGuideLayer = this.getSnappingGuideLayer();
            var line = snappingGuideLayer.getLine(closestSegment);
            // Move line so that it runs through the last added vertex
            line.b += (event.point.y - (line.m*event.point.x+line.b));
            
            this.guideLine = snappingGuideLayer.addLine(line);
        }
    },

    /**
     * @return {OpenLayers.Editor.Layer.Snapping}
     */
    getSnappingGuideLayer: function(){
        return this.map.getLayersByClass('OpenLayers.Editor.Layer.Snapping')[0];
    },

    /**
     * Removes own guide once sketch is completed
     */
    onSketchComplete: function(){
        this.removeGuides();
    },

    /**
     * Removes guides that have been created by this control from snapping layer
     */
    removeGuides: function(){
        if(this.guideLine!==null){
            this.getSnappingGuideLayer().removeFeatures([this.guideLine]);
        }
        this.guideLine = null;
    }
});