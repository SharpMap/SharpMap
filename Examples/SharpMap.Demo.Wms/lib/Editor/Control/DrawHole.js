/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.DrawHole
 * The DrawHole control provides a method to cut holes in features
 *     from a given layer. All vertices from the hole feature must
 *     lay within the targted feature and only the top most feature
 *     will be processed.
 *
 * Inherits from:
 *  - <OpenLayers.Control.DrawFeature>
 */
OpenLayers.Editor.Control.DrawHole = OpenLayers.Class(OpenLayers.Control.DrawFeature, {

    /**
     * Property: minArea
     * {Number} Minimum hole area.
     */
    minArea: 0,

    title: OpenLayers.i18n('oleDrawHole'),
    
    /**
     * Constructor: OpenLayers.Editor.Control.DrawHole
     * Create a new control for deleting features.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>}
     * options - {Object} An optional object whose properties will be used
     *     to extend the control.
     */
    initialize: function (layer, options) {
        this.callbacks = OpenLayers.Util.extend(this.callbacks, {
            point: function(point) {
                this.layer.events.triggerEvent('pointadded', {point: point});
            }
        });
        
        OpenLayers.Control.DrawFeature.prototype.initialize.apply(this,
            [layer, OpenLayers.Handler.Polygon, options]);

        this.title = OpenLayers.i18n('oleDrawHole');

    },

    /**
     * Method: drawFeature
     * Cut hole only if area greater than or equal to minArea and all
     *     vertices intersect the targeted feature.
     * @param {OpenLayers.Geometry} geometry The hole to be drawn
     */
    drawFeature: function (geometry) {

        var feature = new OpenLayers.Feature.Vector(geometry);
        feature.state = OpenLayers.State.INSERT;
        // Trigger sketchcomplete and allow listeners to prevent modifications
        var proceed = this.layer.events.triggerEvent('sketchcomplete', {feature: feature});
        
        if (proceed !== false && geometry.getArea() >= this.minArea) {
            var vertices = geometry.getVertices(), intersects;
            
            features: for (var i = 0, li = this.layer.features.length; i < li; i++) {
                var layerFeature = this.layer.features[i];
                
                intersects = true;
                for (var j = 0, lj = vertices.length; j < lj; j++) {
                    if (!layerFeature.geometry.intersects(vertices[j])) {
                        intersects = false;
                    }
                }
                if (intersects) {
                    layerFeature.state = OpenLayers.State.UPDATE;
                    // Notify listeners that a feature is about to be modified
                    this.layer.events.triggerEvent("beforefeaturemodified", {
                        feature: layerFeature
                    });
                    layerFeature.geometry.components.push(geometry.components[0]);
                    this.layer.drawFeature(layerFeature);
                    // More event triggering but documentation is not clear how the following 2 are distinguished
                    // Notify listeners that a feature is modified
                    this.layer.events.triggerEvent("featuremodified", {
                        feature: layerFeature
                    });
                    // Notify listeners that a feature was modified
                    this.layer.events.triggerEvent("afterfeaturemodified", {
                        feature: layerFeature
                    });
                    break features;
                }
            }
        }
    },

    CLASS_NAME: 'OpenLayers.Editor.Control.DrawHole'
});